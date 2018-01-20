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


namespace Nistec.Caching.Sync
{
    ///// <summary>
    ///// Represent synchronization task interface.
    ///// </summary>
    //public interface ITaskSync
    //{
    //    /// <summary>
    //    /// Do synchronization action.
    //    /// </summary>
    //    void DoSynchronize();
    //    /// <summary>
    //    /// Get indicate whether the item is disposed.
    //    /// </summary>
    //    bool IsDisposed { get; }
    //}

    /// <summary>
    /// Sync item type
    /// </summary>
    public enum SyncEntityType
    {
        /// <summary>
        /// Sync Cache using as a synchronize db cache.
        /// </summary>
        SyncCache,
        /// <summary>
        /// Data Cache using as Databse cache.
        /// </summary>
        DataCache
    }

    /// <summary>
    /// Sync item type
    /// </summary>
    public enum SyncBoxTaskMode
    {
        /// <summary>
        /// PreSync.
        /// </summary>
        PreSync,
        /// <summary>
        /// DataSync.
        /// </summary>
        DataSync
        ///// <summary>
        ///// DataPreSync.
        ///// </summary>
        //DataPreSync
    }

     /// <summary>
    /// Represent sync task for SyncBox synchronization.
    /// </summary>
    internal class SyncBoxTask
    {

        ///// <summary>
        ///// Initialize a new instance of sync box for PreSync.
        ///// </summary>
        ///// <param name="TaskItem"></param>
        ///// <param name="ItemName"></param>
        //public SyncBoxTask(ITaskSync TaskItem, string ItemName)
        //{
        //    this.TaskItem = TaskItem;
        //    this.ItemName = ItemName;
        //    TaskMode = SyncBoxTaskMode.PreSync;
        //    Created = DateTime.Now;
        //    //~Console.WriteLine("Debuger-SyncBoxTask.New: " + ItemName);
        //}

        /// <summary>
        /// Initialize a new instance of sync box for DataSync.
        /// </summary>
        /// <param name="Entity"></param>
        /// <param name="Owner"></param>
        public SyncBoxTask(DataSyncEntity Entity, IDataCache Owner)
        {
            this.Entity = Entity;
            this.Owner = Owner;
            this.ItemName = Entity.EntityName;
            TaskMode = SyncBoxTaskMode.DataSync;
            Created = DateTime.Now;
            //~Console.WriteLine("Debuger-SyncBoxTask.New: " + Entity.EntityName);
        }

        ///// <summary>
        ///// Initialize a new instance of sync box for DataSync.
        ///// </summary>
        ///// <param name="Entity"></param>
        ///// <param name="owner"></param>
        ///// <param name="syncOwner"></param>
        //public SyncBoxTask(DataSyncEntity Entity,IDataCache owner, ISyncStream syncOwner)
        //{
        //    this.Entity = Entity;
        //    this.Owner = owner;
        //    this.SyncOwner=syncOwner;
        //    this.ItemName = Entity.EntityName;
        //    TaskMode = SyncBoxTaskMode.DataPreSync;
        //}

        //ISyncStream SyncOwner;
        public DateTime Created { get; private set; }
        /// <summary>
        /// Get the task mode
        /// </summary>
        public SyncBoxTaskMode TaskMode { get; private set; }

        ///// <summary>
        ///// Get item <see cref="ITaskSync"/>.
        ///// </summary>
        //public ITaskSync TaskItem { get; private set; }
        /// <summary>
        /// Get item name.
        /// </summary>
        public string ItemName { get; private set; }

