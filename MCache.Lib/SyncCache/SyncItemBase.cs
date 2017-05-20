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
using System.Xml;
using System.Data;
using System.Runtime.Serialization;
using Nistec.Data.Entities;
using System.Collections;
using Nistec.Data;
using Nistec.Caching.Data;
using Nistec.Data.Entities.Cache;
using Nistec.Threading;
using Nistec.Caching.Server;
using Nistec.Generic;
using System.Threading.Tasks;
using System.Threading;
using Nistec.Caching.Config;

namespace Nistec.Caching.Sync
{
   
   /// <summary>
    /// Represent the sychronize item info for each item in sync cache
   /// </summary>
   /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class SyncItemBase<T> : ISyncCacheItem,IDisposable
    {
        internal static object syncLock = new object();
        internal static object taskLock = new object();
 
        internal const int TaskerTimeoutSeconds = 600;
        /// <summary>
        /// Get indicate if is ready to use.
        /// </summary>
        public bool IsReady { get; internal set; }
        internal bool IsAsync { get; private set; }
        internal bool IsTimeout { get; set; }
        
        #region ctor

        /// <summary>
        /// Create new <see cref="SyncItemBase{T}"/>
        /// </summary>
        /// <param name="isAsync"></param>
        public SyncItemBase(bool isAsync) 
        {
            if (typeof(T).IsInterface)
            {
                throw new InvalidCastException("typeof(T) should not be an interface");
            }
            IsReady = false;
            IsAsync = isAsync;
            IsTimeout = false;
        }

        /// <summary>
        /// Create new <see cref="SyncItemBase{T}"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isAsync"></param>
        public SyncItemBase(SyncEntity entity, bool isAsync)
            : this(isAsync)
        {

            Info = new CacheKeyInfo() { ItemKeys = entity.EntityKeys, ItemName = entity.EntityName };

            SyncSource = new DataSyncEntity(entity);//entity.EntityName, entity.ViewName, entity.SourceName, new SyncTimer(entity.Interval, entity.SyncType));

            SetContext(entity.ConnectionKey, entity.EntityName, entity.ViewName, entity.SourceType, EntityKeys.Get(entity.EntityKeys));
        }

        void ISyncCacheItem.Set(SyncEntity entity, bool isAsync)
        {

            IsReady = false;
            IsAsync = isAsync;
            IsTimeout = false;

            Info = new CacheKeyInfo() { ItemKeys = entity.EntityKeys, ItemName = entity.EntityName };

            SyncSource = new DataSyncEntity(entity);//entity.EntityName, entity.ViewName, entity.SourceName, new SyncTimer(entity.Interval, entity.SyncType));

            SetContext(entity.ConnectionKey, entity.EntityName, entity.ViewName, entity.SourceType, EntityKeys.Get(entity.EntityKeys));
        }

        /// <summary>
        /// Create new <see cref="SyncItemBase{T}"/>
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="isAsync"></param>
        public SyncItemBase(string connectionKey, string entityName, string mappingName, string[] keys, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : this(isAsync)
        {
            Set(connectionKey, entityName, mappingName, new string[] { mappingName }, EntitySourceType.Table, keys, timer, enableNoLock, commandTimeout);
        }

        /// <summary>
        /// Create new <see cref="SyncItemBase{T}"/>
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="isAsync"></param>
        public SyncItemBase(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] keys, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : this(isAsync)
        {
            Set(connectionKey, entityName, mappingName, sourceName, sourceType, keys, timer,enableNoLock, commandTimeout);
        }
        /// <summary>
        /// Set current item.
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public void Set<Dbc>(string entityName, string mappingName, string[] keys, SyncTimer timer, bool enableNoLock, int commandTimeout) where Dbc : IDbContext
        {
            Info = new CacheKeyInfo() { ItemKeys = keys, ItemName = entityName };//mappingName

           
            SyncSource = new DataSyncEntity(
                new Sync.SyncEntity()
                {
                    EntityName = entityName,
                    ViewName = mappingName,
                    SourceName = new string[] { mappingName },
                    PreserveChanges = false,
                    MissingSchemaAction = MissingSchemaAction.Add,
                    SyncType = timer.SyncType,
                    Interval = timer.Interval,
                    EnableNoLock = enableNoLock,
                    CommandTimeout=commandTimeout
                }, timer);

            SetContext<Dbc>(mappingName, EntityKeys.Get(keys));
        }

        /// <summary>
        /// Set current item.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public void Set(string connectionKey, string entityName, string mappingName, string[] keys, SyncTimer timer, bool enableNoLock,int commandTimeout)
        {
            Set(connectionKey, entityName, mappingName, new string[] { mappingName }, EntitySourceType.Table, keys, timer, enableNoLock, commandTimeout);
        }
        /// <summary>
        /// Set current item.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public void Set(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] keys, SyncTimer timer, bool enableNoLock, int commandTimeout)
        {
            Info = new CacheKeyInfo() { ItemKeys = keys, ItemName = entityName };//mappingName

           

            SyncSource = new DataSyncEntity(
                new Sync.SyncEntity()
                {
                    EntityName = entityName,
                    ViewName = mappingName,
                    SourceName = new string[] { mappingName },
                    PreserveChanges = false,
                    MissingSchemaAction = MissingSchemaAction.Add,
                    SyncType = timer.SyncType,
                    Interval = timer.Interval,
                    EnableNoLock = enableNoLock,
                    CommandTimeout = commandTimeout
                }, timer);

            SetContext(connectionKey, entityName, mappingName, sourceType, EntityKeys.Get(keys));
        }

       
        #endregion

        #region IDisposable
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._Context != null)
                {
                    this._Context.Dispose();
                    this._Context=null;
                }
                if (this.SyncSource != null)
                {
                    this.SyncSource.Dispose();
                    this.SyncSource=null;
                }
            }
            this.ConnectionKey = null;
            this.Filter = null;
            this.Info = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get or Set CacheKeyInfo
        /// </summary>
        public CacheKeyInfo Info
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set <see cref="DataSyncEntity"/>
        /// </summary>
        public DataSyncEntity SyncSource
        {
            get;
            set;
        }


