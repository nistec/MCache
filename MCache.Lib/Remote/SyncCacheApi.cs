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
//using Nistec.Caching.Channels;
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
            RemoteHostName = CacheDefaults.DefaultBundleHostName;
            EnableRemoteException = CacheApiSettings.EnableRemoteException;
        }
        protected void OnFault(string message)
        {
            Console.WriteLine("SyncCacheApi Fault: " + message);
        }

        #region do custom
        public object DoCustom(string command, string entityName, string primaryKey, string field = null,  object value = null, string kvArgs = null)
        {
            string[] arrayArgs = null;
            if (!string.IsNullOrEmpty(kvArgs))
            {
                arrayArgs = KeySet.SplitTrim(kvArgs);
            }

            if (primaryKey != null)
            {
                primaryKey = primaryKey.Replace(",", KeySet.Separator);
            }

            switch ("sync_" + command)
            {
                case SyncCacheCmd.AddEntity:
                    //return AddEntity(itemName, tableName);
                    return CacheState.CommandNotSupported;
                case SyncCacheCmd.AddSyncItem:
                    //return AddSyncItem(itemName, tableName);
                    return CacheState.CommandNotSupported;
                case SyncCacheCmd.Contains:
                    return Contains(ComplexArgs.Parse(entityName));
                case SyncCacheCmd.Get:
                    return Get(ComplexKey.Get(entityName, primaryKey), field);
                case SyncCacheCmd.GetAllEntityNames:
                    return GetAllEntityNames();
                case SyncCacheCmd.GetAs:
                    return GetAs(ComplexKey.Get(entityName,primaryKey));
                case SyncCacheCmd.GetEntity:
                    return GetEntity<object>(ComplexKey.Get(entityName, primaryKey));
                case SyncCacheCmd.GetEntityItems:
                    return GetEntityItems(entityName);
                case SyncCacheCmd.GetEntityItemsCount:
                    return GetEntityItemsCount(entityName);
                case SyncCacheCmd.GetEntityKeys:
                    return GetEntityKeys(entityName);
                case SyncCacheCmd.GetItemProperties:
                    return GetItemProperties(entityName);
                case SyncCacheCmd.GetItemsReport:
                    return GetItemsReport(entityName);
                case SyncCacheCmd.GetRecord:
                    return GetRecord(ComplexKey.Get(entityName, primaryKey));
                case SyncCacheCmd.Refresh:
                    Refresh(entityName);
                    return CacheState.Ok;
                case SyncCacheCmd.RefreshAll:
                    RefreshAll();
                    return CacheState.Ok;
                case SyncCacheCmd.Remove:
                    Remove(entityName);
                    return CacheState.Ok;
                case SyncCacheCmd.Reply:
                    return Reply(entityName);
                case SyncCacheCmd.Reset:
                    Reset();
                    return CacheState.Ok;
                default:
                    throw new ArgumentException("Unknown command " + command);
            }
        }


        public string DoHttpJson(string command, string entityName,string primaryKey, string field=null, object value = null, string kvArgs = null, bool pretty = false)
        {
            string[] arrayArgs = null;
            if (!string.IsNullOrEmpty(kvArgs))
            {
                arrayArgs = KeySet.SplitTrim(kvArgs);
            }

            if (primaryKey != null)
            {
                primaryKey = primaryKey.Replace(",", KeySet.Separator);
            }

            string cmd = "sync_" + command.ToLower();
            switch (cmd)
            {
                case SyncCacheCmd.AddEntity:
                    //return AddEntity(itemName, tableName);
                    return CacheState.CommandNotSupported.ToString();
                case SyncCacheCmd.AddSyncItem:
                    //return AddSyncItem(itemName, tableName);
                    return CacheState.CommandNotSupported.ToString();
                case SyncCacheCmd.Get:
                    {
                        if (string.IsNullOrWhiteSpace(entityName))
                        {
                            throw new ArgumentNullException("entityName is required");
                        }
                        //var ck = ComplexArgs.Parse(itemName);
                        return SendHttpJsonDuplex(new CacheMessage() { Command= cmd, Label = entityName, CustomId = primaryKey, Args= NameValueArgs.Create(KnownArgs.Column, field) }, pretty);
                    }
                case SyncCacheCmd.GetAllEntityNames:
                    {
                        return SendHttpJsonDuplex(new CacheMessage() { Command = cmd}, pretty);
                    }
                case SyncCacheCmd.Contains:
                case SyncCacheCmd.GetRecord:
                case SyncCacheCmd.GetAs:
                case SyncCacheCmd.GetEntity:
                    {
                        if (string.IsNullOrWhiteSpace(entityName))
                        {
                            throw new ArgumentNullException("entityName is required");
                        }
                        //var ck = ComplexArgs.Parse(itemName);
                        //return SendJsonDuplex(cmd, ComplexKey.Get(entityName, primeryKey), pretty);
                        return SendHttpJsonDuplex(new CacheMessage() { Command = cmd, Label = entityName, CustomId = primaryKey }, pretty);
                    }
                case SyncCacheCmd.FindEntity:
                    {
                        if (string.IsNullOrWhiteSpace(entityName))
                        {
                            throw new ArgumentNullException("entityName is required");
                        }
                        return SendHttpJsonDuplex(new CacheMessage() { Command=cmd, Label = entityName, Args= NameValueArgs.Create(arrayArgs) });
                    }
                case SyncCacheCmd.GetEntityPrimaryKey:
                case SyncCacheCmd.GetItemsReport:
                case SyncCacheCmd.GetItemProperties:
                case SyncCacheCmd.GetEntityKeys:
                case SyncCacheCmd.GetEntityItemsCount:
                case SyncCacheCmd.GetEntityItems:
                    {
                        if (string.IsNullOrWhiteSpace(entityName))
                        {
                            throw new ArgumentNullException("entityName is required");
                        }
                        return SendHttpJsonDuplex(new CacheMessage() { Command = cmd, Label = entityName }, pretty);
                    }
                case SyncCacheCmd.Reply:
                case SyncCacheCmd.Remove:
                case SyncCacheCmd.Refresh:
                    SendHttpJsonDuplex(new CacheMessage() { Command = cmd, Label = entityName });
                    return CacheState.Ok.ToString();
                case SyncCacheCmd.Reset:
                case SyncCacheCmd.RefreshAll:
                    SendHttpJsonDuplex(new CacheMessage() { Command = cmd });
                    return CacheState.Ok.ToString();
                default:
                    throw new ArgumentException("Unknown command " + command);
            }
        }

        #endregion

        /// <summary>
        /// Get item value from cache as json.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetJson(ComplexKey info, JsonFormat format)
        {
            if (info == null)
            {
                throw new ArgumentNullException("SyncEntity is required");
            }
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
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys is required");
            }
            var stream = GetAs(entityName, keys);
            return RemoteApi.ToJson(stream, format);
        }


        /// <summary>
        /// Add a new item to sync cache.
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(SyncEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("SyncEntity is required");
            }
            using (var message = new CacheMessage(SyncCacheCmd.AddEntity, entity.EntityName, entity, 0))
            {
                SendOut(message);
            }
        }
        /// <summary>
        /// Get item value from sync cache using <see cref="ComplexKey"/> an <see cref="Type"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public object Get(ComplexKey info, string field)
        {
            if (info == null)
            {
                throw new ArgumentNullException("ComplexKey is required");
            }
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.Get,
                Label = info.Prefix,// info.ToString(),
                CustomId = info.Suffix,
                Args = NameValueArgs.Create(KnownArgs.Column,field)
            })
            {
                return SendDuplexStreamValue(message, OnFault);
            }
         }
        /// <summary>
        /// Get item value from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public object Get(string entityName, string[] keys,  string field)//, Type type)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.Get,
                Label = entityName,// ComplexArgs.GetInfo(entityName, keys),
                CustomId = KeySet.Join(keys),
                Args = NameValueArgs.Create(KnownArgs.Column, field)
            })
            {
                return SendDuplexStreamValue(message, OnFault);
            }
        }

        /// <summary>
        /// Get item value from sync cache using <see cref="ComplexKey"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public T Get<T>(ComplexKey info, string field)
        {
            if (info == null)
            {
                throw new ArgumentNullException("ComplexKey is required");
            }
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.Get,
                Label = info.Prefix,
                CustomId = info.Suffix,
                Args = NameValueArgs.Create(KnownArgs.Column, field)
            })
            {
                return SendDuplexStream<T>(message, OnFault);
            }
        }
        /// <summary>
        /// Get item value from sync cache using arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T Get<T>(string entityName, string[] keys, string field)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys is required");
            }
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.Get,
                Label = entityName,//ComplexArgs.GetInfo(entityName, keys)
                CustomId = KeySet.Join(keys),
                Args = NameValueArgs.Create(KnownArgs.Column, field)
            })
            {
                return SendDuplexStream<T>(message, OnFault);
            }
        }

        ///// <summary>
        ///// Get item as <see cref="GenericRecord"/> from sync cache using <see cref="ComplexKey"/>.
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        ///// <example><code>
        /////  //Get item value from sync cache.
        /////public void GetValue()
        /////{
        /////    string key = "1";
        /////    var item = <![CDATA[SyncCacheApi.GetItem<GenericRecord>(ComplexKey.Get("contactEntity", new string[] { "1" }));]]>
        /////    if (item == null)
        /////        Console.WriteLine("item not found " + key);
        /////    else
        /////        Console.WriteLine(item["FirstName"]);
        /////    //convert to entity
        /////    ContactEntity entity = new <![CDATA[EntityContext<ContactEntity>(item).Entity;]]>
        /////    Console.WriteLine(entity.FirstName);
        /////}
        ///// </code></example>
        //public GenericRecord GetValue(ComplexKey info)//-GenericRecord
        //{
        //    if (info == null)
        //    {
        //        throw new ArgumentNullException("ComplexKey is required");
        //    }
        //    //return SendDuplex<GenericRecord>(SyncCacheCmd.GetSyncItem, info.ToString());
        //    using (var message = new CacheMessage()
        //    {
        //        Command = SyncCacheCmd.GetSyncValue,
        //        Id = info.ToString()
        //    })
        //    {
        //        return SendDuplexStream<GenericRecord>(message, OnFault);
        //    }

        //}

        ///// <summary>
        ///// Get item as <see cref="GenericRecord"/> from sync cache using arguments.
        ///// </summary>
        ///// <param name="entityName"></param>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public GenericRecord GetValue(string entityName, string[] keys)//-GenericRecord
        //{
        //    if (string.IsNullOrWhiteSpace(entityName))
        //    {
        //        throw new ArgumentNullException("entityName is required");
        //    }
        //    if (keys == null)
        //    {
        //        throw new ArgumentNullException("keys is required");
        //    }
        //    //return SendDuplex<GenericRecord>(SyncCacheCmd.GetSyncItem, ComplexKey.Get(entityName, keys).ToString());
        //    using (var message = new CacheMessage()
        //    {
        //        Command = SyncCacheCmd.GetSyncValue,
        //        Id = ComplexKey.Get(entityName, keys).ToString()
        //    })
        //    {
        //        return SendDuplexStream<GenericRecord>(message, OnFault);
        //    }

        //}

        /// <summary>
        /// Get item as <see cref="IDictionary"/> from sync cache using <see cref="ComplexKey"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Get item value from sync cache as Dictionary.
        ///public void GetRecord()
        ///{
        ///    string key = "1";
        ///    var item = SyncCacheApi.GetRecord(ComplexKey.Get("contactEntity", new string[] { "1" }));
        ///    if (item == null)
        ///        Console.WriteLine("item not found " + key);
        ///    else
        ///        Console.WriteLine(item["FirstName"]);
        ///}
        /// </code></example>
        public IDictionary GetRecord(ComplexKey info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("ComplexKey is required");
            }
            //return SendDuplex<Dictionary<string, object>>(SyncCacheCmd.GetRecord, info.ToString());
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetRecord,
                Label = info.Prefix,// info.ToString(),
                CustomId = info.Suffix
            })
            {
                return SendDuplexStream<Dictionary<string, object>>(message, OnFault);
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
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys is required");
            }
            //return SendDuplex<Dictionary<string, object>>(SyncCacheCmd.GetRecord, ComplexKey.Get(entityName, keys).ToString());
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetRecord,
                Label = entityName,
                CustomId = KeySet.Join(keys)
            })
            {
                return SendDuplexStream<Dictionary<string, object>>(message, OnFault);
            }
        }
 
        /// <summary>
        /// Get item as <see cref="NetStream"/> from sync cache using <see cref="ComplexKey"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public NetStream GetAs(ComplexKey info)//, CacheEntityTypes entityType = CacheEntityTypes.GenericEntity)
        {
            if (info == null)
            {
                throw new ArgumentNullException("ComplexKey is required");
            }
            using (CacheMessage message = new CacheMessage()//(typeof(NetStream).FullName, (NetStream)null)
            {
                Command = SyncCacheCmd.GetAs,
                Label = info.Prefix,
                //TypeName = typeof(NetStream).FullName,
                CustomId = info.Suffix
            })
            {
                return SendDuplexStream<NetStream>(message, OnFault);
            }
        }

        /// <summary>
        /// Get item as <see cref="NetStream"/> from sync cache using arguments.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public NetStream GetAs(string entityName, string[] keys)//, CacheEntityTypes entityType = CacheEntityTypes.GenericEntity)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys is required");
            }
            using (CacheMessage message = new CacheMessage()//typeof(NetStream).FullName, (NetStream)null)
            {
                Command = SyncCacheCmd.GetAs,
                Label = entityName,
                //TypeName = typeof(NetStream).FullName,
                CustomId = KeySet.Join(keys)
            })
            {
                return SendDuplexStream<NetStream>(message, OnFault);
            }
        }


        /// <summary>
        /// Get item as entity from sync cache using <see cref="ComplexKey"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <example><code>
        /// //Get item value from sync cache as Entity.
        ///public void GetEntity()
        ///{
        ///    string key = "1";
        ///    var item = <![CDATA[SyncCacheApi.GetEntity<ContactEntity>(ComplexKey.Get("contactEntity", new string[] { "1" }));]]>
        ///    if (item == null)
        ///        Console.WriteLine("item not found " + key);
        ///    else
        ///        Console.WriteLine(item.FirstName);
        ///}
        /// </code></example>
        public T GetEntity<T>(ComplexKey info)
        {
            if (info==null)
            {
                throw new ArgumentNullException("ComplexKey is required");
            }
            //NetStream stream = SendDuplex<NetStream>(SyncCacheCmd.GetEntity, info.ToString());
            //if (stream == null)
            //    return default(T);
            //stream.Position = 0;
            //return BinarySerializer.DeserializeFromStream<T>((NetStream)stream, SerialContextType.GenericEntityAsIEntityType);

            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetEntity,
                Label = info.Prefix,
                CustomId = info.Suffix
            })
            {
                return SendDuplexStream<T>(message, OnFault);
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
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys is required");
            }
            return GetEntity<T>(ComplexArgs.Get(entityName, keys));
        }

        public string[] GetEntityPrimaryKey(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetEntityPrimaryKey,
                Label = entityName
            })
            {
                return SendDuplexStream<string[]>(message, OnFault);
            }
        }
        public T FindEntity<T> (string entityName, NameValueArgs nameValue)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            if (nameValue==null)
            {
                throw new ArgumentNullException("nameValue is required");
            }
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.FindEntity,
                Label = entityName,
                Args= nameValue
            })
            {
                return SendDuplexStream<T>(message, OnFault);
            }
        }

        
        /// <summary>
        /// Refresh all items in sync cache
        /// </summary>
        public void RefreshAll()
        {
            SendOut(SyncCacheCmd.RefreshAll, null);
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
            if (string.IsNullOrWhiteSpace(syncName))
            {
                throw new ArgumentNullException("syncName is required");
            }
            SendOut(SyncCacheCmd.Refresh, syncName);
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
        public void Remove(string syncName)
        {
            if (string.IsNullOrWhiteSpace(syncName))
            {
                throw new ArgumentNullException("syncName is required");
            }
            SendOut(SyncCacheCmd.Remove, syncName);
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
        public void AddSyncItem(string db, string tableName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan ts, EntitySourceType sourceType, string[] entityKeys)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = SyncCacheCmd.AddSyncItem,
                Label = db,
                Args = NameValueArgs.Create(
                KnownArgs.ConnectionKey, db,
                KnownArgs.TableName, tableName,
                KnownArgs.MappingName, mappingName,
                KnownArgs.SourceName, NameValueArgs.JoinArg(sourceName),
                KnownArgs.SyncType, ((int)syncType).ToString(),
                KnownArgs.SyncTime, ts.ToString(),
                KnownArgs.EntityKeys, NameValueArgs.JoinArg(entityKeys),
                KnownArgs.SourceType, ((int)sourceType).ToString())//KnownArgs.EntityType,entityType)
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
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            //return SendDuplex<GenericKeyValue>(SyncCacheCmd.GetEntityItems, entityName);
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetEntityItems,
                Label = entityName
            })
            {
                return SendDuplexStream<GenericKeyValue>(message, OnFault);
            }
        }

        /// <summary>
        /// Get entity count from sync cache using entityName.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public int GetEntityItemsCount(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            //return SendDuplex<int>(SyncCacheCmd.GetEntityItemsCount, entityName);
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetEntityItemsCount,
                Label = entityName
            })
            {
                return SendDuplexStream<int>(message, OnFault);
            }
        }

        /// <summary>
        /// Get entity keys from sync cache using entityName.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public ICollection<string> GetEntityKeys(string entityName)
        {
            if(string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            //return SendDuplex<ICollection<string>>(SyncCacheCmd.GetEntityKeys, entityName);
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetEntityKeys,
                Label = entityName
            })
            {
                return SendDuplexStream<ICollection<string>>(message, OnFault);
            }
        }

        /// <summary>
        /// Get all entity names from sync cache.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllEntityNames()
        {
            using (CacheMessage message = new CacheMessage()//(typeof(ICollection<string>).FullName,(NetStream)null)
            {
                Command = SyncCacheCmd.GetAllEntityNames
                //TypeName = typeof(ICollection<string>).FullName
            })
            {
                return SendDuplexStream<ICollection<string>>(message, OnFault);
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
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            //return (DataTable)SendDuplex(SyncCacheCmd.GetItemsReport, entityName);//, typeof(ICollection<string>).FullName);
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetItemsReport,
                Label = entityName
            })
            {
                return SendDuplexStream<DataTable>(message, OnFault);
            }
        }
        /// <summary>
        /// Reply for test
        /// </summary>
        /// <returns></returns>
        public string Reply(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text is required");
            }
            //return SendDuplex<string>(SyncCacheCmd.Reply, text);
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.Reply,
                CustomId = text
            })
            {
                return SendDuplexStream<string>(message, OnFault);
            }
        }

        /// <summary>
        /// Get if sync cache contains item using arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool Contains(string entityName, string[] keys)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys is required");
            }
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.Contains,
                Label = entityName,
                CustomId = KeySet.Join(keys)
            })
            {
                return SendDuplexState(message) == CacheState.Ok;
            }
        }
        public bool Contains(ComplexKey keyInfo)
        {
            if (keyInfo == null)
            {
                throw new ArgumentNullException("keyInfo is required");
            }
            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.Contains,
                Label = keyInfo.Prefix,
                CustomId = keyInfo.Suffix
            })
            {
                return SendDuplexState(message) == CacheState.Ok;
            }
        }
        /// <summary>
        /// GetItemProperties
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public CacheItemProperties GetItemProperties(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entityName is required");
            }

            using (var message = new CacheMessage()
            {
                Command = SyncCacheCmd.GetItemProperties,
                Label = entityName
            })
            {
                return SendDuplexStream<CacheItemProperties>(message, OnFault);
            }
        }


    }
}
