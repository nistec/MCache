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
using System.Text;
using System.Configuration;
using Nistec.Caching;
using System.Data;
using Nistec.Data;
using Nistec.Caching.Data;
using Nistec.Caching.Remote;
using Nistec.Generic;
using Nistec.Data.Entities;
using System.Collections;
using Nistec.Data.Entities.Cache;
using System.Diagnostics;
using Nistec.IO;
using System.Threading;
using Nistec.Runtime;
using Nistec.Channels;
using System.Xml;
using System.Linq;
using Nistec.Serialization;
using Nistec.Caching.Config;

namespace Nistec.Caching.Sync.Remote
{
    /// <summary>
    /// Represents Synchronize db cache for remote cache.
    /// which manages them using <see cref="SyncDbCache"/>.  
    /// The synchronization properties are configurable using "SyncFile",
    /// that uses <see cref="SysFileWatcher"/> which Listens to the file system change notifications and raises events when a
    /// file is changed.
    /// The synchronization process are using in <see cref="SyncBagStream"/> to ensure that each item will stay synchronized
    /// in run time without any interruption in process.
    /// When problem was occured during the sync process , the item, will stay as the original item.    
    /// </summary>
    public class SyncCacheStream : SyncCacheBase, IDisposable, ICachePerformance//,ISyncStream
    {

        #region ICachePerformance

        CachePerformanceCounter m_Perform;
        /// <summary>
        /// Get <see cref="CachePerformanceCounter"/> Performance Counter.
        /// </summary>
        public CachePerformanceCounter PerformanceCounter
        {
            get { return m_Perform; }
        }

        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void ICachePerformance.MemorySizeExchange(ref long memorySize)
        {
            CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.None, "Memory Size Exchange: " + CacheName);
            long size = GetAllSyncSize();
            Interlocked.Exchange(ref memorySize, size);
        }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long ICachePerformance.GetMaxSize()
        {
            return CacheSettings.MaxSize;
        }
        bool ICachePerformance.IsRemote
        {
            get { return true; }
        }
        int ICachePerformance.IntervalSeconds
        {
            get { return base.IntervalSeconds; }
        }
        bool ICachePerformance.Initialized
        {
            get { return base.Initialized; }
        }
        #endregion

        #region size exchange

        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeValidate(long newSize)
        {
            if (!CacheSettings.EnableSizeHandler)
                return CacheState.Ok;
            return PerformanceCounter.SizeValidate(newSize);
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="newSize"></param>
        /// <param name="currentCount"></param>
        /// <param name="newCount"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        internal protected virtual void SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;
            PerformanceCounter.ExchangeSizeAndCountAsync(currentSize, newSize, currentCount, newCount, exchange, CacheSettings.EnableSizeHandler);

        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected virtual void SizeExchage(ISyncTableStream oldItem, ISyncTableStream newItem)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;

            long oldSize = 0;
            int oldCount = 0;
            long newSize = 0;
            int newCount = 0;

            if (oldItem != null)
            {
                oldSize = oldItem.Size;
                oldCount = oldItem.Count;
            }
            if (newItem != null)
            {
                newSize = newItem.Size;
                newCount = newItem.Count;
            }
            PerformanceCounter.ExchangeSizeAndCountAsync(oldSize, newSize, oldCount, newCount, false, CacheSettings.EnableSizeHandler);
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
            if (CacheSettings.EnablePerformanceCounter)
            {
                PerformanceCounter.RefreshSize();
            }
        }

        #endregion

        #region Load and Sync

        internal void Reload(SyncBagStream copy)
        {
            _SyncBag.Reload(copy);
        }

        SyncBagStream BagCopy()
        {
            return new SyncBagStream(this);
        }

        /*
                int synchronized;

                internal override void LoadSyncTables(XmlNode node)
                {
                    if (node == null)
                        return;

                    //~Console.WriteLine("Debuger-SyncCacheStream.LoadSyncTables start");
                    bool EnableAsyncTask = CacheSettings.EnableAsyncTask;

                    try
                    {
                        if (0 == Interlocked.Exchange(ref synchronized, 1))
                        {
                            XmlNodeList list = node.ChildNodes;
                            if (list == null)
                            {
                                CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, "LoadSyncTables is empty");
                                return;
                            }

                            var newSyncEntityItems = SyncEntity.GetItems(list);

                            if (newSyncEntityItems == null || newSyncEntityItems.Length == 0)
                            {
                                throw new Exception("Can not LoadSyncTables, SyncEntity Items not found");
                            }

                            DoSyncLoader(newSyncEntityItems, EnableAsyncTask);
                        }
                    }
                    catch (Exception ex)
                    {
                        CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, string.Format("LoadSyncTables error: {0}", ex.Message));

                        OnError("LoadSyncTables error " + ex.Message);
                    }
                    finally
                    {
                        //Release the lock
                        Interlocked.Exchange(ref synchronized, 0);
                    }
                }
        */
        protected override void DoSyncLoader(SyncEntity[] newSyncEntityItems, bool EnableAsyncTask)
        {

            bool hasChange = false;
            bool enableLoader = CacheSettings.EnableAsyncLoader;

            if (SyncEntityItems != null)
            {
                //find missing items
                foreach (SyncEntity sync in SyncEntityItems)
                {
                    //var item = newSyncEntityItems.Contains()//Where(i => i.EntityName == sync.EntityName).FirstOrDefault();
                    if (!newSyncEntityItems.Any(s => s.EntityName == sync.EntityName && s.ConnectionKey == sync.ConnectionKey))
                    {
                        if (RemoveItem(sync.EntityName))
                        {
                            this._DataCache.RemoveSyncSource(sync.ConnectionKey, sync.EntityName);
                            this._SyncBag.RemoveTable(sync.EntityName);

                            //dbCopy.RemoveSyncSource(sync.ConnectionKey, sync.EntityName);
                            //bagCopy.RemoveItem(sync.EntityName);

                            hasChange = true;
                        }
                    }
                }

                foreach (SyncEntity sync in newSyncEntityItems)
                {
                    if (!this._DataCache.IsSyncSourceExists(sync))
                    {
                        AddItem(sync);
                        hasChange = true;
                    }
                    else if (SyncEntityItems.Any(cur =>
                                   (sync.EntityType != cur.EntityType ||
                                   sync.Interval != cur.Interval ||
                                   sync.SourceType != cur.SourceType ||
                                   sync.SyncType != cur.SyncType ||
                                   sync.ViewName != cur.ViewName ||
                                   sync.Columns != cur.Columns ||
                                   string.Join(",", sync.EntityKeys) != string.Join(",", cur.EntityKeys) ||
                                   string.Join(",", sync.SourceName) != string.Join(",", cur.SourceName)) &&
                                   (sync.EntityName == cur.EntityName &&
                                   sync.ConnectionKey == cur.ConnectionKey)))
                    {
                        //this.Refresh(sync.EntityName);
                        AddItem(sync);
                        hasChange = true;
                    }
                }
            }
            else
            {
                foreach (SyncEntity sync in newSyncEntityItems)
                {
                    if (!this._DataCache.IsSyncSourceExists(sync))
                    {
                        AddItem(sync);
                        hasChange = true;
                    }
                }
            }

            if (hasChange)
            {
                lock (ThreadLock)
                {
                    _DataCache.Start(IntervalSeconds);

                    SyncEntityItems = newSyncEntityItems;
                }
            }
        }

        internal override CacheState AddItem(SyncEntity entity) //where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            SyncTableStream<EntityStream> item = new SyncTableStream<EntityStream>(entity, true);
            while (!item.IsReady)
            {
                if (item.IsTimeout)
                {
                    throw new Exception("SyncTableStream timeout error: " + entity.EntityName);
                }
                Thread.Sleep(100);
            }
            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

