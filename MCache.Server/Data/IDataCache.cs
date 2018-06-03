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
using Nistec.Data.Entities;
using System.Data;

namespace Nistec.Caching.Data
{

    ///// <summary>
    ///// Represent an interface who implements sync db cache.
    ///// </summary>
    //public interface IDbCache : IDisposable
    //{
    //    IDataCache GetIDb(string ConnectionKe);
    //}

        /// <summary>
        /// Represent an interface who implements sync db cache.
        /// </summary>
    public interface IDataCache:IDisposable
    {
        ISyncronizer Parent { get;}

        IDataCache Copy();

        /// <summary>
        /// Get cache name.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Get the connection key from config file..
        /// </summary>
        string ConnectionKey { get; }

        ///// <summary>
        ///// Get Db as <see cref="IDbContext"/>.
        ///// </summary>
        //IDbContext Db { get; }

        /// <summary>
        ///  Get Db as <see cref="IDbContext"/>.
        /// </summary>
        /// <returns></returns>
        IDbContext Db();

        /// <summary>
        /// Get table watcher name in db.
        /// </summary>
        string TableWatcherName { get; }
        /// <summary>
        /// Get client id.
        /// </summary>
        string ClientId { get; }
        /// <summary>
        /// Get a collection of tables that should synchronize with db.
        /// </summary>
        DataSyncList SyncTables { get; }
        /// <summary>
        /// Get indicate whether cache should store data on synchronization.
        /// </summary>
        bool EnableDataSource { get; }

        /// <summary>
        /// Get indicate if Store trigger for each table in DataSource 
        /// </summary>
        bool EnableTrigger { get; }
        /// <summary>
        /// Get indicate if allow sync by event. 
        /// </summary>
        bool EnableSyncEvent { get; }
        /// <summary>
        /// Get <see cref="CacheSyncState"/> the sync state.
        /// </summary>
        CacheSyncState SyncState { get; }

        /// <summary>
        ///  Wait until the current item is ready for synchronization, using timeout for waiting in milliseconds.
        /// </summary>
        /// <param name="timeout">timeout in milliseconds</param>
        void WaitForReadySyncState(int timeout);


        ///// <summary>
        ///// Refresh specific item in sync cache.
        ///// </summary>
        ///// <param name="name"></param>
        //void Refresh(string name);

        /// <summary>
        /// Stor WithKey data to data cache.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="mappingName">maooing name in database</param>
        /// <param name="tableName"></param>
        void Store(DataTable dt, string mappingName, string tableName);
        /// <summary>
        /// Raise exception event.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="err"></param>
        void RaiseException(string msg, DataCacheError err);

        bool IsEqual(IDataCache dc);
    }

    internal interface IDbSet
    {
        /// <summary>
        /// Get the connection key from config file..
        /// </summary>
        string ConnectionKey { get; }

        DbCache Owner { get; }

        void ChangeSizeInternal(int size);
    }
}
