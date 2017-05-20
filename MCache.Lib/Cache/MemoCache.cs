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
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;

using Nistec.Collections;
using Nistec.Threading;
using Nistec.Drawing;
using Nistec.Runtime;
using Nistec.Data.Entities;
using Nistec.Caching.Remote;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nistec.Caching.Config;
using Nistec.IO;
using Nistec.Data;
using Nistec.Data.Factory;

namespace Nistec.Caching
{
    /// <summary>
    /// Represent memory cache include hash functionality.
    /// </summary>
    [Serializable]
    public class MemoCache :  IEnumerable, IDisposable//, ICacheFinder
    {

        static MemoCache _Current;
        public static MemoCache Current
        {
            get
            {
                if(_Current==null)
                {
                    _Current = new MemoCache("Current");
                }
                return _Current;
            }
        }

        #region ctor and members

        /// <summary>
        /// Get if is disposed.
        /// </summary>
        protected bool IsDisposed { get { return _disposed; } }
        /// <summary>
        /// Get Keys By Session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        protected IEnumerable<string> GetKeysBySession(string sessionId)
        {
            if (this.m_cacheList == null)
            {
                return null;
            }
            IEnumerable<string> k = from n in m_cacheList.Values.Cast<CacheEntry>() where n.Id == sessionId select n.Key;
            return k;
        }

        internal bool _disposed;
        internal long _maxSize;
        private int _timeout;
        
        internal ConcurrentDictionary<string,CacheEntry> m_cacheList;
        TimerDispatcher m_Timer;

        private string m_cacheName;
        internal bool m_IsRemoteCache;
        private int m_numItems;
        private const int MinCacheSize = 2;
 
        /// <summary>
        /// Get cache name
        /// </summary>
        public string CacheName
        {
            get { return m_cacheName; }
        }

        
        /// <summary>
        /// CacheException event 
        /// </summary>
        public event CacheExceptionEventHandler CacheException;
        /// <summary>
        /// CacheStateChanged event.
        /// </summary>
        public event EventHandler CacheStateChanged;
        /// <summary>
        /// SynchronizeStart event.
        /// </summary>
        public event EventHandler SynchronizeStart;

        /// <summary>
        /// Constructor with <see cref="CacheProperties"/> properties.
        /// </summary>
        /// <param name="prop"></param>
        public MemoCache(CacheProperties prop)
            : this(prop, false)
        {

            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized MemoCache");
        }

