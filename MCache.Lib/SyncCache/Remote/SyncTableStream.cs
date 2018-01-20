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
using Nistec.Caching.Config;

namespace Nistec.Caching.Sync.Remote
{

    /// <summary>
    /// Represent the sychronize item info for each item in sync cache stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SyncTableStream<T> : SyncTableBase<T>, ISyncTableStream
    {

        #region ctor

        /// <summary>
        /// Create new <see cref="SyncTableStream{T}"/>
        /// </summary>
        public SyncTableStream(bool isAsync)
            : base(isAsync)
        {

        }


        /// <summary>
        /// Create new <see cref="SyncTableStream{T}"/>
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="isAsync"></param>
        public SyncTableStream(string connectionKey, string entityName, string mappingName, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : base(isAsync)
        {
            Set(connectionKey, entityName, mappingName, new string[] { mappingName }, EntitySourceType.Table, keys, columns, timer, enableNoLock, commandTimeout);
        }

        /// <summary>
        /// Create new <see cref="SyncTableStream{T}"/>
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
        public SyncTableStream(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : base(isAsync)
        {
            Set(connectionKey, entityName, mappingName, sourceName, sourceType, keys, columns, timer, enableNoLock, commandTimeout);
        }

        internal SyncTableStream(NameValueArgs dic, bool isAsync)
            : base(isAsync)
        {
            Set(dic.Get(KnowsArgs.ConnectionKey),
                dic.Get(KnowsArgs.TableName),
                dic.Get(KnowsArgs.MappingName),
                dic.SplitArg(KnowsArgs.SourceName, null),
                (EntitySourceType)dic.Get<int>(KnowsArgs.SourceType),
                dic.SplitArg(KnowsArgs.EntityKeys, null), "*",

                new SyncTimer(dic.TimeArg(KnowsArgs.SyncTime, null), (SyncType)dic.Get<int>(KnowsArgs.SyncType)),
                false, 0);
        }

        /// <summary>
        /// Create new <see cref="SyncTableStream{T}"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isAsync"></param>
        public SyncTableStream(SyncEntity entity, bool isAsync)
            : base(entity, isAsync)
        {

        }

        #endregion

        #region IDisposable
        /// <summary>
        /// Destructor.
        /// </summary>
        ~SyncTableStream()
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
                if (this._Items != null)
                {
                    this._Items = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Properties

        ConcurrentDictionary<string, EntityStream> _Items;

        /// <summary>
        /// Get Entities Storage
        /// </summary>
        public ConcurrentDictionary<string, EntityStream> Items
        {
            get
            {
                if (_Items == null)
                {

                    _Items = DictionaryUtil.CreateConcurrentDictionary<string, EntityStream>(10);

                    //_Items = new ConcurrentDictionary<string, EntityStream>();
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

        /*
        /// <summary>
        /// Get Values
        /// </summary>
        public ICollection Values
        {
            get
            {
                return (ICollection)Items.Values;
            }
        }

        /// <summary>
        /// Get Keys
        /// </summary>
        public ICollection Keys
        {
            get
            {
                return (ICollection)Items.Keys;
            }
        }
        */

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

            EntityStream es;
            if (TryGetEntity(info.Suffix, out es))
            {
                return es.Copy();
            }
            return null;


            //var es = GetEntityStreamInternal(info.Suffix);
            //if (es == null)
            //    return null;
            //return es.Copy();
        }

        /// <summary>
        /// Get copy of item as <see cref="EntityStream"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public EntityStream GetEntityStream(string key)
        {

            EntityStream es;
            if (TryGetEntity(key, out es))
            {
                return es.Copy();
            }
            return null;

            //if (key == null || key=="")
            //    return null;

            //var es = GetEntityStreamInternal(key);
            //if (es == null)
            //    return null;
            //return es.Copy();
        }

        ///// <summary>
        ///// Get item as <see cref="EntityStream"/>
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //internal EntityStream GetEntityStreamInternal(string key)
        //{
        //    if (!IsReady)
        //    {
        //        CacheLogger.Info("SyncTableStream is not ready");
        //        return null;// throw new Exception("SyncTableStream is not ready");
        //    }
        //    EntityStream entity = null;

        //    if (Items.TryGetValue(key, out entity))
        //    {

        //        return entity;
        //    }

        //    return null;
        //}

        public bool TryGetEntity(string key, out EntityStream item)
        {
            if (!IsReady)
            {
                CacheLogger.Info("SyncTableStream is not ready");
                item = null;
                return false;// throw new Exception("SyncTableStream is not ready");
            }
            if (key == null)
            {
                item = null;
                return false;
            }
            item = null;

            if (Items.TryGetValue(key, out item))
            {

                return true;
            }
            return false;
        }

        ///// <summary>
        ///// Get copy of an item as <see cref="NetStream"/>.
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public NetStream GetItemStream(ComplexKey info)
        //{
        //    if (info == null)
        //        return null;
        //    return GetItemStream(info.Query);
        //}

        /// <summary>
        /// Try Get copy of an item as <see cref="NetStream"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool TryGetItemStream(string key, out NetStream item)
        {
            EntityStream es;
            if (TryGetEntity(key, out es))
            {
                item = es.GetCopy();
                return true;
            }
            item = null;
            return false;
        }

        /// <summary>
        /// Get copy of an item as <see cref="NetStream"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public NetStream GetItemStream(string key)
        {
            EntityStream es;
            if (TryGetEntity(key,out es))
            {
                return es.GetCopy();
            }
            return null;

            //if (key == null)
            //    return null;
            //EntityStream es = GetEntityStreamInternal(key);
            //if (es == null)
            //{
            //    return null;
            //}
            //return es.GetCopy();
        }

        ///// <summary>
        ///// Get item as byte array
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public byte[] GetItemBinary(ComplexKey info)
        //{
        //    if (info == null || info.IsEmpty)
        //        return null;
        //    return GetItemBinary(info.Query);
        //}

        public byte[] GetItemBinary(string key)
        {
            EntityStream es;
            if (TryGetEntity(key, out es))
            {
                return es.GetBinary();
            }
            return null;

            //if (key == null)
            //    return null;
            //EntityStream es = GetEntityStreamInternal(key);
            //if (es == null)
            //{
            //    return null;
            //}
            //return es.GetBinary();//.BodyStream == null ? null : es.BodyStream.ToArray();
        }

        #endregion

        #region SetContext

        void OnLoadCompleted(ISyncLoaderStream loader)
        {
            string entityName = null;
            try
            {
                entityName = loader.EntityName;
                if (SyncSource == null)
                {
                    throw new Exception("Invalid DataSyncEntity in SyncTableStream, SyncSource is null.");
                }

                if(loader.Items==null || loader.Items.Count==0)
                {
                    throw new Exception("SyncLoader could not load items, SyncLoader items is null.");
                }
                int currentCount = Count;
                
                lock (syncLock)
                {
                    var items = new ConcurrentDictionary<string, EntityStream>(loader.Items.ToArray());
                    _Items = items;
                }

                AgentManager.SyncCache.SizeExchage(_Size, loader.Size, currentCount, loader.Count, false);//true);
               
                _Size = loader.Size;
                Modified = DateTime.Now;
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Ok, "SyncTableStream Loaded Item completed : {0}, CurrentCount: {1}, SyncCount: {2}, Duration Milliseconds: {3}", entityName, currentCount.ToString(), loader.Count.ToString(), loader.Duration.ToString());

            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "OnLoadCompleted Sync Item error : {0}, Message: {1} ", entityName, ex.Message);
            }
            finally
            {
                if (loader != null)
                {
                    loader.Dispose();
                }
            }
        }


        public override void Refresh(object args)
        {
            SyncLoaderStream<T> loader = new SyncLoaderStream<T>(SyncSource, EntityName,FieldsKey, Context, CacheSettings.EnableAsyncTask);
            loader.Start(OnLoadCompleted);
        }

#if (false)
        public override void Refresh(object args)
        {
            //~Console.WriteLine("Debuger-SyncTableStream.Refresh...");

            if (CacheSettings.EnableAsyncLoader)
            {
                SyncLoaderStream<T> loader = new SyncLoaderStream<T>(SyncSource, Info, Context, CacheSettings.EnableAsyncTask);
                loader.Start(OnLoadCompleted);
            }
            else
            {
                var watch = Stopwatch.StartNew();
                long totalSize = 0;
                int currentCount = Count;

                string entityName = null;
                int syncCount = 0;

                try
                {
                    if (SyncSource == null)
                    {
                        throw new Exception("Invalid DataSyncEntity in SyncTableStream, SyncSource is null.");
                    }

                    entityName = SyncSource.EntityName;
                    ConcurrentDictionary<string, EntityStream> items = null;
                    lock (taskLock)
                    {
                        items = EntityStream.CreateConcurrentEntityRecordStream(Context, Filter, out totalSize);//-GenericRecord
                        if (items == null || items.Count == 0)
                        {
                            throw new Exception("CreateEntityStream is null, Refreshed Sync Item failed.");
                        }
                        syncCount = items.Count;
                    }

#if (Synchronize)

                         //Synchronize(items,true);
                        DictionaryUtil util = new DictionaryUtil();
                        util.SynchronizeParallel<string, EntityStream>(_Items,items,true,LogActionSync);
#else

                    lock (syncLock)
                    {
                        _Items = items;
                    }
                    //items = null;
#endif
                    AgentManager.SyncCache.SizeExchage(_Size, totalSize, currentCount, Count, false);//true);

                    _Size = totalSize;

                    watch.Stop();

                    CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Ok, "Refresh Sync Item completed : {0}, CurrentCount: {1}, SyncCount: {2}, Duration Milliseconds: {3}", entityName, currentCount.ToString(), syncCount.ToString(), watch.ElapsedMilliseconds.ToString());

                }
                catch (Exception ex)
                {
                    watch.Stop();
                    CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "Refresh Sync Item error : {0}, Message: {1} ", entityName, ex.Message);
                }
            }
        }

        /*
                /// <summary>
                /// Refresh all items in Dictionary
                /// </summary>
                public override void Refresh(object state)
                {
                    //~Console.WriteLine("Debuger-SyncTableStream.Refresh...");

                    var watch = Stopwatch.StartNew();
                    long totalSize = 0;
                    int currentCount = Count;

                    string entityName = null;
                    int syncCount = 0;

                    try
                    {
                        if (SyncSource == null)
                        {
                            throw new Exception("Invalid DataSyncEntity in SyncTableStream, SyncSource is null.");
                        }

                        entityName = SyncSource.EntityName;
                        ConcurrentDictionary<string, EntityStream> items = null;
                        lock (taskLock)
                        {
                            items = EntityStream.CreateConcurrentEntityRecordStream(Context, Filter, out totalSize);//-GenericRecord
                            if (items == null || items.Count == 0)
                            {
                                throw new Exception("CreateEntityStream is null, Refreshed Sync Item failed.");
                            }
                            syncCount = items.Count;
                        }

#if (Synchronize)

                         //Synchronize(items,true);
                        DictionaryUtil util = new DictionaryUtil();
                        util.SynchronizeParallel<string, EntityStream>(_Items,items,true,LogActionSync);
#else

                        lock (syncLock)
                        {
                            _Items = items;
                        }
                        //items = null;
#endif
                        AgentManager.SyncCache.SizeExchage(_Size, totalSize,currentCount, Count, false);//true);

                        _Size = totalSize;

                        watch.Stop();

                        CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Ok, "Refresh Sync Item completed : {0}, CurrentCount: {1}, SyncCount: {2}, Duration Milliseconds: {3}", entityName, currentCount.ToString(), syncCount.ToString(), watch.ElapsedMilliseconds.ToString());

                    }
                    catch (Exception ex)
                    {
                        watch.Stop();
                        CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "Refresh Sync Item error : {0}, Message: {1} ", entityName, ex.Message);
                    }
                }
        */

#endif
        public void LogActionSync(string text)
        {
            CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.None, text);
        }

   
        /// <summary>
        /// Get if contains item by key in Dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal override bool Contains(string key)
        {
            
                return Items.ContainsKey(key);

        }
#endregion

    }
}
