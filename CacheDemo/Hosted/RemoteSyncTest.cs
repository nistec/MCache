using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Caching.Sync;
using Nistec.Data.Entities;
using Nistec.Data.Entities.Cache;
using Nistec.Caching.Demo.Entities;
using Nistec.Caching.Sync.Remote;
using Nistec.Channels;
using Nistec.Caching.Remote;
using Nistec.Caching.Server;
using Nistec.Generic;

namespace Nistec.Caching.Demo.Hosted
{
    public class RemoteSyncTest
    {
        //int timeout = 30;

        public static void TestAll()
        {
            RemoteSyncTest test = new RemoteSyncTest();

            test.AddItems();
            test.GetValue();
            test.GetRecord();
            test.RemoveItem();
            test.RefreshItem();
            test.GetEntityStream();
         }

        static SyncCacheAgent SyncCache;

        static RemoteSyncTest()
        {
            SyncCache = new SyncCacheAgent();

            //SyncCache = new SyncCacheStream("SyncCache");
            SyncCache.Start();
        }

        //Add items to remote cache.
        public void AddItems()
        {

            SyncCache.AddItem<ContactEntity>("AdventureWorks", "contactGeneric", "Person.Contact", new string[] { "Person.Contact" }, EntitySourceType.Table, new string[] { "ContactID" }, "*", TimeSpan.FromMinutes(10), SyncType.Interval);

            SyncCache.AddItem<GenericRecord>("AdventureWorks", "contactEntity", "Person.Contact", new string[] { "Person.Contact" }, EntitySourceType.Table, new string[] { "ContactID" }, "*", TimeSpan.FromMinutes(10), SyncType.Interval);

            //SyncCache.Refresh("contactGeneric");
            //SyncCache.Refresh("contactEntity");

        }

        //Get item value from sync cache.
        public void GetValue()
        {
            string key = "1";
            var item = SyncCache.GetEntity<ContactEntity>(ComplexArgs.Get("contactGeneric", new string[] { "1" }));
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.FirstName);

            var item2 = SyncCache.Get(ComplexArgs.Get("contactEntity", new string[] { "1" }));
            if (item2 == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item2);



            var val1 = SyncCache.Get(ComplexArgs.Get("contactEntity", new string[] { "1" }), "FirstName");
            if (val1 == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(val1);


        }


        //Get item value from sync cache as Dictionary.
        public void GetRecord()
        {
            string key = "1";

            var ts= SyncCache.ExecRemote(new CacheMessage() { Command = SyncCacheCmd.GetRecord, Label = "contactEntity" , Id = "1" });
            var o1= ts.ReadValue();
            Console.WriteLine(o1);

            var stream = SyncCache.GetRecord(ComplexArgs.Get("contactEntity", new string[] { "1" }));
            using (var streamer = new Serialization.BinaryStreamer(stream))
            {
                var dic= streamer.ReadGenericEntityAsDictionary(false);
                Console.WriteLine(dic);
            }

            var ts2 = SyncCache.ExecRemote(new CacheMessage() { Command = SyncCacheCmd.GetRecord, Label = "contactGeneric", Id = "1" });
            var o2 = ts.ReadValue();
            Console.WriteLine(o1);

            var stream2 = SyncCache.GetRecord(ComplexArgs.Get("contactGeneric", new string[] { "1" }));
            using (var streamer = new Serialization.BinaryStreamer(stream2))
            {
                var dic = streamer.ReadGenericEntityAsDictionary(false);
                Console.WriteLine(dic);
            }



            var item = SyncCache.GetGenericRecord(ComplexArgs.Get("contactEntity", new string[] { "1" }));
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
            var keyInfo= ComplexArgs.Get("contactEntity", new string[] { "1" });
            var item = SyncCache.GetTable(keyInfo.Prefix);
            var stream = item.GetItemStream(keyInfo.Suffix);
            ContactEntityContext context = new ContactEntityContext();
            context.EntityRead(stream,null);
            ContactEntity entity = context.Entity;

            Console.WriteLine(entity.FirstName);
        }
        
    }
}
