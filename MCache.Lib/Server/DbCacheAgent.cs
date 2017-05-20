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


namespace Nistec.Caching.Server
{
    /// <summary>
    /// Represent <see cref="DbCache"/> as server agent.
    /// </summary>
    [Serializable]
    public class DbCacheAgent : DbCache, ICachePerformance
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

        #endregion

        #region ctor

       /// <summary>
       /// Initialize a new instance of db cache.
       /// </summary>
       /// <param name="dbCacheName"></param>
        public DbCacheAgent(string dbCacheName)
            : base(dbCacheName)
        {
            m_Perform = new CachePerformanceCounter(this, CacheAgentType.DataCache, dbCacheName);
        }
 
        #endregion

        /// <summary>
        /// Execute remote command from client to db cache using <see cref="CacheMessage"/>.
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
                    case DataCacheCmd.Reply:
                        return CacheEntry.GetAckStream(CacheState.Ok, DataCacheCmd.Reply, message.Key);

                    case DataCacheCmd.SetValue:
                        args = message.GetArgs();
                        SetValue(message.Key, args.Get<string>(KnowsArgs.TableName), args.Get<string>(KnowsArgs.Column), args.Get<string>(KnowsArgs.Filter), message.BodyStream);
                        break;
                    case DataCacheCmd.GetDataValue:
                        {
                            args = message.GetArgs();
                            var val = GetValue(message.Key, args.Get<string>(KnowsArgs.TableName), args.Get<string>(KnowsArgs.Column), args.Get<string>(KnowsArgs.Filter));
                            return AckStream.GetAckStream(val, message.Command);
                        }
                    case DataCacheCmd.GetRow:
                        {
                            args = message.GetArgs();
                            var val = GetRow(message.Key, args.Get<string>(KnowsArgs.TableName), args.Get<string>(KnowsArgs.Filter));
                            return AckStream.GetAckStream(val, message.Command);

                        }
                    case DataCacheCmd.GetDataTable:
                        {
                            args = message.GetArgs();
                            var val = GetDataTable(message.Key, args.Get<string>(KnowsArgs.TableName));
                            return AckStream.GetAckStream(val, message.Command);

                        }
                    case DataCacheCmd.RemoveTable:
                        args = message.GetArgs();
                        RemoveTable(message.Key, args.Get<string>(KnowsArgs.TableName));
                        break;
                    case DataCacheCmd.GetItemProperties:
                        args = message.GetArgs();
                        return message.AsyncTask(() => GetItemProperties(message.Key, args.Get<string>(KnowsArgs.TableName)), message.Command);

                    case DataCacheCmd.AddDataItem:
                        args = message.GetArgs();
                        return message.AsyncTask(() => AddDataItem(message.Key, (DataTable)message.DecodeBody(), args.Get<string>(KnowsArgs.TableName)), message.Command);
                    case DataCacheCmd.AddDataItemSync:
                        {
                            args = message.GetArgs();

                            string tableName = args.Get<string>(KnowsArgs.TableName);

                            SyncEntity syncEntity = new SyncEntity()
                               {
                                   EntityName = tableName,
                                   ViewName = args.Get<string>(KnowsArgs.MappingName),
                                   SourceName = CacheMessage.SplitArg(args, KnowsArgs.SourceName, null),
                                   PreserveChanges = false,
                                   MissingSchemaAction = MissingSchemaAction.Add,
                                   SyncType = (SyncType)args.Get<int>(KnowsArgs.SyncType),
                                   Interval = CacheMessage.TimeArg(args, KnowsArgs.SyncTime, null),
                                   EnableNoLock = false,
                                   CommandTimeout = 0
                               };
                            message.AsyncTask(() => AddDataItem(message.Key, (DataTable)message.DecodeBody(), tableName, syncEntity));
                        }
                        break;
                    case DataCacheCmd.AddSyncDataItem:
                        {
                            args = message.GetArgs();

                             SyncEntity syncEntity = new SyncEntity()
                               {
                                   EntityName = args.Get<string>(KnowsArgs.TableName),
                                   ViewName = args.Get<string>(KnowsArgs.MappingName),
                                   SourceName = CacheMessage.SplitArg(args, KnowsArgs.SourceName, null),
                                   PreserveChanges = false,
                                   MissingSchemaAction = MissingSchemaAction.Add,
                                   SyncType = (SyncType)args.Get<int>(KnowsArgs.SyncType),
                                   Interval = CacheMessage.TimeArg(args, KnowsArgs.SyncTime, null),
                                   EnableNoLock = false,
                                   CommandTimeout = 0
                               };

                            message.AsyncTask(() => AddSyncItem(message.Key, syncEntity));
                        }
                        break;
                }

            }

            catch (CacheException ce)
            {
                state = ce.State;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "DbCacheAgent.ExecRemote CacheException error: " + ce.Message);
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = CacheState.SerializationError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "DbCacheAgent.ExecRemote SerializationException error: " + se.Message);
            }
            catch (ArgumentException aex)
            {
                state = CacheState.ArgumentsError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "DbCacheAgent.ExecRemote ArgumentException: " + aex.Message);
            }
            catch (Exception ex)
            {
                state = CacheState.UnexpectedError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "DbCacheAgent.ExecRemote error: " + ex.Message);
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


