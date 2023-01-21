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
        string db = "Netcell_Docs";
        string tableName = "Accounts";
        string mappingName = "Accounts";

        public static void TestAll()
        {
            HostedDataCacheTest test = new HostedDataCacheTest();

            test.AddItems();
            test.GetDataTable();
            test.GetRecord();
            test.GetValues();
            test.RemoveItem();
        }

        public static void TestConters()
        {
            HostedDataCacheTest test = new HostedDataCacheTest();

            test.AddItems();
            test.BackgroundTasks();
            test.GetValueCounter();
            GoOn();
        }

        static DbCache dbCache;

         static HostedDataCacheTest()
        {
            dbCache = new DbCache("hostedDbCache");
            dbCache.Start();
        }

        static void GoOn()
        {
            string entry = Console.ReadLine();
            if (entry == "q")
            {
                Environment.Exit(1);
            }
        }

        //Add data table to data cache.
        public void AddItems()
        {

            DataTable dt = null;

            using (IDbCmd cmd = DbFactory.Create(db))
            {
                dt = cmd.ExecuteCommand<DataTable>("select * from " + mappingName,true);
            }

            var state = dbCache.AddTableWithKey(db, dt, tableName, mappingName, Nistec.Data.Entities.EntitySourceType.Table);//, new string[] { "AccountId" });

            Console.WriteLine("AddItems " + state.ToString());


            //add table to sync tables, for synchronization by interval.
            // dbCache.AddSyncItem(db, tableName, "Accounts", SyncType.Interval, TimeSpan.FromMinutes(60));

        }

        //Get data table from data cache.
        public void GetDataTable()
        {
            var item = dbCache.GetTable(db, tableName);
            if (item == null)
                Console.WriteLine("item not found " + tableName);
            else
                Console.WriteLine(item.Count);
        }

        //Get item value from data cache as Dictionary.
        public void GetRecord()
        {
            string key = "1";
            var item = dbCache.GetRow(db, tableName, "AccountId=1");
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item["AccountName"]);
        }

        void BackgroundTasks()
        {
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                int[] keys = new int[] { 17, 1170, 2793, 2798, 2810, 2835, 11269, 13590, 16259, 19046, 19976, -1 };
                while (true)
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        dbCache.GetRow(db, tableName, keys[i].ToString());
                    }
                    System.Threading.Thread.Sleep(100);
                }
            });
            
        }

        public void GetValueCounter()
        {
            int[] keys = new int[] { 17, 1170, 2793, 2798, 2810, 2835, 11269, 13590, 16259, 19046, 19976, -1 };

            for (int i = 0; i < keys.Length; i++)
            {
                var o = dbCache.GetValue(db, tableName, keys[i].ToString(), "AccountName");
                Console.WriteLine(o==null? "not found": o.ToString() );
                string str = dbCache.GetValue<string>(db, tableName, keys[i].ToString(), "AccountName");
                Console.WriteLine(str == null ? "not found" : str);
                string val = dbCache.FindValue<string>(db, tableName, keys[i].ToString(), "AccountName");
                Console.WriteLine(val == null ? "not found" : val);

                //GetValueObject(keys[i].ToString());
                //GetValue(keys[i].ToString());
                //FindValue(keys[i].ToString());
                GoOn();
            }
        }

        //Get value from data cache.
        public void GetValues()
        {
            int[] keys = new int[] { 17, 1170, 2793, 2798, 2810, 2835, 11269, 13590, 16259, 19046, 19976, -1 };
            for (int i = 0; i < keys.Length; i++)
            {
                string val = dbCache.GetValue<string>(db, tableName, keys[i].ToString(), "AccountName");
                Console.WriteLine(val);
            }
        }

        //Get value from data cache.
        public void GetValue(string key)
        {
            string val = dbCache.GetValue<string>(db, tableName, key, "AccountName");
            Console.WriteLine(val);
        }

        public void GetValueObject(string key)
        {
            var val = dbCache.GetValue(db, tableName, key, "AccountName");
            Console.WriteLine(val);
        }

        //Get value from data cache.
        public void FindValue(string key)
        {
            string val = dbCache.FindValue<string>(db, tableName, key, "AccountName");
            Console.WriteLine(val);
        }

       

        //Remove data table from data cache.
        public void RemoveItem()
        {
            dbCache.RemoveTable(db, tableName);
        }
    }
}
