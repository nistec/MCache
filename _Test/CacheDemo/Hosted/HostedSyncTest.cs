using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Caching.Sync;
using Nistec.Data.Entities;
using Nistec.Data.Entities.Cache;
using Nistec.Caching.Demo.Entities;
using Nistec.Caching.Sync.Embed;


namespace Nistec.Caching.Demo.Hosted
{
    public class HostedSyncTest
    {
        //int timeout = 30;

        public static void TestAll()
        {
            HostedSyncTest test = new HostedSyncTest();

            test.AddItems();
            test.GetValue();
            test.GetRecord();
            test.RemoveItem();
            test.RefreshItem();
            test.GetEntityStream();
         }

        static SyncCache SyncCache;

         static HostedSyncTest()
        {
            SyncCache = new SyncCache("SyncCache", true);
            SyncCache.Start();
        }

        //Add items to remote cache.
        public void AddItems()
        {

            SyncCache.AddItem<ContactEntity>("AdventureWorks", "contactGeneric", "Person.Contact", new string[] { "Person.Contact" }, EntitySourceType.Table, new string[] { "ContactID" }, TimeSpan.FromMinutes(10), SyncType.Interval);

            SyncCache.AddItem<ContactEntity>("AdventureWorks", "contactEntity", "Person.Contact", new string[] { "Person.Contact" }, EntitySourceType.Table, new string[] { "ContactID" }, TimeSpan.FromMinutes(10), SyncType.Interval);
        }

        //Get item value from sync cache.
        public void GetValue()
        {
            string key = "1";
            var item = SyncCache.Get<ContactEntity>(CacheKeyInfo.Get("contactEntity", new string[] { "1" }));
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.FirstName);
        }

        //Get item value from sync cache as Dictionary.
        public void GetRecord()
        {
            string key = "1";
            var item = SyncCache.GetRecord(CacheKeyInfo.Get("contactEntity", new string[] { "1" }));
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item["FirstName"]);
        }


        //Remove item from sync cache.
        public void RemoveItem()
        {
            SyncCache.RemoveItem("contactGeneric");
        }

        //Refresh sync item which mean reload sync item from Db.
        public void RefreshItem()
        {
            SyncCache.Refresh("contactGeneric");
        }
        //get entity from sync cache as EntityStream.
        public void GetEntityStream()
        {
            var keyInfo=CacheKeyInfo.Get("contactEntity", new string[] { "1" });
            var item = SyncCache.GetItem(keyInfo.ToString());
            var stream = item.GetItemStream(keyInfo);
            ContactEntityContext context = new ContactEntityContext();
            context.EntityRead(stream,null);
            ContactEntity entity = context.Entity;

            Console.WriteLine(entity.FirstName);
        }
        
    }
}
