using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Channels;
using Nistec.Data.Factory;
using Nistec.Caching.Remote;



namespace Nistec.Caching.Demo.Remote
{
    public class DataCacheTest
    {
        //int timeout = 30;
        string db = "AdventureWorks";
        string tableName = "Contacts";
        NetProtocol Protocol;
        public static void TestAll(NetProtocol protocol)
        {
            DataCacheTest test = new DataCacheTest() { Protocol = protocol };

            test.AddItems();
            test.GetDataTable();
            test.GetRecord();
            test.GetValue();
            test.RemoveItem();

         }

        //Add data table to data cache.
        public void AddItems()
        {
            DataTable dt=null;

            using (IDbCmd cmd = DbFactory.Create(db))
            {
                dt=cmd.ExecuteCommand<DataTable>("select * from Person.Contact");
            }

            DataCacheApi.Get().AddDataItem(db, dt, "Contact");

            //add table to sync tables, for synchronization by interval.
            DataCacheApi.Get().AddSyncItem(db, tableName, "Person.Contact", SyncType.Interval, TimeSpan.FromMinutes(60));

        }

        //Get data table from data cache.
        public void GetDataTable()
        {
            var item = DataCacheApi.Get().GetDataTable(db, tableName);
            if (item == null)
                Console.WriteLine("item not found " + tableName);
            else
                Console.WriteLine(item.Rows.Count);
        }

        //Get item value from data cache as Dictionary.
        public void GetRecord()
        {
            string key = "1";
            var item = DataCacheApi.Get().GetRow(db,tableName,"ContactID=1");
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item["FirstName"]);
        }

        //Get value from data cache.
        public void GetValue()
        {
           string val= DataCacheApi.Get().GetValue<string>(db,tableName,"FirstName","ContactID=1");
           Console.WriteLine(val);
        }

        //Remove data table from data cache.
        public void RemoveItem()
        {
            DataCacheApi.Get().RemoveTable(db, tableName);
        }
    }
}
