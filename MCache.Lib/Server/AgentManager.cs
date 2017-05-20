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
using Nistec.Generic;
using Nistec.Threading;
using Nistec.IO;
using Nistec.Caching.Remote;
using Nistec.Runtime;
using Nistec.Caching.Session;
using Nistec.Caching.Config;
using System.Data;


namespace Nistec.Caching.Server
{
    /// <summary>
    /// Represent Singleton Agent Manager
    /// </summary>
    public class AgentManager
    {

        #region Remote cache

        static CacheAgent _Cache;
        /// <summary>
        /// Get <see cref="CacheAgent"/> as Singleton.
        /// </summary>
        public static CacheAgent Cache
        {
            get
            {
                if (_Cache == null)
                {
                    _Cache = new CacheAgent(CacheProperties.LoadProperties());
                }
                return _Cache;
            }
        }

        static SessionAgent _Session;
        /// <summary>
        /// Get <see cref="SessionAgent"/> as Singleton.
        /// </summary>
        internal static SessionAgent Session
        {
            get
            {
                if (_Session == null)
                {
                    _Session = new SessionAgent();
                }
                return _Session;
            }
        }


        static DbCacheAgent _DbCache;
        /// <summary>
        /// Get <see cref="DbCache"/> as Singleton.
        /// </summary>
        internal static DbCacheAgent DbCache
        {
            get
            {
                if (_DbCache == null)
                {
                    _DbCache = new DbCacheAgent("DbCache");
                }
                return _DbCache;
            }
        }


        static SyncCacheAgent _SyncCache;
        /// <summary>
        /// Get <see cref="SyncCache"/> as Singleton.
        /// </summary>
        internal static SyncCacheAgent SyncCache
        {
            get
            {
                if (_SyncCache == null)
                {
                    _SyncCache = new SyncCacheAgent();
                }
                return _SyncCache;
            }
        }


        #endregion


        /// <summary>
        /// Get Cache prformance report <see cref="CachePerformanceReport"/>
        /// </summary>
        /// <returns></returns>
        public static CachePerformanceReport PerformanceReport()
        {
            CachePerformanceReport report = new Caching.CachePerformanceReport();
            report.InitReport();

            if (_Cache != null)
                report.AddItemReport(Cache.PerformanceCounter);
            if (_DbCache != null)
                report.AddItemReport(DbCache.PerformanceCounter);
            if (_SyncCache != null)
                report.AddItemReport(SyncCache.PerformanceCounter);
            if (_Session != null)
                report.AddItemReport(Session.PerformanceCounter);

            report.AddTotalReport();

            return report;
        }
        /// <summary>
        /// Get Cache prformance State Counter report.
        /// </summary>
        /// <returns></returns>
        public static DataTable CacheStateCounter()
        {
            CacheStateCounterReport report = new Caching.CacheStateCounterReport();
            if (_Cache != null)
                report.AddItemReport(Cache.PerformanceCounter);
            if (_DbCache != null)
                report.AddItemReport(DbCache.PerformanceCounter);
            if (_SyncCache != null)
                report.AddItemReport(SyncCache.PerformanceCounter);
            if (_Session != null)
                report.AddItemReport(Session.PerformanceCounter);

            return report.StateReport;
        }

        /// <summary>
        /// Get Cache prformance State Counter report.
        /// </summary>
        /// <returns></returns>
        public static DataTable CacheStateCounter(CacheAgentType agentType)
        {
            CacheStateCounterReport report = new Caching.CacheStateCounterReport();
            switch (agentType)
            {
                case CacheAgentType.Cache:
                    if (_Cache != null)
                        report.AddItemReport(Cache.PerformanceCounter); break;
                case CacheAgentType.SyncCache:
                    if (_Cache != null)
                        report.AddItemReport(SyncCache.PerformanceCounter); break;
                case CacheAgentType.SessionCache:
                    if (_Cache != null)
                        report.AddItemReport(Session.PerformanceCounter); break;
                case CacheAgentType.DataCache:
                    if (_Cache != null)
                        report.AddItemReport(DbCache.PerformanceCounter); break;

            }
            return report.StateReport;
        }

        /// <summary>
        /// Reset Cache prformance counter.
        /// </summary>
        /// <returns></returns>
        public static void ResetPerformanceCounter()
        {
            CachePerformanceReport report = new Caching.CachePerformanceReport();
            if (_Cache != null)
                report.ResetCounter(Cache.PerformanceCounter);
            if (_DbCache != null)
                report.ResetCounter(DbCache.PerformanceCounter);
            if (_SyncCache != null)
                report.ResetCounter(SyncCache.PerformanceCounter);
            if (_Session != null)
                report.ResetCounter(Session.PerformanceCounter);
            report.InitReport();
        }


