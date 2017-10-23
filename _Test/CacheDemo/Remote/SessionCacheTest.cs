using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Caching.Remote;
using Nistec.Caching.Demo.Entities;
using System.Threading;
using Nistec.Channels;

namespace Nistec.Caching.Demo.Remote
{
    public class SessionCacheTest
    {
        string sessionId = "12345678";
        string userId = "12";
        int timeout = 0;
        NetProtocol Protocol;
        public static void TestAll(NetProtocol protocol)
        {
            SessionCacheTest test = new SessionCacheTest() { Protocol = protocol };

            test.AddSession();
            test.AddItems();
            test.GetOrCreateSession();
            test.GetItem();
            test.CopyTo();
            test.FetchTo();
            test.RemoveItem();
            test.RemoveSession();
        }
        //Create new session.
        public void AddSession()
        {
            var api = SessionCacheApi.Get(Protocol);
            api.AddSession(sessionId, userId, timeout, null);
            api.Set(sessionId, "item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);

            Thread.Sleep(100);
            var session = SessionCacheApi.Get(Protocol).GetExistingSession(sessionId);

            Console.WriteLine(session.Print());
        }

        //Add items to current session.
        public void AddItems()
        {
            var api = SessionCacheApi.Get(Protocol);

            for (int i = 0; i < 20; i++)
            {
                var dt = ContactEntityContext.GetList();
                api.Set(sessionId, "contact " + (i + 100).ToString(), new EntitySample() { Id = 123, Name = "entity sample " + i, Creation = DateTime.Now, Value = dt }, timeout);
            }

            api.Set(sessionId, "item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
            api.Set(sessionId, "item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
            api.Set(sessionId, "item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
        }

        //Get or create session.
        public void GetOrCreateSession()
        {
            var session = SessionCacheApi.Get(Protocol).GetOrCreate(sessionId);

            Console.WriteLine(session.Print());
        }
        //Get item from existing session.
        public void GetItem()
        {
            string key = "item key 1";
            var entity = SessionCacheApi.Get(Protocol).Get<EntitySample>(sessionId, key);

            if (entity == null)
                Console.WriteLine("entity null " + key);
            else
                Console.WriteLine(entity.Name);
        }
        //Copy item from session to cache.
        public void CopyTo()
        {
            string key = "item key 1";
            SessionCacheApi.Get(Protocol).CopyTo(sessionId, key, key, timeout, true);
        }
        //Fetch item from current session to cache.
        public void FetchTo()
        {
            string key = "item key 2";
            SessionCacheApi.Get(Protocol).FetchTo(sessionId, key, key, timeout, true);
        }

        //Remove item from current session
        public void RemoveItem()
        {
            string key = "item key 3";
            bool ok = SessionCacheApi.Get(Protocol).Remove(sessionId, key);

            Console.WriteLine(ok);
        }

        //remove session with items.
        public void RemoveSession()
        {
            SessionCacheApi.Get(Protocol).RemoveSession(sessionId, false);
        }
    }
}
