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
        string db = "AdventureWorks";
        string tableName = "Contacts";
        string mappingName = "Person.Contact";
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
            string entry=Console.ReadLine();
            if(entry=="q")
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
            //EntityType = "GenericEntity"
            //ConnectionKey = "AdventureWorks"
            //EntityName = "EmployeeDepartmentHistory"
            //MappingName = "[HumanResources].[EmployeeDepartmentHistory]"
            //SourceName = "[HumanResources].[EmployeeDepartmentHistory]"
            //SourceType = "Table"
            //EntityKeys = "[EmployeeID],[DepartmentID],[ShiftID],[StartDate]"
            //SyncType = "Interval"
            //SyncTime = "00:24:30" />

            var table1 = api.QueryTable(db, "[HumanResources].[EmployeeDepartmentHistory]", 10, null);
            //var table1 = api.QueryTable(db, "[HumanResources].[EmployeeDepartmentHistory]", "[EmployeeID],[DepartmentID],[ShiftID],[StartDate]", 10, null);
            foreach (var row in table1.DataSource)
            {
                Console.WriteLine(row.Key);
            }

            var entity = api.QueryEntity(db, "[HumanResources].[EmployeeDepartmentHistory]", "181|7|3|24/03/1999 00:00:00", 10, null);
            //var entity = api.QueryEntity(db, EntitySourceType.Table, "[HumanResources].[EmployeeDepartmentHistory]", "[EmployeeID],[DepartmentID],[ShiftID],[StartDate]", "181|7|3|24/03/1999 00:00:00", 10, null);

            Console.WriteLine(entity.ToJson());

            var table =    api.QueryTable(db, "select * from " + mappingName, "ContactID", 10, null);
            foreach (var row in table.DataSource)
            {
                Console.WriteLine(row.Key);
            }
        }

        //Add data table to data cache.
        public void AddItems()
        {
            DataTable dt = null;

            using (IDbCmd cmd = DbFactory.Create(db))
            {
                dt = cmd.ExecuteCommand<DataTable>("select * from "+ mappingName);
            }

            var state = api.AddTable(db, dt, tableName, mappingName, new string[] { "ContactID" });
            Console.WriteLine("AddItems: {0} is {1}", tableName, state.ToString());

            GoOn();

            //add table to sync tables, for synchronization by interval.
            api.AddSyncItem(db, tableName, mappingName, SyncType.Interval, TimeSpan.FromMinutes(60));

            Console.WriteLine("AddSyncItem: {0} finished", tableName);
            GoOn();

        }

        //Get data table from data cache.
        public void GetDataTable()
        {
            var item = api.GetTable(db, tableName);
            Print(item, db+"=>"+ tableName, "GetDataTable");

            //if (item == null)
            //    Console.WriteLine("item not found " + tableName);
            //else
            //    Console.WriteLine(item.Count);

            GoOn();

        }

        //Get item value from data cache as Dictionary.
        public void GetRecord()
        {
            string key = "1";
            var item = api.GetRecord(db,tableName,"1");
            Print(item, db + "=>" + tableName+","+ key, "GetRecord");

            //if (item == null)
            //    Console.WriteLine("item not found " + key);
            //else
            //    Console.WriteLine(item["FirstName"]);

            GoOn();

        }

        //Get value from data cache.
        public void GetValue()
        {
            string key = "1";
            string val= api.GetValue<string>(db,tableName,"1", "FirstName");
            Print(val, db + "=>" + tableName + "," + key, "GetValue");
           // Console.WriteLine(val);
            GoOn();

        }

        //Remove data table from data cache.
        public void RemoveItem()
        {
            api.RemoveTable(db, tableName);
            Print(CacheState.ItemRemoved.ToString(), db + "=>" + tableName, "RemoveTable");
            GoOn();

        }
    }
}
