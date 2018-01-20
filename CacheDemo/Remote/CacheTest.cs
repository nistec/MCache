using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Caching.Remote;
using Nistec.Caching.Demo.Entities;
using Nistec.Channels;


namespace Nistec.Caching.Demo.Remote
{
    public class CacheTest
    {
        int timeout = 30;
        NetProtocol Protocol;
        public static void TestAll(NetProtocol protocol, bool enableRemove = true)
        {
            CacheTest test = new CacheTest() { Protocol = protocol, api=CacheApi.Get(protocol) };

            test.AddItems();
            test.GetValue();
            test.FetchValue();
            if (enableRemove)
                test.RemoveItem();
            test.CopyItem();
            test.CutItem();
            test.ViewEntry();
            test.DoAll();
            test.LoadData();

            //test.MergeNewItem();
            //test.MergeExistingItem();
            //test.GetMergeValue();
        }

        static void GoOn()
        {
            string entry = Console.ReadLine();
            if (entry == "q")
            {
                Environment.Exit(1);
            }
        }

        CacheApi api;


        //Add items to remote cache.
        public void AddItems()
        {
            CacheState state = CacheState.UnKnown;
            var dt = ContactEntityContext.GetList();
            for (int i=0;i<1000;i++)
            {
                state = api.Add("auto item key "+ i.ToString(), new EntitySample() { Id = i, Name = "entity sample "+i.ToString(), Creation = DateTime.Now, Value = dt }, timeout);
                Console.WriteLine("cache.Add: " + state.ToString());
            }


            state = api.Add("item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
            Console.WriteLine("cache.Add: " + state.ToString());
            state = api.Add("item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
            Console.WriteLine("cache.Add: " + state.ToString());
            state = api.Add("item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
            Console.WriteLine("cache.Add: " + state.ToString());
            state = api.Add("item key 4", new EntitySample() { Id = 126, Name = "entity sample 4", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
            Console.WriteLine("cache.Add: " + state.ToString());
            state = api.Add("item key 5", new EntitySample() { Id = 127, Name = "entity sample 5", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
            Console.WriteLine("cache.Add: " + state.ToString());
            GoOn();
        }

        //Print item to console
        void Print(EntitySample item, string key)
        {
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.Name);
        }

        void Print(object item, string key, string command)
        {
            Console.WriteLine("command: " + command + ", Key: " + key);

            if (item == null)
                Console.WriteLine("item not found " + key);
            else if(item.GetType()==typeof(string))
                Console.WriteLine(item.ToString());
            else
                Console.WriteLine(api.ToJson(item, true));
        }

        //Get item value from cache.
        public void GetValue()
        {
            string key = "item key 1";
            var api = CacheApi.Get(Protocol);
            var item = api.Get<EntitySample>(key);
            Print(item, key);

            var o = api.Get(key);
            Print(item, key);

            var entry = api.GetEntry(key);
            Print(entry, key, "GetEntry");

            var record = api.GetRecord(key);
            Print(record, key, "GetRecord");

            var stream = api.GetStream(key);
            Print(stream, key, "GetStream");

            var json = api.GetJson(key, Serialization.JsonFormat.Indented);
            Print(json, key, "GetJson");

            GoOn();
        }
        
        public void LoadData()
        {


        }
        public void DoAll()
        {
            string key = "item key 1";

            api.KeepAliveItem(key);
            Print("done", key, "KeepAliveItem");


        }

        //Fetch item value from cache.
        public void FetchValue()
        {
            string key = "item key 2";
            var item = CacheApi.Get(Protocol).Fetch<EntitySample>(key);
            Print(item, key);
            GoOn();
        }
        
        //Remove item from cache.
        public void RemoveItem()
        {
            string key = "item key 3";
            var state = api.Remove(key);
            Console.WriteLine("cache.RemoveItem: " + state.ToString());


            key = "item key 4";
            state = api.RemoveItemsBySession(key);
            Print(state.ToString(), key, "RemoveItemsBySession");


            GoOn();
        }
        
        //Duplicate existing item from cache to a new destination.
        public void CopyItem()
        {
            string source = "item key 1";
            string dest = "item key 2";
            var state = api.CopyTo(source, dest, timeout);
            Console.WriteLine("cache.CopyItem: " + state.ToString());
            GoOn();
        }
        
        //Duplicate existing item from cache to a new destination and remove the old one.
        public void CutItem()
        {
            string source = "item key 2";
            string dest = "item key 3";
            var state = api.CutTo(source, dest, timeout);
            Console.WriteLine("cache.CutItem: " + state.ToString());
            GoOn();
        }

        //Get properties for existing item in cache.
        public void ViewEntry()
        {
            string key = "item key 1";
            var item = api.ViewEntry(key);
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.PrintHeader());
            GoOn();
        }

        //Merge collection items to a new item in cache.
        public void MergeNewItem()
        {
            List<EntitySample> items = new List<EntitySample>();
            items.Add(new EntitySample() { Id=1234, Name="entity merge 1" });
            items.Add(new EntitySample() { Id = 2345, Name = "entity merge 2" });
            items.Add(new EntitySample() { Id = 3456, Name = "entity merge 3" });
            
            //CacheApi.Get().MergeItem("Entities merge sample", items, timeout);
        }
        //Merge collection items to exsisting items in cache.
        public void MergeExistingItem()
        {
            List<EntitySample> items = new List<EntitySample>();
            items.Add(new EntitySample() { Id = 1235, Name = "entity merge 4" });
            items.Add(new EntitySample() { Id = 2346, Name = "entity merge 5" });
            items.Add(new EntitySample() { Id = 3457, Name = "entity merge 6" });

            string key = "entity merge key";

            //CacheApi.Get(Protocol).MergeItem(key, items, timeout);
        }

        //Get collection merged value.
        public void GetMergeValue()
        {
            string key = "entity merge key";
            var item = api.Get<List<EntitySample>>(key);

            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.Count);
        }

    }
}
