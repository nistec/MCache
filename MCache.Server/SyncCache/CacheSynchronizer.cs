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
using Nistec.Caching.Data;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Caching.Config;

namespace Nistec.Caching.Sync
{

   
    /// <summary>
    /// Represent Cache Synchronizer
    /// </summary>
    internal class CacheSynchronizer : ITaskSync,IDisposable
    {

        internal CacheSynchronizer Copy()
        {
            return new CacheSynchronizer()
            {
                enableTrigger = this.enableTrigger,
                enableSyncEvent=this.enableSyncEvent,
                intervalSeconds = this.intervalSeconds,
                Owner = this.Owner,//.Copy(),
                synchronized = this.synchronized,
                watcher = this.watcher,
                _TimerTask = this._TimerTask

            };
        }


        internal IDataCache Owner;
        int synchronized;
        private DbWatcher watcher;
        int intervalSeconds = CacheDefaults.DefaultIntervalSeconds;
        bool enableTrigger;
        bool enableSyncEvent;

        SyncTask _TimerTask;
        internal SyncTask TimerTask
        {
            get
            {
                _TimerTask = _TimerTask ??
                new SyncTask() { Item = this, ItemName = Owner.CacheName, Timer = new SyncTimer(TimeSpan.FromSeconds(this.intervalSeconds), SyncType.Interval) };
                return _TimerTask;

                //_TimerTask = _TimerTask ??
                //new SyncTask() { Item = this, IntervalSeconds = this.intervalSeconds, ItemName=Owner.CacheName };
                //return _TimerTask;
            }
        }

        internal SyncTask GetTimerTask(DataSyncEntity entity)
        {
            var syncTime = entity.SyncTime;
            if (syncTime == null)
                syncTime = new SyncTimer(TimeSpan.FromSeconds(this.intervalSeconds), SyncType.Interval);
            return new SyncTask() { Item = this, ItemName = Owner.CacheName, Timer = syncTime, Entity = entity, Owner = Owner };
        }

        private CacheSynchronizer()
        {

        }

