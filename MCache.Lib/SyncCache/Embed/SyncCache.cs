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
using System.Data;
using System.Collections;
using System.Xml;
using Nistec.Caching;
using Nistec.Data;
using Nistec.Caching.Data;
using Nistec.Caching.Remote;
using Nistec.Generic;
using Nistec.Data.Entities;
using Nistec.Data.Entities.Cache;
using Nistec.Xml;
using Nistec.IO;
using Nistec.Channels;
using Nistec.Runtime;

namespace Nistec.Caching.Sync.Embed
{
    /// <summary>
    /// Represents Synchronize db cache for embeded cache.
    /// which manages them using <see cref="SyncDbCache"/>.  
    /// The synchronization properties are configurable using "SyncFile",
    /// that uses <see cref="SysFileWatcher"/> which Listens to the file system change notifications and raises events when a
    /// file is changed.
    /// The synchronization process are using in <see cref="SyncBag"/> to ensure that each item will stay synchronized
    /// in run time without any interruption in process.
    /// When problem was occured during the sync process , the item, will stay as the original item.    
    /// </summary>
    public class SyncCache : SyncCacheBase, IDisposable
    {

        internal void Reload(SyncBag copy)
        {
            _SyncBag.Reload(copy);
        }

        SyncBag BagCopy()
        {
            return new SyncBag(CacheName);
        }

       
        internal override void LoadSyncItems(XmlNode node, bool copy)
        {
            if (node == null)
                return;
            try
            {
                XmlNodeList list = node.ChildNodes;
                if (list == null)
                    return;

                if (copy)
                {

                    var dbCopy = DataCacheCopy();
                    var bagCopy = BagCopy();

                    foreach (XmlNode n in list)
                    {
                        if (n.NodeType == XmlNodeType.Comment)
                            continue;

                        SyncEntity sync = new SyncEntity(new XmlTable(n));

                        AddItem(sync, dbCopy, bagCopy);
                    }

                    _DataCache.Reload(dbCopy);

                    Reload(bagCopy);

                    _DataCache.Start(IntervalSeconds);

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

                        AddItem(new XmlTable(n),false);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError("LoadSyncItems error " + ex.Message);
            }

        }

        /// <summary>
        /// Create new instance of <see cref="ISyncItem"/>
        /// </summary>
        /// <returns></returns>
        internal ISyncItem CreateSyncEntityInstance(SyncEntity entity)
        {
            Type type = entity.GetEntityType();
            Type d1 = typeof(SyncItem<>);
            Type[] typeArgs = { type };
            Type constructed = d1.MakeGenericType(typeArgs);
            object o = Activator.CreateInstance(constructed);

            ((ISyncCacheItem)o).Set(entity, false);

            return (ISyncItem)o;
        }

        internal void AddItem(SyncEntity entity, SyncDbCache dbCopy, SyncBag bagCopy) //where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            var item = entity.CreateInstance();
          
            item.Validate();

            dbCopy.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

            bagCopy.Set(item);

        }

        internal override void AddItem(SyncEntity entity) //where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            var item= entity.CreateInstance();
            
            item.Validate();
            
            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource,IntervalSeconds, true);

