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
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Data;

using Nistec.Drawing;
using Nistec.Data;
using Nistec.Data.Entities;
using Nistec.Collections;
using Nistec.Threading;
using Nistec.Caching;
using Nistec.Caching.Sync;
using Nistec.Data.Entities.Cache;
using Nistec.IO;
using Nistec.Caching.Remote;
using System.Threading.Tasks;
using Nistec.Caching.Config;
using Nistec.Caching.Sync.Remote;
using Nistec.Channels;
using Nistec.Runtime;

namespace Nistec.Caching.Server
{
    /// <summary>
    /// Represent <see cref="SyncCacheStream"/> as server agent.
    /// </summary>
    [Serializable]
    public class SyncCacheAgent : SyncCacheStream//, ICachePerformance
    {
        #region ICachePerformance
        /*
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
            CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.None, "Memory Size Exchange: " + CacheName);
            long size = GetAllSyncSize();
            Interlocked.Exchange(ref memorySize, size);
        }

        internal long MaxSize { get { return CacheDefaults.DefaultCacheMaxSize; } }

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
        */
        #endregion

        #region size exchange
        /*
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
        internal protected override void SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;
            PerformanceCounter.ExchangeSizeAndCountAsync(currentSize, newSize, currentCount, newCount, exchange, CacheSettings.EnableSizeHandler);

        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected override void SizeExchage(ISyncItemStream oldItem, ISyncItemStream newItem)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return;// CacheState.Ok;

            long oldSize = 0;
            int oldCount = 0;
            long newSize = 0;
            int newCount = 0;

            if (oldItem != null)
            {
                oldSize = oldItem.Size;
                oldCount = oldItem.Count;
            }
            if (newItem != null)
            {
                newSize = newItem.Size;
                newCount = newItem.Count;
            }
            PerformanceCounter.ExchangeSizeAndCountAsync(oldSize, newSize, oldCount, newCount, false, CacheSettings.EnableSizeHandler);
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
        */
        #endregion

        #region ctor

        /// <summary>
        /// Initialize a new instance of SyncCacheAgent
        /// </summary>
        public SyncCacheAgent()
            : base("SyncCacheAgent")
        {
            //m_Perform = new CachePerformanceCounter(this, CacheAgentType.SyncCache, CacheName);
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "SyncCacheAgent Initilaized!");
        }

