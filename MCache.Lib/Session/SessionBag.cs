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
using Nistec.Runtime;
using System.IO;
using Nistec.IO;
using Nistec.Caching.Config;

namespace Nistec.Caching.Session
{

   
    /// <summary>
    /// Represent a session bag, each session include HashSet of <see cref="SessionEntry"/> list.
    /// </summary>
    [Serializable]
    public class SessionBag : ISessionBag
    {
        #region members
        /// <summary>
        /// Default Time Out Minute
        /// </summary>
        public const int DefaultTimeOutMinute = 30;
        private string _SessionId;
        private DateTime _Creation;
        private int _Timeout;
        private DateTime _LastUsed;
        private string _Args;
        private string _UserId;
        private int _Size;
        private ConcurrentDictionary<string, SessionEntry> m_SessionItems;
        SessionCache Owner;
        #endregion

        #region ctor
        /// <summary>
        /// Serializabtion constrauctor.
        /// </summary>
        public SessionBag() { }

        /// <summary>
        /// Initialize a new instance of session bag.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="sessionId"></param>
        /// <param name="timeout"></param>
        public SessionBag(SessionCache owner,string sessionId, int timeout)
            : this(owner,sessionId, "0", timeout, "")
        { }
        /// <summary>
        /// Initialize a new instance of session bag.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <param name="timeout"></param>
        /// <param name="args"></param>
        public SessionBag(SessionCache owner, string sessionId, string userId, int timeout, string args)
        {
            Owner = owner;
            _SessionId = sessionId;
            _UserId = userId;
            _Args = args;
            _Creation = DateTime.Now;
            _LastUsed = Creation;
            _Timeout = GetValidTimeout(timeout);
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 10;
            this.m_SessionItems = new ConcurrentDictionary<string, SessionEntry>(concurrencyLevel, initialCapacity);


        }
        #endregion

        internal SessionBagStream GetSessionBagStream()
        {
            SessionBagStream bag = new SessionBagStream(this);
            return bag;
        }

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
            get { return TimeSpan.FromMinutes(Timeout) < DateTime.Now.Subtract(LastUsed); }
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
        /// Synchronize the current session.
        /// </summary>
        public void Sync()
        {
            LastUsed = DateTime.Now;
            //_State = SessionState.Active;
        }
        /// <summary>
        /// Remove all items from current session.
        /// </summary>
        public void Clear()
        {
            m_SessionItems.Clear();
            Owner.SizeRefresh();
            _Size = 0;
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

        /// <summary>
        /// Remove item from session bag using key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            Sync();
            int oldSize = 0;
           
            SessionEntry entry;

            if (m_SessionItems.TryRemove(key, out entry))
            {
                oldSize = entry.Size;
                SetSize(oldSize, 0, 1,0, false);
                return true;
            }
            return false;
        }

        #region properties
        /// <summary>
        /// Get session id.
        /// </summary>
        public string SessionId { get { return _SessionId; } }
        /// <summary>
        /// Get Session Creation time.
        /// </summary>
        public DateTime Creation { get { return _Creation; } }
        /// <summary>
        /// Get session timeout.
        /// </summary>
        public int Timeout { get { return _Timeout; } }
        //public bool AllowExpired { get { return _AllowExpired; } }

        /// <summary>
        /// Get the last used of current session.
        /// </summary>
        public DateTime LastUsed { get { return _LastUsed; } set { _LastUsed = value; } }
        /// <summary>
        /// Get Args of current session.
        /// </summary>
        public string Args { get { return _Args; } set { _Args = value; } }
        /// <summary>
        /// Get the user id who created the current session.
        /// </summary>
        public string UserId { get { return _UserId; } }
        //public SessionState State { get { return _State; } }
        /// <summary>
        /// Get the size in bytes of current session.
        /// </summary>
        public long Size { get { return _Size; } }
  
        /// <summary>
        /// Determines whether the session bag contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            Sync();
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
            Sync();
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
        /// Add new <see cref="SessionEntry"/> to the session bag.
        /// </summary>
        /// <param name="value"></param>
        public void AddItem(SessionEntry value)
        {
            AddItem(value.Key, value);
        }
        /// <summary>
        /// Add new <see cref="SessionEntry"/> to the session bag.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddItem(string key, SessionEntry value)
        {
            if (string.IsNullOrEmpty(key))
                return;
            int oldSize = 0;

            SessionEntry entry;
            if (m_SessionItems.TryRemove(key, out entry))
            {
                Sync();
                oldSize = entry.Size;
                if (value != null)
                {
                    SetSize(oldSize, GetSize(value), 0, 1, false);
                    m_SessionItems[key] = value;
                }
            }
            else if (value != null)
            {
                Sync();
                SetSize(oldSize, GetSize(value), 0, 1, false);
                m_SessionItems[key] = value;
            }
        }

        private CacheState SetSize(int oldSize, int newSize,int oldCount,int newCount,bool exchange)
        {

            _Size += (newSize - oldSize);
            if (_Size < 0)
                _Size = 0;

            return Owner.SizeExchage(oldSize, newSize, oldCount,newCount, exchange);
        }

        private int GetSize(SessionEntry o)
        {
            if (o == null)
                return 0;
            return o.Size;
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
