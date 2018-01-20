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
using Nistec.Channels;

namespace Nistec.Caching.Sync.Remote
{
    /// <summary>
    /// Represents a Sync items <see cref="ISyncTableStream"/> resource for enterprise sync cache.
    /// It is usefull to ensure that each item will stay synchronized
    /// in run time without any interruption in process.
    /// When problem was occured during the sync process , the item, will stay as the original item.    
    /// </summary>
    public class SyncBagStream :IDisposable ,ISyncBag //, ISyncEx, ISyncEx<ISyncTableStream>
    {
        SyncCacheStream Owner;

        /// <summary>
        /// Reload current <see cref="SyncBagStream"/> from a new copy.
        /// </summary>
        /// <param name="copy"></param>
        public void Reload(SyncBagStream copy)
        {
            //~Console.WriteLine("Debuger-SyncBagStream.Reload start");

            foreach (var entry in copy.m_data)
            {
                if (entry.Value == null || entry.Value.Count == 0)
                {
                    CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Debug, string.Format("Reload.SyncBagStream copy {0} is null or empty, item not changed", entry.Key));
                }
                else
                {
                    m_data[entry.Key] = entry.Value;
                    CacheLogger.Debug("SyncBagStream Reload item Completed : " + entry.Key);
                    Thread.Sleep(10);
                }
            }
            copy.DisposeCopy();
        }

        ConcurrentDictionary<string, ISyncTableStream> m_data;
       