        /// <summary>
        /// Get Cache prformance report <see cref="CachePerformanceReport"/> using <see cref="CacheAgentType"/>.
        /// </summary>
        /// <param name="agentType"></param>
        /// <returns></returns>
        public static CachePerformanceReport PerformanceReport(CacheAgentType agentType)
        {
            CachePerformanceReport report = new Caching.CachePerformanceReport(agentType);
            switch (agentType)
            {
                case CacheAgentType.Cache:
                    if (_Cache != null)
                        report.AddItemReport(Cache.PerformanceCounter);
                    break;
                case CacheAgentType.DataCache:
                    if (_DbCache != null)
                        report.AddItemReport(DbCache.PerformanceCounter);
                    break;
                case CacheAgentType.SyncCache:
                    if (_SyncCache != null)
                        report.AddItemReport(SyncCache.PerformanceCounter);
                    break;
                case CacheAgentType.SessionCache:
                    if (_Session != null)
                        report.AddItemReport(Session.PerformanceCounter);
                    break;
            }

            return report;
        }

        static Nistec.Threading.AsyncTasker _Tasker;
        /// <summary>
        /// Get <see cref="AsyncTasker"/> as Singleton.
        /// </summary>
        public static AsyncTasker Tasker
        {
            get
            {
                if (_Tasker == null)
                {
                    _Tasker = new Threading.AsyncTasker(100, 1000);
                   
                    _Tasker.Start();
                }
                return _Tasker;
            }
        }

        static Nistec.Threading.TaskerQueue _PerformanceTasker;
        /// <summary>
        /// Get <see cref="AsyncTasker"/> as Singleton.
        /// </summary>
        public static TaskerQueue PerformanceTasker
        {
            get
            {
                if (_PerformanceTasker == null)
                {
                    _PerformanceTasker = new Threading.TaskerQueue(100, 100);

                    _PerformanceTasker.Start();
                }
                return _PerformanceTasker;
            }
        }

        internal static void OnTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            string message = string.Format("OnTaskCompleted: {0}, state: {1}", e.Args.Key, e.Args.State.ToString());

            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.None, message);

