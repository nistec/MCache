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
//using Nistec.Caching.Channels;
using Nistec.Caching.Config;
using Nistec.Serialization;
using Nistec.IO;
using Nistec.Generic;

namespace Nistec.Caching.Remote
{
    /// <summary>
    /// Represent session api for client.
    /// </summary>
    public class SessionCacheApi : RemoteApi
    {

        #region static

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
            return new SessionCacheApi() { Protocol = protocol, SessionTimeout = CacheApiSettings.SessionTimeout };
        }
        private SessionCacheApi()
        {
            RemoteHostName = CacheDefaults.DefaultBundleHostName;
            EnableRemoteException = CacheApiSettings.EnableRemoteException;
        }
        #endregion


        int _SessionTimeout;
        public int SessionTimeout
        {
            get { return _SessionTimeout; }
            set { _SessionTimeout = (value < 0) ? CacheApiSettings.SessionTimeout : value; }
        }

        protected void OnFault(string message)
        {
            Console.WriteLine("SessionApi Fault: " + message);
        }

        #region do custom
        public object DoCustom(string command, string sessionId, string id, object value = null, int expiration = 0)
        {
            switch ("sess_" + command)
            {
                case SessionCmd.Add:
                    return Add(sessionId, id, value, expiration);
                case SessionCmd.ClearAll:
                    return ClearAll();
                case SessionCmd.ClearItems:
                    return ClearItems(sessionId);
                case SessionCmd.CopyTo:
                    return CopyTo(sessionId, id, ComplexKey.Get(sessionId, id).ToString(), expiration);
                case SessionCmd.CreateSession:
                    return CreateSession(sessionId, "0", expiration, null);
                case SessionCmd.CutTo:
                    return CutTo(sessionId, id, ComplexKey.Get(sessionId, id).ToString(), expiration);
                case SessionCmd.Exists:
                    return Exists(sessionId, id);
                case SessionCmd.Fetch:
                    return Fetch(sessionId, id);
                case SessionCmd.FetchRecord:
                    return FetchRecord(sessionId, id);
                case SessionCmd.Get:
                    return GetValue(sessionId, id);
                case SessionCmd.GetEntry:
                    return GetEntry(sessionId, id);
                case SessionCmd.GetOrCreateSession:
                    return GetOrCreateSession(sessionId);
                case SessionCmd.GetOrCreateRecord:
                    return GetOrCreateRecord(sessionId, id, value, expiration);
                case SessionCmd.GetRecord:
                    return GetRecord(sessionId, id);
                case SessionCmd.GetSessionItems:
                    return GetSessionItems(sessionId);
                case SessionCmd.Refresh:
                    return Refresh(sessionId);
                case SessionCmd.RefreshOrCreate:
                    return RefreshOrCreate(sessionId);
                case SessionCmd.Remove:
                    return Remove(sessionId, id);
                case SessionCmd.RemoveSession:
                    return RemoveSession(sessionId);
                case SessionCmd.Reply:
                    return Reply(sessionId);
                case SessionCmd.Set:
                    return Set(sessionId, id, value, expiration);
                case SessionCmd.ViewAllSessionsKeys:
                    return ViewAllSessionsKeys();
                case SessionCmd.ViewAllSessionsKeysByState:
                    return ViewAllSessionsKeysByState(SessionState.Active);
                case SessionCmd.ViewEntry:
                    return ViewEntry(sessionId, id);
                case SessionCmd.ViewSessionKeys:
                    return ViewSessionKeys(sessionId);
                case SessionCmd.ViewSessionStream:
                    return ViewSessionStream(sessionId);
                default:
                    throw new ArgumentException("Unknown command " + command);
            }
        }

