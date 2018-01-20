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
//using Nistec.Caching.Channels;
using Nistec.IO;
using Nistec.Caching.Config;
using Nistec.Serialization;
using Nistec.Data;
using System.Data;

namespace Nistec.Caching.Remote
{
    /// <summary>
    /// Represent Cache Api for client.
    /// </summary>
    public class CacheApi : RemoteApi
    {
        /// <summary>
        /// Get cache api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public static CacheApi Get(NetProtocol protocol= NetProtocol.Tcp, int expiration = CacheDefaults.DefaultCacheExpiration)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = CacheApiSettings.Protocol;
            }
            return new CacheApi() {Protocol=protocol, CacheExpiration = expiration };
        }


        private CacheApi()
        {
            RemoteHostName = CacheDefaults.DefaultBundleHostName;
            EnableRemoteException = CacheApiSettings.EnableRemoteException;
        }

        protected void OnFault(string message)
        {
            Console.WriteLine("CacheApi Fault: " + message);
        }

        

        int _CacheExpiration;
        public int CacheExpiration
        {
            get { return _CacheExpiration;}
            set { _CacheExpiration = (value < 0) ? CacheDefaults.DefaultCacheExpiration : value; }
        }

        #region do custom
        public object DoCustom(string command, string key, string groupId, string label = null, object value = null, int expiration = 0)
        {
            switch ("cach_" + command)
            {
                case CacheCmd.Add:
                    return Add(key, value, expiration);
                case CacheCmd.CopyTo:
                    return CopyTo(label, key, expiration);
                case CacheCmd.CutTo:
                    return CutTo(label, key, expiration);
                case CacheCmd.Fetch:
                    return Fetch(key);
                case CacheCmd.Get:
                    return Get(key);
                case CacheCmd.GetEntry:
                    return GetEntry(key);
                case CacheCmd.GetRecord:
                    return GetRecord(key);
                case CacheCmd.KeepAliveItem:
                    KeepAliveItem(key);
                    return CacheState.Ok;
                //case CacheCmd.LoadData:
                //    return LoadData();
                case CacheCmd.Remove:
                    return Remove(key);
                case CacheCmd.RemoveAsync:
                    RemoveAsync(key);
                    return CacheState.Ok;
                case CacheCmd.RemoveItemsBySession:
                    return RemoveItemsBySession(key);
                case CacheCmd.Reply:
                    return Reply(key);
                case CacheCmd.Set:
                    return Set(key, value, expiration);
                case CacheCmd.ViewEntry:
                    return ViewEntry(key);
                default:
                    throw new ArgumentException("Unknown command " + command);
            }
        }

        public string DoHttpJson(string command, string key, string groupId=null, string label=null, object value = null, int expiration = 0, bool pretty=false)
        {
            string cmd = "cach_" + command.ToLower();
            switch (cmd)
            {
                case CacheCmd.Add:
                    //return Add(key, value, expiration);
                    {
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            throw new ArgumentNullException("key is required");
                        }
                        var msg = new CacheMessage() { Command = cmd, Id = key, GroupId = groupId, Expiration = expiration };
                        msg.SetBody(value);
                        return SendHttpJsonDuplex(msg, pretty);
                    }
                case CacheCmd.CopyTo:
                    //return CopyTo(key, detail, expiration);
                case CacheCmd.CutTo:
                    //return CutTo(key, detail, expiration);
                    {
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            throw new ArgumentNullException("key is required");
                        }
                        var msg = new CacheMessage() { Command = cmd, Id = key, GroupId= groupId, Expiration=expiration };
                        msg.SetBody(value);
                        msg.Args = MessageStream.CreateArgs(KnowsArgs.Source, label, KnowsArgs.Destination, key);
                        return SendHttpJsonDuplex(msg, pretty);
                    }
                case CacheCmd.Fetch:
                case CacheCmd.Get:
                case CacheCmd.GetEntry:
                case CacheCmd.GetRecord:
                case CacheCmd.RemoveItemsBySession:
                case CacheCmd.Reply:
                case CacheCmd.Remove:
                case CacheCmd.ViewEntry:
                    {
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            throw new ArgumentNullException("key is required");
                        }
                        return SendHttpJsonDuplex(new CacheMessage() {Command=cmd,Id=key }, pretty);
                    }
                case CacheCmd.KeepAliveItem:
                case CacheCmd.RemoveAsync:
                    {
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            throw new ArgumentNullException("key is required");
                        }
                        SendHttpJsonOut(new CacheMessage() { Command = cmd, Id = key });
                        return CacheState.Ok.ToString();
                    }

                //case CacheCmd.LoadData:
                //    return LoadData();
                case CacheCmd.Set:
                    //return Set(key, value, expiration);
                    {
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            throw new ArgumentNullException("key is required");
                        }

                        if (value == null)
                        {
                            throw new ArgumentNullException("value is required");
                        }
                        var message = new CacheMessage(cmd, key, value, expiration);
                        return SendHttpJsonDuplex(message, pretty);
                    }
                default:
                    throw new ArgumentException("Unknown command " + command);
            }
        }
        #endregion

        /// <summary>
        /// Get or Set cache value by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value, CacheExpiration); }
        }

        /// <summary>
        /// Get item value from cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            var message = new CacheMessage() { Command = CacheCmd.Get, Id = key };
            return SendDuplexStream<T>(message, OnFault);

        }

        /// <summary>
        /// Get item value from cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T Get<T>(string key, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            var message = new CacheMessage() { Command = CacheCmd.Get, Id = key};
            var ts = SendDuplexStream(message);
            if (ts == null)
                return defaultValue;
            return TransReader.ReadValue<T>(ts.GetStream(), defaultValue);
        }

        /// <summary>
        /// Get item value from cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            var message = new CacheMessage() { Command = CacheCmd.Get, Id = key };
            var ts = SendDuplexStream(message);
            if (ts == null)
            {
                OnFault("Get: " + key + " return null!");
            }
            return ts.ReadValue(OnFault);
        }

        /// <summary>
        /// Get item value as dictionary from cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IDictionary<string, object> GetRecord(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            var message = new CacheMessage() { Command = CacheCmd.GetRecord, Id = key };
            var ts = SendDuplexStream(message);
            if (ts == null)
            {
                return null;
            }
            return ts.ReadValue<IDictionary<string,object>>(OnFault);

        }
        /// <summary>
        /// Get item value from cache as json.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetJson(string key, JsonFormat format)
        {
            var obj = Get(key);
            if (obj == null)
                return null;
            return JsonSerializer.Serialize(obj, null, format);
        }



        #region items
        /// <summary>
        /// Reply for test
        /// </summary>
        /// <returns></returns>
        public string Reply(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text is required");
            }
            var message = new CacheMessage() { Command = CacheCmd.Reply ,Id = text };
            return SendDuplexStream<string>(message,OnFault);
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
        public CacheState Remove(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentNullException("cacheKey is required");
            }
            var message = new CacheMessage() { Command = CacheCmd.Remove, Id = cacheKey };
            var ts = SendDuplexStream(message);
            if(ts==null)
            {
                return CacheState.UnKnown;
            }
            return (CacheState)ts.ReadState();
        }

        /// <summary>
        /// Remove item from cache asynchronizly
        /// </summary>
        /// <param name="cacheKey"></param>
        public void RemoveAsync(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentNullException("cacheKey is required");
            }
            var message = new CacheMessage() { Command = CacheCmd.RemoveAsync, Id = cacheKey };
            SendOut(message);
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
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentNullException("cacheKey is required");
            }

            var message = new CacheMessage() { Command = CacheCmd.Get, Id = cacheKey };
            var ts = SendDuplexStream(message);
            if (ts == null)
            {
                return null;
            }
            return (NetStream)ts.GetStream();
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
        public T Fetch<T>(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentNullException("cacheKey is required");
            }

            var message = new CacheMessage() { Command = CacheCmd.Fetch, Id = cacheKey};
            return SendDuplexStream<T>(message, OnFault);
        }

        public object Fetch(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentNullException("cacheKey is required");
            }
            var message = new CacheMessage() { Command = CacheCmd.Fetch, Id = cacheKey };
            return SendDuplexStreamValue(message, OnFault);
        }

        /// <summary>
        /// Load data from db to cache or get it if exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionKey"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="keyValueParameters"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public T LoadData<T>(string connectionKey, string commandText, CommandType commandType, int commandTimeout, object[] keyValueParameters, int expiration)
        {
            CommandContext item = new CommandContext(connectionKey, commandText, commandType, commandTimeout,typeof(T));
            item.CreateParameters(keyValueParameters);
            if (item == null)
                return default(T);
            using (var message = new CacheMessage(CacheCmd.LoadData, item.CreateKey(), item, expiration))
            {
                return SendDuplexStream<T>(message, OnFault);
            }
        }

        /// <summary>
        /// Set a new item to the cache, if this item is exists override it with the new one.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        public CacheState Set(string cacheKey, object value, int expiration)
        {
            if (value == null)
                return CacheState.ArgumentsError;

            using (var message = new CacheMessage(CacheCmd.Set, cacheKey, value, expiration))
            {
                return SendDuplexState(message);
            }
        }
        /// <summary>
        /// Set a new item to the cache, if this item is exists override it with the new one.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="sessionId"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        public CacheState Set(string cacheKey, object value, string sessionId, int expiration)
        {
            if (value == null)
                return CacheState.ArgumentsError;

            using (var message = new CacheMessage() {
                Command= CacheCmd.Set,
                Id= cacheKey,
                GroupId=sessionId,
                Expiration=expiration

            })
            {
                message.SetBody(value);
                return SendDuplexState(message);
            }
        }
        /// <summary>
        /// Add a new item to the cache, only if this item not exists.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        public CacheState Add(string cacheKey, object value, int expiration)
        {
            if (value == null)
                return CacheState.ArgumentsError;

            using (var message = new CacheMessage(CacheCmd.Add, cacheKey, value, expiration))
            {
                return SendDuplexState(message);
            }
        }
        /// <summary>
        /// Add a new item to the cache, only if this item not exists.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="sessionId"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="CacheState"/></returns>
        public CacheState Add(string cacheKey, object value, string sessionId, int expiration)
        {
            if (value == null)
                return CacheState.ArgumentsError;

            using (var message = new CacheMessage() {
                    Command = CacheCmd.Set,
                    Id = cacheKey,
                    GroupId = sessionId,
                    Expiration = expiration

            }) //CacheCmd.Add, cacheKey, value, expiration, sessionId))
            {
                message.SetBody(value);
                return SendDuplexState(message);
            }
        }

        /// <summary>
        /// Get item copy from cache
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns>return <see cref="CacheEntry"/></returns>
        public CacheEntry GetEntry(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentNullException("cacheKey is required");
            }

            using (var message = new CacheMessage() { Command = CacheCmd.GetEntry, Id = cacheKey })
            {
                return SendDuplexStream<CacheEntry>(message, OnFault);
            }
        }

        /// <summary>
        /// Get item copy from cache
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns>return <see cref="CacheEntry"/></returns>
        public CacheEntry ViewEntry(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentNullException("cacheKey is required");
            }

            using (var message = new CacheMessage() { Command = CacheCmd.ViewEntry, Id = cacheKey})
            {
                return SendDuplexStream<CacheEntry>(message,OnFault);
            }
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
        public CacheState CopyTo(string source, string dest, int expiration)
        {
            using (var message = new CacheMessage()
            {
                Command = CacheCmd.CopyTo,
                Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Destination, dest),
                Expiration = expiration,
                IsDuplex = false,
                Id = dest
            })
            {
                return SendDuplexState(message);
            }
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
        public CacheState CutTo(string source, string dest, int expiration)
        {

            using (var message = new CacheMessage()
            {
                Command = CacheCmd.CutTo,
                Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Destination, dest),
                Expiration = expiration,
                IsDuplex = false,
                Id = dest
            })
            {
                return SendDuplexState(message);
            }
        }

        #endregion

        /// <summary>
        /// Remove all session items from cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>return number of items removed from cache.</returns>
        public CacheState RemoveItemsBySession(string sessionId)
        {
            if (sessionId == null)
                return CacheState.ArgumentsError;
            using (var message = new CacheMessage() { Command = CacheCmd.RemoveItemsBySession, Label = sessionId })
            {
                return SendDuplexState(message);
            }
        }

        /// <summary>
        /// Keep Alive Cache Item.
        /// </summary>
        /// <param name="cacheKey"></param>
        public void KeepAliveItem(string cacheKey)
        {
            if (cacheKey == null)
                return;
            using (var message = new CacheMessage() { Command = CacheCmd.KeepAliveItem, Id = cacheKey })
            {
                SendOut(message);
            }
        }
    }
}