            Console.WriteLine(message);
        }

        /// <summary>
        /// Execute remote command from client to cache managment using <see cref="CacheMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static NetStream ExecManager(CacheMessage message)
        {
            CacheState state = CacheState.Ok;
            try
            {
                NetStream stream = null;
                switch (message.Command)
                {
                    case CacheManagerCmd.Reply:
                        return CacheEntry.GetAckStream(CacheState.Ok, CacheManagerCmd.Reply, message.Key);
                    case CacheManagerCmd.CacheProperties:
                        if (_Cache == null)
                            return null;
                        return message.AsyncTask(() => Cache.PerformanceCounter.GetPerformanceProperties(), message.Command);


                    case CacheManagerCmd.CloneItems:
                        if (_Cache == null)
                            return null;
                        var args = message.GetArgs();
                        CloneType ct = EnumExtension.Parse<CloneType>(args.Get<string>("value"), CloneType.All);
                        return message.AsyncTask(() => Cache.CloneItems(ct), message.Command);

                    case CacheManagerCmd.GetAllKeys:
                        if (_Cache == null)
                            return null;
                        return message.AsyncTask(() => Cache.GetAllKeys(), message.Command);
                    case CacheManagerCmd.GetAllKeysIcons:
                        if (_Cache == null)
                            return null;
                        return message.AsyncTask(() => Cache.GetAllKeysIcons(), message.Command);
                    case CacheManagerCmd.StateCounterCache:
                        return message.AsyncTask(() => CacheStateCounter(CacheAgentType.Cache), message.Command);
                    case CacheManagerCmd.StateCounterSync:
                        return message.AsyncTask(() => CacheStateCounter(CacheAgentType.SyncCache), message.Command);
                    case CacheManagerCmd.StateCounterSession:
                        return message.AsyncTask(() => CacheStateCounter(CacheAgentType.SessionCache), message.Command);
                    case CacheManagerCmd.StateCounterDataCache:
                        return message.AsyncTask(() => CacheStateCounter(CacheAgentType.DataCache), message.Command);
                    case CacheManagerCmd.GetStateCounterReport:
                        return message.AsyncTask(() => CacheStateCounter(), message.Command);
                    case CacheManagerCmd.GetPerformanceReport:
                        return message.AsyncTask(() => PerformanceReport(), message.Command);
                    case CacheManagerCmd.GetAgentPerformanceReport:
                        CacheAgentType agentType = CachePerformanceCounter.GetAgent(message.Key);
                        return message.AsyncTask(() => PerformanceReport(agentType), message.Command);
                    case CacheManagerCmd.ResetPerformanceCounter:
                        message.AsyncTask(() => ResetPerformanceCounter());
                        return null;
                    case CacheManagerCmd.GetAllDataKeys:
                        if (_DbCache == null)
                            return null;
                        return message.AsyncTask(() => DbCache.GetAllDataKeys(), message.Command);
                    case CacheManagerCmd.GetAllSyncCacheKeys:
                        if (_SyncCache == null)
                            return null;
                        return message.AsyncTask(() => SyncCache.CacheKeys().ToArray(), message.Command);
                    case CacheManagerCmd.CacheLog:
                        return message.AsyncTask(() => CacheLogger.Logger.CacheLog(), message.Command);
                    case CacheManagerCmd.GetAllSessionsKeys:
                        if (_Session == null)
                            return null;
                        return message.AsyncTask(() => Session.GetAllSessionsKeys(), message.Command);
                    case CacheManagerCmd.GetAllSessionsStateKeys:
                        if (_Session == null)
                            return null;
                        stream = new NetStream();
                        SessionState st = (SessionState)message.GetArgs().Get<int>("state");
                        return message.AsyncTask(() => Session.GetAllSessionsStateKeys(st), message.Command);
                    case CacheManagerCmd.GetSessionItemsKeys:
                        if (_Session == null)
                            return null;
                        return message.AsyncTask(() => Session.GetSessionsItemsKeys(message.Id), message.Command);

                    //=== Cache api===================================================
                    case CacheCmd.ViewItem:
                    case CacheCmd.RemoveItem:
                        return Cache.ExecRemote(message);

                    //=== Data Cache api===================================================
                    case DataCacheCmd.GetItemProperties:
                    case DataCacheCmd.RemoveTable:
                    //case DataCacheCmd.GetDataStatistic:
                    case DataCacheCmd.GetDataTable:

                        return DbCache.ExecRemote(message);

                    //=== Sync Cache api===================================================

                    case SyncCacheCmd.RemoveSyncItem:
                    case SyncCacheCmd.GetSyncItem:
                    //case SyncCacheCmd.GetSyncStatistic:
                    case SyncCacheCmd.GetItemsReport:

                        return SyncCache.ExecRemote(message);

                    //=== Session Cache api===================================================

                    case SessionCmd.RemoveSession:
                    case SessionCmd.GetExistingSession:

                        return Session.ExecRemote(message);


                }
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = CacheState.SerializationError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "ExecManager error: " + se.Message);
            }
            catch (Exception ex)
            {
                state = CacheState.UnexpectedError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "ExecManager error: " + ex.Message);
            }

            return CacheEntry.GetAckStream(state, message.Command); //null;
        }


        internal static NetStream ExecCommand(CacheMessage message)
        {

            switch (message.Command)
            {
                case CacheCmd.Reply:
                case CacheCmd.AddItem:
                case CacheCmd.GetValue:
                case CacheCmd.FetchValue:
                case CacheCmd.GetItem:
                case CacheCmd.FetchItem:
                case CacheCmd.ViewItem:
                case CacheCmd.RemoveItem:
                case CacheCmd.RemoveItemAsync:
                case CacheCmd.CopyItem:
                case CacheCmd.CutItem:
                case CacheCmd.KeepAliveItem:
                case CacheCmd.RemoveCacheSessionItems:
                case CacheCmd.LoadData:
                    return AgentManager.Cache.ExecRemote(message);

                case SyncCacheCmd.Reply:
                case SyncCacheCmd.GetSyncItem:
                case SyncCacheCmd.GetRecord:
                case SyncCacheCmd.GetEntity:
                case SyncCacheCmd.GetAs:
                case SyncCacheCmd.Refresh:
                case SyncCacheCmd.Reset:
                case SyncCacheCmd.RefreshItem:
                case SyncCacheCmd.Contains:
                case SyncCacheCmd.AddSyncItem:
                case SyncCacheCmd.RemoveSyncItem:
                case SyncCacheCmd.AddSyncEntity:
                case SyncCacheCmd.GetEntityItems:
                case SyncCacheCmd.GetEntityKeys:
                case SyncCacheCmd.GetAllEntityNames:
                case SyncCacheCmd.GetItemsReport:
                    return AgentManager.SyncCache.ExecRemote(message);

                case SessionCmd.Reply:
                case SessionCmd.AddSession:
                case SessionCmd.RemoveSession:
                case SessionCmd.ClearSessionItems:
                case SessionCmd.ClearAllSessions:
                case SessionCmd.GetOrCreateSession:
                case SessionCmd.GetExistingSession:
                case SessionCmd.SessionRefresh:
                case SessionCmd.RefreshOrCreate:
                case SessionCmd.RemoveSessionItem:
                case SessionCmd.AddItemExisting:
                case SessionCmd.AddSessionItem:
                case SessionCmd.GetSessionItem:
                case SessionCmd.FetchSessionItem:
                case SessionCmd.CopyTo:
                case SessionCmd.FetchTo:
                case SessionCmd.Exists:
                case SessionCmd.GetAllSessionsKeys:
                case SessionCmd.GetAllSessionsStateKeys:
                case SessionCmd.GetSessionItemsKeys:
                    return AgentManager.Session.ExecRemote(message);

                case DataCacheCmd.SetValue:
                case DataCacheCmd.GetDataValue:
                case DataCacheCmd.GetRow:
                case DataCacheCmd.GetDataTable:
                case DataCacheCmd.RemoveTable:
                case DataCacheCmd.GetItemProperties:
                //case DataCacheCmd.GetDataStatistic:
                case DataCacheCmd.AddDataItem:
                case DataCacheCmd.AddDataItemSync:
                case DataCacheCmd.AddSyncDataItem:
                    return AgentManager.DbCache.ExecRemote(message);
                default:
                    CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "AgentManager.ExecCommand error: Command not supported " + message.Command);
                    return CacheEntry.GetAckStream(CacheState.CommandNotSupported, message.Command);

            }
        }
    }
}
