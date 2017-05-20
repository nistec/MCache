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
using System.IO;
using Nistec.Channels;
using Nistec.Generic;
using System.Collections;
using Nistec.Runtime;
using Nistec.Data.Entities;
using System.IO.Pipes;
using Nistec.Caching.Channels;
using Nistec.IO;
using Nistec.Caching.Config;
using Nistec.Serialization;

namespace Nistec.Caching.Remote
{
    /// <summary>
    /// Represent Cache Api for client.
    /// </summary>
    public class CacheApi
    {
        NetProtocol Protocol;
        /// <summary>
        /// Get cache api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static CacheApi Get(NetProtocol protocol= NetProtocol.Tcp)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = CacheApiSettings.Protocol;
            }
            return new CacheApi() {Protocol=protocol };
        }

        string RemoteCacheHostName;
        bool EnableRemoteException;

        private CacheApi()
        {
            RemoteCacheHostName = CacheApiSettings.RemoteCacheHostName;
            EnableRemoteException = CacheApiSettings.EnableRemoteException;
        }

        /// <summary>
        /// CConvert stream to json format.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToJson(NetStream stream, JsonFormat format)
        {
            using (BinaryStreamer streamer = new BinaryStreamer(stream))
            {
                var obj = streamer.Decode();
                if (obj == null)
                    return null;
                else
                    return JsonSerializer.ToJson(obj, null, format);
            }
        }

        /// <summary>
        /// Get item value from cache as json.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetJson(string cacheKey, JsonFormat format)
        {
            var obj = GetValue(cacheKey);
            if (obj == null)
                return null;
            return JsonSerializer.ToJson(obj, null, format);
        }


        #region items
        /// <summary>
        /// Reply for test
        /// </summary>
        /// <returns></returns>
        public string Reply()
        {

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex<string>(new CacheMessage() { Command = CacheCmd.Reply, Key = "Reply" },
                    RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex<string>(new CacheMessage() { Command = CacheCmd.Reply, Key = "Reply" },
                    RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex<string>(new CacheMessage() { Command = CacheCmd.Reply, Key = "Reply" },
               RemoteCacheHostName, EnableRemoteException);

        }
        
       
       /// <summary>
       /// Remove item from cache
       /// </summary>
       /// <param name="cacheKey"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        /// <example>
        /// <code>
        /// //Remove item from cache.
        ///public void RemoveItem()
        ///{
        ///    var state = CacheApi.RemoveItem("item key 3");
        ///    Console.WriteLine(state);
        ///}
        /// </code>
        /// </example>
        public CacheState RemoveItem(string cacheKey)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return (CacheState)HttpClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.RemoveItem, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return (CacheState)PipeClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.RemoveItem, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return (CacheState)TcpClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.RemoveItem, Key = cacheKey }, 
                RemoteCacheHostName, EnableRemoteException);
        }

        /// <summary>
        /// Remove item from cache asynchronizly
        /// </summary>
        /// <param name="cacheKey"></param>
        public void RemoveItemAsync(string cacheKey)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:

                    HttpClientCache.SendOut(new CacheMessage() { Command = CacheCmd.RemoveItemAsync, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);
                    return;
                case NetProtocol.Pipe:

                    PipeClientCache.SendOut(new CacheMessage() { Command = CacheCmd.RemoveItemAsync, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);
                    return;
                case NetProtocol.Tcp:
                    break;
            }

            TcpClientCache.SendOut(new CacheMessage() { Command = CacheCmd.RemoveItemAsync, Key = cacheKey }, 
                RemoteCacheHostName, EnableRemoteException);
        }

        /// <summary>
        /// Get value from cache as <see cref="NetStream"/>
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// //Get item value from cache.
        ///public void GetStream()
        ///{
        ///    string key = "item key 1";
        ///    <![CDATA[var item = CacheApi.GetStream(key);]]>
        ///    Print(item, key);
        ///}
        /// </code>
        /// </example>
        public NetStream GetStream(string cacheKey)
        {
            return TcpClientCache.SendDuplex<NetStream>(new CacheMessage() { Command = CacheCmd.GetValue, Key = cacheKey },
                RemoteCacheHostName, EnableRemoteException);
        }
        /// <summary>
        /// Get value from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// //Get item value from cache.
        ///public void GetValue()
        ///{
        ///    string key = "item key 1";
        ///    <![CDATA[var item = CacheApi.GetValue<EntitySample>(key);]]>
        ///    Print(item, key);
        ///}
        /// </code>
        /// </example>
        public T GetValue<T>(string cacheKey)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex<T>(new CacheMessage() { Command = CacheCmd.GetValue, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex<T>(new CacheMessage() { Command = CacheCmd.GetValue, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex<T>(new CacheMessage() { Command = CacheCmd.GetValue, Key = cacheKey }, 
                RemoteCacheHostName, EnableRemoteException);
        }

        /// <summary>
        /// Get value from cache.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public object GetValue(string cacheKey)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex(new CacheMessage() { Command = CacheCmd.GetValue, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex(new CacheMessage() { Command = CacheCmd.GetValue, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex(new CacheMessage() { Command = CacheCmd.GetValue, Key = cacheKey },
                RemoteCacheHostName, EnableRemoteException);
        }

        /// <summary>
        /// Fetch Value from cache (Cut item from cache)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// //Fetch item value from cache.
        ///public void FetchValue()
        ///{
        ///    string key = "item key 2";
        ///    <![CDATA[var item = CacheApi.FetchValue<EntitySample>(key);]]>
        ///    Print(item, key);
        ///}
        /// </code>
        /// </example>
        public T FetchValue<T>(string cacheKey)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex<T>(new CacheMessage() { Command = CacheCmd.FetchValue, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex<T>(new CacheMessage() { Command = CacheCmd.FetchValue, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex<T>(new CacheMessage() { Command = CacheCmd.FetchValue, Key = cacheKey }, 
                RemoteCacheHostName, EnableRemoteException);
        }
        /// <summary>
        /// Add new item to cache
        /// </summary>
        /// <param name="item"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        public CacheState AddItem(CacheEntry item)
        {
            if (item == null || item.IsEmpty)
                return CacheState.ArgumentsError;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return (CacheState)HttpClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.AddItem, Key = item.Key, BodyStream = item.BodyStream },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return (CacheState)PipeClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.AddItem, Key = item.Key, BodyStream = item.BodyStream },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return (CacheState)TcpClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.AddItem, Key = item.Key, BodyStream = item.BodyStream }, 
                RemoteCacheHostName, EnableRemoteException);
        }
        /// <summary>
        /// Add new item to cache
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        /// <example>
        /// <code>
        /// //Add items to remote cache.
        ///public void AddItems()
        ///{
        ///    int timeout = 30;
        ///    CacheApi.AddItem("item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
        ///    CacheApi.AddItem("item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
        ///    CacheApi.AddItem("item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
        ///}
        /// </code>
        /// </example>
        public CacheState AddItem(string cacheKey, object value, int expiration)
        {
            if (value == null)
                return CacheState.ArgumentsError;
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return (CacheState)HttpClientCache.SendDuplex<int>(new CacheMessage(CacheCmd.AddItem, cacheKey, value, expiration),
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return (CacheState)PipeClientCache.SendDuplex<int>(new CacheMessage(CacheCmd.AddItem, cacheKey, value, expiration),
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return (CacheState)TcpClientCache.SendDuplex<int>(new CacheMessage(CacheCmd.AddItem, cacheKey, value, expiration), 
                RemoteCacheHostName, EnableRemoteException);
        }
        /// <summary>
        /// Add new item to cache
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="sessionId"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        public CacheState AddItem(string cacheKey, object value, string sessionId, int expiration)
        {
            if (value == null)
                return CacheState.ArgumentsError;
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return (CacheState)HttpClientCache.SendDuplex<int>(new CacheMessage(CacheCmd.AddItem, cacheKey, value, expiration, sessionId),
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return (CacheState)PipeClientCache.SendDuplex<int>(new CacheMessage(CacheCmd.AddItem, cacheKey, value, expiration, sessionId),
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return (CacheState)TcpClientCache.SendDuplex<int>(new CacheMessage(CacheCmd.AddItem, cacheKey, value, expiration, sessionId), 
                RemoteCacheHostName, EnableRemoteException);
        }
        /// <summary>
        /// Get item copy from cache
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns>return <see cref="CacheEntry"/></returns>
        public CacheEntry ViewItem(string cacheKey)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex<CacheEntry>(new CacheMessage() { Command = CacheCmd.ViewItem, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex<CacheEntry>(new CacheMessage() { Command = CacheCmd.ViewItem, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex<CacheEntry>(new CacheMessage() { Command = CacheCmd.ViewItem, Key = cacheKey }, 
                RemoteCacheHostName, EnableRemoteException);
        }

        #endregion

        #region Copy and merge
        /// <summary>
        /// Copy item in cache from source to another destination.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        /// <example>
        /// <code>
        /// //Duplicate existing item from cache to a new destination.
        ///public void CopyItem()
        ///{
        ///    string source = "item key 1";
        ///    string dest = "item key 2";
        ///    var state = CacheApi.CopyItem(source, dest, timeout);
        ///    Console.WriteLine(state);
        ///}
        /// </code>
        /// </example>
        public CacheState CopyItem(string source, string dest, int expiration)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return (CacheState)HttpClientCache.SendDuplex<int>(
                        new CacheMessage()
                        {
                            Command = CacheCmd.CopyItem,
                            Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Destination, dest),
                            Expiration = expiration,
                            IsDuplex = false,
                            Key = dest
                        }, RemoteCacheHostName, EnableRemoteException
                   );

                case NetProtocol.Pipe:
                    return (CacheState)PipeClientCache.SendDuplex<int>(
                        new CacheMessage()
                        {
                            Command = CacheCmd.CopyItem,
                            Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Destination, dest),
                            Expiration = expiration,
                            IsDuplex = false,
                            Key = dest
                        }, RemoteCacheHostName, EnableRemoteException
                   );

                case NetProtocol.Tcp:
                    break;
            }
            return (CacheState)TcpClientCache.SendDuplex<int>(
                new CacheMessage()
                {
                    Command = CacheCmd.CopyItem,
                    Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Destination, dest),
                    Expiration = expiration,
                    IsDuplex = false,
                    Key = dest
                }, RemoteCacheHostName, EnableRemoteException
           );

        }
        /// <summary>
        /// Cut item in cache from source to another destination.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        /// <example>
        /// <code>
        /// //Duplicate existing item from cache to a new destination and remove the old one.
        ///public void CutItem()
        ///{
        ///    string source = "item key 2";
        ///    string dest = "item key 3";
        ///    var state = CacheApi.CutItem(source, dest, timeout);
        ///    Console.WriteLine(state);
        ///}
        /// </code>
        /// </example>
        public CacheState CutItem(string source, string dest, int expiration)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return (CacheState)HttpClientCache.SendDuplex<int>(
                       new CacheMessage()
                       {
                           Command = CacheCmd.CutItem,
                           Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Destination, dest),
                           Expiration = expiration,
                           IsDuplex = false,
                           Key = dest
                       }, RemoteCacheHostName, EnableRemoteException
                  );

                case NetProtocol.Pipe:
                    return (CacheState)PipeClientCache.SendDuplex<int>(
                       new CacheMessage()
                       {
                           Command = CacheCmd.CutItem,
                           Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Destination, dest),
                           Expiration = expiration,
                           IsDuplex = false,
                           Key = dest
                       }, RemoteCacheHostName, EnableRemoteException
                  );

                case NetProtocol.Tcp:
                    break;
            }
            return (CacheState)TcpClientCache.SendDuplex<int>(
               new CacheMessage()
               {
                   Command = CacheCmd.CutItem,
                   Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Destination, dest),
                   Expiration = expiration,
                   IsDuplex = false,
                   Key = dest
               }, RemoteCacheHostName, EnableRemoteException
          );
        }
    
        #endregion

        /// <summary>
        /// Remove all session items from cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>return number of items removed from cache.</returns>
        public int RemoveCacheSessionItems(string sessionId)
        {
            if (sessionId == null)
                return -1;
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.RemoveCacheSessionItems, Key = sessionId },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.RemoveCacheSessionItems, Key = sessionId },
                        RemoteCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.RemoveCacheSessionItems, Key = sessionId }, 
                RemoteCacheHostName, EnableRemoteException);
        }
        /// <summary>
        /// Keep Alive Cache Item.
        /// </summary>
        /// <param name="cacheKey"></param>
        public void KeepAliveItem(string cacheKey)
        {
            if (cacheKey == null)
                return;
            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClientCache.SendOut(new CacheMessage() { Command = CacheCmd.KeepAliveItem, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);
                    return;
                case NetProtocol.Pipe:
                    PipeClientCache.SendOut(new CacheMessage() { Command = CacheCmd.KeepAliveItem, Key = cacheKey },
                        RemoteCacheHostName, EnableRemoteException);
                    return;
                case NetProtocol.Tcp:
                    break;
            }
            TcpClientCache.SendOut(new CacheMessage() { Command = CacheCmd.KeepAliveItem, Key = cacheKey }, 
                RemoteCacheHostName, EnableRemoteException);
        }

        #region internal


        #endregion
    }
}
