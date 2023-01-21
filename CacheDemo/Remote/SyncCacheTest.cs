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

        const string entityName = "accountEntity";
        const string entityKey = "1";

        bool keepAlive = false;
        NetProtocol Protocol;
        SyncCacheApi api;
        public static void TestAll(NetProtocol protocol, bool enableRemove = true)
        {
            SyncCacheTest test = new SyncCacheTest() { Protocol = protocol, api = SyncCacheApi.Get(protocol) };
            //test.GetAllEntityNames();
            //test.AddItems();
            test.GetValue(entityKey);
            test.GetRecord(entityKey);
            test.GetEntity(entityKey);
            test.GetAs(entityName, entityKey);
            test.GetEntityKeys(entityName);
            test.GetEntityItems();
            if (enableRemove)
                test.RemoveItem("accountGeneric");
            test.RefreshItem();
        }

        static void GoOn()
        {
            return;
            string entry = Console.ReadLine();
            if (entry == "q")
            {
                Environment.Exit(1);
            }
        }

        void Print(object item, string key, string command)
        {
            Console.WriteLine("command: " + command + ", Key: " + key);

            if (item == null)
                Console.WriteLine("item not found " + key);
            else if (item.GetType() == typeof(string))
                Console.WriteLine(item.ToString());
            else
                Console.WriteLine(api.ToJson(item, true));
        }

       
        //Add items to remote cache.
        public void AddItems()
        {
            try
            {
                var api = SyncCacheApi.Get(Protocol);

                api.AddSyncItem("Netcell_Docs",
                    "accountGeneric",
                    "Accounts",
                    new string[] { "Accounts" },
                    SyncType.Interval,
                    TimeSpan.FromMinutes(10),
                    EntitySourceType.Table,
                    new string[] { "AccountId" });

                Print(CacheState.ItemAdded.ToString(), "accountGeneric", "AddSyncItem");

                api.AddSyncItem("Netcell_Docs",
                   "accountEntity",
                   "Accounts",
                   new string[] { "Accounts" },
                   SyncType.Interval,
                   TimeSpan.FromMinutes(10),
                   EntitySourceType.Table,
                   new string[] { "AccountId" });

                Print(CacheState.ItemAdded.ToString(), "accountEntity", "AddSyncItem");
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddItems error " + ex.Message);
            }

            GoOn();
        }

        //Get item value from sync cache.
        public void GetEntity(string key)
        {
            try
            {
                var item = api.GetEntity<GenericRecord>(ComplexArgs.Get(entityName, new string[] { key }));
                Print(item, key, "GetEntity");

                if (item != null)
                {
                    //convert to entity
                    //AccountEntity entity = EntityContext.Get<AccountEntity>(item);
                    Print(item["AccountName"], key, "GetValue by EntityContext");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetValue error " + ex.Message);
            }
            GoOn();
        }

        //Get item value from sync cache.
        public void GetValue(string key)
        {
            try
            {
                //var item = api.Get<GenericRecord>(ComplexArgs.Get(entityName, new string[] { key }));
                var value = api.Get<string>(ComplexArgs.Get(entityName, new string[] { key }),"AccountName");
                Print(value, key, "GetValue");
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetValue error " + ex.Message);
            }
            GoOn();
        }

        //Get item value from sync cache as Dictionary.
        public void GetRecord(ComplexKey keyInfo)
        {
            try
            {
                var item = api.GetRecord(keyInfo);
                Print(item, keyInfo.ToString(), "GetRecord");
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetRecord error " + ex.Message);
            }
            GoOn();
        }

        //Get item value from sync cache as Dictionary.
        public void GetRecord(string key)
        {
            try
            {
                var item = api.GetRecord(ComplexArgs.Get(entityName, new string[] { key }));

                Print(item, key, "GetRecord");
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetRecord error " + ex.Message);
            }
            GoOn();
        }

        ////Get item value from sync cache as Entity.
        //public void GetEntity(string key)
        //{
        //    try
        //    {
        //        var item = api.GetEntity<AccountEntity>(ComplexArgs.Get(entityName, new string[] { key }));

        //        Print(item, key, "GetEntity");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("GetEntity error " + ex.Message);
        //    }
        //    GoOn();
        //}

        //Get item copy from sync cache as converted.
        public void GetAs(string entityName, string key)
        {
            try
            {

                //get item as stream
                var stream = api.GetAs(ComplexArgs.Get(entityName, new string[] { key }));//, CacheEntityTypes.EntityContext);

                if (stream == null)
                    Console.WriteLine("item not found " + key);
                else
                {
                    stream.Position = 0;
                    var entity = BinarySerializer.DeserializeFromStream<AccountEntity>((NetStream)stream, SerialContextType.GenericEntityAsIEntityType);
                    if (entity == null)
                        Console.WriteLine("item serialization failed " + key);
                    else
                        Console.WriteLine(entity.AccountName);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAs error " + ex.Message);
            }
            GoOn();
        }

        //Remove item from sync cache.
        public void RemoveItem(string itemName)
        {
            try
            {
                api.Remove(itemName);
                Print(CacheState.ItemRemoved.ToString(), itemName, "RemoveItem");
            }
            catch (Exception ex)
            {
                Console.WriteLine("RemoveItem error " + ex.Message);
            }
            GoOn();
        }

        //Refresh sync item which mean reload sync item from Db.
        public void RefreshItem()
        {
            try
            {
                string syncName = "accountGeneric";
                api.Refresh(syncName);
                Print("item Refreshed", syncName, "RefreshItem");
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
                var items = api.GetEntityItems(entityName);
                Print(items, entityName, "GetEntityItems");
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetEntityItems error " + ex.Message);
            }
            GoOn();
        }

        //get all sync names.
        public void GetAllEntityNames()
        {
            try
            {
                var keys = api.GetAllEntityNames();
                Print(keys, "All", "GetAllEntityNames");

                //foreach (string s in keys)
                //{
                //    Console.WriteLine(s);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAllEntityNames error " + ex.Message);
            }
            GoOn();
        }


        //get sync items.
        public void GetEntityKeys(string entityName)
        {
            try
            {
                var keys = api.GetEntityKeys(entityName);
                Print(keys, entityName, "GetEntityKeys");

                //foreach (string s in keys)
                //{
                //    Console.WriteLine(s);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetEntityKeys error " + ex.Message);
            }
            GoOn();
        }

    }
   
}
