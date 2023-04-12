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
//using Nistec.Caching.Channels;
using Nistec.Caching.Config;
using Nistec.Caching.Data;
using System.Data;
using Nistec.Data.Entities.Cache;
using Nistec.Runtime;
using Nistec.Generic;
using Nistec.Serialization;
using Nistec.IO;

namespace Nistec.Caching.Remote
{
    /// <summary>
    /// Represent cache managment api for client.
    /// </summary>
    public static class ManagerApi
    {

        #region Send
        static internal T SendDuplexStream<T>(CacheMessage message, Action<string> onFault)
        {
            TransStream ts = PipeClient.SendDuplexStream(message, CacheDefaults.DefaultBundleHostName, CacheApiSettings.EnableRemoteException, System.IO.Pipes.PipeOptions.None);
            if (ts == null)
            {
                onFault(message.Command + " return null!");
                return default(T);
            }
            return ts.ReadValue<T>(onFault);
        }

        static internal object SendDuplexStreamValue(CacheMessage message, Action<string> onFault)
        {
            TransStream ts = PipeClient.SendDuplexStream(message, CacheDefaults.DefaultBundleHostName, CacheApiSettings.EnableRemoteException, System.IO.Pipes.PipeOptions.None);
            if (ts == null)
            {
                onFault(message.Command + " return null!");
                return null;
            }
            return ts.ReadValue(onFault);
        }
        static internal CacheState SendDuplexState(CacheMessage message)
        {
            TransStream ts = PipeClient.SendDuplexStream(message, CacheDefaults.DefaultBundleHostName, CacheApiSettings.EnableRemoteException, System.IO.Pipes.PipeOptions.None);
            if (ts == null)
            {
                return CacheState.InvalidItem;
            }
            return (CacheState)ts.ReadState();
        }

        static internal bool SendDuplexStateBool(CacheMessage message)
        {
            TransStream ts = PipeClient.SendDuplexStream(message, CacheDefaults.DefaultBundleHostName, CacheApiSettings.EnableRemoteException, System.IO.Pipes.PipeOptions.None);
            if (ts == null)
            {
                return false;
            }
            return ts.ReadState()< 100;
        }

        static internal void OnFault(string message)
        {
            Console.WriteLine("CacheApi Fault: " + message);
        }
        #endregion

