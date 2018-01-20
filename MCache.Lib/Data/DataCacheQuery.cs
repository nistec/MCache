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
using System.Data;
using System.Collections;
using System.Threading;
using Nistec.Data;
using System.Collections.Generic;
using System.Linq;
using Nistec.Generic;
using Nistec.Caching;
using Nistec.Xml;
using System.Xml;
using Nistec.Data.Factory;
using Nistec.Runtime;
using Nistec.Data.Entities;
using Nistec.Caching.Sync;
using System.Threading.Tasks;
using Nistec.Caching.Config;
using System.Collections.Concurrent;
using System.IO;
using Nistec.Serialization;

namespace Nistec.Caching.Data
{

    public class TableInfo
    {
        public int Expiration { get; set; }
        public string ConnectionKey { get; set; }
        public string MappingName { get; set; }
        //public string Query { get; set; }
        public Generic.KeyValueArgs Parameters { get; set; }

        public string ToSql()
        {
            return SqlFormatter.GetCommandText(MappingName, Parameters.Keys);
        }
        public override string ToString()
        {
            return string.Format("{0}&{1}&{2}", ConnectionKey, MappingName, Parameters == null ? "" : Parameters.ToString());
        }
        public string GettMappingKey()
        {
            return string.Format("{0}&{1}&{2}", ConnectionKey, MappingName,Parameters == null ? "" : Parameters.ToString());
        }
        public static string GetMappingKey(string ConnectionKey, string MappingName, object[] keyValueParameters)
        {
            return string.Format("{0}&{1}&{2}", ConnectionKey, MappingName, keyValueParameters == null ? "" : string.Join(",", keyValueParameters));
        }

        public static Channels.TransStream DoCacheTableQuery(string ConnectionKey, EntitySourceType sourceType, string MappingName,string[] primaryKey, int expiration, object[] keyValueParameters)
        {

            EntityDbArgs arg = new EntityDbArgs()
            {
                ConnectionKey = ConnectionKey,
                Keys = primaryKey,
                MappingName = MappingName,
                SourceType = sourceType,
                Args = KeyValueArgs.Get(keyValueParameters)
            };

            var message = new Channels.GenericMessage(arg)
            {
                Command = Remote.DataCacheCmd.QueryTable,
                Expiration = expiration
            };

            //var message = new Channels.GenericMessage(keyValueParameters)
            //{
            //    Command= Remote.DataCacheCmd.QueryTable,
            //    Expiration= expiration,
            //    Id=KeySet.JoinTrim(primaryKey),
            //    Label=MappingName,
            //    GroupId=ConnectionKey
            //    //TypeName = keyValueParameters.GetType().FullName,
            //    //BodyStream = Channels.GenericMessage.SerializeBody(keyValueParameters)
            //};

            return Server.AgentManager.DbCache.ExecRemote(message);
        }
        public static Channels.TransStream DoCacheEntityQuery(string ConnectionKey, EntitySourceType sourceType, string MappingName, NameValueArgs entityKey, int expiration, object[] keyValueParameters)
        {

            EntityDbArgs arg = new EntityDbArgs()
            {
                ConnectionKey = ConnectionKey,
                Keys = entityKey.Keys.ToArray(),
                MappingName = MappingName,
                SourceType = sourceType,
                Args = KeyValueArgs.Get(keyValueParameters)
            };
            var message = new Channels.GenericMessage(arg)
            {
                Command = Remote.DataCacheCmd.QueryEntity,
                Expiration = expiration,
                Id = entityKey.GetPrimaryKey(),
            };

            //var message = new Channels.GenericMessage(keyValueParameters)
            //{
            //    Command = Remote.DataCacheCmd.QueryEntity,
            //    Expiration = expiration,
            //    Id = entityKey.GetPrimaryKey(),//.JoinTrim(entityKey.Keys.ToArray()),
            //    Label = MappingName,
            //    GroupId = ConnectionKey,
            //    Args= entityKey

            //    //TypeName = keyValueParameters.GetType().FullName,
            //    //BodyStream = Channels.GenericMessage.SerializeBody(keyValueParameters)
            //};

            return Server.AgentManager.DbCache.ExecRemote(message);
        }
    }

