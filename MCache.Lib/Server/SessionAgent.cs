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
using Nistec.Channels;

namespace Nistec.Caching.Server
{

    /// <summary>
    /// Represent <see cref="SessionCache"/> as server agent.
    /// </summary>
    public class SessionAgent : SessionCache
    {

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
        */
        #endregion

        #region ctor
        /// <summary>
        /// Initialize a new instance of SessionAgent
        /// </summary>
        public SessionAgent():base()
        {
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "SessionAgent Initilaized!");
        }

        #endregion
                
        #region Remote methods
        /// <summary>
        /// Execute remote command from client to session cache using <see cref="CacheMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public TransStream ExecRemote(CacheMessage message)
        {
            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;

            try
            {
                //IKeyValue args = null;

                switch (message.Command.ToLower())
                {
                    case SessionCmd.Reply:
                        return TransStream.Write("Reply: " + message.Identifier, TransType.Text);
                    case SessionCmd.CreateSession:
                        {
                            var args = message.Args;
                            return AsyncTransState(() => CreateSession(message.SessionId, args.Get<string>(KnownArgs.UserId), message.Expiration, args.Get<string>(KnownArgs.StrArgs)),requestTime, CacheState.AddItemFailed);
                        }
                    case SessionCmd.RemoveSession:
                        return AsyncTransState(() => RemoveSession(message.SessionId), requestTime, CacheState.RemoveItemFailed);
                    case SessionCmd.ClearItems:
                        return AsyncTransState(() => ClearItems(message.SessionId), requestTime, CacheState.RemoveItemFailed);
                    case SessionCmd.ClearAll:
                        return AsyncTransState(() => ClearAll(), requestTime, CacheState.RemoveItemFailed);
                    case SessionCmd.GetOrCreateSession:
                        return AsyncTransObject(() => GetOrCreateSession(message.SessionId), message.Command, requestTime, CacheState.ItemAdded, CacheState.UnexpectedError, message.TransformType.ToTransType());
                    //case SessionCmd.GetSessionStream:
                    //    return AsyncTransObject(() => GetSessionBagStream(message.GroupId),message.Command, requestTime, CacheState.Ok, CacheState.NotFound);
                    case SessionCmd.GetOrCreateRecord:
                        return AsyncTransStream(() => GetOrCreateRecord(message.SessionId), message.Command, requestTime, CacheState.Ok, CacheState.UnexpectedError, message.TransformType.ToTransType());
                    //case SessionCmd.GetExistingSessionRecord:
                    //    return message.AsyncTransStream(() => GetExistingBagRecord(message.Id));
                    case SessionCmd.GetSessionItems:
                        return AsyncTransObject(() => GetSessionItems(message.SessionId), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());

                    case SessionCmd.Refresh:
                        return AsyncTransState(() => Refresh(message.SessionId), requestTime, CacheState.UnKnown);
                    case SessionCmd.RefreshOrCreate:
                        return AsyncTransState(() => RefreshOrCreate(message.SessionId), requestTime, CacheState.UnKnown);

                    case SessionCmd.Remove:
                        return AsyncTransState(() => RemoveItem(message.SessionId, message.Identifier), requestTime, CacheState.RemoveItemFailed);
                    case SessionCmd.Add:
                        return AsyncTransState(() => AddItem(new SessionEntry(message)), requestTime, CacheState.AddItemFailed);
                    case SessionCmd.Set:
                        return AsyncTransState(() => SetItem(new SessionEntry(message)), requestTime, CacheState.SetItemFailed);
                    case SessionCmd.GetEntry:
                        return AsyncTransObject(() => GetEntry(message.SessionId, message.Identifier), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SessionCmd.Get:
                        return AsyncTransStream(() => GetSessionValueStream(message.SessionId, message.Identifier), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SessionCmd.GetRecord:
                        return AsyncTransObject(() => GetItemRecord(message.SessionId, message.Identifier), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SessionCmd.Fetch:
                        return AsyncTransObject(() => FetchItem(message.SessionId, message.Identifier), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SessionCmd.FetchRecord:
                        return AsyncTransObject(() => FetchItemRecord(message.SessionId, message.Identifier), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SessionCmd.CopyTo:
                        {
                             var args = message.Args;
                             return AsyncTransState(() => CopyTo(message.SessionId, message.Identifier, args.Get<string>(KnownArgs.TargetKey), message.Expiration, args.Get<bool>(KnownArgs.AddToCache)), requestTime, CacheState.SetItemFailed);
                        }
                    case SessionCmd.CutTo:
                        {
                            var args = message.Args;
                            return AsyncTransState(() => CutTo(message.SessionId, message.Identifier, args.Get<string>(KnownArgs.TargetKey), message.Expiration, args.Get<bool>(KnownArgs.AddToCache)), requestTime, CacheState.SetItemFailed);
                        }
                    case SessionCmd.Exists:
                        return AsyncTransState(() => Exists(message.SessionId, message.Identifier), requestTime, CacheState.NotFound);
                    case SessionCmd.ViewAllSessionsKeys:
                        return AsyncTransObject(() => ViewAllSessionsKeys(), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SessionCmd.ViewAllSessionsKeysByState:
                        {
                            SessionState st = (SessionState) EnumExtension.Parse<SessionState>(message.Identifier, SessionState.Active);//.GetArgs().Get<int>("state");
                            return AsyncTransObject(() => ViewAllSessionsKeysByState(st), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                        }
                    case SessionCmd.ViewSessionKeys:
                        return AsyncTransObject(() => ViewSessionKeys(message.SessionId), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SessionCmd.ViewEntry:
                        return AsyncTransObject(() => ViewEntry(message.SessionId, message.Identifier), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                    case SessionCmd.ViewSessionStream:
                        return AsyncTransObject(() => ViewSessionBagStream(message.SessionId), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());

                    default:
                        state = CacheState.CommandNotSupported;
                        return TransStream.WriteState((int)state,message.Command + ": " + state.ToString());//, CacheUtil.ToTransType(state));
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
            return TransStream.WriteState((int)state ,message.Command + ": " + state.ToString());//, CacheUtil.ToTransType(state));
        }

        #endregion

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
            return TransStream.WriteState(-1, command + ": " + failedState.ToString());//, TransType.Error);
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
                    return TransStream.WriteState((int)task.Result, task.Result.ToString());//TransType.State);
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
            return TransStream.WriteState((int)failedState, failedState.ToString());// TransType.State);
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
            return TransStream.WriteState((int)failedState, failedState.ToString());// TransType.State);
        }

        #endregion

    }

}
