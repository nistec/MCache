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
using System.Linq;
using Nistec.Collections;
using System.Runtime.Serialization;
using System.Collections.Concurrent;
using Nistec.Generic;
using Nistec.Runtime;
using System.Threading.Tasks;
using Nistec.Caching.Server;
using Nistec.IO;
using Nistec.Caching.Config;

namespace Nistec.Caching.Session
{

   
    /// <summary>
    /// Represent the session cache.
    /// </summary>
    [Serializable]
    public class SessionCache : ISessionCache,IDisposable
    {

        #region members
        /// <summary>
        /// DEfault Session Sync Interval
        /// </summary>
        public const int DefaultSessionSyncIntervalMinute = 10;
        TimerDispatcher m_Timer;
        int m_SessionTimeout = 30;
        ConcurrentDictionary<string, SessionBag> m_SessionList;

        /// <summary>
        /// Get indicate whether the session cache was intialized.
        /// </summary>
        public bool Initialized
        {
            get { return Timer.Initialized; }
        }
        /// <summary>
        /// Get the sync iterval in seconds.
        /// </summary>
        public int IntervalSeconds
        {
            get { return Timer.IntervalSeconds; }
        }

        TimerDispatcher Timer
        {
            get
            {
                if (m_Timer == null)
                {
                    m_Timer = new TimerDispatcher(DefaultSessionSyncIntervalMinute * 60, 0, true);
                }
                return m_Timer;
            }
        }

        #endregion

        #region ctor
        /// <summary>
        /// Initialize a new instance of Session cache.
        /// </summary>
        public SessionCache()
        {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 100;
            this.m_SessionList = new ConcurrentDictionary<string, SessionBag>(concurrencyLevel, initialCapacity);
            m_SessionTimeout = CacheSettings.SessionTimeout;
            m_Timer = new TimerDispatcher(DefaultSessionSyncIntervalMinute * 60, 0, true);
        }
        #endregion

