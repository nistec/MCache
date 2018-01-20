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
using System.Data;
using Nistec.Caching.Session;
using Nistec.Caching.Config;

namespace Nistec.Caching
{
    public class TimerSource
    {
        public const string Cache= "Cache";
        public const string Session= "Session";
        public const string Data= "Data";
    }
    public class TimerItem
    {
        internal string Source;
        internal DateTime Last;
        internal DateTime Ttl;
        internal int Increment;
        internal int State;//0=active, 1=idle, 2=timedout
        internal bool EnableNotification;
        internal int IdlIncrement=3;
        int StateTaken = 0;
        public int GetState()
        {
            TimeSpan ts = TimeSpan.FromMinutes(1);
            if(StateTaken == 0 && ts < DateTime.Now.Subtract(Ttl) && State==0)
            {
                State=1;
                StateTaken = 1;
                Ttl = Ttl.AddMinutes(IdlIncrement);
                Last = DateTime.Now;
                if (EnableNotification)
                {
                    SendEvent();
                }
                return State;
            }
            else if (StateTaken != 2 && ts < DateTime.Now.Subtract(Ttl) && State == 1)
            {
                State=2;
                StateTaken = 2;
                Last = DateTime.Now;
                if (EnableNotification)
                {
                    SendEvent();
                }
                return State;
            }
            return 0;
        }

        internal bool ShouldRemove()
        {
            return State == 2 && DateTime.Now.Subtract(Last).TotalMinutes > 30;
        }
        private void SendEvent()
        {
            //TODO
        }

        internal void Reset()
        {
            State = 0;
            StateTaken = 0;
            Last = DateTime.Now;
        }

        public void UpdateTimer()
        {
            Ttl = Ttl.AddMinutes(Increment);
            State = 0;
            StateTaken = 0;
            Last = DateTime.Now;
        }
        public void UpdateTimer(DateTime expirationTime, int expiration, bool enableIncremental)
        {
            if (enableIncremental && expiration <= 0)
                expiration = (int)expirationTime.Subtract(DateTime.Now).TotalMinutes;
            Ttl = expirationTime;
            Increment = enableIncremental ? expiration : 0;
            State = 0;
            StateTaken = 0;
            Last = DateTime.Now;
        }
        internal static TimerItem Create(string source,int expiration, bool enableIncremental)
        {
            return new TimerItem()
            {
                Source=source,
                Ttl = DateTime.Now.AddMinutes(expiration),
                Increment = enableIncremental ? expiration : 0,
                State = 0
            };
        }
        internal static TimerItem Create(string source, DateTime expirationTime, int expiration, bool enableIncremental)
        {
            if (enableIncremental && expiration <= 0)
                expiration = (int)expirationTime.Subtract(DateTime.Now).TotalMinutes;
 
            return new TimerItem()
            {
                Source = source,
                Ttl = expirationTime,
                Increment = enableIncremental ? expiration : 0,
                State = 0
            };
        }
        public static int GetValidExpiration(int expiration, int defaultExpiration)
        {
            if (expiration > 0)
                return expiration;
            if (defaultExpiration > 0)
                return defaultExpiration;
            return CacheSettings.DefaultExpiration;
        }
    }


    internal class TimerDispatcher : IDisposable
    {
        #region members
        private ThreadTimer SettingTimer;
        private ConcurrentDictionary<string, TimerItem> m_Items;


        public string Source
        {
            get;
            internal set;
        }

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
        #endregion

        #region ctor



        public void Dispose()
        {
            if (SettingTimer != null)
            {
                DisposeTimer();
            }
        }

        #endregion

        #region timer

