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
using Nistec.Channels;

namespace Nistec.Caching.Sync
{
   
   /// <summary>
    /// Represent the sychronize item info for each item in sync cache
   /// </summary>
   /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class SyncTableBase<T> : ISyncCacheItem,IDisposable
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

        internal ISyncBag Owner;

        public string[] GetCleanKeys()
        {
            if (FieldsKey == null)
                return null;
            return KeySet.CleanKeys(FieldsKey);
        }

        public string GetPrimaryKey(string[] keyValueArgs)
        {

            //NameValueArgs

            if (keyValueArgs == null)
            {
                throw new ArgumentNullException("keyValueArgs");
            }


            string[] fieldsKey = GetCleanKeys();
            int length = fieldsKey.Length;


            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            if((length*2) < count)
            {
                throw new ArgumentOutOfRangeException("values parameter Not match to fieldsKey range");
            }

            object[] values = new object[length];

            for (int i = 0; i < count; i++)
            {
                string key = keyValueArgs[i];
                int index = fieldsKey.IndexOf(key);
                ++i;
                if (index >= 0)
                {
                    values[index] = keyValueArgs[i];
                }
            }

            return KeySet.FormatPrimaryKey(values);
        }

        public string GetPrimaryKey(NameValueArgs keyValueArgs)
        {

           if (keyValueArgs == null)
            {
                throw new ArgumentNullException("keyValueArgs");
            }


            string[] fieldsKey = GetCleanKeys();
            int length = fieldsKey.Length;


            int count = keyValueArgs.Count;
            if (length < count)
            {
                throw new ArgumentOutOfRangeException("values parameter Not match to fieldsKey range");
            }

            object[] values = new object[length];
            foreach(var entry in keyValueArgs)
            {
                int index = fieldsKey.IndexOf(entry.Key);
                if (index >= 0)
                {
                    values[index] = entry.Value;
                }
            }

            return KeySet.FormatPrimaryKey(values);
        }

        public string GetPrimaryKey(string queryString)
        {

            if (queryString == null)
            {
                throw new ArgumentNullException("queryString");
            }

            NameValueArgs nv = NameValueArgs.ParseQueryString(queryString);

            return GetPrimaryKey(nv);
        }

        //public SyncCacheBase Owner { get; internal set; }

        #region ctor

        /// <summary>
        /// Create new <see cref="SyncTableBase{T}"/>
        /// </summary>
        /// <param name="isAsync"></param>
        public SyncTableBase(bool isAsync) 
        {
            if (typeof(T).IsInterface)
            {
                throw new InvalidCastException("typeof(T) should not be an interface");
            }
            IsReady = false;
            IsAsync = isAsync;
            IsTimeout = false;
            Modified = DateTime.Now;
            //Owner = owner;
        }

        /// <summary>
        /// Create new <see cref="SyncTableBase{T}"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isAsync"></param>
        public SyncTableBase(SyncEntity entity, bool isAsync)
            : this(isAsync)
        {

            //Info = new ComplexKey() { ItemKeys = entity.EntityKeys, ItemName = entity.EntityName };
            //Info = ComplexArgs.Get(entity.EntityName,entity.EntityKeys);

            EntityName = entity.EntityName;
            FieldsKey = entity.EntityKeys;

            SyncSource = new DataSyncEntity(entity);//entity.EntityName, entity.ViewName, entity.SourceName, new SyncTimer(entity.Interval, entity.SyncType));

            SetContext(entity.ConnectionKey, entity.EntityName, entity.ViewName, entity.SourceType, EntityKeys.Get(entity.EntityKeys), entity.Columns);
        }

        void ISyncCacheItem.Set(SyncEntity entity, bool isAsync)
        {
            //Owner = owner;SyncCacheBase owner
            IsReady = false;
            IsAsync = isAsync;
            IsTimeout = false;
            Modified = DateTime.Now;
            //Info = new ComplexKey() { ItemKeys = entity.EntityKeys, ItemName = entity.EntityName };
            //Info = ComplexArgs.Get(entity.EntityName, entity.EntityKeys);
            EntityName = entity.EntityName;
            FieldsKey = entity.EntityKeys;

            SyncSource = new DataSyncEntity(entity);//entity.EntityName, entity.ViewName, entity.SourceName, new SyncTimer(entity.Interval, entity.SyncType));

            SetContext(entity.ConnectionKey, entity.EntityName, entity.ViewName, entity.SourceType, EntityKeys.Get(entity.EntityKeys),entity.Columns);
        }