        #region static client methods
        /// <summary>
        /// Get cache properties as dictionary
        /// </summary>
        /// <returns></returns>
        public static IDictionary CacheProperties()
        {
            return ManagerApi.SendDuplexStream<Hashtable>(new CacheMessage()
            {
                Command = CacheManagerCmd.CacheProperties
            }, OnFault);
        }
        /// <summary>
        /// Get items copy with specified condition.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static CacheEntry[] CloneItems(CloneType ct)
        {
            return ManagerApi.SendDuplexStream<CacheEntry[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.CloneItems,
                Args = NameValueArgs.Create(KnownArgs.CloneType, ((int)ct).ToString())
            }, OnFault);
        }
        /// <summary>
        /// Get all items keys in cache.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllKeys()
        {
            return ManagerApi.SendDuplexStream<string[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllKeys
            }, OnFault);
        }
        /// <summary>
        /// Get all items keys in cache.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllKeysIcons()
        {
            return ManagerApi.SendDuplexStream<string[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllKeysIcons
            }, OnFault);
        }
       

        /// <summary>
        /// Get state counter report of cache.
        /// </summary>
        /// <returns></returns>
        public static DataTable GetStateCounterReport()
        {
            return ManagerApi.SendDuplexStream<DataTable>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetStateCounterReport
            }, OnFault);
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
            return ManagerApi.SendDuplexStream<DataTable>(new CacheMessage()
            {
                Command = cmd
            }, OnFault);
        }

        /// <summary>
        /// Reset PerformanceC ounter.
        /// </summary>
        /// <returns></returns>
        public static void ResetPerformanceCounter()
        {
            PipeClient.SendOut(new CacheMessage()
            {
                Command = CacheManagerCmd.ResetPerformanceCounter
            }, CacheDefaults.DefaultManagerHostName);
        }

        /// <summary>
        /// Get cache performance counter report.
        /// </summary>
        /// <returns></returns>
        public static ICachePerformanceReport GetPerformanceReport()
        {
            return ManagerApi.SendDuplexStream<ICachePerformanceReport>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetPerformanceReport
            }, OnFault);
        }

        /// <summary>
        /// Get cache performance counter report for specified cache agent.
        /// </summary>
        /// <returns></returns>
        public static ICachePerformanceReport GetAgentPerformanceReport(CacheAgentType agentType)
        {
            return ManagerApi.SendDuplexStream<ICachePerformanceReport>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAgentPerformanceReport,
                CustomId = agentType.ToString()
            }, OnFault);
        }

        /// <summary>
        /// Get all items keys in cache.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllDataKeys()
        {
            return ManagerApi.SendDuplexStream<string[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllDataKeys
            }, OnFault);
        }

        /// <summary>
        /// Get all entities in sync cache.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllSyncCacheKeys()
        {
            return ManagerApi.SendDuplexStream<string[]>(new CacheMessage()
            {
                Command = CacheManagerCmd.GetAllSyncCacheKeys
            }, OnFault);
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
        //        Id =db
        //    }, CacheDefaults.DefaultManagerHostName);
        //}
        /// <summary>
        /// Get cache logger log.
        /// </summary>
        /// <returns></returns>
        public static string CacheLog()
        {
            return ManagerApi.SendDuplexStream<string>(new CacheMessage()
            {
                Command = CacheManagerCmd.CacheLog
            }, OnFault);
        }

        /// <summary>
        /// Get all sessions keys in session cache.
        /// </summary>
        /// <returns></returns>
        public static ICollection<string> GetAllSessionsKeys()
        {
            return ManagerApi.SendDuplexStream<ICollection<string>>(new CacheMessage()
            {
                Command = SessionCmd.ViewAllSessionsKeys
            }, OnFault);
        }

        /// <summary>
        /// Get all sessions keys in session cache using <see cref="SessionState"/> state.
        /// </summary>
        /// <returns></returns>
        public static ICollection<string> GetAllSessionsKeysByState(SessionState state)
        {
            return ManagerApi.SendDuplexStream<ICollection<string>>(new CacheMessage()
            {
                Command = SessionCmd.ViewAllSessionsKeysByState,
                Args = NameValueArgs.Create("state", ((int)state).ToString())
            }, OnFault);
        }

        /// <summary>
        /// Get all items keys in specified session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static ICollection<string> GetSessionsItemsKeys(string sessionId)
        {
            return ManagerApi.SendDuplexStream<ICollection<string>>(new CacheMessage()
            {
                Command = SessionCmd.ViewSessionKeys,
                SessionId = sessionId,
            }, OnFault);
        }

        ///// <summary>
        ///// Get entity items report from cache.
        ///// </summary>
        ///// <returns></returns>
        //public static CacheItemReport ReportCacheItems()
        //{
        //    using (CacheMessage message = new CacheMessage()
        //    {
        //        Command = CacheManagerCmd.ReportCacheItems,
        //        Id = "*",
        //       IsDuplex=true
        //    })
        //    {
        //        return PipeClientCache.SendDuplex<CacheItemReport>(message, CacheDefaults.DefaultManagerHostName, CacheApiSettings.EnableRemoteException);
        //    }
        //}

        ///// <summary>
        ///// Get entity items report from session cache.
        ///// </summary>
        ///// <returns></returns>
        //public static CacheItemReport ReportSessionItems()
        //{
        //    using (CacheMessage message = new CacheMessage()
        //    {
        //        Command = CacheManagerCmd.ReportSessionItems,
        //        Id = "*",
        //        IsDuplex = true
        //    })
        //    {
        //        return PipeClientCache.SendDuplex<CacheItemReport>(message, CacheDefaults.DefaultManagerHostName, CacheApiSettings.EnableRemoteException);
        //    }
        //}

        /// <summary>
        /// Get report.
        /// </summary>
        /// <returns></returns>
        public static CacheItemReport Report(string command)
        {
            //case CacheManagerCmd.ReportCacheItems:
            //case CacheManagerCmd.ReportSessionItems:
            //case CacheManagerCmd.ReportCacheTimer:
            //case CacheManagerCmd.ReportSessionTimer:
            //case CacheManagerCmd.ReportSyncBoxItems:
            //case CacheManagerCmd.ReportSyncBoxQueue:
            //case CacheManagerCmd.ReportTimerSyncDispatcher:

            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                CustomId = "*",
                IsDuplex = true
            })
            {
                return ManagerApi.SendDuplexStream<CacheItemReport>(message, OnFault);
            }
        }





        #endregion


        /// <summary>
        /// CacheApi
        /// </summary>
        public static class CacheApi
        {
            ///// <summary>
            ///// Get item value from cache
            ///// </summary>
            ///// <param name="cacheKey"></param>
            ///// <returns>return <see cref="CacheEntry"/></returns>
            //public static CacheEntry GetEntry(string cacheKey)
            //{
            //    return PipeClientCache.SendDuplex<CacheEntry>(new CacheMessage() { Command = CacheCmd.GetEntry, Id = cacheKey },
            //        CacheDefaults.DefaultManagerHostName);
            //}
            /// <summary>
            /// Get item copy from cache
            /// </summary>
            /// <param name="cacheKey"></param>
            /// <returns>return <see cref="CacheEntry"/></returns>
            public static CacheEntry ViewEntry(string cacheKey)
            {
                return SendDuplexStream<CacheEntry>(new CacheMessage() { Command = CacheCmd.ViewEntry, CustomId = cacheKey },
                    OnFault);
            }
            /// <summary>
            /// RemoveItem
            /// </summary>
            /// <param name="cacheKey"></param>
            /// <returns></returns>
            public static CacheState Remove(string cacheKey)
            {
                return (CacheState)SendDuplexState(new CacheMessage() { Command = CacheCmd.Remove, CustomId = cacheKey });
            }
            /// <summary>
            /// Add Item to cache.
            /// </summary>
            /// <param name="cacheKey"></param>
            /// <param name="value"></param>
            /// <param name="expiration"></param>
            /// <returns></returns>
            public static CacheState Add(string cacheKey, object value, int expiration)
            {
                if (value == null)
                    return CacheState.ArgumentsError;
                return SendDuplexState(new CacheMessage(CacheCmd.Add, cacheKey, value, expiration));
            }
            /// <summary>
            /// Add Item to cache.
            /// </summary>
            /// <param name="cacheKey"></param>
            /// <param name="value"></param>
            /// <param name="expiration"></param>
            /// <returns></returns>
            public static CacheState Set(string cacheKey, object value, int expiration)
            {
                if (value == null)
                    return CacheState.ArgumentsError;
                return SendDuplexState(new CacheMessage(CacheCmd.Set, cacheKey, value, expiration));
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
            public static CacheItemProperties GetItemProperties(string db, string tableName)
            {
                return (CacheItemProperties)ManagerApi.SendDuplexStream<CacheItemProperties>(new CacheMessage()
                {
                    Command = DataCacheCmd.GetItemProperties,
                    DbName = db,
                    Label=tableName
                    //Args = NameValueArgs.Create(KnownArgs.TableName, tableName)
                }, OnFault);

            }

            /// <summary>
            /// Remove data table  from storage
            /// </summary>
            /// <param name="db">db name</param>
            /// <param name="tableName">table name</param>
            public static void RemoveTable(string db, string tableName)
            {
                PipeClient.SendOut(new CacheMessage()
                {
                    Command = DataCacheCmd.RemoveTable,
                    DbName = db,
                    Label=tableName
                    //Args = NameValueArgs.Create(KnownArgs.TableName, tableName)
                }, CacheDefaults.DefaultManagerHostName);
            }
            
            /// <summary>
            /// Get DataTable from storage by table name.
            /// </summary>
            /// <param name="db">db name</param>
            /// <param name="tableName">table name</param>
            /// <returns>DataTable</returns>
            public static DbTable GetTable(string db, string tableName)
            {
                return (DbTable)ManagerApi.SendDuplexStream<DbTable>(new CacheMessage()
                {
                    Command = DataCacheCmd.GetTable,
                    DbName = db,
                    Label= tableName
                    //Args = NameValueArgs.Create(KnownArgs.TableName, tableName)
                },OnFault);
            }

            ///// <summary>
            ///// Add Remoting Data Item to cache
            ///// </summary>
            ///// <param name="db"></param>
            ///// <param name="dt"></param>
            ///// <param name="tableName"></param>
            //public static bool AddDataItem(string db, DataTable dt, string tableName)
            //{
            //    return ManagerApi.SendDuplexStateBool(new CacheMessage()
            //    {
            //        Command = DataCacheCmd.AddTable,
            //        DbName = db,
            //        Label=tableName,
            //        BodyStream = BinarySerializer.ConvertToStream(dt),// CacheMessageStream.EncodeBody(dt),
            //        Args = NameValueArgs.Create(KnownArgs.MappingName, tableName, KnownArgs.SourceType, EntitySourceType.Table.ToString())
            //    });
            //}

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
            public static void AddTableWithSync(string db, DataTable dt, string tableName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan ts)
            {
                PipeClient.SendOut(new CacheMessage()//BinarySerializer.ConvertToStream(dt))//(MessageStream.GetTypeName(dt), BinarySerializer.ConvertToStream(dt))
                {
                    Command = DataCacheCmd.AddTableWithSync,
                    DbName = db,
                    Label=tableName,
                    BodyStream = MessageStream.SerializeBody(dt),
                    TypeName = Types.GetTypeName(dt),
                    Args = NameValueArgs.Create(KnownArgs.MappingName, mappingName, KnownArgs.SourceName, NameValueArgs.JoinArg(sourceName), KnownArgs.SyncType, ((int)syncType).ToString(), KnownArgs.SyncTime, ts.ToString())
                }, CacheDefaults.DefaultBundleHostName, CacheApiSettings.EnableRemoteException);
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
            public static void AddTableWithSync(string db, DataTable dt, string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts)
            {
                PipeClient.SendOut(new CacheMessage()//BinarySerializer.ConvertToStream(dt))//(MessageStream.GetTypeName(dt), BinarySerializer.ConvertToStream(dt))
                {
                    Command = DataCacheCmd.AddTableWithSync,
                    DbName = db,
                    Label=tableName,
                    BodyStream = MessageStream.SerializeBody(dt),
                    TypeName = Types.GetTypeName(dt),
                    Args = NameValueArgs.Create(KnownArgs.MappingName, mappingName, KnownArgs.SourceName, mappingName, KnownArgs.SyncType, ((int)syncType).ToString(), KnownArgs.SyncTime, ts.ToString())
                }, CacheDefaults.DefaultBundleHostName, CacheApiSettings.EnableRemoteException);

            }
            /// <summary>
            /// Get entity items report from sync cache using entityName.
            /// </summary>
            /// <param name="db"></param>
            /// <param name="tableName"></param>
            /// <returns></returns>
            public static CacheItemReport GetItemsReport(string db, string tableName)
            {
                using (CacheMessage message = new CacheMessage()
                {
                    Command = DataCacheCmd.GetItemsReport,
                    DbName = db,
                    Label=tableName
                    //TypeName = typeof(ICollection<string>).FullName
                })
                {
                    return ManagerApi.SendDuplexStream<CacheItemReport>(message, OnFault);
                }
            }

            public static IDictionary GetRecord(string db, string tableName, string primaryKey)
            {
                if (string.IsNullOrWhiteSpace(db))
                {
                    throw new ArgumentNullException("db is required");
                }
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    throw new ArgumentNullException("tableName is required");
                }

                using (var message = new CacheMessage()
                {
                    Command = DataCacheCmd.GetRecord,
                    DbName = db,
                    Label=tableName,
                    CustomId = primaryKey,
                    Args = NameValueArgs.Create(KnownArgs.TableName, tableName, KnownArgs.Pk, primaryKey)
                })
                {
                    return ManagerApi.SendDuplexStream<IDictionary>(message, OnFault);
                }
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
            public static void Remove(string syncName)
            {
                using (CacheMessage message = new CacheMessage()
                {
                    Command = SyncCacheCmd.Remove,
                    Label = syncName
                })
                {
                    PipeClient.SendOut(message, CacheDefaults.DefaultManagerHostName, CacheApiSettings.EnableRemoteException);
                }
            }

            /// <summary>
            /// Get item from sync cache using <see cref="ComplexKey"/> an <see cref="Type"/>.
            /// </summary>
            /// <param name="entityName"></param>
            /// <param name="primaryKey"></param>
            /// <returns></returns>
            public static GenericRecord GetAs(string entityName, string primaryKey)
            {
                using (CacheMessage message = new CacheMessage()
                {
                    Command = SyncCacheCmd.GetAs,
                    Label = entityName,
                    CustomId = primaryKey
                    //TypeName = type.FullName,
                    //Args = NameValueArgs.Create(KnownArgs.Column, field)
                })
                {
                    return ManagerApi.SendDuplexStream<GenericRecord>(message, OnFault);
                }
            }

            /// <summary>
            /// Get item from sync cache using <see cref="ComplexKey"/> an <see cref="Type"/>.
            /// </summary>
            /// <param name="entityName"></param>
            /// <param name="primaryKey"></param>
            /// <returns></returns>
            public static object GetRecord(string entityName, string primaryKey)
            {
                using (CacheMessage message = new CacheMessage()
                {
                    Command = SyncCacheCmd.GetRecord,
                    Label = entityName,
                    CustomId = primaryKey
                })
                {
                    return ManagerApi.SendDuplexStreamValue(message, OnFault);
                }

            }
            /// <summary>
            /// Get entity items report from sync cache using entityName.
            /// </summary>
            /// <param name="entityName"></param>
            /// <returns></returns>
            public static CacheItemReport GetItemsReport(string entityName)
            {
                using (CacheMessage message = new CacheMessage()//(typeof(ICollection<string>).FullName, (NetStream)null)
                {
                    Command = SyncCacheCmd.GetItemsReport,
                    Label = entityName
                    //TypeName = typeof(ICollection<string>).FullName
                })
                {
                    return ManagerApi.SendDuplexStream<CacheItemReport>(message, OnFault);
                }
            }

            /// <summary>
            /// GetItemProperties
            /// </summary>
            /// <param name="entityName"></param>
            /// <returns></returns>
            public static CacheItemProperties GetItemProperties(string entityName)
            {
                return (CacheItemProperties)ManagerApi.SendDuplexStream<CacheItemProperties>(new CacheMessage()
                {
                    Command = SyncCacheCmd.GetItemProperties,
                    Label = entityName
                }, OnFault);

            }
        }
        /// <summary>
        /// SessionApi
        /// </summary>
        public static class SessionApi
        {
            public static SessionEntry ViewSessionItem(string sessionId, string key)
            {
                SessionEntry entry= ManagerApi.SendDuplexStream<SessionEntry>(new CacheMessage() { Command = SessionCmd.ViewEntry, SessionId = sessionId, CustomId = key },
                    OnFault);
                //if (entry != null)
                //    entry.Body= entry.DecodeBody();
                return entry;
            }

            /// <summary>
            /// Remove session from session cache.
            /// </summary>
            /// <param name="sessionId"></param>
            /// <example><code>
            ///  //remove session with items.
            ///public void RemoveSession()
            ///{
            ///    SessionApi.RemoveSession(sessionId, false);
            ///}
            /// </code></example>
            public static void RemoveSession(string sessionId)
            {
                PipeClient.SendOut(new CacheMessage()
                {
                    Command = SessionCmd.RemoveSession,
                    SessionId = sessionId
                    //Args = NameValueArgs.Create(KnownArgs.IsAsync, isAsync.ToString())
                }, CacheDefaults.DefaultManagerHostName);

            }

            /// <summary>
            /// Get existing session in session cache.
            /// </summary>
            /// <param name="sessionId"></param>
            /// <returns></returns>
            public static SessionBagStream ViewExistingSession(string sessionId)
            {
                return ManagerApi.SendDuplexStream<SessionBagStream>(new CacheMessage()
                {
                    Command = SessionCmd.ViewSessionStream,
                    SessionId = sessionId,
                    Args = NameValueArgs.Create(KnownArgs.ShouldSerialized, "true")
                }, OnFault);
            }
        }

    }
}
