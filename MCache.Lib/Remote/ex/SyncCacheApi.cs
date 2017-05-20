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
using Nistec.Caching.Sync;
using Nistec.Data.Entities.Cache;
using System.Collections;
using Nistec.Runtime;
using Nistec.Channels;
using Nistec.Generic;
using System.Data;
using Nistec.Caching.Channels;
using Nistec.Caching.Config;
using Nistec.Caching.Data;
using Nistec.IO;
using Nistec.Serialization;

namespace Nistec.Caching.Remote
{
    /// <summary>
    /// Represent sync cache api for client.
    /// </summary>
    public class SyncCacheApi 
    {

        NetProtocol Protocol;
        /// <summary>
        /// Get cache api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static SyncCacheApi Get(NetProtocol protocol = NetProtocol.Tcp)
        {
            return new SyncCacheApi() { Protocol = protocol };
        }

        string RemoteSyncCacheHostName;
        bool EnableRemoteException;

        private SyncCacheApi()
        {
            RemoteSyncCacheHostName = CacheApiSettings.RemoteSyncCacheHostName;
            EnableRemoteException = CacheApiSettings.EnableRemoteException;
        }

        /// <summary>
        /// Get item value from cache as json.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetJson(CacheKeyInfo info, JsonFormat format)
        {
            var stream = GetAs(info);
            return CacheApi.ToJson(stream, format);
        }

        /// <summary>
        /// Get item value from cache as json.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetJson(string entityName, string[] keys, JsonFormat format)
        {
            var stream = GetAs(entityName, keys);
            return CacheApi.ToJson(stream, format);
        }


