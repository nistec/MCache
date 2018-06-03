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
using System.Data;

namespace Nistec.Caching
{
    internal interface ICachePerformance
    {
        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void MemorySizeExchange(ref long memorySize);
        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long GetMaxSize();
        /// <summary>
        /// Get the sync interval in seconds.
        /// </summary>
        int IntervalSeconds { get; }
        /// <summary>
        /// Get indicate whether the cache item is initialized.
        /// </summary>
        bool Initialized { get; }
        /// <summary>
        /// Get indicate whether the cache item is remote cache.
        /// </summary>
        bool IsRemote { get; }
    }
    /// <summary>
    /// Interface for CachePerformanceReport
    /// </summary>
    public interface ICachePerformanceReport
    {
        #region members

        /// <summary>
        /// Get Counter Name.
        /// </summary>
        string CounterName { get; }

        /// <summary>
        /// Get Items count as an atomic operation.
        /// </summary>
        long ItemsCount { get; }


        /// <summary>
        /// Get Request count as an atomic operation.
        /// </summary>
        long RequestCount { get; }

        /// <summary>
        /// Get Response count per hour as an atomic operation.
        /// </summary>
        long ResponseCountPerHour { get; }

        /// <summary>
        /// Get Response count per day as an atomic operation.
        /// </summary>
        long ResponseCountPerDay { get; }


        /// <summary>
        /// Get Response count per month as an atomic operation.
        /// </summary>
        long ResponseCountPerMonth { get; }

        /// <summary>
        /// Get Sync count as an atomic operation.
        /// </summary>
        long SyncCount { get; }

        /// <summary>
        /// Get Start Time.
        /// </summary>
        DateTime StartTime { get; }
        /// <summary>
        /// Get the last time of request action.
        /// </summary>
        DateTime LastRequestTime { get; }
        /// <summary>
        /// Get the last time of response action.
        /// </summary>
        DateTime LastResponseTime { get; }
        /// <summary>
        /// Get Last Sync Time.
        /// </summary>
        DateTime LastSyncTime { get; }
        /// <summary>
        /// Get Max Hit Per Minute.
        /// </summary>
        int MaxHitPerMinute { get; }
        /// <summary>
        /// Get the avarage hit Per Minute.
        /// </summary>
        int AvgHitPerMinute { get; }
        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long MaxSize { get; }


        /// <summary>
        /// Get memory size for current item in bytes as an atomic operation.
        /// </summary>
        long MemoSize
        {
            get;
        }
        /// <summary>
        /// Get the free size memory in bytes for current item as an atomic operation.
        /// </summary>
        long FreeSize
        {
            get;
        }
        /// <summary>
        /// Get the unit size (byte|Kb|Mb)
        /// </summary>
        string UnitSize { get; }



        /// <summary>
        /// Get avarage response time.
        /// </summary>
        float AvgResponseTime
        {
            get;
        }

        /// <summary>
        /// Get avarage sync time.
        /// </summary>
        float AvgSyncTime
        {
            get;
        }

        #endregion
        /// <summary>
        /// Get Performance Report as <see cref="DataTable"/> report.
        /// </summary>
        DataTable PerformanceReport { get; }

    }
}
