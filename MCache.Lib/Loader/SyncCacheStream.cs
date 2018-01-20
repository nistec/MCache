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

namespace Nistec.Caching.Loader
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
    public class SyncStream : SyncBase, IDisposable//,ISyncStream
    {
        internal void Reload(SyncBagStream copy)
        {
            _SyncBag.Reload(copy);
        }

        SyncBagStream BagCopy()
        {
            return new SyncBagStream(CacheName,this);
        }

        int synchronized;

        internal override void LoadSyncItems(XmlNode node, bool EnableAsyncTask)
        {
            if (node == null)
                return;

            Console.WriteLine("Debuger-SyncCacheStream.LoadSyncItems start");

            bool hasChange = false;
            try
            {
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    XmlNodeList list = node.ChildNodes;
                    if (list == null)
                    {
                        CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, "LoadSyncItems is empty");
                        return;
                    }

                    var newSyncEntityItems = SyncEntity.GetItems(list);

                    if (newSyncEntityItems == null || newSyncEntityItems.Length == 0)
                    {
                        throw new Exception("Can not LoadSyncItems, SyncEntity Items not found");
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
                CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, string.Format("LoadSyncItems error: {0}", ex.Message));

                OnError("LoadSyncItems error " + ex.Message);
            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);

            }
        }

        
   
        internal void AddItem(SyncEntity entity, SyncDbCache dbCopy, SyncBagStream bagCopy) //where T : IEntityItem
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("AddItem.SyncEntity copy");
                }

                entity.ValidateSyncEntity();

                SyncItemStream<EntityStream> item = new SyncItemStream<EntityStream>(entity, true);

                while (!item.IsReady)
                {
                    if (item.IsTimeout)
                    {
                        throw new TimeoutException("SyncItemStream timeout error: " + entity.EntityName);
                    }
                    Thread.Sleep(100);
                }
                item.Validate();

                dbCopy.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

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

        internal void AddItem<T>(SyncItemStream<T> item, SyncDbCache dbCopy, SyncBagStream bagCopy) //where T : IEntityItem
        {
            try
            {
                if (item == null)
                {
                    throw new ArgumentNullException("AddSyncItem<>.SyncItemStream");
                }

                item.Validate();

                dbCopy.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

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

         internal override void AddItem(SyncEntity entity) //where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            SyncItemStream<EntityStream> item = new SyncItemStream<EntityStream>(entity, true);
            while (!item.IsReady)
            {
                if (item.IsTimeout)
                {
                    throw new Exception("SyncItemStream timeout error: " + entity.EntityName);
                }
                Thread.Sleep(100);
            }
            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

            _SyncBag.Set(item);

        }

        #region members

        const string DefaultCacheName = "SyncCache";
        
        #endregion

        #region ctor

        /// <summary>
        /// Initilaize new instance of <see cref="SyncCacheStream"/>
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="isWeb"></param>
        public SyncStream(string cacheName, bool isWeb = false)
            : base(cacheName,isWeb)
        {
            _SyncBag = new SyncBagStream(cacheName,this);
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
        /// Get all <see cref="ISyncItem"/> items in cache. 
        /// </summary>
        /// <returns></returns>
        public ICollection<ISyncItemStream> CacheValues()
        {
           return _SyncBag.GetItems();
        }

        /*
        /// <summary>
        /// Get the count of all items in all cref="ISyncItem"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public ICollection GetAllSyncValues()
        {
   
            ISyncItemStream[] Col = _SyncBag.GetItems();

            if (Col == null)
                return null;
            List<object> list = new List<object>();
            foreach (ISyncItemStream syncitem in Col)
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
        /// Get the size of all items in all cref="ISyncItem"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public long GetAllSyncSize()
        {
            long size = 0;
            ISyncItemStream[] Col = _SyncBag.GetItems();

            foreach (ISyncItemStream syncitem in Col)
            {
                size += syncitem.Size;
            }
          
            return size;
        }

        /// <summary>
        /// Get the count of all items in all cref="ISyncItem"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public int GetAllSyncCount()
        {
            int count = 0;
           
            ISyncItemStream[] Col = _SyncBag.GetItems();
            if (Col == null)
                return 0;
            foreach (ISyncItem syncitem in Col)
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
            return Contains(ComplexKey.Get(name, keys));
        }


        /// <summary>
        /// Refresh <see cref="ISyncItem"/>
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
        //public SyncItemStream<T> GetItem<T>(ComplexKey info) where T : IEntityItem
        //{
        //    return _SyncBag.GetItem<T>(info);
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncItemStream<T> GetItem<T>(string name) where T : IEntityItem
        {
            return _SyncBag.GetItem<T>(name);
        }

        /// <summary>
        ///// Get spesific value from cache using item name and keys, if item not found return null
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="name"></param>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public SyncItemStream<T> GetItem<T>(string name, string[] keys) where T : IEntityItem
        //{
        //    return GetItem<T>(ComplexKey.Get(name, keys));
        //}
        ///// <summary>
        ///// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public ISyncItemStream GetItem(ComplexKey info)
        //{
        //    return _SyncBag.GetItem(info);
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISyncItemStream GetItem(string name)
        {
            return _SyncBag.GetItem(name);
        }
        internal string[] GetNames()
        {
            return _SyncBag.GetKeys();
        }
       

        internal EntityStream GetEntityStreamInternal(MessageStream message)
        {
            var keyinfo = ComplexKey.Parse(message.Key);
            var syncItem = GetItem(keyinfo.ItemName);
            if (syncItem == null)
                return null;
            return syncItem.GetEntityStream(keyinfo);
        }
        internal CacheItemReport GetItemsReportInternal(MessageStream message)
        {
            var syncItem = GetItem(message.Key);
            if (syncItem == null)
                return null;
            return new CacheItemReport(syncItem);
        
        }

        internal GenericKeyValue GetEntityItemsInternal(MessageStream message)
        {
            var syncItem = GetItem(message.Key);
            if (syncItem == null)
                return null;
            return syncItem.GetEntityItems(false);
        }

        internal int GetEntityItemsCountInternal(MessageStream message)
        {
            var syncItem = GetItem(message.Key);
            if (syncItem == null)
                return 0;
            return syncItem.GetEntityItemsCount();
        }

        internal string[] GetEntityKeysInternal(MessageStream message)
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
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream Get<T>(ComplexKey info) where T : IEntityItem
        {
            SyncItemStream<T> syncitem = GetItem<T>(info.ItemName);
            if (syncitem != null)
            {
                return syncitem.GetItemStream(info.ItemKey);
            }
            return null;
        }
        /// <summary>
        /// Get copy of spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public NetStream Get<T>(string name, string[] keys) where T : IEntityItem
        {
            return Get<T>(ComplexKey.Get(name, keys));
        }

        /// <summary>
        ///  Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream Get(ComplexKey info)
        {
            ISyncItemStream syncitem = GetItem(info.ItemName);
            if (syncitem != null)
            {
                return syncitem.GetItemStream(info);
            }
            return null;
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
            ComplexKey info = ComplexKey.Parse(message.Key);
            if (info == null || info.IsEmpty)
                throw new ArgumentException("ComplexKey is null or empty");

            b = Get(info);

            
            if (b == null)
            {
                CacheLogger.Debug("sync get stream not found : " + info.ToString());
            }

            return b;
        }

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
            NetStream b = GetValue(message);

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
            NetStream b = GetValue(message);

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
            NetStream b = GetValue(message);
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
            return GetRecord(ComplexKey.Get(name, keys));
        }
        /// <summary>
        /// Get spesific value as dictionary stream from cache using <see cref="ComplexKey"/> , if item not found return null. 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream GetRecord(ComplexKey info)
        {
            NetStream b = Get(info);
            if (b != null)
            {
               
                SerializeTools.ChangeContextType(b, SerialContextType.GenericEntityAsIDictionaryType);
                return b;
            }
            return null;
        }

        /// <summary>
        /// Get spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public NetStream Get(string name, string[] keys)
        {
            var key = ComplexKey.Get(name, keys);
            var b = Get(ComplexKey.Get(name, keys));
            if (b != null)
            {
                return b;
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
            ComplexKey info = ComplexKey.Parse(message.Key);
            if (info == null || info.IsEmpty)
                throw new ArgumentException("ComplexKey is null or empty");

            return GetEntityStream(info);
        }
        /// <summary>
        /// Get copy of spesific entity from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public EntityStream GetEntityStream(ComplexKey info)
        {
            ISyncItemStream syncitem = GetItem(info.ItemName);
            if (syncitem != null)
            {
                return syncitem.GetEntityStream(info);
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
        public override void AddItem<T>(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] entityKeys, string columns, TimeSpan interval, SyncType syncType, bool enableNoLock=false, int commandTimeout=0)
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


                SyncItemStream<T> item = new SyncItemStream<T>(connectionKey, entityName, mappingName, sourceName, sourceType, entityKeys, columns, timer,enableNoLock, commandTimeout,true);

                while (!item.IsReady)
                {
                    if (item.IsTimeout)
                    {
                        throw new Exception("SyncItemStream timeout error: " + entityName);
                    }
                    Thread.Sleep(100);
                }
              

                item.Validate();

                _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);


                _SyncBag.Set(entityName, item as ISyncItemStream);

            }
            catch (Exception ex)
            {
                OnError("AddItem error " + ex.Message);
            }

        }



        /// <summary>
        /// Add Item to Sync cache
        /// </summary>
        /// <param name="item"></param>
        public void AddItem<T>(SyncItemStream<T> item) //where T : IEntityItem
        {
       
            if (item == null)
            {
                throw new ArgumentNullException("AddSyncItem.SyncItemStream");
            }

            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

            _SyncBag.Set(item);//[item.Info.ItemName] = item as ISyncItemStream;

            
        }
        

        /// <summary>
        /// Add Item to Sync cache
        /// </summary>
        /// <param name="message"></param>
        public void AddItem(CacheMessage message) //where T : IEntityItem
        {
            if (message == null)
            {
                throw new ArgumentNullException("AddItem.CacheMessage");
            }
            SyncItemStream<EntityStream> item = new SyncItemStream<EntityStream>(message.GetArgs(),true);
            while (!item.IsReady)
            {
                if (item.IsTimeout)
                {
                    throw new Exception("SyncItemStream timeout error: " + message.Key);
                }
                Thread.Sleep(100);
            }

            item.Validate();

            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

            _SyncBag.Set(item);//[item.Info.ItemName] = item as ISyncItemStream;

           
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
        internal protected virtual CacheState SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            return CacheState.Ok;
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeExchage(ISyncItemStream oldItem, ISyncItemStream newItem)
        {
            return CacheState.Ok;
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
            
        }

        #endregion

  
    }
}