        /// <summary>
        /// Add a new item to sync cache.
        /// </summary>
        /// <param name="entity"></param>
        public void AddItem(SyncEntity entity)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClientCache.SendOut(new CacheMessage(SyncCacheCmd.AddSyncEntity, entity.EntityName, entity, 0)
                    , RemoteSyncCacheHostName, EnableRemoteException);
                    return;
                case NetProtocol.Pipe:
                    PipeClientCache.SendOut(new CacheMessage(SyncCacheCmd.AddSyncEntity, entity.EntityName, entity, 0)
                    , RemoteSyncCacheHostName, EnableRemoteException);
                    return;
                case NetProtocol.Tcp:
                    break;
            }
            TcpClientCache.SendOut(new CacheMessage(SyncCacheCmd.AddSyncEntity, entity.EntityName, entity,0)
            , RemoteSyncCacheHostName, EnableRemoteException);
        }
        /// <summary>
        /// Get item from sync cache using <see cref="CacheKeyInfo"/> an <see cref="Type"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetItem(CacheKeyInfo info, Type type)
        {
            
            using (CacheMessage message = new CacheMessage()
             {
                 Command = SyncCacheCmd.GetSyncItem,
                 Key = info.ToString(),
                 TypeName = type.FullName
             })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }
        /// <summary>
        /// Get item from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetItem(string entityName, string[] keys, Type type)
        {
            return GetItem(CacheKeyInfo.Get(entityName, keys), type);
        }
        /// <summary>
        /// Get item from sync cache using <see cref="CacheKeyInfo"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public T GetItem<T>(CacheKeyInfo info)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetSyncItem,
                Key = info.ToString(),
                TypeName = typeof(T).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex<T>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex<T>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex<T>(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }
        /// <summary>
        /// Get item from sync cache using arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T GetItem<T>(string entityName, string[] keys)
        {
            return GetItem<T>(CacheKeyInfo.Get(entityName, keys));
        }

        /// <summary>
        /// Get item as <see cref="GenericRecord"/> from sync cache using <see cref="CacheKeyInfo"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <example><code>
        ///  //Get item value from sync cache.
        ///public void GetValue()
        ///{
        ///    string key = "1";
        ///    var item = <![CDATA[SyncCacheApi.GetItem<GenericRecord>(CacheKeyInfo.Get("contactEntity", new string[] { "1" }));]]>
        ///    if (item == null)
        ///        Console.WriteLine("item not found " + key);
        ///    else
        ///        Console.WriteLine(item["FirstName"]);
        ///    //convert to entity
        ///    ContactEntity entity = new <![CDATA[EntityContext<ContactEntity>(item).Entity;]]>
        ///    Console.WriteLine(entity.FirstName);
        ///}
        /// </code></example>
        public GenericRecord GetValue(CacheKeyInfo info)//-GenericRecord
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetSyncItem,//.GetSyncValue,
                Key = info.ToString(),
                TypeName = typeof(GenericRecord).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex<GenericRecord>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex<GenericRecord>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex<GenericRecord>(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }

        /// <summary>
        /// Get item as <see cref="GenericRecord"/> from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public GenericRecord GetValue(string entityName, string[] keys)//-GenericRecord
        {
            return GetValue(CacheKeyInfo.Get(entityName, keys));
        }

        /// <summary>
        /// Get item as <see cref="IDictionary"/> from sync cache using <see cref="CacheKeyInfo"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Get item value from sync cache as Dictionary.
        ///public void GetRecord()
        ///{
        ///    string key = "1";
        ///    var item = SyncCacheApi.GetRecord(CacheKeyInfo.Get("contactEntity", new string[] { "1" }));
        ///    if (item == null)
        ///        Console.WriteLine("item not found " + key);
        ///    else
        ///        Console.WriteLine(item["FirstName"]);
        ///}
        /// </code></example>
        public IDictionary GetRecord(CacheKeyInfo info)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetRecord,
                Key = info.ToString(),
                TypeName = typeof(Dictionary<string, object>).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex<IDictionary>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex<IDictionary>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex<IDictionary>(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }
        /// <summary>
        /// Get item as <see cref="IDictionary"/> from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IDictionary GetRecord(string entityName, string[] keys)
        {
            return GetRecord(CacheKeyInfo.Get(entityName, keys));
        }
 
        /// <summary>
        /// Get item as <see cref="NetStream"/> from sync cache using <see cref="CacheKeyInfo"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public NetStream GetAs(CacheKeyInfo info, CacheEntityTypes entityType = CacheEntityTypes.GenericEntity)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetAs,
                Key = info.ToString(),
                TypeName = typeof(NetStream).FullName,
                Id = entityType.ToString()
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex<NetStream>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex<NetStream>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex<NetStream>(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }

        /// <summary>
        /// Get item as <see cref="NetStream"/> from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public NetStream GetAs(string entityName, string[] keys, CacheEntityTypes entityType = CacheEntityTypes.GenericEntity)
        {
            return GetAs(CacheKeyInfo.Get(entityName, keys), entityType);
        }


        /// <summary>
        /// Get item as entity from sync cache using <see cref="CacheKeyInfo"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Get item value from sync cache as Entity.
        ///public void GetEntity()
        ///{
        ///    string key = "1";
        ///    var item = <![CDATA[SyncCacheApi.GetEntity<ContactEntity>(CacheKeyInfo.Get("contactEntity", new string[] { "1" }));]]>
        ///    if (item == null)
        ///        Console.WriteLine("item not found " + key);
        ///    else
        ///        Console.WriteLine(item.FirstName);
        ///}
        /// </code></example>
        public T GetEntity<T>(CacheKeyInfo info)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetEntity,
                Key = info.ToString(),
                TypeName = typeof(T).FullName
            })
            {
                NetStream stream = null;

                switch (Protocol)
                {
                    case NetProtocol.Http:
                        stream = HttpClientCache.SendDuplex<NetStream>(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        stream = PipeClientCache.SendDuplex<NetStream>(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        stream = TcpClientCache.SendDuplex<NetStream>(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                }
                if (stream == null)
                    return default(T);
                stream.Position = 0;
                return BinarySerializer.DeserializeFromStream<T>((NetStream)stream, SerialContextType.GenericEntityAsIEntityType);


                //return TcpClientCache.SendDuplex<T>(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }

        /// <summary>
        /// Get item as entity from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T GetEntity<T>(string entityName, string[] keys)
        {
            return GetEntity<T>(CacheKeyInfo.Get(entityName, keys));
        }
        
        /// <summary>
        /// Refresh all items in sync cache
        /// </summary>
        public void Refresh()
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.Refresh
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        HttpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        PipeClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        TcpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                }
            }
        }
        /// <summary>
        /// Reset all items in sync cache
        /// </summary>
        public void Reset()
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.Reset
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        HttpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        PipeClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        TcpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                }
            }
        }
        /// <summary>
        /// Refresh specified item in sync cache.
        /// </summary>
        /// <param name="syncName"></param>
        /// <example><code>
        /// //Refresh sync item which mean reload sync item from Db.
        ///public void RefreshItem()
        ///{
        ///    SyncCacheApi.Refresh("contactGeneric");
        ///}
        /// </code></example>
        public void Refresh(string syncName)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.RefreshItem,
                Key = syncName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        HttpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        PipeClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        TcpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                }
            }
        }
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
        public void RemoveItem(string syncName)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.RemoveSyncItem,
                Key = syncName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        HttpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        PipeClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        TcpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                }
            }
        }
        /// <summary>
        /// Add a new item to sync cache.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="ts"></param>
        /// <param name="sourceType"></param>
        /// <param name="entityKeys"></param>
        /// <example>
        /// <code>
        /// //Add items to remote cache.
        ///public void AddItems()
        ///{
        ///    SyncCacheApi.AddItem("AdventureWorks",
        ///        "contactGeneric",
        ///        "Person.Contact",
        ///        new string[] { "Person.Contact" },
        ///        SyncType.Interval,
        ///        TimeSpan.FromMinutes(10),
        ///        EntitySourceType.Table,
        ///        new string[] { "ContactID" });
        ///    SyncCacheApi.AddItem("AdventureWorks",
        ///       "contactEntity",
        ///       "Person.Contact",
        ///       new string[] { "Person.Contact" },
        ///       SyncType.Interval,
        ///       TimeSpan.FromMinutes(10),
        ///       EntitySourceType.Table,
        ///       new string[] { "ContactID" });
        ///}
        /// </code>
        /// </example>
        public void AddItem(string db, string tableName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan ts, EntitySourceType sourceType, string[] entityKeys)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.AddSyncItem,
                Key = db,
                Args = MessageStream.CreateArgs(
                KnowsArgs.ConnectionKey, db,
                KnowsArgs.TableName, tableName,
                KnowsArgs.MappingName, mappingName,
                KnowsArgs.SourceName, GenericNameValue.JoinArg(sourceName),
                KnowsArgs.SyncType, ((int)syncType).ToString(),
                KnowsArgs.SyncTime, ts.ToString(),
                KnowsArgs.EntityKeys, GenericNameValue.JoinArg(entityKeys),
                KnowsArgs.SourceType, ((int)sourceType).ToString())//KnowsArgs.EntityType,entityType)
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        HttpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        PipeClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        TcpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                }
            }
        }

        /// <summary>
        /// Get entity values as <see cref="EntityStream"/> array from sync cache using entityName.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public GenericKeyValue GetEntityItems(string entityName)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetEntityItems,
                Key = entityName,
                TypeName = typeof(GenericKeyValue).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return (GenericKeyValue)HttpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return (GenericKeyValue)PipeClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return (GenericKeyValue)TcpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }

        /// <summary>
        /// Get entity keys from sync cache using entityName.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public ICollection<string> GetEntityKeys(string entityName)
        {
            using (CacheMessage message = new CacheMessage()
            {                          
                Command = SyncCacheCmd.GetEntityKeys,
                Key = entityName,
                TypeName = typeof(ICollection<string>).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return (ICollection<string>)HttpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return (ICollection<string>)PipeClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return (ICollection<string>)TcpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }

        /// <summary>
        /// Get all entity names from sync cache.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllEntityNames()
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetAllEntityNames,
                TypeName = typeof(ICollection<string>).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return (ICollection<string>)HttpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return (ICollection<string>)PipeClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return (ICollection<string>)TcpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }

        /// <summary>
        /// Get entity items report from sync cache using entityName.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public DataTable GetItemsReport(string entityName)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetItemsReport,
                Key = entityName,
                TypeName = typeof(ICollection<string>).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return (DataTable)HttpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return (DataTable)PipeClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return (DataTable)TcpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }
        /// <summary>
        /// Reply for test
        /// </summary>
        /// <returns></returns>
        public string Reply()
        {

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex<string>(new CacheMessage() { Command = SyncCacheCmd.Reply, Key = "Reply" },
                    RemoteSyncCacheHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex<string>(new CacheMessage() { Command = SyncCacheCmd.Reply, Key = "Reply" },
                    RemoteSyncCacheHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex<string>(new CacheMessage() { Command = SyncCacheCmd.Reply, Key = "Reply" },
               RemoteSyncCacheHostName, EnableRemoteException);

        }


        #region internal
        
        internal T SendDuplex<T>(string command,string key, NetProtocol Protocol)
        {

            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                Key = key,
                TypeName = typeof(T).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex<T>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex<T>(message, RemoteSyncCacheHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex<T>(message, RemoteSyncCacheHostName, EnableRemoteException);
            }
        }
        internal void SendOut(string command, string key, GenericNameValue args, NetProtocol Protocol)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                Key = key
            })
            {
                if (args != null)
                    message.Args = args;

                switch (Protocol)
                {
                    case NetProtocol.Http:
                        HttpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        PipeClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        TcpClientCache.SendOut(message, RemoteSyncCacheHostName, EnableRemoteException);
                        break;
                }
            }
        }
        #endregion

    }
}
