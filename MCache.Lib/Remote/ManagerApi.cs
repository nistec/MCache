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
using Nistec.Data.Entities;
using System.Collections;
using Nistec.Channels;
using Nistec.Caching.Session;
using Nistec.Caching.Channels;
using Nistec.Caching.Config;
using Nistec.Caching.Data;
using System.Data;
using Nistec.Data.Entities.Cache;
using Nistec.Runtime;
using Nistec.Generic;
using Nistec.Serialization;

namespace Nistec.Caching.Remote
{
    /// <summary>
    /// Represent cache managment api for client.
    /// </summary>
    public static class ManagerApi
    {

        #region static client methods
        /// <summary>
        /// Get cache properties as dictionary
        /// </summary>
        /// <returns></returns>
        public static IDictionary CacheProperties()
        {
            return PipeClientCache.SendDuplex<Hashtable>(new CacheMessage()
            {
                Command = CacheManagerCmd.CacheProperties
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }
        /// <summary>
        /// Get items copy with specified condition.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static CacheEntry[] CloneItems(CloneType ct)
        {
            return PipeClientCache.SendDuplex<CacheEntry[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.CloneItems,
                Args = MessageStream.CreateArgs(KnowsArgs.CloneType, ((int)ct).ToString())
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }
        /// <summary>
        /// Get all items keys in cache.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllKeys()
        {
            return PipeClientCache.SendDuplex<string[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllKeys
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }
        /// <summary>
        /// Get all items keys in cache.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllKeysIcons()
        {
            return PipeClientCache.SendDuplex<string[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllKeysIcons
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }
       

        /// <summary>
        /// Get state counter report of cache.
        /// </summary>
        /// <returns></returns>
        public static DataTable GetStateCounterReport()
        {
            return PipeClientCache.SendDuplex<DataTable>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetStateCounterReport
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Get state counter report of cache.
        /// </summary>
        /// <returns></returns>
        public static DataTable GetStateCounterReport(CacheAgentType agentType)
        {
            string cmd = CacheManagerCmd.StateCounterCache;

            switch (agentType)
            {
                case CacheAgentType.SyncCache:
                    cmd = CacheManagerCmd.StateCounterSync;break;
                case CacheAgentType.SessionCache:
                    cmd = CacheManagerCmd.StateCounterSession; break;
                case CacheAgentType.DataCache:
                    cmd = CacheManagerCmd.StateCounterDataCache; break;
            }
            return PipeClientCache.SendDuplex<DataTable>(new CacheMessage()
            {
                Command = cmd
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Reset PerformanceC ounter.
        /// </summary>
        /// <returns></returns>
        public static void ResetPerformanceCounter()
        {
            PipeClientCache.SendOut(new CacheMessage()
            {
                Command = CacheManagerCmd.ResetPerformanceCounter
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Get cache performance counter report.
        /// </summary>
        /// <returns></returns>
        public static ICachePerformanceReport GetPerformanceReport()
        {
            return PipeClientCache.SendDuplex<ICachePerformanceReport>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetPerformanceReport
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Get cache performance counter report for specified cache agent.
        /// </summary>
        /// <returns></returns>
        public static ICachePerformanceReport GetAgentPerformanceReport(CacheAgentType agentType)
        {
            return PipeClientCache.SendDuplex<ICachePerformanceReport>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAgentPerformanceReport,
                Key = agentType.ToString()
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Get all items keys in cache.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllDataKeys()
        {
            return PipeClientCache.SendDuplex<string[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllDataKeys
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Get all entities in sync cache.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllSyncCacheKeys()
        {
            return PipeClientCache.SendDuplex<string[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllSyncCacheKeys
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        ///// <summary>
        ///// Get statistic report of data cache.
        ///// </summary>
        ///// <returns></returns>
        //public static CacheView GetDataStatistic(string db)
        //{
        //    return PipeClientCache.SendDuplex<CacheView>(new CacheMessage()
        //    {
        //        Command = CacheManagerCmd.GetDataStatistic,
        //        Key =db
        //    }, CacheApiSettings.RemoteCacheManagerHostName);
        //}
        /// <summary>
        /// Get cache logger log.
        /// </summary>
        /// <returns></returns>
        public static string CacheLog()
        {
            return PipeClientCache.SendDuplex<string>(new CacheMessage()
            {
                Command = CacheManagerCmd.CacheLog
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Get all sessions keys in session cache.
        /// </summary>
        /// <returns></returns>
        public static ICollection<string> GetAllSessionsKeys()
        {
            return PipeClientCache.SendDuplex<ICollection<string>>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllSessionsKeys
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Get all sessions keys in session cache using <see cref="SessionState"/> state.
        /// </summary>
        /// <returns></returns>
        public static ICollection<string> GetAllSessionsStateKeys(SessionState state)
        {
            return PipeClientCache.SendDuplex<ICollection<string>>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllSessionsStateKeys,
                Args = CacheMessage.CreateArgs("state", ((int)state).ToString())
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        /// <summary>
        /// Get all items keys in specified session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static ICollection<string> GetSessionsItemsKeys(string sessionId)
        {
            return PipeClientCache.SendDuplex<ICollection<string>>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetSessionItemsKeys,
                Id = sessionId,
            }, CacheApiSettings.RemoteCacheManagerHostName);
        }

        #endregion


        /// <summary>
        /// CacheApi
        /// </summary>
        public static class CacheApi
        {
            /// <summary>
            /// Get item copy from cache
            /// </summary>
            /// <param name="cacheKey"></param>
            /// <returns>return <see cref="CacheEntry"/></returns>
            public static CacheEntry ViewItem(string cacheKey)
            {
                return PipeClientCache.SendDuplex<CacheEntry>(new CacheMessage() { Command = CacheCmd.ViewItem, Key = cacheKey },
                    CacheApiSettings.RemoteCacheManagerHostName);
            }
            /// <summary>
            /// RemoveItem
            /// </summary>
            /// <param name="cacheKey"></param>
            /// <returns></returns>
            public static CacheState RemoveItem(string cacheKey)
            {
                return (CacheState)PipeClientCache.SendDuplex<int>(new CacheMessage() { Command = CacheCmd.RemoveItem, Key = cacheKey },
                    CacheApiSettings.RemoteCacheManagerHostName);
            }
            /// <summary>
            /// Add Item to cache.
            /// </summary>
            /// <param name="cacheKey"></param>
            /// <param name="value"></param>
            /// <param name="expiration"></param>
            /// <returns></returns>
            public static CacheState AddItem(string cacheKey, object value, int expiration)
            {
                if (value == null)
                    return CacheState.ArgumentsError;
                return (CacheState)PipeClientCache.SendDuplex<int>(new CacheMessage(CacheCmd.AddItem, cacheKey, value, expiration),
                    CacheApiSettings.RemoteCacheHostName, CacheApiSettings.EnableRemoteException);
            }
        }
        /// <summary>
        /// DataCacheApi
        /// </summary>
        public static class DataCacheApi
        {
            /// <summary>
            /// GetItemProperties
            /// </summary>
            /// <param name="db"></param>
            /// <param name="tableName"></param>
            /// <returns></returns>
            public static DataCacheItem GetItemProperties(string db, string tableName)
            {
                return (DataCacheItem)PipeClientCache.SendDuplex<DataCacheItem>(new CacheMessage()
                {
                    Command = DataCacheCmd.GetItemProperties,
                    Key = db,
                    Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName)
                }, CacheApiSettings.RemoteCacheManagerHostName);

            }

            /// <summary>
            /// Remove data table  from storage
            /// </summary>
            /// <param name="db">db name</param>
            /// <param name="tableName">table name</param>
            public static void RemoveTable(string db, string tableName)
            {
                PipeClientCache.SendOut(new CacheMessage()
                {
                    Command = DataCacheCmd.RemoveTable,
                    Key = db,
                    Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName)
                }, CacheApiSettings.RemoteCacheManagerHostName);
            }
            
            /// <summary>
            /// Get DataTable from storage by table name.
            /// </summary>
            /// <param name="db">db name</param>
            /// <param name="tableName">table name</param>
            /// <returns>DataTable</returns>
            public static DataTable GetDataTable(string db, string tableName)
            {
                return (DataTable)PipeClientCache.SendDuplex<DataTable>(new CacheMessage()
                {
                    Command = DataCacheCmd.GetDataTable,
                    Key = db,
                    Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName)
                }, CacheApiSettings.RemoteCacheManagerHostName);
            }
            /// <summary>
            /// Add Remoting Data Item to cache
            /// </summary>
            /// <param name="db"></param>
            /// <param name="dt"></param>
            /// <param name="tableName"></param>
            public static bool AddDataItem(string db, DataTable dt, string tableName)
            {
                return PipeClientCache.SendDuplex<bool>(new CacheMessage()
                {
                    Command = DataCacheCmd.AddDataItem,
                    Key = db,
                    BodyStream = BinarySerializer.ConvertToStream(dt),// CacheMessageStream.EncodeBody(dt),
                    Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName)
                }, CacheApiSettings.RemoteDataCacheHostName, CacheApiSettings.EnableRemoteException);
            }

            /// <summary>
            /// Add Remoting Data Item to cache include SyncTables.
            /// </summary>
            /// <param name="db"></param>
            /// <param name="dt"></param>
            /// <param name="tableName"></param>
            /// <param name="mappingName"></param>
            /// <param name="sourceName"></param>
            /// <param name="syncType"></param>
            /// <param name="ts"></param>
            public static void AddDataItemSync(string db, DataTable dt, string tableName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan ts)
            {
                PipeClientCache.SendOut(new CacheMessage()
                {
                    Command = DataCacheCmd.AddDataItemSync,
                    Key = db,
                    BodyStream = BinarySerializer.ConvertToStream(dt),//CacheMessageStream.EncodeBody(dt),
                    Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.MappingName, mappingName, KnowsArgs.SourceName, GenericNameValue.JoinArg(sourceName), KnowsArgs.SyncType, ((int)syncType).ToString(), KnowsArgs.SyncTime, ts.ToString())
                }, CacheApiSettings.RemoteDataCacheHostName, CacheApiSettings.EnableRemoteException);
            }

            /// <summary>
            /// Add Remoting Data Item to cache include SyncTables.
            /// </summary>
            /// <param name="db"></param>
            /// <param name="dt"></param>
            /// <param name="tableName"></param>
            /// <param name="mappingName"></param>
            /// <param name="syncType"></param>
            /// <param name="ts"></param>
            /// <example><code>
            ///  //Add data table to data cache.
            ///public void AddItems()
            ///{
            ///    DataTable dt = null;
            ///    using (IDbCmd cmd = DbFactory.Create(db))
            ///    {
            ///        dt = <![CDATA[cmd.ExecuteCommand<DataTable>("select * from Person.Contact");]]>
            ///    }
            ///    TcpDataCacheApi.AddDataItem(db, dt, "Contact");
            ///    //add table to sync tables, for synchronization by interval.
            ///    TcpDataCacheApi.AddSyncItem(db, tableName, "Person.Contact", SyncType.Interval, TimeSpan.FromMinutes(60));
            ///}
            /// </code></example>
            public static void AddDataItemSync(string db, DataTable dt, string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts)
            {
                PipeClientCache.SendOut(new CacheMessage()
                {
                    Command = DataCacheCmd.AddDataItemSync,
                    Key = db,
                    BodyStream = BinarySerializer.ConvertToStream(dt),//CacheMessageStream.EncodeBody(dt),
                    Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.MappingName, mappingName, KnowsArgs.SourceName, mappingName, KnowsArgs.SyncType, ((int)syncType).ToString(), KnowsArgs.SyncTime, ts.ToString())
                }, CacheApiSettings.RemoteDataCacheHostName, CacheApiSettings.EnableRemoteException);

            }
        }
        /// <summary>
        /// SyncCacheApi
        /// </summary>
        public static class SyncCacheApi
        {
            /// <summary>
            /// Refresh specified item in sync cache.
            /// </summary>
            /// <param name="syncName"></param>
            /// <example><code>
            /// //Remove item from sync cache.
            ///public void RemoveItem()
            ///{
            ///    SyncCacheApi.RemoveItem("contactGeneric");
            ///}
            /// </code></example>
            public static void RemoveItem(string syncName)
            {
                using (CacheMessage message = new CacheMessage()
                {
                    Command = SyncCacheCmd.RemoveSyncItem,
                    Key = syncName
                })
                {
                    PipeClientCache.SendOut(message, CacheApiSettings.RemoteCacheManagerHostName, CacheApiSettings.EnableRemoteException);
                }
            }

            /// <summary>
            /// Get item from sync cache using <see cref="CacheKeyInfo"/> an <see cref="Type"/>.
            /// </summary>
            /// <param name="info"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            public static object GetItem(CacheKeyInfo info, Type type)
            {
                using (CacheMessage message = new CacheMessage()
                {
                    Command = SyncCacheCmd.GetSyncItem,
                    Key = info.ToString(),
                    TypeName = type.FullName
                })
                {
                    return PipeClientCache.SendDuplex(message, CacheApiSettings.RemoteCacheManagerHostName, CacheApiSettings.EnableRemoteException);
                }
            }

            /// <summary>
            /// Get entity items report from sync cache using entityName.
            /// </summary>
            /// <param name="entityName"></param>
            /// <returns></returns>
            public static CacheItemReport GetItemsReport(string entityName)
            {
                using (CacheMessage message = new CacheMessage()
                {
                    Command = SyncCacheCmd.GetItemsReport,
                    Key = entityName,
                    TypeName = typeof(ICollection<string>).FullName
                })
                {
                    return PipeClientCache.SendDuplex<CacheItemReport>(message, CacheApiSettings.RemoteCacheManagerHostName, CacheApiSettings.EnableRemoteException);
                }
            }
        }
        /// <summary>
        /// SessionApi
        /// </summary>
        public static class SessionApi
        {

            /// <summary>
            /// Remove session from session cache.
            /// </summary>
            /// <param name="sessionId"></param>
            /// <param name="isAsync"></param>
            /// <example><code>
            ///  //remove session with items.
            ///public void RemoveSession()
            ///{
            ///    SessionApi.RemoveSession(sessionId, false);
            ///}
            /// </code></example>
            public static void RemoveSession(string sessionId, bool isAsync = false)
            {
                PipeClientCache.SendOut(new CacheMessage()
                {
                    Command = SessionCmd.RemoveSession,
                    Id = sessionId,
                    Args = MessageStream.CreateArgs(KnowsArgs.IsAsync, isAsync.ToString())
                }, CacheApiSettings.RemoteCacheManagerHostName);

            }

            /// <summary>
            /// Get existing session in session cache.
            /// </summary>
            /// <param name="sessionId"></param>
            /// <returns></returns>
            public static SessionBagStream GetExistingSession(string sessionId)
            {
                return PipeClientCache.SendDuplex<SessionBagStream>(new CacheMessage()
                {
                    Command = SessionCmd.GetExistingSession,
                    Id = sessionId,
                    Args = MessageStream.CreateArgs(KnowsArgs.ShouldSerialized, "true")
                }, CacheApiSettings.RemoteCacheManagerHostName);
            }
        }

    }
}
