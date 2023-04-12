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
using System.Linq;
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
using Nistec.Caching.Config;

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
            return new SyncBag(this);
        }

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

        /*      
               internal override void LoadSyncTables(XmlNode node)//, bool useCopy, bool enableLoader)
               {
                   if (node == null)
                       return;
                   try
                   {
                       XmlNodeList list = node.ChildNodes;
                       if (list == null)
                           return;

                       bool useCopy = true;

                       if (useCopy)
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
                       OnError("LoadSyncTables error " + ex.Message);
                   }

               }
       */
        /// <summary>
        /// Create new instance of <see cref="ISyncTable"/>
        /// </summary>
        /// <returns></returns>
        internal ISyncTable CreateSyncEntityInstance(SyncEntity entity)
        {
            Type type = entity.GetEntityType();
            Type d1 = typeof(SyncTable<>);
            Type[] typeArgs = { type };
            Type constructed = d1.MakeGenericType(typeArgs);
            object o = ActivatorUtil.CreateInstance(constructed);

            ((ISyncCacheItem)o).Set(entity,false);

            return (ISyncTable)o;
        }

        internal CacheState AddItem(SyncEntity entity, SyncDbCache dbCopy, SyncBag bagCopy) //where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            var item = entity.CreateInstance();
          
            item.Validate();

            dbCopy.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

            return bagCopy.Set(item);

        }

        internal override CacheState AddItem(SyncEntity entity) //where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("AddItem.SyncEntity");
            }
            entity.ValidateSyncEntity();

            var item= entity.CreateInstance();
            
            item.Validate();
            
            _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this,IntervalSeconds, true);

            return _SyncBag.Set(item);

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
            _SyncBag = new SyncBag(this);
        }

        ///// <summary>
        ///// Satart cache.
        ///// </summary>
        //public void Start(bool loadSyncConfig)
        //{
        //    base.Start(false, false);

        //    if (loadSyncConfig)
        //        LoadSyncConfig();

        //}

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
        /// Get all <see cref="ISyncTable"/> items in cache. 
        /// </summary>
        /// <returns></returns>
        public ICollection<ISyncTable> CacheValues()
        {
            return _SyncBag.GetTables(); 
        }
        
         /*
        /// <summary>
        /// Get the count of all items in all cref="ISyncTable"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public ICollection GetAllSyncValues()
        {
          
            ISyncTable[] Col = _SyncBag.GetItems();
            if (Col == null)
                return null;
            List<object> list = new List<object>();
            foreach (ISyncTable syncitem in Col)
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
        /// Get the count of all items in all cref="ISyncTable"/> items in cache.
        /// </summary>
        /// <returns></returns>
        public int GetAllSyncCount()
        {
            int count = 0;
           
            ISyncTable[] Col = _SyncBag.GetTables();
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

        /// <summary>
        /// Refresh <see cref="ISyncTable"/>
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

        ///// <summary>
        ///// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public SyncTable<T> GetItem<T>(ComplexKey info) where T : IEntityItem
        //{
        //    return _SyncBag.Get<T>(info);
        //}

        /// <summary>
        /// Get spesific item from cache by name, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncTable<T> GetItem<T>(string name) where T : IEntityItem
        {
            return _SyncBag.GetTable<T>(name);
        }

        ///// <summary>
        ///// Get spesific value from cache using item name and keys, if item not found return null
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="name"></param>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public SyncTable<T> GetItem<T>(string name, string[] keys) where T : IEntityItem
        //{
        //    return GetItem<T>(ComplexKey.Get(name, keys));
        //}

        ///// <summary>
        ///// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public ISyncTable GetItem(ComplexKey info)
        //{
        //    return _SyncBag.Get(info);
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISyncTable GetItem(string name)
        {
            return _SyncBag.GetTable(name);
        }

        public IEnumerable<T> ArrayItems<T>(string name)
        {
            return _SyncBag.ArrayItems<T>(name);
        }

        public T SelectFirst<T>(string name,Func<T, bool> query)
        {
            return ArrayItems<T>(name).Where(query).FirstOrDefault();
        }
        public IEnumerable<T> SelectMany<T>(string name,Func<T, bool> query)
        {
            return ArrayItems<T>(name).Where(query);
        }


        #endregion

        #region Get/Set Values

        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public T Get<T>(ComplexKey info) where T : IEntityItem
        {
            SyncTable<T> syncitem = GetItem<T>(info.Prefix);
            if (syncitem != null)
            {
                return syncitem.Get(info.Suffix);
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
            return Get<T>(ComplexArgs.Get(name, keys));
        }

        /// <summary>
        ///  Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public object Get(ComplexKey info)
        {
            ISyncTable syncitem = GetItem(info.Prefix);
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
            return Get(ComplexArgs.Get(name, keys));
        }

        /// <summary>
        ///  Get spesific value from cache as <see cref="IDictionary"/> using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public IDictionary GetRecord(ComplexKey info)
        {
            ISyncTable syncitem = GetItem(info.Prefix);
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
            return GetRecord(ComplexArgs.Get(name, keys));
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
        
                SyncTable<T> item = new SyncTable<T>(connectionKey, entityName, mappingName, sourceName,sourceType,entityKeys,columns, timer,enableNoLock, commandTimeout,false);

                item.Validate();

                item.OnSyncCompleted = OnSyncCompleted;

                _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

               return _SyncBag.Set(entityName, item as ISyncTable);

               
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
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="entityKeys"></param>
        /// <param name="columns"></param>
        /// <param name="interval"></param>
        /// <param name="syncType"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public void AddItem<T>(string connectionKey, string entityName, string mappingName, string[] entityKeys,string columns, TimeSpan interval, SyncType syncType, bool enableNoLock = false, int commandTimeout = 0)
        {
            if (connectionKey == null)
                throw new ArgumentNullException("AddItem.connectionKey");
            if (entityName == null)
                throw new ArgumentNullException("AddItem.entityName");
            if (mappingName == null)
                throw new ArgumentNullException("AddItem.mappingName");
            if (entityKeys == null)
                throw new ArgumentNullException("AddItem.entityKeys");

            try
            {
                string[] sourceName = new string[] { mappingName };
                EntitySourceType sourceType = EntitySourceType.Table;

                SyncTimer timer = new SyncTimer(interval, syncType);

                SyncTable<T> item = new SyncTable<T>(connectionKey, entityName, mappingName, sourceName, sourceType, entityKeys, columns, timer, enableNoLock, commandTimeout, false);
                
                item.Validate();

                item.OnSyncCompleted = OnSyncCompleted;

                _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

                _SyncBag.Set(entityName, item as ISyncTable);


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
        public void AddItem<T>(SyncTable<T> item) //where T : IEntityItem
        {
            if (item == null)
            {
                throw new ArgumentNullException("AddSyncTable.SyncTable");
            }

            try
            {
                item.Validate();
                item.OnSyncCompleted = OnSyncCompleted;

                _DataCache.AddSyncSource(item.ConnectionKey, item.SyncSource, this, IntervalSeconds, true);

                _SyncBag.Set(item.EntityName,item);// (item.Info.Prefix, item);

               
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
            ComplexKey info = ComplexKey.Get(message.Identifier, message.Label);
            if (info.IsEmpty)
            {
                return null;
            }
            ISyncTable syncitem = GetItem(info.Prefix);
            if (syncitem != null)
            {
                return syncitem.GetItemStream(info.Suffix);
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
            ComplexKey info = ComplexKey.Get(message.Identifier, message.Label);
            if (info.IsEmpty)
            {
                return null;
            }

            ISyncTable syncitem = GetItem(info.Prefix);
            if (syncitem != null)
            {
                return syncitem.GetRecordStream(info.Suffix);
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

            return _SyncBag.Contains(ComplexKey.Get(message.Identifier, message.Label));
        }


        #endregion
    }
}
