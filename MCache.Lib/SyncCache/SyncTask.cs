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
    /// <summary>
    /// Represent synchronization task interface.
    /// </summary>
    public interface ITaskSync
    {
        /// <summary>
        /// Do synchronization action.
        /// </summary>
        void DoSynchronize();
        /// <summary>
        /// Get indicate whether the item is disposed.
        /// </summary>
        bool IsDisposed { get; }
    }

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
    }

     /// <summary>
    /// Represent sync task for SyncBox synchronization.
    /// </summary>
    internal class SyncBoxTask
    {
        /// <summary>
        /// Initialize a new instance of sync box for PreSync.
        /// </summary>
        /// <param name="TaskItem"></param>
        /// <param name="ItemName"></param>
        public SyncBoxTask(ITaskSync TaskItem, string ItemName)
        {
            this.TaskItem = TaskItem;
            this.ItemName = ItemName;
            TaskMode = SyncBoxTaskMode.PreSync;
        }
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
        }

        /// <summary>
        /// Get the task mode
        /// </summary>
        public SyncBoxTaskMode TaskMode { get; private set; }

        /// <summary>
        /// Get item <see cref="ITaskSync"/>.
        /// </summary>
        public ITaskSync TaskItem { get; private set; }
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
                else
                {

                    DataSyncEntity o = Entity;

                    if (o != null)
                    {

                        if (o.Edited)
                        {
                            Task task = Task.Factory.StartNew(() => o.Refresh(Owner));
                            CacheLogger.Info("SyncBoxTask Start Sync : " + o.ViewName);
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
    }

    /// <summary>
    /// Represent sync task for timer synchronization.
    /// </summary>
    public class SyncTask
    {
        /// <summary>
        /// Get or Set item <see cref="ITaskSync"/>.
        /// </summary>
        public ITaskSync Item {get;set;}
        /// <summary>
        /// Get or Set item name.
        /// </summary>
        public string ItemName { get; set; }

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

    }

}
