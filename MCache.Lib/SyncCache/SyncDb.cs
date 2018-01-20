﻿//licHeader
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
using Nistec.Data;
using System.IO;
using Nistec.Caching.Data;
using System.Data;
using Nistec.Caching.Remote;
using Nistec.Data.Entities;
using Nistec.Data.Factory;
using System.Threading;
using Nistec.Caching.Config;

namespace Nistec.Caching.Sync
{
    ///// Represent a db cache that Hold a multiple <see cref="DataCache"/> items in memory.
    ///// It is like a database of database <see cref="DataCache"/> items.
    ///// Each <see cref="DataCache"/> item represent a data set of tables in cache.
    ///// </summary>
    
    /// <summary>
    /// Represent Synchronize db cache that Hold meny tables for synchronization by Event or Interval.
    /// </summary>
    [Serializable]
    public class SyncDb : IDataCache
    {

        public IDataCache Copy()
        {
                        
            //var syncCopy = this._SyncTables.Copy();
            //syncCopy.Owner = this;

            return new SyncDb()
            {
                //Owner=this.Owner,
                Name = this.Name,
                _EnableNoLock = this._EnableNoLock,
                initilized = this.initilized,
                _SyncOption = this._SyncOption,
                SyncState = this.SyncState,
                _TableWatcherName = this._TableWatcherName,
                //.._CacheSynchronize = this._CacheSynchronize,//.Copy(),
                _ClientId = this.ClientId,
                _EnableTrigger = this._EnableTrigger,
                _EnableSyncEvent=this._EnableSyncEvent,
                _state = this._state,
                _storageName = this._storageName,
                _SyncTables = this._SyncTables//.Copy()
            };
        }

        #region memebers
        /// <summary>
        /// Default sync cache name.
        /// </summary>
        public const string DefaultCachename = "SyncDbCache";

        private string _storageName;
        private DataCacheState _state;
        private bool initilized;
        private DataSyncList _SyncTables;
        private SyncOption _SyncOption;

        private string _ClientId;
        private string _TableWatcherName;
        //..private CacheSynchronizer _CacheSynchronize;
        private bool _EnableTrigger;
        bool _EnableSyncEvent;
       // internal ISyncLoader Owner;
        #endregion

        #region IDataCache

        public ISyncronizer Parent { get; internal set; }

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
        /// Cache State Changed event
        /// </summary>
        public event EventHandler CacheStateChanged;
        /// <summary>
        /// DataException
        /// </summary>
        public event DataCacheExceptionEventHandler DataException;
        /// <summary>
        /// Sync State Change event
        /// </summary>
        public event EventHandler SyncTimeState;

        //evt-
        /// <summary>
        /// Sync Data Source EventHandler
        /// </summary>
        public event SyncDataSourceChangedEventHandler SyncDataSourceChanged;
       
        /// <summary>
        /// On Synchronize Data Source Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncDataSourceChanged(SyncDataSourceChangedEventArgs e)
        {
            CacheLogger.Debug("SyncDb OnSyncDataSourceChanged : " + e.SourceName);

            if (SyncDataSourceChanged != null)
                SyncDataSourceChanged(this, e);
        }
       
        void source_SyncSourceChanged(object sender, SyncDataSourceChangedEventArgs e)
        {
            OnSyncDataSourceChanged(e);
        }

        #endregion

        #region Ctor

        private SyncDb()
        {

        }

        //private SyncDb(string connectionKey)
        //{
        //     SyncState = CacheSyncState.Idle;
        //     _storageName = connectionKey;
        //    _ClientId = Environment.MachineName + "$" + _storageName;
        //    _TableWatcherName = DbWatcher.DefaultWatcherName;
        //    initilized = false;
        //    _SyncTables = new DataSyncList(this);
        //    _CacheSynchronize = new CacheSynchronizer(this);
        //    _state = DataCacheState.Closed;
        //    _SyncOption = SyncOption.Manual;
        //}

        /// <summary>
        /// SyncDb Ctor 
        /// </summary>
        /// <param name="connectionKey"></param>
        public SyncDb(string connectionKey)
        {
            //Owner = owner;SyncCacheBase owner,
            SyncState = CacheSyncState.Idle;
            _EnableTrigger = CacheSettings.EnableSyncTypeEventTrigger;
            _EnableSyncEvent = CacheSettings.EnableSyncTypeEvent;
            _storageName = connectionKey;
            _ClientId = Environment.MachineName + "$" + _storageName;
            _TableWatcherName = DbWatcher.DefaultWatcherName;
            initilized = false;
            _SyncTables = new DataSyncList(this);
            //.._CacheSynchronize = new CacheSynchronizer(this);
            _state = DataCacheState.Closed;
            _SyncOption = SyncOption.Manual;
            //_DbContext = new DbContext(connectionKey);
        }

