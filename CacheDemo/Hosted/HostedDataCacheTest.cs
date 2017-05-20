using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data.Factory;
using Nistec.Caching.Data;

namespace Nistec.Caching.Demo.Hosted
{
    public class HostedDataCacheTest
    {
        //int timeout = 30;
        string db = "AdventureWorks";
        string tableName = "Contacts";

        public static void TestAll()
        {
            HostedDataCacheTest test = new HostedDataCacheTest();

            test.AddItems();
            test.GetDataTable();
            test.GetRecord();
            test.GetValue();
            test.RemoveItem();

         }

         static DbCache dbCache;

         static HostedDataCacheTest()
        {
            dbCache = new DbCache("hostedDbCache");
            dbCache.Start();
        }

        //Add data table to data cache.
        public void AddItems()
        {
            DataTable dt=null;

            using (IDbCmd cmd = DbFactory.Create(db))
            {
                dt=cmd.ExecuteCommand<DataTable>("select * from Person.Contact");
            }

            dbCache.AddDataItem(db, dt, "Contact");

            //add table to sync tables, for synchronization by interval.
           // dbCache.AddSyncItem(db, tableName, "Person.Contact", SyncType.Interval, TimeSpan.FromMinutes(60));

        }

        //Get data table from data cache.
        public void GetDataTable()
        {
            var item = dbCache.GetDataTable(db, tableName);
            if (item == null)
                Console.WriteLine("item not found " + tableName);
            else
                Console.WriteLine(item.Rows.Count);
        }

        //Get item value from data cache as Dictionary.
        public void GetRecord()
        {
            string key = "1";
            var item = dbCache.GetRow(db, tableName, "ContactID=1");
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item["FirstName"]);
        }

        //Get value from data cache.
        public void GetValue()
        {
            string val = dbCache.GetValue<string>(db, tableName, "FirstName", "ContactID=1");
           Console.WriteLine(val);
        }

        //Remove data table from data cache.
        public void RemoveItem()
        {
            dbCache.RemoveTable(db, tableName);
        }
    }
}
