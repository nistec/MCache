using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Nistec.Channels;
using Nistec.Caching.Remote;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.Data.Entities.Cache;
using Nistec.Caching.Demo.Entities;
using Nistec.Serialization;
using Nistec.IO;

namespace Nistec.Caching.Demo.Remote
{

    public class SyncCacheTest
    {
        const string TcpSyncHostName = "nistec_cache_sync";
        const int TcpSyncPort = 13001;

        const string entityName = "contactEntity";
        const string entityKey = "1";

        bool keepAlive = false;
        NetProtocol Protocol;
        public static void TestAll(NetProtocol protocol, int count=1)
        {
            SyncCacheTest test = new SyncCacheTest() { Protocol = protocol };
            for (int i = 0; i < count; i++)
            {
                test.GetAllEntityNames();
                test.GetEntityItemsCount();
                //test.AddItems();
                test.GetValue(entityKey);
                test.GetRecord(entityKey);
                test.GetEntity(entityKey);
                test.GetAs(entityName, entityKey);
                test.GetEntityKeys(entityName);
                test.GetEntityItems();
                test.RemoveItem("contactGeneric");
                test.RefreshItem();
                Thread.Sleep(1000);
            }
        }

        //Add items to remote cache.
        public void AddItems()
        {
            try
            {
                var api = SyncCacheApi.Get(Protocol);

                api.AddItem("AdventureWorks",
                    "contactGeneric",
                    "Person.Contact",
                    new string[] { "Person.Contact" },
                    SyncType.Interval,
                    TimeSpan.FromMinutes(10),
                    EntitySourceType.Table,
                    new string[] { "ContactID" });

                api.AddItem("AdventureWorks",
                   "contactEntity",
                   "Person.Contact",
                   new string[] { "Person.Contact" },
                   SyncType.Interval,
                   TimeSpan.FromMinutes(10),
                   EntitySourceType.Table,
                   new string[] { "ContactID" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddItems error " + ex.Message);
            }
        }

        //Get item value from sync cache.
        public void GetValue(string key)
        {
            try
            {
                var item = SyncCacheApi.Get(Protocol).GetItem<GenericRecord>(CacheKeyInfo.Get(entityName, new string[] { key }));
                if (item == null)
                    Console.WriteLine("item not found " + key);
                else
                {
                    Console.WriteLine(item["FirstName"]);

                    //convert to entity
                    ContactEntity entity = EntityContext.Get<ContactEntity>(item);
                    Console.WriteLine(entity.FirstName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetValue error " + ex.Message);
            }
        }

        //Get item value from sync cache as Dictionary.
        public void GetRecord(CacheKeyInfo keyInfo)
        {
            try
            {
                var item = SyncCacheApi.Get(Protocol).GetRecord(keyInfo);
                if (item == null)
                    Console.WriteLine("item not found " + keyInfo.CacheKey);
                else
                    Console.WriteLine(item["FirstName"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetRecord error " + ex.Message);
            }
        }

        //Get item value from sync cache as Dictionary.
        public void GetRecord(string key)
        {
            try
            {
                var item = SyncCacheApi.Get(Protocol).GetRecord(CacheKeyInfo.Get(entityName, new string[] { key }));
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
                var item = SyncCacheApi.Get(Protocol).GetEntity<ContactEntity>(CacheKeyInfo.Get(entityName, new string[] { key }));
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

        //Get item copy from sync cache as converted.
        public void GetAs(string entityName, string key)
        {
            try
            {

                //get item as stream
                var stream = SyncCacheApi.Get(Protocol).GetAs(CacheKeyInfo.Get(entityName, new string[] { key }), CacheEntityTypes.EntityContext);
                if (stream == null)
                    Console.WriteLine("item not found " + key);
                else
                {
                    stream.Position = 0;
                    var entity = BinarySerializer.DeserializeFromStream<ContactEntity>((NetStream)stream, SerialContextType.GenericEntityAsIEntityType);
                    if (entity == null)
                        Console.WriteLine("item serialization failed " + key);
                    else
                        Console.WriteLine(entity.FirstName);

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

        //get entity from sync cache as EntityItems.
        public void GetEntityItems()
        {
            try
            {
                var items = SyncCacheApi.Get(Protocol).GetEntityItems(entityName);
                if (items == null)
                    Console.WriteLine("item not found " + entityName);
                else
                    Console.WriteLine(items.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetEntityItems error " + ex.Message);
            }
        }

        //get entity count from sync cache.
        public void GetEntityItemsCount()
        {
            try
            {
                var count = SyncCacheApi.Get(Protocol).GetEntityItemsCount(entityName);
                 Console.WriteLine(count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetEntityItemsCount error " + ex.Message);
            }
        }

        //get all sync names.
        public void GetAllEntityNames()
        {
            try
            {
                var keys = SyncCacheApi.Get(Protocol).GetAllEntityNames();
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
        public void GetEntityKeys(string entityName)
        {
            try
            {
                var keys = SyncCacheApi.Get(Protocol).GetEntityKeys(entityName);
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
