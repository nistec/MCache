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
using Nistec.Data;
using System.Data;
using Nistec.Threading;
using System.Transactions;

namespace Nistec.Caching.Sync
{


    /// <summary>
    /// Represent Cache Synchronizer
    /// </summary>
    internal class DataSynchronizer : IDisposable
    {
        internal static readonly DataSynchronizer Global = new DataSynchronizer();

        #region members

        internal readonly Dictionary<string, IDataCache> DataEventsOwners;
        internal readonly DataSyncList DataSyncItems;
        //int intervalSeconds = CacheDefaults.DefaultIntervalSeconds;
        bool enableTrigger;
        bool enableSyncEvent;

        int synchronized;

        bool EnableEveventSynchronizer{ get { return enableTrigger || enableSyncEvent; } }
        #endregion

        #region ctor
        public DataSynchronizer()
        {
            DataSyncItems = new DataSyncList();
            DataEventsOwners = new Dictionary<string, IDataCache>();
            enableTrigger = CacheSettings.EnableSyncTypeEventTrigger;
            enableSyncEvent = CacheSettings.EnableSyncTypeEvent;
            this.IntervalSeconds = CacheDefaults.GetValidIntervalSeconds(CacheSettings.SyncInterval);// CacheDefaults.GetValidIntervalSeconds(CacheDefaults.DefaultIntervalSeconds);

            if(enableSyncEvent || enableTrigger)
            {
                Start();
            }
        }
     
     
        #endregion

        #region IDisposable

