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
using Nistec.IO;
using Nistec.Channels;
using Nistec.Caching.Session;
using Nistec.Caching.Config;
using Nistec.Data;
using Nistec.Caching.Server;

namespace Nistec.Caching
{
 
    /// <summary>
    /// Represent remote memory cache include hash functionality.
    /// </summary>
    [Serializable]
    public class MCache : MemoCache,/*ICacheView*/ IDisposable
    {
        

        #region ctor and members
        /// <summary>
        /// Constructor <see cref="CacheProperties"/> argument.
        /// </summary>
        /// <param name="prop"></param>
        public MCache(CacheProperties prop)
            : base(prop, true)
        {
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized MCache");
        }
        /// <summary>
        /// DEfault constructor
        /// </summary>
        /// <param name="cacheName"></param>
        public MCache(string cacheName)
            : this(new CacheProperties(cacheName, CacheSettings.MaxSize))
        {
        }
       
      
        /// <summary>
        /// Destructor.
        /// </summary>
        ~MCache()
        {
            Dispose(false);
        }

        #endregion

        #region Add item

        /// <summary>
        /// Add new item to cache using <see cref="MessageStream"/> argument.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal CacheState Add(MessageStream message)
        {
            return this.Add(new CacheEntry(message));
        }

        /// <summary>
        /// Add new item to cache using <see cref="SessionEntry"/> argument.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual CacheState Add(SessionEntry item)
        {
            if (this._disposed)
            {
                return CacheState.CacheNotReady;
            }

            if (item == null)
            {
                return CacheState.InvalidItem;
            }

            CacheEntry entry = new CacheEntry(item, this.m_IsRemoteCache);

            return Add(entry);
        }

        #endregion

        #region Set item
        /// <summary>
        /// Add new item to cache using <see cref="MessageStream"/> argument.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal CacheState Set(MessageStream message)
        {
            return this.Set(new CacheEntry(message));
        }

      
        /// <summary>
        /// Add new item to cache using <see cref="SessionEntry"/> argument.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual CacheState Set(SessionEntry item)
        {
            if (this._disposed)
            {
                return CacheState.CacheNotReady;
            }

            if (item == null)
            {
                return CacheState.InvalidItem;
            }

            CacheEntry entry = new CacheEntry(item, this.m_IsRemoteCache);

            return Set(entry);
        }

        #endregion

        #region Get item

        /// <summary>
        /// Get value from cache and return it as <see cref="IDictionary"/> stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IDictionary<string,object> GetRecord(MessageStream message)
        {
            CacheEntry item = GetItem(message.Id);
            if (item == null)
            {
                return null;
            }
            return item.ToDictionary();
        }

        /// <summary>
        /// Get value from cache and return it as <see cref="NetStream"/> stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream GetValueStream(MessageStream message)
        {
            CacheEntry item = GetItem(message.Id);
            if (item == null)
            {
                return null;
            }
            return item.GetStream();
        }
        /// <summary>
        /// Fetch value from cache and return it as <see cref="NetStream"/> stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream FetchValueStream(MessageStream message)
        {
            CacheEntry item = FetchItem(message.Id);
            if (item == null)
            {
                return null;
            }
            return item.GetStream();
        }

        ///// <summary>
        ///// Get item properies from cache and return it as <see cref="NetStream"/> stream.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public TransStream ViewItemStream(MessageStream message)
        //{
        //    CacheEntry item = ViewItem(message.Id);
        //    if (item == null)
        //    {
        //        return new TransStream(CacheState.NotFound.ToString(), TransType.Error);
        //    }
        //    return new TransStream(item, TransStream.ToTransType(message.TransformType));
        //}

        ///// <summary>
        ///// Get properties and value from cache and return it as <see cref="NetStream"/> stream.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public TransStream GetItemStream(MessageStream message)
        //{
        //    CacheEntry item = GetItem(message.Id);
        //    if (item == null)
        //    {
        //        return null;// new TransStream(CacheState.NotFound.ToString(), TransType.Error);
        //    }
        //    return new TransStream(item, TransStream.ToTransType(message.TransformType));
        //}

        ///// <summary>
        ///// Fetch properties and value from cache and return it as <see cref="NetStream"/> stream.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public TransStream FetchItemStream(MessageStream message)
        //{
        //    CacheEntry item = FetchItem(message.Id);
        //    if (item == null)
        //    {
        //        return new TransStream(CacheState.NotFound.ToString(), TransType.Error);
        //    }
        //    return new TransStream(item, TransStream.ToTransType(message.TransformType));
        //}

        ///// <summary>
        ///// Read item <see cref="CacheEntry"/> properties from cache to stream.
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="message"></param>
        //public void ReadItemView(Stream stream, MessageStream message)
        //{
        //    byte[] b = ViewItemStream(message).ToArray();
        //    if (b == null)
        //        return;
        //    stream.Write(b, 0, b.Length);
        //}


        #endregion

        #region Merge 

        /// <summary>
        /// Remove item from cache and return <see cref="CacheState"/> as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal CacheState RemoveItemAsync(MessageStream message)
        {
            bool ok = RemoveAsync(message.Id);
            return ok ? CacheState.ItemRemoved : CacheState.RemoveItemFailed;
            //return new TransStream((int)state, TransType.State);
        }