        /// <summary>
        /// SyncDb Ctor 
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="syncOption"></param>
        /// <param name="enableTrigger"></param>
        /// <param name="enableSyncEvent"></param>
        public SyncDb( string connectionKey, SyncOption syncOption,bool enableTrigger, bool enableSyncEvent)
        {
            //Owner = owner;SyncCacheBase owner,
            SyncState = CacheSyncState.Idle;
            _EnableTrigger = enableTrigger;
            _EnableSyncEvent = enableSyncEvent;
            _storageName = connectionKey;
            _ClientId = Environment.MachineName + "$" + _storageName;
            _TableWatcherName = DbWatcher.DefaultWatcherName;
            initilized = false;
            _SyncTables = new DataSyncList(this);
            //.._CacheSynchronize = new CacheSynchronizer(this);
            _state = DataCacheState.Closed;
            _SyncOption = syncOption;
            //_DbContext = new DbContext(connectionKey);
        }


        ///// <summary>
        ///// SyncDb Ctor 
        ///// </summary>
        ///// <param name="cacheName"></param>
        ///// <param name="connection"></param>
        ///// <param name="providerDb"></param>
        //public SyncDb(string cacheName, string connection, DBProvider providerDb)
        //    : this(cacheName)
        //{
        //    ConnectionKey
        //    _DbContext = new DbContext(connection, providerDb);
        //}

        ///// <summary>
        ///// SyncDb Ctor 
        ///// </summary>
        ///// <param name="cacheName"></param>
        ///// <param name="dalDB"></param>
        //public SyncDb(string connectionName, AutoDb dalDB)
        //    : this(connectionName)
        //{
        //    _DbContext = new DbContext(dalDB.Connection.ConnectionString, dalDB.DBProvider);
        //}

        #endregion ctor

        #region IDispose

        /// <summary>
        /// SyncDb
        /// </summary>
        ~SyncDb()
        {
            Dispose(false);
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
                //..
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
            this._storageName = null;
            this._TableWatcherName = null;
            this.Name = null;
            
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void DisposeCopy()
        {
            //..
            //if (_CacheSynchronize != null)
            //{
            //    _CacheSynchronize.DisposeCopy();
            //    _CacheSynchronize = null;
            //}

            //if (_SyncTables != null)
            //{
            //    _SyncTables.Dispose();
            //    _SyncTables = null;
            //}
            GC.SuppressFinalize(this);
        }

        #endregion

        #region internal Setting

        internal int CreateTableWatcher(string tableWatcherName)
        {
            _TableWatcherName = tableWatcherName;
            return DbWatcher.CreateTableWatcher(this.ConnectionKey, tableWatcherName);
        }

        internal void CreateTablesTrigger(params string[] Tables)
        {
            DbWatcher.CreateTablesTrigger(this.ConnectionKey, Tables, TableWatcherName);
        }

        internal void CreateTablesTrigger()
        {
            string[] Tables = this._SyncTables.GetTablesTrigger();
            DbWatcher.CreateTablesTrigger(this.ConnectionKey, Tables, TableWatcherName);
        }

        internal void CreateTablesTrigger(bool checkWatcher, string tableWatcherName)
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
        /// Start Cache Synchronization
        /// </summary>
        /// <param name="intervalSeconds"></param>
        public void Start(int intervalSeconds)
        {
            if (initilized)
            {
                //..StartSynchronize(intervalSeconds);
                return;
            }

            if (_SyncOption == SyncOption.Auto)
            {

                //bool enableTrigger = _EnableTrigger;// CacheSettings.EnableSyncTypeEventTrigger;

                if (_EnableTrigger)
                    CreateTablesTrigger(true, _TableWatcherName);
                else if(_EnableSyncEvent)
                    CreateTableWatcher(_TableWatcherName);

                DataSyncEntity[] items = _SyncTables.GetItems();
                if (items != null && items.Length > 0)
                {
                    foreach (DataSyncEntity source in items)
                    {
                        source.SyncSourceChanged += new SyncDataSourceChangedEventHandler(source_SyncSourceChanged);

                    }
                }

                //if (enableTrigger)
                //..StartSynchronize(intervalSeconds);
                
            }
            initilized = true;
            OnCacheStateChanged(EventArgs.Empty);
        }


       

        internal void RegisterTable(DataSyncEntity entity)
        {
            //.._CacheSynchronize.RegisteredTableEvent(entity);

            SyncTables.Set(entity);
            DataSynchronizer.Global.RegisterSyncTable(entity, this);
        }

        //internal void StartSynchronize(int intervalSeconds)
        //{
        //    //_CacheSynchronize.Start(intervalSeconds);
        //}

        /// <summary>
        /// Stop Cache Synchronization
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
                    //evt-
                    source.SyncSourceChanged -= new SyncDataSourceChangedEventHandler(source_SyncSourceChanged);
                }
                //.._CacheSynchronize.Stop();
            }