    /// <summary>
    ///  Represent an synchronized Data set of tables as a data cache for specific database.
    ///Thru <see cref="IDbContext"/> connector.
    /// </summary>
    public class DataCacheQuery : IDisposable,IDbSet
    {
        #region Load

        //public static GenericEntity GetOrLoadEntity(string key,string ConnectionKey, string MappingName, bool isProcedure, params object[] keyValueParameters)
        //{
        //    DbTable table = GetOrLoad(ConnectionKey, MappingName, Query, keyValueParameters);
        //    return table.GetRow(key);
        //}

        //public static DbTable GetOrLoad(string ConnectionKey, string MappingName, bool isProcedure, params object[] keyValueParameters)
        //{
        //    string key = TableInfo.GetKey(ConnectionKey, MappingName, Query, keyValueParameters);
        //    DbTable table;
        //    var db = new DataCacheQuery();
        //    if(db.DataSource.TryGetValue(key, out table))
        //    {
        //        return table.Copy();
        //    }

        //    table = DbTable.Load(ConnectionKey, MappingName, Query, keyValueParameters);
        //    db.Add(table, key);

        //    //table.Owner = source.Owner;
        //    return table.Copy();
        //}
        //public static GenericEntity GetOrLoadEntity(string key, string ConnectionKey, string MappingName, bool isProcedure, string[] primaryKey, params object[] keyValueParameters)
        //{
        //    DbTable table = GetOrLoad(ConnectionKey, MappingName, isProcedure, primaryKey, keyValueParameters);
        //    return table.GetRow(key);
        //}
        //public static DbTable GetOrLoad(string ConnectionKey, string MappingName, bool isProcedure, string[] primaryKey, params object[] keyValueParameters)
        //{
        //    string key = TableInfo.GetKey(ConnectionKey, MappingName, keyValueParameters);
        //    DbTable table;
        //    var db = new DataCacheQuery();
        //    if (db.DataSource.TryGetValue(key, out table))
        //    {
        //        return table.Copy();
        //    }

        //    table = DbTable.Load(ConnectionKey, MappingName, isProcedure, primaryKey, keyValueParameters);
        //    db.Add(table, key);

        //    //table.Owner = source.Owner;
        //    return table.Copy();
        //}

        #endregion

        #region memebers
        /// <summary>
        /// Default Cache name
        /// </summary>
        public const string DefaultCachename = "McDataCache";

        private long _size;
        private ConcurrentDictionary<string, DbTable> m_ds;
        private string _Name;
        private bool _disposed;
        private int _tableCounts;
        private DataCacheState _state;
        private bool initilized;
        private bool suspend;
        internal DbCache Owner;
        
        bool _EnableNoLock = false;
        /// <summary>
        /// Get indicate whether cache should use with nolock statement.
        /// </summary>
        public bool EnableNoLock
        {
            get { return _EnableNoLock; }
            internal set { _EnableNoLock = value; }
        }

        /// <summary>
        /// Get <see cref="CacheSyncState"/> the sync state.
        /// </summary>
        public CacheSyncState SyncState { get; internal set; }

        /// <summary>
        ///  Wait until the current item is ready for synchronization using timeout for waiting in milliseconds.
        /// </summary>
        /// <param name="timeout">timeout in milliseconds</param>
        public void WaitForReadySyncState(int timeout)
        {
            if (timeout < 1000)
                timeout = 1000;
            int wait_counter = 0;
            while (this.SyncState == CacheSyncState.Started)
            {
                Thread.Sleep(100);
                wait_counter += 100;
                if (wait_counter > timeout)
                {
                    this.SyncState = CacheSyncState.Idle;
                }
            }
        }


        #endregion

        #region Idset impelement

