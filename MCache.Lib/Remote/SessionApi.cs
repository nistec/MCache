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
using Nistec.Channels;
using Nistec.Caching.Session;
using Nistec.Caching.Channels;
using Nistec.Caching.Config;
using Nistec.Serialization;
using Nistec.IO;

namespace Nistec.Caching.Remote
{
    /// <summary>
    /// Represent session api for client.
    /// </summary>
    public class SessionCacheApi : RemoteApi
    {

        /// <summary>
        /// Get cache api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static SessionCacheApi Get(NetProtocol protocol = NetProtocol.Tcp)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = CacheApiSettings.Protocol;
            }
            return new SessionCacheApi() { Protocol = protocol };
        }
        private SessionCacheApi()
        {
            RemoteHostName = CacheApiSettings.RemoteSessionHostName;
            EnableRemoteException = CacheApiSettings.EnableRemoteException;
        }

        /// <summary>
        /// Get item value from cache as json.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetJson(string sessionId, string key, JsonFormat format)
        {
            var stream = Get<NetStream>(sessionId, key);
            return RemoteApi.ToJson(stream, format);
        }

        /// <summary>
        /// Get item from specified session cache using session id and item key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string sessionId, string key)
        {
            return SendDuplex<string>(SessionCmd.GetSessionItem,key, sessionId );
        }

        /// <summary>
        /// Get item from specified session cache using session id and item key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Get item from existing session.
        ///public void GetItem()
        ///{
        ///    string key = "item key 1";
        ///    var entity = <![CDATA[TcpSessionApi.GetItem<EntitySample>(sessionId, key);]]>
        ///    if (entity == null)
        ///        Console.WriteLine("entity null " + key);
        ///    else
        ///        Console.WriteLine(entity.Name);
        ///}
        /// </code></example>
        public T Get<T>(string sessionId, string key)
        {
            return SendDuplex<T>(SessionCmd.GetSessionItem, key, sessionId);
        }
        /// <summary>
        ///  Fetch item from specified session cache using session id and item key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Fetch<T>(string sessionId, string key)
        {
            return SendDuplex<T>(SessionCmd.FetchSessionItem, key, sessionId);
        }
        /// <summary>
        /// Copy session item to a new place in <see cref="MCache"/> cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="targetKey"></param>
        /// <param name="expiration"></param>
        /// <param name="addToCache"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Copy item from session to cache.
        ///public void CopyTo()
        ///{
        ///    string key = "item key 1";
        ///    TcpSessionApi.CopyTo(sessionId, key, key, timeout, true);
        ///}
        /// </code></example>
        public int CopyTo(string sessionId, string key, string targetKey, int expiration, bool addToCache = false)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.CopyTo,
                Id = sessionId,
                Args = MessageStream.CreateArgs(KnowsArgs.TargetKey, targetKey, KnowsArgs.AddToCache, addToCache.ToString()),
                Key = key,
                Expiration = expiration
            })
            {
                return SendDuplex<int>(message);
            }
        }

        /// <summary>
        /// Copy session item to a new place in <see cref="MCache"/> cache, and remove the current session item.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="targetKey"></param>
        /// <param name="expiration"></param>
        /// <param name="addToCache"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Fetch item from current session to cache.
        ///public void FetchTo()
        ///{
        ///    string key = "item key 2";
        ///    TcpSessionApi.FetchTo(sessionId, key, key, timeout, true);
        ///}
        /// </code></example>
        public int FetchTo(string sessionId, string key, string targetKey, int expiration, bool addToCache = false)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.FetchTo,
                Id = sessionId,
                Args = MessageStream.CreateArgs(KnowsArgs.TargetKey, targetKey, KnowsArgs.AddToCache, addToCache.ToString()),
                Key = key,
                Expiration = expiration
            })
            {
                return SendDuplex<int>(message);
            }
        }

        /// <summary>
        /// Add a new session to session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <param name="timeout"></param>
        /// <param name="args"></param>
        /// <example><code>
        /// //Create new session.
        ///public void AddSession()
        ///{
        ///     string sessionId = "12345678";
        ///     string userId = "12";
        ///     int timeout = 0;
        ///     
        ///    TcpSessionApi.AddSession(sessionId, userId, timeout, null);
        ///    TcpSessionApi.AddItem(sessionId, "item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
        ///    Thread.Sleep(10000);
        ///    var session = TcpSessionApi.GetExistingSession(sessionId);
        ///    Console.WriteLine(session.Print());
        ///}
        /// </code></example>
        public void AddSession(string sessionId, string userId, int timeout, string args)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.AddSession,
                Id = sessionId,
                Args = MessageStream.CreateArgs(KnowsArgs.UserId, userId, KnowsArgs.StrArgs, args),
                Expiration = timeout
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// Remove session from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="isAsync"></param>
        /// <example><code>
        ///  //remove session with items.
        ///public void RemoveSession()
        ///{
        ///    TcpSessionApi.RemoveSession(sessionId, false);
        ///}
        /// </code></example>
        public void RemoveSession(string sessionId, bool isAsync = false)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.RemoveSession,
                Id = sessionId,
                Args = MessageStream.CreateArgs(KnowsArgs.IsAsync, isAsync.ToString())
            })
            {
                SendOut(message);
            }
        }
        /// <summary>
        /// Remove all items from specified session.
        /// </summary>
        /// <param name="sessionId"></param>
        public void Clear(string sessionId)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.ClearSessionItems,
                Id = sessionId
            })
            {
                SendOut(message);
            }
        }
        /// <summary>
        /// Remove all sessions from session cache.
        /// </summary>
        public void ClearAllSessions()
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.ClearAllSessions
            })
            {
                SendOut(message);
            }
        }
        /// <summary>
        /// Get or create (if not exists) session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Get or create session.
        ///public void GetOrCreateSession()
        ///{
        ///    var session = TcpSessionApi.GetOrCreate(sessionId);
        ///    Console.WriteLine(session.Print());
        ///}
        /// </code></example>
        public SessionBagStream GetOrCreate(string sessionId)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.GetOrCreateSession,
                Id = sessionId,
                Args = MessageStream.CreateArgs(KnowsArgs.ShouldSerialized, "true")
            })
            {
               return SendDuplex<SessionBagStream>(message);
            }
        }
        /// <summary>
        /// Get existing session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public SessionBagStream GetExistingSession(string sessionId)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.GetExistingSession,
                Id = sessionId,
                Args = MessageStream.CreateArgs(KnowsArgs.ShouldSerialized, "true")
            })
            {
                return SendDuplex<SessionBagStream>(message);
            }
        }


        

        /// <summary>
        /// Refresh specified session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        public void Refresh(string sessionId)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.SessionRefresh,
                Id = sessionId,
            })
            {
                SendOut(message);
            }
        }
        /// <summary>
        /// Refresh sfcific session in session cache or create a new session bag if not exists.
        /// </summary>
        /// <param name="sessionId"></param>
        public void RefreshOrCreate(string sessionId)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.RefreshOrCreate,
                Id = sessionId,
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// Remove item from specified session using session id and item key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Remove item from current session
        ///public void Remove()
        ///{
        ///    string key = "item key 3";
        ///    bool ok = TcpSessionApi.RemoveItem(sessionId, key);
        ///    Console.WriteLine(ok);
        ///}
        /// </code></example>
        public bool Remove(string sessionId, string key)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.RemoveSessionItem,
                Id = sessionId,
                Key = key
            })
            {
                return SendDuplex<bool>(message);
            }
        }

        ///// <summary>
        ///// Add item to specified session only if session exists in session cache.
        ///// </summary>
        ///// <param name="sessionId"></param>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public bool AddItemExisting(string sessionId, string key, object value)
        //{
        //    return TcpClientCache.SendDuplex<bool>(new CacheMessage(SessionCmd.AddItemExisting, key,value, CacheDefaults.SessionTimeout, sessionId), CacheDefaults.MSessionPipeName);
        //}
        /// <summary>
        ///  Add item to specified session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="validateExisting"></param>
        /// <returns></returns>
         /// <example><code>
        /// //Add items to current session.
        ///public void AddItems()
        ///{
        ///     string sessionId = "12345678";
        ///     string userId = "12";
        ///     int timeout = 0;
        ///    for (int i = 0; i <![CDATA[<]]> 20; i++)
        ///    {
        ///        var dt = ContactEntityContext.GetList();
        ///        TcpSessionApi.Set(sessionId, "contact " + (i + 100).ToString(), new EntitySample() { Id = 123, Name = "entity sample " + i, Creation = DateTime.Now, Value = dt }, timeout);
        ///    }
        ///}
        /// </code></example>       
        public int Set(string sessionId, string key, object value, bool validateExisting = false)
        {
            string cmd = (validateExisting) ? SessionCmd.AddItemExisting : SessionCmd.AddSessionItem;
            using (var message = new CacheMessage(cmd, key, value, CacheDefaults.SessionTimeout, sessionId))
            {
                return SendDuplex<int>(message);
            }
        }
        /// <summary>
        /// Add item to specified session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <param name="validateExisting"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Add items to current session.
        ///public void AddItems()
        ///{
        ///     string sessionId = "12345678";
        ///     string userId = "12";
        ///     int timeout = 0;
        ///    TcpSessionApi.Set(sessionId, "item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
        ///    TcpSessionApi.Set(sessionId, "item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
        ///    TcpSessionApi.Set(sessionId, "item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
        ///}
        /// </code></example>
        public int Set(string sessionId, string key, object value, int timeout, bool validateExisting = false)
        {
            string cmd = (validateExisting) ? SessionCmd.AddItemExisting : SessionCmd.AddSessionItem;
            using (var message = new CacheMessage(cmd, key, value, timeout, sessionId))
            {
                return SendDuplex<int>(message);
            }
        }
        /// <summary>
        /// Get indicate whether the session cache contains specified item in specific session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string sessionId, string key)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.Exists,
                Id = sessionId,
                Key = key
            })
            {
                return SendDuplex<bool>(message);
            }
        }
        /// <summary>
        /// Get all sessions keys in session cache.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetAllSessionsKeys()
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.GetAllSessionsKeys
            })
            {
                return SendDuplex<ICollection<string>>(message);
            }
        }

        /// <summary>
        /// Get all sessions keys in session cache using <see cref="SessionState"/> state.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetAllSessionsStateKeys(SessionState state)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.GetAllSessionsStateKeys,
                Args = CacheMessage.CreateArgs("state", ((int)state).ToString())
            })
            {
                return SendDuplex<ICollection<string>>(message);
            }
        }

        /// <summary>
        /// Get all items keys in specified session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public ICollection<string> GetSessionsItemsKeys(string sessionId)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.GetSessionItemsKeys,
                Id = sessionId,
            })
            {
                return SendDuplex<ICollection<string>>(message);
            }
        }

    
        /// <summary>
        /// Reply for test
        /// </summary>
        /// <returns></returns>
        public string Reply(string text)
        {
            return SendDuplex<string>(SessionCmd.Reply, text);
        }
    }
}