        /// <summary>
        /// CacheSynchronize Ctor
        /// </summary>
        /// <param name="owner"></param>
        public CacheSynchronizer(IDataCache owner)
        {
            enableTrigger = owner.EnableTrigger;// CacheSettings.EnableSyncTypeEventTrigger;
            enableSyncEvent = owner.EnableSyncEvent;
            this.Owner = owner;
            this.watcher = new DbWatcher(this.Owner);
        }

      
        ~CacheSynchronizer()
        {
            Dispose(false);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void DisposeCopy()
        {
            if (watcher != null)
            {
                watcher.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        /// <summary>
        /// Get if IsDisposed
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected  void Dispose(bool disposing)
        {

            if (disposing)
            {
                Stop();
                if (watcher != null)
                {
                    watcher.Dispose();
                }
            }
        }

        /// <summary>
        /// Start storage Thread Setting using interval in seconds.
        /// </summary>
        /// <param name="intervalSeconds"></param>
        public void Start(int intervalSeconds)
        {
           //~Console.WriteLine("Debuger-CacheSynchronizer.Start...");
            this.intervalSeconds = CacheDefaults.GetValidIntervalSeconds(intervalSeconds);
            //Task.Factory.StartNew(() => RegisteredTablesEvent());
            if (enableTrigger)
                TimerSyncDispatcher.Instance.Add(TimerTask);
        }

        /// <summary>
        /// Stop storage ThreadSetting
        /// </summary>
        public void Stop()
        {
           //~Console.WriteLine("Debuger-CacheSynchronizer.Stop...");
            //if (enableTrigger)
                TimerSyncDispatcher.Instance.Remove(TimerTask);
            Task.Factory.StartNew(() => RemoveTablesEvent());
        }

        ///// <summary>
        ///// AddToSyncBox
        ///// </summary>
        //public void AddToSyncBox(ITaskSync TaskItem, string ItemName)
        //{
        //    SyncBox.Instance.Add(new SyncBoxTask(TaskItem, ItemName));
        //    CacheLogger.Debug(ItemName + " Added SyncBox");
        //}


        /// <summary>
        /// DoSynchronize
        /// </summary>
        public void DoSynchronize()
        {
           //~Console.WriteLine("Debuger-CacheSynchronizer.DoSynchronize...");

            try
            {
               // CacheLogger.Debug("CacheSynchronizer DoSynchronize start...");

                //0 indicates that the method is not in use.
                while (0 != Interlocked.Exchange(ref synchronized, 1))
                {
                    Thread.Sleep(100);
                }

                //0 indicates that the method is not in use.
                DataSyncList syncTables = Owner.SyncTables;
                if (syncTables == null || syncTables.Count == 0)
                {
                    CacheLogger.Debug("CacheSynchronizer DoSynchronize syncTables not found!");

                    return;
                }
                Owner.WaitForReadySyncState(2000);

                if (syncTables.GetItemsCount(SyncType.Event) > 0)
                {
                    watcher.Refresh();
                }

                DataSyncEntity[] items = CheckRegistryItems(syncTables.GetItems(),true);
                if (items != null && items.Length>0)
                {
                    CacheLogger.Debug("CacheSynchronizer DoSynchronize DataSyncEntities found items: " + items.Length.ToString());
                    int i = 0;
                    foreach (DataSyncEntity o in items)
                    {
                        if (o.Edited)
                        {
                            SyncBox.Instance.Add(new SyncBoxTask(o, Owner));
                            i++;
                        }
                    }
                    //CacheLogger.DebugFormat("CacheSynchronizer DoSynchronize DataSyncEntities Added to SyncBox: {0}!" ,i);
                }
                //else
                //{
                //    CacheLogger.Debug("CacheSynchronizer DoSynchronize DataSyncEntities not found!");
                //}
                //}
            }
            catch (Exception ex)
            {
                CacheLogger.Error("DoSynchronizeTask Error : " + ex.Message);
            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);

            }
        }


       

        internal DataSyncEntity[] CheckRegistryItems(DataSyncEntity[] items, bool eventOnly)
        {

            List<DataSyncEntity> list = new List<DataSyncEntity>();
            if (items == null)
            {
                return null;
            }

            foreach (DataSyncEntity o in items)
            {
                if (o.SyncType == SyncType.Event)
                {

                    if (watcher.GetEdited(o.SourceName))
                    {
                        CacheLogger.Debug("Is Edited : " + o.ViewName);
                        o.SetEdited(true);
                        list.Add(o.Copy());
                    }
                }
                else if (!eventOnly) //if (o.SyncTime.HasTimeToRun())
                {

                    bool isTimeToRun = o.SyncTime.HasTimeToRun();
                    if (isTimeToRun)
                    {
                        o.SetEdited(true);
                        list.Add(o.Copy());

                        CacheLogger.DebugFormat("SyncTimer Added to  DataSyncEntity list: {0} ", o.ViewName);

                    }
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Register Tables that has sync option by event.
        /// </summary>
        public void RegisteredTableEvent(DataSyncEntity entity)
        {

            if (entity != null)
            {
                if (entity.SyncType == SyncType.Event)
                {
                    if (enableTrigger)
                        entity.CreateTableTrigger(Owner);
                    if (enableSyncEvent)
                    {
                        entity.Register(Owner);
                        TimerSyncDispatcher.Instance.Add(TimerTask);
                    }
                }
                else if (entity.SyncType == SyncType.Daily || entity.SyncType == SyncType.Interval)
                {
                    //TimerSyncDispatcher.Instance.Add(new SyncTask() { Item = this, IntervalSeconds =(int) o.SyncTime.Interval.TotalSeconds, ItemName = o.EntityName, Entity=o, Owner=Owner });
                    TimerSyncDispatcher.Instance.Add(new SyncTask() { Item = this, Timer = entity.SyncTime, ItemName = entity.EntityName, Entity = entity, Owner = Owner });
                }

            }
        }

        /// <summary>
        /// Registered All Tables that has sync option by event.
        /// </summary>
        public void RegisteredTablesEvent()
        {
            DataSyncList syncTables = Owner.SyncTables;
            if (syncTables == null || syncTables.Count == 0)
                return;

            DataSyncEntity[] items = syncTables.GetItems();

            if (items != null)
            {
                foreach (DataSyncEntity o in items)
                {
                    if (o.SyncType == SyncType.Event)
                    {
                        o.CreateTableTrigger(Owner);
                        o.Register(Owner);

                        //if (enableTrigger)
                        //    o.CreateTableTrigger(Owner);
                        //if (enableSyncEvent)
                        //{
                        //    o.Register(Owner);
                        //    TimerSyncDispatcher.Instance.Add(TimerTask);
                        //}
                    }
                    else if (o.SyncType == SyncType.Daily || o.SyncType == SyncType.Interval)
                    {
                        //TimerSyncDispatcher.Instance.Add(new SyncTask() { Item = this, IntervalSeconds =(int) o.SyncTime.Interval.TotalSeconds, ItemName = o.EntityName, Entity=o, Owner=Owner });
                        TimerSyncDispatcher.Instance.Add(new SyncTask() { Item = this, Timer = o.SyncTime, ItemName = o.EntityName, Entity = o, Owner = Owner });
                    }
                }
            }
        }

        /// <summary>
        /// Registered All Tables that has sync option by event.
        /// </summary>
        public void RemoveTablesEvent()
        {
            DataSyncList syncTables = Owner.SyncTables;
            if (syncTables == null || syncTables.Count == 0)
                return;

            DataSyncEntity[] items = syncTables.GetItems();

            if (items != null)
            {
                foreach (DataSyncEntity o in items)
                {
                    if (o.SyncType == SyncType.Event)
                    {
                        //TODO:
                        //o.CreateTableTrigger(Owner);
                        //o.Register(Owner);
                    }
                    else if (o.SyncType == SyncType.Daily || o.SyncType == SyncType.Interval)
                    {
                        TimerSyncDispatcher.Instance.Remove(o.EntityName);
                    }
                }
            }
        }
    }
}