        DbCache IDbSet.Owner { get { return Owner; } }
        string IDbSet.ConnectionKey { get { return null; } }
        void IDbSet.ChangeSizeInternal(int size)
        {
            _size += size;
        }

        #endregion

        #region Events

        /// <summary>
        /// CacheStateChanged
        /// </summary>
        public event EventHandler CacheStateChanged;
        /// <summary>
        /// DataCacheChanging 
        /// </summary>
        public event EventHandler DataCacheChanging;
        /// <summary>
        /// DataCacheChanged
        /// </summary>
        public event EventHandler DataCacheChanged;
        /// <summary>
        /// DataValueChanged
        /// </summary>
        public event EventHandler DataValueChanged;
        /// <summary>
        /// DataException
        /// </summary>
        public event DataCacheExceptionEventHandler DataException;
        /// <summary>
        /// SyncStateChange
        /// </summary>
        public event EventHandler SyncTimeState;
        /// <summary>
        /// SyncDataSourceEventHandler
        /// </summary>
        public event SyncDataSourceChangedEventHandler SyncDataSourceChanged;


        #endregion

        #region Ctor

        public DataCacheQuery(DbCache owner)
        {
            Owner = owner;
            SyncState = CacheSyncState.Idle;
            suspend = false;
            initilized = false;
            m_ds = new ConcurrentDictionary<string, DbTable>();
            _size = 0;
            _disposed = false;
            _tableCounts = 0;
            _state = DataCacheState.Closed;
            Expiration = DefaultExpirationMinute;
            m_Timer = new TimerDispatcher(DefaultSessionSyncIntervalMinute * 60, 0, true);
        }

 
        /// <summary>
        /// Initialize a new instance of data cache.
        /// </summary>
        public DataCacheQuery(DbCache owner, string dsetName):this(owner)
        {
            _Name = dsetName;
        }

  
        #endregion ctor

        #region IDispose

        /// <summary>
        /// Destructor.
        /// </summary>
        ~DataCacheQuery()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

