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
using Nistec.Data.Entities;

namespace Nistec.Caching.Data
{
    /// <summary>
    /// Interface for data sync.
    /// </summary>
    public interface IDataSyncEntity : IDisposable
    {
        #region properties


        /// <summary>
        /// Get EntityName
        /// </summary>
        string EntityName { get; }
        /// <summary>
        /// Get ViewName
        /// </summary>
        string ViewName { get; }
        /// <summary>
        /// Get SourceName
        /// </summary>
        string[] SourceName { get; }
        /// <summary>
        /// Get PreserveChanges
        /// </summary>
        bool PreserveChanges { get; }
        /// <summary>
        /// Get MissingSchemaAction
        /// </summary>
        MissingSchemaAction MissingSchemaAction { get; }

        /// <summary>
        /// Get syncTime
        /// </summary>
        TimeSpan Interval { get; }
        /// <summary>
        /// Get syncTime
        /// </summary>
        SyncType SyncType { get; }
        /// <summary>
        /// Get SourceType
        /// </summary>
        EntitySourceType SourceType { get; }

        /// <summary>
        /// Get indicate whether cache should use with nolock statement.
        /// </summary>
        bool EnableNoLock { get; }
        /// <summary>
        /// Get the last sync.
        /// </summary>
        string LastSync { get; }

        #endregion
       

    }
}
