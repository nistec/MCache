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
    public class CacheAgent : MCache
    {


        #region size exchange
        /*
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
        */
        #endregion

        #region ctor

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="prop"></param>
        public CacheAgent(CacheProperties prop)
            : base(prop)
        {
            //m_Perform = new CachePerformanceCounter(this, CacheAgentType.Cache, this.CacheName);
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "CacheAgent Initilaized!");
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
        public TransStream ExecRemote(MessageStream message)
        {
            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;
            try
            {
                switch (message.Command.ToLower())
                {
                    case CacheCmd.Reply:
                        return TransStream.Write("Reply: " + message.Id, TransType.Object);
                    case CacheCmd.Add:
                        return AsyncTransState(() => Add(message),requestTime, CacheState.AddItemFailed);
                    case CacheCmd.Set:
                        return AsyncTransState(() => Set(message), requestTime, CacheState.SetItemFailed);
                    case CacheCmd.GetRecord:
                        return AsyncTransObject(() => GetRecord(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case CacheCmd.Get:
                        return AsyncTransStream(() => GetValueStream(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case CacheCmd.Fetch:
                        return AsyncTransStream(() => FetchValueStream(message), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case CacheCmd.GetEntry:
                        return AsyncTransObject(() => GetItem(message.Id), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case CacheCmd.FetchEntry:
                        return AsyncTransObject(() => FetchItem(message.Id), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case CacheCmd.ViewEntry:
                        return AsyncTransObject(() => ViewItem(message.Id), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case CacheCmd.Remove:
                        return AsyncTransState(() => Remove(message.Id), requestTime, CacheState.ItemRemoved, CacheState.RemoveItemFailed);
                    case CacheCmd.RemoveAsync:
                        return AsyncTransState(() => RemoveAsync(message.Id), requestTime, CacheState.ItemRemoved, CacheState.RemoveItemFailed);
                    case CacheCmd.CopyTo:
                        {
                            var args = message.GetArgs();
                            return AsyncTransState(() => CopyTo(args.Get<string>(KnowsArgs.Source), args.Get<string>(KnowsArgs.Destination), message.Expiration), requestTime, CacheState.SetItemFailed);
                        }
                    case CacheCmd.CutTo:
                        {
                            var args = message.GetArgs();
                            return AsyncTransState(() => CutTo(args.Get<string>(KnowsArgs.Source), args.Get<string>(KnowsArgs.Destination), message.Expiration), requestTime, CacheState.SetItemFailed);
                        }
                    case CacheCmd.KeepAliveItem:
                        {
                            return AsyncTransState(() => KeepAliveItem(message.Id),requestTime, CacheState.Ok, CacheState.UnexpectedError);
                        }
                    case CacheCmd.RemoveItemsBySession:
                        return AsyncTransState(() => RemoveCacheSessionItemsAsync(message.Label), requestTime, CacheState.RemoveItemFailed);
                    case CacheCmd.LoadData:
                        return AsyncTransObject(() => LoadData(message), message.Command, requestTime, CacheState.Ok, CacheState.UnexpectedError);
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
            //finally
            //{
            //    if (CacheSettings.EnablePerformanceCounter)
            //    {
            //        PerformanceCounter.AddResponseAsync(requestTime, state, true);

            //        //if (CacheSettings.EnableAsyncTask)
            //        //    AgentManager.PerformanceTasker.Add(new Nistec.Threading.TaskItem(() => PerformanceCounter.AddResponse(requestTime, state, true), CacheDefaults.DefaultTaskTimeout));
            //        //else
            //        //    Task.Factory.StartNew(() => PerformanceCounter.AddResponse(requestTime, state, true));
            //    }

            //}

            //return new TransStream(message.Command + ": " + state.ToString(), CacheUtil.ToTransType(state));

            SendState(requestTime, state);
            return TransStream.Write(message.Command + ": " + state.ToString(), CacheUtil.ToTransType(state));

        }

        #region Async Task

        public void SendState(DateTime requestTime, CacheState state)
        {
            if (CacheSettings.EnablePerformanceCounter)
            {
                PerformanceCounter.AddResponseAsync(requestTime, state, true);
            }
        }

        public TransStream AsyncTransStream(Func<NetStream> action, string command, DateTime requestTime, CacheState successState, CacheState failedState = CacheState.NotFound, TransType transType = TransType.Object)//TransformType transform = TransformType.Message)
        {
            Task<NetStream> task = Task.Factory.StartNew<NetStream>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result != null)
                    {
                        SendState(requestTime, successState);
                        return TransStream.Write(task.Result, transType);
                    }
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.Write(command + ": " + failedState.ToString(), TransType.Error);
        }
        public TransStream AsyncTransObject(Func<object> action, string command, DateTime requestTime, CacheState successState, CacheState failedState = CacheState.NotFound, TransType transType = TransType.Object)//TransformType transform = TransformType.Message)
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
            return TransStream.Write(command + ": " + failedState.ToString(), TransType.Error);
        }

        public TransStream AsyncTransState(Func<CacheState> action, DateTime requestTime, CacheState failedState = CacheState.NotFound)
        {
            Task<CacheState> task = Task.Factory.StartNew<CacheState>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    SendState(requestTime, task.Result);
                    return TransStream.Write((int)task.Result, TransType.State);
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.Write((int)failedState, TransType.State);
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
                    return TransStream.Write((int)state, TransType.State);
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.Write((int)failedState, TransType.State);
        }

        public TransStream AsyncTransState(Action action, DateTime requestTime, CacheState successState, CacheState failedState = CacheState.UnKnown)
        {
            Task task = Task.Factory.StartNew(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    SendState(requestTime, successState);
                    return TransStream.Write((int)successState, TransType.State);
                }
            }
            task.TryDispose();
            SendState(requestTime, failedState);
            return TransStream.Write((int)failedState, TransType.State);
        }

        #endregion

    }
}


