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


namespace Nistec.Caching.Remote
{

    #region Known args


    internal class SessionCmdDefaults
    {

        /// <summary>MSessionPipeName.</summary>
        public const string MSessionPipeName = "nistec_ession";
        /// <summary>MSessionManagerPipeName.</summary>
        public const string MSessionManagerPipeName = "nistec_session_manager";
    }


    /// <summary>
    /// Represent known args for api commands.
    /// </summary>
    public class KnowsArgs
    {
        //public const string SessionId = "SessionId";
        /// <summary>Source.</summary>
        public const string Source = "source";
        /// <summary>Destination.</summary>
        public const string Destination = "destination";

        /// <summary>ConnectionKey.</summary>
        public const string ConnectionKey = "connectionkey";
        /// <summary>TableName.</summary>
        public const string TableName = "tablename";
        /// <summary>MappingName.</summary>
        public const string MappingName = "mappingname";
        /// <summary>SourceName.</summary>
        public const string SourceName = "sourcename";
        /// <summary>SourceType.</summary>
        public const string SourceType = "sourcetype";
        ///// <summary>EntityName.</summary>
        //public const string EntityName = "EntityName";
        /// <summary>EntityType.</summary>
        public const string EntityType = "entitytype";
        /// <summary>Primary key.</summary>
        public const string Pk = "pk";
        /// <summary>Filter.</summary>
        public const string Filter = "filter";
        /// <summary>Column.</summary>
        public const string Column = "column";
        /// <summary>EntityKeys.</summary>
        public const string EntityKeys = "entitykeys";
        /// <summary>UserId.</summary>
        public const string UserId = "userid";
        /// <summary>TargetKey.</summary>
        public const string TargetKey = "targetkey";
        /// <summary>AddToCache.</summary>
        public const string AddToCache = "addtocache";
        /// <summary>IsAsync.</summary>
        public const string IsAsync = "isasync";
        /// <summary>StrArgs.</summary>
        public const string StrArgs = "strArgs";
        /// <summary>ShouldSerialized.</summary>
        public const string ShouldSerialized = "shouldserialized";

        /// <summary>CloneType.</summary>
        public const string CloneType = "clonetype";
        /// <summary>SyncType.</summary>
        public const string SyncType = "synctype";
        /// <summary>SyncTime.</summary>
        public const string SyncTime = "synctime";
        //public const string Timeout = "Timeout";

    }

    #endregion

    #region commands
    /// <summary>
    /// Represent all cache api command.
    /// </summary>
    public class CacheCmd
    {
        /// <summary>
        /// Reply for test.
        /// </summary>
        public const string Reply = "cach_reply";
        /// <summary>Remove item from cache.</summary>
        public const string Remove = "cach_remove";
        /// <summary>Remove item from cache async.</summary>
        public const string RemoveAsync = "cach_removeasync";
        /// <summary>Get item properties from cache.</summary>
        public const string ViewEntry = "cach_viewentry";
        /// <summary>Get item value and properties from cache.</summary>
        public const string GetEntry = "cach_getentry";
        /// <summary>Get value from cache.</summary>
        public const string GetRecord = "cach_getrecord";
        /// <summary>Get value from cache.</summary>
        public const string Get = "cach_get";
        /// <summary>Fetch value from cache.</summary>
        public const string Fetch = "cach_fetch";
        /// <summary>Fetch item properties and value from cache.</summary>
        public const string FetchEntry = "cach_fetchentry";
        /// <summary>Add new item to cache.</summary>
        public const string Add = "cach_add";
        /// <summary>Add new item to cache.</summary>
        public const string Set = "cach_set";
        /// <summary>Keep alive item in cache.</summary>
        public const string KeepAliveItem = "cach_keepaliveitem";
        /// <summary>Load data item to or from cache.</summary>
        public const string LoadData = "cach_loaddata";

        /// <summary>Duplicate item to a new destination in cache.</summary>
        public const string CopyTo = "cach_copyto";
        /// <summary>Duplicate item to a new destination in cache and remove the old item.</summary>
        public const string CutTo = "cach_cutto";
        ///// <summary>Merge item with a new collection items, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/>.</summary>
        //public const string MergeItem = "cach_MergeItem";
        ///// <summary>Merge item by remove all items from the current item that match the collection items in argument, this feature is valid only for items implements <see cref="System.Collections.ICollection"/> or <see cref="System.ComponentModel.IListSource"/> and return <see cref="CacheState"/>.</summary>
        //public const string MergeItemRemove = "cach_MergeItemRemove";
        /// <summary>Remove all items from cache that belong to specified session..</summary>
        public const string RemoveItemsBySession = "cach_removeitemsbysession";
    }