        internal EntityContext<T> _Context;
        /// <summary>
        /// Get <see cref="EntityContext"/>
        /// </summary>
        public EntityContext<T> Context
        {
            get
            {
                if (_Context == null)
                {
                    _Context = new EntityContext<T>();
                }
                return _Context;
            }
        }

        /// <summary>
        /// Get or Set <see cref="DataFilter"/>
        /// </summary>
        public DataFilter Filter
        {
            get;
            set;
        }

        
        /// <summary>
        /// Get db connection key from config.
        /// </summary>
        public string ConnectionKey
        {
            get;
            internal set;
        }
        /// <summary>
        /// Get ItemType
        /// </summary>
        public Type ItemType
        {
            get {return typeof(T); }
        }

     
        #endregion
 
        #region Set Sync Source

        /// <summary>
        /// Set Item to SyncTables
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="ts"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public void SetSyncSource(string entityName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan ts, bool enableNoLock, int commandTimeout)
        {
          
           SyncSource = new DataSyncEntity(
               new Sync.SyncEntity()
               {
                   EntityName = entityName,
                   ViewName = mappingName,
                   SourceName = new string[] { mappingName },
                   PreserveChanges = false,
                   MissingSchemaAction = MissingSchemaAction.Add,
                   SyncType = syncType,
                   Interval = ts,
                   EnableNoLock = enableNoLock,
                   CommandTimeout = commandTimeout
               });

        }
        #endregion

        #region SetContext