        public string DoHttpJson(string command, string sessionId, string id, object value = null, int expiration = 0, bool pretty=false)
        {
            string cmd = "sync_" + command.ToLower();
            switch (cmd)
            {
                case SessionCmd.Add:
                case SessionCmd.GetOrCreateRecord:
                case SessionCmd.Set:
                    {
                        if (string.IsNullOrWhiteSpace(sessionId))
                        {
                            throw new ArgumentNullException("key is required");
                        }
                        var msg=new CacheMessage() { Command = cmd, SessionId = sessionId, CustomId = id , Expiration=expiration};
                        msg.SetBody(value);
                        return SendHttpJsonDuplex(msg, pretty);
                    }
                case SessionCmd.ViewAllSessionsKeys:
                case SessionCmd.ClearAll:
                    return SendHttpJsonDuplex(new CacheMessage() { Command=cmd }, pretty);
                case SessionCmd.GetOrCreateSession:
                case SessionCmd.RefreshOrCreate:
                case SessionCmd.GetSessionItems:
                case SessionCmd.ViewSessionStream:
                case SessionCmd.Reply:
                case SessionCmd.RemoveSession:
                case SessionCmd.Refresh:
                case SessionCmd.ClearItems:
                    if (string.IsNullOrWhiteSpace(sessionId))
                    {
                        throw new ArgumentNullException("sessionId is required");
                    }
                    return SendHttpJsonDuplex(new CacheMessage() { Command = cmd, SessionId = sessionId }, pretty);
                case SessionCmd.Exists:
                case SessionCmd.Fetch:
                case SessionCmd.FetchRecord:
                case SessionCmd.Get:
                case SessionCmd.GetEntry:
                case SessionCmd.GetRecord:
                case SessionCmd.Remove:
                case SessionCmd.ViewEntry:
                    if (string.IsNullOrWhiteSpace(sessionId))
                    {
                        throw new ArgumentNullException("sessionId is required");
                    }
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        throw new ArgumentNullException("id is required");
                    }
                    return SendHttpJsonDuplex(new CacheMessage() { Command = cmd, CustomId = id, SessionId = sessionId } , pretty);
                case SessionCmd.CopyTo:
                    //return CopyTo(key, detail, ComplexKey.Get(key, detail).ToString(), expiration);
                    return CacheState.CommandNotSupported.ToString();
                case SessionCmd.CreateSession:
                    {
                        if (string.IsNullOrWhiteSpace(sessionId))
                        {
                            throw new ArgumentNullException("sessionId is required");
                        }
                        var message = new CacheMessage()
                        {
                            Command = SessionCmd.CreateSession,
                            SessionId = sessionId,
                            Args = NameValueArgs.Create(KnownArgs.UserId, "0", KnownArgs.StrArgs, ""),
                            Expiration = expiration
                        };
                        return SendHttpJsonDuplex(message, pretty);
                    }
                case SessionCmd.CutTo:
                    //return CutTo(key, detail, ComplexKey.Get(key, detail).ToString(), expiration);
                    return CacheState.CommandNotSupported.ToString();
                case SessionCmd.ViewAllSessionsKeysByState:
                    //return ViewAllSessionsKeysByState(SessionState.Active);
                    {
                        var message = new CacheMessage()
                        {
                            Command = SessionCmd.ViewAllSessionsKeysByState,
                            Args = NameValueArgs.Create("state", ((int)SessionState.Active).ToString())
                        };
                        return SendHttpJsonDuplex(message, pretty);
                    }
               default:
                    throw new ArgumentException("Unknown command " + command);
            }
        }
        #endregion

        /// <summary>
        /// Get or Set cache session value by key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string sessionId, string key]
        {
            get { return GetValue(sessionId, key); }
            set { Set(sessionId, key, value, SessionTimeout); }
        }

        /// <summary>
        /// Get value from session cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetValue<T>(string sessionId, string key)
        {
            var val = GetValue(sessionId, key);
            if (val == null)
                return default(T);
            return GenericTypes.Convert<T>(val);
        }

        /// <summary>
        /// Get value from session cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValue<T>(string sessionId, string key, T defaultValue)
        {
            var val = GetValue(sessionId, key);
            if (val == null)
                return defaultValue;
            return GenericTypes.Convert<T>(val, defaultValue);
        }

        /// <summary>
        /// Get value from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue(string sessionId, string key)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            CacheMessage message = new CacheMessage()
            {
                Command = SessionCmd.Get,
                CustomId = key,
                SessionId = sessionId
            };
            var stream = (TransStream)SendDuplexStream(message);
            if (stream == null)
                return null;
            return stream.ReadValue();
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
            var obj = GetValue(sessionId, key);
            if (obj == null)
                return null;
            return JsonSerializer.Serialize(obj, null, format);
        }



        /// <summary>
        /// Get item value from session using session id and item key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public SessionEntry GetEntry(string sessionId, string key)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            CacheMessage message = new CacheMessage()
            {
                Command = SessionCmd.GetEntry,
                CustomId = key,
                SessionId = sessionId
            };
            return (SessionEntry)SendDuplexStream<SessionEntry>(message, OnFault);
        }

        public SessionEntry ViewEntry(string sessionId, string key)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            CacheMessage message = new CacheMessage()
            {
                Command = SessionCmd.ViewEntry,
                CustomId = key,
                SessionId = sessionId
            };
            return (SessionEntry)SendDuplexStream<SessionEntry>(message, OnFault);
        }

        /// <summary>
        ///  Fetch item from specified session cache using session id and item key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public IDictionary<string, object> GetRecord(string sessionId, string key)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            CacheMessage message = new CacheMessage()
            {
                Command = SessionCmd.GetRecord,
                CustomId = key,
                SessionId = sessionId
            };
            return SendDuplexStream<Dictionary<string, object>>(message, OnFault);
        }

        /// <summary>
        ///  Fetch item from specified session cache using session id and item key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public IDictionary<string, object> FetchRecord(string sessionId, string key)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            CacheMessage message = new CacheMessage()
            {
                Command = SessionCmd.FetchRecord,
                CustomId = key,
                SessionId = sessionId
            };
            return SendDuplexStream<Dictionary<string,object>>(message, OnFault);
        }


        /// <summary>
        ///  Fetch item from specified session cache using session id and item key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Fetch(string sessionId, string key)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            CacheMessage message = new CacheMessage()
            {
                Command = SessionCmd.Fetch,
                CustomId = key,
                SessionId = sessionId
            };
            return SendDuplexStreamValue(message, OnFault);
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
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            CacheMessage message = new CacheMessage()
            {
                Command = SessionCmd.Fetch,
                CustomId = key,
                SessionId = sessionId
            };
            return SendDuplexStream<T>(message, OnFault);
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
        public CacheState CopyTo(string sessionId, string key, string targetKey, int expiration, bool addToCache = false)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            if (string.IsNullOrWhiteSpace(targetKey))
            {
                throw new ArgumentNullException("targetKey is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.CopyTo,
                SessionId = sessionId,
                Args = NameValueArgs.Create(KnownArgs.TargetKey, targetKey, KnownArgs.AddToCache, addToCache.ToString()),
                CustomId = key,
                Expiration = expiration
            })
            {
                return SendDuplexState(message);
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
        public CacheState CutTo(string sessionId, string key, string targetKey, int expiration, bool addToCache = false)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            if (string.IsNullOrWhiteSpace(targetKey))
            {
                throw new ArgumentNullException("targetKey is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.CutTo,
                SessionId = sessionId,
                Args = NameValueArgs.Create(KnownArgs.TargetKey, targetKey, KnownArgs.AddToCache, addToCache.ToString()),
                CustomId = key,
                Expiration = expiration
            })
            {
                return SendDuplexState(message);
            }
        }

        /// <summary>
        /// Add a new session to session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <param name="timeout"></param>
        /// <param name="args"></param>
        public CacheState CreateSession(string sessionId, string userId, int timeout, string args)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException("userId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.CreateSession,
                SessionId = sessionId,
                Args = NameValueArgs.Create(KnownArgs.UserId, userId, KnownArgs.StrArgs, args),
                Expiration = timeout
            })
            {
                return SendDuplexState(message);
            }
        }

        /// <summary>
        /// Remove session from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        public CacheState RemoveSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.RemoveSession,
                SessionId = sessionId
            })
            {
                return SendDuplexState(message);
            }
        }
        /// <summary>
        /// Remove all items from specified session.
        /// </summary>
        /// <param name="sessionId"></param>
        public CacheState ClearItems(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.ClearItems,
                SessionId = sessionId
            })
            {
                return SendDuplexState(message);
            }
        }
        /// <summary>
        /// Remove all sessions from session cache.
        /// </summary>
        public CacheState ClearAll()
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.ClearAll
            })
            {
                return SendDuplexState(message);
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
        public SessionBagStream GetOrCreateSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.GetOrCreateSession,
                SessionId = sessionId,
                Args = NameValueArgs.Create(KnownArgs.ShouldSerialized, "true")
            })
            {
                return SendDuplexStream<SessionBagStream>(message, OnFault);
            }
        }

        public IDictionary<string,object> GetOrCreateRecord(string sessionId,string key,object value,int sessionTimeout)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value is required");
            }
            using (var message = new CacheMessage() {
                Command = SessionCmd.GetOrCreateRecord,
                CustomId = key,
                Expiration= sessionTimeout,
                SessionId = sessionId

            })// SessionCmd.GetOrCreateRecord, key, value, sessionTimeout, sessionId))
            {
                message.SetBody(value);
                return SendDuplexStream<Dictionary<string, object>>(message, OnFault);
            }
        }

        ///// <summary>
        ///// Get existing session in session cache.
        ///// </summary>
        ///// <param name="sessionId"></param>
        ///// <returns></returns>
        //public SessionBagStream GetExistingSession(string sessionId)
        //{
        //    if (string.IsNullOrWhiteSpace(sessionId))
        //    {
        //        throw new ArgumentNullException("sessionId is required");
        //    }

        //    using (var message = new CacheMessage()
        //    {
        //        Command = SessionCmd.GetExistingSession,
        //        Id = sessionId,
        //        Args = NameValueArgs.Create(KnownArgs.ShouldSerialized, "true")
        //    })
        //    {
        //        return SendDuplexStream<SessionBagStream>(message, OnFault);
        //    }
        //}

        /// <summary>
        /// Get existing session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public IDictionary<string, object> GetSessionItems(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.GetSessionItems,
                SessionId = sessionId,
                Args = NameValueArgs.Create(KnownArgs.ShouldSerialized, "true")
            })
            {
                return SendDuplexStream<IDictionary<string, object>>(message, OnFault);
            }
        }




        /// <summary>
        /// Refresh specified session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        public CacheState Refresh(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.Refresh,
                SessionId = sessionId,
            })
            {
                return SendDuplexState(message);
            }
        }
        /// <summary>
        /// Refresh sfcific session in session cache or create a new session bag if not exists.
        /// </summary>
        /// <param name="sessionId"></param>
        public CacheState RefreshOrCreate(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.RefreshOrCreate,
                SessionId = sessionId,
            })
            {
                return SendDuplexState(message);
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
        public CacheState Remove(string sessionId, string key)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.Remove,
                SessionId = sessionId,
                CustomId = key
            })
            {
                return SendDuplexState(message);
            }
        }

        /////// <summary>
        /////// Add item to specified session only if session exists in session cache.
        /////// </summary>
        /////// <param name="sessionId"></param>
        /////// <param name="key"></param>
        /////// <param name="value"></param>
        /////// <returns></returns>
        ////public bool AddItemExisting(string sessionId, string key, object value)
        ////{
        ////    return TcpClientCache.SendDuplex<bool>(new CacheMessage(SessionCmd.AddItemExisting, key,value, CacheDefaults.SessionTimeout, sessionId), CacheDefaults.MSessionPipeName);
        ////}
        ///// <summary>
        /////  Add item to specified session in session cache.
        ///// </summary>
        ///// <param name="sessionId"></param>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        ///// <param name="validateExisting"></param>
        ///// <returns></returns>
        // /// <example><code>
        ///// //Add items to current session.
        /////public void AddItems()
        /////{
        /////     string sessionId = "12345678";
        /////     string userId = "12";
        /////     int timeout = 0;
        /////    for (int i = 0; i <![CDATA[<]]> 20; i++)
        /////    {
        /////        var dt = ContactEntityContext.GetList();
        /////        TcpSessionApi.Set(sessionId, "contact " + (i + 100).ToString(), new EntitySample() { Id = 123, Name = "entity sample " + i, Creation = DateTime.Now, Value = dt }, timeout);
        /////    }
        /////}
        ///// </code></example>       
        //public CacheState Set(string sessionId, string key, object value, bool validateExisting = false)
        //{
        //    if (string.IsNullOrWhiteSpace(sessionId))
        //    {
        //        throw new ArgumentNullException("sessionId is required");
        //    }
        //    if (string.IsNullOrWhiteSpace(key))
        //    {
        //        throw new ArgumentNullException("key is required");
        //    }
        //    if (value==null)
        //    {
        //        throw new ArgumentNullException("value is required");
        //    }


        //    string cmd = (validateExisting) ? SessionCmd.AddItemExisting : SessionCmd.AddSessionItem;
        //    using (var message = new CacheMessage(cmd, key, value, SessionTimeout, sessionId))
        //    {
        //        return SendDuplexState(message);
        //    }
        //}

        ///// <summary>
        ///// Add item to specified session in session cache.
        ///// </summary>
        ///// <param name="sessionId"></param>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        ///// <param name="sessionTimeout"></param>
        ///// <param name="validateExisting"></param>
        ///// <returns></returns>
        ///// <example><code>
        ///// //Add items to current session.
        /////public void AddItems()
        /////{
        /////     string sessionId = "12345678";
        /////     string userId = "12";
        /////     int timeout = 0;
        /////    TcpSessionApi.Set(sessionId, "item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
        /////    TcpSessionApi.Set(sessionId, "item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
        /////    TcpSessionApi.Set(sessionId, "item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
        /////}
        ///// </code></example>
        //public CacheState Set(string sessionId, string key, object value, int sessionTimeout, bool validateExisting = false)
        //{
        //    if (string.IsNullOrWhiteSpace(sessionId))
        //    {
        //        throw new ArgumentNullException("sessionId is required");
        //    }
        //    if (string.IsNullOrWhiteSpace(key))
        //    {
        //        throw new ArgumentNullException("key is required");
        //    }
        //    if (value == null)
        //    {
        //        throw new ArgumentNullException("value is required");
        //    }

        //    string cmd = (validateExisting) ? SessionCmd.AddItemExisting : SessionCmd.AddSessionItem;
        //    using (var message = new CacheMessage(cmd, key, value, sessionTimeout, sessionId))
        //    {
        //        return SendDuplexState(message);
        //    }
        //}

        /// <summary>
        /// Add item to specified session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="sessionTimeout"></param>
        /// <returns></returns>
        public CacheState Set(string sessionId, string key, object value, int sessionTimeout)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value is required");
            }

            using (var message = new CacheMessage() {
                Command = SessionCmd.Set,
                CustomId = key,
                SessionId = sessionId,
                Expiration=sessionTimeout

            })// SessionCmd.Set, key, value, sessionTimeout, sessionId))
            {
                message.SetBody(value);
                return SendDuplexState(message);
            }
        }

        /// <summary>
        /// Add item to a new session in cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="sessionTimeout"></param>
        /// <returns></returns>
        public CacheState Add(string sessionId, string key, object value, int sessionTimeout)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value is required");
            }
            using (var message = new CacheMessage() {
                Command = SessionCmd.Add,
                CustomId = key,
                SessionId = sessionId,
                Expiration = sessionTimeout

            })// SessionCmd.Add, key, value, sessionTimeout, sessionId))
            {
                message.SetBody(value);
                return SendDuplexState(message);
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
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.Exists,
                SessionId = sessionId,
                CustomId = key
            })
            {
                return SendDuplexState(message)== CacheState.Ok;
            }
        }
        /// <summary>
        /// Get all sessions keys in session cache.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> ViewAllSessionsKeys()
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.ViewAllSessionsKeys
            })
            {
                return SendDuplexStream<ICollection<string>>(message, OnFault);
            }
        }

        /// <summary>
        /// Get all sessions keys in session cache using <see cref="SessionState"/> state.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> ViewAllSessionsKeysByState(SessionState state)
        {
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.ViewAllSessionsKeysByState,
                Args = NameValueArgs.Create("state", ((int)state).ToString())
            })
            {
                return SendDuplexStream<string[]>(message, OnFault);
            }
        }

        /// <summary>
        /// Get all items keys in specified session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public ICollection<string> ViewSessionKeys(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.ViewSessionKeys,
                SessionId = sessionId,
            })
            {
                return SendDuplexStream<string[]>(message, OnFault);
            }
        }
        public SessionBagStream ViewSessionStream(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException("sessionId is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SessionCmd.ViewSessionStream,
                SessionId = sessionId,
            })
            {
                return SendDuplexStream<SessionBagStream>(message, OnFault);
            }
        }
        
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
            using (var message = new CacheMessage()
            {
                Command = SessionCmd.Reply,
                CustomId = text,
            })
            {
                return SendDuplexStream<string>(message, OnFault);
            }
        }
    }
}