        ~DataSynchronizer()
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
                //if (watcher != null)
                //{
                //    watcher.Dispose();
                //}
            }
        }
        #endregion

        #region properties

        public int IntervalSeconds
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

        #region Timer Sync

        private ThreadTimer SettingTimer;

        internal void SetCacheSyncState(CacheSyncState state)
        {
            this.SyncState = state;
        }

        private void SettingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Initialized && (this.SyncState == CacheSyncState.Idle))
            {
                this.LastSyncTime = DateTime.Now;
                DoSynchronizeEvents();
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
            this.LogAction(CacheAction.General, CacheActionState.None, "Dispose Timer");
        }

        private void InitializeTimer()
        {
            this.SettingTimer = new ThreadTimer((long)(this.IntervalSeconds * 1000));
            this.SettingTimer.AutoReset = true;
            this.SettingTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer.Enabled = true;
            this.SettingTimer.Start();
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized SyncTimer Interval:{0}", new string[] { this.SettingTimer.Interval.ToString() });
        }
    

        #endregion

        #region LogAction
        protected virtual void LogAction(CacheAction action, CacheActionState state, string text)
        {
            //if (IsRemote)
                CacheLogger.Logger.LogAction(action, state, text);
        }

        protected virtual void LogAction(CacheAction action, CacheActionState state, string text, params string[] args)
        {
            //if (IsRemote)
                CacheLogger.Logger.LogAction(action, state, text, args);
        }
        #endregion


        /// <summary>
        /// DoSynchronize
        /// </summary>
        public void DoSynchronizeEvents()
        {
            try
            {

                if (enableSyncEvent == false)
                    return;

                //0 indicates that the method is not in use.
                while (0 != Interlocked.Exchange(ref synchronized, 1))
                {
                    Thread.Sleep(100);
                }

                int count = 0;
                foreach(var entry in DataEventsOwners)
                {
                    IDataCache owner = entry.Value;
                    var dt= DbWatcher.GetEdited(owner);

                    if (dt != null)
                    {
                        owner.WaitForReadySyncState(1000);

                        foreach (DataRow row in dt.Rows)
                        {
                            string tableName = row.Get<string>("TableName");
                            var syncItems = DataSyncItems.GetItemBySorce(tableName);
                            if (syncItems != null)
                            {
                                foreach (var item in syncItems)
                                {
                                    SyncBox.Instance.Add(new SyncBoxTask(item, owner));
                                    count++;
                                    DbWatcher.UpdateEdited(owner, tableName);
                                    CacheLogger.DebugFormat("DoSynchronizeEvents Added SyncBoxTask to SyncBox: {0} ", item.EntityName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("DoSynchronizeEvents Error : " + ex.Message);
            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }

        /// <summary>
        /// Register Tables to sync.
        /// </summary>
        public void RegisterSyncTable(DataSyncEntity entity, IDataCache owner, bool ensureEventTableWatcher = true)
        {
            
            try
            {
                if (entity != null)
                {
                    if (entity.SyncType == SyncType.Event)
                    {
                        if (enableSyncEvent)
                        {
                            using (TransactionScope trans = new TransactionScope())
                            {
                                IDataCache dc;
                                bool isEqual = false;
                                if (DataEventsOwners.TryGetValue(owner.ConnectionKey, out dc))
                                {
                                    isEqual = owner.IsEqual(dc);
                                }

                                if (!isEqual)//DataEventsOwners.ContainsKey(owner.ConnectionKey))
                                {
                                    entity.Register(owner, ensureEventTableWatcher);
                                    if (enableTrigger)
                                        entity.CreateTableTrigger(owner);
                                    DataEventsOwners[owner.ConnectionKey] = owner;
                                }
                                DataSyncItems.Set(entity);
                                trans.Complete();
                                CacheLogger.DebugFormat("RegisterSyncTable Added SyncType.Event to SyncBox: {0} ", entity.EntityName);
                            }
                        }
                    }
                    else if (entity.SyncType == SyncType.Daily || entity.SyncType == SyncType.Interval)
                    {
                        TimerSyncDispatcher.Instance.Add(new SyncTask() { Timer = entity.SyncTime, ItemName = entity.EntityName, Entity = entity, Owner = owner });
                        CacheLogger.DebugFormat("RegisterSyncTable Added {1} to SyncBox: {0} ", entity.EntityName, entity.SyncType.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("RegisterSyncTable Error : " + ex.Message);
            }
        }

        public void RemoveSyncTable(DataSyncEntity entity, IDataCache owner, bool removeEventOwner = false)
        {

            try
            {
                if (entity != null)
                {
                    if (entity.SyncType == SyncType.Event)
                    {
                        if (DataSyncItems.Contains(entity))
                        {
                            DataSyncItems.Remove(entity);
                            CacheLogger.DebugFormat("RemoveSyncTable Removed DataSyncEntity SyncType.Event from DataSyncItems: {0} ", entity.EntityName);
                        }
                        if (removeEventOwner)
                        {
                            if (DataEventsOwners.ContainsKey(owner.ConnectionKey))
                            {
                                DataEventsOwners.Remove(owner.ConnectionKey);
                                CacheLogger.DebugFormat("RemoveSyncTable Removed IDataCache SyncType.Event from DataEventsOwners: {0} ", owner.ConnectionKey);
                            }
                        }
                    }
                    else if (entity.SyncType == SyncType.Daily || entity.SyncType == SyncType.Interval)
                    {
                        TimerSyncDispatcher.Instance.Remove(new SyncTask() { Timer = entity.SyncTime, ItemName = entity.EntityName, Entity = entity, Owner = owner });
                        CacheLogger.DebugFormat("RegisterSyncTable Removed {1} from SyncBox: {0} ", entity.EntityName, entity.SyncType.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("RemoveSyncTable Error : " + ex.Message);
            }
        }

        public void RemoveTableEvent(IDataCache owner, bool removeOwner, bool removeFromWatcher, bool removeTrigger, bool removeDataSyncItems)
        {
            try
            {
                if (owner != null)
                {
                    if (removeOwner)
                    {
                        if (DataEventsOwners.ContainsKey(owner.ConnectionKey))
                        {
                            DataEventsOwners.Remove(owner.ConnectionKey);
                            CacheLogger.DebugFormat("RemoveTableEvent Removed IDataCache SyncType.Event from DataEventsOwners: {0} ", owner.ConnectionKey);
                        }
                    }


                    var items = DataSyncItems.GetItemsByConnection(owner.ConnectionKey);
                    {
                        foreach (var item in items)
                        {
                            if (removeFromWatcher)
                                item.RegisterRemove(owner);
                            if (removeTrigger)
                                item.RemoveTableTrigger(owner);
                            if (removeDataSyncItems)
                            {
                                DataSyncItems.Remove(item);
                                CacheLogger.DebugFormat("RemoveTableEvent Removed DataSyncEntity SyncType.Event from DataSyncItems: {0} ", item.EntityName);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("RemoveTableEvent Error : " + ex.Message);
            }
        }

        /*
 
        /// <summary>
        /// Registered All Tables that has sync option by event.
        /// </summary>
        public void RemoveTablesEvent(IDataCache Owner)
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


        /// <summary>
        /// Registered All Tables that has sync option by event.
        /// </summary>
        public void RegisteredTablesEvent(IDataCache Owner)
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
                        TimerSyncDispatcher.Instance.Add(new SyncTask() { Item = null, Timer = o.SyncTime, ItemName = o.EntityName, Entity = o, Owner = Owner });
                    }
                }
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

        */
    }
}