        public TimerDispatcher(string source, int initialCapacity, bool isRemote)
        {
            Source = source;
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            if (initialCapacity < 100)
                initialCapacity = 101;

            m_Items = new ConcurrentDictionary<string, TimerItem>(concurrencyLevel, initialCapacity);


            this.IntervalSeconds = CacheDefaults.DefaultTimerIntervalSeconds;
            this.IsRemote = isRemote;
            this.Initialized = false;
            this.SyncState = CacheSyncState.Idle;
            this.LastSyncTime = DateTime.Now;
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized TimeoutDispatcher for "+ source);
        }


        #endregion

        #region events

        public event EventHandler SyncStarted;

        //public event SyncTimeCompletedEventHandler SyncCompleted;
        public event SyncTimerItemEventHandler StateChanged;

        protected virtual void OnSyncStarted(EventArgs e)
        {
            if (this.SyncStarted != null)
            {
                this.SyncStarted(this, e);
            }

            this.OnSyncTimer();
        }

        //protected virtual void OnSyncCompleted(SyncTimeCompletedEventArgs e)
        //{
        //    if (this.SyncCompleted != null)
        //    {
        //        this.SyncCompleted(this, e);
        //    }
        //}

        protected virtual void OnStateChanged(SyncTimerItemEventArgs e)
        {
            if (this.StateChanged != null)
            {
                this.StateChanged(this, e);
            }
        }


        #endregion

        #region cache timeout
        /*
        void UpdateIncremental(string key)
        {
            TimerItem item;
            if (m_Timer.TryGetValue(key, out item))
            {
                item.UpdateIncremental();
            }
        }
        void AddTimer(string key, int expiration, bool enableIncremental)
        {
            m_Timer[key] = TimerItem.Create(expiration, enableIncremental);
        }
        void AddTimer(string key, DateTime expirationTime, int expiration, bool enableIncremental)
        {
            if (enableIncremental && expiration <= 0)
                expiration = (int)expirationTime.Subtract(DateTime.Now).TotalMinutes;

            TimerItem item;
            if (m_Timer.TryGetValue(key, out item))
                item.UpdateTimer(expirationTime, expiration, enableIncremental);
            else
                m_Timer[key] = new TimerItem() { ttl = expirationTime, increment = enableIncremental ? expiration : 0 };
        }
        void AddTimer(string key, DateTime expirationTime, bool enableIncremental)
        {
            int expiration = 0;
            if (enableIncremental)
                expiration = (int)expirationTime.Subtract(DateTime.Now).TotalMinutes;

            m_Timer[key] = new TimerItem() { ttl = expirationTime, increment = expiration };
        }
        */
        /*
        public void Add(CacheEntry item)
        {
            if (item.AllowExpires)
            {
                AddTimer(item.Key, item.ExpirationTime,false);
                //m_Timer[item.Key] = item.ExpirationTime;
            }
        }

        public void Add(string key, DateTime time)
        {
            if (string.IsNullOrEmpty(key))
                return;

            //m_Timer[key] = time;
            AddTimer(key, time,false);
        }

        public void Add(string key, DateTime time, int expiration, bool enableIncremental)
        {
            if (string.IsNullOrEmpty(key))
                return;

            //m_Timer[key] = time;
            AddTimer(key, time, enableIncremental);
        }

        public void Add(string key, int expiration, bool enableIncremental)//, bool chekExists = false)
        {
            if (string.IsNullOrEmpty(key))
                return;
            if (expiration <= 0)
            {
                throw new ArgumentException("expiration is incorrect, should bet greater then zero");
            }
            DateTime time = DateTime.Now.AddMinutes(expiration);

            //m_Timer[key] = time;

            AddTimer(key, expiration, enableIncremental);
        }

        public void Add(string key, int expiration, int defaultExpiration, bool enableIncremental)
        {
            if (string.IsNullOrEmpty(key))
                return;
            if (expiration <= 0 && defaultExpiration <= 0)
            {
                throw new ArgumentException("expiration or defaultExpiration is incorrect, should bet greater then zero");
            }

            //DateTime time = DateTime.Now.AddMinutes(expiration == 0 ? defaultExpiration : expiration);
            //m_Timer[key] = time;

            AddTimer(key, expiration == 0 ? defaultExpiration : expiration, enableIncremental);
        }

        */

