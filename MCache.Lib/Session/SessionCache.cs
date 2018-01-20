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
using Nistec.Channels;
using System.Data;
using System.Threading;

namespace Nistec.Caching.Session
{

   
    /// <summary>
    /// Represent the session cache.
    /// </summary>
    [Serializable]
    public class SessionCache : ISessionCache, ICachePerformance, IDisposable
    {

        #region ICachePerformance

        CachePerformanceCounter m_Perform;
        /// <summary>
        /// Get <see cref="CachePerformanceCounter"/> Performance Counter.
        /// </summary>
        public CachePerformanceCounter PerformanceCounter
        {
            get { return m_Perform; }
        }

        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void ICachePerformance.MemorySizeExchange(ref long memorySize)
        {
            CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.None, "Memory Size Exchange: SessionCache");
            long size = GetSessionsSize();
            Interlocked.Exchange(ref memorySize, size);
        }

        //internal long MaxSize { get { return 999999999L; } }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long ICachePerformance.GetMaxSize()
        {
            return CacheSettings.MaxSize;
        }
        bool ICachePerformance.IsRemote
        {
            get { return true; }
        }
        int ICachePerformance.IntervalSeconds
        {
            get { return this.IntervalSeconds; }
        }
        bool ICachePerformance.Initialized
        {
            get { return this.Initialized; }
        }
        #endregion

