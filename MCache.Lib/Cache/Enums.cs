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

namespace Nistec.Caching
{

   

    /// <summary>CacheState.</summary>
    [Serializable]
    public enum CacheState
    {
        /// <summary>Ok.</summary>
        Ok = 0,
        /// <summary>ItemAdded.</summary>
        ItemAdded = 1,
        /// <summary>ItemChanged.</summary>
        ItemChanged = 2,
        /// <summary>ItemRemoved.</summary>
        ItemRemoved = 3,
        /// <summary>ItemNotFount.</summary>
        NotFound = 100,
        /// <summary>CacheNotReady.</summary>
        CacheNotReady = 501,
        /// <summary>CacheIsFull.</summary>
        CacheIsFull = 502,
        /// <summary>InvalidItem.</summary>
        InvalidItem = 503,
        /// <summary>InvalidSession.</summary>
        InvalidSession = 504,
        /// <summary>AddItemFailed.</summary>
        AddItemFailed = 505,
        /// <summary>MergeItemFailed.</summary>
        MergeItemFailed = 506,
        /// <summary>CopyItemFailed.</summary>
        CopyItemFailed = 507,
        /// <summary>RemoveItemFailed.</summary>
        RemoveItemFailed = 508,
        /// <summary>ArgumentsError.</summary>
        ArgumentsError = 509,
        /// <summary>ItemAllreadyExists.</summary>
        ItemAllreadyExists = 510,
         /// <summary>SerializationError.</summary>
        SerializationError = 511,
        /// <summary>CommandNotSupported.</summary>
        CommandNotSupported = 512,
        /// <summary>UnexpectedError.</summary>
        UnexpectedError = 599
    }

    /// <summary>
    /// Represent one of the cache object type
    /// </summary>
    [Serializable]
    public enum CacheAgentType
    {
        /// <summary>
        /// Cache <see cref="MCache"/>
        /// </summary>
        Cache,
        /// <summary>
        /// DataCache <see cref="DataCache"/>
        /// </summary>
        DataCache,
        /// <summary>
        /// Session Cache <see cref="SessionCache"/>
        /// </summary>
        SessionCache,
        /// <summary>
        /// SyncCache <see cref="SyncCache"/>
        /// </summary>
        SyncCache
    }

    /// <summary>
    /// CacheObjType
    /// </summary>
    [Serializable]
    public enum CacheObjType
    {
        /// <summary>Default.</summary>
        Default = 0,
        /// <summary>RemotingData.</summary>
        RemotingData = 1,
        /// <summary>TextFile.</summary>
        TextFile = 2,
        /// <summary>BinaryFile.</summary>
        BinaryFile = 3,
        /// <summary>ImageFile.</summary>
        ImageFile = 4,
        /// <summary>XmlDocument.</summary>
        XmlDocument = 5,
        /// <summary>SerializeClass.</summary>
        SerializeClass = 6,
        /// <summary>HtmlFile.</summary>
        HtmlFile = 7
    }

    /// <summary>
    /// CloneType
    /// </summary>
    public enum CloneType
    {
        /// <summary>All.</summary>
        All = 0,
        /// <summary>Timeout.</summary>
        Timeout = 1,
        /// <summary>Session.</summary>
        Session = 2
    }

    /// <summary>
    /// CacheSettingState
    /// </summary>
    public enum CacheSettingState
    {
        /// <summary>
        /// Cache CacheSettingState is Started
        /// </summary>
        Started,
        /// <summary>
        /// Cache CacheSettingState is Stoped
        /// </summary>
        Stoped
    }

    /// <summary>
    /// CacheSyncState
    /// </summary>
    public enum CacheSyncState
    {
        /// <summary>
        /// Cache CacheSyncState is in idle
        /// </summary>
        Idle=0,
        /// <summary>
        /// Cache CacheSyncState Should Start
        /// </summary>
        ShouldStart=1,
        /// <summary>
        /// Cache CacheSyncState is Started
        /// </summary>
        Started=2,
        /// <summary>
        /// Cache CacheSyncState is Stoped
        /// </summary>
        Finished=3
    }
    /// <summary>
    /// CacheActionState
    /// </summary>
    public enum CacheActionState
    {
        /// <summary>None.</summary>
        None,
        /// <summary>Ok.</summary>
        Ok,
        /// <summary>Debug.</summary>
        Debug,
        /// <summary>Failed.</summary>
        Failed,
        /// <summary>Error.</summary>
        Error,
    }
    /// <summary>
    /// CacheAction
    /// </summary>
    public enum CacheAction
    {
        /// <summary>General.</summary>
        General,
        /// <summary>GetItem.</summary>
        GetItem,
        /// <summary>FetchItem.</summary>
        FetchItem,
        /// <summary>ViewItem.</summary>
        ViewItem,
        /// <summary>LoadItem.</summary>
        LoadItem,
        /// <summary>AddItem.</summary>
        AddItem,
        /// <summary>RemoveItem.</summary>
        RemoveItem,
        /// <summary>SyncItem.</summary>
        SyncItem,
        /// <summary>TimeoutExpired.</summary>
        TimeoutExpired,
        /// <summary>RecallItem.</summary>
        RecallItem,
        /// <summary>ReduceItem.</summary>
        ReduceItem,
        /// <summary>ChangedItem.</summary>
        ChangedItem,
        /// <summary>ClearCache.</summary>
        ClearCache,
        /// <summary>CacheException.</summary>
        CacheException,
        /// <summary>ResetAll.</summary>
        ResetAll,
        /// <summary>SyncTime.</summary>
        SyncTime,
        /// <summary>DataCacheError.</summary>
        DataCacheError,
        /// <summary>Memory Size Exchange.</summary>
        MemorySizeExchange
    }

}
