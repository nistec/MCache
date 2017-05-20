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
using Nistec.Threading;
using Nistec.Caching.Remote;
using System.Collections.Concurrent;

namespace Nistec.Caching
{
    internal class TimerDispatcher : IDisposable
    {
        private ThreadTimer SettingTimer;
        private ConcurrentDictionary<string, DateTime> m_Timer;

        public int IntervalSeconds
        {
            get;
            internal set;
        }

        public bool IsRemote
        {
            get;
            internal set;
        }

        public bool Initialized
        {
            get;
            private set;
        }

        public DateTime LastSyncTime
        {
            get;
            private set;
        }
        public CacheSyncState SyncState
        {
            get;
            private set;
        }

        public DateTime NextSyncTime
        {
            get
            {
                return this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
            }
        }

        public TimerDispatcher(int intervalSeconds, int initialCapacity, bool isRemote)
        {

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            if (initialCapacity < 100)
                initialCapacity = 101;

            m_Timer = new ConcurrentDictionary<string, DateTime>(concurrencyLevel, initialCapacity);


            this.IntervalSeconds = intervalSeconds;
            this.IsRemote = isRemote;
            this.Initialized = false;
            this.SyncState = CacheSyncState.Idle;
            this.LastSyncTime = DateTime.Now;
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized TimeoutDispatcher");
        }

        public void Dispose()
        {
            if (SettingTimer != null)
            {
                DisposeTimer();
            }
        }

        #region events

        public event EventHandler SyncStarted;

        public event SyncTimeCompletedEventHandler SyncCompleted;

        protected virtual void OnSyncStarted(EventArgs e)
        {
            this.OnSyncTimer();

            if (this.SyncStarted != null)
            {
                this.SyncStarted(this, e);
            }
        }

        protected virtual void OnSyncCompleted(SyncTimeCompletedEventArgs e)
        {
            if (this.SyncCompleted != null)
            {
                this.SyncCompleted(this, e);
            }
        }

        #endregion

        #region cache timeout

        public void Add(CacheEntry item)
        {
                if (item.AllowExpires)
                {
                   
                    m_Timer[item.Key] = item.ExpirationTime;

                }
            
        }

        public void Add(string key, DateTime time)
        {
            if (string.IsNullOrEmpty(key))
                return;

                m_Timer[key] = time;
            
        }

        public void Add(string key, int expiration)//, bool chekExists = false)
        {
            if (string.IsNullOrEmpty(key))
                return;

            DateTime time = DateTime.Now.AddMinutes(expiration);
            
                m_Timer[key] = time;
            
        }

        public bool Remove(string key)
        {
            if (key == null)
                return false;
            DateTime time;
            return m_Timer.TryRemove(key, out time);

        }

        public DateTime Get(string key)
        {
            DateTime res = DateTime.MinValue;
            if (key == null)
                return res;
           
                m_Timer.TryGetValue(key, out res);
            
            return res;
        }

        public bool TryGetValue(string key, out DateTime res)
        {
            if (key == null)
            {
                res = DateTime.MinValue;
                return false;
            }

              return  m_Timer.TryGetValue(key, out res);
           
        }

        public void Update(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

                m_Timer[key] = DateTime.Now;
            
        }

        public Dictionary<string, DateTime> Copy()
        {
            Dictionary<string, DateTime> copy = null;

            copy = new Dictionary<string, DateTime>(m_Timer);

            return copy;
        }

        public Dictionary<string, DateTime> CopyAndClear()
        {
            Dictionary<string, DateTime> copy = null;
                copy = new Dictionary<string, DateTime>(m_Timer);
                m_Timer.Clear();
            return copy;
        }

        public void Clear()
        {
                m_Timer.Clear();
        }

        public string[] GetTimedoutItems()
        {
            List<string> list = new List<string>();

            TimeSpan ts = TimeSpan.FromMinutes(1);
            KeyValuePair<string, DateTime>[] items = m_Timer.Where(dic => ts < DateTime.Now.Subtract(dic.Value)).ToArray();


                foreach (var item in items)
                {
                        list.Add(item.Key);
                }
            return list.ToArray();
        }


        #endregion

        #region Timer Sync

        internal void SetCacheSyncState(CacheSyncState state)
        {
            this.SyncState = state;
        }

        private void SettingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Initialized && (this.SyncState == CacheSyncState.Idle))
            {
                this.LogAction(CacheAction.General, CacheActionState.None, "Synchronize Start");
                this.LastSyncTime = DateTime.Now;
                this.OnSyncStarted(EventArgs.Empty);
                DateTime time = this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
                this.LogAction(CacheAction.General, CacheActionState.None, "Synchronize End, Next Sync:{0}", new string[] { time.ToString() });
           }
        }

        public void Start()
        {

            if (!this.Initialized)
            {
                this.SyncState = CacheSyncState.Idle;
                this.Initialized = true;
                this.InitializeTimer();
            }
        }

        public void Stop()
        {
            if (this.Initialized)
            {
                this.Initialized = false;
                this.SyncState = CacheSyncState.Idle;
                this.DisposeTimer();
            }
        }

        private void DisposeTimer()
        {
            this.SettingTimer.Stop();
            this.SettingTimer.Enabled = false;
            this.SettingTimer.Elapsed -= new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer = null;
            this.LogAction(CacheAction.General, CacheActionState.None, "Dispose Timer");
        }

        private void InitializeTimer()
        {
            this.SettingTimer = new ThreadTimer((long)(this.IntervalSeconds * 1000));
            this.SettingTimer.AutoReset = true;
            this.SettingTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer.Enabled = true;
            this.SettingTimer.Start();
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized Timer Interval:{0}", new string[] { this.SettingTimer.Interval.ToString() });
        }

        public void DoSync()
        {
            OnSyncTimer();
        }

        protected virtual void OnSyncTimer()
        {
            try
            {
                this.LogAction(CacheAction.General, CacheActionState.None, "OnSyncTimer Start");

                string[] list = GetTimedoutItems();
                if (list != null && list.Length > 0)
                {
                    OnSyncCompleted(new SyncTimeCompletedEventArgs(list));
                    this.LogAction(CacheAction.SyncTime, CacheActionState.None, "OnSync End, items removed:{0}", new string[] { list.Length.ToString() });
                }
            }
            catch (Exception ex)
            {
                this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "OnSync End error :" + ex.Message);

            }
        }

        #endregion

        #region LogAction
        protected virtual void LogAction(CacheAction action, CacheActionState state, string text)
        {
            if (IsRemote)
            {
                CacheLogger.Logger.LogAction(action, state, text);
            }
        }

        protected virtual void LogAction(CacheAction action, CacheActionState state, string text, params string[] args)
        {
            if (IsRemote)
            {
                CacheLogger.Logger.LogAction(action, state, text, args);
            }
        }
        #endregion

    }
}
