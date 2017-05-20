using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Caching.Session;
using Nistec.Caching.Demo.Entities;

namespace Nistec.Caching.Demo.Hosted
{
    public class HostedSessionTest
    {
        string sessionId = "12345678";
        string userId = "12";
        int timeout = 0;

        public static void TestAll()
        {
            HostedSessionTest test = new HostedSessionTest();

            test.AddSession();
            test.AddItems();
            test.GetOrCreateSession();
            test.GetItem();
            test.CopyTo();
            test.FetchTo();
            test.RemoveItem();
            test.RemoveSession();
        }


        static SessionCache SessionCache;

        static HostedSessionTest()
        {
            SessionCache = new SessionCache();
            SessionCache.Start();
        }

        //Create new session.
        public void AddSession()
        {
            SessionCache.AddSession(sessionId, userId, timeout, null);
        }

        //Add items to current session.
        public void AddItems()
        {
            SessionCache.AddItem(sessionId, "item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
            SessionCache.AddItem(sessionId, "item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
            SessionCache.AddItem(sessionId, "item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
        }

        //Get or create session.
        public void GetOrCreateSession()
        {
            var session = SessionCache.GetOrCreate(sessionId);

          Console.WriteLine(session.Print());
        }
        //Get item from existing session.
        public void GetItem()
        {
            string key = "item key 1";
            var entity = SessionCache.GetItemValue<EntitySample>(sessionId, key);

            if (entity == null)
                Console.WriteLine("entity null " + key);
            else
                Console.WriteLine(entity.Name);
        }
        //Copy item from session to cache.
        public void CopyTo()
        {
            string key = "item key 1";
            SessionCache.CopyTo(sessionId, key, key, timeout, true);
        }
        //Fetch item from current session to cache.
        public void FetchTo()
        {
            string key = "item key 2";
            SessionCache.FetchTo(sessionId, key, key, timeout, true);
        }

        //Remove item from current session
        public void RemoveItem()
        {
            string key = "item key 3";
            bool ok = SessionCache.RemoveItem(sessionId, key);

            Console.WriteLine(ok);
        }

        //Get or create session.
        public void RemoveSession()
        {
            SessionCache.Remove(sessionId);
        }
    }


}