        /// <summary>
        /// Reply for test.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Reply(string text)
        {
            return text;
        }
        /// <summary>
        /// Reset sync cache.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            CacheLogger.Logger.Clear();
        }

        #endregion

        /// <summary>
        /// Execute remote command from client to sync cache using <see cref="CacheMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public TransStream ExecRemote(CacheMessage message)
        {
            if(!this.Initialized)
            {
                return TransStream.WriteState(-1, message.Command + ": " + CacheState.CacheNotReady.ToString());//,  TransType.Error);
            }

            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;
            try
            {
                
                switch (message.Command.ToLower())
                {
                    case SyncCacheCmd.Reply:
                        return TransStream.Write("Reply: " + message.Identifier, TransType.Text);
                    case SyncCacheCmd.Get:
                        return AsyncTransObject(() => Get(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SyncCacheCmd.GetRecord:
                        return AsyncTransStream(() => GetRecord(message),message.Command,requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SyncCacheCmd.GetEntity:
                        return AsyncTransStream(() => GetEntity(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SyncCacheCmd.GetAs:
                        return AsyncTransStream(() => GetAs(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, TransType.Stream);
                    case SyncCacheCmd.RefreshAll:
                        return AsyncTransState(() => RefreshAll(), requestTime, CacheState.Ok, CacheState.UnKnown);
                    case SyncCacheCmd.Reset:
                        return AsyncTransState(() => Reset(), requestTime, CacheState.Ok, CacheState.UnKnown);
                    case SyncCacheCmd.Refresh:
                        return AsyncTransState(() => Refresh(message.Identifier), requestTime, CacheState.Ok, CacheState.UnKnown);
                    case SyncCacheCmd.Contains:
                        return AsyncTransState(() => Contains(ComplexArgs.Get(message.Identifier, message.Label)), requestTime, CacheState.Ok, CacheState.NotFound);
                    case SyncCacheCmd.AddSyncItem:
                         return AsyncTransState(() => AddItem(message), requestTime, CacheState.AddItemFailed);
                    case SyncCacheCmd.Remove:
                        return AsyncTransState(() => RemoveItem(message.Label), requestTime, CacheState.ItemRemoved, CacheState.RemoveItemFailed);
                    case SyncCacheCmd.AddEntity:
                        return AsyncTransState(() => AddItem(message.DecodeBody<SyncEntity>()), requestTime, CacheState.AddItemFailed);
                    case SyncCacheCmd.GetItemProperties:
                            return AsyncTransObject(() => GetItemProperties(message.Label), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SyncCacheCmd.GetEntityItems:
                            return AsyncTransObject(() => GetEntityItems(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SyncCacheCmd.GetEntityKeys:
                        return AsyncTransObject(() => GetEntityKeys(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());//,message.TransformType);
                    case SyncCacheCmd.GetAllEntityNames:
                            return AsyncTransObject(() => GetNames(), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SyncCacheCmd.GetItemsReport:
                            return AsyncTransObject(() => GetItemsReport(message), message.Command, requestTime, CacheState.Ok,CacheState.UnexpectedError, message.TransformType.ToTransType());
                    case SyncCacheCmd.GetEntityItemsCount:
                            return AsyncTransObject(() => GetEntityItemsCount(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SyncCacheCmd.GetEntityPrimaryKey:
                        return AsyncTransObject(() => GetEntityPrimaryKey(message.Label), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SyncCacheCmd.FindEntity:
                        return AsyncTransStream(() => FindEntity(message.Label, message.Args), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    default:
                        state = CacheState.CommandNotSupported;
                        return TransStream.WriteState((int)state, message.Command + ": " + state.ToString());//, CacheUtil.ToTransType(state));
                }
            }
            catch (CacheException ce)
            {
                state = ce.State;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "SyncCacheAgent.ExecRemote CacheException error: " + ce.Message);
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = CacheState.SerializationError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "SyncCacheAgent.ExecRemote SerializationException error: " + se.Message);
            }
            catch (ArgumentException aex)
            {
                state = CacheState.ArgumentsError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "SyncCacheAgent.ExecRemote ArgumentException: " + aex.Message);
            }
            catch (Exception ex)
            {
                state = CacheState.UnexpectedError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "SyncCacheAgent.ExecRemote error: " + ex.Message);
            }
            SendState(requestTime, state);
            return TransStream.WriteState((int)state ,message.Command + ": " + state.ToString());//, CacheUtil.ToTransType(state));

        }

        #region Async Task

        public void SendState(DateTime requestTime, CacheState state)
        {
            if (CacheSettings.EnablePerformanceCounter)
            {
                PerformanceCounter.AddResponseAsync(requestTime, state, true);
            }
        }
        public TransStream AsyncTransStream(Func<NetStream> action, string command, DateTime requestTime, CacheState successState, CacheState failedState = CacheState.NotFound, TransType transType= TransType.Object )
        {
            Task<NetStream> task = Task.Factory.StartNew<NetStream>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result != null)
                    {
                        SendState(requestTime, successState);
                        return TransStream.Write(task.Result, transType);// TransStream.ToTransType(transform));
                    }
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.WriteState(-1 ,command + ": " + failedState.ToString());//, TransType.Error);
        }
        public TransStream AsyncTransObject(Func<object> action, string command, DateTime requestTime, CacheState successState, CacheState failedState = CacheState.NotFound, TransType transType = TransType.Object)
        {
            Task<object> task = Task.Factory.StartNew<object>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result != null)
                    {
                        SendState(requestTime, successState);
                        return TransStream.Write(task.Result, transType);// TransStream.ToTransType(transform));
                    }
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.WriteState(-1, command + ": " + failedState.ToString());//, TransType.Error);
        }

        public TransStream AsyncTransState(Func<CacheState> action, DateTime requestTime, CacheState failedState = CacheState.NotFound)
        {
            Task<CacheState> task = Task.Factory.StartNew<CacheState>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    SendState(requestTime, task.Result);
                    return TransStream.WriteState((int)task.Result, task.Result.ToString());//, TransType.State);
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.WriteState((int)failedState, failedState.ToString());//TransType.State);
        }


        public TransStream AsyncTransState(Func<bool> action, DateTime requestTime, CacheState successState, CacheState failedState)
        {
            Task<bool> task = Task.Factory.StartNew<bool>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    CacheState state = task.Result ? successState : failedState;
                    SendState(requestTime, state);
                    return TransStream.WriteState((int)state, state.ToString());//TransType.State);
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.WriteState((int)failedState, failedState.ToString());//TransType.State);
        }

        public TransStream AsyncTransState(Action action, DateTime requestTime, CacheState successState, CacheState failedState = CacheState.UnKnown)
        {
            Task task = Task.Factory.StartNew(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    SendState(requestTime, successState);
                    return TransStream.WriteState((int)successState, successState.ToString());//TransType.State);
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.WriteState((int)failedState, failedState.ToString());//TransType.State);
        }

        #endregion

    }
}


