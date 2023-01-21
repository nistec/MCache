﻿using System;
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
        SessionCacheApi api;
        public static void TestAll(NetProtocol protocol, bool enableRemove = true)
        {
            SessionCacheTest test = new SessionCacheTest() { Protocol = protocol, api = SessionCacheApi.Get(protocol) };

            test.AddSession();
            test.AddItems();
            test.GetOrCreateSession();
            test.GetItem();
            test.CopyTo();
            test.CutTo();
            if (enableRemove)
            {
                test.RemoveItem();
                test.RemoveSession();
            }
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

        //Create new session.
        public void AddSession()
        {
            try
            {
                var api = SessionCacheApi.Get(Protocol);
                api.CreateSession(sessionId, userId, timeout, null);
                api.Set(sessionId, "item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);

                Thread.Sleep(100);
                var session = api.GetOrCreateSession(sessionId);

                Console.WriteLine(session.Print());
                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }

        }

        //Add items to current session.
        public void AddItems()
        {
            try
            {
                var api = SessionCacheApi.Get(Protocol);

                for (int i = 0; i < 20; i++)
                {
                    var dt = AccountDocsEntityContext.GetList();
                    api.Set("A-" + sessionId, "contact " + (i + 100).ToString(), new EntitySample() { Id = 123, Name = "entity sample " + i, Creation = DateTime.Now, Value = dt }, timeout);
                }
                for (int i = 0; i < 20; i++)
                {
                    var dt = AccountDocsEntityContext.GetList();
                    api.Set("B-" + sessionId, "contact " + (i + 100).ToString(), new EntitySample() { Id = 123, Name = "entity sample " + i, Creation = DateTime.Now, Value = dt }, timeout);
                }
                for (int i = 0; i < 20; i++)
                {
                    var dt = AccountDocsEntityContext.GetList();
                    api.Set("C-" + sessionId, "contact " + (i + 100).ToString(), new EntitySample() { Id = 123, Name = "entity sample " + i, Creation = DateTime.Now, Value = dt }, timeout);
                }

                CacheState state = CacheState.UnKnown;
                state = api.Set(sessionId, "item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
                Console.WriteLine("session.Set: " + state.ToString());
                state = api.Set(sessionId, "item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
                Console.WriteLine("session.Set: " + state.ToString());
                state = api.Set(sessionId, "item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
                Console.WriteLine("session.Set: " + state.ToString());

                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }

        }

        //Get or create session.
        public void GetOrCreateSession()
        {
            try
            {
                var session = api.GetOrCreateSession(sessionId);
                Print(session.Print(), sessionId, "GetOrCreateSession");
                //Console.WriteLine(session.Print());
                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }

        }
        //Get item from existing session.
        public void GetItem()
        {
            try
            {
                string key = "item key 1";
                var entity = api.GetValue<EntitySample>(sessionId, key);
                Print(entity, key, "GetItem");

                //if (entity == null)
                //    Console.WriteLine("entity null " + key);
                //else
                //    Console.WriteLine(entity.Name);
                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }

        }
        //Copy item from session to cache.
        public void CopyTo()
        {
            try
            {
                string key = "item key 1";
                var state = api.CopyTo(sessionId, key, key, timeout, true);
                Console.WriteLine("session.CopyTo: " + state.ToString());

                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }
        }
        //Fetch item from current session to cache.
        public void CutTo()
        {
            try
            {
                string key = "item key 2";
                var state = api.CutTo(sessionId, key, key, timeout, true);
                Console.WriteLine("session.CutTo: " + state.ToString());

                GoOn();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }
        }

        //Remove item from current session
        public void RemoveItem()
        {
            try
            {
                string key = "item key 3";
                var state = api.Remove(sessionId, key);
                Console.WriteLine("session.RemoveItem: " + state.ToString());

                //Console.WriteLine(state);
                GoOn();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }

        }

        //remove session with items.
        public void RemoveSession()
        {
            try
            {
                var state = api.RemoveSession(sessionId);
                Console.WriteLine("session.RemoveItem: " + state.ToString());

                GoOn();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Error: " + ex.Message);
            }

        }
    }
}

