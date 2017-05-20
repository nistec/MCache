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
        public static void TestAll(NetProtocol protocol)
        {
            CacheTest test = new CacheTest() { Protocol = protocol };

            test.AddItems();
            test.GetValue();
            test.FetchValue();
            test.RemoveItem();
            test.CopyItem();
            test.CutItem();
            test.GetItemView();
            //test.MergeNewItem();
            //test.MergeExistingItem();
            //test.GetMergeValue();
         }

        //Add items to remote cache.
        public void AddItems()
        {
            var api = CacheApi.Get(Protocol);
            api.AddItem("item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
            api.AddItem("item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
            api.AddItem("item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
        }

        //Print item to console
        void Print(EntitySample item, string key)
        {
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.Name);
        }
        
        //Get item value from cache.
        public void GetValue()
        {
            string key = "item key 1";
            var item = CacheApi.Get(Protocol).GetValue<EntitySample>(key);
            Print(item, key);
        }
        
        //Fetch item value from cache.
        public void FetchValue()
        {
            string key = "item key 2";
            var item = CacheApi.Get(Protocol).FetchValue<EntitySample>(key);
            Print(item, key);
        }
        
        //Remove item from cache.
        public void RemoveItem()
        {
            var state = CacheApi.Get(Protocol).RemoveItem("item key 3");
            Console.WriteLine(state);
        }
        
        //Duplicate existing item from cache to a new destination.
        public void CopyItem()
        {
            string source = "item key 1";
            string dest = "item key 2";
            var state = CacheApi.Get(Protocol).CopyItem(source, dest, timeout);
            Console.WriteLine(state);
        }
        
        //Duplicate existing item from cache to a new destination and remove the old one.
        public void CutItem()
        {
            string source = "item key 2";
            string dest = "item key 3";
            var state = CacheApi.Get(Protocol).CutItem(source, dest, timeout);
            Console.WriteLine(state);
        }

        //Get properties for existing item in cache.
        public void GetItemView()
        {
            string key = "item key 1";
            var item = CacheApi.Get(Protocol).ViewItem(key);
            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.PrintHeader());
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
            var item = CacheApi.Get(Protocol).GetValue<List<EntitySample>>(key);

            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.Count);
        }

    }
}