            OnCacheStateChanged(EventArgs.Empty);
        }


        #endregion

        #region Properties
        /*
        IDbContext _DbContext;
        /// <summary>
        /// Get <see cref="IDbContext"/> databse context.
        /// </summary>
        public IDbContext Db
        {
            get { return _DbContext; }
        }

        /// <summary>
        /// Get the connection key for current database. 
        /// </summary>
        public string ConnectionKey
        {
            get { return Db.ConnectionName; }
        }
         */

        public IDbContext Db()
        {
            return DbContext.Create(ConnectionKey, CacheSettings.EnableConnectionProvider);
        }

        /// <summary>
        /// Get the connection key for current database. 
        /// </summary>
        public string ConnectionKey
        {
            get { return _storageName; }
            //private set;
        }

        /// <summary>
        /// Get indicate if Store each table in DataSource 
        /// </summary>
        public bool EnableDataSource
        {
            get { return false; }
        }

        /// <summary>
        /// Get indicate if Store trigger for each table in DataSource 
        /// </summary>
        public bool EnableTrigger
        {
            get { return _EnableTrigger; }
        }
        /// <summary>
        /// Get indicate if allow sync by event. 
        /// </summary>
        public bool EnableSyncEvent
        {
            get { return _EnableSyncEvent; }
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
        /// Get indicating if DataCache are Initilized
        /// </summary>
        public bool Initilized
        {
            get { return this.initilized; }

        }

        /// <summary>
        /// Get or Set Synchronization Option
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
            get { return _storageName; }
            set
            {
                if (value != null)
                {
                    _storageName = value;
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
        /// Get Registred Tables for synchronization with DB
        /// </summary>
        public DataSyncList SyncTables
        {
            get
            {
                return _SyncTables;
            }
        }

        /// <summary>
        /// Determines whether a HashSet list contains the specified element.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsExists(SyncEntity entity)
        {
            if (SyncTables == null)
                return false;

            return SyncTables.IsExists(entity);

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

        #region override

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="mappingName">maooing name in database</param>
        /// <param name="tableName">table name</param>
        public void Store(DataTable dt,string mappingName, string tableName)
        {
            //Parent.Store(dt, tableName);
            Console.WriteLine("SyncDb Store");
        }
        /// <summary>
        /// Refresh specific item in sync cache.
        /// </summary>
        /// <param name="syncName"></param>
        public void Refresh(string syncName)
        {
            Parent.Refresh(syncName);
            Console.WriteLine("SyncDb Refresh " + syncName);
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
        /// On DataException
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataException(DataCacheExceptionEventArgs e)
        {
            if (DataException != null)
                DataException(this, e);
        }

        /// <summary>
        /// Raise <see cref="DataCacheExceptionEventHandler"/>
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

        #region Data cache

        public bool IsEqual(IDataCache dc)
        {
            return (this.Name==dc.Name &&
                this.ClientId==dc.ClientId&&
                this.ConnectionKey==dc.ConnectionKey&&
                this.EnableDataSource==dc.EnableDataSource &&
                this.EnableSyncEvent==dc.EnableSyncEvent &&
                this.EnableTrigger==dc.EnableTrigger&&
                this.Parent==dc.Parent&&
                //this.SyncOption==dc.SyncOption&&
                this.SyncTables==dc.SyncTables&&
                this.TableWatcherName==dc.TableWatcherName);
               
        }


        /// <summary>
        /// Add Item to SyncTables
        /// </summary>
        /// <param name="syncSource"></param>
        public void AddSyncItem(DataSyncEntity syncSource)
        {
            this.SyncTables.Add(syncSource);
        }
        
        #endregion

    }
}
