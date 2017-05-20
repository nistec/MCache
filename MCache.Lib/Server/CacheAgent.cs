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
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Caching.Remote;
using Nistec.Generic;
using Nistec.Channels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Nistec.Caching.Config;


namespace Nistec.Caching.Server
{
    /// <summary>
    /// Represent <see cref="MCache"/> as server agent.
    /// </summary>
    [Serializable]
    public class CacheAgent : MCache, ICachePerformance
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
            this.LogAction(CacheAction.MemorySizeExchange, CacheActionState.None, "Memory Size Exchange:" + CacheName);
            long size = 0;
            ICollection<CacheEntry> items = m_cacheList.Values;
            foreach (var entry in items)
            {
                size += entry.Size;
            }

            Interlocked.Exchange(ref memorySize, size);

        }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long ICachePerformance.GetMaxSize()
        {
            return MaxSize;
        }
        bool ICachePerformance.IsRemote
        {
            get { return base.IsRemote; }
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
        internal protected override CacheState SizeValidate(int newSize)
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
        /// ctor.
        /// </summary>
        /// <param name="prop"></param>
        public CacheAgent(CacheProperties prop)
            : base(prop)
        {
            m_Perform = new CachePerformanceCounter(this, CacheAgentType.Cache, this.CacheName);

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
        /// Reset cache.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            CacheLogger.Logger.Clear();
        }

        #endregion


        /// <summary>
        /// Execute remote command from client to cache using <see cref="CacheMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream ExecRemote(CacheMessage message)
        {
            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;
            try
            {
                switch (message.Command)
                {
                    case CacheCmd.Reply:
                        return CacheEntry.GetAckStream(CacheState.Ok,CacheCmd.Reply,message.Key);
                    case CacheCmd.AddItem:
                        return message.AsyncAckTask(() => AddItemWithAck(message), message.Command);
                    case CacheCmd.GetValue:
                        return message.AsyncAckTask(() => GetValueStream(message.Key), message.Command);
                    case CacheCmd.FetchValue:
                        return message.AsyncAckTask(() => FetchValueStream(message.Key), message.Command);
                    case CacheCmd.GetItem:
                        return message.AsyncAckTask(() => GetItemStream(message.Key), message.Command);
                    case CacheCmd.FetchItem:
                        return message.AsyncAckTask(() => FetchItemStream(message.Key), message.Command);
                    case CacheCmd.ViewItem:
                        return message.AsyncAckTask(() => ViewItemStream(message.Key), message.Command);
                    case CacheCmd.RemoveItem:
                        return message.AsyncAckTask(() => RemoveItem(message), message.Command);
                    case CacheCmd.RemoveItemAsync:
                        message.AsyncTask(() => RemoveItemAsync(message.Key));
                        break;
                    case CacheCmd.CopyItem:
                        {
                            var args = message.GetArgs();
                            return message.AsyncAckTask(() => CopyItemInternal(args.Get<string>(KnowsArgs.Source), args.Get<string>(KnowsArgs.Destination), message.Expiration), message.Command);
                        }
                    case CacheCmd.CutItem:
                        {
                            var args = message.GetArgs();
                            return message.AsyncAckTask(() => CutItemInternal(args.Get<string>(KnowsArgs.Source), args.Get<string>(KnowsArgs.Destination), message.Expiration), message.Command);
                        }
                    case CacheCmd.KeepAliveItem:
                        message.AsyncTask(() => KeepAliveItem(message.Key));
                        break;
                    case CacheCmd.RemoveCacheSessionItems:
                        return message.AsyncAckTask(() => RemoveCacheSessionItemsAsync(message), message.Command);
                    case CacheCmd.LoadData:
                        return message.AsyncAckTask(() => LoadData(message), message.Command);

                }

            }
            catch (CacheException ce)
            {
                state = ce.State;
                LogAction(CacheAction.CacheException, CacheActionState.Error, "CacheAgent.ExecRemote CacheException error: " + ce.Message);
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = CacheState.SerializationError;
                LogAction(CacheAction.CacheException, CacheActionState.Error, "CacheAgent.ExecRemote SerializationException error: " + se.Message);
            }
            catch (ArgumentException aex)
            {
                state = CacheState.ArgumentsError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "CacheAgent.ExecRemote ArgumentException: " + aex.Message);
            }
            catch (Exception ex)
            {
                state = CacheState.UnexpectedError;
                LogAction(CacheAction.CacheException, CacheActionState.Error, "CacheAgent.ExecRemote error: " + ex.Message);
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

            }

            return CacheEntry.GetAckStream(state, message.Command);
        }

    }
}


