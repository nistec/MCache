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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace Nistec.Caching.Sync.Embed
{

    /// <summary>
    /// Represents a Sync <see cref="ISyncItem"/> items resource for local sync cache.
    /// It is usefull to ensure that each item will stay synchronized
    /// in run time without any interruption in process.
    /// When problem was occured during the sync process , the item, will stay as the original item.    
    /// </summary>
    public class SyncBag :  ISyncEx, ISyncEx<ISyncItem>
    {
        /// <summary>
        /// Reload current <see cref="SyncBag"/> from a new copy.
        /// </summary>
        /// <param name="copy"></param>
        public void Reload(SyncBag copy)
        {
            //lock (SyncRoot)
            //{
                m_data = copy.m_data;
            //}
        }

        ConcurrentDictionary<string, ISyncItem> m_data;

        ///// <summary>
        ///// Gets an object that can be used to synchronize access to the System.Collections.ICollection.
        ///// </summary>
        //public object SyncRoot
        //{
        //    get { return ((ICollection)m_data).SyncRoot; }
        //}

        /// <summary>
        /// Get all items from sync bag.
        /// </summary>
        /// <returns></returns>
        public ISyncItem[] GetItems()
        {
            ISyncItem[] items = null;

            //lock (SyncRoot)
            //{
                items = m_data.Values.ToArray();
            //}
            return items;
        }
        /// <summary>
        /// Get all keys from sync bag as array of string.
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            string[] items = null;

            //lock (SyncRoot)
            //{
                items = m_data.Keys.ToArray();
            //}
            return items;
        }

        #region ctor

        /// <summary>
        /// Initilaize new instance of <see cref="SyncCache"/>
        /// </summary>
        /// <param name="cacheName"></param>
        public SyncBag(string cacheName)
        {
            m_data = new ConcurrentDictionary<string, ISyncItem>();
        }

       
        /// <summary>
        /// Releases all resources used by the System.ComponentModel.Component.
        /// </summary>
        public void Dispose()
        {
            if (m_data != null)
            {
                m_data.Clear();
            }
            m_data = null;
        }

        #endregion
        /// <summary>
        /// Removes all items.
        /// </summary>
        public void Clear()
        {
            //lock(this.SyncRoot)
            //{
                m_data.Clear();
            //}
        }
        /// <summary>
        ///  Returns the number of elements in a sequence.
        /// </summary>
        public int Count
        {
            get
            {
                //lock (this.SyncRoot)
                //{
                  return  m_data.Count();
                //}
            }
        }

        IDictionary<string, ISyncItem> ISyncEx<ISyncItem>.Items
        {
            get { return m_data; }
        }

        IDictionary ISyncEx.Items
        {
            get { return m_data; }
        }

        /// <summary>
        /// Get if cache contains spesific item by <see cref="CacheKeyInfo"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Contains(CacheKeyInfo info)
        {
            //lock (this.SyncRoot)
            //{
                return m_data.ContainsKey(info.CacheKey);
            //}
        }
        /// <summary>
        /// Get if cache contains spesific item by name and keys.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool Contains(string name, string[] keys)
        {
            return Contains(CacheKeyInfo.Get(name, keys));
        }
        /// <summary>
        /// Set item into sync bag.
        /// </summary>
        /// <param name="item"></param>
        public void Set(ISyncItem item)
        {
            Set(item.Info.ItemName, item);
        }
        /// <summary>
        /// Set item into sync bag.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, SyncItem<T> value)
        {
            if (key == null || value == null)
            {
                return;
            }
            //lock (this.SyncRoot)
            //{
                m_data[key] = value;
            //}
        }

        ///// <summary>
        ///// Set item into sync bag.
        ///// </summary>
        ///// <param name="info"></param>
        ///// <param name="value"></param>
        //public void Set(CacheKeyInfo info, ISyncItem value)
        //{
        //    if (info == null || value == null)
        //    {
        //        return;
        //    }
        //    //lock (this.SyncRoot)
        //    //{
        //        m_data[info.ItemName] = value;
        //    //}
        //}

        /// <summary>
        /// Set item into sync bag.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, ISyncItem value)
        {
            if (key == null || value == null)
            {
                return;
            }
            //lock (this.SyncRoot)
            //{
                m_data[key] = value;
            //}
        }

        ///// <summary>
        ///// Set item into sync bag.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public SyncItem<T> Get<T>(CacheKeyInfo info) //where T : IEntityItem
        //{
        //    ISyncItem item = null;
        //    //lock (this.SyncRoot)
        //    //{
        //        if (m_data.TryGetValue(info.ItemName, out item))
        //        {
        //            return (SyncItem<T>)item;
        //        }
        //    //}
        //    return null;
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncItem<T> Get<T>(string name) //where T : IEntityItem
        {
            ISyncItem item = null;
            //lock (this.SyncRoot)
            //{
                if (m_data.TryGetValue(name, out item))
                {
                    return (SyncItem<T>)item;
                }
            //}
            return null;
        }

        /// <summary>
        /// Get array items from cache by name, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<T> ArrayItems<T>(string name) //where T : IEntityItem
        {
            ISyncItem item = null;
            if (m_data.TryGetValue(name, out item))
            {
                return ((SyncItem<T>)item).Items.Values;
            }
            return null;
        }

        ///// <summary>
        ///// Get array items from cache by name, if item not found return null
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public IEnumerable ArrayItems(string name)
        //{
        //    ISyncItem item = null;
        //    if (m_data.TryGetValue(name, out item))
        //    {
        //        return item.Values;
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Get spesific value from cache using item name and keys, if item not found return null
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="name"></param>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public SyncItem<T> Get<T>(string name, string[] keys) //where T : IEntityItem
        //{
        //    return Get<T>(CacheKeyInfo.Get(name, keys));
        //}
        ///// <summary>
        ///// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //public ISyncItem Get(CacheKeyInfo info)
        //{
        //    ISyncItem item = null;
        //    //lock (this.SyncRoot)
        //    //{
        //        m_data.TryGetValue(info.ItemName, out item);
        //    //}
        //    return item;
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="CacheKeyInfo"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISyncItem Get(string name)
        {
             ISyncItem item = null;
            //lock (this.SyncRoot)
            //{
                m_data.TryGetValue(name, out item);
            //}
            return item;
        }

        #region Refresh

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
            ISyncItem val;
            return m_data.TryRemove(syncName,out val);
            //lock (SyncRoot)
            //{
            //    if (m_data.ContainsKey(syncName))
            //    {
            //        return m_data.Remove(syncName);
            //    }
            //}
            //return false;
        }

        /// <summary>
        /// Refresh <see cref="ISyncItem"/>
        /// </summary>
        /// <param name="syncName"></param>
        public void Refresh(string syncName)
        {

            if (string.IsNullOrEmpty(syncName))
            {
                CacheLogger.Debug("SyncBag sync name is null!");
                return;
            }

            CacheLogger.Debug("SyncBag Refresh : " + syncName);
            ISyncItem syncitem = null;

            //lock (this.SyncRoot)
            //{
                if (!m_data.TryGetValue(syncName, out syncitem))
                {
                    CacheLogger.Debug("SyncBag Refresh sync item not found : " + syncName);
                    return;
                }
            //}

            if (syncitem != null)
            {

                //Lazy<ISyncItem> lazysync = new Lazy<ISyncItem>(LazyThreadSafetyMode.ExecutionAndPublication);
                //Task.Factory.StartNew(() => lazysync.Value.Refresh(null));

                Task.Factory.StartNew(()=> syncitem.Refresh(null));
            }
        }

        /// <summary>
        /// Refrech cache
        /// </summary>
        public void Refresh()
        {
            foreach (string item in this.GetKeys())
            {
                Refresh(item);
            }
        }

      
       #endregion

    }
}