    /// <summary>
    /// Represent data cache commands.
    /// </summary>
    public class DataCacheCmd
    {
        /// <summary>Reply.</summary>
        public const string Reply = "data_reply";
        /// <summary>Set value to specified field in row.</summary>
        public const string Set = "data_setvalue";
        /// <summary>Add value to specified field in row.</summary>
        public const string Add = "data_addvalue";
        /// <summary>Get value from specified field in row.</summary>
        public const string Get = "data_value";
        /// <summary>Get row from specified table.</summary>
        public const string GetRecord = "data_getrecord";
        /// <summary>Get row stream from specified table.</summary>
        public const string GetStream = "data_getstream";

        ///// <summary>Get data cache statistic.</summary>
        //public const string GetDataStatistic = "data_GetDataStatistic";

        /// <summary>Add a new data item to data cache.</summary>
        public const string AddTable = "data_addtable";
        /// <summary>Add a new data item to data cache with sync.</summary>
        public const string AddTableWithSync = "data_addtablewithsync";

        /// <summary>Set a new data item with override to data cache.</summary>
        public const string SetTable = "data_settable";
        /// <summary>Get data table from specified data cache.</summary>
        public const string GetTable = "data_gettable";
        /// <summary>Remove table from data cache.</summary>
        public const string RemoveTable = "data_removetable";

        ///// <summary>Add a new data item to data cache async.</summary>
        //public const string AddDataItemSync = "data_AddDataItemSync";
        /// <summary>Add Item to SyncTables.</summary>
        public const string AddSyncItem = "data_addsyncitem";
        /// <summary>Get item properties.</summary>
        public const string GetItemProperties = "data_getitemproperties";
        /// <summary>Refresh specified table in db cache.</summary>
        public const string Refresh = "data_refresh";
        /// <summary>Restart all items in db cache.</summary>
        public const string Reset = "data_reset";
        /// <summary>Get indicate werher the table exists in db cache.</summary>
        public const string Contains = "data_contains";

        /// <summary>Get all items copy for specified entity from data cache.</summary>
        public const string GetEntityItems = "data_getentityitems";
        /// <summary>Get all keys for specified entity from data cache.</summary>
        public const string GetEntityKeys = "data_getentitykeys";
        /// <summary>Get report of all items for specified entity from data cache.</summary>
        public const string GetItemsReport = "data_getitemsreport";
        /// <summary>Get all entites names from data cache.</summary>
        public const string GetAllEntityNames = "data_getallentitynames";

        /// <summary>Get all items count for specified entity from data cache.</summary>
        public const string GetEntityItemsCount = "data_getentityitemscount";

        /// <summary>Exceute table query from data cache.</summary>
        public const string QueryTable = "data_querytable";
        /// <summary>Exceute entity query from data cache.</summary>
        public const string QueryEntity = "data_queryentity";

    }

    /// <summary>
    /// Represent session commands.
    /// </summary>
    public class SessionCmd
    {
        /// <summary>Reply.</summary>
        public const string Reply = "sess_reply";
        /// <summary>Add new session to sessions cache.</summary>
        public const string CreateSession = "sess_createsession";
        /// <summary>Remove session from sessions cache.</summary>
        public const string RemoveSession = "sess_removesession";
        /// <summary>Clear all item from specified session.</summary>
        public const string ClearItems = "sess_clearitems";
        /// <summary>Clear all sessions from session cache.</summary>
        public const string ClearAll = "sess_clearall";
        ///// <summary>Get existing session, if session not exists return null.</summary>
        //public const string GetSessionStream = "sess_GetSessionStream";

        ///// <summary>Get existing session, if session not exists return null.</summary>
        //public const string GetExistingSession = "sess_GetExistingSession";
        ///// <summary>Get existing session, if session not exists return null.</summary>
        //public const string GetExistingSessionRecord = "sess_GetExistingSessionRecord";

        /// <summary>Get session items.</summary>
        public const string GetSessionItems = "sess_getsessionitems";

        /// <summary>Get existing session, if session not exists create new one.</summary>
        public const string GetOrCreateSession = "sess_getorcreatesession";
        /// <summary>Get existing session, if session not exists create new one.</summary>
        public const string GetOrCreateRecord = "sess_getorcreaterecord";
        /// <summary>Refresh session.</summary>
        public const string Refresh = "sess_refresh";
        /// <summary>Refresh sfcific session in session cache or create a new session bag if not exists.</summary>
        public const string RefreshOrCreate = "sess_refreshorcreate";

