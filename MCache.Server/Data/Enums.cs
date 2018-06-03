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
using System.Text;

namespace Nistec.Caching.Data
{

 

    /// <summary>
    /// SyncOption
    /// </summary>
    public enum DataSyncOption
    {
        /// <summary>
        /// Manual
        /// </summary>
        Manual,
        /// <summary>
        /// Auto
        /// </summary>
        Auto,
 
    }
    /// <summary>
    /// CacheState
    /// </summary>
    public enum DataCacheState
    {
        /// <summary>
        /// Dal storage is Open
        /// </summary>
        Open,
        /// <summary>
        /// Dal storage is in Synch
        /// </summary>
        Synch,
        /// <summary>
        /// Dal storage is Closed
        /// </summary>
        Closed
    }

    /// <summary>
    /// CacheSettingState
    /// </summary>
    public enum CacheSettingState
    {
        /// <summary>
        /// Dal CacheSettingState is Started
        /// </summary>
        Started,
        /// <summary>
        /// Dal CacheSettingState is Stoped
        /// </summary>
        Stoped
    }


    /// <summary>
    /// SyncType
    /// </summary>
    public enum CacheSyncType
    {
        /// <summary>
        /// No sync time
        /// </summary>
        None,
        /// <summary>
        /// Dal SyncType By Timer
        /// </summary>
        Timer,
        /// <summary>
        /// Dal SyncType By Event
        /// </summary>
        Event

    }

}
