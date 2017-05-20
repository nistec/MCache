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
            : this(new CacheProperties(cacheName, 1000000L))
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
        internal CacheState AddItem(MessageStream message)
        {
            return this.AddItem(new CacheEntry(message));
        }

        internal AckStream AddItemWithAck(MessageStream message)
        {
            var state = this.AddItem(new CacheEntry(message));
            return CacheEntry.GetAckStream(state, message.Command);
        }

        

        /// <summary>
        /// Add new item to cache using <see cref="SessionEntry"/> argument.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual CacheState AddItem(SessionEntry item)
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

            return AddItem(entry);
        }

        #endregion

        #region Get item

        /// <summary>
        /// Get value from cache and return it as <see cref="NetStream"/> stream.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public AckStream GetValueStream(string cacheKey)
        {
            CacheEntry item = GetItem(cacheKey);
            if (item == null)// || item.IsStreamHasState())
            {
                return CacheEntry.GetAckNotFound("GetValueStream", cacheKey);
            }
            return item.GetAckStream();//.BodyStream;
        }
        /// <summary>
        /// Fetch value from cache and return it as <see cref="NetStream"/> stream.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public AckStream FetchValueStream(string cacheKey)
        {
            CacheEntry item = FetchItem(cacheKey);
            if (item == null)// || item.IsStreamHasState())
            {
                return CacheEntry.GetAckNotFound("FetchValueStream", cacheKey);
            }
            return item.GetAckStream();//item.BodyStream;
        }

        /// <summary>
        /// Get item properies from cache and return it as <see cref="NetStream"/> stream.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public AckStream ViewItemStream(string cacheKey)
        {
            CacheEntry item = ViewItem(cacheKey);
            if (item == null)// || item.IsStreamHasState())
            {
                return CacheEntry.GetAckNotFound("ViewItemStream", cacheKey);
            }
            return new AckStream(item);
        }

        /// <summary>
        /// Get properties and value from cache and return it as <see cref="NetStream"/> stream.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public AckStream GetItemStream(string cacheKey)
        {
            CacheEntry item = GetItem(cacheKey);
            if (item == null)
            {
                return CacheEntry.GetAckNotFound("GetItemStream", cacheKey);
            }
            return new AckStream(item);
        }
        /// <summary>
        /// Fetch properties and value from cache and return it as <see cref="NetStream"/> stream.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public AckStream FetchItemStream(string cacheKey)
        {
            CacheEntry item = FetchItem(cacheKey);
            if (item == null)// || item.IsStreamHasState())
            {
                return CacheEntry.GetAckNotFound("FetchItemStream", cacheKey);
            }
            return new AckStream(item);
        }
        /// <summary>
        /// Read item <see cref="CacheEntry"/> properties from cache to stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cacheKey"></param>
        public void ReadItemView(Stream stream, string cacheKey)
        {
            byte[] b = ViewItemStream(cacheKey).ToArray();
            if (b == null)
                return;
            stream.Write(b, 0, b.Length);
        }
        
      
        #endregion

        #region Merge 

        /// <summary>
        /// Remove item from cache and return <see cref="CacheState"/> as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal AckStream RemoveItem(MessageStream message)
        {
            bool ok = RemoveItem(message.Key);
            //CacheState state = ok ? CacheState.ItemRemoved : CacheState.RemoveItemFailed;
            //return MessageAck.DoAck<int>((int)state);
            return CacheEntry.GetAckStream(ok, "RemoveItem");
        }
        /// <summary>
        /// Merge item with a new collection items, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/> as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal AckStream MergeItem(MessageStream message)
        {
            object val = message.DecodeBody();
            CacheState res= MergeItem(message.Key, val);
            //return MessageAck.DoAck<int>(res);
            return CacheEntry.GetAckStream(res, "MergeItem");
        }

        /// <summary>
        /// Merge item by remove all items from the current item that match the collection items in argument, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/> as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal AckStream MergeRemoveItem(MessageStream message)
        {
            object val = message.DecodeBody();
            CacheState res = MergeRemoveItem(message.Key, val);
            return CacheEntry.GetAckStream(res == CacheState.ItemChanged, "MergeRemoveItem");
        }

        #endregion

        #region Load item
        public AckStream LoadData(MessageStream message)
        {
            object result = null;
            CommandContext command = new CommandContext(message.BodyStream);
            string key = command.CreateKey(true);
            result = this.GetValue(key);

            if (result == null)
            {
                string sessionId=message.Id;
                int expiration = message.Expiration;

                result = command.Exec();
                AddItem(key, result, CacheObjType.RemotingData, sessionId, expiration);
            }

            if (result == null)
                return AckStream.GetAckStream(false, "LoadData.CommandText: " + command.CommandText);

            return new AckStream(result);

        }

        #endregion

        #region Session methods

        /// <summary>
        /// Remove all items from cache that belong to specified session.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal virtual AckStream RemoveCacheSessionItemsAsync(MessageStream message)
        {
            int res= RemoveCacheSessionItems(message.Key);
            return CacheEntry.GetAckStream(res >= 0, "RemoveCacheSessionItems");
        }
        /// <summary>
        /// Remove all items from cache that belong to specified sessionId.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual int RemoveCacheSessionItemsAsync(string sessionId)
        {
            return ExecuteTask<int>(() => RemoveCacheSessionItems(sessionId));
        }

        int RemoveCacheSessionItems(string sessionId)
        {
            int count = 0;
            try
            {
                ICollection<string> items = GetCacheSessionKeys(sessionId);
                if (items == null || items.Count == 0)
                    return 0;
                foreach (string item in items)
                {
                    RemoveItemAsync(item);
                    count++;
                }
                
                //RefreshSize();

                SizeRefresh();
            }
            catch (Exception ex)
            {
                count = -1;
                this.LogAction(CacheAction.CacheException, CacheActionState.Error, "Error RemoveSession:{0}", ex.Message);
            }
            return count;
        }

        string[] GetCacheSessionKeys(string sessionId)
        {
            string[] keys = null;
            if (this.m_cacheList != null)
            {
                 IEnumerable<string> k = from n in m_cacheList.Values.Cast<CacheEntry>() where n.Id == sessionId select n.Key;
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
        /// <summary>
        /// Get memory usage.
        /// </summary>
        /// <returns></returns>
        public long GetMemoryUsage()
        {
            return CachePerformanceCounter.GetMemoryUsage();
        }

        #endregion

    }
}
 

 
