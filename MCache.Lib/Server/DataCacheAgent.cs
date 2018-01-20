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
using Nistec.Caching.Data;
using System.Data;
using System.Threading.Tasks;
using System.Threading;
using Nistec.Caching.Sync;
using Nistec.Caching.Config;
using Nistec.Data.Entities;

namespace Nistec.Caching.Server
{
    /// <summary>
    /// Represent <see cref="DbCache"/> as server agent.
    /// </summary>
    [Serializable]
    public class DataCacheAgent : DbCache//, ICachePerformance
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
            CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.None, "Memory Size Exchange: DbCache" );
            long size = 0;

            Interlocked.Exchange(ref memorySize, size);

        }

        internal long MaxSize { get { return 999999999L; } }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long ICachePerformance.GetMaxSize()
        {
            return MaxSize * 1024;
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
        internal protected override CacheState SizeExchage(DataCache oldItem, DataCache newItem)
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
        */
        #endregion

        #region ctor

        /// <summary>
        /// Initialize a new instance of db cache.
        /// </summary>
        /// <param name="dbCacheName"></param>
        public DataCacheAgent(string dbCacheName)
            : base(dbCacheName)
        {
            //m_Perform = new CachePerformanceCounter(this, CacheAgentType.DataCache, dbCacheName);
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "DataCacheAgent Initilaized!");
        }
        
        #endregion

        /// <summary>
        /// Execute remote command from client to db cache using <see cref="CacheMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public TransStream ExecRemote(MessageStream message)
        {
            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;
            //TransStream response = null;
            try
            {
                //IKeyValue args = null;

                switch (message.Command.ToLower())
                {
                    case DataCacheCmd.Reply:
                        return TransStream.Write("Reply: " + message.Id, TransType.Object);

                    case DataCacheCmd.Add:
                        {
                            var args = message.GetArgs();
                            return AsyncTransState(() => AddValue(message.GroupId,message.Label, message.Id, args[KnowsArgs.Column], message.DecodeBody()), requestTime, CacheState.ItemAdded, CacheState.AddItemFailed);
                        }
                    case DataCacheCmd.Set:
                        {
                            var args = message.GetArgs();
                            return AsyncTransState(() => SetValue(message.GroupId, message.Label, message.Id, args[KnowsArgs.Column],  message.DecodeBody()),requestTime, CacheState.ItemChanged, CacheState.SetItemFailed);
                        }
                    case DataCacheCmd.Get:
                        {
                            var args = message.GetArgs();
                            return AsyncTransObject(()=> GetValue(message.GroupId, message.Label, message.Id, args[KnowsArgs.Column]), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.GetRecord:
                        {
                            var args = message.GetArgs();
                            return AsyncTransObject(() => GetRecord(message.GroupId, message.Label,message.Id), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.GetStream:
                        {
                            var args = message.GetArgs();
                            return AsyncTransStream(() => GetStream(message.GroupId, message.Label, message.Id), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, TransType.Stream);
                        }
                    case DataCacheCmd.AddTable:
                        {
                            var args = message.GetArgs();
                            EntitySourceType sourceType = EnumExtension.Parse<EntitySourceType>(args[KnowsArgs.SourceType], EntitySourceType.Table);//(EntitySourceType)Types.ToInt(args[KnowsArgs.SourceType]);
                            return AsyncTransState(() => AddTable(message.GroupId, (DataTable)message.DecodeBody(), message.Label, args[KnowsArgs.MappingName], sourceType, message.Id.SplitTrim(',')), requestTime, CacheState.ItemAdded, CacheState.AddItemFailed);
                        }
                    case DataCacheCmd.SetTable:
                        {
                            var args = message.GetArgs();
                            EntitySourceType sourceType = EnumExtension.Parse<EntitySourceType>(args[KnowsArgs.SourceType], EntitySourceType.Table);// (EntitySourceType)Types.ToInt(args[KnowsArgs.SourceType]);
                            return AsyncTransState(() => SetTable(message.GroupId, (DataTable)message.DecodeBody(), message.Label, args[KnowsArgs.MappingName], sourceType, message.Id.SplitTrim(',')),requestTime, CacheState.ItemChanged, CacheState.SetItemFailed);
                        }
                    case DataCacheCmd.GetTable:
                        {
                            return AsyncTransObject(() => GetTable(message.GroupId, message.Label), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.RemoveTable:
                        {
                            return AsyncTransState(() => RemoveTable(message.GroupId, message.Label), requestTime, CacheState.ItemRemoved, CacheState.RemoveItemFailed);
                        }
                    case DataCacheCmd.GetItemProperties:
                        {
                            return AsyncTransObject(() => GetItemProperties(message.GroupId, message.Label), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.AddTableWithSync:
                        {
                            var args = message.GetArgs();

                            string tableName = message.Label;
                            string mappingName = args[KnowsArgs.MappingName];
                            EntitySourceType sourceType = EnumExtension.Parse<EntitySourceType>(args[KnowsArgs.SourceType], EntitySourceType.Table);//(EntitySourceType) Types.ToInt(args[KnowsArgs.SourceType]);
                            string[] pk = message.Id.SplitTrim(',');
                            SyncEntity syncEntity = new SyncEntity()
                            {
                                EntityName = tableName, 
                                ViewName = mappingName,
                                SourceType=sourceType,
                                SourceName = args.SplitArg(KnowsArgs.SourceName, null),
                                PreserveChanges = false,
                                MissingSchemaAction = MissingSchemaAction.Add,
                                SyncType = (SyncType)args.Get<int>(KnowsArgs.SyncType),
                                Interval = args.TimeArg(KnowsArgs.SyncTime, null),
                                EnableNoLock = false,
                                CommandTimeout = 0
                            };
                            return AsyncTransState(() => AddTableWithSync(message.GroupId, (DataTable)message.DecodeBody(),  tableName, mappingName, sourceType, pk, syncEntity), requestTime, CacheState.ItemAdded, CacheState.NotFound);

                        }

                    case DataCacheCmd.AddSyncItem:
                        {
                            var args = message.GetArgs();

                            SyncEntity syncEntity = new SyncEntity()
                            {
                                EntityName = message.Label,
                                ViewName = args[KnowsArgs.MappingName],
                                SourceName = args.SplitArg(KnowsArgs.SourceName, null),
                                PreserveChanges = false,
                                MissingSchemaAction = MissingSchemaAction.Add,
                                SyncType = (SyncType)args.Get<int>(KnowsArgs.SyncType),
                                Interval = args.TimeArg(KnowsArgs.SyncTime, null),
                                EnableNoLock = false,
                                CommandTimeout = 0
                            };

                            return AsyncTransState(() => AddSyncItem(message.GroupId, syncEntity), requestTime, CacheState.ItemAdded, CacheState.NotFound);

                        }

                    case DataCacheCmd.Reset:
                        return AsyncTransState(() => RefreshDataSource(message.GroupId), requestTime, CacheState.Ok, CacheState.UnKnown);
                    case DataCacheCmd.Refresh:
                        return AsyncTransState(() => Refresh(message.GroupId, message.Label), requestTime, CacheState.Ok, CacheState.UnKnown);
                    case DataCacheCmd.Contains:
                        {
                            return AsyncTransState(() => Contains(message.GroupId, message.Label), requestTime, CacheState.Ok, CacheState.NotFound);
                        }
                    case DataCacheCmd.GetEntityItems:
                        {
                            return AsyncTransObject(() => GetEntityItems(message.GroupId, message.Label), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                        }

                    case DataCacheCmd.GetEntityKeys:
                        {
                            return AsyncTransObject(() => GetEntityKeys(message.GroupId, message.Label), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.GetAllEntityNames:
                        {
                            return AsyncTransObject(() => GetNames(message.GroupId), message.Command, requestTime, CacheState.Ok, CacheState.NotFound, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.GetItemsReport:
                        {
                            return AsyncTransObject(() => GetItemsReport(message.GroupId, message.Label), message.Command, requestTime, CacheState.Ok, CacheState.UnexpectedError, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.GetEntityItemsCount:
                        {
                            return AsyncTransObject(() => GetEntityItemsCount(message.GroupId, message.Label), message.Command, requestTime, CacheState.Ok, CacheState.UnexpectedError, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.QueryTable:
                        {
                            return AsyncTransObject(() => QueryTable(message), message.Command, requestTime, CacheState.Ok, CacheState.UnexpectedError, message.TransformType.ToTransType());
                        }
                    case DataCacheCmd.QueryEntity:
                        {
                            return AsyncTransObject(() => QueryEntity(message), message.Command, requestTime, CacheState.Ok, CacheState.UnexpectedError, message.TransformType.ToTransType());
                        }
                    default:
                        state = CacheState.CommandNotSupported;
                        return TransStream.Write(message.Command + ": " + state.ToString(), CacheUtil.ToTransType(state));
                }
            }
            catch (CacheException ce)
            {
                state = ce.State;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "DataCacheAgent.ExecRemote CacheException error: " + ce.Message);
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = CacheState.SerializationError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "DataCacheAgent.ExecRemote SerializationException error: " + se.Message);
            }
            catch (ArgumentException aex)
            {
                state = CacheState.ArgumentsError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "DataCacheAgent.ExecRemote ArgumentException: " + aex.Message);
            }
            catch (Exception ex)
            {
                state = CacheState.UnexpectedError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "DataCacheAgent.ExecRemote error: " + ex.Message);
            }
            SendState(requestTime, state);
            return TransStream.Write(message.Command + ": " + state.ToString(), CacheUtil.ToTransType(state));
        }


        #region Async Task

        public void SendState(DateTime requestTime,CacheState state)
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

        public TransStream AsyncTransState(Action action, DateTime requestTime, CacheState successState, CacheState failedState= CacheState.UnKnown)
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