        /// <summary>
        /// Remove item from cache and return <see cref="CacheState"/> as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal CacheState RemoveItem(MessageStream message)
        {
            bool ok = Remove(message.Id);
            return ok ? CacheState.ItemRemoved : CacheState.RemoveItemFailed;
            //return new TransStream((int)state, TransType.State);
        }
        /// <summary>
        /// Merge item with a new collection items, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/> as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal TransStream MergeItem(MessageStream message)
        {
            object val = message.DecodeBody();
            CacheState state= MergeItem(message.Id, val);
            //return TransStream.Write((int)state, state.ToString());//TransType.State);
            return TransStream.WriteState((int)state, state.ToString());
        }

        /// <summary>
        /// Merge item by remove all items from the current item that match the collection items in argument, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/> as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal TransStream MergeRemoveItem(MessageStream message)
        {
            object val = message.DecodeBody();
            CacheState state = MergeRemoveItem(message.Id, val);
            //return TransStream.Write((int)state, state.ToString());//TransType.State);
            return TransStream.WriteState((int)state, state.ToString());
        }

        #endregion

        #region Load item

        public object LoadData(MessageStream message)
        {
            object result = null;
            CommandContext command = new CommandContext(message.GetStream());
            string key = command.CreateKey(true);
            result = this.GetValueStream(message);

            if (result == null)
            {
                string sessionId=message.GroupId;
                int expiration = message.Expiration;

                result = command.Exec();
                Set(key, result, sessionId, expiration);
            }

            //if (result == null)
            //    return TransStream.GetAckStream("LoadData.CommandText: " + command.CommandText, false);//AckStream.GetAckStream(false, "LoadData.CommandText: " + command.CommandText);

            return result;// new TransStream(result, TransType.Object);

        }

        #endregion

        #region Session methods

      
        /// <summary>
        /// Remove all items from cache that belong to specified sessionId.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual CacheState RemoveCacheSessionItemsAsync(string sessionId)
        {
            return ExecuteTask<CacheState>(() => RemoveCacheSessionItems(sessionId));
        }

        CacheState RemoveCacheSessionItems(string sessionId)
        {
            int count = 0;
            try
            {
                ICollection<string> items = GetCacheSessionKeys(sessionId);
                if (items == null || items.Count == 0)
                    return 0;
                foreach (string item in items)
                {
                    RemoveAsync(item);
                    count++;
                }
                
                //RefreshSize();

                SizeRefresh();
            }
            catch (Exception ex)
            {
                count = -1;
                this.LogAction(CacheAction.CacheException, CacheActionState.Error, "Error RemoveSession:{0}", ex.Message);
                return CacheState.UnexpectedError;
            }
            return count > 0 ? CacheState.ItemRemoved : CacheState.RemoveItemFailed; 
        }

        string[] GetCacheSessionKeys(string sessionId)
        {
            string[] keys = null;
            if (this.m_cacheList != null)
            {
                 IEnumerable<string> k = from n in m_cacheList.Values.Cast<CacheEntry>() where n.GroupId == sessionId select n.Id;
                if (k != null)
                {
                    keys = k.ToArray();
                }
            }

            this.LogAction(CacheAction.General, CacheActionState.None, "Clone Keys by session :" + sessionId);
            return keys;
        }

        #endregion

        #region Cache methods

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

        private byte[] Serialize(Hashtable h)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Position = 0L;
                formatter.Serialize(stream, h);
                return stream.ToArray();
            }
        }


        #endregion

        #region size exchange
        /*
        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected override CacheState SizeValidate(int newSize)
        {
            return base.SizeValidate(newSize);
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
        internal protected override CacheState SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            return base.SizeExchage(currentSize, newSize, currentCount,newCount ,exchange);
        }
        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected override void SizeRefresh()
        {
            base.SizeRefresh();
        }
        */

        /// <summary>
        /// Get memory usage.
        /// </summary>
        /// <returns></returns>
        public long GetMemoryUsage()
        {
            return CachePerformanceCounter.GetMemoryUsage();
        }

        #endregion

        #region Report

        /// <summary>
        /// Save all cache item to <see cref="DataSet"/>.
        /// </summary>
        /// <returns></returns>
        public DataTable CacheReport(bool noBody)
        {
            DataTable table;
            this.LogAction(CacheAction.General, CacheActionState.None, "CacheReport");
            try
            {
                ICollection<CacheEntry> items = this.Items;
                if ((items == null) || (items.Count == 0))
                {
                    return null;
                }
                table = CacheEntry.CacheItemSchema();
                foreach (CacheEntry item in items)
                {
                    table.Rows.Add(item.ToDataRow(noBody));
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return table;
        }
        internal CacheItemReport GetReport()
        {
            var data = CacheReport(true);
            if (data == null)
                return null;
            return new CacheItemReport() { Count = data.Rows.Count, Data = data, Name = "Cache Report", Size = 0 };
        }

        internal CacheItemReport GetTimerReport()
        {
            return m_Timer.GetReport("Cache");
        }
        #endregion
    }
}
 

 
