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
using Nistec.Generic;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Threading;
using Nistec.Caching.Server;
using System.Collections.Concurrent;
using Nistec.Channels;
using Nistec.Caching.Remote;

namespace Nistec.Caching.Sync.Embed
{
   
    /// <summary>
    ///  Represent the sychronize item info for each item in sync cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SyncTable<T> : SyncTableBase<T>,ISyncTable, ISyncEx<T>, ISyncEx //where T: IEntityItem
    {

        internal Action<string,DataTable> OnSyncCompleted;
        #region ctor

        /// <summary>
        /// Create new <see cref="SyncTable{T}"/>
        /// </summary>
        public SyncTable()
            : base(false)
        {

        }

        /// <summary>
        /// Create new <see cref="SyncTable{T}"/>
        /// </summary>
        public SyncTable(bool isAsync)
            : base(isAsync)
        {

        }


        /// <summary>
        /// Create new <see cref="SyncTable{T}"/>
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
        public SyncTable(string connectionKey, string entityName, string mappingName, string[] keys,string columns, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : base(isAsync)
        {

            Set(connectionKey, entityName, mappingName, new string[] { mappingName }, EntitySourceType.Table, keys, columns, timer, enableNoLock, commandTimeout);
        }

        /// <summary>
        /// Create new <see cref="SyncTable{T}"/>
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
        public SyncTable(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] keys, string columns, SyncTimer timer, bool enableNoLock, int commandTimeout, bool isAsync)
            : base(isAsync)
        {
            Set(connectionKey, entityName, mappingName, sourceName, sourceType, keys,columns, timer, enableNoLock, commandTimeout);
        }
        /// <summary>
        /// Create new <see cref="SyncTable{T}"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isAsync"></param>
        public SyncTable(SyncEntity entity, bool isAsync)
            : base(entity,isAsync)
        {
        }

                

        #endregion

        #region IDisposable
        /// <summary>
        /// Destructor.
        /// </summary>
        ~SyncTable()
        {
             Dispose(false);
        }
        /// <summary>
        /// Dispose item.
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

        #region Collection Properties

        ConcurrentDictionary<string,T> _Items;
        ConcurrentDictionary<string, T> ItemsInternal
        {
            get
            {
                if (_Items == null)
                {
                    _Items = new ConcurrentDictionary<string, T>();
                }
                return _Items;
            }
        }

        /// <summary>
        /// Get Entities Storage
        /// </summary>
        public IDictionary<string,T> Items
        {
            get
            {
                //if (_Items == null)
                //{
                //    _Items = new ConcurrentDictionary<string, T>();
                //}
                return ItemsInternal;
            }
        }

        IDictionary<string, T> ISyncEx<T>.Items
        {
            get { return ItemsInternal; }
        }

        IDictionary ISyncEx.Items
        {
            get {

                //if (_Items == null)
                //{
                //    _Items = new ConcurrentDictionary<string, T>();
                //}
                return ItemsInternal;
            }
        }

        ///// <summary>
        ///// Get Values
        ///// </summary>
        //public ICollection Values
        //{
        //    get
        //    {
        //        return ((ISyncEx)ItemsInternal).Items.Values;
        //    }
        //}

        ///// <summary>
        ///// Get Keys
        ///// </summary>
        //public ICollection Keys
        //{
        //    get
        //    {
        //        return Items.Keys;
        //    }
        //}

        /// <summary>
        /// Get Items count
        /// </summary>
        public int Count
        {
            get
            {
                return ItemsInternal.Count;
            }
        }
        #endregion
       
        #region Get item

       
        /// <summary>
        /// Get item as IDictionary
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public IDictionary GetRecord(ComplexKey info)
        {
            if (info == null || info.IsEmpty)
                return null;
           return GetRecord(info.Suffix);
        }

        /// <summary>
        /// Get item as IDictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IDictionary GetRecord(string key)
        {
            T entity = Get(key);

            using (EntityContext context = new EntityContext<T>(entity))
            {
                GenericRecord gr = context.EntityRecord;
                if (gr != null)
                {
                    return gr;
                }
            }
            return null;
        }
       
         /// <summary>
        /// Get item as object
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public object GetItem(ComplexKey info)
        {
             if (info == null || info.IsEmpty)
                return null;
            return Get(info.Suffix);
        }

        /// <summary>
        /// Get item as object
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal object GetItem(string key)
        {

            T entity = this.Get(key);

            return (object)entity;
        }

        /// <summary>
        /// Get item as Generic Entity
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public T Get(ComplexKey info)
        {
            if (info == null || info.IsEmpty)
               return default(T);
            return Get(info.Suffix);
        }

        /// <summary>
        /// Get item as Generic Entity
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal T Get(string key)
        {
            T entity = default(T);
  
            //lock (syncLock)
            //{
                if (ItemsInternal.TryGetValue(key, out entity))
                {
                    return entity;
                }
            //}

            return entity;
        }

        /// <summary>
        /// Get item as <see cref="NetStream"/> Entity
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream GetItemStream(ComplexKey info)
        {
            if (info == null)
                return null;// TransWriter.Write(CacheState.ArgumentsError.ToString() , TransType.Error);
            return GetItemStream(info.Suffix);
        }

        public NetStream GetItemStream(string key)
        {
            return TransStream.ToStream(Get(key));
            //return TransWriter.Write(Get(key), TransType.Object);
        }

       
        /// <summary>
        /// Get item as byte array
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public byte[] GetItemBinary(ComplexKey info)
        {
            if (info == null)
                return null;
            return GetItemBinary(info.Suffix);
        }

        internal byte[] GetItemBinary(string key)
        {
            var ns = GetItemStream(key);
            if (ns == null)
                return null;
            return ns.ToArray();
        }

        /// <summary>
        /// Get item as <see cref="NetStream"/> Dictionary
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream GetRecordStream(ComplexKey info)
        {
            if (info == null)
                return null;// TransWriter.Write(CacheState.ArgumentsError.ToString(), TransType.Error); 
            return GetItemStream(info.Suffix);
        }

        /// <summary>
        /// Get item as IDictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public NetStream GetRecordStream(string key)
        {
            return TransStream.ToStream(GetRecord(key));
        }

        /// <summary>
        /// Get item as IDictionary
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public byte[] GetRecordBinary(ComplexKey info)
        {
            if (info == null)
                return null;
            return GetRecordBinary(info.Suffix);

        }
        /// <summary>
        /// Get item as IDictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal byte[] GetRecordBinary(string key)
        {

            var ns=GetRecordStream(key);
            if(ns==null)
                return null;
            return ns.ToArray();

        }

        #endregion

        #region override


        /// <summary>
        /// Refresh all items in Dictionary
        /// </summary>
        public override void Refresh(object state)
        {
            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Debug, "Refreshe Sync Item start : " + this.SyncSource.EntityName);

            ConcurrentDictionary<string, T> items = null;
            DataTable dt;
            lock (taskLock)
            {

                dt = EntityExtension.CreateEntityData<T>(_Context, Filter, OnError);
                if (dt == null)
                {
                    CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Failed, "Refreshe Sync Data Failed : " + this.SyncSource.EntityName);
                    return;
                }
                items = EntityExtension.CreateConcurrentEntityList<T>(_Context, dt, OnError);
                if (items == null)
                {
                    CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Failed, "Refreshe Sync Item Failed : " + this.SyncSource.EntityName);
                    return;
                }
            }

#if (Synchronize)

                //Synchronize(items,true);
                DictionaryUtil util = new DictionaryUtil();
                util.SynchronizeParallel<string, T>(_Items, items, true, LogActionSync);
#else
            lock (syncLock)
            {
                _Items = items;
            }
#endif
            if (SyncSource != null)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Ok, "Refresh Sync Item completed : " + this.SyncSource.EntityName);
            }

            //T item;
            //if(_Items.TryGetValue(this.SyncSource.EntityName, out item))
            //{

            //}

            if (OnSyncCompleted != null)
                OnSyncCompleted(this.SyncSource.EntityName, dt.Copy());
        }

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
            //lock (syncLock)//(((ICollection)items).SyncRoot)
            //{
                return ItemsInternal.ContainsKey(key);

            //}
        }
        #endregion
    }
}