        //public void Add(string key, DateTime time)
        //{
        //    if (string.IsNullOrEmpty(key))
        //        return;
        //    m_Timer[key] = TimerItem.Create(time, expiration, true);
        //    m_Timer[key] = TimerItem.Create( time;
        //    CacheLogger.Logger.LogAction(CacheAction.SessionCache, CacheActionState.Debug, "TimerDispatcher Add : " + key + ", time: " + time.ToString());
        //}

        //public void Add(string key, int expiration, int defaultExpiration)
        //{
        //    if (string.IsNullOrEmpty(key))
        //        return;
        //    //if (expiration <= 0 && defaultExpiration <= 0)
        //    //{
        //    //    throw new ArgumentException("expiration or defaultExpiration is incorrect, should bet greater then zero");
        //    //}
        //    expiration=TimerItem.GetValidExpiration(expiration, defaultExpiration);

        //    TimerItem item;
        //    if (m_Timer.TryGetValue(key, out item))
        //    {
        //        item.Ttl = item.Ttl.AddMinutes(expiration == 0 ? defaultExpiration : expiration);
        //        item.Increment = expiration;
        //    }
        //    else
        //    {
        //        m_Timer[key] = TimerItem.Create(DateTime.Now.AddMinutes(expiration), expiration,true);
        //    }
        //    CacheLogger.Logger.LogAction(CacheAction.SessionCache, CacheActionState.Debug, "TimerDispatcher Add : " + key + ", expiration: " + time.ToString());

        //}

        public void UpdateTimer(string key)
        {
            TimerItem item;
            if (m_Items.TryGetValue(key, out item))
            {
                item.UpdateTimer();
            }
        }

        public void AddOrUpdate(string key, DateTime time, int expiration = 0)
        {
            if (string.IsNullOrEmpty(key))
                return;
            if (expiration == 0)
                expiration = (int)time.Subtract(DateTime.Now).TotalMinutes;
            if (expiration < 0)
            {
                expiration = CacheSettings.DefaultExpiration;
            }

            TimerItem item;
            if (m_Items.TryGetValue(key, out item))
            {
                item.Ttl = time;
                item.Increment = expiration;
                item.Reset();
            }
            else
            {
                m_Items[key] = TimerItem.Create(Source,DateTime.Now.AddMinutes(expiration), expiration, true);
            }
        }
        public void AddOrUpdate(string key, int expiration, int defaultExpiration=0)
        {
            if (string.IsNullOrEmpty(key))
                return;

            expiration = TimerItem.GetValidExpiration(expiration, defaultExpiration);

            TimerItem item;
            if (m_Items.TryGetValue(key, out item))
            {
                item.Ttl = item.Ttl.AddMinutes(expiration == 0 ? defaultExpiration : expiration);
                item.Increment = expiration;
            }
            else
            {
                m_Items[key] = TimerItem.Create(Source, DateTime.Now.AddMinutes(expiration), expiration, true);
            }
        }

        public bool Remove(string key)
        {
            if (key == null)
                return false;
            TimerItem time;
            if (m_Items.TryRemove(key, out time))
            {
                CacheLogger.Logger.LogAction(CacheAction.TimerDispatcher, CacheActionState.Debug, "TimerDispatcher Removed : " + key);
                return true;
            }
            return false;
        }

        public TimerItem Get(string key)
        {
            TimerItem res=null;
            if (key == null)
                return res;

            m_Items.TryGetValue(key, out res);

            return res;
        }

        public bool TryGetValue(string key, out TimerItem res)
        {
            if (key == null)
            {
                res = null;
                return false;
            }

            return m_Items.TryGetValue(key, out res);

        }



