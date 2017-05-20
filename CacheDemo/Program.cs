using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using Nistec.Caching.Demo.Remote;
using Nistec.Caching.Demo.Hosted;
using Nistec.Caching.Demo.Mass;
using Nistec.Caching.Demo.Entities;
using Nistec.IO;
using Nistec.Channels;

namespace Nistec.Caching.Demo
{

      class Program
    {

          [STAThread]
          static void Main(string[] args)
          {

              Console.WriteLine("Start test...");
             
              //string mode = "remote-sync";
              string protocol="tcp";
              string cmd="";
              string menu = "commands: remote-cache, remote-sync,remote-sync-mass, remote-api, remote-session";
              NetProtocol netProtocol = NetProtocol.Tcp;
              
              do
              {
                  Console.WriteLine("Choos protocol : tcp , pipe");
                  protocol = Console.ReadLine().ToLower();
                  netProtocol = GetProtocol(protocol);
              }
              while (netProtocol == NetProtocol.NA);
             
              while (cmd != "quit")
              {

                  Console.WriteLine(menu);
                  cmd = Console.ReadLine().ToLower();

                  switch (cmd)
                  {
                      case "remote-cache":
                          Nistec.Caching.Demo.Remote.CacheTest.TestAll(netProtocol);
                          break;
                      case "remote-sync":
                          Nistec.Caching.Demo.Remote.SyncCacheTest.TestAll(netProtocol);
                          break;
                      case "remote-session":
                          Nistec.Caching.Demo.Remote.SessionCacheTest.TestAll(netProtocol);
                          break;
                      case "remote-data":
                          Nistec.Caching.Demo.Remote.DataCacheTest.TestAll(netProtocol);
                          break;
                      case "remote-sync-mass":
                          Console.WriteLine("Write count");
                          int okCount=Types.ToInt( Console.ReadLine(),1000);

                          Console.WriteLine("Write wrong count");
                          int wrongCount=Types.ToInt( Console.ReadLine(),0);

                          Nistec.Caching.Demo.Mass.SyncCacheRemoteMass.SyncCacheTestMass(netProtocol,okCount, wrongCount);
                          break;
                      case "remote-api":
                          Nistec.Caching.Demo.RemoteApi.CacheTest.TestAll(netProtocol);
                          break;
                      case "hosted-cache":
                          HostedCacheTest.TestAll();
                          break;
                      case "hosted-sync":
                          HostedSyncTest.TestAll();
                          break;
                      case "hosted-session":
                          HostedSessionTest.TestAll();
                          break;
                      case "hosted-data":
                          HostedDataCacheTest.TestAll();
                          break;
                      case "quit":
                          break;
                      default:
                          Console.WriteLine("Unknown command!");
                          break;
                  }
                  Console.WriteLine("Finished...");
              }
              Console.WriteLine("Finished an quit...");
              Console.ReadLine();


          }

          static NetProtocol GetProtocol(string protocol)
          {
              switch(protocol.ToLower())
              {
                  case "pipe":
                      return NetProtocol.Pipe;
                  case "tcp":
                      return NetProtocol.Tcp;
                  default:
                      return NetProtocol.NA;
              }
          }
          public static int GetUsage()
          {

              System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("McRemoteCache");
              int usage = 0;
              if (process == null)
                  return 0;
              for (int i = 0; i < process.Length; i++)
              {
                  usage += (int)((int)process[i].WorkingSet64) / 1024;
              }

              return usage;
          }

    }
}
