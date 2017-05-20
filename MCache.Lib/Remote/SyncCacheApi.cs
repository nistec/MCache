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
    public class SyncCacheApi : RemoteApi
    {

        //NetProtocol Protocol;

        /// <summary>
        /// Get cache api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static SyncCacheApi Get(NetProtocol protocol = NetProtocol.Tcp)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = CacheApiSettings.Protocol;
            }
            return new SyncCacheApi() { Protocol = protocol };
        }

        private SyncCacheApi()
        {
            RemoteHostName = CacheApiSettings.RemoteSyncCacheHostName;
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
            return RemoteApi.ToJson(stream, format);
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
            return RemoteApi.ToJson(stream, format);
        }


        /// <summary>
        /// Add a new item to sync cache.
        /// </summary>
        /// <param name="entity"></param>
        public void AddItem(SyncEntity entity)
        {
            using (var message = new CacheMessage(SyncCacheCmd.AddSyncEntity, entity.EntityName, entity, 0))
            {
                SendOut(message);
            }
        }
        /// <summary>
        /// Get item from sync cache using <see cref="CacheKeyInfo"/> an <see cref="Type"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetItem(CacheKeyInfo info, Type type)
        {
            return SendDuplex(SyncCacheCmd.GetSyncItem, info.ToString(), type.FullName);
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
            return SendDuplex(SyncCacheCmd.GetSyncItem, CacheKeyInfo.Get(entityName, keys).ToString(), type.FullName);
        }
        /// <summary>
        /// Get item from sync cache using <see cref="CacheKeyInfo"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public T GetItem<T>(CacheKeyInfo info)
        {
            return SendDuplex<T>(SyncCacheCmd.GetSyncItem, info.ToString());
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
            return SendDuplex<T>(SyncCacheCmd.GetSyncItem, CacheKeyInfo.Get(entityName, keys).ToString());
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
            return SendDuplex<GenericRecord>(SyncCacheCmd.GetSyncItem, info.ToString());
        }

        /// <summary>
        /// Get item as <see cref="GenericRecord"/> from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public GenericRecord GetValue(string entityName, string[] keys)//-GenericRecord
        {
            return SendDuplex<GenericRecord>(SyncCacheCmd.GetSyncItem, CacheKeyInfo.Get(entityName, keys).ToString());
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
            return SendDuplex<Dictionary<string, object>>(SyncCacheCmd.GetRecord, info.ToString());
        }
        /// <summary>
        /// Get item as <see cref="IDictionary"/> from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IDictionary GetRecord(string entityName, string[] keys)
        {
            return SendDuplex<Dictionary<string, object>>(SyncCacheCmd.GetRecord, CacheKeyInfo.Get(entityName, keys).ToString());
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
                return SendDuplex<NetStream>(message);
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
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetAs,
                Key = CacheKeyInfo.Get(entityName, keys).ToString(),
                TypeName = typeof(NetStream).FullName,
                Id = entityType.ToString()
            })
            {
                return SendDuplex<NetStream>(message);
            }
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

            NetStream stream = SendDuplex<NetStream>(SyncCacheCmd.GetEntity, info.ToString());
            if (stream == null)
                return default(T);
            stream.Position = 0;
            return BinarySerializer.DeserializeFromStream<T>((NetStream)stream, SerialContextType.GenericEntityAsIEntityType);

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
            SendOut(SyncCacheCmd.Refresh, null);
        }

        /// <summary>
        /// Reset all items in sync cache
        /// </summary>
        public void Reset()
        {
            SendOut(SyncCacheCmd.Reset, null);
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
            SendOut(SyncCacheCmd.RefreshItem, syncName);
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
            SendOut(SyncCacheCmd.RemoveSyncItem, syncName);
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
                SendOut(message);
            }
        }

        /// <summary>
        /// Get entity values as <see cref="EntityStream"/> array from sync cache using entityName.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public GenericKeyValue GetEntityItems(string entityName)
        {
            return SendDuplex<GenericKeyValue>(SyncCacheCmd.GetEntityItems, entityName);
        }

        /// <summary>
        /// Get entity keys from sync cache using entityName.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public ICollection<string> GetEntityKeys(string entityName)
        {
            return SendDuplex<ICollection<string>>(SyncCacheCmd.GetEntityKeys, entityName);
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
                return SendDuplex<ICollection<string>>(message);
            }

            //using (CacheMessage message = new CacheMessage()
            //{
            //    Command = SyncCacheCmd.GetAllEntityNames,
            //    TypeName = typeof(ICollection<string>).FullName
            //})
            //{
            //    switch (Protocol)
            //    {
            //        case NetProtocol.Http:
            //            return (ICollection<string>)HttpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

            //        case NetProtocol.Pipe:
            //            return (ICollection<string>)PipeClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);

            //        case NetProtocol.Tcp:
            //            break;
            //    }
            //    return (ICollection<string>)TcpClientCache.SendDuplex(message, RemoteSyncCacheHostName, EnableRemoteException);
            //}
        }

        /// <summary>
        /// Get entity items report from sync cache using entityName.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public DataTable GetItemsReport(string entityName)
        {
            return (DataTable)SendDuplex(SyncCacheCmd.GetItemsReport, entityName, typeof(ICollection<string>).FullName);
        }
        /// <summary>
        /// Reply for test
        /// </summary>
        /// <returns></returns>
        public string Reply(string text)
        {
            return SendDuplex<string>(SyncCacheCmd.Reply, text);
        }

    }
}
