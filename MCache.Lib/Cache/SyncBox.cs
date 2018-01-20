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
using Nistec.Caching.Sync;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Caching.Data;
using Nistec.Caching.Config;
using System.Transactions;
using System.Data;

namespace Nistec.Caching
{

    internal class SyncBox : IDisposable
    {
        #region memebers

        //int synchronized;

        public static readonly SyncBox Instance = new SyncBox(true,true);
        //private ConcurrentQueue<SyncBoxTask> m_SynBox;

        private ConcurrentDictionary<string, SyncBoxTaskMode> m_SynBox;
        private BlockingCollection<SyncBoxTask> m_SynQueue;
        //object mlock = new object();

        //private bool KeepAlive = false;

        #endregion

        #region properties

        /// <summary>
        /// Get indicate whether the sync box is remote.
        /// </summary>
        public bool IsRemote
        {
            get;
            internal set;
        }

        ///// <summary>
        ///// Get indicate whether the sync box intialized.
        ///// </summary>
        //public bool Initialized
        //{
        //    get;
        //    private set;
        //}

        /// <summary>
        /// Gets the number of elements contained in the SyncBox. 
        /// </summary>
        public int Count
        {
            get { return m_SynBox.Count; }
        }
      
        #endregion

        #region ctor

        public SyncBox(bool autoStart, bool isRemote)//, int intervalSeconds=60)
        {

            m_SynBox = new ConcurrentDictionary<string, SyncBoxTaskMode>();// new ConcurrentQueue<SyncBoxTask>();
            m_SynQueue = new BlockingCollection<SyncBoxTask>();
            
            IsRemote = isRemote;
            //this.IntervalSeconds = CacheSettings.SyncBoxInterval;
            //this.Initialized = true;
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized SynBox");

            if (autoStart)
            {
                Start();
            }
        }

        public void Dispose()
        {
           
        }
        #endregion

        #region queue methods


        public void Add(SyncBoxTask item)
        {
            if (item == null)
            {

                this.LogAction(CacheAction.SyncTime, CacheActionState.Failed, "SyncBox can not add task null!");
                return;
            }

            using (TransactionScope scop = new TransactionScope())
            {
                if (m_SynBox.TryAdd(item.ItemName, item.TaskMode))
                {
                    m_SynQueue.Add(item);
                    scop.Complete();
                };
            }

            //m_SynBox.AddOrUpdate(item.ItemName, item, (oldkey, oldValue) =>
            //{
            //    return item;
            //});

            //~Console.WriteLine("Debuger-SyncBox.Add: " + item.ItemName+ ", Count: " + m_SynBox.Count.ToString());

            //m_SynBox[item.ItemName] = item;
            //m_SynBox.Enqueue(item);
            this.LogAction(CacheAction.SyncTime, CacheActionState.Debug, "SyncBox Added SyncBoxTask {0}", item.ItemName);
        }

        //private SyncBoxTask PeekFirst()
        //{
        //    //SyncBoxTask res = null;
        //    // m_SynBox.TryDequeue(out res);


        //     SyncBoxTask res = m_SynBox.Values.OrderBy(p => p.Created).FirstOrDefault();
                       

        //    return res;
        //}
        //private bool TryDequeue(out SyncBoxTask item)
        //{
        //    lock (mlock)
        //    {
        //        SyncBoxTask task = m_SynBox.Values.OrderBy(p => p.Created).FirstOrDefault();
        //        if (task == null)
        //        {
        //            item = null;
        //            return false;
        //        }

        //        return m_SynBox.TryRemove(task.ItemName, out item);
        //    }
        //}

        public void Clear()
        {
            m_SynBox.Clear();
            
            while (m_SynQueue.Count > 0)
            {
                SyncBoxTask item;
                m_SynQueue.TryTake(out item);
                Thread.Sleep(100);
            }
          
        }

        #endregion

        #region Timer Sync

        int synchronized;
        Thread tt;
        private volatile bool KeepAlive;

        public bool Initialized
        {
            get { return KeepAlive; }
            //private set;
        }

        public void Start()
        {

            if (!KeepAlive)
            {
                KeepAlive = true;
                tt = new Thread(new ThreadStart(DequeueWorker));
                tt.IsBackground = true;
                tt.Start();
            }
        }

