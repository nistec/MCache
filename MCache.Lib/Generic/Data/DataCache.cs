using System;
using System.Data;
using System.Collections;
using System.Threading;
using Nistec.Data;
using System.Collections.Generic;
//using Nistec.Util;
using Nistec.Caching;
//using Nistec.Data.Common;
using Nistec.Xml;
using System.Xml;

namespace Nistec.Caching.Data
{

    public interface IDataCache
    {
        #region Properties

        IDBCmd dbCmd { get; }

        /// <summary>
        /// Get or Set SyncOption
        /// </summary>
        SyncOption SyncOption{get;}


        /// <summary>
        /// Get or set Cache Name  
        /// </summary>
        string CacheName{get;set;}
 
        /// <summary>
        /// Get DataCacheState  
        /// </summary>
        DataCacheState DataCacheState{get;}
      
        /// <summary>
        /// Get SyncTables collection
        /// </summary>
        SyncSourceCollection SyncTables{get;}
       

        /// <summary>
        /// Get the ClientId  (MachineName)
        /// </summary>
        string ClientId{get;}
        /// <summary>
        /// Get or set Table Watcher Name
        /// </summary>
        string TableWatcherName{get;}
       
        #endregion

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        void Store(DataTable dt, string tableName);

    }

}
