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
using System.Collections;
using System.Diagnostics;
using Nistec.Data;
using Nistec.Data.Entities.Cache;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.Caching.Remote;
using Nistec.Caching.Data;
using Nistec.Caching.Server;
using Nistec.Threading;
using Nistec.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Nistec.Channels;
using Nistec.Caching.Sync;
using Nistec.Caching.Config;

namespace Nistec.Caching
{
    /// <summary>
    /// Implements SyncLoaderStream{T}
    /// </summary>
    public interface ISyncLoaderStream:IDisposable
    {
        //ComplexKey Info { get; }

        /// <summary>
        /// Get <see cref="ComplexKey"/>
        /// </summary>
        string[] FieldsKey { get; }
        /// <summary>
        /// Get <see cref="ComplexKey"/>
        /// </summary>
        string EntityName { get; }
        Dictionary<string, EntityStream> Items { get; }
        int Count { get; }
        long Size { get; }
        int Duration { get; }
    }

    /// <summary>
    /// Represent the sychronize loader.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SyncLoaderStream<T> : ISyncLoaderStream
    {
        #region members
        internal static object syncLock = new object();
        internal static object taskLock = new object();

        internal const int TaskerTimeoutSeconds = 600;
        /// <summary>
        /// Get indicate if is ready to use.
        /// </summary>
        public bool IsReady { get; internal set; }
        //internal bool IsAsync { get; private set; }
        internal bool IsTimeout { get; set; }
        public bool EnableAsyncTask { get; internal set; }

        bool IsOwenResources=false;
        bool EnableParallel = false;
        //internal LoaderEntityType LoaderEntityType { get; set; }
        //SyncLoaderBagStream Owner;

        #endregion

        #region ctor

        /// <summary>
        /// Create new <see cref="SyncLoaderStream{T}"/>
        /// </summary>
        SyncLoaderStream(bool enableAsyncTask, bool isOwenResources)
        {
            IsReady = false;
            //IsAsync = isAsync;
            IsTimeout = false;
            EnableAsyncTask = enableAsyncTask;
            IsOwenResources = isOwenResources;
        }

        /// <summary>
        /// Create new <see cref="SyncLoaderStream{T}"/>
        /// </summary>
        /// <param name="syncSource"></param>
        /// <param name="entityName"></param>
        /// <param name="fileldsKey"></param>
        /// <param name="context"></param>
        /// <param name="enableAsyncTask"></param>
        public SyncLoaderStream(DataSyncEntity syncSource, string entityName,string[] fileldsKey, EntityContext<T> context, bool enableAsyncTask)
            : this(enableAsyncTask,false)
        {

            //Info = info;

            EntityName = entityName;
            FieldsKey = fileldsKey;


            SyncSource = syncSource;

            Context = context;
        }

        /// <summary>
        /// Create new <see cref="SyncLoaderStream{T}"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="enableAsyncTask"></param>
        public SyncLoaderStream(SyncEntity entity, bool enableAsyncTask)
            : this(enableAsyncTask,true)
        {

            //Info = ComplexArgs.Get(entity.EntityName, entity.EntityKeys);
            EntityName = entity.EntityName;
            FieldsKey = entity.EntityKeys;

            SyncSource = new DataSyncEntity(entity);

            SetContext(entity.ConnectionKey, entity.EntityName, entity.ViewName, entity.SourceType, EntityKeys.Get(entity.EntityKeys), entity.Columns);
        }

        /// <summary>
        /// Create new <see cref="SyncLoaderStream{T}"/>
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="enableAsyncTask"></param>
        public SyncLoaderStream(string connectionKey, string entityName, string mappingName, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout, bool enableAsyncTask)
            : this(enableAsyncTask, true)
        {
            Set(connectionKey, entityName, mappingName, new string[] { mappingName }, EntitySourceType.Table, keys, columns, timer, enableNoLock, commandTimeout);
        }

        /// <summary>
        /// Create new <see cref="SyncLoaderStream{T}"/>
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
        /// <param name="enableAsyncTask"></param>
        public SyncLoaderStream(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout, bool enableAsyncTask)
            : this(enableAsyncTask, true)
        {
            Set(connectionKey, entityName, mappingName, sourceName, sourceType, keys, columns, timer, enableNoLock, commandTimeout);
        }

        //internal SyncLoaderStream(IKeyValue dic, string columns, bool enableAsyncTask)
        //    : this(enableAsyncTask, false)
        //{
        //    Set(dic.Get<string>(KnowsArgs.ConnectionKey),
        //        dic.Get<string>(KnowsArgs.TableName),
        //        dic.Get<string>(KnowsArgs.MappingName),
        //        CacheMessage.SplitArg(dic, KnowsArgs.SourceName, null),
        //        (EntitySourceType)dic.Get<int>(KnowsArgs.SourceType),
        //        CacheMessage.SplitArg(dic, KnowsArgs.EntityKeys, null), columns,

        //        new SyncTimer(CacheMessage.TimeArg(dic, KnowsArgs.SyncTime, null), (SyncType)dic.Get<int>(KnowsArgs.SyncType)),
        //        false, 0);
        //}


        #endregion

        #region set

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
                    CommandTimeout = commandTimeout,
                    Columns = columns
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
        /// <param name="columns"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public void Set(string connectionKey, string entityName, string mappingName, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout)
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
        /// <param name="columns"></param>
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
                    Columns = columns
                }, timer);

            SetContext(connectionKey, entityName, mappingName, sourceType, EntityKeys.Get(keys));
        }


        #endregion

        #region IDisposable

        /// <summary>
        /// Destructor.
        /// </summary>
        ~SyncLoaderStream()
        {
            Dispose(false);
        }

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
                if (this._Items != null)
                {
                    this._Items = null;
                }
                if (IsOwenResources)
                {
                    if (this.Context != null)
                    {
                        this.Context.Dispose();
                        this.Context = null;
                    }
                    if (this.SyncSource != null)
                    {
                        this.SyncSource.Dispose();
                        this.SyncSource = null;
                    }
                }
            }
            if (IsOwenResources)
            {
                //this.ConnectionKey = null;
                this.Filter = null;
                //this.Info = null;
                this.EntityName = null;
                this.FieldsKey = null;
            }
        }

        #endregion

        #region Set Sync Source

        /// <summary>
        /// Set Item to SyncTables
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="columns"></param>
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
                    Columns = columns
                });

        }
        #endregion

        #region SetContext



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
            var edb = new EntityDbContext(db, mappingName, keys);
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
        /// <param name="columns"></param>
        public void SetContext(IDbContext db, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys, string columns = "*")
        {
            var edb = new EntityDbContext(db, entityName, mappingName, sourceType, keys, columns);
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
        /// <param name="columns"></param>
        public void SetContext(string connectionKey, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys, string columns = "*")
        {
            var db = new EntityDbContext(entityName, mappingName, connectionKey, sourceType, keys, columns);
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
            var db = EntityDbContext.Get<Dbc>(mappingName, keys);
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
            var db = EntityDbContext.Get<Dbc>(entityName, mappingName, sourceType, keys);
            SetContext(db);
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

        /// <summary>
        /// Get or Set <see cref="DataFilter"/>
        /// </summary>
        public DataFilter Filter
        {
            get;
            set;
        }


        ///// <summary>
        ///// Get db connection key from config.
        ///// </summary>
        //public string ConnectionKey
        //{
        //    get;
        //    internal set;
        //}
        /// <summary>
        /// Get ItemType
        /// </summary>
        public Type ItemType
        {
            get { return typeof(EntityStream); }
        }


        Dictionary<string, EntityStream> _Items;

        /// <summary>
        /// Get Entities Storage
        /// </summary>
        public Dictionary<string, EntityStream> Items
        {
            get
            {
                if (_Items == null)
                {

                    //_Items = DictionaryExtension.CreateConcurrentDictionary<string, EntityStream>(10);

                    _Items = new Dictionary<string, EntityStream>();
                }
                return _Items;
            }
        }

        /// <summary>
        /// Get entities keys
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetEntityKeys()
        {
            return Items.Keys;
        }

        /// <summary>
        /// Get entity items Report
        /// </summary>
        /// <returns></returns>
        public DataTable GetItemsReport()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("TypeName");
            dt.Columns.Add("Label");
            dt.Columns.Add("Modified");
            dt.Columns.Add("Expiration");

            EntityStream[] items = GetEntityValues();
            foreach (var item in items)
            {
                dt.Rows.Add(item.Id, item.TypeName, item.Label, item.Modified, item.Expiration);
            }
            return dt;

        }

        /// <summary>
        /// Get entities copy.
        /// </summary>
        /// <param name="copyBody"></param>
        /// <returns></returns>
        public GenericKeyValue GetEntityItems(bool copyBody)
        {
            GenericKeyValue kv = new GenericKeyValue();
            EntityStream[] items = GetEntityValues();
            foreach (var item in items)
            {
                if (copyBody)
                    kv.Add(item.Id, item.Copy());
                else
                    kv.Add(item.Id, item.Clone());
            }
            return kv;
        }

        /// <summary>
        /// Get entities items count.
        /// </summary>
        /// <returns></returns>
        public int GetEntityItemsCount()
        {
            return Items.Count;
        }


        /// <summary>
        /// Get entity values.
        /// </summary>
        /// <returns></returns>
        public EntityStream[] GetEntityValues()
        {

            return Items.Values.ToArray();

        }

        /// <summary>
        /// Get entities size in bytes.
        /// </summary>
        /// <returns></returns>
        public long GetEntityItemsSize()
        {
            long size = 0;
            EntityStream[] items = GetEntityValues();
            foreach (var item in items)
            {
                size += item.Size;
            }
            return size;
        }



        /// <summary>
        /// Get Items count
        /// </summary>
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }
        /// <summary>
        /// Get EntityType
        /// </summary>
        public Type EntityType
        {
            get { return typeof(T); }
        }

        long _Size;
        /// <summary>
        /// Get the cuurent size in bytws.
        /// </summary>
        public long Size
        {
            get
            {
                return _Size;
            }
        }

        /// <summary>
        /// Get <see cref="EntityContext"/>
        /// </summary>
        public EntityContext<T> Context
        {
            get;private set;
        }

        public int Duration
        {
            get; private set;
            
        }

        #endregion

        #region Get item

        /// <summary>
        /// Get copy of item as <see cref="EntityStream"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public EntityStream GetEntityStream(ComplexKey info)
        {
            if (info == null || info.IsEmpty)
                return null;

            var es = GetEntityStreamInternal(info.Suffix);
            if (es == null)
                return null;
            return es.Copy();
        }

        /// <summary>
        /// Get item as <see cref="EntityStream"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal EntityStream GetEntityStreamInternal(string key)
        {
            if (!IsReady)
            {
                CacheLogger.Info("SyncItemStream is not ready");
                return null;// throw new Exception("SyncItemStream is not ready");
            }
            EntityStream entity = null;

            if (Items.TryGetValue(key, out entity))
            {

                return entity;
            }

            return null;
        }
        /// <summary>
        /// Get copy of an item as <see cref="NetStream"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream GetItemStream(ComplexKey info)
        {
            if (info == null)
                return null;
            return GetItemStream(info.Suffix);
        }

        /// <summary>
        /// Get copy of an item as <see cref="NetStream"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal NetStream GetItemStream(string key)
        {
            EntityStream es = GetEntityStreamInternal(key);
            if (es == null)
            {
                return null;
            }
            return es.GetCopy();
        }
        /// <summary>
        /// Get item as byte array
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public byte[] GetItemBinary(ComplexKey info)
        {
            if (info == null || info.IsEmpty)
                return null;
            return GetItemBinary(info.Suffix);
        }

        internal byte[] GetItemBinary(string key)
        {
            EntityStream es = GetEntityStreamInternal(key);
            if (es == null)
            {
                return null;
            }
            return es.GetBinary();//.BodyStream == null ? null : es.BodyStream.ToArray();
        }

        #endregion

        #region Loader Methods

        void SetContext(EntityDbContext db)
        {
            db.EnableConnectionProvider = CacheSettings.EnableConnectionProvider;
            Context = new EntityContext<T>();
            Context.EntityDb = db;
            db.ValidateContext();
            //ConnectionKey = db.ConnectionKey;//.Context.ConnectionName;
        }
        
        Action<ISyncLoaderStream> OnCompleted;


        /// <summary>
        /// Start Sync loader
        /// </summary>
        public void Start(Action<ISyncLoaderStream> onCompleted)
        {
            try
            {
                if(EntityName==null || FieldsKey==null)
                {
                    throw new ArgumentException("Invalid ComplexKey info in SyncLoaderStream");
                }
                CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Info, "SyncLoaderStream Started, Item: {0}", EntityName);

                OnCompleted = onCompleted;

                object args = EntityName;// Info.Prefix;
                IsReady = false;

                if (EnableAsyncTask)//(IsAsync)
                {
                   //~Console.WriteLine("Debuger-SyncItemBase.SetContext EnableAsyncTask");
                    TaskItem task = new TaskItem(Load, args, OnTaskCompleted);
                    task.Timeout = TimeSpan.FromMinutes(3);
                    AgentManager.Tasker.Add(task);

                }
                else
                {
                   //~Console.WriteLine("Debuger-SyncItemBase.SetContext task");

                    Task task = new Task(() => Load(args));
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

                    string message = string.Format("SetContext TaskCompleted: {0}, state:{1}", EntityName, IsReady.ToString());

                    CacheLogger.Logger.LogAction(CacheAction.SyncTime, IsReady ? CacheActionState.Ok : CacheActionState.Failed, message);

                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        ///// <summary>
        ///// Refresh Async
        ///// </summary>
        ///// <param name="state"></param>
        //public virtual void LoadAsync(object state)
        //{
        //    if (EnableAsyncTask)
        //    {
        //        TaskItem task = new TaskItem(Load, state, OnTaskCompleted);
        //        task.Timeout = TimeSpan.FromMinutes(3);
        //        AgentManager.Tasker.Add(task);
        //    }
        //    else
        //    {
        //        Task t = Task.Factory.StartNew(() => Load(state));
        //    }
        //}

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

            if (OnCompleted != null)
                OnCompleted(this);
        }

        /// <summary>
        /// Validate ComplexKey
        /// </summary>
        public void Validate()
        {
            if (EntityName == null || FieldsKey==null)
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
        /// Refresh all items in Dictionary
        /// </summary>
        void Load(object args)
        {
           //~Console.WriteLine("Debuger-SyncItemStream.Refresh...");

            var watch = Stopwatch.StartNew();
            long totalSize = 0;
            int currentCount = Count;

            string entityName = null;
            
            int syncCount = 0;

            try
            {
                if (SyncSource == null)
                {
                    throw new Exception("Invalid DataSyncEntity in SyncLoaderStream, SyncSource is null.");
                }

                entityName = SyncSource.EntityName;
                string[] fieldsKey = FieldsKey;

                if (EnableParallel)
                {
                    Dictionary<string, EntityStream> items = null;

                    lock (taskLock)
                    {
                        items = EntityStream.CreateEntityRecordStream<T>(Context, Filter, fieldsKey, out totalSize);//-GenericRecord
                        if (items == null || items.Count == 0)
                        {
                            throw new Exception("CreateEntityStream is null at SyncLoaderStream, Refreshed Sync Item failed.");
                        }
                        syncCount = items.Count;
                    }

                    //Synchronize(items,true);
                    DictionaryUtil util = new DictionaryUtil();
                    util.SynchronizeParallel<string, EntityStream>(_Items, items, true, LogActionSync);
                }
                else
                {

                    lock (taskLock)
                    {
                        _Items = EntityStream.CreateEntityRecordStream<T>(Context, Filter, fieldsKey, out totalSize);//-GenericRecord

                        if (_Items == null || _Items.Count == 0)
                        {
                            throw new Exception("CreateEntityStream is null at SyncLoaderStream, Refreshed Sync Item failed.");
                        }
                        syncCount = _Items.Count;
                    }
                }
                //AgentManager.SyncCache.SizeExchage(_Size, totalSize, currentCount, Count, false);//true);

                _Size = totalSize;
                //FieldsKey = fieldsKey;

                watch.Stop();
                Duration = (int)watch.ElapsedMilliseconds;

                //CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Ok, "SyncLoaderStream Load Item completed : {0}, CurrentCount: {1}, SyncCount: {2}, Duration Milliseconds: {3}", entityName, currentCount.ToString(), syncCount.ToString(), watch.ElapsedMilliseconds.ToString());

                if (!EnableAsyncTask)
                {
                    if (OnCompleted != null)
                        OnCompleted(this);
                }

            }
            catch (Exception ex)
            {
                watch.Stop();
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncLoaderStream Load Item error : {0}, Message: {1} ", entityName, ex.Message);
            }
        }


        #endregion

        #region Methods

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
        internal bool Contains(string key)
        {
            return Items.ContainsKey(key);
        }

        public void LogActionSync(string text)
        {
            CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.None, text);
        }

        #endregion

    }
}
