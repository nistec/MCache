using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Caching.Sync;
using Nistec.Data.Entities;
using Nistec.Data.Entities.Cache;
using Nistec.Caching.Demo.Entities;
using Nistec.Caching.Sync.Embed;
using Nistec.Channels;

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
            SyncCache.Start(true);
        }

        //Add items to remote cache.
        public void AddItems()
        {

            SyncCache.AddItem<AccountEntity>("Netcell_Docs", "accountGeneric", "Accounts", new string[] { "Accounts" }, EntitySourceType.Table, new string[] { "AccountId" }, "*", TimeSpan.FromMinutes(10), SyncType.Interval);

            SyncCache.AddItem<AccountEntity>("Netcell_Docs", "accountEntity", "Accounts", new string[] { "Accounts" }, EntitySourceType.Table, new string[] { "AccountId" }, "*", TimeSpan.FromMinutes(10), SyncType.Interval);

            SyncCache.Refresh("accountGeneric");
            SyncCache.Refresh("accountEntity");

        }

        //Get item value from sync cache.
        public void GetValue()
        {
            string key = "1";
            var item = SyncCache.Get<AccountEntity>(ComplexArgs.Get("accountEntity", new string[] { "1" }));
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.AccountName);
        }

        //Get item value from sync cache as Dictionary.
        public void GetRecord()
        {
            string key = "1";
            var item = SyncCache.GetRecord(ComplexArgs.Get("accountEntity", new string[] { "1" }));
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item["AccountName"]);
        }


        //Remove item from sync cache.
        public void RemoveItem()
        {
            SyncCache.RemoveItem("accountGeneric");
        }

        //Refresh sync item which mean reload sync item from Db.
        public void RefreshItem()
        {
            SyncCache.Refresh("accountGeneric");
        }
        //get entity from sync cache as EntityStream.
        public void GetEntityStream()
        {
            var keyInfo= ComplexArgs.Get("accountEntity", new string[] { "1" });
            var item = SyncCache.GetItem(keyInfo.Prefix);
            var stream = item.GetItemStream(keyInfo.Suffix);
            AccountDocsEntityContext context = new AccountDocsEntityContext();
            context.EntityRead(stream,null);
            AccountEntity entity = context.Entity;

            Console.WriteLine(entity.AccountName);
        }
        
    }
}
