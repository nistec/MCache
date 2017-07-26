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

namespace Nistec.Caching.Server
{
    /// <summary>
    /// Represent <see cref="SyncCacheStream"/> as server agent.
    /// </summary>
    [Serializable]
    public class SyncCacheAgent : SyncCacheStream, ICachePerformance
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
            return PerformanceCounter.ExchangeSizeAndCount(currentSize, newSize, currentCount, newCount, exchange, CacheSettings.EnableSizeHandler);

        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        internal protected override CacheState SizeExchage(ISyncItemStream oldItem, ISyncItemStream newItem)
        {
            if (!CacheSettings.EnablePerformanceCounter)
                return CacheState.Ok;

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
            return PerformanceCounter.ExchangeSizeAndCount(oldSize, newSize, oldCount, newCount, false, CacheSettings.EnableSizeHandler);
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
        /// Initialize a new instance of SyncCacheAgent
        /// </summary>
        public SyncCacheAgent()
            : base("SyncCacheAgent")
        {
            m_Perform = new CachePerformanceCounter(this, CacheAgentType.SyncCache, CacheName);
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
        public NetStream ExecRemote(CacheMessage message)
        {
            if(!this.Initialized)
            {
                return CacheEntry.GetAckStream(CacheState.CacheNotReady, message.Command, message.Key);
            }

            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;
            try
            {
                switch (message.Command)
                {
                    case SyncCacheCmd.Reply:
                        return CacheEntry.GetAckStream(CacheState.Ok, SyncCacheCmd.Reply, message.Key);
                    case SyncCacheCmd.GetSyncItem:
                        return message.AsyncAckTask(() => Get(message), message.Command);
                    case SyncCacheCmd.GetRecord:
                        return message.AsyncAckTask(() => GetRecord(message), message.Command);
                    case SyncCacheCmd.GetEntity:
                        return message.AsyncAckTask(() => GetEntity(message), message.Command);
                    case SyncCacheCmd.GetAs:
                        return message.AsyncAckTask(() => GetAs(message), message.Command);
                    case SyncCacheCmd.Refresh:
                        message.AsyncTask(() => Refresh());
                        break;
                    case SyncCacheCmd.Reset:
                        message.AsyncTask(() => Reset());
                        break;
                    case SyncCacheCmd.RefreshItem:
                        message.AsyncTask(() => Refresh(message.Key));
                        break;
                    case SyncCacheCmd.Contains:
                        return message.AsyncTask(() => Contains(CacheKeyInfo.Parse(message.Key)), message.Command);
                    case SyncCacheCmd.AddSyncItem:
                        message.AsyncTask(() => AddItem(message));
                        break;
                    case SyncCacheCmd.RemoveSyncItem:
                        message.AsyncTask(() => RemoveItem(message.Key));
                        break;
                    case SyncCacheCmd.AddSyncEntity:
                        message.AsyncTask(() => AddItem(message.DecodeBody<SyncEntity>()));
                        break;
                    case SyncCacheCmd.GetEntityItems:
                            return message.AsyncTask(() => GetEntityItemsInternal(message), message.Command);
                    case SyncCacheCmd.GetEntityKeys:
                            return message.AsyncTask(() => GetEntityKeysInternal(message), message.Command);
                    case SyncCacheCmd.GetAllEntityNames:
                            return message.AsyncTask(() => GetNames(), message.Command);
                    case SyncCacheCmd.GetItemsReport:
                            return message.AsyncTask(() => GetItemsReportInternal(message), message.Command);
                    case SyncCacheCmd.GetEntityItemsCount:
                            return message.AsyncTask(() => GetEntityItemsCountInternal(message), message.Command);
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

    }
}


