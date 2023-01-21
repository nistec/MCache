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
using Nistec.Channels;
using System.Threading.Tasks;
using Nistec.Data.Ado;

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


        static DataCacheAgent _DbCache;
        /// <summary>
        /// Get <see cref="DbCache"/> as Singleton.
        /// </summary>
        internal static DataCacheAgent DbCache
        {
            get
            {
                if (_DbCache == null)
                {
                    _DbCache = new DataCacheAgent("DbCache");
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

        static ConnectionSettings _Connections;
        public static ConnectionSettings Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = ConnectionSettings.Instance;
                    //_Connections.Load();
                }
                return _Connections;
            }
        }

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
                    if (_SyncCache != null)
                        report.AddItemReport(SyncCache.PerformanceCounter); break;
                case CacheAgentType.SessionCache:
                    if (_Session != null)
                        report.AddItemReport(Session.PerformanceCounter); break;
                case CacheAgentType.DataCache:
                    if (_DbCache != null)
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
                    _Tasker = new Threading.AsyncTasker(false, true, 300, 3000);
                   
                    _Tasker.Start();
                   //~Console.WriteLine("Debuger-AgentManager.Tasker satart...");
                }
                return _Tasker;
            }
        }

        static AsyncTasker _PerformanceTasker;
        /// <summary>
        /// Get <see cref="AsyncTasker"/> as Singleton.
        /// </summary>
        public static AsyncTasker PerformanceTasker
        {
            get
            {
                if (_PerformanceTasker == null)
                {
                    _PerformanceTasker = new AsyncTasker(false,false,10,5000);

                    _PerformanceTasker.Start();
                   //~Console.WriteLine("Debuger-AgentManager.PerformanceTasker satart...");
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
        internal static TransStream ExecManager(MessageStream message)
        {
            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;
            try
            {
                //NetStream stream = null;

                if (message == null || message.Command == null)
                {
                    throw new ArgumentNullException("ExecManager.message");
                }

                if (!message.Command.StartsWith("mang_"))
                    return ExecCommand(message);
                
                switch (message.Command.ToLower())
                {
                    case CacheManagerCmd.Reply:
                        return TransStream.Write("Reply: " + message.Id, TransType.Text);
                    case CacheManagerCmd.CacheProperties:
                        if (_Cache == null)
                            return null;
                        return AsyncTransObject(() => Cache.PerformanceCounter.GetPerformanceProperties(), message.Command);

                    case CacheManagerCmd.ReportCacheItems:
                        return AsyncTransObject(() => Cache.GetReport(), message.Command);
                    case CacheManagerCmd.ReportSessionItems:
                        return AsyncTransObject(() => Session.GetReport(), message.Command);
                    case CacheManagerCmd.ReportDataTimer:
                        return AsyncTransObject(() => DbCache.GetTimerReport(), message.Command);

                    case CacheManagerCmd.ReportCacheTimer:
                        return AsyncTransObject(() => Cache.GetTimerReport(), message.Command);
                    case CacheManagerCmd.ReportSessionTimer:
                        return AsyncTransObject(() => Session.GetTimerReport(), message.Command);
                    case CacheManagerCmd.ReportSyncBoxItems:
                        return AsyncTransObject(() => SyncBox.Instance.GetBoxReport(), message.Command);
                    case CacheManagerCmd.ReportSyncBoxQueue:
                        return AsyncTransObject(() => SyncBox.Instance.GetQueueReport(), message.Command);
                    case CacheManagerCmd.ReportTimerSyncDispatcher:
                        return AsyncTransObject(() => TimerSyncDispatcher.Instance.GetReport(), message.Command);


                    case CacheManagerCmd.CloneItems:
                        if (_Cache == null)
                            return null;
                        var args = message.GetArgs();
                        CloneType ct = EnumExtension.Parse<CloneType>(args.Get<string>("value"), CloneType.All);
                        return AsyncTransObject(() => Cache.CloneItems(ct), message.Command);

                    case CacheManagerCmd.GetAllKeys:
                        if (_Cache == null)
                            return null;
                        return AsyncTransObject(() => Cache.GetAllKeys(), message.Command);
                    case CacheManagerCmd.GetAllKeysIcons:
                        if (_Cache == null)
                            return null;
                        return AsyncTransObject(() => Cache.GetAllKeysIcons(), message.Command);
                    case CacheManagerCmd.StateCounterCache:
                        return AsyncTransObject(() => CacheStateCounter(CacheAgentType.Cache), message.Command);
                    case CacheManagerCmd.StateCounterSync:
                        return AsyncTransObject(() => CacheStateCounter(CacheAgentType.SyncCache), message.Command);
                    case CacheManagerCmd.StateCounterSession:
                        return AsyncTransObject(() => CacheStateCounter(CacheAgentType.SessionCache), message.Command);
                    case CacheManagerCmd.StateCounterDataCache:
                        return AsyncTransObject(() => CacheStateCounter(CacheAgentType.DataCache), message.Command);
                    case CacheManagerCmd.GetStateCounterReport:
                        return AsyncTransObject(() => CacheStateCounter(), message.Command);
                    case CacheManagerCmd.GetPerformanceReport:
                        return AsyncTransObject(() => PerformanceReport(), message.Command);
                    case CacheManagerCmd.GetAgentPerformanceReport:
                        CacheAgentType agentType = CachePerformanceCounter.GetAgent(message.Id);
                        return AsyncTransObject(() => PerformanceReport(agentType), message.Command);
                    case CacheManagerCmd.ResetPerformanceCounter:
                        message.AsyncTask(() => ResetPerformanceCounter());
                        return null;
                    case CacheManagerCmd.GetAllDataKeys:
                        if (_DbCache == null)
                            return null;
                        return AsyncTransObject(() => DbCache.GetAllDataKeys(), message.Command);
                    case CacheManagerCmd.GetAllSyncCacheKeys:
                        if (_SyncCache == null)
                            return null;
                        return AsyncTransObject(() => SyncCache.CacheKeys().ToArray(), message.Command);
                    case CacheManagerCmd.CacheLog:
                        return AsyncTransObject(() => CacheLogger.Logger.CacheLog(), message.Command);
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

            return TransStream.WriteState((int)state, message.Command + ", " + state.ToString());//, CacheUtil.ToTransType(state));
        }
        //TOD:~
        internal static TransStream ExecCommand(MessageStream message)
        {
            if(message==null || message.Command==null)
            {
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "AgentManager.ExecCommand error: Message is null or Command not supported!");
                return TransStream.WriteState(-1,"Unknown message or command");//,  TransType.Error); 
            }

            string CommandType = message.Command.Substring(0, 5);

            switch (CommandType)
            {
                case "cach_":
                    return AgentManager.Cache.ExecRemote(message);
                case "sync_":
                    return AgentManager.SyncCache.ExecRemote(message);
                case "sess_":
                    return AgentManager.Session.ExecRemote(message);
                case "data_":
                    return AgentManager.DbCache.ExecRemote(message);
                case "mang_":
                    return ExecManager(message);
                default:
                    CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "AgentManager.ExecCommand error: Command not supported " + message.Command);
                    return TransStream.WriteState(-1, "CommandNotSupported");//, , TransType.Error);
            }
        }

        #region Async Task

        internal static TransStream AsyncTransStream(Func<NetStream> action, string command, CacheState successState= CacheState.Ok, CacheState failedState = CacheState.NotFound, TransType transType = TransType.Object)//TransformType transform = TransformType.Message)
        {
            Task<NetStream> task = Task.Factory.StartNew<NetStream>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result != null)
                    {
                        //SendState(requestTime, successState);
                        return TransStream.Write(task.Result, transType);
                    }
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.WriteState(-1, command + ": " + failedState.ToString());//, , TransType.Error);
        }
        internal static TransStream AsyncTransObject(Func<object> action, string command, CacheState successState= CacheState.Ok, CacheState failedState = CacheState.NotFound, TransType transType = TransType.Object)//TransformType transform = TransformType.Message)
        {
            Task<object> task = Task.Factory.StartNew<object>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result != null)
                    {
                        //SendState(requestTime, successState);
                        return TransStream.Write(task.Result, transType);// TransStream.ToTransType(transform));
                    }
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.WriteState(-1, command + ": " + failedState.ToString());//, TransType.Error);
        }

        internal static TransStream AsyncTransState(Func<CacheState> action, CacheState failedState = CacheState.NotFound)
        {
            Task<CacheState> task = Task.Factory.StartNew<CacheState>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    //SendState(requestTime, task.Result);
                    return TransStream.WriteState((int)task.Result, failedState.ToString());// TransType.State);
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.WriteState((int)failedState, failedState.ToString());// TransType.State);
        }


        internal static TransStream AsyncTransState(Func<bool> action, CacheState successState, CacheState failedState)
        {
            Task<bool> task = Task.Factory.StartNew<bool>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    CacheState state = task.Result ? successState : failedState;
                    //SendState(requestTime, state);
                    return TransStream.WriteState((int)state, failedState.ToString());//TransType.State);
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.WriteState((int)failedState, failedState.ToString());//TransType.State);
        }

        internal static TransStream AsyncTransState(Action action, CacheState successState= CacheState.Ok, CacheState failedState = CacheState.UnKnown)
        {
            Task task = Task.Factory.StartNew(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    //SendState(requestTime, successState);
                    return TransStream.WriteState((int)successState, successState.ToString());//TransType.State);
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.WriteState((int)failedState, failedState.ToString());// TransType.State);
        }

        #endregion


    }
}
