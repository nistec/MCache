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

   

    /// <summary>
    ///  Represent an synchronized Data set of tables as a data cache for specific database.
    ///The <see cref="DataSynchronizer"/> "Synchronizer" manages the synchronization for each item
    ///in  <see cref="DataSyncList"/> items.
    ///Thru <see cref="IDbContext"/> connector.
    /// </summary>
    public class DbSet : IDisposable,IDataCache, ISyncronizer, ISerialEntity, IDbSet
    {
        public ISyncronizer Parent { get { return this; } }

        public IDataCache Copy()
        {
            return new DbSet()
            {
                _Name = this._Name,
                ConnectionKey = this.ConnectionKey,
                _EnableNoLock = this._EnableNoLock,
                initilized = this.initilized,
                //m_TableList = this.m_TableList,
                Owner = this.Owner,
                //Site = this.Site,
                suspend = this.suspend,
                _SyncOption = this._SyncOption,
                //SyncState = this.SyncState,
                _TableWatcherName = this._TableWatcherName,
                //_CacheSynchronize = this._CacheSynchronize,
                _ClientId = this._ClientId,
                _EnableDataSource = this._EnableDataSource,
                _size = this._size,
                _state = this._state,
                _SyncTables = this._SyncTables.Copy(),
                _tableCounts = this._tableCounts,
                m_ds = new ConcurrentDictionary<string, DbTable>(this.m_ds.ToArray())

            };
        }

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
        private DataSyncList _SyncTables;
        private SyncOption _SyncOption;
        private bool suspend;

        private string _ClientId;
        private string _TableWatcherName;
        //private CacheSynchronizer _CacheSynchronize;
        internal DbCache Owner;

        public IDbContext Db()
        {
            return new DbContext(ConnectionKey);
        }

        /// <summary>
        /// Get the connection key for current database. 
        /// </summary>
        public string ConnectionKey
        {
            get;
            private set;
        }

        internal bool _EnableDataSource = true;
        /// <summary>
        /// Get indicate if Store each table in DataSource 
        /// </summary>
        public bool EnableDataSource
        {
            get { return _EnableDataSource; }
        }

        /// <summary>
        /// Get indicate if Store trigger for each table in DataSource 
        /// </summary>
        public bool EnableTrigger
        {
            get;set;
        }

        /// <summary>
        /// Get indicate if allow sync by event. 
        /// </summary>
        public bool EnableSyncEvent
        {
            get; set;
        }

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

        private DbSet()
        {
            _TableWatcherName = DbWatcher.DefaultWatcherName;
            SyncState = CacheSyncState.Idle;
            suspend = false;
            initilized = false;
            m_ds = new ConcurrentDictionary<string, DbTable>();
            _SyncTables = new DataSyncList(this);
            _size = 0;
            _disposed = false;
            _tableCounts = 0;
            //_IntervalSeconds = 60;
            _state = DataCacheState.Closed;
            _SyncOption = SyncOption.Manual;

            EnableSyncEvent = true;
            EnableTrigger = true;
        }

        /// <summary>
        /// Initialize a new instance of data cache.
        /// </summary>
        private DbSet(string dsetName):this()
        {
            _Name = dsetName;
            _ClientId = Environment.MachineName + "$" + dsetName;
        }


        /// <summary>
        /// Initialize a new instance of data cache using connection key from config.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="connectionKey"></param>
        public DbSet(DbCache db, string connectionKey)
            : this(connectionKey)
        {
            Owner = db;
            ConnectionKey = connectionKey;
            //_DbContext = new DbContext(connectionKey);
        }
 
       
        ///// <summary>
        ///// Synchronize DbSet
        ///// </summary>
        ///// <param name="cache"></param>
        //public void Synchronize(DbSet cache)
        //{
        //    if (initilized)
        //        return;
        //    //this._DbContext = cache.Db;
        //    this.DS = cache.DataSource;

        //    this._CacheName = cache._CacheName;
        //    this._tableCounts = cache._tableCounts;

        //    this._SyncTables = cache._SyncTables;
        //    this._SyncOption = cache._SyncOption;

        //    this._ClientId = cache._ClientId;
        //    this._TableWatcherName = cache._TableWatcherName;
        //    //this._CacheSynchronize = cache._CacheSynchronize;
        //}


        #endregion ctor

        #region IDispose

        /// <summary>
        /// Destructor.
        /// </summary>
        ~DbSet()
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
                //if (_CacheSynchronize != null)
                //{
                //    _CacheSynchronize.Dispose();
                //    _CacheSynchronize=null;
                //}
                if (_SyncTables != null)
                {
                    _SyncTables.Dispose();
                    _SyncTables=null;
                }
                
            }

            this._ClientId=null;
            //this._DbContext = null;
            this._Name = null;
            this._TableWatcherName = null;
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

        #region Setting
        /// <summary>
        /// Create table watcher in database.
        /// </summary>
        /// <param name="tableWatcherName"></param>
        /// <returns></returns>
        public int CreateTableWatcher(string tableWatcherName)
        {
            _TableWatcherName = tableWatcherName;
            return DbWatcher.CreateTableWatcher(this.ConnectionKey, tableWatcherName);
        }
        /// <summary>
        /// Add tables to table watcher.
        /// </summary>
        /// <param name="Tables"></param>
        public void CreateTablesTrigger(params string[] Tables)
        {
            DbWatcher.CreateTablesTrigger(this.ConnectionKey, Tables, TableWatcherName);
        }
        /// <summary>
        /// Create tables trigger.
        /// </summary>
        public void CreateTablesTrigger()
        {
            string[] Tables = this._SyncTables.GetTablesTrigger();
            DbWatcher.CreateTablesTrigger(this.ConnectionKey, Tables, TableWatcherName);
        }
        /// <summary>
        /// Create tables trigger.
        /// </summary>
        /// <param name="checkWatcher"></param>
        /// <param name="tableWatcherName"></param>
        public void CreateTablesTrigger(bool checkWatcher, string tableWatcherName)
        {
            string[] Tables = this._SyncTables.GetTablesTrigger();
            bool enableTrigger = Tables != null && Tables.Length > 0;
            if (!enableTrigger)
                return;
            if (checkWatcher )
            {
                CreateTableWatcher(tableWatcherName);
            }
            DbWatcher.CreateTablesTrigger(this.ConnectionKey, Tables, TableWatcherName);
        }
        
        /// <summary>
        /// Start storage ThreadSetting
        /// </summary>
        /// <param name="intervalSeconds"></param>
        public void Start(int intervalSeconds)
        {
            if (initilized)
                return;

            intervalSeconds = CacheDefaults.GetValidIntervalSeconds(intervalSeconds);

            if (_SyncOption == SyncOption.Auto)
            {
                if (CacheSettings.EnableSyncTypeEventTrigger)
                    CreateTablesTrigger(true, _TableWatcherName);

                DataSyncEntity[] items = _SyncTables.GetItems();
                if (items != null)
                {
                    //_CacheSynchronize.RegisterTables(items);

                    foreach (DataSyncEntity source in items)
                    {
                        //source.RegisterOwner(this);
                        source.SyncSourceChanged += new SyncDataSourceChangedEventHandler(source_SyncSourceChanged);

                    }
                }

                //_CacheSynchronize.Start(intervalSeconds);

            }
            OnCacheStateChanged(EventArgs.Empty);
        }

        void source_SyncSourceChanged(object sender, SyncDataSourceChangedEventArgs e)
        {
            OnSyncDataSourceChanged(e);
        }


        /// <summary>
        /// Stop storage ThreadSetting
        /// </summary>
        public void Stop()
        {
            if (!initilized)
                return;
            initilized = false;

            if (_SyncOption == SyncOption.Auto)
            {
                foreach (DataSyncEntity source in _SyncTables.GetItems())
                {
                    source.SyncSourceChanged -= new SyncDataSourceChangedEventHandler(source_SyncSourceChanged);
                }
               // _CacheSynchronize.Stop();
            }

            OnCacheStateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Restart storage ThreadSetting
        /// </summary>
        internal void RestartThreadSetting(int intervalSeconds)
        {
            Stop();
            Start(intervalSeconds);//Encryption.Enlock());
        }




        /// <summary>
        /// Get storage ThreadSetting
        /// </summary>
        public CacheSettingState ThreadSettingState()
        {
            if (initilized)
            {
                return CacheSettingState.Started;
            }
            return CacheSettingState.Stoped;
        }

        /// <summary>
        /// Synchronize Table in Data Source
        /// </summary>
        public void Refresh(string tableName)
        {
            try
            {

                DbTable table;

                if (TryGetTable(tableName, out table))
                {
                    var newtable = DbTable.Load(table);
                    Set(newtable, tableName);
                }

                //string[] pk = GetTableKeys(tableName);
                //using (IDbCmd dbCmd = DbFactory.Create(ConnectionKey))//this.Db.NewCmd())
                //{

                //    DataTable dtSource = dbCmd.ExecuteDataTable(tableName);
                //    if (dtSource != null)
                //    {
                //        if (EnableDataSource)
                //        {
                //            this.Store(dtSource, tableName, pk);
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorSyncCache);
            }
        }


        /// <summary>
        /// Synchronize All Table in Data Source
        /// </summary>
        public void RefreshDataSource()
        {
            try
            {

                foreach (var entry in m_ds.ToArray())
                {
                    string tableName = entry.Key;
                    var newtable = DbTable.Load(entry.Value);
                    if (EnableDataSource)
                    {
                        Set(newtable, tableName);
                    }
                }


                //using (IDbCmd dbCmd = DbFactory.Create(ConnectionKey))//this.Db.NewCmd())
                //{
                //    foreach (var entry in m_ds)
                //    {
                //        string[] pk = GetTableKeys(entry.Key);

                //        DataTable dtSource = dbCmd.ExecuteDataTable(entry.Key, true);
                //        if (dtSource != null)
                //        {
                //            if (EnableDataSource)
                //            {
                //                this.Store(dtSource, entry.Key, pk);
                //            }
                //        }

                //    }
                //}
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorSyncCache);
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

        public bool IsEqual(IDataCache dc)
        {
            return (this.Name == dc.Name &&
                this.ClientId == dc.ClientId &&
                this.ConnectionKey == dc.ConnectionKey &&
                this.EnableDataSource == dc.EnableDataSource &&
                this.EnableSyncEvent == dc.EnableSyncEvent &&
                this.EnableTrigger == dc.EnableTrigger &&
                this.Parent == dc.Parent &&
                //this.SyncOption==dc.SyncOption&&
                this.SyncTables == dc.SyncTables &&
                this.TableWatcherName == dc.TableWatcherName);

        }

        //bool dataChanged;

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

            //Task<int[]> task = new Task<int[]>(() => CacheUtil.SizeOf(currentValue,newValue));
            //{
            //    task.Start();
            //    task.Wait(120000);
            //    if (task.IsCompleted)
            //    {
            //        int[] result = task.Result;
            //        Owner.SizeExchage(result[0], result[1], currentValue == null ? 0 : 1, 1, true);
            //        _size += result[0] - result[1];
            //    }
            //}
            //task.TryDispose();
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

        //private void OnSizeChanged(DbTable ge, int currentSize, int currentCount)
        //{
        //    Task<long> task = new Task<long>(() => CacheUtil.SizeOf(ge));
        //    {
        //        task.Start();
        //        task.Wait(120000);
        //        if (task.IsCompleted)
        //        {
        //            long newSize = task.Result;
        //            Owner.SizeExchage(currentSize, newSize, currentCount, 1, true);
        //            _size += (newSize - currentSize);

        //        }
        //    }
        //    task.TryDispose();
        //}

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

        //private void OnSizeChanged()
        //{
        //    int currentCount = Count;
        //    Task<long> task = new Task<long>(() => DataCacheUtil.DataSetSize(m_ds));
        //    {
        //        task.Start();
        //        task.Wait(120000);
        //        if (task.IsCompleted)
        //        {
        //            long newSize = task.Result;
        //            Owner.SizeExchage(_size, newSize,currentCount, Count, true);
        //            _size = newSize;

        //        }
        //    }
        //    task.TryDispose();
        //}

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
        /// Get indicating if DbSet are Initilized
        /// </summary>
        public bool Initilized
        {
            get { return this.initilized; }

        }


        /// <summary>
        /// Get or Set SyncOption
        /// </summary>
        public SyncOption SyncOption
        {
            get { return _SyncOption; }
            set
            {
                if (initilized)
                {
                    throw new Exception("Cache was initilized, you can not change SyncOption");
                }
                _SyncOption = value;
            }
        }

    
        /// <summary>
        /// Get or set Cache Name  
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != null)
                {
                    _Name = value;
                }
            }
        }

        /// <summary>
        /// Get DataCacheState  
        /// </summary>
        public DataCacheState DataCacheState
        {
            get { return _state; }
        }

        /// <summary>
        /// Get SyncTables collection
        /// </summary>
        public DataSyncList SyncTables
        {
            get
            {
                return _SyncTables;
            }
        }
  
        /// <summary>
        /// Get the ClientId  (MachineName)
        /// </summary>
        public string ClientId
        {
            get { return _ClientId; }
        }

        /// <summary>
        /// Get or set Table Watcher Name
        /// </summary>
        public string TableWatcherName
        {
            get { return _TableWatcherName; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _TableWatcherName = value;
                }
            }
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
        /*
        /// <summary>
        /// Write Cache To Xml file
        /// </summary>
        /// <param name="fileName">full file name</param>
        /// <param name="mode">XmlWriteMode</param>
        public void CacheToXml(string fileName, XmlWriteMode mode)
        {
            if (fileName.Length == 0)
            {
                RaiseException("fileName Required", DataCacheError.ErrorFileNotFound);
                throw new ArgumentException("fileName Required");
            }
            if (!_disposed)
            {
                DS.WriteXml(fileName, mode);
            }
        }

        /// <summary>
        /// Write Cache To XmlSchema file
        /// </summary>
        /// <param name="fileName">full file name</param>
        public void CacheToXmlSchema(string fileName)
        {
            if (fileName.Length == 0)
            {
                RaiseException("fileName Required", DataCacheError.ErrorFileNotFound);
                throw new ArgumentException("fileName Required");
            }
            if (!_disposed)
            {
                DS.WriteXmlSchema(fileName);
            }
        }
        */
        /// <summary>
        /// Get properties of specified table name in data cache..
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public CacheItemProperties GetItemProperties(string tableName)
        {
            DbTable dt = DataSource[tableName];

            if (dt == null)
                return null;
            var dtprop = new TableProperties() { RecordCount = dt.Count, Size = dt.Size, ColumnCount = dt.FieldsCount };
            if (this._SyncTables.Contains(tableName))
            {
                return new CacheItemProperties(dtprop, this._SyncTables.Get(tableName));
            }
            else
            {
                return new CacheItemProperties(dtprop, tableName);
            }
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
                if (this._SyncTables.Contains(entry.Key))
                {
                    items.Add(new CacheItemProperties(dtprop, this._SyncTables.Get(entry.Key)));
                }
                else
                {
                    items.Add(new CacheItemProperties(dtprop, entry.Key));
                }
            }

            //for (int i = 0; i < DataSource.Count; i++)
            //{
            //    DbTable dt = DataSource.Tables[i];
            //    if (dt == null)
            //        continue;
            //    string tableName = dt.TableName;
            //    if (this._SyncTables.Contains(tableName))
            //    {
            //        items.Add(new DataCacheItem(dt.Copy(), this._SyncTables.Get(tableName)));
            //    }
            //    else
            //    {
            //        items.Add(new DataCacheItem(dt.Copy(), tableName));
            //    }
            //}
            return items.ToArray();
        }

        #endregion

        #region Create Cache
        /*
        /// <summary>
        /// Create Cache from xml file
        /// </summary>
        /// <param name="fileName">full file name</param>
        /// <param name="mode">XmlReadMode</param>
        /// <param name="storageName">storage Name</param>
        public void CreateCache(string fileName, XmlReadMode mode, string storageName)
        {
            try
            {
                if (fileName.Length == 0)
                {
                    throw new ArgumentException("fileName Required");
                }
                
                DataSet ds = m_ds.Clone();
                ds.ReadXml(fileName, mode);
                ds.DataSetName = _CacheName;

                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                _CacheName = storageName;

                lock (DS)
                {
                    DS = ds.Copy();
                }
                OnSizeChanged();
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorCreateCache);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Create new storage from data set
        /// </summary>
        /// <param name="ds">data set</param>
        /// <param name="storageName">storage Name</param>
        public void CreateCache(DataSet ds, string storageName)
        {
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                _CacheName = storageName;

                lock (DS)
                {

                    DS = ds.Copy();
                    DS.DataSetName = _CacheName;
                }
                OnSizeChanged();
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorCreateCache);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }
       
    
        /// <summary>
        /// Store data to storage
        /// </summary>
        /// <param name="tables">array of data tables to add into storage</param>
        /// <param name="tablesName">array of table names</param>
        public bool Add(DataTable[] tables, string[] tablesName)
        {
            try
            {
                if (tables == null)
                    return false;
                
                if (tables.Length != tablesName.Length)
                {
                    throw new ArgumentException("tables length must be equal to tablesName length");
                }

                int i = 0;
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                lock (DS)
                {
                     foreach (DataTable dt in tables)
                    {
                        StoreDataInternal(dt, tablesName[i]);
                        i++;
                    }
                 }
                OnSizeChanged();

                return true;

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
         */


        #endregion

        #region Stor Data

        ///// <summary>
        ///// Store data to storage
        ///// </summary>
        ///// <param name="tables">array of data tables to add into storage</param>
        ///// <param name="tablesName">array of table names</param>
        //public void Store(DataTable[] tables, string[] tablesName)
        //{
        //    try
        //    {

        //        if (tables.Length != tablesName.Length)
        //        {
        //            throw new ArgumentException("tables length must be equal to tablesName length");
        //        }
        //        SyncState = CacheSyncState.Started;
        //        int i = 0;
        //        if (!suspend)
        //            OnDataCacheChanging(EventArgs.Empty);

        //        lock (DS)
        //        {
        //            foreach (DataTable dt in tables)
        //            {
        //                StoreDataInternal(dt, tablesName[i]);
        //                i++;
        //            }
        //         }
        //        OnSizeChanged();
        //    }
        //    catch (Exception ex)
        //    {
        //        RaiseException(ex.Message, DataCacheError.ErrorStoreData);
        //    }
        //    finally
        //    {
        //        SyncState = CacheSyncState.Idle;
        //        if (!suspend)
        //            OnDataCacheChanged(EventArgs.Empty);
        //    }
        //}

        //private void StoreDataInternal(DataTable dt, string tableName)
        //{
        //    if (dt == null)
        //        return;

        //    DbTable entity;
        //    if (m_ds.ContainsKey(tableName))
        //    {
        //        m_ds.TryRemove(tableName, out entity);
        //    }
        //    dt.TableName = tableName;
        //    DbTable table = DbTable.Creat(dt);
        //    m_ds[]
        //    GenericEntity edt=new GenericEntity()
        //    DS.Tables.Add(dt.Copy());
        //    AddKey(tableName);
        //}


        ///// <summary>
        ///// Store data table to storage
        ///// </summary>
        ///// <param name="dt">data table to add into the storage</param>
        ///// <param name="tableName">table name</param>
        //public void Store(DataTable dt, string tableName)
        //{
        //    try
        //    {
        //        if (dt == null)
        //            return;
        //        SyncState = CacheSyncState.Started;
        //        if (!suspend)
        //            OnDataCacheChanging(EventArgs.Empty);

        //        DbTable table = DbTable.Creat(dt);

        //        lock (DS)
        //        {
        //            if (DS.Tables.Contains(tableName))
        //            {
        //                DS.Tables.Remove(tableName);
        //            }
        //            dt.TableName = tableName;
        //            DS.Tables.Add(dt.Copy());
        //        }
        //        AddKey(tableName);
        //        OnSizeChanged();
        //    }
        //    catch (Exception ex)
        //    {
        //        RaiseException(ex.Message, DataCacheError.ErrorStoreData);
        //    }
        //    finally
        //    {
        //        SyncState = CacheSyncState.Idle;
        //        if (!suspend)
        //            OnDataCacheChanged(EventArgs.Empty);
        //    }
        //}

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="mappingName">maooing name in database</param>
        /// <param name="sourceType">sourceType</param>
        /// <param name="primaryKey">table name</param>
        public void Store(DataTable dt, string tableName, string mappingName, EntitySourceType sourceType, string[] primaryKey)
        {
            Set(dt, tableName, mappingName, sourceType, primaryKey);
        }

        /// <summary>
        /// Store data table WithKey to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="mappingName">maooing name in database</param>
        /// <param name="sourceType">sourceType</param>
        public void Store(DataTable dt, string tableName, string mappingName, EntitySourceType sourceType)
        {
            SetWithKey(dt, tableName, mappingName,sourceType);
        }
        /// <summary>
        /// Store data table WithKey to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="mappingName">maooing name in database</param>
        public void Store(DataTable dt, string tableName, string mappingName)
        {
            SetWithKey(dt, tableName, mappingName, EntitySourceType.Table);
        }
        #endregion

        #region Add / Remove table


        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="mappingName">maooing name in database</param>
        /// <param name="sourceType">table name</param>
        /// <param name="primaryKey"></param>
        /// <param name="timeout">table name</param>
        public bool Add(DataTable dt, string tableName, string mappingName, EntitySourceType sourceType, string[] primaryKey, int timeout = 60000)
        {
            try
            {
                if (dt == null)
                    return false;

                if (m_ds.ContainsKey(tableName))
                {
                    throw new ArgumentException("Table allready exists " + tableName);
                }

                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                Task<bool> t = new Task<bool>(() =>
                {
                    DbTable table = DbTable.Creat(dt, mappingName, sourceType,primaryKey);
                    if (m_ds.TryAdd(tableName, table))
                    {
                        table.Owner = this;
                        OnSizeChanged(table, 1);
                        return true;
                    }
                    return false;
                });
                {
                    t.Start();
                    t.Wait(timeout);
                    if (t.IsCompleted)
                    {
                        return t.Result;
                    }
                }
                t.TryDispose();

                return false;

                //DbTable cur;
                //if (m_ds.TryGetValue(tableName, out cur))
                //{

                //}
                //table.Owner = this;
                //m_ds[tableName] = table;
                //OnSizeChanged(cur.Size,table.Size,1,1);
                //return true;

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
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="mappingName">maooing name in database</param>
        /// <param name="timeout">table name</param>
        public bool AddWithKey(DataTable dt,  string tableName, string mappingName, EntitySourceType sourceType, int timeout = 60000)
        {
            try
            {
                if (dt == null)
                    return false;

                if (m_ds.ContainsKey(tableName))
                {
                    throw new ArgumentException("Table allready exists " + tableName);
                }
                dt.TableName = tableName;
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                Task<bool> t = new Task<bool>(() =>
                {

                    DbTable table = DbTable.CreatWithKey(dt, mappingName,sourceType);
                    if (m_ds.TryAdd(tableName, table))
                    {
                        table.Owner = this;
                        table.MappingName = mappingName;
                        OnSizeChanged(table, 1);
                        return true;
                    }
                    return false;
                });
                {
                    t.Start();
                    t.Wait(timeout);
                    if (t.IsCompleted)
                    {
                        return t.Result;
                    }
                }
                t.TryDispose();

                return false;
                //DbTable cur;
                //if (m_ds.TryGetValue(tableName, out cur))
                //{

                //}
                //table.Owner = this;
                //m_ds[tableName] = table;
                //OnSizeChanged(cur.Size,table.Size,1,1);
                //return true;

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
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="mappingName">maooing name in database</param>
        /// <param name="sourceType"></param>
        /// <param name="primaryKey"></param>
        /// <param name="timeout"></param>
        public bool Set(DataTable dt, string tableName, string mappingName, EntitySourceType sourceType, string[] primaryKey,int timeout=60000)
        {
            try
            {
                if (dt == null)
                    return false;
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                Task<bool> t = new Task<bool>(() =>
                {
                    DbTable table = DbTable.Creat(dt, mappingName, sourceType, primaryKey);
                    table.Owner = this;

                    DbTable cur;
                    long curSize = 0;
                    if (m_ds.TryGetValue(tableName, out cur))
                    {
                        curSize = cur.Size;
                    }
                    m_ds[tableName] = table;
                    OnSizeChanged(curSize, table.Size, cur==null?0:cur.Count, table==null?0:table.Count);
                    return true;
                });
                {
                    t.Start();
                    t.Wait(timeout);
                    if (t.IsCompleted)
                    {
                        return t.Result;
                    }
                }
                t.TryDispose();

                return false;


                //DbTable table = DbTable.Creat(dt, primaryKey);
                //table.Owner = this;

                //DbTable cur;
                //long curSize = 0;
                //if (m_ds.TryGetValue(tableName, out cur))
                //{
                //    curSize = cur.Size;
                //}
                //m_ds[tableName] = table;
                //OnSizeChanged(curSize, table.Size, 1, 1);
                //return true;

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
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="mappingName">maooing name in database</param>
        /// <param name="sourceType"></param>
        /// <param name="timeout"></param>
        public bool SetWithKey(DataTable dt, string tableName, string mappingName, EntitySourceType sourceType, int timeout = 60000)
        {
            try
            {
                if (dt == null)
                    return false;
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);


                Task<bool> t = new Task<bool>(() =>
                {
                    DbTable table = DbTable.CreatWithKey(dt, mappingName, sourceType);
                    table.Owner = this;

                    DbTable cur;
                    long curSize = 0;
                    if (m_ds.TryGetValue(tableName, out cur))
                    {
                        curSize = cur.Size;
                    }
                    m_ds[tableName] = table;
                    OnSizeChanged(curSize, table.Size, 1, 1);
                    return true;
                });
                {
                    t.Start();
                    t.Wait(timeout);
                    if (t.IsCompleted)
                    {
                        return t.Result;
                    }
                }
                t.TryDispose();

                return false;


                //DbTable table = DbTable.CreatWithKey(dt);
                //table.Owner = this;

                //DbTable cur;
                //long curSize = 0;
                //if (m_ds.TryGetValue(tableName, out cur))
                //{
                //    curSize = cur.Size;
                //}
                //m_ds[tableName] = table;
                //OnSizeChanged(curSize, table.Size, 1, 1);
                //return true;

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
        /// <param name="tableName">table name</param>
        public bool Remove(string tableName)
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
                if (m_ds.TryRemove(tableName, out table))
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
        /// <param name="tableName"></param>
        public bool Add(DbTable value, string tableName)
        {

            if (tableName == null)
            {
                throw new ArgumentNullException("Add.tableName");
            }
            if (value == null)
            {
                throw new ArgumentNullException("Set.value");
            }

            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                if (m_ds.TryAdd(tableName, value))
                {
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
        /// <param name="tableName"></param>
        public bool Set(DbTable value, string tableName)
        {

            if (tableName == null)
            {
                throw new ArgumentNullException("Add.tableName");
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
                if (m_ds.TryGetValue(tableName, out table))
                {
                    m_ds[tableName] = value;
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

        #region Add / Remove value

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="value">data table to add into the storage</param>
        /// <param name="tableName"></param>
        public bool AddValue(GenericEntity value, string tableName)
        {

            if (tableName == null)
            {
                throw new ArgumentNullException("Add.tableName");
            }
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                DbTable table;
                if (m_ds.TryGetValue(tableName, out table))
                {
                    if(table.Add(value))
                    {
                        _size += value.Size();
                        return true;
                    }
                }
                return false;

                //DbTable tab=GetTable(tableName);
                //return tab.Add(value);
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
        /// Set Value into local data table  
        /// </summary>
        /// <param name="value">value to set</param>
        /// <param name="tableName">value to set</param>
        public bool SetValue(GenericEntity value, string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("Set.value");
            }
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                DbTable table;
                if (m_ds.TryGetValue(tableName, out table))
                {
                    return table.Set(value);
                }
                return false;

                //DbTable tab = GetTable(tableName);
                //return tab.Set(value);
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
        /// Add Value into local data table  
        /// </summary>
        /// <param name="tableName">value to set</param>
        /// <param name="primaryKey">primary key</param>
        /// <param name="field">primary key</param>
        /// <param name="value">value to set</param>
        public bool AddValue(string tableName, string primaryKey, string field, object value)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("Set.value");
            }
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                DbTable table;
                if (m_ds.TryGetValue(tableName, out table))
                {
                    return table.Add(primaryKey, field, value);
                }
                return false;

                //DbTable tab = GetTable(tableName);
                //return tab.Set(primaryKey, field,value);
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
        /// Set Value into local data table  
        /// </summary>
        /// <param name="tableName">value to set</param>
        /// <param name="primaryKey">primary key</param>
        /// <param name="field">primary key</param>
        /// <param name="value">value to set</param>
        public bool SetValue(string tableName, string primaryKey, string field, object value)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("Set.value");
            }
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                DbTable table;
                if (m_ds.TryGetValue(tableName, out table))
                {
                    return table.Set(primaryKey, field, value);
                }
                return false;

                //DbTable tab = GetTable(tableName);
                //return tab.Set(primaryKey, field,value);
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
        /// <param name="tableName">value to set</param>
        /// <param name="key">table name</param>
        public bool RemoveValue(string tableName, string key)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("Set.value");
            }

            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                DbTable table;
                if (m_ds.TryGetValue(tableName, out table))
                {
                    if (table.Remove(key))
                    {
                        return true;
                    }
                }
                return false;

                //DbTable tab = GetTable(tableName);
                //return tab.Remove(key);
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
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string[] GetTableKeys(string tableName)
        {
            DbTable value;
            if (TryGetTable(tableName, out value))
            {
                return value.PrimaryKey;
            }
            return null;
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool TryGetTable(string tableName, out DbTable table)
        {
            if (tableName == null)
            {
                table = null;
                throw new ArgumentNullException("tableName");
            }

            if (_disposed || _tableCounts == 0)
            {
                table = null;
                RaiseException("DatSet is disposed", DataCacheError.ErrorInitialized);
                return false;
            }
            return m_ds.TryGetValue(tableName, out table);
        }
           
        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        public DbTable GetTable(string tableName)
        {

            DbTable table;
            if (TryGetTable(tableName, out table))
            {
                return table;
            }
            return null;

            //if (_disposed || _tableCounts == 0)
            //{
            //    RaiseException("DatSet is disposed", DataCacheError.ErrorInitialized);
            //    return null;
            //}
            //if (tableName == null)
            //{
            //    throw new ArgumentNullException("tableName");
            //}
            //try
            //{
            //    DbTable value;
            //    if (TryGetTable(tableName, out value))
            //    {
            //        return value;
            //    }
            //    return null;
            //}
            //catch (Exception ex)
            //{
            //    RaiseException(ex.Message, DataCacheError.ErrorTableNotExist);
            //    return null;
            //}
        }

 
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">table name</param>
        /// <param name="key">table name</param>
        /// <param name="column">column name</param>
        /// <returns></returns>
        public T GetValue<T>(string tableName, string key, string column)
        {

            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
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
                if (m_ds.TryGetValue(tableName, out table))
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
        /// <param name="tableName">table name</param>
        /// <param name="key">table name</param>
        /// <param name="column">column name</param>
        /// <returns>object value</returns>
        public object GetValue(string tableName, string key, string column)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
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
                if (m_ds.TryGetValue(tableName, out table))
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
        /// <param name="tableName">table name</param>
        /// <param name="key">table name</param>
        /// <returns>Hashtable object</returns>
        public GenericEntity GetRow(string tableName, string key)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            try
            {
                DbTable value;
                if (m_ds.TryGetValue(tableName, out value))
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

        #region IserialEntity

        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteString(ClientId);
            streamer.WriteString(ConnectionKey);
            streamer.WriteValue(EnableDataSource);
            streamer.WriteValue(EnableNoLock);
            streamer.WriteValue(EnableSyncEvent);
            streamer.WriteValue(EnableTrigger);
            streamer.WriteString(Name);
            streamer.WriteValue(Size);
            streamer.WriteValue((int)SyncOption);
            streamer.WriteValue((int)SyncState);
            streamer.WriteValue(SyncTables);
            streamer.WriteString(TableWatcherName);
            streamer.WriteValue(DataSource);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            _ClientId = streamer.ReadString();
            ConnectionKey = streamer.ReadString();
            _EnableDataSource = streamer.ReadValue<bool>();
            EnableNoLock = streamer.ReadValue<bool>();
            EnableSyncEvent = streamer.ReadValue<bool>();
            EnableTrigger = streamer.ReadValue<bool>();
            Name = streamer.ReadString();
            SyncOption =(SyncOption) streamer.ReadValue<int>();
            SyncState = (CacheSyncState)streamer.ReadValue<int>();
            _SyncTables =(DataSyncList) streamer.ReadValue();
            TableWatcherName = streamer.ReadString();
            var dictionary = (Dictionary<string, DbTable>)streamer.ReadValue();
            m_ds = new ConcurrentDictionary<string, DbTable>(dictionary.ToArray());
        }
        #endregion

        /// <summary>
        /// Get if db set contains spesific item by tableName
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool Contains(string tableName)
        {
            return m_ds.ContainsKey(tableName);
        }
    }
}