        /// <summary>Remove item from specified session.</summary>
        public const string Remove = "sess_remove";
        /// <summary>Add item to existing session, if session not exists do nothing.</summary>
        public const string Add = "sess_add";
        /// <summary>Add item to session, if session not exists create new one and add the the item to session created.</summary>
        public const string Set = "sess_set";
        /// <summary>Get item from specified session.</summary>
        public const string GetEntry = "sess_getentry";
        /// <summary>Get value from specified session.</summary>
        public const string Get = "sess_get";
        /// <summary>Get item from specified session.</summary>
        public const string GetRecord = "sess_getrecord";
        /// <summary>Fetch item from specified session.</summary>
        public const string Fetch = "sess_fetch";
        /// <summary>Fetch item from specified session.</summary>
        public const string FetchRecord = "sess_fetchrecord";
        /// <summary>Copy item from specified session to a new place.</summary>
        public const string CopyTo = "sess_copyto";
        /// <summary>Cut item from specified session to a new place..</summary>
        public const string CutTo = "sess_cutto";
        /// <summary>Get indicate whether the session is exists.</summary>
        public const string Exists = "sess_exists";

        /// <summary>Get all sessions keys.</summary>
        public const string ViewAllSessionsKeys = "sess_viewallsessionskeys";
        /// <summary>Get all sessions keys using SessionState state.</summary>
        public const string ViewAllSessionsKeysByState = "sess_viewallsessionskeysbystate";
        /// <summary>Get all items keys from specified session.</summary>
        public const string ViewSessionKeys = "sess_viewsessionkeys";
        /// <summary>Get existing session, if session not exists return null.</summary>
        public const string ViewSessionStream = "sess_viewtsessionstream";

        /// <summary>View item from specified session.</summary>
        public const string ViewEntry = "sess_viewentry";
    }

    /// <summary>
    /// Represent sync cache commands.
    /// </summary>
    public class SyncCacheCmd
    { 
        /// <summary>Reply.</summary>
        public const string Reply = "sync_reply";
        /// <summary>Get item from sync cache.</summary>
        public const string Get = "sync_get";
        /// <summary>Get item as dictionary from sync cache.</summary>
        public const string GetRecord = "sync_getrecord";
        /// <summary>Get item as Entity from sync cache.</summary>
        public const string GetEntity = "sync_getentity";
        /// <summary>Get item as Entity copy using stream convert from sync cache.</summary>
        public const string GetAs = "sync_getas";
        /// <summary>Get indicate werher the item exists in sync cache.</summary>
        public const string Contains = "sync_contains";
        /// <summary>Add new item to sync cache.</summary>
        public const string AddSyncItem = "sync_addsyncitem";
        /// <summary>Add new entity to sync cache.</summary>
        public const string AddEntity = "sync_addentity";
        /// <summary>Remove item from sync cache.</summary>
        public const string Remove = "sync_remove";

        ///// <summary>Get item as entity stream from sync cache.</summary>
        //public const string GetEntityStream = "sync_GetEntityStream";

        /// <summary>Refresh all items in sync cache.</summary>
        public const string RefreshAll = "sync_refreshall";
        /// <summary>Refresh specified item in sync cache.</summary>
        public const string Refresh = "sync_refresh";
        /// <summary>Get all items copy for specified entity from sync cache.</summary>
        public const string GetEntityItems = "sync_getentityitems";
        /// <summary>Get all keys for specified entity from sync cache.</summary>
        public const string GetEntityKeys = "sync_getentitykeys";
        /// <summary>Get report of all items for specified entity from sync cache.</summary>
        public const string GetItemsReport = "sync_getitemsreport";
        /// <summary>Get all entites names from sync cache.</summary>
        public const string GetAllEntityNames = "sync_getallentitynames";
        /// <summary>Restart all items in sync cache.</summary>
        public const string Reset = "sync_reset";
        /// <summary>Get all items count for specified entity from sync cache.</summary>
        public const string GetEntityItemsCount = "sync_getentityitemscount";
        /// <summary>Get item properties.</summary>
        public const string GetItemProperties = "sync_getitemproperties";

        ///// <summary>Get sync cache statistic.</summary>
        //public const string GetSyncStatistic = "sync_GetSyncStatistic";

        /// <summary>Get item EntityPrimaryKey.</summary>
        public const string GetEntityPrimaryKey = "sync_getentityprimarykey";
        /// <summary>Find entity.</summary>
        public const string FindEntity = "sync_findentity";

    }

    #endregion

    #region Manager cmd

