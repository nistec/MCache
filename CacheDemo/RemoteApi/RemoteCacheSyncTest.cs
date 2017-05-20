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


namespace Nistec.Caching.Demo.RemoteApi
{
    public class RemoteCacheSyncTest
    {
        NetProtocol Protocol = NetProtocol.Tcp;

        const string TcpSyncHostName = "nistec_cache_sync";
        const int TcpSyncPort = 13001;

        const string entityName = "contactEntity";
        const string entityKey = "1";

        public static void TestAll(NetProtocol protocol)
        {
            RemoteCacheSyncTest test = new RemoteCacheSyncTest() { Protocol = protocol };

            test.GetAllEntityNames();
            test.GetValue("1");
            test.GetRecord("1");
            test.GetEntity("1");
            test.GetAs("contactEntity", "1");
            test.RemoveItem("contactGeneric");
            test.RefreshItem();
        }


        //Get item value from sync cache.
        public void GetValue(string key)
        {
            try
            {
                var item = SyncCacheApi.Get(Protocol).Get<GenericRecord>(entityName, new string[] { key });
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                {
                    Console.WriteLine(item["FirstName"]);

                    //convert to entity
                    ContactEntity entity = new EntityContext<ContactEntity>(item).Entity;
                    Console.WriteLine(entity.FirstName);
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
                var item = SyncCacheApi.Get(Protocol).GetRecord(entityName, new string[] { key });
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                    Console.WriteLine(item["FirstName"]);
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
                var item = SyncCacheApi.Get(Protocol).GetEntity<ContactEntity>(entityName, new string[] { key });
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                    Console.WriteLine(item.FirstName);
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
                //get item as ContactEntity
                var item = SyncCacheApi.Get(Protocol).GetAs(entityName, new string[] { key });
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                {
                    var gr = BinarySerializer.DeserializeFromStream<GenericRecord>(item);
                    if (gr == null)
                        Console.WriteLine("Deserialize  error");
                    else
                        Console.WriteLine(gr["FirstName"]);
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
                SyncCacheApi.Get(Protocol).RemoveItem(itemName);
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
                SyncCacheApi.Get(Protocol).Refresh("contactGeneric");
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
                var keys = SyncCacheApi.Get(Protocol).GetAllEntityNames();
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
                var keys = SyncCacheApi.Get(Protocol).GetEntityKeys(key);
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
