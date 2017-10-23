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
using Nistec.Caching.Channels;
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
            RemoteHostName = CacheApiSettings.RemoteDataCacheHostName;
            EnableRemoteException = CacheApiSettings.EnableRemoteException;
        }

        #region items

        /// <summary>
        /// Set Value into specific row and column in local data table  
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="value">value to set</param>
        public void SetValue(string db, string tableName, string column, string filterExpression, object value)
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
            if (value==null)
            {
                throw new ArgumentNullException("value is required");
            }
            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.SetValue,
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.Column, column, KnowsArgs.Filter, filterExpression),
                BodyStream = BinarySerializer.ConvertToStream(value)// CacheMessageStream.EncodeBody(value)
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
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns></returns>
        /// <example><code>
        /// //Get value from data cache.
        ///public void GetValue()
        ///{
        ///    <![CDATA[string val = TcpDataCacheApi.GetValue<string>(db, tableName, "FirstName", "ContactID=1");]]>
        ///    Console.WriteLine(val);
        ///}
        /// </code></example>
        public T GetValue<T>(string db, string tableName, string column, string filterExpression)
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
            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.GetDataValue,
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.Column, column, KnowsArgs.Filter, filterExpression)
            })
            {
                return SendDuplex<T>(message);
            }
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>object value</returns>
        public object GetValue(string db, string tableName, string column, string filterExpression)
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
            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.GetDataValue,
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.Column, column, KnowsArgs.Filter, filterExpression)
            })
            {
                return SendDuplex(message);
            }
        }

        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="db">db name</param>
        /// <param name="tableName">table name</param>
        /// <param name="filterExpression">filter Expression</param>
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
        public IDictionary GetRow(string db, string tableName, string filterExpression)
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
                Command = DataCacheCmd.GetRow,
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName, KnowsArgs.Filter, filterExpression)
            })
            {
                return SendDuplex<IDictionary>(message);
            }
        }

        /// <summary>
        /// Get DataTable from storage by table name.
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
        public DataTable GetDataTable(string db, string tableName)
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
                Command = DataCacheCmd.GetDataTable,
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName)
            })
            {
                return SendDuplex<DataTable>(message);
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
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName)
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
        public DataCacheItem GetItemProperties(string db, string tableName)
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
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName)
            })
            {
                return SendDuplex<DataCacheItem>(message);
            }
        }


        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        public bool AddDataItem(string db, DataTable dt, string tableName)
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

            using (var message = new CacheMessage()
            {
                Command = DataCacheCmd.AddDataItem,
                Key = db,
                BodyStream = BinarySerializer.ConvertToStream(dt),// CacheMessageStream.EncodeBody(dt),
                Args = MessageStream.CreateArgs(KnowsArgs.TableName, tableName)
            })
            {
                return SendDuplex<bool>(message);
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
        public void AddDataItemSync(string db, DataTable dt, string tableName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan ts)
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
                Command = DataCacheCmd.AddDataItemSync,
                Key = db,
                BodyStream = BinarySerializer.ConvertToStream(dt),//CacheMessageStream.EncodeBody(dt),
                Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.MappingName, mappingName, KnowsArgs.SourceName, GenericNameValue.JoinArg(sourceName), KnowsArgs.SyncType, ((int)syncType).ToString(), KnowsArgs.SyncTime, ts.ToString())
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
        public void AddDataItemSync(string db, DataTable dt, string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts)
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
                Command = DataCacheCmd.AddDataItemSync,
                Key = db,
                BodyStream = BinarySerializer.ConvertToStream(dt),//CacheMessageStream.EncodeBody(dt),
                Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.MappingName, mappingName, KnowsArgs.SourceName, mappingName, KnowsArgs.SyncType, ((int)syncType).ToString(), KnowsArgs.SyncTime, ts.ToString())
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
                Command = DataCacheCmd.AddSyncDataItem,
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.MappingName, mappingName, KnowsArgs.SourceName, mappingName, KnowsArgs.SyncType, ((int)syncType).ToString(), KnowsArgs.SyncTime, ts.ToString())
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
                Command = DataCacheCmd.AddSyncDataItem,
                Key = db,
                Args = MessageStream.CreateArgs(KnowsArgs.ConnectionKey, db, KnowsArgs.TableName, tableName, KnowsArgs.MappingName, mappingName, KnowsArgs.SourceName, GenericNameValue.JoinArg(sourceName), KnowsArgs.SyncType, ((int)syncType).ToString(), KnowsArgs.SyncTime, ts.ToString())
            })
            {
                SendOut(message);
            }
        }
        #endregion
    }
}
