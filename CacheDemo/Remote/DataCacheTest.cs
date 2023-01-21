using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Channels;
using Nistec.Data.Factory;
using Nistec.Caching.Remote;
using Nistec.Data.Entities;

namespace Nistec.Caching.Demo.Remote
{
    public class DataCacheTest
    {
        //int timeout = 30;
        string db = "Netcell_Docs";
        string tableName = "Accounts";
        string mappingName = "Accounts";
        NetProtocol Protocol;
        DataCacheApi api;

        public static void TestAll(NetProtocol protocol, bool enableRemove = true)
        {
            DataCacheTest test = new DataCacheTest() { Protocol = protocol, api = DataCacheApi.Get(protocol) };
            test.AddItems();
            test.QueryItems();
            test.GetDataTable();
            test.GetRecord();
            test.GetValue();
            if (enableRemove)
                test.RemoveItem();
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

        public void QueryItems()
        {
            try
            {
                //EntityType = "GenericEntity"
                //ConnectionKey = "AdventureWorks"
                //EntityName = "EmployeeDepartmentHistory"
                //MappingName = "Accounts_Category"
                //SourceName = "Accounts_Category"
                //SourceType = "Table"
                //EntityKeys = "[EmployeeID],[DepartmentID],[ShiftID],[StartDate]"
                //SyncType = "Interval"
                //SyncTime = "00:24:30" />

                var table1 = api.QueryTable(db, "Accounts_Category", 3, null);
                //var table1 = api.QueryTable(db, "Accounts_Category", "[EmployeeID],[DepartmentID],[ShiftID],[StartDate]", 10, null);
                foreach (var row in table1.DataSource)
                {
                    Console.WriteLine(row.Key);
                }

                var entity = api.QueryEntity(db, "Accounts_Category", "181|7|3|24/03/1999 00:00:00", 3, null);
                //var entity = api.QueryEntity(db, EntitySourceType.Table, "Accounts_Category", "[EmployeeID],[DepartmentID],[ShiftID],[StartDate]", "181|7|3|24/03/1999 00:00:00", 10, null);

                Console.WriteLine(entity.ToJson());

                var table = api.QueryTable(db, "select * from " + mappingName, "AccountId", 2, null);
                foreach (var row in table.DataSource)
                {
                    Console.WriteLine(row.Key);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }
        }

        //Add data table to data cache.
        public void AddItems()
        {
            try
            {
                DataTable dt = null;

                using (IDbCmd cmd = DbFactory.Create(db))
                {
                    dt = cmd.ExecuteCommand<DataTable>("select * from " + mappingName);
                }

                var state = api.AddTable(db, dt, tableName, mappingName, new string[] { "AccountId" });
                Console.WriteLine("AddItems: {0} is {1}", tableName, state.ToString());

                GoOn();

                //add table to sync tables, for synchronization by interval.
                api.AddSyncItem(db, tableName, mappingName, SyncType.Interval, TimeSpan.FromMinutes(60));

                Console.WriteLine("AddSyncItem: {0} finished", tableName);
                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }

        }

        //Get data table from data cache.
        public void GetDataTable()
        {
            try
            {
                var item = api.GetTable(db, tableName);
                Print(item, db + "=>" + tableName, "GetDataTable");

                //if (item == null)
                //    Console.WriteLine("item not found " + tableName);
                //else
                //    Console.WriteLine(item.Count);

                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }
        }

        //Get item value from data cache as Dictionary.
        public void GetRecord()
        {
            try
            {
                string key = "1";
                var item = api.GetRecord(db, tableName, "1");
                Print(item, db + "=>" + tableName + "," + key, "GetRecord");

                //if (item == null)
                //    Console.WriteLine("item not found " + key);
                //else
                //    Console.WriteLine(item["AccountName"]);

                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }
        }

        //Get value from data cache.
        public void GetValue()
        {
            try
            {
                string key = "1";
                string val = api.GetValue<string>(db, tableName, "1", "AccountName");
                Print(val, db + "=>" + tableName + "," + key, "GetValue");
                // Console.WriteLine(val);
                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }
        }

        //Remove data table from data cache.
        public void RemoveItem()
        {
            try
            {
                api.RemoveTable(db, tableName);
                Print(CacheState.ItemRemoved.ToString(), db + "=>" + tableName, "RemoveTable");
                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }
        }
    }
}
