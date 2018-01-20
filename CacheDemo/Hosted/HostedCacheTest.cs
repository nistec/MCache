using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Caching;
using Nistec.Caching.Demo.Entities;

namespace Nistec.Caching.Demo.Hosted
{
    public class HostedCacheTest
    {
        int timeout = 30;

        public static void TestAll()
        {
            HostedCacheTest test = new HostedCacheTest();

            test.AddItems();
            test.GetValue();
            test.FetchValue();
            test.RemoveItem();
            test.CopyItem();
            test.CutItem();
            test.GetItemView();
            test.MergeNewItem();
            test.MergeExistingItem();
            test.GetMergeValue();
         }

        static MemoCache MemoCache;

        static HostedCacheTest()
        {
            MemoCache = new MemoCache("hosted");
            MemoCache.Start();
        }


        //Add items to remote cache.
        public void AddItems()
        {
            MemoCache.Add("item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
            MemoCache.Add("item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
            MemoCache.Add("item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
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
            var item = MemoCache.Get<EntitySample>(key);
            Print(item, key);
        }
        //Fetch item value from cache.
        public void FetchValue()
        {
            string key = "item key 2";
            var item = MemoCache.Fetch<EntitySample>(key);
            Print(item, key);
        }
        //Remove item from cache.
        public void RemoveItem()
        {
            var state = MemoCache.Remove("item key 3");
            Console.WriteLine(state);
        }
        //Duplicate existing item from cache to a new destination.
        public void CopyItem()
        {
            string source = "item key 1";
            string dest = "item key 2";
            var state = MemoCache.CopyTo(source, dest, timeout);
            Console.WriteLine(state);
        }
        //Duplicate existing item from cache to a new destination and remove the old one.
        public void CutItem()
        {
            string source = "item key 2";
            string dest = "item key 3";
            var state = MemoCache.CutTo(source, dest, timeout);
            Console.WriteLine(state);
        }
        //Get properties for existing item in cache.
        public void GetItemView()
        {
            string key = "item key 1";
            var item = MemoCache.ViewItem(key);
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

            MemoCache.MergeItem("Entities merge sample", items);
        }
        //Merge collection items to exsisting items in cache.
        public void MergeExistingItem()
        {
            List<EntitySample> items = new List<EntitySample>();
            items.Add(new EntitySample() { Id = 1235, Name = "entity merge 4" });
            items.Add(new EntitySample() { Id = 2346, Name = "entity merge 5" });
            items.Add(new EntitySample() { Id = 3457, Name = "entity merge 6" });

            string key = "entity merge key";

            MemoCache.MergeItem<List<EntitySample>>(key, items);
        }
        //Get collection merged value.
        public void GetMergeValue()
        {
            string key = "entity merge key";
            var item = MemoCache.Get<List<EntitySample>>(key);

            if (item == null)
                Console.WriteLine("item not found " + key);
            else
                Console.WriteLine(item.Count);
        }

    }
}
