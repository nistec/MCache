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

namespace Nistec.Caching.Sync.Remote
{

    /// <summary>
    /// Represent the sychronize item info for each item in sync cache stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SyncItemStream<T> : SyncItemBase<T>, ISyncItemStream 
    {
 
        #region ctor

        /// <summary>
        /// Create new <see cref="SyncItemStream{T}"/>
        /// </summary>
        public SyncItemStream(bool isAsync)
            : base(isAsync)
        {
            
        }


       /// <summary>
        /// Create new <see cref="SyncItemStream{T}"/>
       /// </summary>
       /// <param name="connectionKey"></param>
       /// <param name="entityName"></param>
       /// <param name="mappingName"></param>
       /// <param name="keys"></param>
       /// <param name="timer"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="isAsync"></param>
        public SyncItemStream(string connectionKey, string entityName, string mappingName, string[] keys, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : base(isAsync)
        {
            Set(connectionKey, entityName, mappingName, new string[] { mappingName }, EntitySourceType.Table, keys, timer, enableNoLock, commandTimeout);
        }

        /// <summary>
        /// Create new <see cref="SyncItemStream{T}"/>
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
        public SyncItemStream(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] keys, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : base(isAsync)
        {
            Set(connectionKey, entityName, mappingName, sourceName, sourceType, keys, timer, enableNoLock, commandTimeout);
        }

        internal SyncItemStream(IKeyValue dic, bool isAsync)
            : base(isAsync)
        {
            Set(dic.Get<string>(KnowsArgs.ConnectionKey),
                dic.Get<string>(KnowsArgs.TableName),
                dic.Get<string>(KnowsArgs.MappingName),
                CacheMessage.SplitArg(dic, KnowsArgs.SourceName, null),
                (EntitySourceType)dic.Get<int>(KnowsArgs.SourceType),
                CacheMessage.SplitArg(dic, KnowsArgs.EntityKeys, null),
                new SyncTimer(CacheMessage.TimeArg(dic, KnowsArgs.SyncTime, null), (SyncType)dic.Get<int>(KnowsArgs.SyncType)),
                false, 0);
        }

        /// <summary>
        /// Create new <see cref="SyncItemStream{T}"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isAsync"></param>
        public SyncItemStream(SyncEntity entity, bool isAsync)
            : base(entity,isAsync)
        {
 
        }

        #endregion

        #region IDisposable
        /// <summary>
        /// Destructor.
        /// </summary>
        ~SyncItemStream()
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
            dt.Columns.Add("Key");
            dt.Columns.Add("TypeName");
            dt.Columns.Add("Id");
            dt.Columns.Add("Modified");
            dt.Columns.Add("Expiration");

            EntityStream[] items = GetEntityValues();
            foreach (var item in items)
            {
                dt.Rows.Add(item.Key,item.TypeName,item.Id,item.Modified,item.Expiration);
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
                kv.Add(item.Key, item.Copy(copyBody));
            }
            return kv;
        }

        /// <summary>
        /// Get entity values.
        /// </summary>
        /// <returns></returns>
        public EntityStream[] GetEntityValues()
        {
            lock (syncLock)
            {
                return Items.Values.ToArray();
            }
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
        /// Get Values
        /// </summary>
        public ICollection Values
        {
            get
            {
                return Items.Values;
            }
        }

        /// <summary>
        /// Get Keys
        /// </summary>
        public ICollection Keys
        {
            get
            {
                return Items.Keys;
            }
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
            get {return typeof(T); }
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
        /// Get item as <see cref="EntityStream"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public EntityStream GetEntityStream(CacheKeyInfo info)
        {
            if (info == null || info.IsEmpty)
                return null;

            return GetEntityStream(info.CacheKey);
        }

        /// <summary>
        /// Get item as <see cref="EntityStream"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal EntityStream GetEntityStream(string key)
        {
            if (!IsReady)
            {
                CacheLogger.Info("SyncItemStream is not ready");
                return null;// throw new Exception("SyncItemStream is not ready");
            }
            EntityStream entity = null;

            lock (syncLock)
            {
                if (Items.TryGetValue(key, out entity))
                {
                    return entity;
                }
            }

            return null;
        }
        /// <summary>
        /// Get item as <see cref="NetStream"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream GetItemStream(CacheKeyInfo info)
        {
            if (info == null)
                return null;
            return GetItemStream(info.CacheKey);
        }

        internal NetStream GetItemStream(string key)
        {
            EntityStream es = GetEntityStream(key);
            if (es == null)
            {
                return null;
            }
            return es.BodyStream;
        }
        /// <summary>
        /// Get item as byte array
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public byte[] GetItemBinary(CacheKeyInfo info)
        {
            if (info == null || info.IsEmpty)
                return null;
            return GetItemBinary(info.CacheKey);
        }

        internal byte[] GetItemBinary(string key)
        {
            EntityStream es = GetEntityStream(key);
            if (es == null)
            {
                return null;
            }
            return es.BodyStream == null ? null : es.BodyStream.ToArray();
        }

        #endregion

        #region SetContext

        /// <summary>
        /// Refresh all items in Dictionary
        /// </summary>
        public override void Refresh(object state)
        {
            var watch = Stopwatch.StartNew();
            long totalSize = 0;
            int currentCount = Count;
            Dictionary<string, EntityStream> items = null;
            string entityName = null;

            try
            {
                if (SyncSource == null)
                {
                    throw new Exception("Invalid DataSyncEntity in SyncItemStream, SyncSource is null.");
                }

                entityName = SyncSource.EntityName;

                lock (taskLock)
                {
                    items = EntityStream.CreateEntityRecordStream(Context, Filter, out totalSize);//-GenericRecord
                    if (items == null)
                    {
                        throw new Exception("CreateEntityStream is null, Refreshed Sync Item failed.");
                    }
                }

                lock (syncLock)
                {
                    _Items = items;
                }

                AgentManager.SyncCache.SizeExchage(_Size, totalSize,currentCount, Count, false);//true);
                _Size = totalSize;

                watch.Stop();

                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Ok, "Refresh Sync Item completed : {0}, Duration Milliseconds: {1}", entityName, watch.ElapsedMilliseconds.ToString());

            }
            catch (Exception ex)
            {
                watch.Stop();
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "Refresh Sync Item error : {0}, Message: {1} ", entityName, ex.Message);
            }
        }

        

        /// <summary>
        /// Get if contains item by key in Dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal override bool Contains(string key)
        {
            lock (syncLock)
            {
                return Items.ContainsKey(key);
            }

        }
        #endregion

    }
}
