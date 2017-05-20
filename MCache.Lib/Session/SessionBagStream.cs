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
using System.Collections;
using System.Collections.Concurrent;
using Nistec.Serialization;
using System.IO;
using Nistec.IO;
using Nistec.Caching.Config;

namespace Nistec.Caching.Session
{

   
    /// <summary>
    /// Represent a session bag, each session include HashSet of <see cref="SessionEntry"/> list.
    /// </summary>
    [Serializable]
    public class SessionBagStream : ISerialEntity, ISessionBagStream
    {
        #region members
       
        private Dictionary<string, SessionEntry> m_SessionItems;

        /// <summary>
        /// Get Session items.
        /// </summary>
        [EntitySerialize]
        public Dictionary<string, SessionEntry> SessionItems
        {
            get { return m_SessionItems; }
            private set { m_SessionItems = value; }
        }

        #endregion

        #region ctor

        /// <summary>
        /// Initialize a new instance of session bag stream.
        /// </summary>
        public SessionBagStream()
        {
            m_SessionItems = new Dictionary<string, SessionEntry>();
        }

        internal SessionBagStream(SessionBag bag)
            : this()
        {
            this.SessionId = bag.SessionId;
            this.Args = bag.Args;
            this.Creation = bag.Creation;
            this.LastUsed = bag.LastUsed;
            this.Size = bag.Size;
            this.Timeout = bag.Timeout;
            this.UserId = bag.UserId;
            foreach (var entry in bag.ItemsValues())
            {
                m_SessionItems.Add(entry.Key, entry);
            }

        }
        #endregion

        #region  ISerialEntity

       
        /// <summary>
        /// Write entity stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteString(SessionId);
            streamer.WriteValue(Creation);
            streamer.WriteValue(LastUsed);
            streamer.WriteValue((int)Timeout);
            streamer.WriteString(Args);
            streamer.WriteString(UserId);
            streamer.WriteValue((int)Size);
            streamer.WriteValue(SessionItems);
            streamer.Flush();
        }

        /// <summary>
        /// Read entity stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            SessionId = streamer.ReadString();
            Creation = streamer.ReadValue<DateTime>();
            LastUsed = streamer.ReadValue<DateTime>();
            Timeout = streamer.ReadValue<int>();
            Args = streamer.ReadString();
            UserId = streamer.ReadString();
            Size = streamer.ReadValue<int>();
            SessionItems = (Dictionary<string, SessionEntry>)streamer.ReadValue();

        }

      
        #endregion


        /// <summary>
        /// Get valid timeout for timeout argument.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static int GetValidTimeout(int timeout)
        {
            if (timeout == 0)
                return CacheDefaults.MaxSessionTimeout;
            if (timeout < 0)
                return CacheDefaults.SessionTimeout;
            if (timeout > CacheDefaults.MaxSessionTimeout)
                return CacheDefaults.MaxSessionTimeout;
            return timeout;
        }

        /// <summary>
        /// Get indicate whether the item is timeout 
        /// </summary>
        public bool IsTimeOut
        {
            get { return /*AllowExpired &&*/ TimeSpan.FromMinutes(Timeout) < DateTime.Now.Subtract(LastUsed); }
        }

        /// <summary>
        /// Get the session state.
        /// </summary>
        public SessionState State
        {
            get 
            {

                if (CacheDefaults.MaxSessionTimeout >= Timeout && TimeSpan.FromMinutes(Timeout) < DateTime.Now.Subtract(LastUsed))
                    return SessionState.Idle;
                if (TimeSpan.FromMinutes(Timeout) < DateTime.Now.Subtract(LastUsed))
                    return SessionState.Timedout;
                return  SessionState.Active; 
            }
        }

 
        /// <summary>
        /// Gets a collection containing the keys in the <see cref="SessionBag"/>.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> ItemsKeys()
        {
            return m_SessionItems.Keys; 
        }
        /// <summary>
        /// Gets a collection containing the values in the <see cref="SessionBag"/>.
        /// </summary>
        /// <returns></returns>
        public ICollection<SessionEntry> ItemsValues()
        {
            return m_SessionItems.Values; 
        }
        /// <summary>
        /// Get item value as string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ItemAsString(string key)
        {
            SessionEntry o = Get(key);
            if (o == null)
            {
                return null;
            }
            return o.TypeName;
        }

        private void AddEntries(ICollection<SessionEntry> entries)
        {
            foreach (var entry in entries)
            {
                m_SessionItems[entry.Key] = entry;
            }
        }

        #region properties
        /// <summary>
        /// Get session id.
        /// </summary>
        public string SessionId { get; internal set; }
        /// <summary>
        /// Get Session Creation time.
        /// </summary>
        public DateTime Creation { get; internal set; }
        /// <summary>
        /// Get session timeout.
        /// </summary>
        public int Timeout { get; internal set; }
        
        /// <summary>
        /// Get the last used of current session.
        /// </summary>
        public DateTime LastUsed { get; internal set; }
        /// <summary>
        /// Get Args of current session.
        /// </summary>
        public string Args { get; internal set; }
        /// <summary>
        /// Get the user id who created the current session.
        /// </summary>
        public string UserId { get; internal set; }
        
        /// <summary>
        /// Get the size in bytes of current session.
        /// </summary>
        public long Size { get; internal set; }

        /// <summary>
        /// Determines whether the session bag contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            //Sync();
            if (string.IsNullOrEmpty(key))
                return false;
            return m_SessionItems.ContainsKey(key);
        }
        /// <summary>
        /// Get <see cref="SessionEntry"/> item from session bag by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SessionEntry Get(string key)
        {
            //Sync();
            if (string.IsNullOrEmpty(key))
                return null;
            SessionEntry o = null;
            if (m_SessionItems.TryGetValue(key, out o))
            {
                return o;
            }

            return null;
        }
        /// <summary>
        /// Print current session.
        /// </summary>
        /// <returns></returns>
        public string Print()
        {
            return string.Format("SessionId:{0},Size:{1},State:{2},UserId:{3},Timeout:{4},ItemsCount:{5}", SessionId, Size,State, UserId, Timeout, m_SessionItems.Count);
        }
       
        #endregion

    }

}