        public void Stop(int timeout = 0)
        {
            if (KeepAlive)
            {
                //this.Initialized = false;

                try
                {
                    KeepAlive = false;
                    // stop the thread
                    tt.Interrupt();

                    // Optionally block the caller
                    // by wait until the thread exits.
                    // If they leave the default timeout, 
                    // then they will not wait at all
                    tt.Join(timeout);
                }
                catch (ThreadInterruptedException tiex)
                {
                    /* Clean up. */
                    this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncBox.Stop ThreadInterruptedException error "+ tiex.Message);
                }
                catch (Exception ex)
                {
                    this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncBox.Stop error " + ex.Message);
                }
            }
        }


        private void DequeueWorker()
        {

            while (KeepAlive)
            {
                SyncBoxTask item = null;
                try
                {
                    if (0 == Interlocked.Exchange(ref synchronized, 1))
                    {
                        //while (!m_SynQueue.IsCompleted)
                        //{
                            if (m_SynQueue.TryTake(out item))
                            {
                                if (item != null)
                                {
                                    SyncBoxTaskMode taskMode;
                                    if (!m_SynBox.TryRemove(item.ItemName, out taskMode))
                                    {
                                        this.LogAction(CacheAction.SyncTime, CacheActionState.Failed, "SyncBox.DequeueWorker SynBox.TryRemove failed " + item.ItemName + ", SyncBoxTaskMode: " + taskMode.ToString());
                                    }
                                    item.DoAsyncTask();
                                }
                                Thread.Sleep(500);
                            }
                            else
                            Thread.Sleep(6000);
                        //}
                        //Thread.Sleep(1000);
                    }
                }
                catch (ThreadInterruptedException tiex)
                {
                    this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncBox.DequeueWorker error " + tiex.Message);
                }
                catch (Exception ex)
                {
                    this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncBox.DequeueWorker error " + ex.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref synchronized, 0);
                }
                //Thread.Sleep(100);
            }

        }
        #endregion

        #region Timer Sync
        /*
        private ThreadTimer SettingTimer;

        public int IntervalSeconds
        {
            get;
            internal set;
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
        internal void SetCacheSyncState(CacheSyncState state)
        {
            this.SyncState = state;
        }

        private void SettingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Initialized && (this.SyncState == CacheSyncState.Idle))
            {
                this.LastSyncTime = DateTime.Now;
                this.OnSyncStarted(EventArgs.Empty);
                DateTime time = this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
                //this.NextSyncTime = time;
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
            this.LogAction(CacheAction.General, CacheActionState.None, "Dispose SyncBox Timer");
        }

        private void InitializeTimer()
        {
            this.SettingTimer = new ThreadTimer((long)(this.IntervalSeconds * 1000));
            this.SettingTimer.AutoReset = true;
            this.SettingTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer.Enabled = true;
            this.SettingTimer.Start();
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized SyncBox Interval:{0}", new string[] { this.SettingTimer.Interval.ToString() });
        }
        */

        /*
         public void Start()
         {

             if (!this.Initialized)
             {
                 throw new Exception("The SyncBox not initialized!");
             }

             if (KeepAlive)
                 return;
             this.LogAction(CacheAction.General, CacheActionState.None, "SyncBox Started...");

             KeepAlive = true;
             Thread.Sleep(1000);
             Thread th = new Thread(new ThreadStart(InternalStart));
             th.IsBackground = true;
             th.Start();
         }

         public void Stop()
         {
             KeepAlive = false;
             this.Initialized = false;
             this.LogAction(CacheAction.General, CacheActionState.None, "SyncBox Stoped");
         }


         private void InternalStart()
         {
             while (KeepAlive)
             {
                 DoSync();
                 Thread.Sleep(1000);
             }
             this.LogAction(CacheAction.General, CacheActionState.None, "Initialized SyncBox Not keep alive");
         }
         */

        #endregion