        /// <summary>
        /// Set Db Context
        /// </summary>
        /// <param name="db"></param>
        public virtual void SetContext(EntityDbContext db)
        {
            lock (syncLock)
            {
                Context.EntityDb = db;
            }
            db.ValidateContext();
            ConnectionKey = db.ConnectionKey;//.Context.ConnectionName;

            object args = db.EntityName;
            IsReady = false;


            if (IsAsync)
            {
                TaskItem task = new TaskItem(Refresh, args, OnTaskCompleted);
                task.Timeout = TimeSpan.FromMinutes(3);
                AgentManager.Tasker.Add(task);

            }
            else
            {
                try
                {
                    using (Task task = new Task(() => Refresh(args)))
                    {
                        task.Start();
                        task.Wait();
                        if (task.IsCompleted)
                        {
                            IsReady = true;
                        }
                        else
                        {
                            IsTimeout = true;
                        }
                    }

                    string message = string.Format("SetContext TaskCompleted: {0}, state:{1}", db.EntityName, IsReady.ToString());

                    CacheLogger.Logger.LogAction(CacheAction.SyncTime, IsReady? CacheActionState.Ok: CacheActionState.Failed, message);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
         }

        /// <summary>
        /// Refresh
        /// </summary>
        /// <param name="args"></param>
        public abstract void Refresh(object args);
        
        /// <summary>
        /// On Error
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnError(Exception ex)
        {
            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncItem OnError: " + ex.Message);
        }

        /// <summary>
        ///  Set Db Context
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        public void SetContext(IDbContext db, string mappingName, EntityKeys keys)
        {
            EntityDbContext edb = new EntityDbContext(db, mappingName, keys);
            SetContext(edb);
        }

       /// <summary>
        ///  Set Db Context
       /// </summary>
       /// <param name="db"></param>
       /// <param name="entityName"></param>
       /// <param name="mappingName"></param>
       /// <param name="sourceType"></param>
       /// <param name="keys"></param>
        public void SetContext(IDbContext db, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys)
       {
           EntityDbContext edb = new EntityDbContext(db, entityName, mappingName, sourceType, keys);
           SetContext(edb);
       }
        /// <summary>
        ///  Set Db Context
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        public void SetContext(string connectionKey, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys)
        {
            EntityDbContext db = new EntityDbContext(entityName, mappingName, connectionKey, sourceType, keys);
            SetContext(db);
        }
        
  
        /// <summary>
        ///  Set Db Context
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        public void SetContext<Dbc>(string mappingName, EntityKeys keys) where Dbc : IDbContext
        {
            EntityDbContext db = EntityDbContext.Get<Dbc>(mappingName, keys);
            SetContext(db);
        }
        /// <summary>
        ///  Set Db Context
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        public void SetContext<Dbc>(string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys) where Dbc : IDbContext
        {
            EntityDbContext db = EntityDbContext.Get<Dbc>(entityName, mappingName, sourceType, keys);
            SetContext(db);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Refresh Async
        /// </summary>
        /// <param name="state"></param>
        public virtual void RefreshAsync(object state)
        {
            if (CacheSettings.EnableAsyncTask)
            {
                TaskItem task = new TaskItem(Refresh, state, OnTaskCompleted);
                task.Timeout = TimeSpan.FromMinutes(3);
                AgentManager.Tasker.Add(task);
            }
            else
            {
                Task t = Task.Factory.StartNew(() => Refresh(state));
            }
        }
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected void OnTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            if (e.Args.State == TaskState.Completed)
            {
                IsReady = true;
            }
            else
            {
                IsTimeout = true;
            }

        }

        /// <summary>
        /// Validate CacheKeyInfo
        /// </summary>
        public void Validate()
        {
            if (Info == null)
            {
                throw new Exception("Invalid CacheKeyInfo");
            }
            if (Info.ItemKeys==null || Info.ItemKeys.Length==0 || string.IsNullOrEmpty(Info.ItemName))
            {
                throw new Exception("CacheKeyInfo is not valid");
            }
        }

        /// <summary>
        /// Get if contains item by key in Dictionary
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Contains(CacheKeyInfo info)
        {
            if (info == null || info.IsEmpty)
                return false;

            return Contains(info.CacheKey);
        }

        /// <summary>
        /// Get if contains item by key in Dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal abstract bool Contains(string key);
        
        #endregion
    }
}