            return _SyncBag.Set(item);

        }
        #endregion

        #region members

        const string DefaultCacheName = "SyncCache";
        
        #endregion

        #region ctor

        /// <summary>
        /// Initilaize new instance of <see cref="SyncCacheStream"/>
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="isWeb"></param>
        public SyncCacheStream(string cacheName, bool isWeb = false)
            : base(cacheName,isWeb)
        {
            m_Perform = new CachePerformanceCounter(this, CacheAgentType.SyncCache, CacheName);
            _SyncBag = new SyncBagStream(this);
        }

        #endregion

        #region override

      
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_SyncBag != null)
            {
                _SyncBag.Clear();
                _SyncBag = null;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region ISyncBag implementation

        private SyncBagStream _SyncBag;


        /// <summary>
        /// Returns the number of elements in the sync cache.
        /// </summary>
        public int CacheCount
        {
            get { return _SyncBag.Count; }
        }

       
        /// <summary>
        /// Get Data Cache connection Keys
        /// </summary>
        /// <returns></returns>
        public ICollection<string> DataCacheConnections()
        {
            return _DataCache.GetKeys();
        }

        /// <summary>
        /// Get Cache Keys
        /// </summary>
        /// <returns></returns>
        public ICollection<string> CacheKeys()
        {
            return _SyncBag.GetKeys(); 
        }

        /// <summary>
        /// Get all <see cref="ISyncTable"/> items in cache. 
        /// </summary>
        /// <returns></returns>
        public ICollection<ISyncTableStream> CacheValues()
        {
           return _SyncBag.GetTables();
        }

        /// <summary>
        /// Get the size of all items in all cref="ISyncTable"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public long GetAllSyncSize()
        {
            long size = 0;
            ISyncTableStream[] Col = _SyncBag.GetTables();

            foreach (ISyncTableStream syncitem in Col)
            {
                size += syncitem.Size;
            }
          
            return size;
        }

        /// <summary>
        /// Get the count of all items in all cref="ISyncTable"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public int GetAllSyncCount()
        {
            int count = 0;
           
            ISyncTableStream[] Col = _SyncBag.GetTables();
            if (Col == null)
                return 0;
            foreach (ISyncTable syncitem in Col)
            {
                count += syncitem.Count;
            }
            return count;
        }

        /// <summary>
        /// Get if cache contains spesific item by <see cref="ComplexKey"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Contains(ComplexKey info)
        {
            return _SyncBag.Contains(info);
        }

        /// <summary>
        /// Get if cache contains spesific item by keys
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool Contains(string name, string[] keys)
        {
            return Contains(ComplexArgs.Get(name, keys));
        }


        /// <summary>
        /// Refresh <see cref="ISyncTable"/>
        /// </summary>
        /// <param name="syncName"></param>
        public override void Refresh(string syncName)
        {
            if (_SyncBag == null)
                return;
            _SyncBag.Refresh(syncName);
        }

     
        /// <summary>
        /// Remove specific item from sync cache.
        /// </summary>
        /// <param name="syncName"></param>
        /// <returns></returns>
        public override bool RemoveItem(string syncName)
        {
            if (_SyncBag == null)
                return false;
           return _SyncBag.RemoveTable(syncName);
        }

        public bool Exists(string syncName)
        {
            if (_SyncBag == null)
                return false;
            return _SyncBag.Exists(syncName);
        }

        /// <summary>
        /// Refrech cache
        /// </summary>
        public void RefreshAll()
        {
            if (_SyncBag == null)
                return;

            _SyncBag.RefreshAllAsync();
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        /// <param name="clearDataCache"></param>
        public override void Clear(bool clearDataCache)
        {
            if (_SyncBag == null)
                return;

            _SyncBag.Clear();

            if (clearDataCache)
            {
                if (_DataCache == null)
                    return;
                _DataCache.ClearSafe();
            }
        }
        #endregion

        #region Get/Set Items


        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncTableStream<T> GetItem<T>(string name) where T : IEntityItem
        {
            return _SyncBag.GetTable<T>(name);
        }

        internal bool TryGetTable<T>(string name, out SyncTableStream<T> item) where T : IEntityItem
        {
            return (_SyncBag.TryGetTable<T>(name, out item));
        }
        internal bool TryGetTable(string name, out ISyncTableStream item)
        {
            return (_SyncBag.TryGetTable(name, out item));
        }


        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISyncTableStream GetTable(string name)
        {
            return _SyncBag.GetTable(name);
        }
        internal string[] GetNames()
        {
            return _SyncBag.GetKeys();
        }

        internal EntityStream GetEntityStreamInternal(MessageStream message)
        {
            //var keyinfo = ComplexArgs.Parse(message.Key);
            //var syncItem = GetItem(keyinfo.Name);

            var syncItem = GetTable(message.Label);
            if (syncItem == null)
                return null;
            return syncItem.GetEntityStream(message.Identifier);
        }
        internal CacheItemReport GetItemsReport(MessageStream message)
        {
            var syncItem = GetTable(message.Label);
            if (syncItem == null)
                return null;
            return new CacheItemReport(syncItem);
        
        }

        internal GenericKeyValue GetEntityItems(MessageStream message)
        {
            var syncItem = GetTable(message.Label);
            if (syncItem == null)
                return null;
            return syncItem.GetEntityItems(false);
        }

        internal int GetEntityItemsCount(MessageStream message)
        {
            var syncItem = GetTable(message.Label);
            if (syncItem == null)
                return 0;
            return syncItem.GetEntityItemsCount();
        }

        internal string[] GetEntityKeys(MessageStream message)
        {
            var syncItem = GetTable(message.Label);
            if (syncItem == null)
                return null;
            return syncItem.GetEntityKeys().ToArray();
        }


        #endregion

        #region Get/Set Stream

        /// <summary>
        /// Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public object Get(ComplexKey info)
        {
            ISyncTableStream syncitem;
            if (TryGetTable(info.Prefix, out syncitem))
            {
                EntityStream es;
                if (syncitem.TryGetEntity(info.Suffix, out es))
                {
                    return es.DecodeBody();
                }
            }

            return null;
;
        }

        
        /// <summary>
        /// Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public T GetBody<T>(ComplexKey info) where T : IEntityItem
        {
            SyncTableStream<T> syncitem;
            if (TryGetTable<T>(info.Prefix, out syncitem))
            {
                EntityStream es;
                if(syncitem.TryGetEntity(info.Suffix, out es))
                {
                    return es.DecodeBody<T>();
                }
            }

            return default(T);

        }
        /// <summary>
        /// Get copy of spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T GetBody<T>(string name, string[] keys) where T : IEntityItem
        {
            return GetBody<T>(ComplexArgs.Get(name, keys));
        }


        internal NetStream GetBodyStream(MessageStream message)
        {

            NetStream b = null;

            if (message == null)
                throw new ArgumentNullException("CacheMessage");
            message.ValiddateInfo();

            if (TryGetBodyStream(message.Label, message.Identifier, out b))
            {
                return b;
            }

            //b = Get(message.Key,message.Detail);

            if (b == null)
            {
                CacheLogger.Debug("sync get stream not found : " + message.KeyInfo());
            }

            return b;
        }

        /// <summary>
        ///  Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetBodyStream(string name, string key, out NetStream value)
        {
            ISyncTableStream syncitem;
            if (TryGetTable(name, out syncitem))
            {
                EntityStream es;
                if (syncitem.TryGetEntity(key, out es))
                {
                    value= es.GetCopy();
                    return true;
                }
            }
            value = null;
            return false;

            //ISyncTableStream syncitem = GetItem(name);
            //if (syncitem != null)
            //{
            //    return syncitem.GetItemStream(key);
            //}
            //return null;
        }

        public bool TryGetBodyStream(ComplexKey ck, out NetStream value)
        {
            ISyncTableStream syncitem;
            if (TryGetTable(ck.Prefix, out syncitem))
            {
                EntityStream es;
                if (syncitem.TryGetEntity(ck.Suffix, out es))
                {
                    value = es.GetCopy();
                    return true;
                }
            }
            value = null;
            return false;

            //ISyncTableStream syncitem = GetItem(name);
            //if (syncitem != null)
            //{
            //    return syncitem.GetItemStream(key);
            //}
            //return null;
        }


        /// <summary>
        /// Get copy of spesific value from cache as entity, using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream GetAs(MessageStream message)
        {
            return GetBodyStream(message);
        }

        public string[] GetEntityPrimaryKey(string entityName)
        {
            ISyncTableStream table;
            if (TryGetTable(entityName, out table))
            {
                return table.FieldsKey;
            }
            return null;
        }

        public NetStream FindEntityByJson(string entityName, string json)
        {
            return FindEntity(entityName, NameValueArgs.ParseJson(json));
        }
        public NetStream FindEntityByQueryString(string entityName, string queryString)
        {
            return FindEntity(entityName, NameValueArgs.ParseQueryString(queryString));
        }
        public NetStream FindEntityByQueryString(string entityName, string[] keyValue)
        {
            return FindEntity(entityName, NameValueArgs.Create(keyValue));
        }
        public NetStream FindEntity(string entityName, NameValueArgs nv)
        {
            if(entityName==null)
            {
                throw new ArgumentNullException("entityName");
            }
            if (nv == null)
            {
                throw new ArgumentNullException("nv");
            }
            NetStream b = null;

            ISyncTableStream table;
            if (TryGetTable(entityName, out table))
            {
                string pk = table.GetPrimaryKey(nv);
                b = table.GetItemStream(pk);
            }

            if (b == null)
            {
                CacheLogger.Debug("sync get stream not found : " + entityName);
            }

            return b;
        }

        /// <summary>
        /// Get copy of spesific value from cache as entity, using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream GetEntity(MessageStream message)
        {
            NetStream b = GetBodyStream(message);

            if (b != null)
            {
                //SerializeTools.ChangeContextType(b, SerialContextType.dictionaryEntityType);//.AnyClassType.GenericEntityAsIEntityType);
                return b;
            }
            return null;
        }
        /// <summary>
        /// Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public T GetEntity<T>(ComplexKey info) where T : IEntityItem
        {
            SyncTableStream<T> syncitem;
            if (TryGetTable<T>(info.Prefix, out syncitem))
            {
                EntityStream es;
                if (syncitem.TryGetEntity(info.Suffix, out es))
                {
                    return es.DecodeBody<T>();
                }
            }

            return default(T);
        }
        /// <summary>
        /// Get copy of spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T GetEntity<T>(string name, string[] keys) where T : IEntityItem
        {
            return GetEntity<T>(ComplexArgs.Get(name, keys));
        }

        /// <summary>
        /// Get spesific value as dictionary stream from cache using <see cref="MessageStream"/> , if item not found return null. 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream GetRecord(MessageStream message)
        {
            NetStream b = GetBodyStream(message);
            if (b != null)
            {
                //NetStream copy = b.Copy();
                SerializeTools.ChangeContextType(b, SerialContextType.IEntityDictionaryType);
                return b;
            }

            return null;
        }
        /// <summary>
        /// Get spesific value as dictionary stream from cache using name and keys arguments , if item not found return null. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public NetStream GetRecord(string name, string[] keys)
        {
            return GetRecord(ComplexArgs.Get(name, keys));
        }
        /// <summary>
        /// Get spesific value as dictionary stream from cache using <see cref="ComplexKey"/> , if item not found return null. 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream GetRecord(ComplexKey info)
        {
            NetStream b;
            if (TryGetBodyStream(info, out b))
            {
                SerializeTools.ChangeContextType(b, SerialContextType.dictionaryEntityType);//.GenericEntityAsIDictionaryType);
                return b;
            }
            return null;
        }

        /// <summary>
        /// Get spesific value as dictionary stream from cache using <see cref="ComplexKey"/> , if item not found return null. 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public IDictionary<string,object> GetGenericRecord(ComplexKey info)
        {
            NetStream value;

            if (TryGetBodyStream(info, out value))
            {
                //SerializeTools.ChangeContextType(value, SerialContextType.dictionaryEntityType);// GenericEntityAsIDictionaryType);
                using (var streamer = new BinaryStreamer(value))
                {
                    return streamer.ReadEntityAsDictionary(true,true);//.ReadGenericEntityAsDictionary(true);
                }
            }
            return null;
        }

        internal object Get(MessageStream message)
        {
            return Get(ComplexKey.Get(message.Label, message.Identifier), message.Args[KnownArgs.Column]);
        }

        public object Get(ComplexKey info, string field)
        {
            NetStream ns;

            if (TryGetBodyStream(info, out ns))
            {
                //SerializeTools.ChangeContextType(ns, SerialContextType.dictionaryEntityType);// GenericEntityAsIDictionaryType);
                using (var streamer = new BinaryStreamer(ns))
                {
                    var dic = streamer.ReadEntityAsDictionary(true,true);
                    object value;
                    if(dic.TryGetValue(field, out value))
                    {
                        return value;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Get/Set Values

        /// <summary>
        ///  Get copy of spesific entity from cache using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public EntityStream GetEntityStream(MessageStream message)
        {
            if (message == null)
                throw new ArgumentNullException("CacheMessage");

            message.ValiddateInfo();

            //ComplexKey info = ComplexArgs.Parse(message.Key);
            //if (info == null || info.IsEmpty)
            //    throw new ArgumentException("ComplexKey is null or empty");
            
            return GetEntityStream(message.Label, message.Identifier);
        }
        

        /// <summary>
        /// Get copy of spesific entity from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public EntityStream GetEntityStream(string name, string key)
        {
            ISyncTableStream syncitem = GetTable(name);
            if (syncitem != null)
            {
                return syncitem.GetEntityStream(key);
            }
            return null;
        }
        #endregion

        #region Add items

        /// <summary>
        /// Add Item to Sync cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="sourceType"></param>
        /// <param name="entityKeys"></param>
        /// <param name="columns"></param>
        /// <param name="interval"></param>
        /// <param name="syncType"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public override CacheState AddItem<T>(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] entityKeys, string columns, TimeSpan interval, SyncType syncType, bool enableNoLock=false, int commandTimeout=0)
        {
            if (connectionKey == null)
                throw new ArgumentNullException("AddItem.connectionKey");
            if (entityName == null)
                throw new ArgumentNullException("AddItem.entityName");
            if (mappingName == null)
                throw new ArgumentNullException("AddItem.mappingName");
            if (sourceName == null)
                throw new ArgumentNullException("AddItem.sourceName");
            if (entityKeys == null)
                throw new ArgumentNullException("AddItem.entityKeys");

            try
            {
                SyncTimer timer = new SyncTimer(interval, syncType);


                SyncTableStream<T> item = new SyncTableStream<T>(connectionKey, entityName, mappingName, sourceName, sourceType, entityKeys, columns, timer,enableNoLock, commandTimeout,true);

                while (!item.IsReady)
                {
                    if (item.IsTimeout)
                    {
                        throw new Exception("SyncTableStream timeout error: " + entityName);
                    }
                    Thread.Sleep(100);
                }
              

                item.Validate();

                _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);


               return _SyncBag.Set(entityName, item as ISyncTableStream);

            }
            catch (Exception ex)
            {
                OnError("AddItem error " + ex.Message);
                return CacheState.AddItemFailed;
            }

        }



        /// <summary>
        /// Add Item to Sync cache
        /// </summary>
        /// <param name="item"></param>
        public CacheState AddItem<T>(SyncTableStream<T> item) //where T : IEntityItem
        {
       
            if (item == null)
            {
                throw new ArgumentNullException("AddSyncTable.SyncTableStream");
            }

            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

            return _SyncBag.Set(item);//[item.Info.ItemName] = item as ISyncTableStream;
           
        }


        /// <summary>
        /// Add Item to Sync cache
        /// </summary>
        /// <param name="message"></param>
        public CacheState AddItem(MessageStream message) //where T : IEntityItem
        {
            if (message == null)
            {
                throw new ArgumentNullException("AddItem.CacheMessage");
            }
            SyncTableStream<EntityStream> item = new SyncTableStream<EntityStream>(message.Args, true);
            while (!item.IsReady)
            {
                if (item.IsTimeout)
                {
                    throw new Exception("SyncTableStream timeout error: " + message.Identifier);
                }
                Thread.Sleep(100);
            }

            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

            return _SyncBag.Set(item);//[item.Info.ItemName] = item as ISyncTableStream;
        }




        #endregion

        /// <summary>
        /// Get properties of specified table name in data cache..
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public CacheItemProperties GetItemProperties(string entityName)
        {
            ISyncTableStream syncitem = GetTable(entityName);
            if (syncitem == null)
                return null;

            var dtprop = new TableProperties() { RecordCount = syncitem.Count, Size = syncitem.Size, ColumnCount = 0 };
            if (syncitem.SyncSource != null)
                return new CacheItemProperties(dtprop, syncitem.SyncSource);
            else
                return new CacheItemProperties(dtprop, entityName);
        }
    }

#if (false)
 public class SyncCacheStream : SyncCacheBase, IDisposable, ICachePerformance//,ISyncStream
    {

    #region ICachePerformance

        CachePerformanceCounter m_Perform;
        /// <summary>
        /// Get <see cref="CachePerformanceCounter"/> Performance Counter.
        /// </summary>
        public CachePerformanceCounter PerformanceCounter
        {
            get { return m_Perform; }
        }

        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void ICachePerformance.MemorySizeExchange(ref long memorySize)
        {
            CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.None, "Memory Size Exchange: " + CacheName);
            long size = GetAllSyncSize();
            Interlocked.Exchange(ref memorySize, size);
        }

        //internal long MaxSize { get { return CacheDefaults.DefaultCacheMaxSize; } }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long ICachePerformance.GetMaxSize()
        {
            return CacheSettings.MaxSize;
        }
        bool ICachePerformance.IsRemote
        {
            get { return true; }
        }
        int ICachePerformance.IntervalSeconds
        {
            get { return base.IntervalSeconds; }
        }
        bool ICachePerformance.Initialized
        {
            get { return base.Initialized; }
        }
    #endregion

    #region size exchange

        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeValidate(long newSize)
        {
            if (!CacheSettings.EnableSizeHandler)
                return CacheState.Ok;
            return PerformanceCounter.SizeValidate(newSize);
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="newSize"></param>
        /// <param name="currentCount"></param>
        /// <param name="newCount"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        internal protected virtual void SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;
            PerformanceCounter.ExchangeSizeAndCountAsync(currentSize, newSize, currentCount, newCount, exchange, CacheSettings.EnableSizeHandler);

        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected virtual void SizeExchage(ISyncTableStream oldItem, ISyncTableStream newItem)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;

            long oldSize = 0;
            int oldCount = 0;
            long newSize = 0;
            int newCount = 0;

            if (oldItem != null)
            {
                oldSize = oldItem.Size;
                oldCount = oldItem.Count;
            }
            if (newItem != null)
            {
                newSize = newItem.Size;
                newCount = newItem.Count;
            }
            PerformanceCounter.ExchangeSizeAndCountAsync(oldSize, newSize, oldCount, newCount, false, CacheSettings.EnableSizeHandler);
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
            if (CacheSettings.EnablePerformanceCounter)
            {
                PerformanceCounter.RefreshSize();
            }
        }

    #endregion

    #region Load and Sync

        ///// <summary>
        ///// ReloadSyncTable
        ///// </summary>
        ///// <param name="entity"></param>
        //public void ReloadSyncTable(DataSyncEntity entity) 
        //{
        //    try
        //    {
        //        if (entity == null)
        //        {
        //            throw new ArgumentNullException("SyncTable.DataSyncEntity.entity");
        //        }
        //        if (entity.SyncEntity == null)
        //        {
        //            throw new ArgumentNullException("SyncTable.DataSyncEntity.SyncEntity");
        //        }

        //        entity.SyncEntity.ValidateSyncEntity();

        //        SyncTableStream<EntityStream> item = new SyncTableStream<EntityStream>(entity.SyncEntity, true);

        //        while (!item.IsReady)
        //        {
        //            if (item.IsTimeout)
        //            {
        //                throw new TimeoutException("SyncTableStream timeout error: " + entity.EntityName);
        //            }
        //            Thread.Sleep(100);
        //        }
        //        item.Validate();

        //        var dbCopy = DataCacheCopy();
        //        var bagCopy = BagCopy();

        //        //var bagCopy = new SyncBagStream(this.CacheName, this);

        //        dbCopy.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

        //        bagCopy.Set(item);

        //        _DataCache.Reload(dbCopy);

        //        Reload(bagCopy);

        //        _DataCache.Start(IntervalSeconds);
        //    }
        //    catch (Exception ex)
        //    {
        //        string entityName = "";
        //        if (!(ex is ArgumentNullException))
        //        {
        //            entityName = entity.EntityName;
        //        }
        //        CacheLogger.Logger.LogAction(CacheAction.SyncTable, CacheActionState.Error, string.Format("SyncTable.SyncEntity copy {0} error: {1}", entityName, ex.Message));
        //    }

        //}

        internal void Reload(SyncBagStream copy)
        {
            _SyncBag.Reload(copy);
        }

        SyncBagStream BagCopy()
        {
            return new SyncBagStream(this);
        }

        int synchronized;

        internal override void LoadSyncTables(XmlNode node)
        {
            if (node == null)
                return;

            //~Console.WriteLine("Debuger-SyncCacheStream.LoadSyncTables start");
            bool EnableAsyncTask = CacheSettings.EnableAsyncTask;

            try
            {
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    XmlNodeList list = node.ChildNodes;
                    if (list == null)
                    {
                        CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, "LoadSyncTables is empty");
                        return;
                    }

                    var newSyncEntityItems = SyncEntity.GetItems(list);

                    if (newSyncEntityItems == null || newSyncEntityItems.Length == 0)
                    {
                        throw new Exception("Can not LoadSyncTables, SyncEntity Items not found");
                    }

                    DoSyncLoader(newSyncEntityItems, EnableAsyncTask);
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, string.Format("LoadSyncTables error: {0}", ex.Message));

                OnError("LoadSyncTables error " + ex.Message);
            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }

        void DoSyncLoader(SyncEntity[] newSyncEntityItems, bool EnableAsyncTask)
        {

            bool hasChange = false;
            bool enableLoader = CacheSettings.EnableAsyncLoader;

            if (SyncEntityItems != null)
            {
                //find missing items
                foreach (SyncEntity sync in SyncEntityItems)
                {
                    //var item = newSyncEntityItems.Contains()//Where(i => i.EntityName == sync.EntityName).FirstOrDefault();
                    if (!newSyncEntityItems.Any(s => s.EntityName == sync.EntityName && s.ConnectionKey == sync.ConnectionKey))
                    {
                        if (RemoveItem(sync.EntityName))
                        {
                            this._DataCache.RemoveSyncSource(sync.ConnectionKey, sync.EntityName);
                            this._SyncBag.RemoveItem(sync.EntityName);

                            //dbCopy.RemoveSyncSource(sync.ConnectionKey, sync.EntityName);
                            //bagCopy.RemoveItem(sync.EntityName);

                            hasChange = true;
                        }
                    }
                }

                foreach (SyncEntity sync in newSyncEntityItems)
                {
                    if (!this._DataCache.SyncSourceExists(sync))
                    {
                        AddItem(sync);
                        hasChange = true;
                    }
                    else if (SyncEntityItems.Any(cur =>
                                   (sync.EntityType != cur.EntityType ||
                                   sync.Interval != cur.Interval ||
                                   sync.SourceType != cur.SourceType ||
                                   sync.SyncType != cur.SyncType ||
                                   sync.ViewName != cur.ViewName ||
                                   sync.Columns != cur.Columns ||
                                   string.Join(",", sync.EntityKeys) != string.Join(",", cur.EntityKeys) ||
                                   string.Join(",", sync.SourceName) != string.Join(",", cur.SourceName)) &&
                                   (sync.EntityName == cur.EntityName &&
                                   sync.ConnectionKey == cur.ConnectionKey)))
                    {
                        //this.Refresh(sync.EntityName);
                        AddItem(sync);
                        hasChange = true;
                    }
                }
            }
            else
            {
                foreach (SyncEntity sync in newSyncEntityItems)
                {
                    if (!this._DataCache.SyncSourceExists(sync))
                    {
                        AddItem(sync);
                        hasChange = true;
                    }
                }
            }

            if (hasChange)
            {
                lock (ThreadLock)
                {
                    _DataCache.Start(IntervalSeconds);

                    SyncEntityItems = newSyncEntityItems;
                }
            }
        }


#if (false)
        internal override void LoadSyncTables(XmlNode node)
        {
            if (node == null)
                return;

            //~Console.WriteLine("Debuger-SyncCacheStream.LoadSyncTables start");

            bool EnableAsyncTask = CacheSettings.EnableAsyncTask;
            bool enableLoader = CacheSettings.EnableAsyncLoader;

            bool hasChange = false;
            try
            {
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    XmlNodeList list = node.ChildNodes;
                    if (list == null)
                    {
                        CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, "LoadSyncTables is empty");
                        return;
                    }

                    var newSyncEntityItems = SyncEntity.GetItems(list);

                    if (newSyncEntityItems == null || newSyncEntityItems.Length == 0)
                    {
                        throw new Exception("Can not LoadSyncTables, SyncEntity Items not found");
                    }
                    if (enableLoader)
                    {

                        DoSyncLoader(newSyncEntityItems, EnableAsyncTask);
                        return;
                    }
                    
                    if (EnableAsyncTask)
                    {
                        var dbCopy = DataCacheCopy();
                        var bagCopy = BagCopy();
                        //var syncBox = SyncBox.Instance;

                        if (SyncEntityItems != null)
                        {
                            //find missing items
                            foreach (SyncEntity sync in SyncEntityItems)
                            {
                                //var item = newSyncEntityItems.Contains()//Where(i => i.EntityName == sync.EntityName).FirstOrDefault();
                                if (!newSyncEntityItems.Any(s=> s.EntityName == sync.EntityName && s.ConnectionKey==sync.ConnectionKey))
                                {
                                    if (RemoveItem(sync.EntityName))
                                    {
                                        this._DataCache.RemoveSyncSource(sync.ConnectionKey, sync.EntityName);
                                        this._SyncBag.RemoveItem(sync.EntityName);

                                        //dbCopy.RemoveSyncSource(sync.ConnectionKey, sync.EntityName);
                                        //bagCopy.RemoveItem(sync.EntityName);

                                        hasChange = true;
                                    }
                                }
                            }

                            foreach (SyncEntity sync in newSyncEntityItems)
                            {
                                if (!this._DataCache.SyncSourceExists(sync))
                                {
                                    AddItem(sync, dbCopy, bagCopy);
                                    hasChange = true;
                                }
                            }

                            //foreach (SyncEntity nsync in newSyncEntityItems)
                            //{
                            //    if (!SyncEntityItems.Any(s => s.EntityName == nsync.EntityName && s.ConnectionKey == nsync.ConnectionKey))
                            //    {
                            //        AddItem(nsync);//, dbCopy, bagCopy);
                            //        hasChange = true;
                            //    }
                            //    else if(SyncEntityItems.Any(cur => 
                            //                                 (nsync.EntityType != cur.EntityType ||
                            //                                  nsync.Interval != cur.Interval ||
                            //                                  nsync.SourceType != cur.SourceType ||
                            //                                  nsync.SyncType != cur.SyncType ||
                            //                                  nsync.ViewName != cur.ViewName ||
                            //                                  string.Join(",", nsync.EntityKeys) != string.Join(",", cur.EntityKeys) ||
                            //                                  string.Join(",", nsync.SourceName) != string.Join(",", cur.SourceName)
                            //                                  )
                            //                                  && (nsync.EntityName == cur.EntityName &&
                            //                                  nsync.ConnectionKey == cur.ConnectionKey


                            //    )))
                            //    {
                            //        AddItem(nsync);//, dbCopy, bagCopy);
                            //        hasChange = true;
                            //    }
                            //}
                        }
                        else
                        {
                            foreach (SyncEntity sync in newSyncEntityItems)
                            {
                                if (!this._DataCache.SyncSourceExists(sync))
                                {
                                    AddItem(sync, dbCopy, bagCopy);
                                    hasChange = true;
                                }
                            }
                        }

                        //foreach (SyncEntity sync in newSyncEntityItems)
                        //{
 
                        //    if (!this._DataCache.SyncSourceExists(sync))
                        //    {
                        //        AddItem(sync, dbCopy, bagCopy);
                        //        hasChange = true;
                        //    }
                        //}

                        if (hasChange)
                        {
                            lock (ThreadLock)
                            {
                                _DataCache.Reload(dbCopy);

                                Reload(bagCopy);

                                _DataCache.Start(IntervalSeconds);

                                SyncEntityItems = newSyncEntityItems;
                            }
                        }

                        //dbCopy.DisposCopy();
                        //bagCopy.DisposeCopy();

                    }
                    else
                    {

                        if (_reloadOnChange)
                        {
                            Clear(true);
                        }

                        foreach (SyncEntity sync in newSyncEntityItems)
                        {
                            AddItem(sync);
                        }

                        //foreach (XmlNode n in list)
                        //{
                        //    if (n.NodeType == XmlNodeType.Comment)
                        //        continue;

                        //    AddItem(new XmlTable(n), false);
                        //}
                    }
                    
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, string.Format("LoadSyncTables error: {0}", ex.Message));

                OnError("LoadSyncTables error " + ex.Message);
            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }

        /*           
       internal override void LoadSyncTables(XmlNode node, bool EnableAsyncTask)
       {
           if (node == null)
               return;

           bool hasChange = false;
           //SyncDbCache dbCopy = null;
           //SyncBagStream bagCopy = null;
           try
           {
               //~Console.WriteLine("Debuger-SyncCacheStream.LoadSyncTables start");

               if (0 == Interlocked.Exchange(ref synchronized, 1))
               {
                   XmlNodeList list = node.ChildNodes;
                   if (list == null)
                       return;

                   if (EnableAsyncTask)
                   {
                       //dbCopy = DataCacheCopy();
                       //bagCopy = BagCopy();

                       //var syncBox = SyncBox.Instance;

                       foreach (XmlNode n in list)
                       {
                           if (n.NodeType == XmlNodeType.Comment)
                               continue;
                           SyncEntity sync = new SyncEntity(new XmlTable(n));
                           if (sync.SyncType == SyncType.Remove)
                           {
                               if (RemoveItem(sync.EntityName))
                               {
                                   this._DataCache.RemoveSyncSource(sync.ConnectionKey, sync.EntityName);
                                   this._SyncBag.RemoveItem(sync.EntityName);
                                   hasChange = true;
                               }
                           }
                           else
                           {
                               if (!this._DataCache.SyncSourceExists(sync))
                               {
                                   AddItem(sync);//, dbCopy, bagCopy);
                                   hasChange = true;
                               }

                           }
                       }
                       if (hasChange)
                       {
                           lock (ThreadLock)
                           {
                               //_DataCache.Reload(dbCopy);

                               //Reload(bagCopy);

                               _DataCache.Start(IntervalSeconds);
                           }
                       }
                   }
                   else
                   {

                       if (_reloadOnChange)
                       {
                           Clear(true);
                       }
                       foreach (XmlNode n in list)
                       {
                           if (n.NodeType == XmlNodeType.Comment)
                               continue;

                           AddItem(new XmlTable(n), false);
                       }
                   }
               }
           }
           catch (Exception ex)
           {
               CacheLogger.Logger.LogAction(CacheAction.SyncTable, CacheActionState.Error, string.Format("LoadSyncTables error: {0}", ex.Message));

               OnError("LoadSyncTables error " + ex.Message);
           }
           finally
           {
               //if(dbCopy!=null)
               //{
               //    dbCopy.Dispose();
               //    dbCopy = null;
               //}
               //if (bagCopy != null)
               //{
               //    bagCopy.Dispose();
               //    bagCopy = null;
               //}

               //Release the lock
               Interlocked.Exchange(ref synchronized, 0);

           }
       }
  */


        internal void AddItem(SyncEntity entity, SyncDbCache dbCopy, SyncBagStream bagCopy) //where T : IEntityItem
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("AddItem.SyncEntity copy");
                }

                entity.ValidateSyncEntity();

                SyncTableStream<EntityStream> item = new SyncTableStream<EntityStream>(entity, true);

                while (!item.IsReady)
                {
                    if (item.IsTimeout)
                    {
                        throw new TimeoutException("SyncTableStream timeout error: " + entity.EntityName);
                    }
                    Thread.Sleep(100);
                }
                item.Validate();

                dbCopy.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

                bagCopy.Set(item);
            }
            catch (Exception ex)
            {
                string entityName = "";
                if (!(ex is ArgumentNullException))
                {
                    entityName = entity.EntityName;
                }
                CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, string.Format("AddItem.SyncEntity copy {0} error: {1}" ,entityName, ex.Message));
            }

        }

        internal void AddItem<T>(SyncTableStream<T> item, SyncDbCache dbCopy, SyncBagStream bagCopy) //where T : IEntityItem
        {
            try
            {
                if (item == null)
                {
                    throw new ArgumentNullException("AddSyncTable<>.SyncTableStream");
                }

                item.Validate();

                dbCopy.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

                bagCopy.Set(item);
            }
            catch (Exception ex)
            {
                string entityName = "";
                if (item.Info!=null && !(ex is ArgumentNullException))
                {
                    entityName = item.Info.ItemName;
                }
                CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, string.Format("AddItem<>.SyncEntity {0} error: {1}", entityName, ex.Message));
            }
        }