            _SyncBag.Set(item);

        }


        #region members

        const string DefaultCacheName = "SyncCache";
        
       
        #endregion

        #region ctor

        /// <summary>
        /// Initilaize new instance of <see cref="SyncCache"/>
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="isWeb"></param>
        public SyncCache(string cacheName, bool isWeb)
            : base(cacheName,isWeb)
        {
            _SyncBag = new SyncBag(cacheName);
        }
        /// <summary>
        /// Satart cache.
        /// </summary>
        public void Start()
        {
            base.Start(false, false);

            LoadSyncConfig();

        }

        #endregion

        #region override

      
        /// <summary>
        /// Dispose instance.
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

        private SyncBag _SyncBag;

        /// <summary>
        ///  Returns the number of elements in the sync cache.
        /// </summary>
        public int CacheCount
        {
            get { return _SyncBag.Count; }
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
        public ICollection<ISyncItem> CacheValues()
        {
            return _SyncBag.GetItems(); 
        }
        
         
        /// <summary>
        /// Get the count of all items in all cref="ISyncItem"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public ICollection GetAllSyncValues()
        {
          
            ISyncItem[] Col = _SyncBag.GetItems();
            if (Col == null)
                return null;
            List<object> list = new List<object>();
            foreach (ISyncItem syncitem in Col)
            {
                foreach (object o in syncitem.Values)
                {
                    list.Add(o);
                }
            }
            return list;
        }

        /// <summary>
        /// Get the count of all items in all cref="ISyncItem"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public int GetAllSyncCount()
        {
            int count = 0;
           
            ISyncItem[] Col = _SyncBag.GetItems();
            if (Col == null)
                return 0;
            foreach (ISyncItem syncitem in Col)
            {
                count += syncitem.Count;
            }
            return count;
        }

        /// <summary>
        /// Get if cache contains spesific item by <see cref="CacheKeyInfo"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Contains(CacheKeyInfo info)
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
            return Contains(CacheKeyInfo.Get(name, keys));
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

        /// <summary>
        /// Refresh <see cref="ISyncItem"/>
        /// </summary>
        /// <param name="syncName"></param>
        public override void Refresh(string syncName)
        {
            CacheLogger.Debug("SyncCache Refresh : " + syncName);

            if (_SyncBag == null)
            {
                CacheLogger.Debug("SyncCache Refresh SyncBag is null " + syncName);
                return;
            }
            _SyncBag.Refresh(syncName);
        }

        /// <summary>
        /// Refrech cache
        /// </summary>
        public void Refresh()
        {
            if (_SyncBag == null)
                return;

            _SyncBag.Refresh();
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
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public SyncItem<T> GetItem<T>(CacheKeyInfo info) where T : IEntityItem
        {
            return _SyncBag.Get<T>(info);
        }

        /// <summary>
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncItem<T> GetItem<T>(string name) where T : IEntityItem
        {
            return _SyncBag.Get<T>(name);
        }

        /// <summary>
        /// Get spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public SyncItem<T> GetItem<T>(string name, string[] keys) where T : IEntityItem
        {
            return GetItem<T>(CacheKeyInfo.Get(name, keys));
        }
        /// <summary>
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ISyncItem GetItem(CacheKeyInfo info)
        {
            return _SyncBag.Get(info);
        }
        /// <summary>
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISyncItem GetItem(string name)
        {
            return _SyncBag.Get(name);
        }

        #endregion

        #region Get/Set Values

        /// <summary>
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public T Get<T>(CacheKeyInfo info) where T : IEntityItem
        {
            SyncItem<T> syncitem = GetItem<T>(info);
            if (syncitem != null)
            {
                return syncitem.Get(info.CacheKey);
            }
            return default(T);
        }
        /// <summary>
        /// Get spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T Get<T>(string name, string[] keys) where T : IEntityItem
        {
            return Get<T>(CacheKeyInfo.Get(name, keys));
        }

        /// <summary>
        ///  Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public object Get(CacheKeyInfo info)
        {
            ISyncItem syncitem = GetItem(info);
            if (syncitem != null)
            {
                return syncitem.GetItem(info);
            }
            return null;
        }

        /// <summary>
        /// Get spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public object Get(string name, string[] keys)
        {
            return Get(CacheKeyInfo.Get(name, keys));
        }

        /// <summary>
        ///  Get spesific value from cache as <see cref="IDictionary"/> using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public IDictionary GetRecord(CacheKeyInfo info)
        {
            ISyncItem syncitem = GetItem(info);
            if (syncitem != null)
            {
                return syncitem.GetRecord(info);
            }
            return null;
        }

        /// <summary>
        /// Get spesific value from cache as <see cref="IDictionary"/> using item name and keys, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IDictionary GetRecord(string name, string[] keys)
        {
            return GetRecord(CacheKeyInfo.Get(name, keys));
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
        /// <param name="interval"></param>
        /// <param name="syncType"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public override void AddItem<T>(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] entityKeys, TimeSpan interval, SyncType syncType, bool enableNoLock=false, int commandTimeout=0)
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
        
                SyncItem<T> item = new SyncItem<T>(connectionKey, entityName, mappingName, sourceName,sourceType,entityKeys, timer,enableNoLock, commandTimeout,false);

               
                item.Validate();

                _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

                _SyncBag.Set(item.Info, item as ISyncItem);

               
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
        public void AddItem<T>(SyncItem<T> item) //where T : IEntityItem
        {
            if (item == null)
            {
                throw new ArgumentNullException("AddSyncItem.SyncItem");
            }

            try
            {
                item.Validate();

                _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, IntervalSeconds, true);

                _SyncBag.Set(item.Info, item);

               
            }
            catch (Exception ex)
            {
                OnError("AddItem error " + ex.Message);
            }

        }

        #endregion
       
        #region Cache Stream

        /// <summary>
        ///  Get spesific value from cache using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream Get(MessageStream message)
        {
            CacheKeyInfo info = CacheKeyInfo.Parse(message.Key);
            if (info.IsEmpty)
            {
                return null;
            }
            ISyncItem syncitem = GetItem(info.ItemName);
            if (syncitem != null)
            {
                return syncitem.GetItemStream(info);
            }
            return null;
        }

        /// <summary>
        ///  Get spesific value from cache as <see cref="IDictionary"/> using <see cref="MessageStream"/>, if item not found return null
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream GetRecord(MessageStream message)
        {
            CacheKeyInfo info = CacheKeyInfo.Parse(message.Key);
            if (info.IsEmpty)
            {
                return null;
            }

            ISyncItem syncitem = GetItem(info.ItemName);
            if (syncitem != null)
            {
                return syncitem.GetRecordStream(info);
            }
            return null;
        }

        /// <summary>
        /// Get if cache contains spesific item by <see cref="MessageStream"/>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Contains(MessageStream message)
        {

            return _SyncBag.Contains(CacheKeyInfo.Parse(message.Key));
        }


        #endregion
    }
}
