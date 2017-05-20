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
using System.Threading;
using System.Collections;
using System.Linq;
using Nistec.Caching.Remote;
using Nistec.Generic;
using System.Threading.Tasks;
using Nistec.Data.Entities;
using Nistec.Runtime;
using Nistec.IO;
using Nistec.Caching.Session;
using Nistec.Caching.Config;

namespace Nistec.Caching.Server
{

    /// <summary>
    /// Represent <see cref="SessionCache"/> as server agent.
    /// </summary>
    public class SessionAgent : SessionCache, ICachePerformance
    {

        #region ICachePerformance

        CachePerformanceCounter m_Perform;
        /// <summary>
        /// Get <see cref="CachePerformanceCounter"/> Performance Counter.
        /// </summary>
        public CachePerformanceCounter PerformanceCounter
        {
            get { return m_Perform; }
        }

        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void ICachePerformance.MemorySizeExchange(ref long memorySize)
        {
            CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.None, "Memory Size Exchange: SessionCache");
            long size = GetSessionsSize();
            Interlocked.Exchange(ref memorySize, size);
        }

        internal long MaxSize { get { return 999999999L; } }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long ICachePerformance.GetMaxSize()
        {
            return MaxSize;
        }
        bool ICachePerformance.IsRemote
        {
            get { return true; }
        }
        int ICachePerformance.IntervalSeconds
        {
            get { return base.IntervalSeconds; }
        }
        bool ICachePerformance.Initialized
        {
            get { return base.Initialized; }
        }
        #endregion

        #region size exchange

        /// <summary>
        /// Validate if the new size is not exceeds the CacheMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected override CacheState SizeValidate(long newSize)
        {
            if (!CacheSettings.EnableSizeHandler)
                return CacheState.Ok;
            return PerformanceCounter.SizeValidate(newSize);
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="newSize"></param>
        /// <param name="currentCount"></param>
        /// <param name="newCount"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        internal protected override CacheState SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return CacheState.Ok;
            return PerformanceCounter.ExchangeSizeAndCount(currentCount,newSize, currentCount,newCount,exchange,  CacheSettings.EnableSizeHandler);
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected override void SizeRefresh()
        {
            if (CacheSettings.EnablePerformanceCounter)
            {
                PerformanceCounter.RefreshSize();
            }

        }

        #endregion

        #region ctor
        /// <summary>
        /// Initialize a new instance of SessionAgent
        /// </summary>
        public SessionAgent():base()
        {
            m_Perform = new CachePerformanceCounter(this,CacheAgentType.SessionCache, "SessionAgent");
        }

        #endregion
                
        #region Remote methods
        /// <summary>
        /// Execute remote command from client to session cache using <see cref="CacheMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream ExecRemote(CacheMessage message)
        {
            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;
            try
            {
                IKeyValue args = null;

                switch (message.Command)
                {
                    case SessionCmd.Reply:
                        return CacheEntry.GetAckStream(CacheState.Ok, SessionCmd.Reply, message.Key);
                    case SessionCmd.AddSession:
                        args= message.GetArgs();
                        message.AsyncTask(() => AddSession(message.Id, args.Get<string>(KnowsArgs.UserId), message.Expiration, args.Get<string>(KnowsArgs.StrArgs)));
                        break;
                    case SessionCmd.RemoveSession:
                        return message.AsyncTask(() => Remove(message.Id),message.Command);
                    case SessionCmd.ClearSessionItems:
                        message.AsyncTask(() => ClearItems(message.Id));
                        break;
                    case SessionCmd.ClearAllSessions:
                        message.AsyncTask(() => Clear());
                        break;
                    case SessionCmd.GetOrCreateSession:
                        return message.AsyncTask(() => GetOrCreateStream(message.Id), message.Command);
                    case SessionCmd.GetExistingSession:
                        return message.AsyncTask(() => GetExistingBagStream(message.Id), message.Command);
                    case SessionCmd.SessionRefresh:
                        message.AsyncTask(() => Refresh(message.Id));
                        break;
                    case SessionCmd.RefreshOrCreate:
                        message.AsyncTask(() => RefreshOrCreate(message.Id));
                        break;
                    case SessionCmd.RemoveSessionItem:
                        return message.AsyncTask(() => RemoveItem(message.Id, message.Key), message.Command);
                    case SessionCmd.AddItemExisting:
                        message.AsyncTask(() => AddItem(new SessionEntry(message), true));
                        break;
                    case SessionCmd.AddSessionItem:
                        message.AsyncTask(() => AddItem(new SessionEntry(message), false));
                        break;
                    case SessionCmd.GetSessionItem:
                        return message.AsyncTask(() => GetItem(message.Id, message.Key), message.Command);
                    case SessionCmd.FetchSessionItem:
                        return message.AsyncTask(() => FetchItem(message.Id, message.Key), message.Command);
                    case SessionCmd.CopyTo:
                        {
                             args = message.GetArgs();
                             return message.AsyncTask(() => CopyTo(message.Id, message.Key, args.Get<string>(KnowsArgs.TargetKey), message.Expiration, args.Get<bool>(KnowsArgs.AddToCache)), message.Command);
                        }
                    case SessionCmd.FetchTo:
                        {
                            args = message.GetArgs();
                            return message.AsyncTask(() => FetchTo(message.Id, message.Key, args.Get<string>(KnowsArgs.TargetKey), message.Expiration, args.Get<bool>(KnowsArgs.AddToCache)), message.Command);
                        }
                    case SessionCmd.Exists:
                        return message.AsyncTask(() => Exists(message.Id, message.Key), message.Command);
                    case SessionCmd.GetAllSessionsKeys:
                        return message.AsyncTask(() => GetAllSessionsKeys(), message.Command);
                    case SessionCmd.GetAllSessionsStateKeys:
                        {
                            SessionState st = (SessionState) EnumExtension.Parse<SessionState>(message.Key, SessionState.Active);//.GetArgs().Get<int>("state");
                            return message.AsyncTask(() => GetAllSessionsStateKeys(st), message.Command);
                        }
                    case SessionCmd.GetSessionItemsKeys:
                        return message.AsyncTask(() => GetSessionsItemsKeys(message.Id), message.Command);
                }
                
            }
            catch (CacheException ce)
            {
                state = ce.State;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "SessionAgent.ExecRemote CacheException error: " + ce.Message);
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = CacheState.SerializationError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "SessionAgent.ExecRemote SerializationException error: " + se.Message);
            }
            catch (ArgumentException aex)
            {
                state = CacheState.ArgumentsError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "SessionAgent.ExecRemote ArgumentException: " + aex.Message);
            }
            catch (Exception ex)
            {
                state = CacheState.UnexpectedError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "SessionAgent.ExecRemote error: " + ex.Message);
            }
            finally
            {
                if (CacheSettings.EnablePerformanceCounter)
                {
                    if (CacheSettings.EnableAsyncTask)
                        AgentManager.PerformanceTasker.Add(new Nistec.Threading.TaskItem(() => PerformanceCounter.AddResponse(requestTime, state, true), CacheDefaults.DefaultTaskTimeout));
                    else
                        Task.Factory.StartNew(() => PerformanceCounter.AddResponse(requestTime, state, true));
                }

                //if (CacheSettings.EnablePerformanceCounter)
                //    Task.Factory.StartNew(() => PerformanceCounter.AddResponse(requestTime, state, true));
            }
            return CacheEntry.GetAckStream(state, message.Command); //null;

        }
       
        #endregion
       
    }

}
