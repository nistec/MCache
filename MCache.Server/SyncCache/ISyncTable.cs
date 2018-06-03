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
using Nistec.Channels;

namespace Nistec.Caching.Sync
{
   
    /// <summary>
    /// ISyncTableBase interface
    /// </summary>
    public interface ISyncTableBase //-<T>
    {
        //SyncCacheBase Owner { get; }

        /// <summary>
        /// Get Item type.
        /// </summary>
        Type ItemType { get; }
        ///// <summary>
        ///// Get <see cref="ComplexKey"/>
        ///// </summary>
        //ComplexKey Info { get; set; }
        
        /// <summary>
        /// Get <see cref="ComplexKey"/>
        /// </summary>
        string[] FieldsKey { get;}
        /// <summary>
        /// Get <see cref="ComplexKey"/>
        /// </summary>
        string EntityName { get;}
        /// <summary>
        /// Get or Set <see cref="DataSyncEntity"/>
        /// </summary>
        DataSyncEntity SyncSource { get; set; }
        /// <summary>
        /// Get or Set <see cref="DataFilter"/>
        /// </summary>
        DataFilter Filter { get; set; }
        /// <summary>
        /// Get Items count
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Get Modified time.
        /// </summary>
        DateTime Modified { get; }

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
        /// Get if Sync item contains specific item using <see cref="ComplexKey"/>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool Contains(ComplexKey info);

        string GetPrimaryKey(NameValueArgs keyValueArgs);
    }

    internal interface ISyncCacheItem 
    {
        
        void Set(SyncEntity entity,  bool isAsync);//SyncCacheBase owner,
    }
    //internal interface ISyncTableOwner
    //{
    //    SyncCacheBase Owner{get;set;}
    //}
    /// <summary>
    /// ISyncTable interface
    /// </summary>
    public interface ISyncTable : ISyncTableBase //-<T>
    {
 
        /// <summary>
        /// Get item from sync cache as dictionary.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        IDictionary GetRecord(ComplexKey info);
        /// <summary>
        /// Get item from sync cache.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        object GetItem(ComplexKey info);
        /// <summary>
        /// Get item from sync cache as stream dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        NetStream GetRecordStream(string key);
        /// <summary>
        /// Get item from sync cache as stream.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        NetStream GetItemStream(string key);

        /// <summary>
        /// Get db connection key.
        /// </summary>
        string ConnectionKey { get; }
       /// <summary>
        /// Validate ComplexKey
        /// </summary>
        void Validate();
    }

    /// <summary>
    /// ISyncTableStream interface
    /// </summary>
    public interface ISyncTableStream : ISyncTableBase //-<T>
    {

        /// <summary>
        /// Get copy of item from sync cache as <see cref="EntityStream"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        EntityStream GetEntityStream(string key);
        /// <summary>
        /// TryGet copy of item from sync cache as <see cref="EntityStream"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TryGetEntity(string key, out EntityStream item);

        /// <summary>
        /// Get copy of item from sync cache as stream.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        NetStream GetItemStream(string key);

        /// <summary>
        /// Get entity items copy.
        /// </summary>
        /// <param name="copyBody"></param>
        /// <returns></returns>
        GenericKeyValue GetEntityItems(bool copyBody);
        /// <summary>
        /// Get entity items count.
        /// </summary>
        /// <returns></returns>
        int GetEntityItemsCount();
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
        /// Get the cuurent size in bytes.
        /// </summary>
        long Size { get; }
        
    }
}