        #region members
        /// <summary>
        /// DEfault Session Sync Interval
        /// </summary>
        public const int DefaultSessionSyncIntervalMinute = 10;
        public readonly int DefaultSessionExpirationMinute = 30;

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
            DefaultSessionExpirationMinute = CacheSettings.SessionTimeout;
            if (DefaultSessionExpirationMinute <= 0)
                DefaultSessionExpirationMinute = 30;
            m_Perform = new CachePerformanceCounter(this, CacheAgentType.SessionCache, "SessionAgent");

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
                //m_Timer.SyncCompleted += new SyncTimeCompletedEventHandler(m_Timer_SyncCompleted);
                m_Timer.StateChanged += M_Timer_StateChanged;
                m_Timer.Start();
            }
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "SessionCache Started!");
        }
        /// stop session cache.<summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (m_Timer.Initialized)
            {
                m_Timer.SyncStarted -= new EventHandler(m_Timer_SyncStarted);
                //m_Timer.SyncCompleted -= new SyncTimeCompletedEventHandler(m_Timer_SyncCompleted);
                m_Timer.StateChanged += M_Timer_StateChanged;
                m_Timer.Stop();
            }
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "SessionCache Stoped!");
        }

        private void M_Timer_StateChanged(object sender, SyncTimerItemEventArgs e)
        {
            if (e.Items == null)
                return;
            foreach (var entry in e.Items)
            {
                if (entry.Value.Source == TimerSource.Session && entry.Value.State == 2)
                {
                    RemoveSessionAsync(entry.Key);
                }
            }
        }

        public void RemoveSessionAsync(string sessionId)
        {
            try
            {
                Task tsk = new Task(() =>
                {
                    RemoveSession(sessionId);
                });
                {
                    tsk.Start();
                }
                tsk.TryDispose();

            }
            catch (Exception ex)
            {
                DumpError("RemoveSessionAsync", ex);
            }
        }
    
        //void m_Timer_SyncCompleted(object sender, SyncTimeCompletedEventArgs e)
        //{
        //    //Sync(e.Items);

        //    OnSyncSession(e.Items);
        //}

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


        internal void OnUsed(string sessionId, int timeout)
        {
            Task.Factory.StartNew(() => m_Timer.UpdateTimer(sessionId));//, timeout,DefaultSessionExpirationMinute));
        }


        internal void Sync(string sessionId)
        {
            Task task = new Task(() =>
            {
                Sync(sessionId, false);
            });
            {
                task.Start();
            }
            task.TryDispose();
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
        internal CacheState RemoveSessionInternal(string sessionId)
        {
            OnSessionTimeout(new GenericEventArgs<string>(sessionId));
            return AgentManager.Cache.RemoveCacheSessionItemsAsync(sessionId);
        }

        ///// <summary>
        ///// Synchronize the current session.
        ///// </summary>
        //public void SyncSession()
        //{
        //    OnSyncSession();
        //}

        void SyncRemove(string[] items)
        {

            if (items == null)
            {
                return;
            }
            try
            {

                foreach (string s in items)
                {
                    RemoveSession(s);
                    //SessionBag entry;
                    //m_SessionList.TryRemove(s, out entry);
                    //m_Timer.Remove(s);
                    //CacheLogger.Logger.LogAction(CacheAction.SessionCache, CacheActionState.Debug, "SessionBag removed : " + s);
                }

                //foreach (string s in items)
                //{
                //    RemoveSessionInternal(s);
                //}
            }

            catch (Exception ex)
            {
                DumpError("Sync", ex);
            }
        }
        /// <summary>
        /// On Sync Session.
        /// </summary>
        protected virtual void OnSyncSession(string[] items)
        {
            CacheLogger.Logger.LogAction( CacheAction.SessionCache, CacheActionState.Info,"OnSyncSession Start");
            try
            {
                //string[] items = m_Timer.GetTimedoutItems();

                if (items != null && items.Length > 0)
                {
                    Task tsk = new Task(() =>
                    {
                        SyncRemove(items);
                    });
                    {
                        tsk.Start();
                    }
                    tsk.TryDispose();
                }
            }
            catch (Exception ex)
            {
                DumpError("OnSyncSession", ex);
            }
            CacheLogger.Logger.LogAction(CacheAction.SessionCache, CacheActionState.Info, "OnSyncSession End");
        }
        #endregion

        #region collections

        internal void ChangeState(string sessionId, int state)
        {
            SessionBag bag;
           if( m_SessionList.TryGetValue(sessionId,out bag))
            {
                bag.State =(SessionState) state;
            }
        }

        /// <summary>
        /// Get all sessions as collection of <see cref="SessionBag"/>.
        /// </summary>
        /// <returns></returns>
        public SessionBag[] GetAllSessions()
        {
            try
            {
                return m_SessionList.Values.ToArray();
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
        public SessionBag[] GetActiveSessions()
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
            return values.ToArray();
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
        public string[] ViewAllSessionsKeys()
        {
            return m_SessionList.Keys.ToArray();
        }
        /// <summary>
        /// Returns all the sessions keys with specified condition.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public string[] ViewAllSessionsKeysByState(SessionState state)        {
            ICollection<string> keys = null;

            var k = from n in m_SessionList.Values where n.State == state select n.SessionId;
            if (k != null)
            {
                keys = k.ToList();
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Get all items keys in specified session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public string[] ViewSessionKeys(string sessionId)
        {
            if (sessionId == null)
                return null;

            SessionBag item = null;
            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                return item.ItemsKeys().ToArray();
            }
            return null;
        }
        #endregion

        #region Report


        internal CacheItemReport GetTimerReport()
        {
            return m_Timer.GetReport("Session");
        }

        /// <summary>
        /// Cache Item Schema as <see cref="DataTable"/> class.
        /// </summary>
        /// <returns></returns>
        internal static DataTable ReportSchema()
        {
            DataTable dt = new DataTable("SessionBag");
            dt.Columns.Add("SessionKey", typeof(string));
            dt.Columns.Add("Body", typeof(string));
            dt.Columns.Add("TypeName", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("SessionId", typeof(string));
            dt.Columns.Add("Creation", typeof(DateTime));
            dt.Columns.Add("Timeout", typeof(int));
            dt.Columns.Add("Size", typeof(int));
            dt.Columns.Add("LastUsed", typeof(string));
            dt.Columns.Add("UserId", typeof(int));
            return dt.Clone();
        }
        internal CacheItemReport GetReport()
        {
            var data = SessionCacheReport(true);
            if (data == null)
                return null;
            return new CacheItemReport() { Count = data.Rows.Count, Data = data, Name = "Session Cache Report", Size = 0 };
        }

        /// <summary>
        /// Save all cache item to <see cref="DataSet"/>.
        /// </summary>
        /// <returns></returns>
        public DataTable SessionCacheReport(bool noBody)
        {
            DataTable table;
            //this.LogAction(CacheAction.General, CacheActionState.None, " SessionCacheToDataTable");
            try
            {
                ICollection<SessionBag> items = this.m_SessionList.Values.ToArray();
                if ((items == null) || (items.Count == 0))
                {
                    return null;
                }
                table = ReportSchema();
                foreach (var bag in items)
                {
                    var entryList = bag.ItemsValues().ToArray();
                    foreach (SessionEntry item in entryList)
                    {
                        table.Rows.Add(item.ToDataRow(bag, noBody));
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return table;
        }
        #endregion

        #region Items

        //internal TransStream ClearSessionItemsStream(string sessionId)
        //{
        //    var state = ClearItems(sessionId);
        //    return new TransStream((int)state, TransType.State);
        //}
        /// <summary>
        /// Remove all items from spcific session.
        /// </summary>
        /// <param name="sessionId"></param>
        public CacheState ClearItems(string sessionId)
        {
            try
            {
                if (sessionId == null)
                return CacheState.ArgumentsError;
            long size = 0;
            SessionBag item = null;
            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                size = item.Size;
                item.Clear();
                SizeExchage(size, 0, 1,0, false);
            }
                return CacheState.Ok;
            }
            catch (Exception ex)
            {
                DumpError("ClearItems", ex);
                return CacheState.UnexpectedError;
            }
        }
        /// <summary>
        /// Remove all sessions from session cache.
        /// </summary>
        public CacheState ClearAll()
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
                return  CacheState.Ok;
            }
            catch (Exception ex)
            {
                DumpError("Clear", ex);
                return  CacheState.UnexpectedError;
            }
        }

        //internal TransStream ClearAllSessionsStream()
        //{
        //    var state = Clear();
        //    return new TransStream((int)state, TransType.State);
        //}

        /// <summary>
        /// Get existing session bag <see cref="SessionBag"/> from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public SessionBagStream GetExisting(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                OnUsed(sessionId, item.Timeout);
            }

            return item.GetSessionBagStream();
        }

        SessionBag GetExistingInternal(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                OnUsed(sessionId, item.Timeout);
            }

            return item;
        }

        //internal TransStream GetExistingSessionStream(string sessionId)
        //{
        //    SessionBagStream bag = GetExistingBagStream(sessionId);
        //    if (bag == null)
        //    {
        //        new TransStream(CacheState.NotFound.ToString(), TransType.Error);
        //    }
        //    return new TransStream(bag);
        //}

        /// <summary>
        /// Get existing session bag stream from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal SessionBagStream GetSessionBagStream(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                OnUsed(sessionId, item.Timeout);

                return item.GetSessionBagStream();
            }

            return null;
        }

        internal SessionBagStream ViewSessionBagStream(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                return item.GetSessionBagStream();
            }

            return null;

        }
        //internal TransStream GetExistingSessionRecordStream(string sessionId)
        //{
        //    var bag= GetExistingBagRecord(sessionId);
        //    if (bag == null)
        //    {
        //        new TransStream(CacheState.NotFound.ToString(), TransType.Error);
        //    }
        //    return new TransStream(bag);
        //}

        /// <summary>
        /// Get existing session bag stream from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal dynamic GetExistingBagRecord(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                OnUsed(sessionId,item.Timeout);
                return item.ToEntity();
            }

            return null;
        }

        /// <summary>
        /// Get existing session items from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal IDictionary<string, object> GetSessionItems(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;
            SessionBag item = null;

            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                OnUsed(sessionId, item.Timeout);
                return item.ToDictionary();
            }

            return null;
        }

        ///// <summary>
        ///// Get or create a new session bag if not exists.
        ///// </summary>
        ///// <param name="sessionId"></param>
        ///// <returns></returns>
        //internal TransStream GetOrCreateSessionStream(string sessionId)
        //{
        //    SessionBagStream bag= GetOrCreateBagStream(sessionId);
        //    if(bag==null)
        //    {
        //        new TransStream(CacheState.UnexpectedError.ToString(), TransType.Error);
        //    }
        //    return new TransStream(bag);
        //}
        internal SessionBagStream GetOrCreateSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (!m_SessionList.TryGetValue(sessionId, out item))
            {
                item = new SessionBag(this, sessionId, m_SessionTimeout);
                m_Timer.AddOrUpdate(TimerSource.Session, sessionId, m_SessionTimeout, DefaultSessionExpirationMinute);
                m_SessionList[sessionId] = item;
            }

            OnUsed(sessionId, item.Timeout);

            return item.GetSessionBagStream();
        }

        //internal TransStream GetOrCreateSessionRecordStream(string sessionId)
        //{
        //    var record= GetOrCreateRecord(sessionId);
        //    if (record == null)
        //    {
        //        new TransStream(CacheState.UnexpectedError.ToString(), TransType.Error);
        //    }
        //    return new TransStream(record);
        //}

        /// <summary>
        /// Get or create a new session bag if not exists.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal dynamic GetOrCreateRecord(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (!m_SessionList.TryGetValue(sessionId, out item))
            {
                item = new SessionBag(this, sessionId, m_SessionTimeout);
                m_Timer.AddOrUpdate(TimerSource.Session, sessionId, m_SessionTimeout, DefaultSessionExpirationMinute);
                m_SessionList[sessionId] = item;
            }

            OnUsed(sessionId, item.Timeout);

            return item.ToEntity();
        }
        /// <summary>
        /// Get or create a new session bag if not exists.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public SessionBagStream GetOrCreate(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (!m_SessionList.TryGetValue(sessionId, out item))
            {
                item = new SessionBag(this, sessionId, m_SessionTimeout);
                m_Timer.AddOrUpdate(TimerSource.Session, sessionId, m_SessionTimeout, DefaultSessionExpirationMinute);
                m_SessionList[sessionId] = item;
            }

            OnUsed(sessionId, item.Timeout);

            return item.GetSessionBagStream(); 
        }

        SessionBag GetOrCreateInternal(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            SessionBag item = null;

            if (!m_SessionList.TryGetValue(sessionId, out item))
            {
                item = new SessionBag(this, sessionId, m_SessionTimeout);
                m_Timer.AddOrUpdate(TimerSource.Session, sessionId, m_SessionTimeout, DefaultSessionExpirationMinute);
                m_SessionList[sessionId] = item;
            }

            OnUsed(sessionId, item.Timeout);

            return item;
        }

      
        //internal TransStream RemoveSessionItemStream(string sessionId, string key)
        //{
        //    bool ok=RemoveItem(sessionId, key);
        //    CacheState state = ok ? CacheState.ItemRemoved : CacheState.RemoveItemFailed;
        //    return new TransStream((int)state, TransType.State);
        //}

        /// <summary>
        /// Remove item from session bag.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public CacheState RemoveItem(string sessionId, string key)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            if (string.IsNullOrEmpty(key))
                return CacheState.ArgumentsError;
            SessionBag item;
            if (m_SessionList.TryGetValue(sessionId, out item))
            {
                if (item.Remove(key))//m_SessionList[sessionId].Remove(key))
                {
                    return CacheState.ItemRemoved;
                }
            }
            else
                return CacheState.NotFound;

            return CacheState.RemoveItemFailed;
        }

        /// <summary>
        /// Add a new <see cref="SessionEntry"/> item to session bag using arguments
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public CacheState AddItem(SessionEntry entry)
        {
            SessionBag si = GetExistingInternal(entry.GroupId);
            if (si == null)
            {
                return CacheState.AddItemFailed;
            }
            if (si.Exists(entry.Id))
            {
                return CacheState.ItemAllreadyExists;
            }
            return si.AddItem(entry);
        }

        /// <summary>
        /// Add a new <see cref="SessionEntry"/> item to session bag using arguments
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public CacheState AddItem(string sessionId, string key, object value, int expiration)
        {
            return AddItem(new SessionEntry(sessionId, key, value, expiration));

            //SessionBag si = GetExistingInternal(sessionId);
            //if (si == null)
            //{
            //    return CacheState.AddItemFailed;
            //}
            //if (si.Exists(key))
            //{
            //    return CacheState.ItemAllreadyExists;
            //}
            //si.AddItem(new SessionEntry(sessionId, key, value, expiration));
            //return CacheState.ItemAdded;
        }

        /// <summary>
        /// Add or Set <see cref="SessionEntry"/> item to session bag using arguments
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public CacheState SetItem(SessionEntry entry)
        {
            SessionBag si = GetOrCreateInternal(entry.GroupId);
            if (si == null)
            {
                return CacheState.AddItemFailed;
            }
            return si.SetItem(entry);
        }

        /// <summary>
        /// Add or Set <see cref="SessionEntry"/> item to session bag using arguments
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public CacheState SetItem(string sessionId, string key, object value, int expiration)
        {

            return SetItem(new SessionEntry(sessionId, key, value, expiration));

            //SessionBag si = GetOrCreateInternal(sessionId);
            //if (si == null)
            //{
            //    return CacheState.AddItemFailed;
            //}
            //if (si.Exists(key))
            //{
            //    return CacheState.ItemAllreadyExists;
            //}
            //si.AddItem(new SessionEntry(sessionId, key, value, expiration));
            //return CacheState.ItemAdded;
        }


        ///// <summary>
        ///// Add a new <see cref="SessionEntry"/> item to session bag using arguments
        ///// </summary>
        ///// <param name="sessionId"></param>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        ///// <param name="expiration"></param>
        ///// <param name="validateExisting"></param>
        ///// <returns></returns>
        //public CacheState AddItem(string sessionId,string key, object value, int expiration, bool validateExisting = false)
        //{
        //    return AddItem(new SessionEntry(sessionId,key, value, expiration), validateExisting);
        //}

        ///// <summary>
        ///// Add a new <see cref="SessionEntry"/> item to session bag using the SessionEntry.SessionId
        ///// </summary>
        ///// <param name="item"></param>
        ///// <param name="validateExisting"></param>
        ///// <returns></returns>
        //public CacheState AddItem(SessionEntry item, bool validateExisting = false)
        //{
        //    string sessionId = item.Id;

        //    //OnUsed(sessionId);
        //    SessionBag si = null;

        //    if (validateExisting)
        //        si = GetExistingInternal(sessionId);
        //    else
        //        si = GetOrCreateInternal(sessionId);

        //    if (si == null)
        //    {
        //        return CacheState.AddItemFailed;
        //    }
        //    si.AddItem(item);
        //    return CacheState.ItemAdded;
        //}

        //internal TransStream AddItemExistingStream(SessionEntry item, bool validateExisting = false)
        //{
        //    var state = this.AddItem(item, validateExisting);
        //    return new TransStream((int)state, TransType.State);
        //}

        /// <summary>
        /// Determines whether the session bag contains the specified sessionid and key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public CacheState Exists(string sessionId, string key)
        {
            if (sessionId == null)
                return CacheState.ArgumentsError;

            SessionBag si;
            if (m_SessionList.TryGetValue(sessionId, out si))
            {
                if (si.Exists(key))
                    return CacheState.Ok;
            }
            return  CacheState.NotFound;
        }

        /// <summary>
        /// Get item value from session using session id and item key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public NetStream GetSessionValueStream(string sessionId, string key)
        {
            SessionEntry entry = GetEntry(sessionId, key);
            return (entry == null) ? null : entry.GetStream();
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
            SessionEntry entry = GetEntry(sessionId, key);
            if(entry==null)
                return default(T);
            return GenericTypes.Cast<T>(entry.DecodeBody(), true);
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
            return GenericTypes.Cast<T>(entry.DecodeBody(), true);
        }

        //internal TransStream GetSessionItemStream(string sessionId, string key)
        //{
        //    SessionEntry entry = GetItem(sessionId, key);
        //    if (entry == null)
        //    {
        //        new TransStream(CacheState.NotFound.ToString(), TransType.Error);
        //    }
        //    return new TransStream(entry);
        //}

        /// <summary>
        /// Get <see cref="SessionEntry"/> item from session using session id and item key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public SessionEntry GetEntry(string sessionId, string key)
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

        internal SessionEntry ViewEntry(string sessionId, string key)
        {
            if (sessionId == null)
                return null;

            SessionBag si;
            if (m_SessionList.TryGetValue(sessionId, out si))
            {
                return si.View(key);
            }

            return null;
        }

        //internal TransStream GetSessionItemRecordStream(string sessionId, string key)
        //{
        //    var entry = GetItemRecord(sessionId, key);
        //    if (entry == null)
        //    {
        //        new TransStream(CacheState.NotFound.ToString(), TransType.Error);
        //    }
        //    return new TransStream(entry);
        //}

        /// <summary>
        /// Get <see cref="SessionEntry"/> item from session using session id and item key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public DynamicEntity GetItemRecord(string sessionId, string key)
        {
            if (sessionId == null)
                return null;

            SessionEntry entry = GetEntry(sessionId, key);
            if (entry==null)
            {
                return null;
            }

            return entry.ToEntity();
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
        /// Fetch <see cref="SessionEntry"/> item from session using session id and item key.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public DynamicEntity FetchItemRecord(string sessionId, string key)
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

            return o.ToEntity();
        }

        //internal TransStream CopyToWithAck(string sessionId, string key, string targetKey, int expiration, bool addToCache = false)
        //{
        //    var state = CopyTo(sessionId, key, targetKey, expiration, addToCache);
        //    //return SessionEntry.GetAckStream(state, "CopySessionTo: " + sessionId + ", Target: " + targetKey + ", State:" + state.ToString());
        //    return new TransStream((int)state, TransType.State);
        //}

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
            
            SessionEntry o = GetEntry(sessionId, key);

            if (o == null)
                return CacheState.InvalidItem;
            o.Id = targetKey;
            o.Expiration = expiration;

            if (addToCache)
            {
                return AgentManager.Cache.Add(o);
            }
            return AddItem(o);
        }

        //internal TransStream FetchToWithAck(string sessionId, string key, string targetKey, int expiration, bool addToCache = false)
        //{
        //    var state = CopyTo(sessionId, key, targetKey, expiration, addToCache);
        //    //SessionEntry.GetAckStream(state, "FetchSessionTo: " + sessionId + ", Target: " + targetKey + ", State:" + state.ToString());
        //    return  new TransStream((int)state, TransType.State); 
        //}

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
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            SessionEntry o = FetchItem(sessionId, key);

            if (o == null)
                return CacheState.InvalidItem;
            o.Id = targetKey;
            o.Expiration = expiration;
           
            if (addToCache)
            {
                return AgentManager.Cache.Add(o);
            }
            return AddItem(o);
        }

        #endregion

        #region Add remove methods

        //internal TransStream AddSessionWithAck(string sessionId, string userId, int timeout, string args)
        //{
        //    CacheState state = AddSession(sessionId, userId, timeout, args);
        //    return new TransStream((int)state,TransType.State);
        //}

        /// <summary>
        /// Add a new session to session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <param name="timeout"></param>
        /// <param name="args"></param>
        public CacheState CreateSession(string sessionId, string userId, int timeout, string args)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            if (!m_SessionList.ContainsKey(sessionId))
            {
                var sess = new SessionBag(this, sessionId, userId, timeout, args);
                m_Timer.AddOrUpdate(TimerSource.Session, sessionId, timeout, DefaultSessionExpirationMinute);
                m_SessionList[sessionId] = sess;
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
        public CacheState CreateSession(string sessionId, string userId, string args)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            if (!m_SessionList.ContainsKey(sessionId))
            {
                var sess = new SessionBag(this, sessionId, userId, m_SessionTimeout, args);
                m_Timer.AddOrUpdate(TimerSource.Session, sessionId, m_SessionTimeout, DefaultSessionExpirationMinute);
                m_SessionList[sessionId] = sess;
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
                m_Timer.AddOrUpdate(TimerSource.Session, item.SessionId, item.Timeout, DefaultSessionExpirationMinute);
                m_SessionList[item.SessionId] = item;
                SizeExchage(0,item.Size,0,1,false);
                return CacheState.ItemAdded;
            }
            return CacheState.ItemAllreadyExists;
        }

        //internal TransStream RemoveSessionStream(string sessionId)
        //{
        //    var state = Remove(sessionId);
        //    return new TransStream((int)state, TransType.State);
        //}

        /// <summary>
        /// Remove session from session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public CacheState RemoveSession(string sessionId)
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
                else
                {
                    m_Timer.Remove(sessionId);
                }
                CacheLogger.Logger.LogAction(CacheAction.SessionCache, CacheActionState.Debug, "Session removed : " + sessionId);

                return CacheState.InvalidSession;
            }
            catch (Exception ex)
            {
                DumpError("Remove", ex);
                return CacheState.UnexpectedError;
            }
        }

        //internal TransStream SessionRefreshStream(string sessionId)
        //{
        //    var state = Refresh(sessionId);
        //    return new TransStream((int)state, TransType.State);
        //}

        /// <summary>
        /// Refresh sfcific session in session cache.
        /// </summary>
        /// <param name="sessionId"></param>
        public CacheState Refresh(string sessionId)
        {
            if (sessionId == null)
                return CacheState.ArgumentsError;

            try
            {
                SessionBag si;

                if (m_SessionList.TryGetValue(sessionId, out si))
                {
                    si.Sync();
                }
                return CacheState.Ok;
            }
            catch (Exception ex)
            {
                DumpError("Refresh", ex);
                return CacheState.UnexpectedError;
            }
        }

        //internal TransStream RefreshOrCreateStream(string sessionId)
        //{
        //    var state = RefreshOrCreate(sessionId);
        //    return new TransStream((int)state, TransType.State);
        //}

        /// <summary>
        /// Refresh sfcific session in session cache or create a new session bag if not exists.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public CacheState RefreshOrCreate(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return CacheState.ArgumentsError;

            SessionBag si = null;
            CacheState state;
            try
            {
                if (m_SessionList.TryGetValue(sessionId, out si))
                {
                    si.Sync();
                }
                else
                {
                    si = new SessionBag(this, sessionId, m_SessionTimeout);
                    m_Timer.AddOrUpdate(TimerSource.Session, sessionId, m_SessionTimeout, DefaultSessionExpirationMinute);
                    m_SessionList[sessionId] = si;
                }
                state = CacheState.Ok;
            }
            catch (Exception ex)
            {
                DumpError("RefreshOrCreate", ex);
                state = CacheState.UnexpectedError;
            }

            OnUsed(sessionId, si.Timeout);
            return state;
        }
        #endregion

        /// <summary>
        /// Dump Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        protected void DumpError(string message, Exception ex)
        {
            CacheLogger.Logger.LogAction( CacheAction.SessionCache, CacheActionState.Error,message + " error: " + ex.Message);
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
            if (!CacheSettings.EnableSizeHandler)
                return CacheState.Ok;
            return PerformanceCounter.SizeValidate(newSize);

            //return CacheState.Ok;
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
        internal protected virtual void SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;
            PerformanceCounter.ExchangeSizeAndCountAsync(currentSize, newSize, currentCount, newCount, exchange, CacheSettings.EnableSizeHandler);

            //return CacheState.Ok;
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected virtual void SizeExchage(ISessionBag oldItem, ISessionBag newItem)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;
            PerformanceCounter.ExchangeSizeAndCountAsync(oldItem.Size, newItem.Size, oldItem.Count, newItem.Count, false, CacheSettings.EnableSizeHandler);

            //return CacheState.Ok;
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected virtual void SizeRefresh()
        {
            if (CacheSettings.EnablePerformanceCounter)
            {
                PerformanceCounter.RefreshSize();
            }
        }

        #endregion

        #region ISessionCache

        void ISessionCache.SizeExchage(long currentSize, long newSize, int currentCount,int newCount ,bool exchange)
        {
            SizeExchage(currentSize, newSize, currentCount,newCount, exchange);
        }

        void ISessionCache.SizeExchage(ISessionBag oldItem, ISessionBag newItem)
        {
            SizeExchage(oldItem, newItem);
        }

        void ISessionCache.SizeRefresh()
        {
            SizeRefresh();
        }

        #endregion
    }
   
}