        /// <summary>
        /// Get all items from sync bag.
        /// </summary>
        /// <returns></returns>
        public ISyncTableStream[] GetTables()
        {
            ISyncTableStream[] items = null;
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
        /// <param name="owner"></param>
        public SyncBagStream(SyncCacheStream owner)
        {
            Owner = owner;
            m_data = new ConcurrentDictionary<string, ISyncTableStream>();
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
        internal void DisposeCopy()
        {
            if (m_data != null)
            {
                m_data.Clear();
                m_data = null;
            }
            GC.SuppressFinalize(this);
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
        //    ISyncTableStream item = null;

        //        if (m_data.TryGetValue(info.ItemName, out item))
        //        {
        //            return (SyncTableStream<T>)item;
        //        }

        //    return null;
        //}

        /// <summary>
        /// Get spesific item from cache by name, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncTableStream<T> GetTable<T>(string name) where T : IEntityItem
        {
            ISyncTableStream item = null;
            
                if (m_data.TryGetValue(name, out item))
                {
                    return (SyncTableStream<T>)item;
                }
            
            return null;
        }

        internal bool TryGetTable<T>(string name, out SyncTableStream<T> item) where T : IEntityItem
        {
            ISyncTableStream itm;
            if (m_data.TryGetValue(name, out itm))
            {
                item = (SyncTableStream<T>)itm;
                return true;
            }
            item = null;
            return false;
        }
        internal bool TryGetTable(string name, out ISyncTableStream item)
        {
            if (m_data.TryGetValue(name, out item))
            {
                return true;
            }
            item = null;
            return false;
        }

        ///// <summary>
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
        //    ISyncTableStream item = null;

        //        m_data.TryGetValue(info.ItemName, out item);

        //    return item;
        //}

        /// <summary>
        /// Get spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISyncTableStream GetTable(string name)
        {
            ISyncTableStream item = null;
          
                m_data.TryGetValue(name, out item);
           
            return item;
        }


        #endregion

        #region Get/Set Values

        /// <summary>
        /// Get copy of spesific value from cache using <see cref="ComplexKey"/>, if item not found return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public EntityStream Get<T>(ComplexKey info) where T : IEntityItem
        {
            SyncTableStream<T> syncitem = GetTable<T>(info.Prefix);
            if (syncitem != null)
            {
                return syncitem.GetEntityStream(info.Suffix);//.CacheKey);
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
        public EntityStream Get<T>(string name, string[] keys) where T : IEntityItem
        {
            return Get<T>(ComplexArgs.Get(name, keys));
        }

        #endregion

        #region Set

        /// <summary>
        /// Set item into sync bag stream.
        /// </summary>
        /// <param name="item"></param>
        public CacheState Set(ISyncTableStream item)
        {
            return Set(item.EntityName,item);// (item.Info.Prefix, item);
        }

        /// <summary>
        /// Set item into sync bag stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public CacheState Set<T>(string key, SyncTableStream<T> value)
        {
            if (key == null || value == null)
            {
                return CacheState.ArgumentsError;
            }

            long newSize = value.Size;
            long curSize = 0;
            int curCount = 0;
            ISyncTableStream item;
            if (m_data.TryGetValue(key, out item))
            {
                curSize = item.Size;
                curCount = 1;
                if (m_data.TryUpdate(key, value, item))
                {
                    SetSize(curSize, newSize, curCount, 1, false);
                    return CacheState.ItemChanged;
                }
                else
                {
                    return CacheState.SetItemFailed;
                }
            }
            else
            {
                if (m_data.TryAdd(key, value))
                {
                    SetSize(curSize, newSize, curCount, 1, false);
                    return CacheState.ItemAdded;
                }
                else
                {
                    return CacheState.AddItemFailed;
                }

            }

            //value.Owner = this.Owner;
            //ISyncTableStream item;
            //m_data.TryGetValue(key, out item);

            //value.Owner = this;


            //m_data[key] = value;
        }

        ///// <summary>
        ///// Set item into sync bag stream.
        ///// </summary>
        ///// <param name="info"></param>
        ///// <param name="value"></param>
        //public void Set(ComplexKey info, ISyncTableStream value)
        //{
        //    if (info == null || value == null)
        //    {
        //        return;
        //    }

        //    ISyncTableStream item;

        //        m_data.TryGetValue(info.ItemName, out item);
        //        m_data[info.ItemName] = value;

        //}

        /// <summary>
        /// Set item into sync bag stream.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public CacheState Set(string key, ISyncTableStream value)
        {
            if (key == null || value == null)
            {
                return CacheState.ArgumentsError;
            }
            //((ISyncTableOwner)value).Owner = this.Owner;

            //ISyncTableStream item;
            //m_data.TryGetValue(key, out item);
            //value.Owner = this;

            long newSize = value.Size;
            long curSize = 0;
            int curCount = 0;
            ISyncTableStream item;
            if (m_data.TryGetValue(key, out item))
            {
                curSize = item.Size;
                curCount = 1;
                if (m_data.TryUpdate(key, value, item))
                {
                    SetSize(curSize, newSize, curCount, 1, false);
                    return CacheState.ItemChanged;
                }
                else
                {
                    return CacheState.SetItemFailed;
                }
            }
            else
            {
                if (m_data.TryAdd(key, value))
                {
                    SetSize(curSize, newSize, curCount, 1, false);
                    return CacheState.ItemAdded;
                }
                else
                {
                    return CacheState.AddItemFailed;
                }

            }
            //m_data[key] = value;
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

        void SetSize(ISyncTableStream oldItem, ISyncTableStream newItem)
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
        /// Get if cache contains spesific item by <see cref="ComplexKey"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Contains(ComplexKey info)
        {
            ISyncTableStream item = null;
           
                if (!m_data.TryGetValue(info.Prefix, out item))
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
            return Contains(ComplexArgs.Get(name, keys));
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
        public bool RemoveTable(string syncName)
        {
            if (string.IsNullOrEmpty(syncName))
            {
                return false;
            }
            if (!m_data.ContainsKey(syncName))
            {
                return false;
            }

            ISyncTableStream item;
            int currentCount = Count;
          
            if (m_data.TryRemove(syncName, out item))
            {
                SetSize(item.Size, 0, currentCount, Count, false);
                return true;// m_data.Remove(syncName);
            }

           

            return false;
        }

        public bool Exists(string syncName)
        {
            if (string.IsNullOrEmpty(syncName))
            {
                return false;
            }
            return m_data.ContainsKey(syncName);
        }

        /// <summary>
        /// Refresh <see cref="ISyncTable"/>
        /// </summary>
        /// <param name="syncName"></param>
        public void Refresh(string syncName)
        {
            if (string.IsNullOrEmpty(syncName))
            {
                return;
            }
            ISyncTableStream syncitem = null;

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