        #region IDispose
        /// <summary>
        /// Release all resources from current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_SessionList != null)
                {
                    m_SessionList.Clear();
                }
                if (m_Timer != null)
                {
                    m_Timer.Dispose();//.Clear();
                }
            }
        }

        #endregion


        /// <summary>
        /// Get all keys in session bag.
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            int count = m_SessionList.Keys.Count;
            if (count == 0)
                return null;

            string[] list = new string[m_SessionList.Keys.Count];
            m_SessionList.Keys.CopyTo(list, 0);
            return list;
        }

        #region Timer Sync

        /// <summary>
        /// Synchronize Start Event Handler.
        /// </summary>
        public event EventHandler SynchronizeStart;
        /// <summary>
        /// Session Removed Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SessionRemoved;
        /// <summary>
        /// Start Session cache.
        /// </summary>
        public void Start()
        {
            if (!m_Timer.Initialized)
            {
                m_Timer.SyncStarted += new EventHandler(m_Timer_SyncStarted);
                m_Timer.SyncCompleted += new SyncTimeCompletedEventHandler(m_Timer_SyncCompleted);
                m_Timer.Start();
            }
        }
        /// stop session cache.<summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (m_Timer.Initialized)
            {
                m_Timer.SyncStarted -= new EventHandler(m_Timer_SyncStarted);
                m_Timer.SyncCompleted -= new SyncTimeCompletedEventHandler(m_Timer_SyncCompleted);

                m_Timer.Stop();
            }
        }

        void m_Timer_SyncCompleted(object sender, SyncTimeCompletedEventArgs e)
        {
            Sync(e.Items);
        }

        void m_Timer_SyncStarted(object sender, EventArgs e)
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

        #region Sync


        internal void OnUsed(string sessionId)
        {
            Task.Factory.StartNew(() => m_Timer.Update(sessionId));
        }


        internal void Sync(string sessionId)
        {
            using (Task tsk = new Task(() =>
            {
                Sync(sessionId, false);
            }))
            {
                tsk.Start();
            }
        }

        bool Sync(string sessionId, bool addIfNotExists)
        {
            if (string.IsNullOrEmpty(sessionId))
                return false;

            bool ok = false;
            try
            {
                SessionBag entry;
                if (m_SessionList.TryGetValue(sessionId, out entry))
                {
                    entry.Sync();
                    ok = true;
                }
                else if (addIfNotExists)
                {
                    var sess= new SessionBag(this,sessionId, m_SessionTimeout);
                    sess.Sync();
                    m_SessionList[sessionId] = sess;
                    ok = true;
                }
            }
            catch (Exception ex)
            {
                DumpError("Sync", ex);
            }
            return ok;
        }

        /// <summary>
        /// Remove all items from session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal int RemoveSessionInternal(string sessionId)
        {
            OnSessionTimeout(new GenericEventArgs<string>(sessionId));
            return AgentManager.Cache.RemoveCacheSessionItemsAsync(sessionId);
        }
        /// <summary>
        /// Synchronize the current session.
        /// </summary>
        public void SyncSession()
        {
            OnSyncSession();
        }

        void Sync(string[] items)
        {

            if (items == null)
            {
                return;
            }
            try
            {

                foreach (string s in items)
                {
                    SessionBag entry;
                    m_SessionList.TryRemove(s, out entry);
                    m_Timer.Remove(s);
                }

                foreach (string s in items)
                {
                    RemoveSessionInternal(s);
                }
            }

            catch (Exception ex)
            {
                DumpError("Sync", ex);
            }
        }
        /// <summary>
        /// On Sync Session.
        /// </summary>
        protected virtual void OnSyncSession()
        {
            CacheLogger.Logger.Log("OnSyncSession Start");
            try
            {
                string[] items = m_Timer.GetTimedoutItems();

                if (items != null && items.Length > 0)
                {
                    using (Task tsk = new Task(() =>
                    {
                        Sync(items);
                    }))
                    {
                        tsk.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                DumpError("OnSyncSession", ex);
            }
            CacheLogger.Logger.Log("OnSyncSession End");
        }
        #endregion

        #region collections
        /// <summary>
        /// Get all sessions as collection of <see cref="SessionBag"/>.
        /// </summary>
        /// <returns></returns>
        public ICollection<SessionBag> GetAllSessions()
        {
            try
            {
                return m_SessionList.Values;
            }
            catch (Exception ex)
            {
                DumpError("GetAllSessions", ex);
            }
            return null;
        }
        /// <summary>
        /// Get all active sessions as collection of <see cref="SessionBag"/>.
        /// </summary>
        /// <returns></returns>
        public ICollection<SessionBag> GetActiveSessions()
        {
            ICollection<SessionBag> values = null;
            try
            {
                var k = m_SessionList.Values.Where(n => n.State == SessionState.Active);
                if (k != null)
                {
                    values = k.ToList();
                }

            }
            catch (Exception ex)
            {
                DumpError("GetActiveSessions", ex);
            }
            return values;
        }
        /// <summary>
        /// Get sessions size.
        /// </summary>
        /// <returns></returns>
        public long GetSessionsSize()
        {
            long size = 0;
            foreach (var k in m_SessionList.Values)
            {
                if (k != null)
                {
                    size += k.Size;
                }
            }
            return size;
        }

        /// <summary>
        /// Get all active sessions keys as collection of string.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetAllSessionsKeys()
        {
            return m_SessionList.Keys;
        }
        /// <summary>
        /// Returns all the sessions keys with specified condition.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public ICollection<string> GetAllSessionsStateKeys(SessionState state)
        {
            ICollection<string> keys = null;

            var k = from n in m_SessionList.Values where n.State == state select n.SessionId;
            if (k != null)
            {
                keys = k.ToList();
            }

            return keys;
        }

        /// <summary>
        /// Get all items keys in specified session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public ICollection<string> GetSessionsItemsKeys(string sessionId)
        {
            if (sessionId == null)
                return null;

            SessionBag item = null;
            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                return item.ItemsKeys();
            }
            return null;
        }
        #endregion

        #region Items
        /// <summary>
        /// Remove all items from spcific session.
        /// </summary>
        /// <param name="sessionId"></param>
        public void ClearItems(string sessionId)
        {
            if (sessionId == null)
                return;
            long size = 0;
            SessionBag item = null;
            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                size = item.Size;
                item.Clear();
                SizeExchage(size, 0, 1,0, false);
            }
        }
        /// <summary>
        /// Remove all sessions from session cache.
        /// </summary>
        public void Clear()
        {
            try
            {

                foreach (string key in m_SessionList.Keys)
                {
                    RemoveSessionInternal(key);
                }
                m_SessionList.Clear();

                m_Timer.Clear();

                SizeExchage(0, 0, 0,0, true);
            }
            catch (Exception ex)
            {
                DumpError("Clear", ex);
            }

        }

        /// <summary>
        /// Get existing session bag <see cref="SessionBag"/> from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public SessionBag GetExisting(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                OnUsed(sessionId);
            }

            return item;
        }

        /// <summary>
        /// Get existing session bag stream from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal SessionBagStream GetExistingBagStream(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                //OnUsed(sessionId);

                return item.GetSessionBagStream();
            }

            return null;
        }

        /// <summary>
        /// Get or create a new session bag if not exists.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal SessionBagStream GetOrCreateStream(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (!m_SessionList.TryGetValue(sessionId, out item))
            {
                item = new SessionBag(this, sessionId, m_SessionTimeout);
                m_SessionList[sessionId] = item;
                m_Timer.Add(sessionId, m_SessionTimeout);
            }

            OnUsed(sessionId);

            return item.GetSessionBagStream();
        }
        /// <summary>
        /// Get or create a new session bag if not exists.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public SessionBag GetOrCreate(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (!m_SessionList.TryGetValue(sessionId, out item))
            {
                item = new SessionBag(this, sessionId, m_SessionTimeout);
                m_SessionList[sessionId] = item;
                m_Timer.Add(sessionId, m_SessionTimeout);
            }

            OnUsed(sessionId);

            return item;
        }
        /// <summary>
        /// Remove item from session bag.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveItem(string sessionId, string key)
        {
            if (string.IsNullOrEmpty(sessionId))
                return false;

            if (string.IsNullOrEmpty(key))
                return false;

            if (m_SessionList.ContainsKey(sessionId))
            {
                return m_SessionList[sessionId].Remove(key);
            }

            return false;
        }

        /// <summary>
        /// Add a new <see cref="SessionEntry"/> item to session bag using arguments
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <param name="validateExisting"></param>
        /// <returns></returns>
        public CacheState AddItem(string sessionId,string key, object value, int expiration, bool validateExisting = false)
        {
            return AddItem(new SessionEntry(sessionId,key, value, expiration), validateExisting);
        }

        /// <summary>
        /// Add a new <see cref="SessionEntry"/> item to session bag using the SessionEntry.SessionId
        /// </summary>
        /// <param name="item"></param>
        /// <param name="validateExisting"></param>
        /// <returns></returns>
        public CacheState AddItem(SessionEntry item, bool validateExisting = false)
        {
            string sessionId = item.Id;

            //OnUsed(sessionId);
            SessionBag si = null;

            if (validateExisting)
                si = GetExisting(sessionId);
            else
                si = GetOrCreate(sessionId);

            if (si == null)
            {
                return CacheState.AddItemFailed;
            }
            si.AddItem(item);
            return CacheState.ItemAdded;
        }

        /// <summary>
        /// Determines whether the session bag contains the specified sessionid and key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string sessionId, string key)
        {
            if (sessionId == null)
                return false;

            SessionBag si;
            if (m_SessionList.TryGetValue(sessionId, out si))
            {
                return si.Exists(key);
            }
            return false;
        }

        /// <summary>
        /// Get item value from session using session id and item key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetItemValue<T>(string sessionId, string key)
        {
            SessionEntry entry = GetItem(sessionId, key);
            if(entry==null)
                return default(T);
            return GenericTypes.Cast<T>(entry.DecodeBody());
        }

        /// <summary>
        /// Fetch item value from session using session id and item key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public T FetchItemValue<T>(string sessionId, string key)
        {
            SessionEntry entry = FetchItem(sessionId, key);
            if (entry == null)
                return default(T);
            return GenericTypes.Cast<T>(entry.DecodeBody());
        }

        /// <summary>
        /// Get <see cref="SessionEntry"/> item from session using session id and item key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public SessionEntry GetItem(string sessionId, string key)
        {
            if (sessionId == null)
                return null;

            SessionBag si;
            if (m_SessionList.TryGetValue(sessionId, out si))
            {
                //OnUsed(sessionId);
                return si.Get(key);
            }

            return null;
        }
        /// <summary>
        /// Fetch <see cref="SessionEntry"/> item from session using session id and item key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public SessionEntry FetchItem(string sessionId, string key)
        {
            if (sessionId == null)
                return null;
            SessionEntry o = null;

            SessionBag si;
            if (m_SessionList.TryGetValue(sessionId, out si))
            {
                o = si.Get(key);
                si.Remove(key);
            }

            return o;
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
        public CacheState CopyTo(string sessionId, string key, string targetKey, int expiration, bool addToCache = false)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;
            if (string.IsNullOrEmpty(targetKey))
                return CacheState.ArgumentsError;
            
            SessionEntry o = GetItem(sessionId, key);

            if (o == null)
                return CacheState.InvalidItem;
            o.Key = targetKey;
            o.Expiration = expiration;

            if (addToCache)
            {
                return AgentManager.Cache.AddItem(o);
            }
            return AddItem(o);
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
        public CacheState FetchTo(string sessionId, string key, string targetKey, int expiration, bool addToCache = false)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            SessionEntry o = FetchItem(sessionId, key);

            if (o == null)
                return CacheState.InvalidItem;
            o.Key = targetKey;
            o.Expiration = expiration;
           
            if (addToCache)
            {
                return AgentManager.Cache.AddItem(o);
            }
            return AddItem(o);
        }

        #endregion

        #region Add remove methods

       

        /// <summary>
        /// Add a new session to session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <param name="timeout"></param>
        /// <param name="args"></param>
        public CacheState AddSession(string sessionId, string userId, int timeout, string args)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            if (!m_SessionList.ContainsKey(sessionId))
            {
                var sess = new SessionBag(this, sessionId, userId, timeout, args);
                m_SessionList[sessionId] = sess;
                m_Timer.Add(sessionId, timeout);
                return CacheState.ItemAdded;
            }
            return CacheState.ItemAllreadyExists;
        }
        /// <summary>
        /// Add a new session to session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <param name="args"></param>
        public CacheState AddSession(string sessionId, string userId, string args)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            if (!m_SessionList.ContainsKey(sessionId))
            {
                var sess = new SessionBag(this, sessionId, userId, m_SessionTimeout, args);
                m_SessionList[sessionId] = sess;
                m_Timer.Add(sessionId, m_SessionTimeout);
                return CacheState.ItemAdded;
            }
            return CacheState.ItemAllreadyExists;
        }

      
        /// <summary>
        /// Add a new session to session cache.
        /// </summary>
        /// <param name="item"></param>
        public CacheState Add(SessionBag item)
        {
            if (item == null)
                return CacheState.ArgumentsError;
            if (string.IsNullOrEmpty(item.SessionId))
                return CacheState.ArgumentsError;
            if (!m_SessionList.ContainsKey(item.SessionId))
            {
                m_SessionList[item.SessionId] = item;
                m_Timer.Add(item.SessionId, item.Timeout);
                SizeExchage(0,item.Size,0,1,false);
                return CacheState.ItemAdded;
            }
            return CacheState.ItemAllreadyExists;
        }
        /// <summary>
        /// Remove session from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public CacheState Remove(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            try
            {
                SessionBag entry;
                if (m_SessionList.TryRemove(sessionId, out entry))
                {
                    RemoveSessionInternal(sessionId);
                     m_Timer.Remove(sessionId);
                     SizeExchage(entry.Size, 0, 1,0,false);
                     return CacheState.ItemRemoved;
                }
                return CacheState.InvalidSession;
            }
            catch (Exception ex)
            {
                DumpError("Remove", ex);
                return CacheState.UnexpectedError;
            }
        }

        /// <summary>
        /// Refresh sfcific session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        public void Refresh(string sessionId)
        {
            if (sessionId == null)
                return;

            try
            {
                SessionBag si;

                if (m_SessionList.TryGetValue(sessionId, out si))
                {
                    si.Sync();
                }

            }
            catch (Exception ex)
            {
                DumpError("Refresh", ex);
            }
        }

        /// <summary>
        /// Refresh sfcific session in session cache or create a new session bag if not exists.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public void RefreshOrCreate(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return;

            SessionBag si = null;

            try
            {
                if (m_SessionList.TryGetValue(sessionId, out si))
                {
                    si.Sync();
                }
                else
                {
                    si = new SessionBag(this, sessionId, m_SessionTimeout);
                    m_SessionList[sessionId] = si;
                    m_Timer.Add(sessionId, m_SessionTimeout);
                }
            }
            catch (Exception ex)
            {
                DumpError("RefreshOrCreate", ex);
            }

            OnUsed(sessionId);
        }
        #endregion

        /// <summary>
        /// Dump Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        protected void DumpError(string message, Exception ex)
        {
            CacheLogger.Logger.Log(message + " error: " + ex.Message);
            System.Diagnostics.Trace.WriteLine(message + " error: " + ex.Message);
        }

        /// <summary>
        /// On Session Timeout event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSessionTimeout(GenericEventArgs<string> e)
        {
            if (SessionRemoved != null)
            {
                SessionRemoved(this, e);
            }
        }

        #region size exchange

        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeValidate(long newSize)
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
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected virtual CacheState SizeExchage(ISessionBag oldItem, ISessionBag newItem)
        {
            return CacheState.Ok;
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {

        }

        #endregion

        #region ISessionCache

        CacheState ISessionCache.SizeExchage(long currentSize, long newSize, int currentCount,int newCount ,bool exchange)
        {
            return SizeExchage(currentSize, newSize, currentCount,newCount, exchange);
        }

        CacheState ISessionCache.SizeExchage(ISessionBag oldItem, ISessionBag newItem)
        {
            return SizeExchage(oldItem, newItem);
        }

        void ISessionCache.SizeRefresh()
        {
            SizeRefresh();
        }

        #endregion
    }
   
}
