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
using System.Data;
using Nistec.Caching.Data;
using System.Collections;
using Nistec.IO;
using Nistec.Channels;
using Nistec.Runtime;
using Nistec.Generic;
//using Nistec.Caching.Channels;
using Nistec.Caching.Config;
using Nistec.Serialization;

namespace Nistec.Caching.Remote
{

    /// <summary>
    /// Represent data cache api for client.
    /// </summary>
    public class DataCacheApi : RemoteApi
    {

        /// <summary>
        /// Get cache api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static DataCacheApi Get(NetProtocol protocol = NetProtocol.Tcp)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = CacheApiSettings.Protocol;
            }
            return new DataCacheApi() { Protocol = protocol };
        }

        private DataCacheApi()
        {
            RemoteHostName = CacheDefaults.DefaultBundleHostName;
            EnableRemoteException = CacheApiSettings.EnableRemoteException;
        }

        protected void OnFault(string message)
        {
            Console.WriteLine("DataCacheApi Fault: " + message);
        }

        #region do custom
        public object DoCustom(string command, string db, string tableName, string primaryKey, string field=null, object value = null, string kvArgs=null)
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
            switch ("data_" + command)
            {
                case DataCacheCmd.Add:
                    AddValue(db, tableName, primaryKey, field, value);
                    return CacheState.Ok;
                case DataCacheCmd.AddSyncItem:
                    //return AddSyncItem(db, tableName);
                    return CacheState.CommandNotSupported;
                case DataCacheCmd.AddTable:
                    //return AddTable(db, tableName);
                    return CacheState.CommandNotSupported;
                case DataCacheCmd.AddTableWithSync:
                    //return AddTableWithSync(db, tableName,valArgs[KnownArgs.MappingName],);
                    return CacheState.CommandNotSupported;
                case DataCacheCmd.Contains:
                    return Contains(db, tableName);
                case DataCacheCmd.Get:
                    return GetValue(db, tableName, primaryKey, field);
                case DataCacheCmd.GetAllEntityNames:
                    return GetAllEntityNames(db);
                case DataCacheCmd.GetEntityItems:
                    return GetEntityItems(db, tableName);
                case DataCacheCmd.GetEntityItemsCount:
                    return GetEntityItemsCount(db, tableName);
                case DataCacheCmd.GetEntityKeys:
                    return GetEntityKeys(db, tableName);
                case DataCacheCmd.GetItemProperties:
                    return GetItemProperties(db, tableName);
                case DataCacheCmd.GetItemsReport:
                    return GetItemsReport(db, tableName);
                case DataCacheCmd.GetRecord:
                    return GetRecord(db, tableName,primaryKey);
                case DataCacheCmd.GetStream:
                    return GetStream(db, tableName, primaryKey);
                case DataCacheCmd.GetTable:
                    return GetTable(db, tableName);
                case DataCacheCmd.Refresh:
                    Refresh(db, tableName);
                    return CacheState.Ok;
                case DataCacheCmd.RemoveTable:
                    RemoveTable(db, tableName);
                    return CacheState.Ok;
                case DataCacheCmd.Reply:
                    return Reply(db);
                case DataCacheCmd.Reset:
                    Reset(db);
                    return CacheState.Ok;
                case DataCacheCmd.Set:
                    SetValue(db, tableName, primaryKey, field, value);
                    return CacheState.Ok;
                case DataCacheCmd.SetTable:
                    //return SetTable(db, tableName);
                    return CacheState.CommandNotSupported;
                default:
                    throw new ArgumentException("Unknown command " + command);
            }
        }

        public string DoHttpJson(string command, string db, string tableName, string primaryKey, string field=null, object value = null, string kvArgs = null, bool pretty=false)
        {
            string[] arrayArgs=null;
            if (!string.IsNullOrEmpty(kvArgs))
            {
                arrayArgs = KeySet.SplitTrim(kvArgs);
            }

            string cmd = "data_" + command.ToLower();
            switch (cmd)
            {
                case DataCacheCmd.Add:
                    //AddValue(db, tableName, field, primaryKey, value);
                    {
                        if (string.IsNullOrWhiteSpace(db))
                        {
                            throw new ArgumentNullException("db is required");
                        }
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            throw new ArgumentNullException("tableName is required");
                        }
                        if (string.IsNullOrWhiteSpace(field))
                        {
                            throw new ArgumentNullException("field is required");
                        }
                        if (string.IsNullOrWhiteSpace(primaryKey))
                        {
                            throw new ArgumentNullException("primaryKey is required");
                        }

                        if (value == null)
                        {
                            throw new ArgumentNullException("value is required");
                        }

                        var bodyStream = BinarySerializer.ConvertToStream(value);

                        var message = new CacheMessage()
                        {
                            Command = DataCacheCmd.Add,
                            DbName = db,
                            Label= tableName,
                            CustomId = primaryKey,
                            Args = NameValueArgs.Create(KnownArgs.Column, field).Merge(arrayArgs)
                            //BodyStream = BinarySerializer.ConvertToStream(value)
                        };
                        message.SetBody(bodyStream);
                        SendHttpJsonOut(message);
                        return CacheState.Ok.ToString();
                    }
                case DataCacheCmd.AddSyncItem:
                    //return AddSyncItem(db, tableName);
                    return CacheState.CommandNotSupported.ToString();
                case DataCacheCmd.AddTable:
                    //return AddTable(db, tableName);
                    return CacheState.CommandNotSupported.ToString();
                case DataCacheCmd.AddTableWithSync:
                    //return AddTableWithSync(db, tableName,valArgs[KnownArgs.MappingName],);
                    return CacheState.CommandNotSupported.ToString();
                case DataCacheCmd.Get:
                    //return GetValue(db, tableName, field, primaryKey);
                    {
                        if (string.IsNullOrWhiteSpace(db))
                        {
                            throw new ArgumentNullException("db is required");
                        }
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            throw new ArgumentNullException("tableName is required");
                        }
                        if (string.IsNullOrWhiteSpace(field))
                        {
                            throw new ArgumentNullException("field is required");
                        }
                        if (string.IsNullOrWhiteSpace(primaryKey))
                        {
                            throw new ArgumentNullException("primaryKey is required");
                        }

                        var message = new CacheMessage()
                        {
                            Command = DataCacheCmd.Get,
                            DbName = db,
                            Label = tableName,
                            CustomId = primaryKey,
                            Args = NameValueArgs.Create(KnownArgs.Column, field).Merge(arrayArgs)
                        };
                        return SendHttpJsonDuplex(message, pretty);
                    }
                case DataCacheCmd.GetAllEntityNames:
                case DataCacheCmd.Reply:
                    {
                        if (string.IsNullOrWhiteSpace(db))
                        {
                            throw new ArgumentNullException("db is required");
                        }
                        return SendHttpJsonDuplex(new CacheMessage() { Command = cmd, DbName = db }, pretty);
                    }
                case DataCacheCmd.GetEntityItems:
                case DataCacheCmd.GetEntityItemsCount:
                case DataCacheCmd.GetEntityKeys:
                case DataCacheCmd.GetItemProperties:
                case DataCacheCmd.GetItemsReport:
                case DataCacheCmd.GetTable:
                case DataCacheCmd.Contains:
                    {
                        if (string.IsNullOrWhiteSpace(db))
                        {
                            throw new ArgumentNullException("db is required");
                        }

                        if (tableName == null)
                        {
                            throw new ArgumentNullException("tableName is required");
                        }
                        return SendHttpJsonDuplex(new CacheMessage() { Command=cmd, DbName = db,Label=tableName}, pretty);
                    }

                case DataCacheCmd.GetRecord:
                    //return GetRecord(db, tableName, primaryKey);
                case DataCacheCmd.GetStream:
                    //return GetStream(db, tableName, primaryKey);
                    {
                        if (string.IsNullOrWhiteSpace(db))
                        {
                            throw new ArgumentNullException("db is required");
                        }
                        if (tableName == null)
                        {
                            throw new ArgumentNullException("tableName is required");
                        }
                        if (primaryKey == null)
                        {
                            throw new ArgumentNullException("primaryKey is required");
                        }
                        return SendHttpJsonDuplex(new CacheMessage() { Command = cmd, DbName = db, Label = tableName, Args = NameValueArgs.Create(arrayArgs) }, pretty);
                    }
                case DataCacheCmd.Refresh:
                case DataCacheCmd.RemoveTable:
                    {
                        if (string.IsNullOrWhiteSpace(db))
                        {
                            throw new ArgumentNullException("db is required");
                        }

                        if (tableName == null)
                        {
                            throw new ArgumentNullException("tableName is required");
                        }
                        SendHttpJsonOut(new CacheMessage() { Command = cmd, DbName = db, Label = tableName });
                        return CacheState.Ok.ToString();
                   }
                case DataCacheCmd.Reset:
                    //Reset(db);
                    {
                        if (string.IsNullOrWhiteSpace(db))
                        {
                            throw new ArgumentNullException("db is required");
                        }
                        SendHttpJsonOut(new CacheMessage() { Command = cmd, DbName = db });
                        return CacheState.Ok.ToString();
                    }
                case DataCacheCmd.Set:
                    //SetValue(db, tableName, field, primaryKey, value);
                    {
                        if (string.IsNullOrWhiteSpace(db))
                        {
                            throw new ArgumentNullException("db is required");
                        }
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            throw new ArgumentNullException("tableName is required");
                        }
                        if (string.IsNullOrWhiteSpace(field))
                        {
                            throw new ArgumentNullException("field is required");
                        }
                        if (string.IsNullOrWhiteSpace(primaryKey))
                        {
                            throw new ArgumentNullException("primaryKey is required");
                        }
                        if (value == null)
                        {
                            throw new ArgumentNullException("value is required");
                        }
                        var message = new CacheMessage()//BinarySerializer.ConvertToStream(value))//(MessageStream.GetTypeName(value), BinarySerializer.ConvertToStream(value))
                        {
                            Command = DataCacheCmd.Set,
                            //BodyStream=MessageStream.SerializeBody(value),
                            DbName = db,
                            Label = tableName,
                            CustomId = primaryKey,
                            Args = NameValueArgs.Create(KnownArgs.Column, field).Merge(arrayArgs)
                            //BodyStream = BinarySerializer.ConvertToStream(value)// CacheMessageStream.EncodeBody(value)
                        };
                        message.SetBody(value);
                        SendHttpJsonOut(message);
                        return CacheState.Ok.ToString();
                    }
                case DataCacheCmd.SetTable:
                    //return SetTable(db, tableName);
                    return CacheState.CommandNotSupported.ToString();
                default:
                    throw new ArgumentException("Unknown command " + command);
            }
        }
        #endregion

        #region items

        /// <summary>
        /// Add Value into specific row and column in local data table  
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">filter Expression</param>
        /// <param name="column">column name</param>
        /// <param name="value">value to set</param>
        public void AddValue(string db, string tableName, string primaryKey, string column, object value)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new ArgumentNullException("column is required");
            }
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                throw new ArgumentNullException("primaryKey is required");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value is required");
            }
            using (var message = new CacheMessage()// BinarySerializer.ConvertToStream(value))//(MessageStream.GetTypeName(value), BinarySerializer.ConvertToStream(value))
            {
                //Command = DataCacheCmd.Add,
                DbName = db,
                Label=tableName,
                CustomId = primaryKey,
                Args = NameValueArgs.Create(KnownArgs.Column, column),//KnownArgs.ConnectionKey, db, KnownArgs.TableName, tableName, KnownArgs.Column, column, KnownArgs.Pk, primaryKey),
                BodyStream = MessageStream.SerializeBody(value),
                TypeName = Types.GetTypeName(value)
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// Set Value into specific row and column in local data table  
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primary Id</param>
        /// <param name="column">column name</param>
        /// <param name="value">value to set</param>
        public void SetValue(string db, string tableName, string primaryKey, string column, object value)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new ArgumentNullException("column is required");
            }
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                throw new ArgumentNullException("primaryKey is required");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value is required");
            }
            using (var message = new CacheMessage()//(BinarySerializer.ConvertToStream(value))//(MessageStream.GetTypeName(value), BinarySerializer.ConvertToStream(value))
            {
                Command = DataCacheCmd.Set,
                DbName = db,
                Label=tableName,
                CustomId = primaryKey,
                Args = NameValueArgs.Create(KnownArgs.Column, column),//(KnownArgs.ConnectionKey, db, KnownArgs.TableName, tableName, KnownArgs.Column, column, KnownArgs.Pk, primaryKey),
                BodyStream = MessageStream.SerializeBody(value),
                TypeName = Types.GetTypeName(value)
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primary Id</param>
        /// <param name="column">column name</param>
        /// <returns></returns>
        /// <example><code>
        /// //Get value from data cache.
        ///public void GetValue()
        ///{
        ///    <![CDATA[string val = TcpDataCacheApi.GetValue<string>(db, tableName, "FirstName", "ContactID=1");]]>
        ///    Console.WriteLine(val);
        ///}
        /// </code></example>
        public T GetValue<T>(string db, string tableName, string primaryKey, string column)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new ArgumentNullException("column is required");
            }
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                throw new ArgumentNullException("primaryKey is required");
            }

            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.Get,
                DbName = db,
                Label = tableName,
                CustomId = primaryKey,
                Args = NameValueArgs.Create(KnownArgs.Column, column),//(KnownArgs.ConnectionKey, db, KnownArgs.TableName, tableName, KnownArgs.Column, column, KnownArgs.Pk, primaryKey)
            })
            {
                return SendDuplexStream<T>(message, OnFault);
            }
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primary Id</param>
        /// <param name="column">column name</param>
        /// <returns>object value</returns>
        public object GetValue(string db, string tableName, string primaryKey, string column)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new ArgumentNullException("column is required");
            }
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                throw new ArgumentNullException("primaryKey is required");
            }

            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.Get,
                DbName = db,
                Label = tableName,
                CustomId = primaryKey,
                Args = NameValueArgs.Create(KnownArgs.Column, column),//(KnownArgs.ConnectionKey, db, KnownArgs.TableName, tableName, KnownArgs.Column, column, KnownArgs.Pk, primaryKey)
            })
            {
                return SendDuplexStreamValue(message, OnFault);
            }
        }

        /// <summary>
        /// Get item value from cache as json.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primary key</param>
        /// <returns></returns>
        public string GetJson(string db, string tableName, string primaryKey, JsonFormat format)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                throw new ArgumentNullException("primaryKey is required");
            }
            var stream = GetStream(db, tableName, primaryKey);
            return RemoteApi.ToJson(stream, format);
        }

        /// <summary>
        /// Get item as <see cref="NetStream"/> from data cache.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">primary key</param>
        /// <returns></returns>
        public NetStream GetStream(string db, string tableName, string primaryKey)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            using (var message = new CacheMessage()//typeof(NetStream).FullName, (NetStream)null)
            {
                Command = DataCacheCmd.GetStream,
                DbName = db,
                Label = tableName,
                CustomId = primaryKey,
                //Args = NameValueArgs.Get(KnownArgs.TableName, tableName, KnownArgs.Pk, primaryKey),
                //TypeName = typeof(NetStream).FullName,
                TransformType = TransformType.Stream
            })
            {
                return SendDuplexStream<NetStream>(message, OnFault);
            }
        }


        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="primaryKey">filter Expression</param>
        /// <returns>Hashtable object</returns>
        /// <example><code>
        /// //Get item record from data cache as Dictionary.
        ///public void GetRecord()
        ///{
        ///    string key = "1";
        ///    var item = TcpDataCacheApi.GetRow(db, tableName, "ContactID=1");
        ///    if (item == null)
        ///        Console.WriteLine("item not found " + key);
        ///    else
        ///        Console.WriteLine(item["FirstName"]);
        ///}
        /// </code></example>
        public IDictionary GetRecord(string db, string tableName, string primaryKey)
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
                Label = tableName,
                CustomId = primaryKey
                //Args = NameValueArgs.Create(KnownArgs.TableName, tableName, KnownArgs.Pk, primaryKey)
            })
            {
                return SendDuplexStream<IDictionary>(message, OnFault);
            }
        }

        /// <summary>
        /// Get DbTable from storage by table name.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        /// <example><code>
        ///  //Get data table from data cache.
        ///public void GetDataTable()
        ///{
        ///    var item = TcpDataCacheApi.GetDataTable(db, tableName);
        ///    if (item == null)
        ///        Console.WriteLine("item not found " + tableName);
        ///    else
        ///        Console.WriteLine(item.Rows.Count);
        ///}
        /// </code></example>
        public DbTable GetTable(string db, string tableName)
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
                Command = DataCacheCmd.GetTable,
                DbName = db,
                Label = tableName
            })
            {
                return SendDuplexStream<DbTable>(message, OnFault);
            }

        }
        

        /// <summary>
        /// Remove data table  from storage
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <example><code>
        /// //Remove data table from data cache.
        ///public void RemoveItem()
        ///{
        ///    TcpDataCacheApi.RemoveTable(db, tableName);
        ///}
        /// </code></example>
        public void RemoveTable(string db, string tableName)
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
                Command = DataCacheCmd.RemoveTable,
                DbName = db,
                Label = tableName
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// GetItemProperties
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public CacheItemProperties GetItemProperties(string db, string tableName)
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
                Command = DataCacheCmd.GetItemProperties,
                DbName = db,
                Label = tableName
            })
            {
                return SendDuplexStream<CacheItemProperties>(message, OnFault); 
            }
        }


        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName">mapping name</param>
        /// <param name="primaryKey"></param>
        public CacheState AddTable(string db, DataTable dt, string tableName, string mappingName, string[] primaryKey)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (dt==null)
            {
                throw new ArgumentNullException("dt is required");
            }

            using (var message = new CacheMessage()//(BinarySerializer.ConvertToStream(dt))//(typeof(DbTable).FullName, BinarySerializer.ConvertToStream(dt))
            {
                Command = DataCacheCmd.AddTable,
                DbName = db,
                Label = tableName,
                CustomId = primaryKey.JoinTrim(),
                BodyStream = MessageStream.SerializeBody(dt),
                TypeName = Types.GetTypeName(dt),
                //Label = primaryKey.JoinTrim(),
                Args = NameValueArgs.Create(KnownArgs.MappingName, mappingName, KnownArgs.SourceType, EntitySourceType.Table.ToString())
            })
            {
                return SendDuplexState(message);
            }
        }

        /// <summary>
        /// Set Remoting Data Item to cache
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName">mapping name</param>
        /// <param name="primaryKey"></param>
        public CacheState SetTable(string db, DataTable dt,  string tableName, string mappingName, string[] primaryKey)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (dt == null)
            {
                throw new ArgumentNullException("dt is required");
            }
            using (var message = new CacheMessage()//BinarySerializer.ConvertToStream(dt))//(typeof(DbTable).FullName, BinarySerializer.ConvertToStream(dt))
            {
                Command = DataCacheCmd.SetTable,
                DbName = db,
                Label = tableName,
                CustomId = primaryKey.JoinTrim(),
                BodyStream = MessageStream.SerializeBody(dt),
                TypeName = Types.GetTypeName(dt),
                //Label = primaryKey.JoinTrim(),
                Args = NameValueArgs.Create(KnownArgs.MappingName, mappingName, KnownArgs.SourceType, EntitySourceType.Table.ToString())
            })
            {
                return SendDuplexState(message);
            }
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
        public void AddTableWithSync(string db, DataTable dt, string tableName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan ts)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(mappingName))
            {
                throw new ArgumentNullException("mappingName is required");
            }
            if (sourceName==null)
            {
                throw new ArgumentNullException("sourceName is required");
            }

            using (var message = new CacheMessage()//(BinarySerializer.ConvertToStream(dt))//(MessageStream.GetTypeName(dt), BinarySerializer.ConvertToStream(dt))
            {
                Command = DataCacheCmd.AddTableWithSync,
                DbName = db,
                Label = tableName,
                BodyStream = MessageStream.SerializeBody(dt),
                TypeName = Types.GetTypeName(dt),
                Args = NameValueArgs.Create(KnownArgs.MappingName, mappingName, KnownArgs.SourceName, NameValueArgs.JoinArg(sourceName), KnownArgs.SyncType, ((int)syncType).ToString(), KnownArgs.SyncTime, ts.ToString(), KnownArgs.SourceType, EntitySourceType.Table.ToString())
            })
            {
                SendOut(message);
            }
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
        public void AddTableWithSync(string db, DataTable dt, string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(mappingName))
            {
                throw new ArgumentNullException("mappingName is required");
            }

            using (var message = new CacheMessage()//(BinarySerializer.ConvertToStream(dt))//(MessageStream.GetTypeName(dt), BinarySerializer.ConvertToStream(dt))
            {
                Command = DataCacheCmd.AddTableWithSync,
                DbName = db,
                Label = tableName,
                BodyStream = MessageStream.SerializeBody(dt),
                TypeName = Types.GetTypeName(dt),
                Args = NameValueArgs.Create(tableName, KnownArgs.MappingName, mappingName, KnownArgs.SourceName, mappingName, KnownArgs.SyncType, ((int)syncType).ToString(), KnownArgs.SyncTime, ts.ToString())
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// Add Item to SyncTables
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="syncType"></param>
        /// <param name="ts"></param>
        public void AddSyncItem(string db, string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(mappingName))
            {
                throw new ArgumentNullException("mappingName is required");
            }

            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.AddSyncItem,
                DbName = db,
                Label = tableName,
                Args = NameValueArgs.Create(KnownArgs.MappingName, mappingName, KnownArgs.SourceName, mappingName, KnownArgs.SyncType, ((int)syncType).ToString(), KnownArgs.SyncTime, ts.ToString())
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// Add Item to SyncTables
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="ts"></param>
        public void AddSyncItem(string db, string tableName, string mappingName, string[] sourceName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName is required");
            }
            if (string.IsNullOrWhiteSpace(mappingName))
            {
                throw new ArgumentNullException("mappingName is required");
            }
            if (sourceName==null)
            {
                throw new ArgumentNullException("sourceName is required");
            }

            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.AddSyncItem,
                DbName = db,
                Label = tableName,
                Args = NameValueArgs.Create(KnownArgs.MappingName, mappingName, KnownArgs.SourceName, NameValueArgs.JoinArg(sourceName), KnownArgs.SyncType, ((int)syncType).ToString(), KnownArgs.SyncTime, ts.ToString())
            })
            {
                SendOut(message);
            }
        }
        #endregion

        /// <summary>
        /// Get if DbSet contains specified table.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        public bool Contains(string db, string tableName)
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
                Command = DataCacheCmd.Contains,
                DbName = db,
                Label=tableName
            })
            {
                return SendDuplexState(message)== CacheState.Ok;
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
        /// Reset all items in sync cache
        /// </summary>
        /// <param name="db"></param>
        public void Reset(string db)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }

            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.Reset,
                DbName = db
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// Refresh specified item in sync cache.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        public void Refresh(string db,string tableName)
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
                Command = DataCacheCmd.Refresh,
                DbName = db,
                Label= tableName
            })
            {
                SendOut(message);
            }
        }

        /// <summary>
        /// Get entity values as <see cref="EntityStream"/> array from sync cache using entityName.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public KeyValuePair<string, GenericEntity>[] GetEntityItems(string db, string tableName)
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
                Command = DataCacheCmd.GetEntityItems,
                DbName = db,
                Label = tableName
            })
            {
                return SendDuplexStream<KeyValuePair<string, GenericEntity>[]>(message, OnFault);
            }
        }

        /// <summary>
        /// Get entity count from sync cache using entityName.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public int GetEntityItemsCount(string db, string tableName)
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
                Command = DataCacheCmd.GetEntityItemsCount,
                DbName = db,
                Label = tableName
            })
            {
                return SendDuplexStream<int>(message, OnFault);
            }
        }

        /// <summary>
        /// Get entity keys from sync cache using entityName.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public ICollection<string> GetEntityKeys(string db, string tableName)
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
                Command = DataCacheCmd.GetEntityKeys,
                DbName = db,
                Label = tableName
            })
            {
                return SendDuplexStream<ICollection<string>>(message, OnFault);
            }
        }

        /// <summary>
        /// Get all entity names from sync cache.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public IEnumerable<string> GetAllEntityNames(string db)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new ArgumentNullException("db is required");
            }

            using (CacheMessage message = new CacheMessage()
            {
                Command = DataCacheCmd.GetAllEntityNames,
                DbName = db
            })
            {
                return SendDuplexStream<ICollection<string>>(message, OnFault);
            }

   
        }

        /// <summary>
        /// Get entity items report from sync cache using entityName.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetItemsReport(string db, string tableName)
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
                Command = DataCacheCmd.GetItemsReport,
                DbName = db,
                Label = tableName
            })
            {
                return SendDuplexStream<DataTable>(message, OnFault);
            }
        }

        public DbTable QueryTable(string ConnectionKey, string MappingName, int expiration, params object[] keyValueParameters)
        {
            return QueryTable(ConnectionKey, MappingName, null, EntitySourceType.Table, expiration, keyValueParameters);
        }
        public DbTable QueryTable(string ConnectionKey, string MappingName, string fieldsKey, int expiration, params object[] keyValueParameters)
        {
            return QueryTable(ConnectionKey, MappingName, fieldsKey, EntitySourceType.Table, expiration, keyValueParameters);
        }
        public DbTable QueryTable(string ConnectionKey, string MappingName, string fieldsKey, EntitySourceType sourceType, int expiration, params object[] keyValueParameters)
        {

            EntityDbArgs arg = new EntityDbArgs()
            {
                ConnectionKey = ConnectionKey,
                Keys = KeySet.SplitKeysTrim(fieldsKey),
                MappingName = MappingName,
                SourceType = sourceType,
                Args = KeyValueArgs.Get(keyValueParameters)
            };

            var message = new CacheMessage()//MessageStream.SerializeBody(arg))
            {
                Command = Remote.DataCacheCmd.QueryTable,
                Expiration = expiration,
                BodyStream = MessageStream.SerializeBody(arg),
                TypeName = Types.GetTypeName(arg)
            };

            return SendDuplexStream<DbTable>(message, OnFault);
        }

        public GenericEntity QueryEntity(string ConnectionKey, string MappingName, string id, int expiration, params object[] keyValueParameters)
        {
            return QueryEntity(ConnectionKey, EntitySourceType.Table, MappingName, null, id, expiration, keyValueParameters);
        }
        public GenericEntity QueryEntity(string ConnectionKey, string MappingName, string fieldsKey, string id, int expiration, params object[] keyValueParameters)
        {
            return QueryEntity(ConnectionKey, EntitySourceType.Table, MappingName, fieldsKey, id, expiration, keyValueParameters);
        }

        public GenericEntity QueryEntity(string ConnectionKey, EntitySourceType sourceType, string MappingName, string fieldsKey,string id, int expiration, params object[] keyValueParameters)
        {

            EntityDbArgs arg = new EntityDbArgs()
            {
                ConnectionKey = ConnectionKey,
                Keys = KeySet.SplitKeysTrim(fieldsKey),
                MappingName = MappingName,
                SourceType = sourceType,
                Args = KeyValueArgs.Get(keyValueParameters)
            };
            var message = new CacheMessage()//MessageStream.SerializeBody(arg))
            {
                Command = Remote.DataCacheCmd.QueryEntity,
                Expiration = expiration,
                CustomId = id,
                BodyStream = MessageStream.SerializeBody(arg),
                TypeName = Types.GetTypeName(arg)
            };

            return SendDuplexStream<GenericEntity>(message, OnFault);
        }

        public GenericEntity QueryEntity(string ConnectionKey, EntitySourceType sourceType, string MappingName, NameValueArgs entityKey, int expiration, object[] keyValueParameters)
        {

            EntityDbArgs arg = new EntityDbArgs()
            {
                ConnectionKey = ConnectionKey,
                Keys = entityKey.Keys.ToArray(),
                MappingName = MappingName,
                SourceType = sourceType,
                Args = KeyValueArgs.Get(keyValueParameters)
            };
            var message = new CacheMessage()//MessageStream.SerializeBody(arg))
            {
                Command = Remote.DataCacheCmd.QueryEntity,
                Expiration = expiration,
                CustomId = entityKey.GetPrimaryKey(),
                BodyStream = MessageStream.SerializeBody(arg),
                TypeName = Types.GetTypeName(arg)
            };

            return SendDuplexStream<GenericEntity>(message, OnFault);
        }


    }
}