#endif

        internal override CacheState AddItem(SyncEntity entity) //where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            SyncTableStream<EntityStream> item = new SyncTableStream<EntityStream>(entity, true);
            while (!item.IsReady)
            {
                if (item.IsTimeout)
                {
                    throw new Exception("SyncTableStream timeout error: " + entity.EntityName);
                }
                Thread.Sleep(100);
            }
            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

            return _SyncBag.Set(item);

        }
    #endregion

    #region members

        const string DefaultCacheName = "SyncCache";
        
    #endregion

    #region ctor

        /// <summary>
        /// Initilaize new instance of <see cref="SyncCacheStream"/>
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="isWeb"></param>
        public SyncCacheStream(string cacheName, bool isWeb = false)
            : base(cacheName,isWeb)
        {
            m_Perform = new CachePerformanceCounter(this, CacheAgentType.SyncCache, CacheName);
            _SyncBag = new SyncBagStream(this);
        }

    #endregion

    #region override

      
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_SyncBag != null)
            {
                _SyncBag.Clear();
                _SyncBag = null;
            }
            base.Dispose(disposing);
        }

    #endregion

    #region ISyncBag implementation

        private SyncBagStream _SyncBag;


        /// <summary>
        /// Returns the number of elements in the sync cache.
        /// </summary>
        public int CacheCount
        {
            get { return _SyncBag.Count; }
        }

       
        /// <summary>
        /// Get Data Cache connection Keys
        /// </summary>
        /// <returns></returns>
        public ICollection<string> DataCacheConnections()
        {
            return _DataCache.GetKeys();
        }

        /// <summary>
        /// Get Cache Keys
        /// </summary>
        /// <returns></returns>
        public ICollection<string> CacheKeys()
        {
            return _SyncBag.GetKeys(); 
        }

        /// <summary>
        /// Get all <see cref="ISyncTable"/> items in cache. 
        /// </summary>
        /// <returns></returns>
        public ICollection<ISyncTableStream> CacheValues()
        {
           return _SyncBag.GetItems();
        }

        /*
        /// <summary>
        /// Get the count of all items in all cref="ISyncTable"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public ICollection GetAllSyncValues()
        {
   
            ISyncTableStream[] Col = _SyncBag.GetItems();

            if (Col == null)
                return null;
            List<object> list = new List<object>();
            foreach (ISyncTableStream syncitem in Col)
            {
                foreach (object o in syncitem.Values)
                {
                    list.Add(o);
                }
            }
            return list;
        }
        */

        /// <summary>
        /// Get the size of all items in all cref="ISyncTable"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public long GetAllSyncSize()
        {
            long size = 0;
            ISyncTableStream[] Col = _SyncBag.GetItems();

            foreach (ISyncTableStream syncitem in Col)
            {
                size += syncitem.Size;
            }
          
            return size;
        }

        /// <summary>
        /// Get the count of all items in all cref="ISyncTable"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public int GetAllSyncCount()
        {
            int count = 0;
           
            ISyncTableStream[] Col = _SyncBag.GetItems();
            if (Col == null)
                return 0;
            foreach (ISyncTable syncitem in Col)
            {
                count += syncitem.Count;
            }
            return count;
        }

        /// <summary>
        /// Get if cache contains spesific item by <see cref="ComplexKey"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Contains(ComplexKey info)
        {
            return _SyncBag.Contains(info);
        }

        /// <summary>
        /// Get if cache contains spesific item by keys
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool Contains(string name, string[] keys)
        {
            return Contains(ComplexArgs.Get(name, keys));
        }


        /// <summary>
        /// Refresh <see cref="ISyncTable"/>
        /// </summary>
        /// <param name="syncName"></param>
        public override void Refresh(string syncName)
        {
            if (_SyncBag == null)
                return;
            _SyncBag.Refresh(syncName);
        }

     
        /// <summary>
        /// Remove specific item from sync cache.
        /// </summary>
        /// <param name="syncName"></param>
        /// <returns></returns>
        public override bool RemoveItem(string syncName)
        {
            if (_SyncBag == null)
                return false;
           return _SyncBag.RemoveItem(syncName);
        }

        public bool Exists(string syncName)
        {
            if (_SyncBag == null)
                return false;
            return _SyncBag.Exists(syncName);
        }

        /// <summary>
        /// Refrech cache
        /// </summary>
        public void RefreshAll()
        {
            if (_SyncBag == null)
                return;

            _SyncBag.RefreshAllAsync();
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        /// <param name="clearDataCache"></param>
        public override void Clear(bool clearDataCache)
        {
            if (_SyncBag == null)
                return;

            _SyncBag.Clear();

            if (clearDataCache)
            {
                if (_DataCache == null)
                    return;
                _DataCache.ClearSafe();
            }
        }
    #endregion

    #region Get/Set Items

        ///// <summary>
        ///// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public SyncTableStream<T> GetItem<T>(ComplexKey info) where T : IEntityItem
        //{
        //    return _SyncBag.GetItem<T>(info);
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncTableStream<T> GetItem<T>(string name) where T : IEntityItem
        {
            return _SyncBag.GetItem<T>(name);
        }

        internal bool TryGetItem<T>(string name, out SyncTableStream<T> item) where T : IEntityItem
        {
            return (_SyncBag.TryGetItem<T>(name, out item));
        }
        internal bool TryGetItem(string name, out ISyncTableStream item)
        {
            return (_SyncBag.TryGetItem(name, out item));
        }

        /// <summary>
        ///// Get spesific value from cache using item name and keys, if item not found return null
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="name"></param>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public SyncTableStream<T> GetItem<T>(string name, string[] keys) where T : IEntityItem
        //{
        //    return GetItem<T>(ComplexKey.Get(name, keys));
        //}
        ///// <summary>
        ///// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public ISyncTableStream GetItem(ComplexKey info)
        //{
        //    return _SyncBag.GetItem(info);
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISyncTableStream GetItem(string name)
        {
            return _SyncBag.GetItem(name);
        }
        internal string[] GetNames()
        {
            return _SyncBag.GetKeys();
        }
       

        internal EntityStream GetEntityStreamInternal(MessageStream message)
        {
            //var keyinfo = ComplexArgs.Parse(message.Key);
            //var syncItem = GetItem(keyinfo.Name);

            var syncItem = GetItem(message.Key);
            if (syncItem == null)
                return null;
            return syncItem.GetEntityStream(message.Detail);
        }
        internal CacheItemReport GetItemsReport(MessageStream message)
        {
            var syncItem = GetItem(message.Key);
            if (syncItem == null)
                return null;
            return new CacheItemReport(syncItem);
        
        }

        internal GenericKeyValue GetEntityItems(MessageStream message)
        {
            var syncItem = GetItem(message.Key);
            if (syncItem == null)
                return null;
            return syncItem.GetEntityItems(false);
        }

        internal int GetEntityItemsCount(MessageStream message)
        {
            var syncItem = GetItem(message.Key);
            if (syncItem == null)
                return 0;
            return syncItem.GetEntityItemsCount();
        }

        internal string[] GetEntityKeys(MessageStream message)
        {
            var syncItem = GetItem(message.Key);
            if (syncItem == null)
                return null;
            return syncItem.GetEntityKeys().ToArray();
        }


    #endregion

    #region Get/Set Stream

        /// <summary>
        /// Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public object Get(ComplexKey info)
        {
            ISyncTableStream syncitem;
            if (TryGetItem(info.Prefix, out syncitem))
            {
                EntityStream es;
                if (syncitem.TryGetEntity(info.Suffix, out es))
                {
                    return es.DecodeBody();
                }
            }

            return null;

            //var syncitem = GetItem(info.Prefix);
            //if (syncitem != null)
            //{
            //    return syncitem.GetItemStream(info.Suffix);
            //}
            //return null;
        }

        
        /// <summary>
        /// Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public T Get<T>(ComplexKey info) where T : IEntityItem
        {
            SyncTableStream<T> syncitem;
            if (TryGetItem<T>(info.Prefix, out syncitem))
            {
                EntityStream es;
                if(syncitem.TryGetEntity(info.Suffix, out es))
                {
                    return es.DecodeBody<T>();
                }
            }

            return default(T);

            //SyncTableStream<T> syncitem = GetItem<T>(info.Prefix);
            //if (syncitem != null)
            //{
            //    return syncitem.GetItemStream(info.Suffix);
            //}
            //return null;

        }
        /// <summary>
        /// Get copy of spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T Get<T>(string name, string[] keys) where T : IEntityItem
        {
            return Get<T>(ComplexArgs.Get(name, keys));
        }

        ///// <summary>
        /////  Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public NetStream Get(ComplexKey info)
        //{
        //    ISyncTableStream syncitem = GetItem(info.Name);
        //    if (syncitem != null)
        //    {
        //        return syncitem.GetItemStream(info.Suffix);
        //    }
        //    return null;
        //}


        internal NetStream GetBodyStream(MessageStream message)
        {

            NetStream b = null;

            if (message == null)
                throw new ArgumentNullException("CacheMessage");
            message.ValiddateInfo();

            if (TryGetBodyStream(message.Key, message.Detail, out b))
            {
                return b;
            }

            //b = Get(message.Key,message.Detail);

            if (b == null)
            {
                CacheLogger.Debug("sync get stream not found : " + message.KeyInfo());
            }

            return b;
        }

        /// <summary>
        ///  Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetBodyStream(string name, string key, out NetStream value)
        {
            ISyncTableStream syncitem;
            if (TryGetItem(name, out syncitem))
            {
                EntityStream es;
                if (syncitem.TryGetEntity(key, out es))
                {
                    value= es.GetCopy();
                    return true;
                }
            }
            value = null;
            return false;

            //ISyncTableStream syncitem = GetItem(name);
            //if (syncitem != null)
            //{
            //    return syncitem.GetItemStream(key);
            //}
            //return null;
        }

        public bool TryGetBodyStream(ComplexKey ck, out NetStream value)
        {
            ISyncTableStream syncitem;
            if (TryGetItem(ck.Prefix, out syncitem))
            {
                EntityStream es;
                if (syncitem.TryGetEntity(ck.Suffix, out es))
                {
                    value = es.GetCopy();
                    return true;
                }
            }
            value = null;
            return false;

            //ISyncTableStream syncitem = GetItem(name);
            //if (syncitem != null)
            //{
            //    return syncitem.GetItemStream(key);
            //}
            //return null;
        }


        /* CHEK THIS
                /// <summary>
                ///  Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
                /// </summary>
                /// <param name="name"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public object Get(string name, string key)
                {
                    ISyncTableStream syncitem;
                    if (TryGetItem(name, out syncitem))
                    {
                        EntityStream es;
                        if (syncitem.TryGetEntity(key, out es))
                        {
                            return es.DecodeBody();
                        }
                    }

                    return null;

                    //ISyncTableStream syncitem = GetItem(name);
                    //if (syncitem != null)
                    //{
                    //    return syncitem.GetItemStream(key);
                    //}
                    //return null;
                }
        
        /// <summary>
        ///  Get copy of spesific value from cache, using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal NetStream GetValue(MessageStream message)
        {

            NetStream b = null;

            if (message == null)
                throw new ArgumentNullException("CacheMessage");
            message.ValiddateInfo();

            if(TryGetBodyStream(message.Key, message.Detail, out b))
            {
                return b;
            }

            //b = Get(message.Key,message.Detail);

            if (b == null)
            {
                CacheLogger.Debug("sync get stream not found : " + message.KeyInfo());
            }

            return b;
        }
        */
        ///// <summary>
        /////  Get copy of spesific value from cache, using <see cref="MessageStream"/>, if item not found return null
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public NetStream Get(MessageStream message)//-GenericRecord
        //{
        //    NetStream b = GetInternal(message);
        //    if (b == null)
        //    {
        //        return null;
        //    }
        //    return b;
        //}


        /// <summary>
        /// Get copy of spesific value from cache as entity, using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream GetAs(MessageStream message)
        {
            NetStream b = GetBodyStream(message);

            if (b != null)
            {
                //NetStream copy = b.Copy();

                return b;

            }
            return null;
        }

        /// <summary>
        /// Get copy of spesific value from cache as entity, using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream GetEntity(MessageStream message)
        {
            NetStream b = GetBodyStream(message);

            if (b != null)
            {
                //NetStream copy = b.Copy();
                SerializeTools.ChangeContextType(b, SerialContextType.GenericEntityAsIEntityType);
                return b;
            }
            return null;
        }

        /// <summary>
        /// Get spesific value as dictionary stream from cache using <see cref="MessageStream"/> , if item not found return null. 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream GetRecord(MessageStream message)
        {
            NetStream b = GetBodyStream(message);
            if (b != null)
            {
                //NetStream copy = b.Copy();
                SerializeTools.ChangeContextType(b, SerialContextType.IEntityDictionaryType);
                return b;
            }

            return null;
        }
        /// <summary>
        /// Get spesific value as dictionary stream from cache using name and keys arguments , if item not found return null. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public NetStream GetRecord(string name, string[] keys)
        {
            return GetRecord(ComplexArgs.Get(name, keys));
        }
        /// <summary>
        /// Get spesific value as dictionary stream from cache using <see cref="ComplexKey"/> , if item not found return null. 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream GetRecord(ComplexKey info)
        {
            NetStream b;
            if (TryGetBodyStream(info, out b))
            {
                SerializeTools.ChangeContextType(b, SerialContextType.dictionaryEntityType);//.GenericEntityAsIDictionaryType);
                return b;
            }
            return null;

            //NetStream b = Get(info.Prefix, info.Suffix);
            //if (b != null)
            //{

            //    SerializeTools.ChangeContextType(b, SerialContextType.GenericEntityAsIDictionaryType);
            //    return b;
            //}
            //return null;
        }

        /// <summary>
        /// Get spesific value as dictionary stream from cache using <see cref="ComplexKey"/> , if item not found return null. 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public IDictionary<string,object> GetGenericRecord(ComplexKey info)
        {
            NetStream value;

            if (TryGetBodyStream(info, out value))
            {
                //SerializeTools.ChangeContextType(value, SerialContextType.dictionaryEntityType);// GenericEntityAsIDictionaryType);
                using (var streamer = new BinaryStreamer(value))
                {
                    return streamer.ReadDictionaryEntity(true,true);//.ReadGenericEntityAsDictionary(true);
                }
            }
            return null;
        }

        internal object Get(MessageStream message)
        {
            return Get(ComplexKey.Get(message.Key, message.Detail), message.Args[KnownArgs.Column]);
        }

        public object Get(ComplexKey info, string field)
        {
            NetStream ns;

            if (TryGetBodyStream(info, out ns))
            {
                //SerializeTools.ChangeContextType(ns, SerialContextType.dictionaryEntityType);// GenericEntityAsIDictionaryType);
                using (var streamer = new BinaryStreamer(ns))
                {
                    var dic = streamer.ReadDictionaryEntity(true,true);
                    object value;
                    if(dic.TryGetValue(field, out value))
                    {
                        return value;
                    }
                }
            }
            return null;
        }

        /*
                /// <summary>
                /// Get spesific value from cache using item name and keys, if item not found return null
                /// </summary>
                /// <param name="name"></param>
                /// <param name="keys"></param>
                /// <returns></returns>
                public NetStream Get(string name, string[] keys)
                {
                    //var key = ComplexArgs.Get(name, keys);
                    //var b = Get(ComplexArgs.Get(name, keys));
                    NetStream value;
                    if (TryGetValueStream(name, KeySet.Join(keys), out value))
                    {
                        return value;
                    }
                    return null;

                    //var b=Get(name, KeySet.Join(keys));
                    //if (b != null)
                    //{
                    //    return b;
                    //}

                    //return null;
                }
                */
    #endregion

    #region Get/Set Values

        /// <summary>
        ///  Get copy of spesific entity from cache using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public EntityStream GetEntityStream(MessageStream message)
        {
            if (message == null)
                throw new ArgumentNullException("CacheMessage");

            message.ValiddateInfo();

            //ComplexKey info = ComplexArgs.Parse(message.Key);
            //if (info == null || info.IsEmpty)
            //    throw new ArgumentException("ComplexKey is null or empty");
            
            return GetEntityStream(message.Key, message.Detail);
        }
        ///// <summary>
        ///// Get copy of spesific entity from cache using <see cref="ComplexKey"/>, if item not found return null
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public EntityStream GetEntityStream(ComplexKey info)
        //{
        //    ISyncTableStream syncitem = GetItem(info.Name);
        //    if (syncitem != null)
        //    {
        //        return syncitem.GetEntityStream(info);
        //    }
        //    return null;
        //}

        /// <summary>
        /// Get copy of spesific entity from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public EntityStream GetEntityStream(string name, string key)
        {
            ISyncTableStream syncitem = GetItem(name);
            if (syncitem != null)
            {
                return syncitem.GetEntityStream(key);
            }
            return null;
        }
    #endregion

    #region Add items

        /// <summary>
        /// Add Item to Sync cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="sourceType"></param>
        /// <param name="entityKeys"></param>
        /// <param name="columns"></param>
        /// <param name="interval"></param>
        /// <param name="syncType"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public override CacheState AddItem<T>(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] entityKeys, string columns, TimeSpan interval, SyncType syncType, bool enableNoLock=false, int commandTimeout=0)
        {
            if (connectionKey == null)
                throw new ArgumentNullException("AddItem.connectionKey");
            if (entityName == null)
                throw new ArgumentNullException("AddItem.entityName");
            if (mappingName == null)
                throw new ArgumentNullException("AddItem.mappingName");
            if (sourceName == null)
                throw new ArgumentNullException("AddItem.sourceName");
            if (entityKeys == null)
                throw new ArgumentNullException("AddItem.entityKeys");

            try
            {
                SyncTimer timer = new SyncTimer(interval, syncType);


                SyncTableStream<T> item = new SyncTableStream<T>(connectionKey, entityName, mappingName, sourceName, sourceType, entityKeys, columns, timer,enableNoLock, commandTimeout,true);

                while (!item.IsReady)
                {
                    if (item.IsTimeout)
                    {
                        throw new Exception("SyncTableStream timeout error: " + entityName);
                    }
                    Thread.Sleep(100);
                }
              

                item.Validate();

                _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);


               return _SyncBag.Set(entityName, item as ISyncTableStream);

            }
            catch (Exception ex)
            {
                OnError("AddItem error " + ex.Message);
                return CacheState.AddItemFailed;
            }

        }



        /// <summary>
        /// Add Item to Sync cache
        /// </summary>
        /// <param name="item"></param>
        public CacheState AddItem<T>(SyncTableStream<T> item) //where T : IEntityItem
        {
       
            if (item == null)
            {
                throw new ArgumentNullException("AddSyncTable.SyncTableStream");
            }

            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

            return _SyncBag.Set(item);//[item.Info.ItemName] = item as ISyncTableStream;
           
        }


        /// <summary>
        /// Add Item to Sync cache
        /// </summary>
        /// <param name="message"></param>
        public CacheState AddItem(CacheMessage message) //where T : IEntityItem
        {
            if (message == null)
            {
                throw new ArgumentNullException("AddItem.CacheMessage");
            }
            SyncTableStream<EntityStream> item = new SyncTableStream<EntityStream>(message.GetArgs(), true);
            while (!item.IsReady)
            {
                if (item.IsTimeout)
                {
                    throw new Exception("SyncTableStream timeout error: " + message.Key);
                }
                Thread.Sleep(100);
            }

            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

            return _SyncBag.Set(item);//[item.Info.ItemName] = item as ISyncTableStream;
        }




    #endregion

    #region size exchange
        /*
        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeValidate(long newSize)
        {
            return CacheState.Ok;
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="newSize"></param>
        /// <param name="currentCount"></param>
        /// <param name="newCount"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        internal protected virtual void SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            //return CacheState.Ok;
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected virtual void SizeExchage(ISyncTableStream oldItem, ISyncTableStream newItem)
        {
            //return CacheState.Ok;
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
            
        }
        */
    #endregion

        /// <summary>
        /// Get properties of specified table name in data cache..
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public CacheItemProperties GetItemProperties(string entityName)
        {
            ISyncTableStream syncitem = GetItem(entityName);
            if (syncitem == null)
                return null;

            var dtprop = new TableProperties() { RecordCount = syncitem.Count, Size = syncitem.Size, ColumnCount = 0 };
            if (syncitem.SyncSource != null)
                return new CacheItemProperties(dtprop, syncitem.SyncSource);
            else
                return new CacheItemProperties(dtprop, entityName);
        }
    }
#endif
}
