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
using Nistec.Data;
using Nistec.Data.Entities;
using System.Collections;
using Nistec.Data.Entities.Cache;
using Nistec.IO;
using Nistec.Generic;
using System.Data;

namespace Nistec.Caching.Sync
{
   
    /// <summary>
    /// ISyncItemBase interface
    /// </summary>
    public interface ISyncItemBase
    {
        /// <summary>
        /// Get Item type.
        /// </summary>
        Type ItemType { get; }
        /// <summary>
        /// Get <see cref="CacheKeyInfo"/>
        /// </summary>
        CacheKeyInfo Info { get; set; }
        /// <summary>
        /// Get or Set <see cref="DataSyncEntity"/>
        /// </summary>
        DataSyncEntity SyncSource { get; set; }
        /// <summary>
        /// Get or Set <see cref="DataFilter"/>
        /// </summary>
        DataFilter Filter { get; set; }
        /// <summary>
        /// Get Sync item values
        /// </summary>
        ICollection Values { get; }

        /// <summary>
        /// Get Sync item Keys
        /// </summary>
        ICollection Keys { get; }

        /// <summary>
        /// Get Items count
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Refresh Item
        /// </summary>
        /// <param name="state"></param>
        void Refresh(object state);
        /// <summary>
        /// Refresh Item Async
        /// </summary>
        /// <param name="state"></param>
        void RefreshAsync(object state);
        /// <summary>
        /// Get if Sync item contains specific item using <see cref="CacheKeyInfo"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool Contains(CacheKeyInfo info);
    }

    internal interface ISyncCacheItem 
    {
        void Set(SyncEntity entity, bool isAsync);
    }

    /// <summary>
    /// ISyncItem interface
    /// </summary>
    public interface ISyncItem : ISyncItemBase
    {
 
        /// <summary>
        /// Get item from sync cache as dictionary.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        IDictionary GetRecord(CacheKeyInfo info);
        /// <summary>
        /// Get item from sync cache.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        object GetItem(CacheKeyInfo info);
        /// <summary>
        /// Get item from sync cache as stream dictionary.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        AckStream GetRecordStream(CacheKeyInfo info);
        /// <summary>
        /// Get item from sync cache as stream.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        AckStream GetItemStream(CacheKeyInfo info);

        /// <summary>
        /// Get db connection key.
        /// </summary>
        string ConnectionKey { get; }
       /// <summary>
        /// Validate CacheKeyInfo
        /// </summary>
        void Validate();
    }

    /// <summary>
    /// ISyncItemStream interface
    /// </summary>
    public interface ISyncItemStream : ISyncItemBase
    {
        /// <summary>
        /// Get item from sync cache as <see cref="EntityStream"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        EntityStream GetEntityStream(CacheKeyInfo info);
        /// <summary>
        /// Get item from sync cache as stream.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        NetStream GetItemStream(CacheKeyInfo info);

        /// <summary>
        /// Get entity items copy.
        /// </summary>
        /// <param name="copyBody"></param>
        /// <returns></returns>
        GenericKeyValue GetEntityItems(bool copyBody);
        /// <summary>
        /// Get entity keys
        /// </summary>
        /// <returns></returns>
        ICollection<string> GetEntityKeys();
        /// <summary>
        /// Get entity items Report
        /// </summary>
        /// <returns></returns>
        DataTable GetItemsReport();
        /// <summary>
        /// Get the cuurent size in bytws.
        /// </summary>
        long Size { get; }
        
    }
}
