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
using Nistec.Data.Entities;
using System.Collections;
using Nistec.Data.Entities.Cache;
using Nistec.Threading;
using Nistec.Generic;
using Nistec.Caching.Server;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Nistec.Caching.Sync.Remote
{
    /// <summary>
    /// Represents a Sync items <see cref="ISyncItemStream"/> resource for enterprise sync cache.
    /// It is usefull to ensure that each item will stay synchronized
    /// in run time without any interruption in process.
    /// When problem was occured during the sync process , the item, will stay as the original item.    
    /// </summary>
    public class SyncBagStream :IDisposable 
    {
        SyncCacheStream Owner;

        /// <summary>
        /// Reload current <see cref="SyncBagStream"/> from a new copy.
        /// </summary>
        /// <param name="copy"></param>
        public void Reload(SyncBagStream copy)
        {
            foreach (var entry in copy.m_data)
            {
                if (entry.Value == null || entry.Value.Count == 0)
                {
                    CacheLogger.Logger.LogAction(CacheAction.SyncItem, CacheActionState.Debug, string.Format("Reload.SyncBagStream copy {0} is null or empty, item not changed", entry.Key));
                }
                else
                {
                    m_data[entry.Key] = entry.Value;
                    CacheLogger.Debug("SyncBagStream Reload item Completed : " + entry.Key);
                    Thread.Sleep(10);
                }
            }
        }

        ConcurrentDictionary<string, ISyncItemStream> m_data;
       
        /// <summary>
        /// Get all items from sync bag.
        /// </summary>
        /// <returns></returns>
        public ISyncItemStream[] GetItems()
        {
            ISyncItemStream[] items = null;
            items = m_data.Values.ToArray();
            return items;
        }
        /// <summary>
        /// Get all keys from sync bag as array of string.
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            string[] items = null;

                items = m_data.Keys.ToArray();

            return items;
        }

        #region ctor

        /// <summary>
        /// Initilaize new instance of <see cref="SyncBagStream"/>
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="owner"></param>
        public SyncBagStream(string cacheName, SyncCacheStream owner)
        {
            Owner = owner;
            m_data = new ConcurrentDictionary<string, ISyncItemStream>();
        }

      
        #endregion

        #region IDisposable
        /// <summary>
        /// Destructor.
        /// </summary>
        ~SyncBagStream()
        {
             Dispose(false);
        }
        /// <summary>
        /// Release allocated resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_data != null)
                {
                    m_data = null;
                }
            }
        }

        #endregion


        #region Get/Set Items

        ///// <summary>
        ///// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public SyncItemStream<T> GetItem<T>(CacheKeyInfo info) where T : IEntityItem
        //{
        //    ISyncItemStream item = null;
           
        //        if (m_data.TryGetValue(info.ItemName, out item))
        //        {
        //            return (SyncItemStream<T>)item;
        //        }
            
        //    return null;
        //}

        /// <summary>
        /// Get spesific item from cache by name, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncItemStream<T> GetItem<T>(string name) where T : IEntityItem
        {
            ISyncItemStream item = null;
            
                if (m_data.TryGetValue(name, out item))
                {
                    return (SyncItemStream<T>)item;
                }
            
            return null;
        }

        ///// <summary>
        ///// Get spesific value from cache using item name and keys, if item not found return null
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="name"></param>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public SyncItemStream<T> GetItem<T>(string name, string[] keys) where T : IEntityItem
        //{
        //    return GetItem<T>(CacheKeyInfo.Get(name, keys));
        //}
        ///// <summary>
        ///// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public ISyncItemStream GetItem(CacheKeyInfo info)
        //{
        //    ISyncItemStream item = null;
            
        //        m_data.TryGetValue(info.ItemName, out item);
           
        //    return item;
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISyncItemStream GetItem(string name)
        {
            ISyncItemStream item = null;
          
                m_data.TryGetValue(name, out item);
           
            return item;
        }


        #endregion

        #region Get/Set Values

        /// <summary>
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public EntityStream Get<T>(CacheKeyInfo info) where T : IEntityItem
        {
            SyncItemStream<T> syncitem = GetItem<T>(info.ItemName);
            if (syncitem != null)
            {
                return syncitem.GetEntityStream(info.CacheKey);
            }
            return null;
        }
        /// <summary>
        /// Get spesific value from cache using item name and keys, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public EntityStream Get<T>(string name, string[] keys) where T : IEntityItem
        {
            return Get<T>(CacheKeyInfo.Get(name, keys));
        }

        #endregion

        #region Set

        /// <summary>
        /// Set item into sync bag stream.
        /// </summary>
        /// <param name="item"></param>
        public void Set(ISyncItemStream item)
        {
            Set(item.Info.ItemName, item);
        }

        /// <summary>
        /// Set item into sync bag stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, SyncItemStream<T> value)
        {
            if (key == null || value == null)
            {
                return;
            }

            ISyncItemStream item;

                m_data.TryGetValue(key, out item);
                m_data[key] = value;
          

        }

        ///// <summary>
        ///// Set item into sync bag stream.
        ///// </summary>
        ///// <param name="info"></param>
        ///// <param name="value"></param>
        //public void Set(CacheKeyInfo info, ISyncItemStream value)
        //{
        //    if (info == null || value == null)
        //    {
        //        return;
        //    }

        //    ISyncItemStream item;

        //        m_data.TryGetValue(info.ItemName, out item);
        //        m_data[info.ItemName] = value;
          
        //}

        /// <summary>
        /// Set item into sync bag stream.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, ISyncItemStream value)
        {
            if (key == null || value == null)
            {
                return;
            }

            ISyncItemStream item;

            
            m_data.TryGetValue(key, out item);

            m_data[key] = value;
           
        }

        #endregion

        #region collection methods
        /// <summary>
        /// Removes all items.
        /// </summary>
        public void Clear()
        {
           
                m_data.Clear();
          

            Owner.SizeRefresh();
        }
        /// <summary>
        ///  Returns the number of elements in a sequence.
        /// </summary>
        public int Count
        {
            get
            {
              
                    return m_data.Count();
                
            }
        }

        long _Size;
        /// <summary>
        /// Get the total size of all items in bytws.
        /// </summary>
        public long Size
        {
            get
            {
                return _Size;
            }
        }

        void SetSize(long currentSize, long newSize,int oldCount, int newCount, bool exchange)
        {
            _Size += (newSize - currentSize);
            Owner.SizeExchage(currentSize, newSize, oldCount,newCount, exchange);
        }

        void SetSize(ISyncItemStream oldItem, ISyncItemStream newItem)
        {
            long oldSize = 0; 
            long newSize=0;

            if(oldItem!=null)
                oldSize = oldItem.Size;
             if(newItem!=null)
                newSize=newItem.Size;

             _Size += (newSize - oldSize);
            Owner.SizeExchage(oldItem, newItem);
        }

        /// <summary>
        /// Get if cache contains spesific item by <see cref="CacheKeyInfo"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Contains(CacheKeyInfo info)
        {
            ISyncItemStream item = null;
           
                if (!m_data.TryGetValue(info.ItemName, out item))
                {
                    return false;
                }
            
            return item.Contains(info);
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

        #endregion

        #region Refresh


        void OnRefreshTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.None, e.Args.State.ToString());

            Console.WriteLine("OnRefreshTaskCompleted: " + e.Args.State.ToString());
        }

        /// <summary>
        /// Remove item from sync bag.
        /// </summary>
        /// <param name="syncName"></param>
        public bool RemoveItem(string syncName)
        {
            if (string.IsNullOrEmpty(syncName))
            {
                return false;
            }
            if (!m_data.ContainsKey(syncName))
            {
                return false;
            }

            ISyncItemStream item;
            int currentCount = Count;
          
            if (m_data.TryRemove(syncName, out item))
            {
                SetSize(item.Size, 0, currentCount, Count, false);
                return true;// m_data.Remove(syncName);
            }

           

            return false;
        }


        /// <summary>
        /// Refresh <see cref="ISyncItem"/>
        /// </summary>
        /// <param name="syncName"></param>
        public void Refresh(string syncName)
        {
            if (string.IsNullOrEmpty(syncName))
            {
                return;
            }
            ISyncItemStream syncitem = null;
           
                if (!m_data.TryGetValue(syncName, out syncitem))
                {
                    return;
                }
           

            if (syncitem != null)
            {
                syncitem.RefreshAsync(syncName);
            }
        }

        /// <summary>
        /// Refrech cache
        /// </summary>
        public void Refresh()
        {
            foreach (string item in GetKeys())
            {
                Refresh(item);
            }
        }

        /// <summary>
        /// REfresh all items in sync bag stream.
        /// </summary>
        public void RefreshAllAsync()
        {
          
            Task rask = Task.Factory.StartNew(() => RefreshAllTask(null));
        }

        private void RefreshAllTask(object args)
        {
            Refresh();
        }

        void OnRefreshAllTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.None, e.Args.State.ToString());

            Console.WriteLine("OnRefreshAllTaskCompleted: " + e.Args.State.ToString());
        }


       #endregion

    }
}