        /// <summary>
        /// Get item <see cref="DataSyncEntity"/>.
        /// </summary>
        public DataSyncEntity Entity { get; private set; }
        /// <summary>
        /// Get Owner <see cref="IDataCache"/>.
        /// </summary>
        public IDataCache Owner { get; private set; }
        /*
        /// <summary>
        /// DoSync
        /// </summary>
        public void DoSync()
        {
            try
            {

                if (TaskMode == SyncBoxTaskMode.PreSync)
                {
                    Task task = Task.Factory.StartNew(() => TaskItem.DoSynchronize());
                    CacheLogger.Debug("SyncBoxTask PreSync : " + ItemName);
                }
                //else if (TaskMode == SyncBoxTaskMode.DataPreSync)
                //{
                //    DataSyncEntity o = Entity;

                //    if (o != null)
                //    {
                             
                //            Task task = Task.Factory.StartNew(() => SyncOwner.ReloadSyncItem(o));
                //            CacheLogger.Info("SyncBoxTask Start DataPreSync : " + o.ViewName);
                //    }
                //    else
                //    {
                //        CacheLogger.Debug("SyncBoxTask DoRefresh DataPreSync Entitiy not found!");
                //    }
                //}
                else
                {

                    DataSyncEntity o = Entity;

                    if (o != null)
                    {

                        if (o.Edited)
                        {
                            Task task = Task.Factory.StartNew(() => o.Refresh(Owner));
                            CacheLogger.Info("SyncBoxTask Start DataSync : " + o.ViewName);
                        }

                    }
                    else
                    {
                        CacheLogger.Debug("SyncBoxTask DoRefresh DataSyncEntitiy not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("SyncBoxTask DoSync Error : " + ex.Message);
            }

        }
        */
        public void DoAsyncTask()
        {
            //~Console.WriteLine("Debuger-SyncTask.DoAsync...");

            //if (this.TaskMode == SyncBoxTaskMode.PreSync)
            //{
            //    Task task = Task.Factory.StartNew(() => this.TaskItem.DoSynchronize());
            //    //CacheLogger.Debug("SyncBoxTask PreSync : " + this.ItemName);
            //}
            //else
            //{
                if (Owner == null)
                {
                    throw new ArgumentException("SyncBoxTask Owner is null");
                }

                if (this.Entity == null)
                {
                    throw new ArgumentException("SyncBoxTask Entity is null");
                }


                if (Owner is DbSet)//DataCache)
                {
                    Task task = Task.Factory.StartNew(() => Entity.SyncAndStore(this.Owner));
                }
                else if (Owner is SyncDb)
                {
                    if (Owner.Parent == null)
                    {
                        throw new ArgumentException("SyncBoxTask Owner.Parent is null");
                    }
                    Task task = Task.Factory.StartNew(() => Owner.Parent.Refresh(Entity.EntityName));
                }
                else
                {
                    throw new NotSupportedException("Owner not supported " + Owner.ToString());
                }
                //Task task = Task.Factory.StartNew(() => o.Refresh(Owner));


                //CacheLogger.Info("SyncBoxTask Start DataSync : " + o.ViewName);
                //else
                //{
                //    CacheLogger.Debug("SyncBoxTask DoRefresh DataSyncEntitiy not found!");
                //}
            //}
        }

        internal object[] ToDataRow()
        {
            if(this.Entity!=null)
                return new object[] { ItemName, TaskMode.ToString(), Created, Entity.LastSync, Entity.SourceName, Owner.ClientId };
            else
                return new object[] { ItemName, TaskMode.ToString(), Created, "", Owner.ConnectionKey, Owner.ClientId };
        }
    }

    /// <summary>
    /// Represent sync task for timer synchronization.
    /// </summary>
    public class SyncTask
    {
        /// <summary>
        /// Get <see cref="IDataCache"/>.
        /// </summary>
        public IDataCache Owner { get; internal set; }

        /// <summary>
        /// Get <see cref="DataSyncEntity"/>.
        /// </summary>
        public DataSyncEntity Entity { get; internal set; }

        ///// <summary>
        ///// Get or Set item <see cref="ITaskSync"/>.
        ///// </summary>
        //public ITaskSync Item {get;set;}
        /// <summary>
        /// Get or Set item name.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Get or Set SyncTimer.
        /// </summary>
        public SyncTimer Timer { get; set; }

        internal DateTime GetNextTime()
        {
            //if (Timer == null)
            //{
            //    NextTime = DateTime.Now.AddSeconds(CacheDefaults.DefaultIntervalSeconds);
            //}
            //    return DateTime.Now;
            return Timer.GetNextValidTime();
        }

        internal bool ShouldRun()
        {
            //if (Timer == null)
            //    return false
            return Timer.HasTimeToRun();
        }

        internal object[] ToDataRow()
        {

            if (Entity == null)
                return new object[] { ItemName, Timer.SyncType.ToString(), Timer.Interval.ToString(), Timer.GetLastTime().ToString("s"), Owner.ConnectionKey, Owner.ClientId };
            else
                return new object[] { ItemName, Timer.SyncType.ToString(), Timer.Interval.ToString(), Entity.LastSync, Entity.ViewName, Owner.ClientId };
        }


        /*
        /// <summary>
        /// Get or Set the interval for synchronization timer.
        /// </summary>
        public int IntervalSeconds { get; set; }

        DateTime NextTime;
        DateTime LastTime;
        /// <summary>
        /// Get indicate if item should run synchronization
        /// </summary>
        /// <returns>return true to run, otherwise return false.</returns>
        public bool ShouldRun()
        {
            if (DateTime.Now < NextTime)
                return false;
            LastTime = NextTime;
            NextTime = DateTime.Now.AddSeconds(IntervalSeconds);
            return true;
        }

        /// <summary>
        /// Get the next time to synchronize by interval.
        /// </summary>
        /// <returns><see cref="DateTime"/></returns>
        public DateTime GetNextTime()
        {
            return NextTime;
        }
        */


    }

}
