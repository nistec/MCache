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

using Nistec.Caching;
using Nistec.Xml;
using System.Xml;
using Nistec.Data.Factory;
using Nistec.Runtime;
using Nistec.Data.Entities;
using Nistec.Caching.Sync;
using System.Threading.Tasks;
using Nistec.Caching.Config;


namespace Nistec.Caching.Data
{

   

    /// <summary>
    ///  Represent an synchronized Data set of tables as a data cache for specific database.
    ///The <see cref="CacheSynchronizer"/> "Synchronizer" manages the synchronization for each item
    ///in  <see cref="DataSyncList"/> items.
    ///Thru <see cref="IDbContext"/> connector.
    /// </summary>
    public class DataCache : System.ComponentModel.Component, IDataCache
    {
        #region memebers
        /// <summary>
        /// Default Cache name
        /// </summary>
        public const string DefaultCachename = "McDataCache";

        private long _size;
        private DataSet m_ds;
        private string _CacheName;
        private bool _disposed;
        private int _tableCounts;
        private DataCacheState _state;
        private bool initilized;
        private DataSyncList _SyncTables;
        private SyncOption _SyncOption;
        private bool suspend;

        private string _ClientId;
        private string _TableWatcherName;
        private CacheSynchronizer _CacheSynchronize;
        DbCache Owner;

        //IDbContext _DbContext;
        ///// <summary>
        ///// Get db as <see cref="IDbContext"/>.
        ///// </summary>
        //public IDbContext Db
        //{
        //    get { return _DbContext; }
        //}

       
        ///// <summary>
        ///// Get the connection key for current database. 
        ///// </summary>
        //public string ConnectionKey
        //{
        //    get { return Db.ConnectionName; }
        //}

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

        /// <summary>
        /// Initialize a new instance of data cache.
        /// </summary>
        private DataCache(string cacheName)
        {
            _CacheName = cacheName;
            _ClientId = Environment.MachineName + "$" + cacheName;
            _TableWatcherName = DbWatcher.DefaultWatcherName;
            SyncState = CacheSyncState.Idle;
            suspend = false;
            initilized = false;
            m_ds = new DataSet();
            _SyncTables = new DataSyncList(this);
            _CacheSynchronize = new CacheSynchronizer(this);
            _size = 0;
            _disposed = false;
            _tableCounts = 0;
            //_IntervalSeconds = 60;
            _state = DataCacheState.Closed;
            _SyncOption = SyncOption.Manual;

            m_ds.Disposed += new EventHandler(_ds_Disposed);
        }

     
        ///// <summary>
        ///// Initialize a new instance of data cache using connection string and <see cref="DBProvider"/>.
        ///// </summary>
        ///// <param name="owner"></param>
        ///// <param name="cacheName"></param>
        ///// <param name="connection"></param>
        ///// <param name="providerDb"></param>
        //public DataCache(DbCache owner,string cacheName, string connection, DBProvider providerDb)
        //    : this(cacheName)
        //{
        //    Owner = owner;
        //    _DbContext = new DbContext(connection, providerDb);
        //}
 

        /// <summary>
        /// Initialize a new instance of data cache using connection key from config.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="cacheName"></param>
        /// <param name="connectionKey"></param>
        public DataCache(DbCache owner, string cacheName, string connectionKey)
            : this(cacheName)
        {
            Owner = owner;
            ConnectionKey = connectionKey;
            //_DbContext = new DbContext(connectionKey);
        }
 
        ///// <summary>
        ///// Initialize a new instance of data cache using <see cref="AutoDb"/>.
        ///// </summary>
        ///// <param name="owner"></param>
        ///// <param name="cacheName"></param>
        ///// <param name="dalDB"></param>
        //public DataCache(DbCache owner, string cacheName, AutoDb dalDB)
        //    : this(cacheName)
        //{
        //    Owner = owner;
        //    _DbContext = new DbContext( dalDB.Connection.ConnectionString, dalDB.DBProvider);
        //}

