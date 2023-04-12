using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using Nistec.Caching;
using Nistec.Threading;

namespace Nistec.Caching.Data
{
    internal class CacheSynchronize:IDisposable 
    {
        const int DefaultIntervalSetting = 60000;
        internal DalCache Owner;
        int usingResource;
        bool hasTableToMerge;
        int synchronized;
        private ActiveWatcher watcher;
        private ThreadTimer _timer;
        //private Mutex mut = new Mutex();

        //MControl.Loggers.Logger logger;

        /// <summary>
        /// CacheSynchronize Ctor
        /// </summary>
        /// <param name="owner"></param>
        public CacheSynchronize(DalCache owner)
        {
            //logger = MControl.Loggers.Logger.Instance;
            this.Owner = owner;
            this.watcher = new ActiveWatcher(this.Owner);
            _timer = new ThreadTimer(DefaultIntervalSetting);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            //logger.RegisterLoggers(new MControl.Loggers.FileLogger(@"D:\\MControl\Logs\Cache_log.txt",true));
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                if (_timer != null)
                {
                    _timer.Elapsed -= new System.Timers.ElapsedEventHandler(_timer_Elapsed);
                    _timer.Dispose();
                }
            }
            if (watcher != null)
            {
                watcher.Dispose();
            }
        }

        /// <summary>
        /// Start storage ThreadSetting
        /// </summary>
        public void Start(int IntervalSetting)
        {
            
             RegisteredTablesEvent();
            _timer.Interval = IntervalSetting;
            _timer.Start();
        }

        /// <summary>
        /// Stop storage ThreadSetting
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }


        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DoSynchronize();
        }

        /// <summary>
        /// DoSynchronize
        /// </summary>
        public void DoSynchronize()
        {
            //logger.WriteLoge("started", MControl.Loggers.Mode.DEBUG);
            try
            {
                //0 indicates that the method is not in use.
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    //logger.WriteLoge("Refresh", MControl.Loggers.Mode.DEBUG);
                    //MControl.Caching.CacheLogger.LogWrite("Refresh synchronization");
                    watcher.Refresh();
                    //logger.WriteLoge("CheckRegisteredTablesInternal", MControl.Loggers.Mode.DEBUG);
                    //MControl.Caching.CacheLogger.LogWrite("Check Registered Worker");
                    CheckRegisteredWorker();// CheckRegisteredTablesInternal();
                    if (hasTableToMerge)
                    {
                        //MControl.Caching.CacheLogger.LogWrite("Table To Merge founded");
                        //logger.WriteLoge("SyncRegisteredTablesInternal", MControl.Loggers.Mode.DEBUG);
                        SyncRegisteredWorker();// SyncRegisteredTablesInternal();
                    }
                }
            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);

            }
        }

        /// <summary>
        /// Registered All Tables that has sync option by event.
        /// </summary>
        public void RegisteredTablesEvent()
        {
            SyncSourceCollection syncTables = Owner.SyncTables;
            if (syncTables == null || syncTables.Count == 0)
                return;
            foreach (SyncSource o in syncTables)
            {
                if (o.SyncType == SyncType.Event)
                {
                    o.CreateTableTrigger();
                    o.Register();
                    //watcher.Register(o.MappingName);
                }
            }
        }

        /// <summary>
        /// Sync All Tables in SyncTables list.
        /// </summary>
        private void CheckRegisteredTablesInternal()
        {
            Thread th = new Thread(new ThreadStart(CheckRegisteredWorker));
            //th.IsBackground = true;
            th.Start();
        }

        /// <summary>
        /// Sync All Tables in SyncTables list.
        /// </summary>
        private void SyncRegisteredTablesInternal()
        {
            Thread th = new Thread(new ThreadStart(SyncRegisteredWorker));
            //th.IsBackground = true;
            th.Start();
        }


        private void CheckRegisteredWorker()
        {
            //mut.WaitOne();
            //logger.WriteLoge("start CheckRegisteredWorker", MControl.Loggers.Mode.DEBUG);

            try
            {
                //0 indicates that the method is not in use.
                if (0 == Interlocked.Exchange(ref usingResource, 1))
                {
                    hasTableToMerge = false;
                    SyncSourceCollection syncTables = Owner.SyncTables;
                    if (syncTables == null || syncTables.Count == 0)
                        return;
                    lock (((IList)syncTables).SyncRoot)
                    {
                        foreach (SyncSource o in syncTables)
                        {
                            if (o.SyncType == SyncType.Event)
                            {
                                //MControl.Caching.CacheLogger.LogWrite("Check Edited: " + o.SourceName);
                                //logger.WriteLoge("GetEdited", MControl.Loggers.Mode.DEBUG);
                                if (watcher.GetEdited(o.SourceName))
                                {
                                    MControl.Caching.CacheLogger.LogWrite("Is Edited : " + o.SourceName);
                                    //logger.WriteLoge("m_Edited = true", MControl.Loggers.Mode.DEBUG);
                                    o.SetEdited (true);
                                    hasTableToMerge = true;
                                }
                            }
                            //else if (o.syncTime.SyncType == SyncType.None)
                            //    SyncTableSource(o);
                            else if (o.SyncTime.HasTimeToRun())
                            {
                                o.SetEdited(true);
                                hasTableToMerge = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                //mut.ReleaseMutex();
                //Release the lock
                Interlocked.Exchange(ref usingResource, 0);
            }
        }

        /// <summary>
        /// Sync All Tables in SyncTables list.
        /// </summary>
        private void SyncRegisteredWorker()
        {
            //mut.WaitOne();
            //logger.WriteLoge("start SyncRegisteredWorker", MControl.Loggers.Mode.DEBUG);

            try
            {
                //0 indicates that the method is not in use.
                if (0 == Interlocked.Exchange(ref usingResource, 1))
                {
                    SyncSourceCollection syncTables = Owner.SyncTables;
                    if (syncTables == null || syncTables.Count == 0)
                        return;
                    lock (((IList)syncTables).SyncRoot)
                    {
                        foreach (SyncSource o in syncTables)
                        {
                            //MControl.Caching.CacheLogger.LogWrite("Sync worker check "+o.SourceName);
                            //logger.WriteLoge("SyncSource " + o.MappingName, MControl.Loggers.Mode.DEBUG);
                            if (o.Edited)
                            {
                                //logger.WriteLoge("MergeTableSource " + o.MappingName, MControl.Loggers.Mode.DEBUG);
                                //object res = Owner.GetValue("Customers", "CompanyName", "CustomerID='aaaaa'");
                                //if (res != null)
                                //{
                                //    logger.WriteLoge("changes is " + res.ToString(), MControl.Loggers.Mode.DEBUG);
                                //}
                                o.StoreTableSource();
                            }
                        }
                    }
                }
            }
            finally
            {
                //mut.ReleaseMutex();
                //Release the lock
                Interlocked.Exchange(ref usingResource, 0);
            }
        }

    }
}
