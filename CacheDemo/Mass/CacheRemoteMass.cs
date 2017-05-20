using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Nistec.Caching.Remote;
using Nistec.Caching.Server;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Channels;
using Nistec.Serialization;

namespace Nistec.Caching.Demo.Mass
{
    [Serializable]
    public class CacheEntityDemo
    {
        public string Name{get;set;}
        public int ID { get; set; }
    }


    public class CacheRemoteMass
    {
 
        public static readonly MCache Cache = new MCache("demo");

        static NetProtocol Protocol;

        public static void CacheTestMass(NetProtocol protocol)
        {
            Protocol = protocol;

            Console.WriteLine("SyncCache...");

            CacheRemoteAddTest();

            int LOOP = 100;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            Console.WriteLine();
            Console.WriteLine("Cache  Entity");

            var watch = Stopwatch.StartNew();

            for (int i = 0; i < LOOP; i++)
            {
                //sync
                //CacheRemoteGetTest(null);
                //CacheRemoteWrongTest(null);

                //async
                ThreadPool.QueueUserWorkItem(CacheRemoteGetTest);

                Thread.Sleep(10);
            }

            watch.Stop();

            Console.WriteLine("TestMass : " + watch.ElapsedMilliseconds);

            Console.WriteLine("Finished: ");

            Console.ReadKey();
        }


        static void CacheRemoteAddTest()
        {
            
            CacheApi.Get(Protocol).AddItem("ABEntity", new CacheEntityDemo() { ID = 1234, Name = "nissim" }, 30);
            PrintCache();
            CacheApi.Get(Protocol).AddItem("CDEntity", new CacheEntityDemo() { ID = 2345, Name = "neomi" }, 30);
            PrintCache();
            CacheApi.Get(Protocol).AddItem("EFEntity", new CacheEntityDemo() { ID = 3456, Name = "liron" }, 30);
            PrintCache();
            CacheApi.Get(Protocol).AddItem("shaniEntity", new CacheEntityDemo() { ID = 4567, Name = "shani" }, 30);
            PrintCache();
            CacheApi.Get(Protocol).AddItem("karinEntity", new CacheEntityDemo() { ID = 5678, Name = "karin" }, 30);
            PrintCache();
            
        }

        static void PrintCache()
        {
            Console.WriteLine("PrintCache...");

            string[] keys=ManagerApi.GetAllKeys();
            foreach (string s in keys)
            {
                Console.WriteLine(s);
            }
        }

        public static void CacheRemoteGetTest(object state)
        {
            var watch = Stopwatch.StartNew();

            var entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("ABEntity");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);
            entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("CDEntity");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);
            entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("EFEntity");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);
            entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("shaniEntity");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);
            entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("karinEntity");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);

            watch.Stop();

            Console.WriteLine("CacheRemote : " + watch.ElapsedMilliseconds);
 
        }

        public static void CacheRemoteWrongTest(object state)
        {
            var watch = Stopwatch.StartNew();

            var entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("nissimEntity1");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);
            entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("neomiEntity1");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);
            entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("lironEntity1");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);
            entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("shaniEntity1");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);
            entity = CacheApi.Get(Protocol).GetValue<CacheEntityDemo>("karinEntity1");
            Console.WriteLine(entity == null ? "Not found" : entity.Name);

            watch.Stop();

            Console.WriteLine("CacheRemote : " + watch.ElapsedMilliseconds);

        }

        public static void CacheRemoteLocalTest()
        {
            AgentManager.Cache.ExecRemote(new CacheMessage(CacheCmd.AddItem, "ABEntity", new CacheEntityDemo() { ID = 1234, Name = "nissim" }, 30));
            AgentManager.Cache.ExecRemote(new CacheMessage(CacheCmd.AddItem, "CDEntity", new CacheEntityDemo() { ID = 2345, Name = "neomi" }, 30));
            AgentManager.Cache.ExecRemote(new CacheMessage(CacheCmd.AddItem, "EFEntity", new CacheEntityDemo() { ID = 3456, Name = "liron" }, 30));

           NetStream b = AgentManager.Cache.ExecRemote(new CacheMessage() { Command = CacheCmd.GetValue, Key = "ABEntity" });
           var entity = new BinarySerializer().Deserialize<CacheEntityDemo>(b);

            b = AgentManager.Cache.ExecRemote(new CacheMessage() { Command = CacheCmd.GetValue, Key = "nissimEntity1" });
            entity = new BinarySerializer().Deserialize<CacheEntityDemo>(b);

        }
    }
}