        /// <summary>
        /// Create new <see cref="SyncTableBase{T}"/>
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="isAsync"></param>
        public SyncTableBase(string connectionKey, string entityName, string mappingName, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : this(isAsync)
        {
            Set(connectionKey, entityName, mappingName, new string[] { mappingName }, EntitySourceType.Table, keys,columns, timer, enableNoLock, commandTimeout);
        }

        /// <summary>
        /// Create new <see cref="SyncTableBase{T}"/>
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="isAsync"></param>
        public SyncTableBase(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : this(isAsync)
        {
            Set(connectionKey, entityName, mappingName, sourceName, sourceType, keys,columns, timer,enableNoLock, commandTimeout);
        }
        /// <summary>
        /// Set current item.
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public void Set<Dbc>(string entityName, string mappingName, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout) where Dbc : IDbContext
        {
            //Info = new ComplexKey() { ItemKeys = keys, ItemName = entityName };//mappingName
            //Info = ComplexArgs.Get(entityName, keys);
            EntityName = entityName;
            FieldsKey = keys;

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
                    CommandTimeout=commandTimeout,
                    Columns=columns
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
        public void Set(string connectionKey, string entityName, string mappingName, string[] keys, string columns, SyncTimer timer, bool enableNoLock,int commandTimeout)
        {
            Set(connectionKey, entityName, mappingName, new string[] { mappingName }, EntitySourceType.Table, keys, columns, timer, enableNoLock, commandTimeout);
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
        public void Set(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout)
        {
            //Info = new ComplexKey() { ItemKeys = keys, ItemName = entityName };//mappingName
            //Info = ComplexArgs.Get(entityName, keys);
            EntityName = entityName;
            FieldsKey = keys;


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
                    CommandTimeout = commandTimeout,
                    Columns=columns
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
            //this.Info = null;
            this.EntityName = null;
            this.FieldsKey = null;
        }

        #endregion

        #region Properties

        ///// <summary>
        ///// Get or Set ComplexKey
        ///// </summary>
        //public ComplexKey Info
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Get or Set FieldsKey
        /// </summary>
        public string[] FieldsKey
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set EntityName\TableName
        /// </summary>
        public string EntityName
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

        public DateTime Modified
        {
            get;
            set;
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
        public void SetSyncSource(string entityName, string mappingName, string[] sourceName, string columns, SyncType syncType, TimeSpan ts, bool enableNoLock, int commandTimeout)
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
                   CommandTimeout = commandTimeout,
                   Columns= columns
               });

        }
        #endregion

        #region SetContext

        /// <summary>
        /// Set Db Context and load entity
        /// </summary>
        /// <param name="db"></param>
        public virtual void SetContextAndLoadEntity(EntityDbContext db)
        {
            if (CacheSettings.EnableConnectionProvider)
                db.EnableConnectionProvider = true;

            lock (syncLock)
            {
                Context.EntityDb = db;
            }
            db.ValidateContext();
            ConnectionKey = db.ConnectionKey;//.Context.ConnectionName;

            object args = db.EntityName;
            IsReady = false;


            if (CacheSettings.EnableAsyncTask)//(IsAsync)
            {
                //~Console.WriteLine("Debuger-SyncTableBase.SetContext EnableAsyncTask");
                TaskItem task = new TaskItem(Refresh, args, OnTaskCompleted);
                task.Timeout = TimeSpan.FromMinutes(3);
                AgentManager.Tasker.Add(task);

            }
            else
            {
                //~Console.WriteLine("Debuger-SyncTableBase.SetContext task");
                try
                {
                    Task task = new Task(() => Refresh(args));
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
                    task.TryDispose();

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
            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncTable OnError: " + ex.Message);
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
            SetContextAndLoadEntity(edb);
        }

        /// <summary>
        ///  Set Db Context
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        public void SetContext(IDbContext db, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys, string columns="*")
       {
           EntityDbContext edb = new EntityDbContext(db, entityName, mappingName, sourceType, keys, columns);
            SetContextAndLoadEntity(edb);
       }
        /// <summary>
        ///  Set Db Context
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        public void SetContext(string connectionKey, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys, string columns = "*")
        {
            EntityDbContext db = new EntityDbContext(entityName, mappingName, connectionKey, sourceType, keys, columns);
            SetContextAndLoadEntity(db);
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
            SetContextAndLoadEntity(db);
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
            SetContextAndLoadEntity(db);
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
        /// Validate ComplexKey
        /// </summary>
        public void Validate()
        {
            if (EntityName == null ||FieldsKey==null)
            {
                throw new Exception("Invalid ComplexKey");
            }
            //if (Info.ItemKeys==null || Info.ItemKeys.Length==0 || string.IsNullOrEmpty(Info.ItemName))
            //{
            //    throw new Exception("ComplexKey is not valid");
            //}
            if (FieldsKey == null || FieldsKey.Length == 0 || string.IsNullOrEmpty(EntityName))
            {
                throw new Exception("ComplexKey is not valid");
            }
        }

        /// <summary>
        /// Get if contains item by key in Dictionary
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Contains(ComplexKey info)
        {
            if (info == null || info.IsEmpty)
                return false;

            return Contains(info.Suffix);
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