        #region events
        /*
    public event EventHandler SyncStarted;

    public event SyncEntityTimeCompletedEventHandler SyncAccepted;

    protected virtual void OnSyncStarted(EventArgs e)
    {
        if (this.SyncStarted != null)
        {
            this.SyncStarted(this, e);
        }

        this.OnSyncDequeue();
    }

    protected virtual void OnSyncAccepted(SyncEntityTimeCompletedEventArgs e)
    {
        //~Console.WriteLine("Debuger-SyncBox.OnSyncAccepted: " + e.Item.ItemName);

        if (this.SyncAccepted != null)
        {
            this.SyncAccepted(this, e);
        }
        else
        {
            e.Item.DoAsync();
        }
    }

    protected virtual void OnSyncDequeue()
    {
        //~Console.WriteLine("Debuger-SyncBox.OnSyncDequeue: " + m_SynBox.Count.ToString());

        try
        {

            SyncBoxTask syncTask = null;
            while (TryDequeue(out syncTask))
            {
                OnSyncAccepted(new SyncEntityTimeCompletedEventArgs(syncTask));
                Thread.Sleep(1000);
            }


            //SyncBoxTask syncTask = null;
            //while (m_SynBox.TryDequeue(out syncTask))
            //{
            //    OnSyncAccepted(new SyncEntityTimeCompletedEventArgs(syncTask));
            //    Thread.Sleep(1000);
            //}
        }
        catch (Exception ex)
        {
            this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncBox OnSyncDequeue error :" + ex.Message);

        }
    }

    */


        #endregion

        #region Sync
        /*
        //not use
        public void DoSyncAll()
        {
            int count= this.Count;
            if (count > 0)
            {
                this.LogAction(CacheAction.General, CacheActionState.Debug, "SyncBox DoSyncAll items: " + count.ToString());
                int i = 0;
                while (i < count)
                {
                    OnSyncTask();
                    i++;
                    Thread.Sleep(100);
                }
            }
        }

        public void DoSync()
        {
            this.LogAction(CacheAction.General, CacheActionState.Debug, "SyncBox DoSync...");
            OnSyncTask();
        }

        protected virtual void OnSyncTask()
        {
            try
            {
                //0 indicates that the method is not in use.
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    SyncBoxTask syncTask = null;
                    if (TryDequeue(out syncTask))
                    {
                        syncTask.DoAsync();
                    }

                    //if (m_SynBox.TryDequeue(out syncTask))
                    //{
                    //    syncTask.DoSync();
                    //}
                }
            }
            catch (Exception ex)
            {
                this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncBox OnSyncTask End error :" + ex.Message);

            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }
       */
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


        internal static DataTable ReportQueueSchema()
        {
            DataTable dt = new DataTable("QueueSyncBox");
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("TaskMode", typeof(string));
            dt.Columns.Add("Created", typeof(DateTime));
            dt.Columns.Add("LastSync", typeof(string));
            dt.Columns.Add("SourceName", typeof(string));
            dt.Columns.Add("ClientId", typeof(string));

            return dt.Clone();
        }

        internal CacheItemReport GetQueueReport()
        {
            var data = SyncTimerQueueReport();
            if (data == null)
                return null;
            return new CacheItemReport() { Count = data.Rows.Count, Data = data, Name = "Sync Box Queue Report", Size = 0 };
        }


        public DataTable SyncTimerQueueReport()
        {
            DataTable table;
            try
            {
                ICollection<SyncBoxTask> items = this.m_SynQueue.ToArray();
                //if ((items == null) || (items.Count == 0))
                //{
                //    return null;
                //}
                table = ReportQueueSchema();
                foreach (var t in items)
                {
                    table.Rows.Add(t.ToDataRow());
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return table;
        }

        internal static DataTable ReportBoxSchema()
        {
            DataTable dt = new DataTable("SyncBox");
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("TaskMode", typeof(string));
            //dt.Columns.Add("Created", typeof(DateTime));
            //dt.Columns.Add("LastSync", typeof(string));
            //dt.Columns.Add("SourceName", typeof(string));
            //dt.Columns.Add("ClientId", typeof(string));

            return dt.Clone();
        }

        internal CacheItemReport GetBoxReport()
        {
            var data = SyncBoxReport();
            if (data == null)
                return null;
            return new CacheItemReport() { Count = data.Rows.Count, Data = data, Name = "Sync Box Report", Size = 0 };
        }


        public DataTable SyncBoxReport()
        {
            DataTable table;
            try
            {
                table = ReportBoxSchema();
                foreach (var t in m_SynBox)
                {
                    table.Rows.Add(t.Key,t.Value.ToString());
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
