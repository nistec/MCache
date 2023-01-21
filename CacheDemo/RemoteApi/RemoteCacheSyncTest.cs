using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using Nistec.Channels;
using Nistec.Channels.RemoteCache;
using Nistec.Generic;
using Nistec.Caching.Demo.Entities;
using Nistec.Data.Entities;
using Nistec.Serialization;
using Nistec.Caching.Remote;

namespace Nistec.Caching.Demo.RemoteApi
{
    public class RemoteCacheSyncTest
    {
        NetProtocol Protocol = NetProtocol.Tcp;

        const string TcpSyncHostName = "nistec_cache_bundle";
        const int TcpSyncPort = 13001;

        const string entityName = "accountEntity";
        const string entityKey = "1";
        SyncCacheApi api;

        public static void TestValues(NetProtocol protocol, int count)
        {
            var arr = SyncCacheApi.Get(protocol).GetEntityKeys(entityName).ToArray();
            if (arr == null || arr.Length == 0)
            {
                Console.WriteLine("items not found!");
            }
            else
            {
                if (count <= 0)
                    count = 1;
                for (int i = 0; i < count; i++)
                {
                    foreach (var k in arr)
                    {
                        var record = SyncCacheApi.Get(protocol).GetRecord(entityName, k.Split(';'));
                        var json = JsonSerializer.Serialize(record, null, JsonFormat.Indented);
                        Console.WriteLine(json);
                    }

                    Console.WriteLine("finished items: " + arr.Length.ToString());
                }
            }
        }

        public static void GetAllEntityNames(NetProtocol protocol)
        {
            RemoteCacheSyncTest test = new RemoteCacheSyncTest() { Protocol = protocol, api = SyncCacheApi.Get(protocol) };

            test.GetAllEntityNames();
            
        }

        public static void TestAll(NetProtocol protocol)
        {
            RemoteCacheSyncTest test = new RemoteCacheSyncTest() { Protocol = protocol , api= SyncCacheApi.Get(protocol) };

            test.GetAllEntityNames();
            test.GetValue("1");
            test.GetRecord("1");
            test.GetEntity("1");
            test.GetAs("accountEntity", "1");
            test.RemoveItem("accountGeneric");
            test.RefreshItem();
        }


        //Get item value from sync cache.
        public void GetValue(string key)
        {
            try
            {
                var value = api.Get<string>(entityName, new string[] { key },"AccountName");
                if (value == null)
                    Console.WriteLine("item not found " + key);
                else
                {
                    Console.WriteLine(value);
                }
                var item = api.GetEntity<AccountEntity>(entityName, new string[] { key });
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                {
                    Console.WriteLine(item);

                    //convert to entity
                    AccountEntity entity = new EntityContext<AccountEntity>(item).Entity;
                    Console.WriteLine(entity.AccountName);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("GetValue error " + ex.Message);
            }

        }

        //Get item value from sync cache as Dictionary.
        public void GetRecord(string key)
        {
            try
            {
                var item = api.GetRecord(entityName, new string[] { key });
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                    Console.WriteLine(item["AccountName"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetRecord error " + ex.Message);
            }
        }

        //Get item value from sync cache as Entity.
        public void GetEntity(string key)
        {
            try
            {
                var item = api.GetEntity<AccountEntity>(entityName, new string[] { key });
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                    Console.WriteLine(item.AccountName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetEntity error " + ex.Message);
            }

        }

        //Get item copy from sync cache as stream.
        public void GetAs(string entityName, string key)
        {
            try
            {
                //get item as AccountEntity
                var item = api.GetAs(entityName, new string[] { key });
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                {
                    var gr = BinarySerializer.DeserializeFromStream<GenericRecord>(item);
                    if (gr == null)
                        Console.WriteLine("Deserialize  error");
                    else
                        Console.WriteLine(gr["AccountName"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAs error " + ex.Message);
            }
        }


        //Remove item from sync cache.
        public void RemoveItem(string itemName)
        {
            try
            {
                api.Remove(itemName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RemoveItem error " + ex.Message);
            }

        }

        //Refresh sync item which mean reload sync item from Db.
        public void RefreshItem()
        {
            try
            {
                api.Refresh("accountGeneric");
            }
            catch (Exception ex)
            {
                Console.WriteLine("RefreshItem error " + ex.Message);
            }
        }


        //get all sync names.
        public void GetAllEntityNames()
        {
            try
            {
                var keys = api.GetAllEntityNames();
                if (keys == null)
                    Console.WriteLine("GetAllEntityNames not found ");

                foreach (string s in keys)
                {
                    Console.WriteLine(s);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAllEntityNames error " + ex.Message);
            }
        }

        //get sync items.
        public void GetEntityKeys(string key)
        {
            try
            {
                var keys = api.GetEntityKeys(key);
                foreach (string s in keys)
                {
                    Console.WriteLine(s);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetEntityKeys error " + ex.Message);
            }
        }

    }
}