    /// <summary>
    /// Represent the cache managment command.
    /// </summary>
    public class CacheManagerCmd
    {
        /// <summary>Reply.</summary>
        public const string Reply = "mang_reply";
        /// <summary>Get cache properties.</summary>
        public const string CacheProperties = "mang_cacheproperties";
        /// <summary>Cmd.</summary>
        public const string Timeout = "mang_timeout";
        /// <summary>Cmd.</summary>
        public const string SessionTimeout = "mang_sessiontimeout";
        /// <summary>Get list of all key items in cache.</summary>
        public const string GetAllKeys = "mang_getallkeys";
        /// <summary>Get list of all key items in cache.</summary>
        public const string GetAllKeysIcons = "mang_getallkeysicons";
        /// <summary>Get list of all items copy in cache .</summary>
        public const string CloneItems = "mang_cloneitems";
        ///// <summary>Get statistic report.</summary>
        //public const string GetStatistic = "mang_GetStatistic";
        /// <summary>Get performance report.</summary>
        public const string GetPerformanceReport = "mang_getperformancereport";
        /// <summary>Get performance report for specified agent.</summary>
        public const string GetAgentPerformanceReport = "mang_getagentperformancereport";
        /// <summary>Get list of all key items in data cache.</summary>
        public const string GetAllDataKeys = "mang_getalldatakeys";
        ///// <summary>Get data statistic report.</summary>
        //public const string GetDataStatistic = "mang_GetDataStatistic";
        /// <summary>Get list of all entites name items in sync cache.</summary>
        public const string GetAllSyncCacheKeys = "mang_getallsynccachekeys";
        /// <summary>Save cache to xml file.</summary>
        public const string CacheToXml = "mang_cachetoxml";
        /// <summary>Load cache from xml file.</summary>
        public const string CacheFromXml = "mang_cachefromxml";
        /// <summary>Get cache log.</summary>
        public const string CacheLog = "mang_cachelog";
        /// <summary>Reset cache.</summary>
        public const string Reset = "mang_reset";
        ///// <summary>Get all sessions keys.</summary>
        //public const string GetAllSessionsKeys = "mang_GetAllSessionsKeys";
        ///// <summary>Get all sessions keys using <see cref="Nistec.Caching.Session.SessionState"/> state.</summary>
        //public const string GetAllSessionsStateKeys = "mang_GetAllSessionsStateKeys";
        ///// <summary>Get all items keys from specified session.</summary>
        //public const string GetSessionItemsKeys = "mang_GetSessionItemsKeys";
        ///// <summary>Get all items keys from specified session.</summary>
        //public const string GetSessionItemRecord = "mang_GetSessionItemRecord";
        /// <summary>Get state counter report.</summary>
        public const string GetStateCounterReport = "mang_getstatecounterreport";
        /// <summary>Reset Performance Counter.</summary>
        public const string ResetPerformanceCounter = "mang_resetperformancecounter";
        /// <summary>Get Cache State Counter.</summary>
        public const string StateCounterCache = "mang_statecountercache";
        /// <summary>Get SyncCache State Counter.</summary>
        public const string StateCounterSync = "mang_statecountersync";
        /// <summary>Get SessionCache State Counter.</summary>
        public const string StateCounterSession = "mang_statecountersession";
        /// <summary>Get DataCache State Counter.</summary>
        public const string StateCounterDataCache = "mang_statecounterdatacache";
        ///// <summary>Get report of all items in cache.</summary>
        //public const string GetCacheItemsReport = "mang_GetCacheItemsReport";
        ///// <summary>Get report of all items in session cache.</summary>
        //public const string GetSessionItemsReport = "mang_GetSessionItemsReport";

        /// <summary>Get report of all items in session cache.</summary>
        public const string ReportSessionItems = "mang_reportsessionitems";
        /// <summary>Get report of all items in cache.</summary>
        public const string ReportCacheItems = "mang_reportcacheitems";
        /// <summary>Get report of cache timer.</summary>
        public const string ReportCacheTimer = "mang_reportcachetimer";
        /// <summary>Get report of session timer.</summary>
        public const string ReportSessionTimer = "mang_reportsessiontimer";
        /// <summary>Get report of cache timer.</summary>
        public const string ReportDataTimer = "mang_reportdatatimer";
        /// <summary>Get report of sync box items.</summary>
        public const string ReportSyncBoxItems = "mang_reportsyncboxitems";
        /// <summary>Get report of sync box queue.</summary>
        public const string ReportSyncBoxQueue = "mang_reportsyncboxqueue";
        /// <summary>Get report of sync timer.</summary>
        public const string ReportTimerSyncDispatcher = "mang_reporttimersyncdispatcher";

    }


    /// <summary>
    /// Represent session managment command.
    /// </summary>
    public class SessionManagerCmd
    {
        /// <summary>Log.</summary>
        public const string Log = "Log";
        /// <summary>GetAllSessionsKeys.</summary>
        public const string GetAllSessionsKeys = "GetAllSessionsKeys";
        /// <summary>GetAllSessionsStateKeys.</summary>
        public const string GetAllSessionsStateKeys = "GetAllSessionsStateKeys";
        /// <summary>ReGetAllSessionsply.</summary>
        public const string GetAllSessions = "GetAllSessions";
        /// <summary>GetActiveSessions.</summary>
        public const string GetActiveSessions = "GetActiveSessions";
        /// <summary>GetSessionsItemsKeys.</summary>
        public const string GetSessionsItemsKeys = "GetSessionsItemsKeys";
    }

    #endregion
}