       /// <summary>
       /// Dispose
       /// </summary>
       /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                m_ds.Clear();
                m_ds = null;
               
            }
            this._Name = null;
        }
        #endregion

        #region Setting
      

        /// <summary>
        /// Dump Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        protected void DumpError(string message, Exception ex)
        {
            CacheLogger.Logger.LogAction(CacheAction.DataCache, CacheActionState.Error, message + " error: " + ex.Message);
            System.Diagnostics.Trace.WriteLine(message + " error: " + ex.Message);
        }

        /// <summary>
        /// Synchronize Table in Data Source
        /// </summary>
        public void Refresh(string mappingKey)
        {
            try
            {

                DbTable table;

                if (TryGetTable(mappingKey, out table))
                {
                    var newtable = DbTable.Load(table);
                    Set(newtable, mappingKey,Expiration);
                }

            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorSyncCache);
            }
        }


        #endregion

        #region Timer Sync

        /// <summary>
        /// DEfault Session Sync Interval
        /// </summary>
        public const int DefaultSessionSyncIntervalMinute = 10;
        public readonly int DefaultExpirationMinute = 30;

        TimerDispatcher m_Timer;

        int m_CommandTimeout = 30;

        public int Expiration { get; set; } = 30;

        int GetValidExpiration(int expiration)
        {
            if (expiration > 0)
                return expiration;
            return Expiration;
        }

        /// <summary>
        /// Synchronize Start Event Handler.
        /// </summary>
        public event EventHandler SynchronizeStart;
        /// <summary>
        /// Session Removed Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> TableRemoved;
        /// <summary>
        /// Start Session cache.
        /// </summary>
        public void Start()
        {
            if (!m_Timer.Initialized)
            {
                m_Timer.SyncStarted += new EventHandler(m_Timer_SyncStarted);
                m_Timer.StateChanged += M_Timer_StateChanged;
                m_Timer.Start();
            }
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "DataCacheQuery Started!");
        }
        /// stop session cache.<summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (m_Timer.Initialized)
            {
                m_Timer.SyncStarted -= new EventHandler(m_Timer_SyncStarted);
                m_Timer.StateChanged += M_Timer_StateChanged;
                m_Timer.Stop();
            }
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "DataCacheQuery Stoped!");
        }

        private void M_Timer_StateChanged(object sender, SyncTimerItemEventArgs e)
        {
            if (e.Items == null)
                return;
            foreach (var entry in e.Items)
            {
                if (entry.Value.Source == TimerSource.Data && entry.Value.State == 2)
                {
                    RemoveTableAsync(entry.Key);
                }
            }
        }

        public void RemoveTableAsync(string key)
        {
            try
            {
                Task tsk = new Task(() =>
                {
                    Remove(key);
                });
                {
                    tsk.Start();
                }
                tsk.TryDispose();

            }
            catch (Exception ex)
            {
                DumpError("RemoveTableAsync", ex);
            }
        }

        //void m_Timer_SyncCompleted(object sender, SyncTimeCompletedEventArgs e)
        //{
        //    //Sync(e.Items);

        //    OnSyncSession(e.Items);
        //}

        void m_Timer_SyncStarted(object sender, EventArgs e)
        {
            OnSynchronizeStart(e);
        }

        /// <summary>
        /// On Synchronize Start
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSynchronizeStart(EventArgs e)
        {
            if (this.SynchronizeStart != null)
            {
                this.SynchronizeStart(this, e);
            }
        }

        #endregion

        #region Keys


        /// <summary>
        /// Get all keys from data cache.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllKeys()
        {
           return m_ds.Keys.ToArray();
        }
        
        #endregion

        #region override

        /// <summary>
        /// Get the items (Tables) count of data cache.
        /// </summary>
        public int Count
         {
             get
             {
                 return m_ds.Count;
             }
         }

        /// <summary>
        /// Get the size of data cache in bytes
        /// </summary>
        public long Size
        {
            get
            {
               
                return _size;
            }
        }

        internal void ChangeSizeInternal(int size)
        {
            _size += size;
        }
        private void OnSizeChanged(DbTable currentValue, DbTable newValue)
        {
            long currentSize = currentValue == null ? 0 : currentValue.Size;
            long newSize = newValue == null ? 0 : newValue.Size;

            OnSizeChanged(currentSize,newSize, currentValue == null ? 0 : currentValue.Count, newValue==null? 0: newValue.Count);
        }
        private void OnSizeChanged(DbTable value, int oprator)
        {
            Task<long> task = new Task<long>(() => CacheUtil.SizeOf(value));
            {
                task.Start();
                task.Wait(10000);
                if (task.IsCompleted)
                {
                    long newSize = task.Result;
                    if (oprator < 0)
                        Owner.SizeExchage(newSize, 0, value.Count, 0, true);
                    else
                        Owner.SizeExchage(0, newSize, 0, value.Count, true);
                    _size += (newSize * oprator);

                }
            }
            task.TryDispose();
        }

        private void OnSizeChanged(long currentSize, long newSize, int currentCount, int newCount)
        {
            Task task = new Task(() => Owner.SizeExchage(currentSize, newSize, currentCount, newCount, true));
            {
                task.Start();
                task.Wait(10000);
                if (task.IsCompleted)
                {
                    _size += (newSize- currentSize);
                }
            }
            task.TryDispose();
        }

  
        /// <summary>
        /// On Cache ThreadSetting State Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCacheStateChanged(EventArgs e)
        {
            if (CacheStateChanged != null)
                CacheStateChanged(this, e);
        }

        /// <summary>
        /// On Cache Sync State Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncTimeState(EventArgs e)
        {
            if (SyncTimeState != null)
                SyncTimeState(this, e);
        }

        /// <summary>
        /// On Synchronize Data Source Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncDataSourceChanged(SyncDataSourceChangedEventArgs e)
        {
            if (SyncDataSourceChanged != null)
                SyncDataSourceChanged(this, e);
        }

        


        /// <summary>
        /// On Data Cache Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataCacheChanged(EventArgs e)
        {

            _tableCounts = Count;
            if (_tableCounts > 0)
            {
                _state = DataCacheState.Open;
            }
            else
            {
                _state = DataCacheState.Closed;
            }
 
            if (DataCacheChanged != null)
                DataCacheChanged(this, e);
        }

        /// <summary>
        /// On Data Cache Changing
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataCacheChanging(EventArgs e)
        {
            _state = DataCacheState.Synch;
            if (DataCacheChanging != null)
                DataCacheChanging(this, e);
        }

        /// <summary>
        /// On Data Value Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataValueChanged(EventArgs e)
        {
            if (DataValueChanged != null)
                DataValueChanged(this, e);
        }

        /// <summary>
        /// On DataException
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataException(DataCacheExceptionEventArgs e)
        {
            if (DataException != null)
                DataException(this, e);
        }
        /// <summary>
        /// Raise exception event.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="err"></param>
        public void RaiseException(string msg, DataCacheError err)
       {
            if (DataException != null)
                DataException(this, new DataCacheExceptionEventArgs(msg, err));

            CacheLogger.Logger.LogAction(CacheAction.DataCacheError, CacheActionState.Error, msg);
 
        }

        #endregion

        #region Properties

      
        /// <summary>
        /// Get copy of storage dataset
        /// </summary>
        public ConcurrentDictionary<string, DbTable> DataSource
        {
            get { return m_ds; }
        }

        /// <summary>
        /// Get indicating if DataCacheQuery are Initilized
        /// </summary>
        public bool Initilized
        {
            get { return this.initilized; }

        }


        /// <summary>
        /// Get DataCacheState  
        /// </summary>
        public DataCacheState DataCacheState
        {
            get { return _state; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Suspend layout when Store Data
        /// </summary>
        public void Suspend()
        {
            this.suspend = true;
        }
        /// <summary>
        /// Resume Store Data
        /// </summary>
        /// <param name="resumeCache"></param>
        public void Resume(bool resumeCache)
        {
            this.suspend = false;
            if (_disposed || !resumeCache)
            {
                return;
            }
            try
            {

                OnDataCacheChanging(EventArgs.Empty);
                OnDataCacheChanged(EventArgs.Empty);

            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
            }
        }

        /// <summary>
        /// Get properties of specified table name in data cache..
        /// </summary>
        /// <param name="queryKey"></param>
        /// <returns></returns>
        public CacheItemProperties GetItemProperties(string queryKey)
        {
            DbTable dt = DataSource[queryKey];

            if (dt == null)
                return null;
            var dtprop = new TableProperties() { RecordCount = dt.Count, Size = dt.Size, ColumnCount = dt.FieldsCount };

                return new CacheItemProperties(dtprop, queryKey);
        }
        /// <summary>
        /// Get cache properties.
        /// </summary>
        /// <returns></returns>
        public CacheItemProperties[] GetCacheProperties()
        {
            List<CacheItemProperties> items = new List<CacheItemProperties>();
            foreach(var entry in m_ds)
            {
                var dt = entry.Value;
                var dtprop = new TableProperties() { RecordCount = dt.Count, Size = dt.Size, ColumnCount = dt.FieldsCount };
                    items.Add(new CacheItemProperties(dtprop, entry.Key));
            }

            return items.ToArray();
        }

        #endregion

        #region QueryLoad

        //public GenericEntity QueryEntity(string key, string ConnectionKey, string MappingName, EntitySourceType sourceType, int expiration, params object[] keyValueParameters)
        //{
        //    string mappingKey = TableInfo.GetMappingKey(ConnectionKey, MappingName, keyValueParameters);
        //    DbTable table;
        //    var db = new DataCacheQuery();
        //    if (TryGetTable(mappingKey, out table))
        //    {
        //        return table.GetRow(key);
        //    }
        //    table = DbTable.LoadAsync(ConnectionKey, MappingName, sourceType, m_CommandTimeout, keyValueParameters);
        //    Add(table, mappingKey, expiration);

        //    return table.GetRow(key); ;
        //}

        //public DbTable QueryLoad(string ConnectionKey, string MappingName, EntitySourceType sourceType, int expiration, params object[] keyValueParameters)
        //{
        //    string mappingKey = TableInfo.GetMappingKey(ConnectionKey, MappingName, keyValueParameters);
        //    DbTable table;
        //    var db = new DataCacheQuery();
        //    if (TryGetTable(mappingKey, out table))
        //    {
        //        return table.Copy();
        //    }

        //    table = DbTable.LoadAsync(ConnectionKey, MappingName, sourceType, m_CommandTimeout, keyValueParameters);
        //    Add(table, mappingKey, expiration);

        //    //table.Owner = source.Owner;
        //    return table.Copy();
        //}

        public GenericEntity QueryEntity(string key, string ConnectionKey, string MappingName, EntitySourceType sourceType, int expiration, string[] primaryKey, object[] keyValueParameters)
        {
            string mappingKey = TableInfo.GetMappingKey(ConnectionKey, MappingName, keyValueParameters);

            DbTable table;
            if (TryGetTable(mappingKey, out table))
            {
                return table.GetRow(key);
            }
            table = DbTable.LoadTableAsync(ConnectionKey, MappingName, sourceType, m_CommandTimeout, primaryKey, keyValueParameters);
            if (table == null)
            {
                throw new Exception("Unable to load query table " + MappingName);
            }
            Add(table, mappingKey, expiration);

            return table.GetRow(key);
        }

        public DbTable QueryTable(string ConnectionKey, string MappingName, EntitySourceType sourceType, int expiration, string[] primaryKey, object[] keyValueParameters)
        {
            string mappingKey = TableInfo.GetMappingKey(ConnectionKey, MappingName, keyValueParameters);
            DbTable table;
            if (TryGetTable(mappingKey, out table))
            {
                return table.Copy();
            }

            table = DbTable.LoadTable(ConnectionKey, MappingName, sourceType, m_CommandTimeout, primaryKey, keyValueParameters);
            if (table == null)
            {
                throw new Exception("Unable to load query table "+ MappingName);
            }
            Add(table, mappingKey, expiration);

            //table.Owner = source.Owner;
            return table.Copy();
        }

        #endregion

        #region Add / Remove table

        private bool AddAsync(DbTable table, string mappingKey, int expiration = 60000)
        {

            try
            {
                if (table == null)
                    return false;

                //key = ti.GetKey();
                if (m_ds.ContainsKey(mappingKey))
                {
                    throw new ArgumentException("Table allready exists " + mappingKey);
                }

                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                Task<bool> t = new Task<bool>(() =>
                {
                    if (m_ds.TryAdd(mappingKey, table))
                    {
                        table.Owner = this;
                        m_Timer.AddOrUpdate(TimerSource.Data, mappingKey, GetValidExpiration(expiration), DefaultExpirationMinute);
                        OnSizeChanged(table, 1);
                        return true;
                    }
                    return false;
                });
                {
                    t.Start();
                    t.Wait(m_CommandTimeout);
                    if (t.IsCompleted)
                    {
                        return t.Result;
                    }
                }
                t.TryDispose();

                return false;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Remove data table  from storage
        /// </summary>
        /// <param name="mappingKey">table name</param>
        public bool Remove(string mappingKey)
        {
        //    Task.Factory.StartNew(() => RemoveInternal(tableName));
        //}
        //private void RemoveInternal(string tableName)
        //{
            try
            {

                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                DbTable table;
                if (m_ds.TryRemove(mappingKey, out table))
                {
                    OnSizeChanged(table, -1);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="value">data table to add into the storage</param>
        /// <param name="mappingKey"></param>
        /// <param name="expiration"></param>
        public bool Add(DbTable value, string mappingKey, int expiration)
        {

            if (mappingKey == null)
            {
                throw new ArgumentNullException("Add.mappingKey");
            }
            if (value == null)
            {
                throw new ArgumentNullException("Set.value");
            }

            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                if (m_ds.TryAdd(mappingKey, value))
                {
                    m_Timer.AddOrUpdate(TimerSource.Data, mappingKey, GetValidExpiration(expiration), DefaultExpirationMinute);
                    OnSizeChanged(value, 1);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="value">data table to add into the storage</param>
        /// <param name="mappingKey"></param>
        /// <param name="expiration"></param>
        public bool Set(DbTable value, string mappingKey, int expiration)
        {

            if (mappingKey == null)
            {
                throw new ArgumentNullException("Add.mappingKey");
            }
            if (value == null)
            {
                throw new ArgumentNullException("Set.value");
            }

            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                DbTable table;
                if (m_ds.TryGetValue(mappingKey, out table))
                {
                    m_ds[mappingKey] = value;
                    m_Timer.AddOrUpdate(TimerSource.Data, mappingKey, GetValidExpiration(expiration), DefaultExpirationMinute);
                    OnSizeChanged(table, value);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        #endregion

        #region get and set values

        /// <summary>
        /// Get DataTable primary key from storage by table name.
        /// </summary>
        /// <param name="mappingKey"></param>
        /// <returns></returns>
        public string[] GetTableKeys(string mappingKey)
        {
            DbTable value;
            if (TryGetTable(mappingKey, out value))
            {
                return value.PrimaryKey;
            }
            return null;
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="mappingKey"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool TryGetTable(string mappingKey, out DbTable table)
        {
            if (mappingKey == null)
            {
                table = null;
                throw new ArgumentNullException("mappingKey");
            }

            if (_disposed || _tableCounts == 0)
            {
                table = null;
                //RaiseException("Data cache query is empty", DataCacheError.ErrorInitialized);
                return false;
            }

            if( m_ds.TryGetValue(mappingKey, out table))
            {
                m_Timer.UpdateTimer(mappingKey);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="mappingKey">table name</param>
        /// <returns>DataTable</returns>
        public DbTable GetTable(string mappingKey)
        {

            DbTable table;
            if (TryGetTable(mappingKey, out table))
            {
                return table;
            }
            return null;

        }


        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingKey">table name</param>
        /// <param name="key">table name</param>
        /// <param name="column">column name</param>
        /// <returns></returns>
        public T GetValue<T>(string mappingKey, string key, string column)
        {

            if (mappingKey == null)
            {
                throw new ArgumentNullException("mappingKey");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            try
            {
                DbTable table;
                if (TryGetTable(mappingKey, out table))
                {
                    return table.GetValue<T>(key, column);
                }
                return default(T);
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorColumnNotExist);
                return default(T);
            }
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="mappingKey">table name</param>
        /// <param name="key">table name</param>
        /// <param name="column">column name</param>
        /// <returns>object value</returns>
        public object GetValue(string mappingKey, string key, string column)
        {
            if (mappingKey == null)
            {
                throw new ArgumentNullException("mappingKey");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            try
            {
                DbTable table;
                if (TryGetTable(mappingKey, out table))
                {
                    return table.GetValue(key,column);
                }
                return null;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorColumnNotExist);
                return null;
            }
        }

        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="mappingKey">table name</param>
        /// <param name="key">table name</param>
        /// <returns>Hashtable object</returns>
        public GenericEntity GetRow(string mappingKey, string key)
        {
            if (mappingKey == null)
            {
                throw new ArgumentNullException("mappingKey");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            try
            {
                DbTable value;
                if (TryGetTable(mappingKey, out value))
                {
                    return value.GetRow(key);
                }
                return null;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorColumnNotExist);
                return null;
            }
        }

        #endregion



        /// <summary>
        /// Get if db set contains spesific item by tableName
        /// </summary>
        /// <param name="mappingKey"></param>
        /// <returns></returns>
        public bool Contains(string mappingKey)
        {
            return m_ds.ContainsKey(mappingKey);
        }

        internal CacheItemReport GetTimerReport()
        {
            return m_Timer.GetReport("Data");
        }
    }
}
