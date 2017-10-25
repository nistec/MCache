using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Channels;
using Nistec.Generic;
using Nistec.Channels.RemoteCache;
using System.Collections;
using Nistec.Logging;

namespace Nistec.Caching.Demo.Mass
{

    public class SyncCacheRemoteMass
    {

        static string itemName = "contactEntity";
        static string printField = "FirstName";
        static int LoopCount = 1000;
        static NetProtocol Protocol = NetProtocol.Tcp;
        static long ElapsedMilliseconds;
        static long TransComplete;

        static int GetRandomIndex()
        {
            return new Random().Next(1, 12);
        }

        static string[] GetRandomKey(int index)
        {
            string[] keys = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
            return new string[] { keys[index] };
        }

        static bool ValidateResult(int index, string result)
        {
            string[] keys = new string[] { "Gustavo", "Catherine", "Kim", "Humberto", "Pilar", "Frances", "Margaret", "Carla", "Jay", "Ronald", "Samuel", "James" };
            return result.Equals(keys[index]);
        }

        static string[] GetRandomWrongKey()
        {

            string[] keys = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };

            int index = new Random().Next(1, 12);

            return new string[] { "a" + keys[index] };
        }

        public static void SyncCacheTestMass(NetProtocol protocol, int count, int wrongCount = 0)
        {
            Protocol = protocol;

            Console.WriteLine("SyncCache...");

            Interlocked.Exchange(ref ElapsedMilliseconds, 0);
            Interlocked.Exchange(ref TransComplete, 0);

            LoopCount = count;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            Console.WriteLine();
            Console.WriteLine("SyncCache  Entity");

            var watch = Stopwatch.StartNew();
            int counter = 0;

            for (int i = 0; i < LoopCount; i++)
            {
                //sync
                //CacheRemoteGetTest(null);
                //CacheRemoteWrongTest(null);

                //async
                ThreadPool.QueueUserWorkItem(CacheRemoteGetTest);

                Thread.Sleep(10);
                counter++;
            }

            for (int i = 0; i < wrongCount; i++)
            {
                ThreadPool.QueueUserWorkItem(CacheRemoteWrongTest);

                Thread.Sleep(10);
                counter++;
            }

            watch.Stop();

            Console.WriteLine("SyncTestMass : " + watch.ElapsedMilliseconds);

            Console.WriteLine("Finished: ");

            Netlog.InfoFormat("SyncCacheRemote Finished counter : {0}, Milliseconds: {1}", counter, watch.ElapsedMilliseconds);


            int waitcount = 0;
            while (Interlocked.Read(ref TransComplete) < LoopCount)
            {
                Console.WriteLine("Wait...{0}", TransComplete);

                Thread.Sleep(100);
                waitcount++;
                if (waitcount > 100)
                    break;
            }
            Console.WriteLine("SyncCacheRemote summarize counter : {0}, Total Elapsed Milliseconds: {1}", counter, Interlocked.Read(ref ElapsedMilliseconds));


            Console.ReadKey();
        }


        static void PrintCache()
        {
            Console.WriteLine("PrintCache...");

            string[] keys = Nistec.Caching.Remote.ManagerApi.GetAllKeys();
            foreach (string s in keys)
            {
                Console.WriteLine(s);
            }
        }

        static object mlock = new object();

        static void CacheRemoteGetTest(object state)
        {
            try
            {
                int index = GetRandomIndex();
                var key = GetRandomKey(index);

                var watch = Stopwatch.StartNew();
                var entity = SyncCacheApi.Get(Protocol).GetRecord(itemName, key);

                watch.Stop();
                Console.WriteLine("SyncCacheRemote duration: " + watch.ElapsedMilliseconds);

                Interlocked.Add(ref ElapsedMilliseconds, watch.ElapsedMilliseconds);
                Interlocked.Increment(ref TransComplete);

                string result = string.Format("{0}", entity == null ? "Not found" : entity[printField]);

                bool isValid = (entity == null) ? false : ValidateResult(index, result);

                Console.WriteLine(result);

                if (entity == null)
                {
                    Netlog.Error("Test client item Not found:");
                }

                Netlog.InfoFormat("SyncCacheRemote : {0}, item: {1}, index: {2}, isValid: {3}", watch.ElapsedMilliseconds, result, index, isValid);
            }
            catch(Exception ex)
            {
                Netlog.ErrorFormat("SyncCacheRemote Error: {0}", ex.Message);
            }
        }

        static void CacheRemoteWrongTest(object state)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var entity = SyncCacheApi.Get(Protocol).GetRecord(itemName, GetRandomWrongKey());
                Console.WriteLine(entity == null ? "Not found" : entity[printField]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CacheRemoteWrongTest : " + ex.Message);

            }
            watch.Stop();

            Console.WriteLine("SyncCacheRemote : " + watch.ElapsedMilliseconds);

        }

    }
}
