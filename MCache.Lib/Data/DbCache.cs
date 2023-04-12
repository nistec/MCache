//licHeader
//===============================================================================================================
// System  : Nistec.Cache - Nistec.Cache Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Caching.Remote;
using System.Collections;
using System.Data;
using Nistec.Generic;
using Nistec.Data.Entities;
using Nistec.Runtime;
using Nistec.IO;
using Nistec.Channels;
using Nistec.Caching.Sync;
using System.Collections.Concurrent;
using System.Xml;
using Nistec.Caching.Config;
using System.Threading.Tasks;
using System.Threading;

namespace Nistec.Caching.Data
{
    /// <summary>
    /// Represent a db cache that Hold a multiple <see cref="DbSet"/> items in memory.
    /// It is like a database of database <see cref="DbSet"/> items.
    /// Each <see cref="DbSet"/> item represent a data set of tables in cache.
    /// </summary>
    public class DbCache : IDisposable, ICachePerformance
    {
        #region ICachePerformance

        CachePerformanceCounter m_Perform;
        /// <summary>
        /// Get <see cref="CachePerformanceCounter"/> Performance Counter.
        /// </summary>
        public CachePerformanceCounter PerformanceCounter
        {
            get { return m_Perform; }
        }

        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void ICachePerformance.MemorySizeExchange(ref long memorySize)
        {
            CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.None, "Memory Size Exchange: DataCache");
            long size = GetSize();
            Interlocked.Exchange(ref memorySize, size);
        }

        //internal long MaxSize { get { return 999999999L; } }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long ICachePerformance.GetMaxSize()
        {
            return CacheSettings.MaxSize;
        }
        bool ICachePerformance.IsRemote
        {
            get { return true; }
        }
        int ICachePerformance.IntervalSeconds
        {
            get { return this.IntervalSeconds; }
        }
        bool ICachePerformance.Initialized
        {
            get { return this.Initialized; }
        }
        #endregion

        #region members

        ConcurrentDictionary<string, DbSet> m_db;

        #endregion

        #region events
    
        /// <summary>
        /// Sync Error Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SyncError;

        /// <summary>
        /// On Error Occured
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnError(string e)
        {
            if (SyncError != null)
                SyncError(this, new GenericEventArgs<string>(e));
        }
        #endregion

        #region ctor

        /// <summary>
        /// Initialize a new instance of db cache using db cache name.
        /// </summary>
        /// <param name="dbCacheName"></param>
        public DbCache(string dbCacheName)
        {
            _dbCacheName = dbCacheName;
            _Initialized = false;
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 10;
            this.m_db = new ConcurrentDictionary<string, DbSet>(concurrencyLevel, initialCapacity);
            m_Perform = new CachePerformanceCounter(this, CacheAgentType.DataCache, dbCacheName);
        }
        #endregion 

        #region Dispose

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {

            if (disposing)
            {
                this.Stop();
                
                if (this.m_db != null)
                {
                    this.m_db.Clear();
                     this.m_db = null;
                }
            }
        }

        #endregion

        #region properties

        string _dbCacheName;
        /// <summary>
        /// Get Db cache name.
        /// </summary>
        public string DbCacheName
        {
            get { return _dbCacheName; }
        }

        bool _Initialized;
        /// <summary>
        /// Get indicate whether the cache item is initialized.
        /// </summary>
        public bool Initialized 
        {
            get { return _Initialized; }
        }

        /// <summary>
        /// Get thie sync interval in seconds.
        /// </summary>
        public int IntervalSeconds
        {
            get;
            set;
        }
        #endregion

