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
        internal IDataCache Owner;
        int synchronized;
        private DbWatcher watcher;
        int intervalSeconds = CacheDefaults.DefaultIntervalSeconds;
 
        SyncTask _TimerTask;
        internal SyncTask TimerTask
        {
            get
            {
                _TimerTask = _TimerTask ??
                new SyncTask() { Item = this, IntervalSeconds = this.intervalSeconds, ItemName=Owner.CacheName };
                return _TimerTask;
            }
        }

        /// <summary>
        /// CacheSynchronize Ctor
        /// </summary>
        /// <param name="owner"></param>
        public CacheSynchronizer(IDataCache owner)
        {
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
            this.intervalSeconds =  CacheDefaults.GetValidIntervalSeconds(intervalSeconds);
            RegisteredTablesEvent();
            TimerSyncDispatcher.Instance.Add(TimerTask);
        }

        /// <summary>
        /// Stop storage ThreadSetting
        /// </summary>
        public void Stop()
        {
            TimerSyncDispatcher.Instance.Remove(TimerTask);
        }

        
        /// <summary>
        /// DoSynchronize
        /// </summary>
        public void DoSynchronize()
        {
            try
            {
                CacheLogger.Debug("CacheSynchronizer DoSynchronize start...");

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

                DataSyncEntity[] items = CheckRegistryItems(syncTables.GetItems());
                if (items != null)
                {
                    foreach (DataSyncEntity o in items)
                    {
                        if (o.Edited)
                        {

                            SyncBox.Instance.Add(new SyncBoxTask(o, Owner));

                        }
                    }
                }
                else
                {
                    CacheLogger.Debug("CacheSynchronizer DoSynchronize DataSyncEntities not found!");
                }
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


        internal DataSyncEntity[] CheckRegistryItems(DataSyncEntity[] items)
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
                        list.Add(o);
                    }
                }
                else //if (o.SyncTime.HasTimeToRun())
                {

                    bool isTimeToRun = o.SyncTime.HasTimeToRun();
                    if (isTimeToRun)
                    {
                        o.SetEdited(true);
                        list.Add(o);

                        CacheLogger.DebugFormat("SyncTimer Added to  DataSyncEntity list: {0} ", o.ViewName);

                    }
                }
            }

            return list.ToArray();
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
                    }
                }
            }
        }

    }
}