        internal MemoCache(CacheProperties prop, bool isRemote)
        {
            this.m_cacheName = Types.NZorEmpty(prop.CacheName,"MCache");
            EnableDynamicCache = true;
            this._maxSize = prop.MaxSize;
            this._timeout = prop.DefaultExpiration;
            int initialCapacity = prop.InitialCapacity;

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            if (initialCapacity < 100)
                initialCapacity = 101;

            this.m_cacheList = new ConcurrentDictionary<string, CacheEntry>(concurrencyLevel, initialCapacity);
            m_Timer = new TimerDispatcher(prop.SyncIntervalSeconds, initialCapacity, isRemote);
            m_IsRemoteCache = isRemote;

            this._disposed = false;
            this.m_numItems = 0;
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized MemoCache");
        }
        /// <summary>
        /// Default constructor with cache name.
        /// </summary>
        /// <param name="cacheName"></param>
        public MemoCache(string cacheName)
            : this(new CacheProperties(cacheName, 1000000L),false)
        {
        }
        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.Stop();
                    if (this.m_cacheList != null)
                    {
                        this.m_cacheList.Clear();
                        this.m_cacheList = null;
                    }
                    if (this.m_Timer != null)
                    {
                        this.m_Timer.Clear();
                        this.m_Timer = null;
                    }
                }
                this._disposed = true;
            }

        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~MemoCache()
        {
            Dispose(false);
        }

     
      
        #endregion

        #region Add item
        /// <summary>
        /// Add new item to cache async.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual CacheState AddItemAsync(CacheEntry item)
        {
            return ExecuteTask<CacheState>(() => AddItem(item));
        }
        /// <summary>
        /// Add new item to cache.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal protected virtual CacheState AddItem(CacheEntry item)
        {
            if (this._disposed)
            {
                return CacheState.CacheNotReady;
            }

            if (item == null)
            {
                return CacheState.InvalidItem;
            }
            string cacheKey = item.Key;
            CacheState state = CacheState.Ok;

            try
            {
                state = SizeValidate(item.Size);//, 0.5f);
                if ((int)state > 500)
                {
                    return state;
                }

                if (m_cacheList.TryAdd(cacheKey, item))
                {
                    state = CacheState.ItemAdded;
                    SizeExchage(0, item.Size,0,1,false);
                }
                else
                {
                    CacheEntry curitem;
                    if (m_cacheList.TryGetValue(cacheKey, out curitem))
                    {
                        SizeExchage(curitem.Size, item.Size, 0,0, false);
                        curitem = item;
                    }
                    else
                    {
                        throw new CacheException(CacheState.AddItemFailed, "Could not find existing specified item with key: " + cacheKey);
                    }
                    state = CacheState.ItemChanged;
                }
                if (item.AllowExpires)
                {
                    m_Timer.Add(item);
                }
                if (state == CacheState.ItemAdded)
                    this.OnItemAdded(item);
                else
                    this.OnItemChanged(item.Key, item.Size);

                return state;
            }
            catch (Exception ex)
            {
                OnCacheException(ex.Message, CacheErrors.ErrorSetValue);
                return CacheState.AddItemFailed;
            }

        }
        /// <summary>
        /// Add new item to cache async
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public virtual CacheState AddItemAsync(string cacheKey, object value, int expiration)
        {
            return ExecuteTask<CacheState>(() => AddItem(cacheKey, value, expiration));
        }
        /// <summary>
        /// Add item to cache.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public CacheState AddItem(string cacheKey, object value, int expiration)
        {
            CacheEntry item = new CacheEntry(cacheKey, value,null, expiration, this.m_IsRemoteCache);
            return this.AddItem(item);
        }
        /// <summary>
        /// Add item to cache.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="cacheObjType"></param>
        /// <param name="sessionId"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public CacheState AddItem(string cacheKey, object value, CacheObjType cacheObjType, string sessionId, int expiration)
        {
            CacheEntry item = new CacheEntry(cacheKey, value, sessionId, expiration, this.m_IsRemoteCache);
            return this.AddItem(item);
        }

        #endregion

        #region Copy Item
        /// <summary>
        /// Duplicate item with new cacheKey.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public virtual CacheState CopyItem(string source, string dest, int expiration)
        {
            if (this._disposed)
            {
                return CacheState.CacheNotReady;
            }
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(dest))
                return CacheState.InvalidItem;

            CacheEntry item = null;
            if (m_cacheList.TryGetValue(source, out item))
            {
                return AddItem(item.Copy(dest, expiration));
            }
            return CacheState.InvalidItem;

        }

        internal AckStream CopyItemInternal(string source, string dest, int expiration)
        {
            var state = CopyItem(source, dest, expiration);
            return CacheEntry.GetAckStream(state, "CopyItem");
        }

        internal AckStream CutItemInternal(string source, string dest, int expiration)
        {
            var state = CutItem(source, dest, expiration);
            return CacheEntry.GetAckStream(state, "CutItem");
        }

        /// <summary>
        /// Cut item from cache an place it with a new cacheKey.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public virtual CacheState CutItem(string source, string dest, int expiration)
        {
            if (this._disposed)
            {
                return CacheState.CacheNotReady;
            }
            CacheState state=(CacheState) CopyItem(source, dest, expiration);
            if (state == CacheState.ItemChanged || state == CacheState.ItemAdded)
            {
                RemoveItem(source);
            }
            return state;
        }

        /// <summary>
        /// Merge item with a new collection items, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public virtual CacheState MergeItem<T>(string cacheKey, T value) //where T : IMergeable
        {
            return MergeItem(cacheKey, value);
            
        }
        /// <summary>
        /// Merge item with a new collection items, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/>.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        public virtual CacheState MergeItem(string cacheKey, object value)
        {
            if (this._disposed)
            {
                return CacheState.CacheNotReady;
            }
            if (string.IsNullOrEmpty(cacheKey))
                return CacheState.InvalidItem;

            CacheEntry item = null;

            if (m_cacheList.TryGetValue(cacheKey, out item))
            {
                try
                {
                    int curSize = item.Size;
                    item.Merge(cacheKey, value);
                    SizeExchage(curSize, item.Size, 0,0, false);
                    if (item.AllowExpires)
                    {
                        m_Timer.Add(item);
                    }
                    this.OnItemChanged(item.Key, item.Size);

                    return CacheState.ItemChanged;
                }
                catch (NotSupportedException nex)
                {
                    OnCacheException(nex.Message, CacheErrors.ErrorNotSupportedItem);
                    return CacheState.MergeItemFailed;
                }
                catch (Exception ex)
                {
                    OnCacheException(ex.Message, CacheErrors.ErrorSetValue);
                    return CacheState.MergeItemFailed;
                }
            }
            return CacheState.InvalidItem;
        }

        /// <summary>
        /// Merge item by remove all items from the current item that match the collection items in argument, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public virtual CacheState MergeRemoveItem<T>(string cacheKey, T value) //where T : IMergeable
        {
            return MergeRemoveItem(cacheKey, value);
        }
        /// <summary>
        /// Merge item by remove all items from the current item that match the collection items in argument, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/>.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        public virtual CacheState MergeRemoveItem(string cacheKey, object value)
        {
            if (this._disposed)
            {
                return CacheState.CacheNotReady;
            }
            if (string.IsNullOrEmpty(cacheKey))
                return CacheState.InvalidItem;

            CacheEntry item = null;
            if (m_cacheList.TryGetValue(cacheKey, out item))
            {
                try
                {
                    int curSize = item.Size;
                    item.MergeRemove(cacheKey, value);
                    SizeExchage(curSize, item.Size, 0,0, false);
                    if (item.AllowExpires)
                    {
                        m_Timer.Add(item);
                    }
                    this.OnItemChanged(item.Key, item.Size);

                    return CacheState.ItemChanged;
                }
                catch (NotSupportedException nex)
                {
                    OnCacheException(nex.Message, CacheErrors.ErrorNotSupportedItem);
                    return CacheState.MergeItemFailed;
                }
                catch (Exception ex)
                {
                    OnCacheException(ex.Message, CacheErrors.ErrorSetValue);
                    return CacheState.MergeItemFailed;
                }
            }

            return CacheState.InvalidItem;
        }
        #endregion

        #region Get item
        /// <summary>
        /// View <see cref="CacheEntry"/> item properties async. 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual CacheEntry ViewItemAsync(string cacheKey)
        {
            return ExecuteTask<CacheEntry>(() => ViewItem(cacheKey));
        }
        /// <summary>
        /// View <see cref="CacheEntry"/> item properties.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual CacheEntry ViewItem(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                return null;

            CacheEntry item = null;
            if (this.m_cacheList.TryGetValue(cacheKey, out item))
            {
                CacheActionState ack = item == null ? CacheActionState.Failed : CacheActionState.Ok;
                this.LogAction(CacheAction.ViewItem, CacheActionState.Ok, cacheKey);
                return item.Clone();
            }
            this.LogAction(CacheAction.ViewItem, CacheActionState.Failed, cacheKey);
            return item;
        }
        /// <summary>
        /// Get <see cref="CacheEntry"/> item from cache async.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual CacheEntry GetItemAsync(string cacheKey)
        {
            return ExecuteTask<CacheEntry>(() => GetItem(cacheKey));
        }
        /// <summary>
        /// Get <see cref="CacheEntry"/> item from cache.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual CacheEntry GetItem(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                return null;
 
            CacheEntry item = null;
            if (this.m_cacheList.TryGetValue(cacheKey, out item))
            {
                item.SetStatistic();
                if (item.AllowExpires)
                {
                    m_Timer.Add(item);
                }
            }

            CacheActionState ack = item == null ? CacheActionState.Failed : CacheActionState.Ok;
            this.LogAction(CacheAction.GetItem, ack, cacheKey);
            return item;
        }
        /// <summary>
        /// Fetch <see cref="CacheEntry"/> item from cache sync (Get and remove item from cache).
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual CacheEntry FetchItemAsync(string cacheKey)
        {
            return ExecuteTask<CacheEntry>(() => FetchItem(cacheKey));
        }
        /// <summary>
        ///  Fetch <see cref="CacheEntry"/> item from cache (Get and remove item from cache).
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual CacheEntry FetchItem(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                return null;

            CacheEntry item = null;
            if (this.m_cacheList.TryRemove(cacheKey, out item))
            {
                SizeExchage(item.Size, 0, 1,0, false);
                if (item.AllowExpires)
                {
                    m_Timer.Remove(cacheKey);
                }
            }
            CacheActionState ack = item == null ? CacheActionState.Failed : CacheActionState.Ok;
            this.LogAction(CacheAction.FetchItem, ack, cacheKey);
            return item;
        }

        internal int GetItemSize(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                return 0;
            try
            {

                CacheEntry item = null;
                if (this.m_cacheList.TryGetValue(cacheKey, out item))
                {
                    return item.Size;
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Get item value from cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual T GetValue<T>(string cacheKey)
        {
            return GenericTypes.Cast<T> (GetValue(cacheKey));
        }

        /// <summary>
        /// Get item value from cache.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual object GetValue(string cacheKey)
        {
            CacheEntry item = GetItem(cacheKey);
            if (item == null || item.IsEmpty)
            {
                return null;
            }
            return item.GetValue();
        }
        /// <summary>
        /// Get item value from cache as json.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual string GetValueJson(string cacheKey)
        {
            CacheEntry item = GetItem(cacheKey);
            if (item == null || item.IsEmpty)
            {
                return null;
            }
            return item.GetValueJson();
        }

        /// <summary>
        /// Fetch item value from cache (Get and remove item from cache).
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual object FetchValue(string cacheKey)
        {
            CacheEntry item = FetchItem(cacheKey);
            if (item == null || item.IsEmpty)
            {
                return null;
            }
            return item.GetValue();
        }
        /// <summary>
        /// Fetch item value from cache (Get and remove item from cache).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual T FetchValue<T>(string cacheKey)
        {
            return GenericTypes.Cast<T>(cacheKey);
        }
        #endregion

        #region Remove Item
        /// <summary>
        /// Remove item from cache async.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual bool RemoveItemAsync(string cacheKey)
        {
            return ExecuteTask<bool>(() => RemoveItem(cacheKey));
        }
        /// <summary>
        /// Remove item from cache.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public bool RemoveItem(string cacheKey)
        {
            if (this._disposed)
            {
                return false;
            }
            if (string.IsNullOrEmpty(cacheKey))
                return false;

            bool flag = false;
            int size = 0;

            CacheEntry item = null;
            if (this.m_cacheList.TryRemove(cacheKey, out item))
            {
                //size = item.GetSize();
                SizeExchage(item.Size, 0, 1,0, false);
                if (item.AllowExpires)
                {
                    m_Timer.Remove(cacheKey);
                }
                flag = true;
                this.OnItemRemoved(cacheKey, size, false);
            }


            return flag;
        }
        /// <summary>
        /// Replace item with the same cache key in cache.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ReplaceItem(CacheEntry item)
        {
            if (this._disposed)
            {
                return false;
            }
            if (item == null)
            {
                return false;
            }

            bool flag = false;
            int size = 0;

            try
            {
                string cacheKey = item.Key;
                if (string.IsNullOrEmpty(cacheKey))
                    return false;

                CacheEntry curitem = null;
                if (this.m_cacheList.TryGetValue(cacheKey, out curitem))
                {
                    size = item.Size - curitem.Size;
                    SizeExchage(curitem.Size, item.Size, 0,0, false);
                    curitem = item;
                    if (item.AllowExpires)
                    {
                        m_Timer.Update(cacheKey);
                    }
                    flag = true;
                    this.OnItemChanged(cacheKey, size);
                }
            }
            catch (Exception ex)
            {
                OnCacheException("ReplaceItem error: " + ex.Message, CacheErrors.ErrorUnexpected);
            }
            return flag;
        }

        /// <summary>
        /// Remove meny items from cache.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="isTimeout"></param>
        public void RemoveItems(string[] items, bool isTimeout)
        {
            if (items == null || items.Length == 0)
                return;
            try
            {
                foreach (string str in items)
                {
                    RemoveItemAsync(str);
                }
            }
            catch (Exception ex)
            {
                OnCacheException("RemoveItems error: " + ex.Message, CacheErrors.ErrorUnexpected);
            }
            this.GcCollect();
        }

        #endregion

        #region Keep alive Item
        /// <summary>
        /// Keep alive item in cache, using for long expiration.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual bool KeepAliveItemAsync(string cacheKey)
        {
            return ExecuteTask<bool>(() => KeepAliveItem(cacheKey));
        }
        /// <summary>
        /// Keep alive item in cache, using for long expiration.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual bool KeepAliveItem(string cacheKey)//, bool isTimeout)
        {
            if (this._disposed)
            {
                return false;
            }
            if (string.IsNullOrEmpty(cacheKey))
                return false;

            bool flag = false;
            CacheEntry item = null;
            if (m_cacheList.TryGetValue(cacheKey, out item))
            {
                item.SetStatistic();
                m_Timer.Add(item);
                flag = true;
            }
            
            return flag;
        }

        #endregion
        
        #region Xml cache methods
        /// <summary>
        /// Save all cache item to xml file.
        /// </summary>
        /// <param name="fileName"></param>
        public void CacheToXml(string fileName)
        {
            DataSet set2 = this.CacheToDataSet();
            if (set2 == null)
            {
                return;
            }
            set2.WriteXml(fileName);
            this.LogAction(CacheAction.General, CacheActionState.None, "CacheToXml");
        }
        /// <summary>
        /// Save all cache item to <see cref="DataSet"/>.
        /// </summary>
        /// <returns></returns>
        public DataSet CacheToDataSet()
        {
            DataSet set2;
            this.LogAction(CacheAction.General, CacheActionState.None, "CacheToXmlData");
            try
            {
               

                ICollection<CacheEntry> items = this.Items;
                if ((items == null) || (items.Count == 0))
                {
                    return null;
                }
                DataSet set = new DataSet("Items");
                DataTable table = CacheEntry.CacheItemSchema();
                foreach (CacheEntry item in items)
                {
                    table.Rows.Add(item.ToDataRow());
                }
                set.Tables.Add(table);
                set2 = set;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return set2;
        }
        /// <summary>
        /// Load cache from <see cref="DataSet"/>
        /// </summary>
        /// <param name="dset"></param>
        public void CacheFromDataSet(DataSet dset)
        {
            this.LogAction(CacheAction.General, CacheActionState.None, "CacheToXmlData");
            try
            {
                foreach (DataTable table in dset.Tables)
                {
                    if (table.TableName == "CacheEntry")
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            CacheEntry item = CacheEntry.ItemFromDataRow(row, IsRemote);
                            this.AddItem(item);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        /// <summary>
        /// Load cache from xml file.
        /// </summary>
        /// <param name="fileName"></param>
        public void CacheFromXml(string fileName)
        {
            this.LogAction(CacheAction.General, CacheActionState.None, "CacheToXmlData");
            try
            {
                DataSet set = new DataSet("Items");
                set.ReadXml(fileName);
                DataTable table = set.Tables["CacheEntry"];
                foreach (DataRow row in table.Rows)
                {
                    CacheEntry item = CacheEntry.ItemFromDataRow(row, IsRemote);
                    this.AddItem(item);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        /// <summary>
        /// Load cache from xml file.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="filName"></param>
        public static void CacheFromXml(CacheProperties prop, string filName)
        {
            new MemoCache(prop).CacheFromXml(filName);
        }
        
        #endregion

        #region Timer Sync
        /// <summary>
        /// Start Timer
        /// </summary>
        public void Start()
        {
            if (!m_Timer.Initialized)
            {
                m_Timer.SyncStarted += new EventHandler(m_cacheTimeout_SyncStarted);
                m_Timer.SyncCompleted += new SyncTimeCompletedEventHandler(m_cacheTimeout_SyncCompleted);
                m_Timer.Start();

                this.OnCacheStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Stop Timer
        /// </summary>
        public void Stop()
        {
            if (m_Timer.Initialized)
            {
                m_Timer.SyncStarted -= new EventHandler(m_cacheTimeout_SyncStarted);
                m_Timer.SyncCompleted -= new SyncTimeCompletedEventHandler(m_cacheTimeout_SyncCompleted);

                m_Timer.Stop();
                this.OnCacheStateChanged(EventArgs.Empty);
            }
        }

        void m_cacheTimeout_SyncCompleted(object sender, SyncTimeCompletedEventArgs e)
        {
            RemoveItems(e.Items, true);
        }

        void m_cacheTimeout_SyncStarted(object sender, EventArgs e)
        {
            OnSynchronizeStart(e);
        }
        /// <summary>
        /// On Synchronize Start
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSynchronizeStart(EventArgs e)
        {
            if (this.SynchronizeStart != null)
            {
                this.SynchronizeStart(this, e);
            }
        }
        #endregion

        #region Load Items
        /// <summary>
        /// Load Item to cache
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public object LoadItem(string cacheKey, string source, CacheObjType type, int expiration)
        {
            
            string key = cacheKey;
            if (key == null)
            {
                key = this.GetPathKey(source);
            }
            if (!this.Contains(key))
            {
                return this.LoadNewItem(key, source, type, expiration);
            }
            return this.GetValue(key);
        }


        private object LoadNewItem(string cacheKey, string source, CacheObjType cacheObjType, int expiration)
        {
            this.LogAction(CacheAction.LoadItem, CacheActionState.None, cacheKey);
            CacheEntry item = null;
            object val = null;
            switch (cacheObjType)
            {
                case CacheObjType.Default:
                case CacheObjType.RemotingData:
                case CacheObjType.SerializeClass:
                    return null;

                case CacheObjType.TextFile:
                case CacheObjType.BinaryFile:
                case CacheObjType.XmlDocument:
                case CacheObjType.HtmlFile:
                    item = new CacheEntry();
                    item.LoadItem(cacheObjType, null, source, m_IsRemoteCache);
                    val = item.GetValue();
                    this.AddItem(item);
                    return val;

                case CacheObjType.ImageFile:
                    item = new CacheEntry();
                    item.LoadItem(cacheObjType, null, source, m_IsRemoteCache);
                    val = item.GetValue();
                    if (val != null)
                    {
                        val = CacheUtil.DeserializeImage(val.ToString());
                    }
                    this.AddItem(item);
                    return val;
            }
            return val;
        }

        /// <summary>
        /// Load item to cache from db.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionKey"></param>
        /// <param name="commandText"></param>
        /// <param name="cmdType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="keyValueParameters"></param>
        /// <param name="sessionId"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public T LoadData<T>(string connectionKey, string commandText, CommandType cmdType, int commandTimeout, object[] keyValueParameters, string sessionId, int expiration)
        {
            T result = default(T);
            CommandContext command = new CommandContext(connectionKey, commandText, cmdType,commandTimeout, typeof(T));
            command.CreateParameters(keyValueParameters);
            string key = command.CreateKey();
            result = this.GetValue<T>(key);

            if (result == null)
            {
                result = command.ExecCommand<T>();
                AddItem(key, result, CacheObjType.RemotingData, sessionId, expiration);
            }

            return result;
        }

        /// <summary>
        /// Load item to cache from db.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="connectionKey"></param>
        /// <param name="commandText"></param>
        /// <param name="cmdType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="keyValueParameters"></param>
        /// <param name="sessionId"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public TResult LoadData<TItem, TResult>(string connectionKey, string commandText, CommandType cmdType, int commandTimeout, object[] keyValueParameters, string sessionId, int expiration)
        {
            TResult result = default(TResult);
            CommandContext command = new CommandContext(connectionKey, commandText, cmdType, commandTimeout, typeof(TResult));
            command.CreateParameters(keyValueParameters);
            string key = command.CreateKey();
            result = this.GetValue<TResult>(key);

            if (result == null)
            {
                result = command.ExecCommand<TItem, TResult>();
                AddItem(key, result, CacheObjType.RemotingData, sessionId, expiration);
            }

            return result;
        }

 
        /// <summary>
        /// Load item to cache from text file.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public object LoadTextFile(string source, int expiration)
        {
            return this.LoadItem(null, source, CacheObjType.TextFile, expiration);
        }
        /// <summary>
        ///  Load item to cache from xml file.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public object LoadXmlDocument(string source, int expiration)
        {
            return this.LoadItem(null, source, CacheObjType.XmlDocument, expiration);
        }
        /// <summary>
        ///  Load item to cache from image file.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public object LoadImage(string source, int expiration)
        {
            return this.LoadItem(null, source, CacheObjType.ImageFile, expiration);
        }
        /// <summary>
        ///  Load item to cache from file.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public object LoadItem(string source, CacheObjType type, int expiration)
        {
            return this.LoadItem(null, source, type, expiration);
        }
        
        #endregion

        #region On event virtual
        /// <summary>
        /// LogAction
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="text"></param>
        protected virtual void LogAction(CacheAction action, CacheActionState state, string text)
        {

        }
        /// <summary>
        /// LogAction
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="text"></param>
        /// <param name="args"></param>
        protected virtual void LogAction(CacheAction action, CacheActionState state, string text, params string[] args)
        {
        }
        /// <summary>
        /// On Cache Exception
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCacheException(CacheExceptionEventArgs e)
        {
            if (this.CacheException != null)
            {
                this.CacheException(this, e);
            }
        }

        internal void OnCacheException(string msg, CacheErrors err)
        {
            this.LogAction(CacheAction.CacheException, CacheActionState.Error, msg + " :{0}", new string[] { err.ToString() });
            if (this.CacheException != null)
            {
                this.CacheException(this, new CacheExceptionEventArgs(msg, err));
            }
        }
        /// <summary>
        /// On Cache State Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCacheStateChanged(EventArgs e)
        {
            if (this.CacheStateChanged != null)
            {
                this.CacheStateChanged(this, e);
            }
        }

        private void OnItemAdded(CacheEntry item)
        {
            this.LogAction(CacheAction.AddItem, CacheActionState.None, "cacheKey:" + item.Key + " size:{0}", item.Size.ToString());
            this.m_numItems++;
            this.OnItemAdded(new CacheEntryChangedEventArgs(CacheAction.AddItem, item.Key, item.Size));
        }
        /// <summary>
        /// On Item Added
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnItemAdded(CacheEntryChangedEventArgs e)
        {
          
        }
        /// <summary>
        /// On Item Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnItemChanged(CacheEntryChangedEventArgs e)
        {
            //RefreshSizeAsync();
        }

        private void OnItemChanged(string cacheKey, int size)
        {
            this.LogAction(CacheAction.ChangedItem, CacheActionState.None, cacheKey);
            this.OnItemChanged(new CacheEntryChangedEventArgs(CacheAction.ChangedItem, cacheKey, size));
        }

        private void OnItemRemoved(string cacheKey,int size, bool isTimeout)
        {
            this.LogAction(CacheAction.RemoveItem, CacheActionState.None, cacheKey);
            this.m_numItems--;
            this.OnItemRemoved(new CacheEntryChangedEventArgs(isTimeout ? CacheAction.TimeoutExpired : CacheAction.RemoveItem, cacheKey, size));
        }


        /// <summary>
        /// On Item Removed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnItemRemoved(CacheEntryChangedEventArgs e)
        {
           
        }
        /// <summary>
        /// On Load Remoting Data
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnLoadRemotingData(ref CacheEntry item)
        {
        }

        #endregion

        #region Cache methods
        /// <summary>
        /// Reset
        /// </summary>
        public virtual void Reset()
        {
            this.m_cacheList.Clear();
            this.m_Timer.Clear();
            this.m_numItems = 0;
            this.LogAction(CacheAction.ResetAll, CacheActionState.None, "");
        }

        /// <summary>
        /// Clear cache.
        /// </summary>
        public virtual void Clear()
        {

            this.m_cacheList.Clear();
            this.m_Timer.Clear();
            this.m_numItems = 0;
            this.LogAction(CacheAction.ClearCache, CacheActionState.None, "");
        }
        /// <summary>
        /// Determines whether the cache contains the specified <see cref="CacheEntry"/> item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(CacheEntry item)
        {
            return ((!this._disposed && (item != null)) && this.m_cacheList.ContainsKey(item.Key));
        }
        /// <summary>
        /// Determines whether the cache contains the specified cacheKey.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public bool Contains(string cacheKey)
        {
            return m_cacheList.ContainsKey(cacheKey);
        }
        /// <summary>
        /// Find Items in cache.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ICollection<CacheEntry> FindItems(EntryFindType findType, object key)
        {
            this.LogAction(CacheAction.General, CacheActionState.None, "Find Items");
            FindItemCallback<CacheEntry> caller = new FindItemCallback<CacheEntry>(FindItemsInternal);
            return AsyncFinder<CacheEntry>.Find(findType.ToString(), key, caller);
        }

        internal ICollection<CacheEntry> FindItemsInternal(TimeSpan timeout, string findType, object key)
        {
            List<CacheEntry> items = new List<CacheEntry>();

            switch (findType)
            {
                case "Key":
                    {
                        if (key == null)
                            goto Label_exit;

                        CacheEntry item = ViewItem(key.ToString());
                        if (item != null)
                        {
                            items.Add(item);
                        }
                    }
                    break;
                case "InKey":
                    {
                        if (key == null)
                            goto Label_exit;
                        DateTime startFind = DateTime.Now;
                        foreach (object o in Items)
                        {
                            CacheEntry item = (CacheEntry)o;
                            if (item.Key.Contains(key.ToString()))
                            {
                                items.Add(item);
                            }
                            Thread.Sleep(1);
                        }
                    }
                    break;
                case "SessionId":
                    {
                        if (key == null)
                            goto Label_exit;
                        string sessionId = key.ToString();

                        foreach (object o in Items)
                        {
                            CacheEntry item = (CacheEntry)o;

                            if (item.Id == sessionId)
                            {
                                items.Add(item);
                            }
                            Thread.Sleep(1);
                        }
                    }
                    break;
                case "Timeout":
                    {
                        foreach (object o in Items)
                        {
                            CacheEntry item = (CacheEntry)o;
                            if (item.IsTimeOut)
                            {
                                items.Add(item);
                            }
                            Thread.Sleep(1);
                        }
                    }
                    break;
                case "Type":
                    {
                        if (key == null)
                            goto Label_exit;
                        string type = key.ToString();

                        foreach (object o in Items)
                        {
                            CacheEntry item = (CacheEntry)o;

                            if (item.TypeName == type)
                            {
                                items.Add(item);
                            }
                            Thread.Sleep(1);
                        }
                    }
                    break;
            }

        Label_exit:
            return items;
        }

        internal void GcCollect()
        {
            try
            {

                GC.Collect();
                GC.GetTotalMemory(false);
                GC.Collect();
 
            }
            catch (Exception ex)
            {
                this.LogAction(CacheAction.General, CacheActionState.Error, "GcCollect, error:{0}", ex.Message);
            }
        }

        private string GetPathKey(string source)
        {
            if (source.ToLower().StartsWith("http"))
            {
                Uri uri = new Uri(source);
                return uri.AbsolutePath;
            }
            string pathRoot = Path.GetPathRoot(source);
            if (pathRoot != null)
            {
                return source.Substring(pathRoot.Length);
            }
            return source;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_cacheList.GetEnumerator();
        }

        /// <summary>
        /// Refresh Size Async and release un used memory.
        /// </summary>
        public virtual void TrimExess()
        {
            this.LogAction(CacheAction.General, CacheActionState.None, "Trim Exess");
           
            SizeRefresh();

        }

        #endregion

        #region Collection methods
        /// <summary>
        /// Clone items by <see cref="CloneType"/>  type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public CacheEntry[] CloneItems(CloneType type)
        {
            List<CacheEntry> is2 = new List<CacheEntry>();
            if (this.m_cacheList != null)
            {
                int count = this.m_cacheList.Count;
                foreach (CacheEntry item in this.m_cacheList.Values)
                {
                    switch (type)
                    {
                        case CloneType.Session:
                            if (!string.IsNullOrEmpty(item.Id))
                            {
                                is2.Add(item.Clone());
                            }
                            break;
                        case CloneType.Timeout:
                            if (item.IsTimeOut)
                            {
                                is2.Add(item.Clone());
                            }
                            break;
                        default:
                            is2.Add(item.Clone());
                            break;

                    }
                }
            }
            this.LogAction(CacheAction.General, CacheActionState.None, "Clone Items");
            return is2.ToArray();
        }

        /// <summary>
        /// Copy items.
        /// </summary>
        /// <param name="valueAswell"></param>
        /// <returns></returns>
        public CacheEntry[] CopyItems(bool valueAswell)
        {
            List<CacheEntry> is2 = new List<CacheEntry>();
            if (this.m_cacheList != null)
            {
                int count = this.m_cacheList.Count;
                foreach (CacheEntry item in this.m_cacheList.Values)
                {
                    is2.Add(item.Copy(valueAswell));
                }
            }
            this.LogAction(CacheAction.General, CacheActionState.None, "Clone Items");
            return is2.ToArray();
        }

        /// <summary>
        /// Get all keys.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllKeys()
        {
            string[] array = null;
            array = new string[this.m_cacheList.Keys.Count];
            this.m_cacheList.Keys.CopyTo(array, 0);
            return array;
        }
        /// <summary>
        /// Get all keys by icon.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllKeysIcons()
        {
            ICollection<CacheEntry> is2 = this.CloneItems(CloneType.All);
            string[] strArray = new string[is2.Count];
            int index = 0;
            foreach (CacheEntry item in is2)
            {
                strArray[index] = item.GetKeyIcon();
                index++;
            }
            return strArray;
        }
        #endregion

        #region Find items
        public IList<CacheEntry> FindItemsByArgsAsync(params string[] keyValuesArgs)
        {
            return Task.Factory.StartNew<IList<CacheEntry>>(() => FindItemsByArgs(keyValuesArgs)).Result;
        }
        public IList<CacheEntry> FindItemsByArgs(params string[] keyValuesArgs)
        {
            var args = CacheEntry.ArgsToDictionary(keyValuesArgs);

            IList<CacheEntry> values = (from CacheEntry dict in Items
                                        let key = dict.Key.ToString()
                                        let value = dict
                                        where dict.IsMatchArgs(args)
                                        select value).ToList();

            return values;
        }

        public IEnumerable<KeyValuePair<string, CacheEntry>> FindItemsAsync(string regexPattern)
        {
            return Task.Factory.StartNew<IEnumerable<KeyValuePair<string, CacheEntry>>>(() => FindItems(regexPattern)).Result;
        }
        public IEnumerable<KeyValuePair<string, CacheEntry>> FindItems(string regexPattern)
        {
            var list = m_cacheList.Where(k => System.Text.RegularExpressions.Regex.IsMatch(k.Key, regexPattern));
            return list;
        }
        public T FindFirstItemAsync<T>(string regexPattern)
        {
            return Task.Factory.StartNew<T>(() => FindFirstItem<T>(regexPattern)).Result;
        }
        public T FindFirstItem<T>(string regexPattern)
        {
            var item = m_cacheList.Where(k => System.Text.RegularExpressions.Regex.IsMatch(k.Key, regexPattern)).FirstOrDefault();
            if (item.Value == null)
                return default(T);
            return item.Value.GetValue<T>(); ;
        }
        public IEnumerable<object> FindValues(string regexPattern)
        {
            IEnumerable<object> values = (from CacheEntry dict in FindItemsAsync(regexPattern)
                                    let value = dict.GetValue()
                                    select value);

            
            //IList<object> values = (from CacheEntry dict in m_cacheList
            //                            let key = dict.Key.ToString()
            //                            let value = dict.GetValue()
            //                            where System.Text.RegularExpressions.Regex.IsMatch(key,regexPattern)
            //                            select value).ToList();

            return values;
        }
        public IList<CacheEntry> FindItemsAsync(string searchValue, bool searchStartsWith)
        {
            return Task.Factory.StartNew<IList<CacheEntry>>(() => FindItems(searchValue, searchStartsWith)).Result;
        }
        public IList<CacheEntry> FindItems(string searchValue, bool searchStartsWith)
        {
            IList<CacheEntry> values = (from CacheEntry dict in Items
                                        let key = dict.Key.ToString()
                                        let value = dict
                                        where searchStartsWith ? key.StartsWith(searchValue) : key.Contains(searchValue)
                                        select value).ToList();

            return values;
        }
        public IList<string> FindKeysAsync(string searchValue, bool searchStartsWith)
        {
            return Task.Factory.StartNew<IList<string>>(() => FindKeys(searchValue, searchStartsWith)).Result;
        }
        public IList<string> FindKeys(string searchValue, bool searchStartsWith)
        {
            IList<string> keys = (from CacheEntry dict in Items
                                  let key = dict.Key.ToString()
                                  where searchStartsWith ? key.StartsWith(searchValue) : key.Contains(searchValue)
                                  select key).ToList();

            return keys;
        }

        public void RemoveItems(string searchValue, bool searchStartsWith)
        {
            IList<string> keys = FindKeysAsync(searchValue, searchStartsWith);
            foreach (var key in keys)
            {
                RemoveItem(key);
            }
        }
 
        #endregion

        #region Properties

        /// <summary>
        /// Get cache sync state.
        /// </summary>
        public CacheSyncState CacheSyncState
        {
            get
            {
                return m_Timer.SyncState;
            }
        }
        /// <summary>
        /// Gets the number of items contained in the cache. 
        /// </summary>
        public int Count
        {
            get
            {
                return this.m_cacheList.Count;
            }
        }

        /// <summary>
        /// Get indicate whether the cache item is initialized.
        /// </summary>
        public bool Initialized
        {
            get
            {
                return m_Timer.Initialized;
            }
        }
        /// <summary>
        /// Get the sync interval in seconds.
        /// </summary>
        public int IntervalSeconds
        {
            get
            {
                return m_Timer.IntervalSeconds;
            }
        }


        /// <summary>
        /// Gets a collection containing the values in the cache.
        /// </summary>
        public ICollection<CacheEntry> Items
        {
            get
            {
                if (m_cacheList == null)
                {
                    return new List<CacheEntry>();
                }
                return this.m_cacheList.Values;

            }
        }
        /// <summary>
        /// Get the last sync time.
        /// </summary>
        public DateTime LastSyncTime
        {
            get
            {
                return m_Timer.LastSyncTime;
            }
        }

 
        /// <summary>
        /// Get MaxSize in bytes
        /// </summary>
        public long MaxSize
        {
            get
            {
                if (this._maxSize <= 0L)
                {
                    return CacheDefaults.DefaultCacheMaxSize;
                }
                return this._maxSize;
            }
        }
        /// <summary>
        /// Get the next sync time.
        /// </summary>
        public DateTime NextSyncTime
        {
            get
            {
                return m_Timer.NextSyncTime;
            }
        }
        /// <summary>
        /// Get or Set the default timeout for new item.
        /// </summary>
        public int Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                if (value >= 0)
                {
                    this._timeout = value;
                }
            }
        }

   
        /// <summary>
        /// Get indicate whether the cache item is remote cache.
        /// </summary>
        public bool IsRemote
        {
            get { return m_IsRemoteCache; }
        }

        #endregion

        #region Size properties

        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeValidate(int newSize)
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
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
            
        }
        /// <summary>
        /// Get memory usage.
        /// </summary>
        /// <returns></returns>
        protected long GetMemorySize()
        {
            long size = 0;
            ICollection<CacheEntry> items = m_cacheList.Values;
            foreach (var entry in items)
            {
                size += entry.Size;
            }
            return size;
        }
  
        #endregion

        #region class CacheMemoryPressure

        /// <summary>
        /// Represent Cache Memory Pressure.
        /// </summary>
        public class CacheMemoryPressure
        {
            // Methods
            internal static long GetPrivateBytes(bool nocache)
            {
                uint num2;
                uint pid = 0;
                uint privatePageCount = 0;
                GetProcessMemoryInformation(pid, out privatePageCount, out num2, nocache);
                return (long)(privatePageCount << 20);
            }

            [DllImport("webengine.dll")]
            internal static extern int GetProcessMemoryInformation(uint pid, out uint privatePageCount, out uint peakPagefileUsage, bool nocache);
            [DllImport("webengine.dll")]
            internal static extern void SetGCLastCalledTime(out int pfCall);
        }
        #endregion

        #region ExecuteTask
        /// <summary>
        /// Execute task using <see cref="TaskFactory"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public T ExecuteTask<T>(Func<T> action)
        {
            using (Task<T> task = Task.Factory.StartNew<T>(action))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    return task.Result;
                }
            }
            return default(T);
        }
        /// <summary>
        /// Execute task using <see cref="TaskFactory"/>.
        /// </summary>
        /// <param name="action"></param>
        public void ExecuteTask(Action action)
        {
            using (Task task = Task.Factory.StartNew(action))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                }
            }
        }
        #endregion

        #region Dynamic

        public bool EnableDynamicCache{get;set;}
        public string GetOrCreateJson<T>(string key, Func<T> function, int expirationMinutes = 0)
        {
            T o = GetOrCreate<T>(key, function, expirationMinutes);
            return o == null ? null : Nistec.Serialization.JsonSerializer.Serialize(o, true);
        }
        public string GetOrCreateJson<T>(string key, string[] args, Func<T> function, int expirationMinutes = 0)
        {
            T o = GetOrCreate<T>(key, args, function, expirationMinutes);
            return o == null ? null : Nistec.Serialization.JsonSerializer.Serialize(o, true);
        }
        public T GetOrCreate<T>(string key, Func<T> function, int expirationMinutes = 0)
        {
            T instance = default(T);
            if (EnableDynamicCache)
            {
                instance = GetValue<T>(key);
                if (instance == null)
                {
                    instance = function();

                    if (instance != null)
                    {
                        //Insert(key, instance, expirationMinutes);
                        AddItem(new CacheEntry(key, instance, null, expirationMinutes <= 0 ? Timeout : expirationMinutes, false));
                    }
                }
                else
                {
                    return instance;
                }
            }
            else
            {
                instance = function();
            }
            return instance;
        }

        public T GetOrCreate<T>(string key, Dictionary<string,string> args, Func<T> function, int expirationMinutes = 0)
        {
            T instance = default(T);
            if (EnableDynamicCache)
            {
                instance = GetValue<T>(key);
                if (instance == null)
                {
                    instance = function();

                    if (instance != null)
                    {
                        //Insert(key, instance, expirationMinutes);
                        var item = new CacheEntry(key, instance, null, expirationMinutes <= 0 ? Timeout : expirationMinutes, false);
                        item.Args = args;
                        AddItem(item);
                    }
                }
                else
                {
                    return instance;
                }
            }
            else
            {
                instance = function();
            }
            return instance;
        }
        public T GetOrCreate<T>(string key, string[] args, Func<T> function, int expirationMinutes = 0)
        {
            T instance = default(T);
            if (EnableDynamicCache)
            {
                instance = GetValue<T>(key);
                if (instance == null)
                {
                    instance = function();
                    if (instance != null)
                    {
                        //Insert(key, instance, expirationMinutes);
                        var item = new CacheEntry(key, instance, null, expirationMinutes <= 0 ? Timeout : expirationMinutes, false);
                        item.Args = CacheEntry.ArgsToDictionary(args);
                        AddItem(item);
                    }
                }
                else
                {
                    return instance;
                }
            }
            else
            {
                instance = function();
            }
            return instance;
        }

        //public void Insert(string key, object value, int expirationMinutes = 0)
        //{
        //    CacheEntry entry = new CacheEntry(key, value, null, expirationMinutes <= 0 ? Timeout : expirationMinutes, false);
        //    AddItem(entry);
        //}
        public class KeyBuilder
        {
            public static string Get(string LibName, string GroupName, int AccountId, int UserId, string EntityName)
            {
                return string.Format("^{0}_{1}_{2}_{3}_{4}$", (LibName != null) ? LibName : ".+", (GroupName != null) ? GroupName : ".+", (AccountId > 0) ? AccountId.ToString() : ".+", (UserId > 0) ? UserId.ToString() : ".+", (EntityName != null) ? EntityName : ".+").ToLower();
            }
            public static string AllAccountKeys(string LibName, int AccountId)
            {
                return string.Format("^{0}_.+_{1}_.+_.+$", LibName, AccountId).ToLower();
            }
            public static string AllAccountUserKeys(string LibName, int AccountId, int UserId)
            {
                return string.Format("^{0}_.+_{1}_{2}_.+$", LibName, AccountId, UserId).ToLower();
            }
            public static string AllAccountGroupKeys(string LibName, string GroupName, int AccountId)
            {
                return string.Format("^{0}_{1}_{2}_.+_.+$", LibName, GroupName, AccountId).ToLower();
            }
            public static string AllGroupKeys(string LibName, string GroupName)
            {
                return string.Format("^{0}_{1}_.+_.+_.+$", LibName, GroupName).ToLower();
            }
            public static string AllLibKeys(string LibName)
            {
                return string.Format("^{0}_.+_.+_.+_.+$", LibName).ToLower();
            }

        }
        #endregion

     }
}
 

 