        #region Load config
        /// <summary>
        /// Load db cache.
        /// </summary>
        public virtual void LoadDbCache()
        {

            string file = CacheSettings.DbConfigFile;

            LoadDbConfigFile(file);
        }

        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        /// <param name="file"></param>
        public void LoadDbConfigFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                return;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
                LoadDbConfig(doc);
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Error, "LoadDbConfigFile error: " + ex.Message);
                OnError("LoadDbConfigFile error " + ex.Message);
            }
        }
        /// <summary>
        /// Load sync cache from xml string argument.
        /// </summary>
        /// <param name="xml"></param>
        public void LoadDbConfig(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                LoadDbConfig(doc);
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Error, "LoadDbConfig error: " + ex.Message);
                OnError("LoadSyncConfig error " + ex.Message);
            }
        }
        /// <summary>
        /// Load sync db cache from <see cref="XmlDocument"/> document.
        /// </summary>
        /// <param name="doc"></param>
        public void LoadDbConfig(XmlDocument doc)
        {
            if (doc == null)
                return;

            XmlNode items = doc.SelectSingleNode("//DbBase");
            if (items == null)
                return;
            LoadDbItems(items, CacheSettings.EnableAsyncTask);
        }

        internal void LoadDbItems(XmlNode node, bool copy)
        {
            if (node == null)
                return;
            try
            {
                XmlNodeList list = node.ChildNodes;
                if (list == null)
                    return;

                var db = new Dictionary<string, DbSet>();
                foreach (XmlNode n in list)
                {
                    if (n.NodeType == XmlNodeType.Comment)
                        continue;

                    SyncEntity sync = new SyncEntity(new XmlTable(n));
                    AddItem(sync, db);
                }

                Reload(db);

            }
            catch (Exception ex)
            {
                OnError("LoadSyncItems error " + ex.Message);
            }
            finally
            {
                EnsureSyncState();
            }
        }

        internal void AddItem(SyncEntity entity,Dictionary<string, DbSet> db) 
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            DbSet dc;

            if (!db.TryGetValue(entity.ConnectionKey, out dc))
            {
                dc = new DbSet(this, entity.ConnectionKey);
                db.Add(entity.ConnectionKey, dc);
            }
            dc.SyncState = CacheSyncState.Started;
            dc.SyncTables.Add(entity);

        }

        internal void EnsureSyncState()
        {
            foreach (var entry in m_db)
            {
                entry.Value.SyncState = CacheSyncState.Idle;
            }
        }

        internal void Reload(Dictionary<string, DbSet> db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("DbBase.Reload.db");
            }

            foreach (var entry in m_db.ToArray())
            {
                try
                {
                    entry.Value.SyncState = CacheSyncState.Started;




                    m_db[entry.Key] = entry.Value;
                }
                catch (Exception)
                {
                }
                finally
                {
                    entry.Value.SyncState = CacheSyncState.Idle;
                }
            }
        }

        #endregion

        #region start/stop

        /// <summary>
        /// Start db cache.
        /// </summary>
        public void Start()
        {
            if (_Initialized)
                return;
            foreach (DbSet dc in m_db.Values)
            {
                dc.Start(IntervalSeconds);
            }
            CacheQuery.Start();
            _Initialized = true;
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "DbBase Started!");
        }
        /// <summary>
        /// Stop dn cache.
        /// </summary>
        public void Stop()
        {
            CacheQuery.Stop();
            foreach (DbSet dc in m_db.Values)
            {
                try
                {
                    dc.Stop();
                }
                catch { }

            }
            _Initialized = false;
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "DbBase Stoped!");
        }
        #endregion

        #region public methods

        /// <summary>
        /// Synchronize Table in Data Source
        /// </summary>
        public void Refresh(string db, string tableName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("Refresh.db");
            }
            DbSet dc;
            if (m_db.TryGetValue(db, out dc))
            {
                dc.Refresh(tableName);
            }
        }

        /// <summary>
        /// Synchronize All Tables in Data Source
        /// </summary>
        public void RefreshDataSource(string db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("Refresh.db");
            }
            DbSet dc;
            if (m_db.TryGetValue(db, out dc))
            {
                dc.RefreshDataSource();
            }
        }

        /// <summary>
        /// Get specified <see cref="DbSet"/> from db cache.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public DbSet Get(string db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("Get.db");
            }
            DbSet item = null;
            if (m_db.TryGetValue(db, out item))
            {
                return item;
            }
            return null;
        }
        /// <summary>
        /// Get or create specified <see cref="DbSet"/> from db cache.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public DbSet GetOrCreate(string db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("GetOrCreate.db");
            }
            DbSet item = null;
            if (m_db.TryGetValue(db, out item))
            {
                return item;
            }
            item = new DbSet(this, db);
            if (m_db.TryAdd(db, item))
            {
                return item;
            }
            return null;
        }

        /// <summary>
        /// Set a new <see cref="DbSet"/> with a specified key in db cache.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="value"></param>
        public void Set(string db, DbSet value)
        {
            if (db == null)
                return;
            m_db[db] = value;
        }

        /// <summary>
        /// Set a new <see cref="DbSet"/> with a specified arguments.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="connectionKey"></param>
        /// <param name="syncOption"></param>
        public void Set(string db, string connectionKey, SyncOption syncOption)
        {
            if (db == null)
                return;
            DbSet dc = new DbSet(this, connectionKey);
            dc.SyncOption = syncOption;

            Set(db, dc);
        }

        /// <summary>
        /// Set Value into specific row and column in local data table  
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primaryKey</param>
        /// <param name="field">column name</param>
        /// <param name="value">value to set</param>
        public bool AddValue(string db, string tableName, string primaryKey, string field, object value)
        {
            DbSet item = Get(db);
            if (item != null)
            {
                return item.AddValue(tableName, primaryKey, field, value);
            }
            return false;
        }
        /// <summary>
        /// Set Value into specific row and column in local data table  
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primaryKey</param>
        /// <param name="field">column name</param>
        /// <param name="value">value to set</param>
        public bool SetValue(string db, string tableName, string primaryKey, string field, object value)
        {
            DbSet item = Get(db);
            if (item != null)
            {
               return item.SetValue(tableName, primaryKey, field, value);
            }
            return false;
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primaryKey</param>
        /// <param name="field">column name</param>
        /// <returns></returns>
        public T GetValue<T>(string db, string tableName, string primaryKey, string field)
        {
            object value = GetValue(db, tableName, primaryKey, field);
            if (value == null)
                return default(T);
            return GenericTypes.Convert<T>(value);

            //DbSet item = Get(db);
            //if (item == null)
            //    return default(T);
            //return item.GetValue<T>(tableName, primaryKey, field);
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primaryKey</param>
        /// <param name="field">column name</param>
        /// <returns>object value</returns>
        public object GetValue(string db, string tableName, string primaryKey, string field)
        {

            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }


            DbSet dbset;
            if (this.m_db.TryGetValue(db, out dbset))
            {
                DbTable table;
                if (dbset.DataSource.TryGetValue(tableName, out table))
                {
                    GenericEntity row;
                    if (table.DataSource.TryGetValue(primaryKey, out row))
                    {
                        object value;
                        if (row.Record.TryGetValue(field, out value))
                        {
                            return value;
                        }
                    }

                }
            }
            return null;


            //linq optional
            //var value1 =
            //  from dset in m_db
            //  from table in dset.Value.DataSource
            //  from row in table.Value.DataSource
            //  from f in row.Value.Record
            //  where dset.Key == db && table.Key == tableName && row.Key == primaryKey && f.Key == field
            //  select f.Value;


            //DbSet item = Get(db);
            //if (item == null)
            //    return null;
            //return item.GetValue(tableName, primaryKey, field);
        }

        public T FindValue<T>(string db, string tableName, string primaryKey, string field)
        {

            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            //linq optional
            var value =
          (from dset in m_db
           from table in dset.Value.DataSource
           from row in table.Value.DataSource
           from f in row.Value.Record
           where dset.Key == db && table.Key == tableName && row.Key == primaryKey && f.Key == field
           select f.Value).Cast<T>().FirstOrDefault();

            return value;

            //var val = m_db.Where(d => d.Key == db).Select(d => d.Value.GetValue(tableName, primaryKey, field)).FirstOrDefault();
        }


        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primaryKey</param>
        /// <returns>Hashtable object</returns>
        public GenericEntity GetRow(string db, string tableName, string primaryKey)
        {
            DbSet item = Get(db);
            if (item == null)
                return null;
            return item.GetRow(tableName, primaryKey);
        }

        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primaryKey</param>
        /// <returns>Hashtable object</returns>
        public NetStream GetStream(string db, string tableName, string primaryKey)
        {
            GenericEntity entity = GetRow(db, tableName, primaryKey);
            if (entity == null)
                return null;
            return entity.ToStream();
        }

        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primaryKey</param>
        /// <returns>Hashtable object</returns>
        public GenericRecord GetRecord(string db, string tableName, string primaryKey)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (primaryKey == null)
            {
                throw new ArgumentNullException("primaryKey");
            }

            DbSet dbset;
            if (this.m_db.TryGetValue(db, out dbset))
            {
                DbTable table;
                if (dbset.DataSource.TryGetValue(tableName, out table))
                {
                    GenericEntity row;
                    if (table.DataSource.TryGetValue(primaryKey, out row))
                    {
                        return row.Record;
                    }

                }
            }
            return null;

            //GenericEntity entity = GetRow(db, tableName, primaryKey);
            //if (entity == null)
            //    return null;
            //return entity.Record;
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        public DbTable GetTable(string db, string tableName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            DbSet dbset;
            if (this.m_db.TryGetValue(db, out dbset))
            {
                DbTable table;
                if (dbset.DataSource.TryGetValue(tableName, out table))
                {
                    return table;
                }
            }
            return null;

            //DbSet item = Get(db);
            //if (item == null)
            //    return null;
            //return item.GetTable(tableName);
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        public bool RemoveTable(string db, string tableName)
        {
            DbSet item = Get(db);
            if (item == null)
                return false;
            return item.Remove(tableName);
        }

        /// <summary>
        /// GetItemProperties
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public CacheItemProperties GetItemProperties(string db, string tableName)
        {
            DbSet item = Get(db);
            if (item == null)
                return null;

            DbTable dt = item.GetTable(tableName);
            if (dt == null)
                return null;
            var dtprop = new TableProperties() { RecordCount = dt.Count, Size = dt.Size, ColumnCount = dt.FieldsCount };
            if (item.SyncTables.Contains(tableName))
            {
                return new CacheItemProperties(dtprop, item.SyncTables.Get(tableName));
            }
            else
            {
                return new CacheItemProperties(dtprop, tableName);
            }
        }

        /// <summary>
        /// Get all keys.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllDataKeys()
        {
            List<string> list = new List<string>();
            foreach(var d in m_db.ToArray())
           {
                foreach (var entry in d.Value.DataSource.ToArray())
                {
                    list.Add(ComplexKey.Get(d.Key, entry.Key).ToString());
                }
           }
            return list.ToArray();

            //string[] array = null;
            //array = new string[this.m_db.Keys.Count];
            //this.m_db.Keys.CopyTo(array, 0);
            //return array;
        }

        /// <summary>
        /// Store Remoting Data Item to cache, if item exists override it.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="primaryKey"></param>
        public bool SetTable(string db, DataTable dt,  string tableName, string mappingName, EntitySourceType sourceType, string[] primaryKey)
        {
            DbSet item = GetOrCreate(db);
            if (item == null)
                return false;
            return item.Set(dt, tableName, mappingName,sourceType, primaryKey);
        }

        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="primaryKey"></param>
        public bool AddTable(string db, DataTable dt,  string tableName, string mappingName, EntitySourceType sourceType, string [] primaryKey)
        {
            DbSet item = GetOrCreate(db);
            if (item == null)
                return false;
            return item.Add(dt,  tableName, mappingName,sourceType, primaryKey);
        }

        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        public bool AddTableWithKey(string db, DataTable dt, string tableName, string mappingName, EntitySourceType sourceType)
        {
            DbSet item = GetOrCreate(db);
            if (item == null)
                return false;
            return item.AddWithKey(dt, tableName, mappingName, sourceType);
        }

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="primaryKey"></param>
        /// <param name="syncSource"></param>
        public void AddTableWithSync(string db, DataTable dt,  string tableName, string mappingName, EntitySourceType sourceType, string[] primaryKey, DataSyncEntity syncSource)
        {
            DbSet item = GetOrCreate(db);
            if (item == null)
                return;
            if (AddTable(db,dt, tableName, mappingName,sourceType, primaryKey))
            {
                item.SyncTables.Add(syncSource);
            }
        }

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="primaryKey"></param>
        /// <param name="syncEntity"></param>
        public bool AddTableWithSync(string db, DataTable dt,   string tableName, string mappingName, EntitySourceType sourceType, string[] primaryKey, SyncEntity syncEntity)
        {
            DbSet item = GetOrCreate(db);
            if (item == null)
                return false;
            if (AddTable(db, dt, tableName, mappingName, sourceType, primaryKey))
            {
                return item.SyncTables.Add(new DataSyncEntity(syncEntity))>0;
            }
            return false;
        }

        /// <summary>
        /// Add Item to SyncTables.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="syncSource"></param>
        public void AddSyncItem(string db, DataSyncEntity syncSource)
        {
            DbSet item = GetOrCreate(db);
            if (item == null)
                return;
            item.SyncTables.Add(syncSource);
        }

        /// <summary>
        /// Add Item to SyncTables.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="syncEntity"></param>
        public bool AddSyncItem(string db, SyncEntity syncEntity)
        {
            DbSet item = GetOrCreate(db);
            if (item == null)
                return false;
            return item.SyncTables.Add(new DataSyncEntity(syncEntity)) > 0;
        }

        #endregion

        #region size exchange

        /// <summary>
        /// Get sessions size.
        /// </summary>
        /// <returns></returns>
        public long GetSize()
        {
            long size = 0;
            foreach (var k in m_db.Values)
            {
                if (k != null)
                {
                    size += k.Size;
                }
            }
            return size;
        }

        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeValidate(long newSize)
        {
            if (!CacheSettings.EnableSizeHandler)
                return CacheState.Ok;
            return PerformanceCounter.SizeValidate(newSize);

            //return CacheState.Ok;
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="newSize"></param>
        /// <param name="currentCount"></param>
        /// <param name="newCount"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        internal protected virtual void SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;
            PerformanceCounter.ExchangeSizeAndCountAsync(currentSize, newSize, currentCount, newCount, exchange, CacheSettings.EnableSizeHandler);

            //return CacheState.Ok;
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected virtual void SizeExchage(DbSet oldItem, DbSet newItem)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;
            //return PerformanceCounter.ExchangeSizeAndCount(oldItem.Size, newItem.Size, oldItem.Count, newItem.Count, false, CacheSettings.EnableSizeHandler);

            long oldSize = 0;
            int oldCount = 0;
            long newSize = 0;
            int newCount = 0;

            if (oldItem != null)
            {
                oldSize = oldItem.Size;
                oldCount = oldItem.Count;
            }
            if (newItem != null)
            {
                newSize = newItem.Size;
                newCount = newItem.Count;
            }
            PerformanceCounter.ExchangeSizeAndCountAsync(oldSize, newSize, oldCount, newCount, false, CacheSettings.EnableSizeHandler);

            //return CacheState.Ok;
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
            if (CacheSettings.EnablePerformanceCounter)
            {
                PerformanceCounter.RefreshSize();
            }
        }

        #endregion

        #region size exchange
        /*
        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeValidate(long newSize)
        {
            return CacheState.Ok;
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="newSize"></param>
        /// <param name="currentCount"></param>
        /// <param name="newCount"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        internal protected virtual CacheState SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            return CacheState.Ok;
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeExchage(DbSet oldItem, DbSet newItem)
        {
            return CacheState.Ok;
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
           
        }
        */
        #endregion

        #region static size change

        /*
        internal static void OnSizeChanged(DbTable currentValue, DbTable newValue)
        {
            Task<int[]> task = new Task<int[]>(() => CacheUtil.SizeOf(currentValue, newValue));
            {
                task.Start();
                task.Wait(120000);
                if (task.IsCompleted)
                {
                    int[] result = task.Result;
                    Owner.SizeExchage(result[0], result[1], currentValue == null ? 0 : 1, 1, true);
                    _size += result[0] - result[1];
                }
            }
            task.TryDispose();
        }
        internal static void OnSizeChanged(DbTable value, int oprator)
        {
            Task<long> task = new Task<long>(() => CacheUtil.SizeOf(value));
            {
                task.Start();
                task.Wait(120000);
                if (task.IsCompleted)
                {
                    long newSize = task.Result;
                    if (oprator < 0)
                        Owner.SizeExchage(newSize, 0, 1, 0, true);
                    else
                        Owner.SizeExchage(0, newSize, 0, 1, true);
                    _size += (newSize * oprator);

                }
            }
            task.TryDispose();
        }

        internal static void OnSizeChanged(DbTable ge, int currentSize, int currentCount)
        {
            Task<long> task = new Task<long>(() => CacheUtil.SizeOf(ge));
            {
                task.Start();
                task.Wait(120000);
                if (task.IsCompleted)
                {
                    long newSize = task.Result;
                    Owner.SizeExchage(currentSize, newSize, currentCount, 1, true);
                    _size += (newSize - currentSize);

                }
            }
            task.TryDispose();
        }

        internal static void OnSizeChanged(long currentSize, long newSize, int currentCount, int newCount)
        {
            Task task = new Task(() => Owner.SizeExchage(currentSize, newSize, currentCount, newCount, true));
            {
                task.Start();
                task.Wait(120000);
                if (task.IsCompleted)
                {
                    _size += (newSize - currentSize);
                }
            }
            task.TryDispose();
        }
        */

        #endregion

        #region QueryLoad

        DataCacheQuery _CacheQuery;

        public DataCacheQuery CacheQuery
        {
            get
            {
                if(_CacheQuery==null)
                {
                    _CacheQuery = new DataCacheQuery(this);
                }
                return _CacheQuery;
            }
        }

        public GenericEntity QueryEntity(MessageStream message)
        {
            EntityDbArgs dbArgs = (EntityDbArgs)message.DecodeBody();
            GenericEntity entity = CacheQuery.QueryEntity(message.Identifier, dbArgs.ConnectionKey, dbArgs.MappingName, dbArgs.SourceType, message.Expiration, dbArgs.Keys, dbArgs.GetKeyValueArray());
            return entity;
        }

        public DbTable QueryTable(MessageStream message)
        {
            EntityDbArgs dbArgs = (EntityDbArgs)message.DecodeBody();
            var parameters = dbArgs.GetKeyValueArray();
            DbTable table = CacheQuery.QueryTable(dbArgs.ConnectionKey, dbArgs.MappingName, dbArgs.SourceType, message.Expiration, dbArgs.Keys, parameters);
            return table;
        }

        //public GenericEntity QueryEntity(MessageStream message)
        //{
        //    EntityDbArgs dbArgs = (EntityDbArgs)message.DecodeBody();
        //    GenericEntity entity = CacheQuery.QueryEntity(message.Id,dbArgs.ConnectionKey, dbArgs.MappingName, dbArgs.SourceType, message.Expiration, dbArgs.Keys, dbArgs.GetKeyValueArray());
        //    return entity;
        //}

        //public DbTable QueryLoad(string ConnectionKey, string MappingName, EntitySourceType sourceType, string[] primaryKey, params object[] keyValueParameters)
        //{
        //    string key = TableInfo.GetKey(ConnectionKey, MappingName, keyValueParameters);
        //    DbTable table;
        //    var db = new DbQuery();
        //    if (db.DataSource.TryGetValue(key, out table))
        //    {
        //        return table.Copy();
        //    }

        //    table = DbTable.LoadAsync(ConnectionKey, MappingName, sourceType, m_CommandTimeout, primaryKey, keyValueParameters);
        //    db.Add(table, key);

        //    //table.Owner = source.Owner;
        //    return table.Copy();
        //}

        #endregion

        /// <summary>
        /// Get if db cache contains spesific item by <see cref="ComplexKey"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool Contains(string db, string tableName)
        {
            var dset = Get(db);
            if (dset == null)
                return false;
            return dset.Contains(tableName);
        }

        /// <summary>
        /// Get if db cache contains spesific item by db
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool Contains(string db)
        {
            return m_db.ContainsKey(db);
        }

        internal CacheItemReport GetTimerReport()
        {
            return CacheQuery.GetTimerReport();
        }

        internal CacheItemReport GetItemsReport(string db,string tableName)
        {
            var dset = Get(db);
            if (dset == null)
                return null;
            var table = dset.GetTable(tableName);
            if (table == null)
                return null;
            return new CacheItemReport(table);

        }

        internal KeyValuePair<string, GenericEntity>[] GetEntityItems(string db, string tableName)
        {
            var dset = Get(db);
            if (dset == null)
                return null;
            var table = dset.GetTable(tableName);
            if (table == null)
                return null;
            return table.DataSource.ToArray();
        }

        internal int GetEntityItemsCount(string db, string tableName)
        {
            var dset = Get(db);
            if (dset == null)
                return 0;
            var table = dset.GetTable(tableName);
            if (table == null)
                return 0;
            return table.Count;
        }

        internal string[] GetEntityKeys(string db, string tableName)
        {
            var dset = Get(db);
            if (dset == null)
                return null;
            var table = dset.GetTable(tableName);
            if (table == null)
                return null;
            return table.DataSource.Keys.ToArray();
        }

        internal string[] GetNames(string db)
        {
            var dset = Get(db);
            if (dset == null)
                return null;
            return dset.DataSource.Keys.ToArray();
        }

    }
}