        /// <summary>
        /// Synchronize DataCache
        /// </summary>
        /// <param name="cache"></param>
        public void Synchronize(DataCache cache)
        {
            if (initilized)
                return;
            //this._DbContext = cache.Db;
            this.DS = cache.DataSource;

            this._CacheName = cache._CacheName;
            this._tableCounts = cache._tableCounts;

            this._SyncTables = cache._SyncTables;
            this._SyncOption = cache._SyncOption;

            this._ClientId = cache._ClientId;
            this._TableWatcherName = cache._TableWatcherName;
            this._CacheSynchronize = cache._CacheSynchronize;
        }


        #endregion ctor

        #region Load xml config
        /// <summary>
        /// Load data cache from xml config file.
        /// </summary>
        /// <param name="file"></param>
        public void LoadXmlConfigFile(string file)
        {

            if (string.IsNullOrEmpty(file))
                return;
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(file);
            LoadXmlConfig(doc);
        }
        /// <summary>
        /// Load data cache from xml string argument.
        /// </summary>
        /// <param name="xml"></param>
        public void LoadXmlConfig(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            LoadXmlConfig(doc);
        }

        /// <summary>
        /// Load data cache from <see cref="System.Xml.XmlDocument"/> dicument.
        /// </summary>
        /// <param name="doc"></param>
        public void LoadXmlConfig(System.Xml.XmlDocument doc)
        {

            //  <RemoteData>
            //  <Settings>
            //    <ConnectionString value ="Data Source=MCONTROL; Initial Catalog=Northwind; uid=sa;password=tishma; Connection Timeout=30"/>
            //    <Provider value ="SqlServer"/>
            //    <DataCacheName value ="McRemoteData"/>
            //    <LoadRemoteSettings value ="true"/>
            //  </Settings>

            //  <DataSource>
            //    <Table Name="Customers">
            //      <MappingName value="Customers"/>
            //      <SyncType value="Interval"/>
            //      <SyncTime value="0:20:0"/>
            //    </Table>
            //  </DataSource>
            //</RemoteData>

            if (doc == null)
                return;

            System.Xml.XmlNode root = null;
            System.Xml.XmlNode settings = null;
            System.Xml.XmlNode dataSource = null;
            try
            {
                XmlParser parser = new XmlParser(doc);
                root = parser.SelectSingleNode("//RemoteData", true);
                settings = parser.SelectSingleNode(root, "//Settings", true);
                string connection = parser.GetAttributeValue(settings, "ConnectionString", "value", "");
                DBProvider provider = DbFactory.GetProvider(parser.GetAttributeValue(settings, "Provider", "value", "SqlServer"));
                this.CacheName = parser.GetAttributeValue(settings, "DataCacheName", "value", DataCache.DefaultCachename);
                if (string.IsNullOrEmpty(connection))
                    return;
                bool useWatcher = Types.ToBool(parser.GetAttributeValue(settings, "UseTableWatcher", "value", "false"), false);


                dataSource = parser.SelectSingleNode(root, "//DataSource", false);
                if (dataSource == null)
                    return;
                XmlNodeList list = dataSource.ChildNodes;
                if (list == null)
                    return;
                foreach (XmlNode n in list)
                {
                    if (n.NodeType == XmlNodeType.Comment)
                        continue;

                    LoadDbCacheItem(new SyncEntity(n));//new DataCacheEntity(n));
                }
                if (useWatcher)
                {
                    DbWatcher.CreateTableWatcher(this.ConnectionKey);
                }
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorReadFromXml);
            }
        }