        public Dictionary<string, TimerItem> Copy()
        {
            Dictionary<string, TimerItem> copy = null;

            copy = new Dictionary<string, TimerItem>(m_Items);

            return copy;
        }

        public Dictionary<string, TimerItem> CopyAndClear()
        {
            Dictionary<string, TimerItem> copy = null;
            copy = new Dictionary<string, TimerItem>(m_Items);
            m_Items.Clear();
            return copy;
        }

        public void Clear()
        {
            m_Items.Clear();
        }

        public string[] GetTimedoutItems()
        {
            List<string> list = new List<string>();

            TimeSpan ts = TimeSpan.FromMinutes(1);
            KeyValuePair<string, TimerItem>[] items = m_Items.ToArray().Where(dic => dic.Value.GetState()==2).ToArray();

            foreach (var item in items)
            {
                list.Add(item.Key);
            }
            return list.ToArray();
        }

        public Dictionary<string, TimerItem> GetStateChangedItems()
        {

            //var dict = m_Timer.ToArray().Where(t => t.Value.GetState() != 0)
            //    .Select(t => new { t.Key, t.Value.State })
            //    .ToDictionary(t => t.Key, t => t.State);
            //return dict;

            var dict = m_Items.ToArray().Where(t => t.Value.GetState() != 0)
                .Select(t => new { t.Key, t.Value })
                .ToDictionary(t => t.Key, t => t.Value);
            return dict;

            //Dictionary<string,int> list = new Dictionary<string, int>();
            //TimeSpan ts = TimeSpan.FromMinutes(1);
            //KeyValuePair<string, TimerItem>[] items = m_Timer.ToArray().Where(dic => dic.Value.GetState() !=0).ToArray();

            //foreach (var item in items)
            //{
            //    list.Add(item.Key, item.Value.State);
            //}
            //return list;
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
                //this.LogAction(CacheAction.General, CacheActionState.None, "Synchronize Start");
                this.LastSyncTime = DateTime.Now;
                this.OnSyncStarted(EventArgs.Empty);
                DateTime time = this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
                //this.LogAction(CacheAction.General, CacheActionState.None, "Synchronize End, Next Sync:{0}", new string[] { time.ToString() });
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
            //~Console.WriteLine("Debuger-TimerDispatcher.OnSyncTimer...");

            try
            {
                //this.LogAction(CacheAction.General, CacheActionState.None, "OnSyncTimer Start");

                var items = GetStateChangedItems();
                if (items != null && items.Count > 0)
                {
                    OnStateChanged(new SyncTimerItemEventArgs(items));
                }

                //string[] list = GetTimedoutItems();
                //if (list != null && list.Length > 0)
                //{
                //    OnSyncCompleted(new SyncTimeCompletedEventArgs(list));
                //    //this.LogAction(CacheAction.SyncTime, CacheActionState.None, "OnSync End, items removed:{0}", new string[] { list.Length.ToString() });
                //}

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

        #region Report


        internal static DataTable ReportSchema()
        {
            DataTable dt = new DataTable("SyncBox");
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("Ttl", typeof(DateTime));
            dt.Columns.Add("LastSync", typeof(DateTime));
            dt.Columns.Add("Source", typeof(string));
            dt.Columns.Add("State", typeof(int));
            dt.Columns.Add("Expiration", typeof(int));

            return dt.Clone();
        }

        internal CacheItemReport GetReport(string owner)
        {
            var data = TimerReport();
            if (data == null)
                return null;
            return new CacheItemReport() { Count = data.Rows.Count, Data = data, Name = "Timer Dispatcher Report", Size = 0 };
        }


        public DataTable TimerReport()
        {
            DataTable table;
            try
            {

                table = ReportSchema();
                foreach (var t in m_Items)
                {
                    table.Rows.Add(t.Key, t.Value.Ttl, t.Value.Last, t.Value.Source, t.Value.State, t.Value.Increment);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return table;
        }
        #endregion
    }


}
