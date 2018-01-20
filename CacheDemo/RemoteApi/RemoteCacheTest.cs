using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Channels.RemoteCache;
using Nistec.Caching.Demo.Entities;
using Nistec.Channels;
using Nistec.Caching.Remote;

namespace Nistec.Caching.Demo.RemoteApi
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
         }

        //Add items to remote cache.
        public void AddItems()
        {
            var api = CacheApi.Get(Protocol);
            api.Add("item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
            api.Add("item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
            api.Add("item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
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
            var item = CacheApi.Get(Protocol).Get<EntitySample>(key);
            Print(item, key);
        }
        
        //Fetch item value from cache.
        public void FetchValue()
        {
            string key = "item key 2";
            var item = CacheApi.Get(Protocol).Fetch<EntitySample>(key);
            Print(item, key);
        }
        
        //Remove item from cache.
        public void RemoveItem()
        {
            CacheApi.Get(Protocol).Remove("item key 3");
        }
        
        //Duplicate existing item from cache to a new destination.
        public void CopyItem()
        {
            string source = "item key 1";
            string dest = "item key 2";
            CacheApi.Get(Protocol).CopyTo(source, dest, timeout);
        }
        
        //Duplicate existing item from cache to a new destination and remove the old one.
        public void CutItem()
        {
            string source = "item key 2";
            string dest = "item key 3";
            CacheApi.Get(Protocol).CutTo(source, dest, timeout);
        }

        
    }
}