        /// <summary>
        /// Load data cache from <see cref="DataCacheSettings"/> properties.
        /// </summary>
        /// <param name="prop"></param>
        public void LoadXmlConfig(DataCacheSettings prop)
        {
            //dbCmd = DbFactory.Create(prop.ConnectionString, prop.Provider);
            this.CacheName = prop.DataCacheName;
            if (prop.UseTableWatcher)
            {
                CreateTableWatcher(TableWatcherName);
            }
            if (!string.IsNullOrEmpty(prop.Xmlsettings))
            {
                LoadDbCacheItems(prop.GetItemsSettings());
            }
        }
        /// <summary>
        /// Load <see cref="SyncEntity"/> entity to data cache.
        /// </summary>
        /// <param name="item"></param>
        public virtual void LoadDbCacheItem(SyncEntity item)
        {
            try
            {

                DataTable dt = null;
                using (IDbCmd dbCmd = DbFactory.Create(ConnectionKey))// Db.NewCmd())
                {
                    dt = dbCmd.ExecuteDataTable(item.EntityName, "SELECT * FROM " + item.ViewName, false);
                }
                if (dt == null)
                {
                    throw new Exception("db cmd error for table: " + item.EntityName);
                }
                this.Add(dt, item.EntityName);
                if (item.SyncType != SyncType.None)
                {
                    this.SyncTables.Add(item);
                }

            }
            catch (Exception ex)
            {
                RaiseException("LoadDbCacheItem error : " + ex.Message, DataCacheError.ErrorSyncCache);
            }
        }
        /// <summary>
        /// Load array of <see cref="SyncEntity"/> to data cache.
        /// </summary>
        /// <param name="items"></param>
        public void LoadDbCacheItems(SyncEntity[] items)//DataCacheEntity[] items)
        {
            if (items == null)
                return;

            try
            {
                foreach (var item in items)
                {
                    LoadDbCacheItem(item);
                }

            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorReadFromXml);
            }

        }
        #endregion load xml config

        #region IDispose

        /// <summary>
        /// Destructor.
        /// </summary>
        ~DataCache()
        {
            Dispose(false);
        }

       /// <summary>
        /// Dispose
       /// </summary>
       /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_ds.Dispose();
                Stop();

                if (_CacheSynchronize != null)
                {
                    _CacheSynchronize.Dispose();
                    _CacheSynchronize=null;
                }
                 if (_SyncTables != null)
                {
                    _SyncTables.Dispose();
                    _SyncTables=null;
                }
                
            }
            this._ClientId=null;
            //this._DbContext = null;
            this._CacheName = null;
            this._TableWatcherName = null;
            this.CacheName = null;
            

            base.Dispose(disposing);
        }

        private void _ds_Disposed(object sender, EventArgs e)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
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

                _CacheSynchronize.Start(intervalSeconds);

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
                _CacheSynchronize.Stop();
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
        /// Synchronize All Table in Data Source
        /// </summary>
        public void RefreshDataSource()
        {
            try
            {
                using (IDbCmd dbCmd = DbFactory.Create(ConnectionKey))//this.Db.NewCmd())
                {
                    foreach (DataTable dt in DS.Tables)
                    {
                        DataTable dtSource = dbCmd.ExecuteDataTable(dt.TableName);
                        if (dtSource != null)
                        {
                            if (EnableDataSource)
                            {
                                this.Store(dtSource, dt.TableName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorSyncCache);
            }
        }

        #endregion

       
        #region Keys

        List<string> m_TableList = new List<string>();
        /// <summary>
        /// Get all keys from data cache.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllKeys()
        {
           return m_TableList.ToArray();
        }
        private void AddKey(string name)
        {
            if (m_TableList.Contains(name))
                return;
            m_TableList.Add(name);
        }
        private void RemoveKey(string name)
        {
            m_TableList.Remove(name);
        }
        /// <summary>
        /// Set all keys.
        /// </summary>
        public void SetAllKeys()
        {
            //AsyncInvoke("setallkeys", null);

            Task.Factory.StartNew(() => SetAllKeysInternal()); 
        }
         private void SetAllKeysInternal()
        {
            int count = DS.Tables.Count;
            string[] list = new string[count];
            for (int i = 0; i < count; i++)
            {
                list[i] = DS.Tables[i].TableName;
            }
            m_TableList.Clear();
            m_TableList.AddRange( list);
        }
        #endregion

        #region override

        //bool dataChanged;

         /// <summary>
        /// Get the items (Tables) count of data cache.
        /// </summary>
         public int Count
         {
             get
             {
                 return m_ds.Tables.Count;
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

        private void OnSizeChanged()
        {
            int currentCount = Count;
            using (Task<long> task = new Task<long>(() => DataCacheUtil.DataSetSize(m_ds)))
            {
                task.Start();
                task.Wait(120000);
                if (task.IsCompleted)
                {
                    long newSize = task.Result;
                    Owner.SizeExchage(_size, newSize,currentCount, Count, true);
                    _size = newSize;
                   
                }
            }
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
 
            DS.AcceptChanges();

            _tableCounts = DS.Tables.Count;
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

        #region data set methods

        /// <summary>
        /// Get copy of storage dataset in xml
        /// </summary>
        public string GetXml()
        {
 
            return DS.GetXml();
        }

        /// <summary>
        /// HasChanges
        /// </summary>
        public bool HasChanges()
        {

            return DS.HasChanges();
        }

        /// <summary>
        /// Get copy of storage dataset changes
        /// </summary>
        public DataSet GetChanges()
        {
  
            return DS.GetChanges();
        }

        /// <summary>
        /// Update all changes in data sorce
        /// </summary>
        /// <returns></returns>
        public void UpdateChanges()//IDbConnection cnn)
        {
           Task.Factory.StartNew(() => UpdateChangesInternal()); 

        }
        private int UpdateChangesInternal()//IDbConnection cnn)
        {
     
            int res = 0;
            using (IDbCmd dbCmd = DbFactory.Create(ConnectionKey))//this.Db.NewCmd())
            {
                foreach (DataTable dt in DS.Tables)
                {
                    DataTable dtChanges = dt.GetChanges();
                    res += dbCmd.Adapter.UpdateChanges(dt);
                    dt.AcceptChanges();
                }
            }
            return res;
        }


        /// <summary>
        /// Update changes for Specific table in data sorce
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void UpdateChanges(/*IDbConnection cnn,*/ string tableName)
        {
            Task.Factory.StartNew(() => UpdateChangesInternal(tableName)); 
        }
        private int UpdateChangesInternal(/*IDbConnection cnn,*/ string tableName)
        {
            DataTable dt = DS.Tables[tableName];
            if (dt == null)
                return 0;

            int res = 0;
            using (IDbCmd dbCmd = DbFactory.Create(ConnectionKey))//this.Db.NewCmd())
            {
                res = dbCmd.Adapter.UpdateChanges(dt);
            }
            dt.AcceptChanges();
            return res;
        }

        /// <summary>
        /// Reject changes in data sorce
        /// </summary>
        /// <param name="tableName"></param>
        public void RejectChanges(string tableName)
        {
            DataTable dt = DS.Tables[tableName];
            if (dt != null)
                dt.RejectChanges();
        }

        /// <summary>
        /// Reject all changes in data sorce
        /// </summary>
        public void RejectAllChanges()
        {
            foreach (DataTable dt in DS.Tables)
            {
                dt.RejectChanges();
            }
        }
        #endregion

        #region Properties

        private DataSet DS
        {
            get 
            {
                if (m_ds == null)
                {
                    m_ds = new DataSet();
                }
                return m_ds; 
            }
            set 
            {
                m_ds = value;
                SetAllKeys();
            }
        }

        /// <summary>
        /// Get copy of storage dataset
        /// </summary>
        public DataSet DataSource
        {
            get { return DS.Copy(); }
        }

        /// <summary>
        /// Get indicating if DataCache are Initilized
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
        public string CacheName
        {
            get { return _CacheName; }
            set
            {
                if (value != null)
                {
                    _CacheName = value;
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
        /// <summary>
        /// Get properties of specified table name in data cache..
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataCacheItem GetItemProperties(string tableName)
        {
            DataTable dt = DataSource.Tables[tableName];
            if (dt == null)
                return null;
            if (this._SyncTables.Contains(tableName))
            {
                return new DataCacheItem(dt.Copy(), this._SyncTables.Get(tableName));
            }
            else
            {
                return new DataCacheItem(dt.Copy(), tableName);
            }
        }
        /// <summary>
        /// Get cache properties.
        /// </summary>
        /// <returns></returns>
        public DataCacheItem[] GetCacheProperties()
        {
            List<DataCacheItem> items = new List<DataCacheItem>();
            for (int i = 0; i < DataSource.Tables.Count; i++)
            {
                DataTable dt = DataSource.Tables[i];
                if (dt == null)
                    continue;
                string tableName = dt.TableName;
                if (this._SyncTables.Contains(tableName))
                {
                    items.Add(new DataCacheItem(dt.Copy(), this._SyncTables.Get(tableName)));
                }
                else
                {
                    items.Add(new DataCacheItem(dt.Copy(), tableName));
                }
            }
            return items.ToArray();
        }

        #endregion

        #region Create Cache
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
                    return false; ;
                
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



        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        public bool Add(DataTable dt, string tableName)
        {
            return Add(dt, tableName, false);
        }

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="force">should replace data table if exists</param>
        public bool Add(DataTable dt, string tableName, bool force)
        {
            try
            {
                if (dt == null)
                    return false;
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                lock (DS)
                {
                    if (DS.Tables.Contains(tableName))
                    {
                        if (force)
                        {
                            DS.Tables.Remove(tableName);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    dt.TableName = tableName;
                    DS.Tables.Add(dt);
                }
                AddKey(tableName);
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

        /// <summary>
        /// Remove data table  from storage
        /// </summary>
        /// <param name="tableName">table name</param>
        public void Remove(string tableName)
        {
            Task.Factory.StartNew(() => RemoveInternal(tableName)); 
        }
        private void RemoveInternal(string tableName)
        {
            try
            {
               
                if (DS.Tables.Contains(tableName))
                {
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    lock (DS)
                    {
                         DS.Tables.Remove(tableName);
                        RemoveKey(tableName);
                    }
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        #endregion

        #region Stor Data

        /// <summary>
        /// Store data to storage
        /// </summary>
        /// <param name="tables">array of data tables to add into storage</param>
        /// <param name="tablesName">array of table names</param>
        public void Store(DataTable[] tables, string[] tablesName)
        {
            try
            {
               
                if (tables.Length != tablesName.Length)
                {
                    throw new ArgumentException("tables length must be equal to tablesName length");
                }
                SyncState = CacheSyncState.Started;
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
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
            }
            finally
            {
                SyncState = CacheSyncState.Idle;
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        private void StoreDataInternal(DataTable dt, string tableName)
        {
            if (dt == null)
                return;
            if (DS.Tables.Contains(tableName))
            {
                DS.Tables.Remove(tableName);
            }
            dt.TableName = tableName;
            DS.Tables.Add(dt.Copy());
            AddKey(tableName);
        }


        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        public void Store(DataTable dt, string tableName)
        {
            try
            {
                if (dt == null)
                    return;
                SyncState = CacheSyncState.Started;
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                lock (DS)
                {
                    if (DS.Tables.Contains(tableName))
                    {
                        DS.Tables.Remove(tableName);
                    }
                    dt.TableName = tableName;
                    DS.Tables.Add(dt.Copy());
                }
                AddKey(tableName);
                OnSizeChanged();
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
            }
            finally
            {
                SyncState = CacheSyncState.Idle;
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }


        #endregion

        #region Merge

        /// <summary>
        /// Merge Cache with data set source
        /// </summary>
        /// <param name="ds">data set source</param>
        /// <param name="storageName">storage Name</param>
        /// <param name="preserveChanges">preserving or discarding changes in the data set </param>
        public void Merge(DataSet ds, string storageName, bool preserveChanges)
        {
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                _CacheName = storageName;
 
                lock (DS)
                {
                     DS.Merge(ds.Copy(), preserveChanges);
                    DS.DataSetName = _CacheName;
                }
                SetAllKeys();
                OnSizeChanged();
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorMergeData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Merge Cache with data table source
        /// </summary>
        /// <param name="dt">merges data table into storage</param>
        /// <param name="tableName">tablename</param>
        /// <param name="preserveChanges">preserving or discarding changes in the data set </param>
        /// <param name="schemaAction">handling an incopatible schema accordingto the given arguments </param>
        public void Merge(DataTable dt, string tableName, bool preserveChanges, MissingSchemaAction schemaAction)
        {
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);
                lock (DS)
                {
                    dt.TableName = tableName;
                    DS.Merge(dt.Clone(), preserveChanges, schemaAction);
                }
                SetAllKeys();
                OnSizeChanged();
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorMergeData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Merge Cache with data table source
        /// </summary>
        /// <param name="dt">merges data table into storage</param>
        /// <param name="tableName">tablename</param>
        public void Merge(DataTable dt, string tableName)
        {
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                lock (DS)
                {
                     dt.TableName = tableName;
                    DS.Merge(dt.Clone());
                }
                AddKey(tableName);
                OnSizeChanged();
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorMergeData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Merge Cache with data rows source
        /// </summary>
        /// <param name="rows">merges an array of data rows source into storage </param>
        /// <param name="preserveChanges">preserving or discarding changes in the data set </param>
        /// <param name="schemaAction">handling an incopatible schema accordingto the given arguments </param>
        public void Merge(DataRow[] rows, bool preserveChanges, MissingSchemaAction schemaAction)
        {
            try
            {
                if (!suspend)
                    OnDataCacheChanging(EventArgs.Empty);

                lock (DS)
                {
                    DS.Merge(rows, preserveChanges, schemaAction);
                }
                OnSizeChanged();
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorMergeData);
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
        /// Set Value into specific row and column in local data table  
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="value">value to set</param>
        public void SetValue(string tableName, string column, string filterExpression, object value)
        {
            DataRow dr = GetDataRow(tableName, filterExpression);
            try
            {
                if (dr != null)
                {
                    lock (dr)
                    {
                        dr[column] = value;
                    }
                    DS.Tables[tableName].AcceptChanges();
                    OnDataValueChanged(EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorSetValue);
            }
        }
           
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns></returns>
        public T GetValue<T>(string tableName, string column, string filterExpression)
        {
            DataRow dr = GetDataRow(tableName, filterExpression);
            if (dr == null)
                return default(T);
            try
            {
                return dr.Get<T>(column);
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
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>object value</returns>
        public object GetValue(string tableName, string column, string filterExpression)
        {
            DataRow dr = GetDataRow(tableName, filterExpression);
            if (dr == null)
                return null;
            try
            {
                return dr[column];
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
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>Hashtable object</returns>
        public IDictionary GetRow(string tableName, string filterExpression)
        {
            DataRow dr = GetDataRow(tableName, filterExpression);
            if (dr == null)
                return null;
            return DataUtil.DataRowToHashtable(dr);
        }


        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>Data row object</returns>
        public DataRow GetDataRow(string tableName, string filterExpression)
        {
            DataRow[] drs = GetDataRows(tableName, filterExpression, "");
            if (drs == null || drs.Length == 0)
                return null;
            return drs[0];
        }
        /// <summary>
        /// Get array of dataRows from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="sort">sort fileld</param>
        /// <returns>array of data rows </returns>
        public DataRow[] GetDataRows(string tableName, string filterExpression, string sort)
        {
            if (_disposed || _tableCounts == 0)
            {
                RaiseException("DatSet is disposed", DataCacheError.ErrorInitialized);
                return null;
            }
            DataRow[] drs = null;
            DataTable dt = null;

            dt = DS.Tables[tableName];
            if (dt == null)
            {
                RaiseException("Table " + tableName + " not found. ", DataCacheError.ErrorTableNotExist);
                return null;
            }
            try
            {
                drs = dt.Select(filterExpression, sort);
                return drs;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorInFilterExspression);
                return null;
            }
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(string tableName)
        {
            if (_disposed || _tableCounts == 0)
            {
                RaiseException("DatSet is disposed", DataCacheError.ErrorInitialized);
                return null;
            }
            try
            {
                return DS.Tables[tableName];
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorTableNotExist);
                return null;
            }
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="index">table index</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(int index)
        {
            if (_disposed || _tableCounts == 0)
            {
                RaiseException("DatSet is disposed", DataCacheError.ErrorInitialized);
                return null;
            }
            try
            {
                return DS.Tables[index];
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorTableNotExist);
                return null;
            }
        }

        /// <summary>
        /// Get DataView from storage by table name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns>DataView</returns>
        public DataView this[string tableName]
        {
            get
            {
                DataTable dt = GetDataTable(tableName);
                if (dt == null)
                    return null;
                return dt.DefaultView;
            }
        }

        /// <summary>
        /// Get DataView from storage by table index.
        /// </summary>
        /// <param name="index">table index</param>
        /// <returns>DataView</returns>
        public DataView this[int index]
        {
            get
            {
                DataTable dt = GetDataTable(index);
                if (dt == null)
                    return null;
                return dt.DefaultView;
            }
        }


        #endregion

    }
}
