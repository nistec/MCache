using Nistec.Channels.RemoteCache;
using Nistec.Serialization;
using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nistec.Channels;
using Nistec.Caching.Remote;
using Nistec.Caching;
using System.Data;
using API = Nistec.Caching.Remote;

namespace Nistec
{
    internal class CacheType
    {
        public const string cache = "remote-cache";
        public const string sync = "remote-sync";
        public const string session = "remote-session";
    }
    class Controller
    {
        public static void Run(string[] args)
        {
            NetProtocol cmdProtocol = NetProtocol.Tcp;
            string protocol = "tcp";
            string cmd = "";
            string cacheType = CacheType.cache;
            string cmdName = "";
            string cmdArg1 = "";

            DisplayMenu("menu", "", "");
            DisplayCacheTypeMenu();
            cacheType = GetCacheType(Console.ReadLine().ToLower(), cacheType);

            if (cacheType == "quit")
            {
                return;
            }
            Console.WriteLine("Current cache type : {0}.", cacheType);
            SetCommands();

            while (cmd != "quit")
            {
                Console.WriteLine("Enter command :");

                cmd = Console.ReadLine();

                try
                {

                    string[] cmdargs = SplitCmd(cmd);
                    cmdName = GetCommandType(cmdargs[0], cmdName);
                    cmdArg1 = GetCommandType(cmdargs[1], cmdArg1);

                    switch (cmdName.ToLower())
                    {
                        case "menu":
                            DisplayMenu("menu", "", "");
                            break;
                        case "menu-items":
                            DisplayMenu("menu-items", cacheType, "");
                            break;
                        case "cache-type":
                            DisplayCacheTypeMenu();
                            cacheType = GetCacheType(Console.ReadLine().ToLower(), cacheType);
                            Console.WriteLine("Current cache type : {0}.", cacheType);
                            break;
                        case "protocol":
                            Console.WriteLine("Choose protocol : tcp , pipe, http");
                            protocol = EnsureProtocol(Console.ReadLine().ToLower(), protocol);
                            cmdProtocol = GetProtocol(protocol,cmdProtocol);
                            Console.WriteLine("Current protocol : {0}.", protocol);
                            break;
                        case "args":
                            DisplayMenu("args", cacheType, cmdArg1);
                            break;
                        case "report":
                            CmdController.DoCommandManager(cmdArg1, cacheType);
                            break;
                        case "quit":

                            break;
                        default:
                            switch (cacheType)
                            {
                                case CacheType.cache:
                                    CmdController.DoCommandCache(cmdProtocol,cmdName, cmdArg1, cmdargs[2]);
                                    break;
                                case CacheType.sync:
                                    CmdController.DoCommandCacheSync(cmdProtocol,cmdName, cmdArg1, cmdargs[2]);
                                    break;
                                case CacheType.session:
                                    CmdController.DoCommandSession(cmdProtocol,cmdName, cmdArg1, cmdargs[2]);
                                    break;
                                default:
                                    Console.WriteLine("Unknown command!");
                                    break;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                }
                Console.WriteLine();
            }
        }

        static Dictionary<string, string> cmdCache = new Dictionary<string, string>();
        static Dictionary<string, string> cmdSyncCache = new Dictionary<string, string>();
        static Dictionary<string, string> cmdSessionCache = new Dictionary<string, string>();
        static Dictionary<string, string> cmdManager = new Dictionary<string, string>();
        static void SetCommands()
        {
            cmdCache.Add("GetValue", "key");
            cmdCache.Add("AddItem", "key, value, expiration");
            cmdCache.Add("RemoveItem", "key");
            cmdCache.Add("Reply", "text");

            cmdSyncCache.Add("GetRecord", "tableName, key");
            cmdSyncCache.Add("GetAllEntityNames", "");
            cmdSyncCache.Add("GetEntityKeys", "tableName");
            cmdSyncCache.Add("GetEntityItemsCount", "tableName");
            cmdSyncCache.Add("PrintEntityValues", "tableName");
            cmdSyncCache.Add("Refresh", "");
            cmdSyncCache.Add("RefreshItem", "tableName");
            cmdSyncCache.Add("Reset", "");
            cmdSyncCache.Add("Reply", "text");
            

            cmdSessionCache.Add("GetSessionItem", "sessionId, key");
            cmdSessionCache.Add("GetAllSessionsKeys", "sessionId");
            cmdSessionCache.Add("RemoveSession", "sessionId");
            cmdSessionCache.Add("Reply", "text");

            cmdManager.Add("Statistic", "cache-type");
            cmdManager.Add("CounterState", "cache-type");
            cmdManager.Add("CounterReset", "");
            cmdManager.Add("PrintLog", "");
        }

        static string EnsureArg(string arg)
        {
            if (arg == null)
                return "";
            return arg.Replace("/", "").ToLower();
        }
        static void DisplayCacheTypeMenu()
        {
            Console.WriteLine("Choose cache type : remote-cache, remote-sync, remote-session");
        }
        static void DisplayArgs(string cmdType, string arg)
        {
            string a = EnsureArg(arg);
            KeyValuePair<string, string> kv = new KeyValuePair<string, string>();
            switch (cmdType)
            {
                case CacheType.cache:
                    kv = cmdCache.Where(p => p.Key.ToLower() == a).FirstOrDefault();
                    break;
                case CacheType.sync:
                    kv = cmdSyncCache.Where(p => p.Key.ToLower() == a).FirstOrDefault();
                    break;
                case CacheType.session:
                    kv = cmdSessionCache.Where(p => p.Key.ToLower() == a).FirstOrDefault();
                    break;
            }

            if (kv.Key != null)
                Console.WriteLine("commands: {0} Arguments: {1}.", kv.Key, kv.Value);
            else
                Console.WriteLine("Bad commands: {0} Arguments: {1}.", cmdType, arg);
        }

        static void DisplayMenu(string cmdType, string cacheType, string arg)
        {
            //string menu = "cache-type: remote-cache, remote-sync, remote-session";
            //Console.WriteLine(menu);

            switch (cmdType)
            {
                case "menu":
                    Console.WriteLine("Enter: cache-type, To change cache type");
                    Console.WriteLine("Enter: protocol, To change protocol (tcp, pipe, http)");
                    Console.WriteLine("Enter: menu, To display menu");
                    Console.WriteLine("Enter: menu-items, To display menu items for current cache-type");
                    Console.WriteLine("Enter: args, and /command to display command argument");
                    Console.WriteLine("Enter: report, and /command to display cache report");

                    break;
                case "menu-items":
                    DisplayMenuItems(cmdType, cacheType);
                    break;
                case "args":
                    if (arg != null && arg.StartsWith("/"))
                    {
                        DisplayArgs(cacheType, arg);
                    }
                    break;
            }
            Console.WriteLine("");

        }

        static void DisplayMenuItems(string cmdType, string cacheType)
        {
            switch (cacheType)
            {
                case CacheType.cache:
                    Console.Write("{0} commands: ", cacheType);
                    foreach (var entry in cmdCache)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                case CacheType.sync:
                    Console.Write("{0} commands: ", cacheType);
                    foreach (var entry in cmdSyncCache)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                case CacheType.session:
                    Console.Write("{0} commands: ", cacheType);
                    foreach (var entry in cmdSessionCache)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                default:
                    Console.Write("Bad commands: Invalid cache-type");
                    break;
            }
            Console.WriteLine();

        }

        static string[] SplitCmd(string cmd)
        {
            string[] args = new string[4] { "", "", "", "" };

            string[] cmdargs = cmd.SplitTrim(' ');
            if (cmdargs.Length > 0)
                args[0] = cmdargs[0];
            if (cmdargs.Length > 1)
                args[1] = cmdargs[1];
            if (cmdargs.Length > 2)
                args[2] = cmdargs[2];
            if (cmdargs.Length > 3)
                args[3] = cmdargs[3];
            return args;
        }

        static string GetCacheType(string cmd, string curItem)
        {
            switch (cmd.ToLower())
            {
                case CacheType.cache:
                case CacheType.sync:
                case CacheType.session:
                    return cmd.ToLower();
                default:
                    Console.WriteLine("Invalid cache-type {0}", cmd);
                    return curItem;
            }
        }
        static string GetCommandType(string cmd, string curItem)
        {
            if (cmd == "..")
                return curItem;
            return cmd;
        }

        static string EnsureProtocol(string protocol, string curProtocol)
        {
            switch (protocol.ToLower())
            {
                case "tcp":
                case "pipe":
                case "http":
                    return protocol.ToLower();
                default:
                    return curProtocol;
            }
        }

        static NetProtocol GetProtocol(string protocol, NetProtocol curProtocol)
        {
            switch (protocol.ToLower())
            {
                case "tcp":
                    return NetProtocol.Tcp;
                case "pipe":
                    return NetProtocol.Pipe;
                case "http":
                    return NetProtocol.Http;
                default:
                    return curProtocol;
            }
        }

        public static int GetUsage()
        {

            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("NistecCache");
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

    #region Cmd Controller
    class CmdController
    {
        public static void DoCommandCache(NetProtocol cmdProtocol,string cmd, string key, string value)
        {
            bool ok = true;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                switch (cmd.ToLower())
                {
                    case "getvalue":
                        var json = API.CacheApi.Get(cmdProtocol).GetJson(key, JsonFormat.Indented);
                        Display(cmd,json);
                        break;
                    case "additem":
                        var res = API.CacheApi.Get(cmdProtocol).AddItem(key, value, 0);
                        Display(cmd, res.ToString());
                        break;
                    case "removeitem":
                        API.CacheApi.Get(cmdProtocol).RemoveItem(key);
                        Display(cmd, "Cache item will remove.");
                        break;
                    case "reply":
                        var r = API.CacheApi.Get(cmdProtocol).Reply(key);
                        Display(cmd, key);
                        break;
                    default:
                        ok = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Console.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                watch.Stop();
                if (ok)
                    Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
            }
        }
        public static void DoCommandCacheSync(NetProtocol cmdProtocol,string cmd, string name, string keys)
        {
            Stopwatch watch = Stopwatch.StartNew();
            bool ok = true;
            try
            {
                switch (cmd.ToLower())
                {
                    case "getjson":
                        {
                            var json = API.SyncCacheApi.Get(cmdProtocol).GetJson(name, keys.Split(';'), JsonFormat.Indented);
                            Display(cmd, json);
                        }
                        break;
                    case "getrecord":
                        {
                            //var record = CacheApi.Sync.GetEntity<ContactEntity>(name, keys.Split(';'));

                            var record = API.SyncCacheApi.Get(cmdProtocol).GetRecord(name, keys.Split(';'));
                            var json = JsonSerializer.Serialize(record, null, JsonFormat.Indented);
                            Display(cmd, json);
                        }
                        break;
                    case "getallentitynames":
                        var names = API.SyncCacheApi.Get(cmdProtocol).GetAllEntityNames().ToArray();
                        DisplayArray(cmd,names);
                        break;
                    case "getentitykeys":
                        var ks = API.SyncCacheApi.Get(cmdProtocol).GetEntityKeys(name).ToArray();
                        DisplayArray(cmd, ks);
                        break;
                    case "printentityvalues":
                        {
                            var arr = API.SyncCacheApi.Get(cmdProtocol).GetEntityKeys(name).ToArray();
                            if (arr == null || arr.Length == 0)
                            {
                                Display(cmd, "items not found!");
                            }
                            else
                            {
                                int count = Types.ToInt(keys);
                                if (count <= 0)
                                    count = 1;
                                for (int i = 0; i < count; i++)
                                {
                                    foreach (var k in arr)
                                    {
                                        var record = API.SyncCacheApi.Get(cmdProtocol).GetRecord(name, k.Split(';'));
                                        var json = JsonSerializer.Serialize(record, null, JsonFormat.Indented);
                                        Display(cmd, json);
                                    }

                                    Display(cmd, "finished items: " + arr.Length.ToString());
                                }
                            }
                        }
                        break;
                    case "getentityitemscount":
                        {
                            var count = API.SyncCacheApi.Get(cmdProtocol).GetEntityItemsCount(name);
                            Display(cmd, count.ToString());
                        }
                        break;
                    case "refreshitem":
                        API.SyncCacheApi.Get(cmdProtocol).Refresh(name);
                        Display(cmd, "Refresh sync cache item started");
                        break;
                    case "refresh":
                        API.SyncCacheApi.Get(cmdProtocol).Refresh();
                        Display(cmd, "Refresh all sync cache items started");
                        break;
                    case "reset":
                        API.SyncCacheApi.Get(cmdProtocol).Reset();
                        Display(cmd, "Sync cache restart");
                        break;
                    case "reply":
                        var r = API.SyncCacheApi.Get(cmdProtocol).Reply(name);
                        Display(cmd, name);
                        break;

                    default:
                        ok = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Console.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                watch.Stop();
                if (ok)
                    Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
            }
        }
        public static void DoCommandSession(NetProtocol cmdProtocol,string cmd, string sessionId, string key)
        {

            bool ok = true;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                switch (cmd.ToLower())
                {
                    case "getsessionitem":
                        var json = API.SessionCacheApi.Get(cmdProtocol).GetJson(sessionId, key, JsonFormat.Indented);
                        Display(cmd, json);
                        break;
                    case "getallsessionskeys":
                        var ks = API.SessionCacheApi.Get(cmdProtocol).GetAllSessionsKeys().ToArray();
                        DisplayArray(cmd, ks);
                        break;
                    case "removesession":
                        API.SessionCacheApi.Get(cmdProtocol).RemoveSession(sessionId);
                        Display(cmd, "session {0} will remove", sessionId);
                        break;
                    case "removeitem":
                        API.SessionCacheApi.Get(cmdProtocol).Remove(sessionId, key);
                        Display(cmd, "session item {0},{1} will remove", sessionId, key);
                        break;
                    case "reply":
                        var r = API.SessionCacheApi.Get(cmdProtocol).Reply(key);
                        Display(cmd, key);
                        break;
                    default:
                        ok = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Console.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                watch.Stop();
                if (ok)
                    Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
            }
        }

        public static void DoCommandManager(string cmd, string key)
        {

            bool ok = true;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                switch (cmd.ToLower())
                {
                    case "statistic":
                        DoPerformanceReport(key);
                        break;
                    case "counterstate":
                        DoPerformanceStateReport(key);
                        break;
                    case "counterreset":
                        DoResetPerformanceCounter();
                        break;
                    case "printlog":
                        DoLog();
                        break;
                    default:
                        ok = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Console.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                watch.Stop();
                if (ok)
                    Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
            }
        }

        static void Display(string cmd, string val)
        {
            Console.WriteLine("command - {0} :", cmd);
            Console.WriteLine(val);
        }
        static void Display(string cmd, string val, params string[] args)
        {
            Console.WriteLine("command - {0} :", cmd);
            Console.WriteLine(val, args);
        }
        static void DisplayArray(string cmd, string[] arr)
        {
            if (arr == null)
                Console.WriteLine("{0} not found", cmd);
            else
            {
                Console.WriteLine("command - {0} :", cmd);
                foreach (string s in arr)
                {
                    Console.WriteLine(s);
                }
            }

        }

        static void DoPerformanceReport(string cmdType)
        {
            ICachePerformanceReport report = null;
            if (cmdType == "all" || cmdType == null)
            {
                report = ManagerApi.GetPerformanceReport();
            }
            else
            {
                var agentType = GetCacheAgentType(cmdType);
                report = ManagerApi.GetAgentPerformanceReport(agentType);
            }
                        
            if (report == null)
            {
                Console.WriteLine("Invalid cache performance property");
            }
            else
            {
                Console.WriteLine("Cache Performance Report");
                var dt = report.PerformanceReport;

                var json = Nistec.Serialization.JsonSerializer.Serialize(dt, null, JsonFormat.Indented);
                Console.WriteLine(json);
            }
            Console.WriteLine();
        }


        static void DoPerformanceStateReport(string cmdType)
        {
            DataTable report = null;
            if (cmdType == "all" || cmdType == null)
            {
                report = ManagerApi.GetStateCounterReport();
            }
            else
            {
                var agentType = GetCacheAgentType(cmdType);
                report = ManagerApi.GetStateCounterReport(agentType);
            }

            if (report == null)
            {
                Console.WriteLine("Invalid cache performance property");
            }
            else
            {
                Console.WriteLine("Cache State Report");
                var json = Nistec.Serialization.JsonSerializer.Serialize(report, null, JsonFormat.Indented);
                Console.WriteLine(json);
            }
            Console.WriteLine();
        }

        static void DoResetPerformanceCounter()
        {
            ManagerApi.ResetPerformanceCounter();
        }

        static void DoLog()
        {
            string log = ManagerApi.CacheLog();
            Console.WriteLine("Cache Log");
            Console.WriteLine(log);
            Console.WriteLine();
        }

        static CacheAgentType GetCacheAgentType(string cmdType)
        {
            switch (cmdType)
            {
                case CacheType.cache:
                    return CacheAgentType.Cache;
                case CacheType.sync:
                    return CacheAgentType.SyncCache;
                case CacheType.session:
                    return CacheAgentType.SessionCache;
                default:
                    return CacheAgentType.Cache;
            }
        }
    }
    #endregion
}
