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

namespace Nistec.Caching.Data
{
    /// <summary>
    /// Represent a db cache that Hold a multiple <see cref="DataCache"/> items in memory.
    /// It is like a database of database <see cref="DataCache"/> items.
    /// Each <see cref="DataCache"/> item represent a data set of tables in cache.
    /// </summary>
    public class DbCache :IDisposable
    {

        #region members

        ConcurrentDictionary<string, DataCache> m_db;

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
            this.m_db = new ConcurrentDictionary<string, DataCache>(concurrencyLevel, initialCapacity);
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

            XmlNode items = doc.SelectSingleNode("//DbCache");
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

                var db = new Dictionary<string, DataCache>();
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

        internal void AddItem(SyncEntity entity,Dictionary<string, DataCache> db) 
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            DataCache dc;

            if (!db.TryGetValue(entity.ConnectionKey, out dc))
            {
                dc = new DataCache(this,DbCacheName, entity.ConnectionKey);
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

        internal void Reload(Dictionary<string, DataCache> db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("DbCache.Reload.db");
            }

            foreach (var entry in db)
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
            foreach (DataCache dc in m_db.Values)
            {
                dc.Start(IntervalSeconds);
            }
            _Initialized = true;
        }
        /// <summary>
        /// Stop dn cache.
        /// </summary>
        public void Stop()
        {
            foreach (DataCache dc in m_db.Values)
            {
                try
                {
                    dc.Stop();
                }
                catch { }

            }
            _Initialized = false;
        }
        #endregion

        /// <summary>
        /// Get specified <see cref="DataCache"/> from db cache.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public DataCache Get(string db)
        {
            if (db == null)
                db = CacheApiSettings.RemoteDataCacheHostName;//.DbCacheName;
            DataCache item = null;
            if (m_db.TryGetValue(db, out item))
            {
                return item;
            }
            return null;
        }
        /// <summary>
        /// Set a new <see cref="DataCache"/> with a specified key in db cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, DataCache value)
        {
            if (key == null)
                return;
            m_db[key] = value;
        }

        /// <summary>
        /// Set a new <see cref="DataCache"/> with a specified arguments.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="connectionKey"></param>
        /// <param name="syncOption"></param>
        public void Set(string key, string connectionKey, SyncOption syncOption)
        {
            if (key == null)
                return;
            DataCache dc = new DataCache(this,key, connectionKey);
            dc.SyncOption = syncOption;

            Set(key, dc);
        }


        /// <summary>
        /// Set Value into specific row and column in local data table  
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="value">value to set</param>
        public void SetValue(string db, string tableName, string column, string filterExpression, object value)
        {

            DataCache item = Get(db);
            if (item != null)
            {
                item.SetValue(tableName, column, filterExpression, value);
            }

        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns></returns>
        public T GetValue<T>(string db, string tableName, string column, string filterExpression)
        {
            DataCache item = Get(db);
            if (item == null)
                return default(T);
            return item.GetValue<T>(tableName, column, filterExpression);
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>object value</returns>
        public object GetValue(string db, string tableName, string column, string filterExpression)
        {
            DataCache item = Get(db);
            if (item == null)
                return null;
            return item.GetValue(tableName, column, filterExpression);
        }

        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>Hashtable object</returns>
        public IDictionary GetRow(string db, string tableName, string filterExpression)
        {
            DataCache item = Get(db);
            if (item == null)
                return null;
            return item.GetRow(tableName, filterExpression);
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(string db, string tableName)
        {
            DataCache item = Get(db);
            if (item == null)
                return null;
            return item.GetDataTable(tableName);
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        public void RemoveTable(string db, string tableName)
        {
            DataCache item = Get(db);
            if (item == null)
                return;
            item.Remove(tableName);
        }

        /// <summary>
        /// GetItemProperties
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataCacheItem GetItemProperties(string db, string tableName)
        {
            DataCache item = Get(db);
            if (item == null)
                return null;

            DataTable dt = item.DataSource.Tables[tableName];
            if (dt == null)
                return null;
            if (item.SyncTables.Contains(tableName))
            {
                return new DataCacheItem(dt.Copy(), item.SyncTables.Get(tableName));
            }
            else
            {
                return new DataCacheItem(dt.Copy(), tableName);
            }
        }

        /// <summary>
        /// Get all keys.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllDataKeys()
        {
            string[] array = null;
            array = new string[this.m_db.Keys.Count];
            this.m_db.Keys.CopyTo(array, 0);
            return array;
        }

        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        public bool AddDataItem(string db, DataTable dt, string tableName)
        {
            DataCache item = Get(db);
            if (item == null)
                return false;
            return item.Add(dt, tableName);
        }

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="syncSource"></param>
        public void AddDataItem(string db, DataTable dt, string tableName, DataSyncEntity syncSource)
        {
            DataCache item = Get(db);
            if (item == null)
                return;
            if (AddDataItem(db,dt, tableName))
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
        /// <param name="syncEntity"></param>
        public void AddDataItem(string db, DataTable dt, string tableName, SyncEntity syncEntity)
        {
            DataCache item = Get(db);
            if (item == null)
                return;
            if (AddDataItem(db, dt, tableName))
            {
                item.SyncTables.Add(new DataSyncEntity(syncEntity));
            }
        }

        /// <summary>
        /// Add Item to SyncTables.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="syncSource"></param>
        public void AddSyncItem(string db, DataSyncEntity syncSource)
        {
            DataCache item = Get(db);
            if (item == null)
                return;
            item.SyncTables.Add(syncSource);
        }

        /// <summary>
        /// Add Item to SyncTables.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="syncEntity"></param>
        public void AddSyncItem(string db, SyncEntity syncEntity)
        {
            DataCache item = Get(db);
            if (item == null)
                return;
            item.SyncTables.Add(new DataSyncEntity(syncEntity));
        }
        
        #region size exchange

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
        internal protected virtual CacheState SizeExchage(DataCache oldItem, DataCache newItem)
        {
            return CacheState.Ok;
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
           
        }

        #endregion

    }
}
