//licHeader
//===============================================================================================================
// System  : Nistec.Cache - Nistec.Cache Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Channels.Tcp;
using Nistec.Channels;
using System.IO.Pipes;

namespace Nistec.Caching.Config
{
   

    /// <summary>
    /// Represent the cache settings as read only.
    /// </summary>
    public class CacheSettings
    {
        /// <summary>EnableDynamic.</summary>
        public static bool EnableDynamic { get; private set; } = false;

        /// <summary>EnableConnectionProvider.</summary>
        public static bool EnableConnectionProvider { get; private set; } = true;

        //all|list|table
        //public static string SyncEmbedEntityEventMode { get; private set; } = "all";
        
        /// <summary>EnableSyncTypeEvent.</summary>
        public static bool EnableSyncTypeEventTrigger { get; private set; } = true;
        /// <summary>MaxSize.</summary>
        public static long MaxSize { get; private set; } = CacheDefaults.DefaultCacheMaxSize;
        /// <summary>DefaultExpiration.</summary>
        public static int DefaultExpiration { get; private set; } = 30;
        /// <summary>RemoveExpiredItemOnSync.</summary>
        public static bool RemoveExpiredItemOnSync { get; private set; } = true;
        /// <summary>Sync Interval in seconds.</summary>
        public static int SyncInterval { get; private set; } = CacheDefaults.DefaultIntervalSeconds;
        /// <summary>SyncBox Interval in seconds.</summary>
        public static int SyncBoxInterval { get; private set; } = CacheDefaults.DefaultIntervalSeconds;
        /// <summary>SyncOption.</summary>
        public static string SyncOption { get; private set; } = "Auto";
        /// <summary>SessionTimeout.</summary>
        public static int SessionTimeout { get; private set; } = CacheDefaults.DefaultSessionTimeout;
        /// <summary>MaxSessionTimeout.</summary>
        public static int MaxSessionTimeout { get; private set; } = 1440;
        /// <summary>EnableLog file.</summary>
        public static bool EnableLog = false;
 
        /// <summary>LogActionDebugEnabled.</summary>
        public static bool LogMonitorDebugEnabled { get; private set; } = true;
        /// <summary>LogActionCapacity.</summary>
        public static int LogMonitorCapacityLines { get; private set; } = 1000;

        /// <summary>SyncConfigFile.</summary>
        public static string SyncConfigFile { get; private set; } = "";
        /// <summary>DbConfigFile.</summary>
        public static string DbConfigFile { get; private set; } = "";
        /// <summary>EnableSyncFileWatcher.</summary>
        public static bool EnableSyncFileWatcher { get; private set; } = false;
        /// <summary>ReloadSyncOnChange.</summary>
        public static bool ReloadSyncOnChange { get; private set; } = false;
        /// <summary>SyncTaskerTimeout.</summary>
        public static int SyncTaskerTimeout { get; private set; } = 60;
        /// <summary>EnableAsyncTask.</summary>
        public static bool EnableAsyncTask { get; private set; } = true;
        /// <summary>EnableAsyncLoader.</summary>
        public static bool EnableAsyncLoader { get; private set; } = true;
        /// <summary>EnableSyncTypeEvent.</summary>
        public static bool EnableSyncTypeEvent { get; private set; } = false;

        /// <summary>Get the interval in hours for auto reset performance counter.</summary>
        public static int AutoResetIntervalHours { get; private set; } = CacheDefaults.DefaultAutoResetIntervalHours;
        

    /// <summary>EnableSizeHandler.</summary>
    public static bool EnableSizeHandler { get; private set; } = false;
        /// <summary>EnablePerformanceCounter.</summary>
        public static bool EnablePerformanceCounter { get; private set; } = false;

        /// <summary>EnablePipeBundle.</summary>
        public static bool EnablePipeBundle { get; private set; } = false;
        /// <summary>EnableTcpBundle.</summary>
        public static bool EnableTcpBundle { get; private set; } = false;
        /// <summary>EnableHttpBundle.</summary>
        public static bool EnableHttpBundle { get; private set; } = false;

        ///// <summary>EnablePipeBundle.</summary>
        //public readonly static bool EnablePipeJsonBundle = false;
        ///// <summary>EnableTcpBundle.</summary>
        //public readonly static bool EnableTcpJsonBundle = false;
        ///// <summary>EnableHttpBundle.</summary>
        //public readonly static bool EnableHttpJsonBundle = false;

        //internal static long GetValidCacheMaxSize(long maxSize)
        //{
        //    return maxSize < CacheDefaults.MinCacheMaxSize ? CacheDefaults.DefaultCacheMaxSize : maxSize;
        //}

        internal static int GetValidSessionTimeout(int timeout)
        {
            return timeout== -1 ? MaxSessionTimeout: timeout <= 0 ? SessionTimeout : timeout;
        }


        #region BundleFormatter

        /// <summary>PipeBundleFormatter.</summary>
        public static BundleFormatter PipeBundleFormatter { get; private set; } = BundleFormatter.NA;
        /// <summary>TcpBundleFormatter.</summary>
        public static BundleFormatter TcpBundleFormatter { get; private set; } = BundleFormatter.NA;
        /// <summary>HttpBundleFormatter.</summary>
        public static BundleFormatter HttpBundleFormatter { get; private set; } = BundleFormatter.NA;

        internal static BundleFormatter GetBundleFormatter(string formatter)
        {
            return EnumExtension.Parse<BundleFormatter>(formatter, BundleFormatter.NA);
        }

        //internal static BundleFormatter GetBundleFormatter(string formatter)
        //{
        //    BundleFormatter modFlags = BundleFormatter.NA;
        //    if (!string.IsNullOrEmpty(formatter))
        //    {
        //        BundleFormatter[] mflags = EnumExtension.GetEnumFlags<BundleFormatter>(formatter, BundleFormatter.NA);
        //        foreach (BundleFormatter flg in mflags)
        //        {
        //            modFlags = modFlags | flg;
        //        }
        //    }
        //    return modFlags;
        //}
        ///// <summary>
        ///// Has Formatter Flag
        ///// </summary>
        ///// <param name="e"></param>
        ///// <param name="flag"></param>
        ///// <returns></returns>
        //public static bool HasFormatterFlag(BundleFormatter e, Enum flag)
        //{
        //    return e.HasFlag(flag);
        //}
        #endregion

        #region NetProtocol

        /// <summary>EnableRemoteCache.</summary>
        public static NetProtocol RemoteCacheProtocol { get; private set; } = NetProtocol.NA;
        /// <summary>EnableSyncCache.</summary>
        public static NetProtocol SyncCacheProtocol { get; private set; } = NetProtocol.NA;
        /// <summary>EnableSessionCache.</summary>
        public static NetProtocol SessionCacheProtocol { get; private set; } = NetProtocol.NA;
        /// <summary>EnableDataCache.</summary>
        public static NetProtocol DataCacheProtocol { get; private set; } = NetProtocol.NA;
        /// <summary>EnableCacheManager.</summary>
        public static NetProtocol CacheManagerProtocol { get; private set; } = NetProtocol.NA;


        internal static NetProtocol GetNetProtocol(string protocol)
        {
            NetProtocol modFlags = NetProtocol.NA;
            if (!string.IsNullOrEmpty(protocol))
            {
                NetProtocol[] mflags = EnumExtension.GetEnumFlags<NetProtocol>(protocol, NetProtocol.NA);
                foreach (NetProtocol flg in mflags)
                {
                    modFlags = modFlags | flg;
                }
            }
            return modFlags;
        }

        internal static NetProtocol[] GetSupportProtocol(string protocol)
        {
            if (!string.IsNullOrEmpty(protocol))
            {
                NetProtocol[] mflags = EnumExtension.GetEnumFlags<NetProtocol>(protocol, NetProtocol.NA);
                return mflags;
            }

            return new NetProtocol[] { NetProtocol.NA };
        }

        /// <summary>
        /// Has Protocol Flag
        /// </summary>
        /// <param name="e"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool HasProtocolFlag(NetProtocol e, Enum flag)
        {
            return e.HasFlag(flag);
        }
        #endregion

        #region NetFormatter
        /*
        /// <summary>EnableRemoteCache.</summary>
        public readonly static NetFormatter RemoteCacheFormatter = NetFormatter.Binary;
        /// <summary>EnableSyncCache.</summary>
        public readonly static NetFormatter SyncCacheFormatter = NetFormatter.Binary;
        /// <summary>EnableSessionCache.</summary>
        public readonly static NetFormatter SessionCacheFormatter = NetFormatter.Binary;
        /// <summary>EnableDataCache.</summary>
        public readonly static NetFormatter DataCacheFormatter = NetFormatter.Binary;


        internal static NetFormatter GetNetFormatter(string formatter)
        {
            NetFormatter modFlags = NetFormatter.Binary;
            if (!string.IsNullOrEmpty(formatter))
            {
                NetFormatter[] mflags = EnumExtension.GetEnumFlags<NetFormatter>(formatter, NetFormatter.Binary);
                foreach (NetFormatter flg in mflags)
                {
                    modFlags = modFlags | flg;
                }
            }
            return modFlags;
        }
        /// <summary>
        /// Has Formatter Flag
        /// </summary>
        /// <param name="e"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool HasFormatterFlag(NetFormatter e, Enum flag)
        {
            return e.HasFlag(flag);
        }
        */
        #endregion

        static CacheSettings()
        {
            var section = CacheConfigServer.GetConfig();
            if (section == null)
            {
                throw new ArgumentException("CacheSettings.GetConfig");
            }
            LoadCacheSettings(section.CacheSettings, false);
        }

        internal static void LoadCacheSettings(NetConfigItems table, bool isReload)
        {
            //XmlTable table = NetConfig.GetCustomConfig("CacheSettings");
            if (table == null)
            {
                throw new ArgumentException("Could not load XmlTable config");
            }

            MaxSize = table.Get<long>("MaxSize", CacheDefaults.DefaultCacheMaxSize);
            if (MaxSize <= 0)
                MaxSize = CacheDefaults.DefaultCacheMaxSize;
            if (MaxSize > CacheDefaults.CacheMaxSizeLimit)
                MaxSize = CacheDefaults.CacheMaxSizeLimit;

            DefaultExpiration = table.Get<int>("DefaultExpiration", CacheDefaults.DefaultCacheExpiration);
            if (DefaultExpiration <= 0)
                DefaultExpiration = CacheDefaults.DefaultCacheExpiration;

            RemoveExpiredItemOnSync = table.Get<bool>("RemoveExpiredItemOnSync", true);
            SyncInterval = table.Get<int>("SyncInterval", 60);
            SyncBoxInterval = table.Get<int>("SyncBoxInterval", 60);

            SessionTimeout = table.Get<int>("SessionTimeout", CacheDefaults.DefaultSessionTimeout);
            if (SessionTimeout <= 0)
                SessionTimeout = CacheDefaults.DefaultSessionTimeout;

            MaxSessionTimeout = table.Get<int>("MaxSessionTimeout", CacheDefaults.DefaultMaxSessionTimeout);//1 month 1440);
            if (MaxSessionTimeout <= 0)
                MaxSessionTimeout = CacheDefaults.DefaultMaxSessionTimeout;

            EnableLog = table.Get<bool>("EnableLog", false);

            LogMonitorDebugEnabled = table.Get<bool>("LogMonitorDebugEnabled", false);
            LogMonitorCapacityLines = table.Get<int>("LogMonitorCapacityLines", 1000);
            if (LogMonitorCapacityLines > 10000)
                LogMonitorCapacityLines = 10000;

            CacheLogger.debugEnabled = LogMonitorDebugEnabled;
            CacheLogger.logCapacity = LogMonitorCapacityLines;

            SyncConfigFile = table.Get("SyncConfigFile");
            DbConfigFile = table.Get("DbConfigFile");
            EnableSyncFileWatcher = table.Get<bool>("EnableSyncFileWatcher", false);
            ReloadSyncOnChange = table.Get<bool>("ReloadSyncOnChange", false);
            SyncTaskerTimeout = table.Get<int>("SyncTaskerTimeout", 60);
            EnableAsyncTask = table.Get<bool>("EnableAsyncTask", true);
            EnableAsyncLoader = table.Get<bool>("EnableAsyncLoader", true);
            EnableSyncTypeEvent = table.Get<bool>("EnableSyncTypeEvent", false);

            AutoResetIntervalHours = table.Get<int>("AutoResetIntervalHours", CacheDefaults.DefaultAutoResetIntervalHours);

            EnableSizeHandler = table.Get<bool>("EnableSizeHandler", false);
            EnablePerformanceCounter = table.Get<bool>("EnablePerformanceCounter", false);

            //EnableTcpBundle = table.Get<bool>("EnableTcpBundle", false);
            //EnablePipeBundle = table.Get<bool>("EnablePipeBundle", false);
            //EnableHttpBundle = table.Get<bool>("EnableHttpBundle", false);

            //EnablePipeJsonBundle = table.Get<bool>("EnablePipeJsonBundle", false);
            //EnableTcpJsonBundle = table.Get<bool>("EnableTcpJsonBundle", false);
            //EnableHttpJsonBundle = table.Get<bool>("EnableHttpJsonBundle", false);


            EnableSyncTypeEventTrigger = table.Get<bool>("EnableSyncTypeEventTrigger", true);
            EnableConnectionProvider = table.Get<bool>("EnableConnectionProvider", true);
            //SyncEmbedEntityEventMode = table.Get("SyncEmbedEntityEventMode", "all");//all|list|table
                       
            //MaxTcpBundlePool = table.Get<int>("MaxTcpBundlePool", 0);

            if (!isReload)
            {

                PipeBundleFormatter = GetBundleFormatter(table.Get("PipeBundleFormatter"));
                TcpBundleFormatter = GetBundleFormatter(table.Get("TcpBundleFormatter"));
                HttpBundleFormatter = GetBundleFormatter(table.Get("HttpBundleFormatter"));

                RemoteCacheProtocol = GetNetProtocol(table.Get("RemoteCacheProtocol"));
                SyncCacheProtocol = GetNetProtocol(table.Get("SyncCacheProtocol"));
                SessionCacheProtocol = GetNetProtocol(table.Get("SessionCacheProtocol"));
                DataCacheProtocol = GetNetProtocol(table.Get("DataCacheProtocol"));
                CacheManagerProtocol = GetNetProtocol(table.Get("CacheManagerProtocol"));

                //RemoteCacheFormatter = GetNetFormatter(table.Get("RemoteCacheFormatter"));
                //SyncCacheFormatter = GetNetFormatter(table.Get("SyncCacheFormatter"));
                //SessionCacheFormatter = GetNetFormatter(table.Get("SessionCacheFormatter"));
                //DataCacheFormatter = GetNetFormatter(table.Get("DataCacheFormatter"));

                EnableTcpBundle = TcpBundleFormatter != BundleFormatter.NA && (RemoteCacheProtocol.HasFlag(NetProtocol.Tcp) || SyncCacheProtocol.HasFlag(NetProtocol.Tcp) || SessionCacheProtocol.HasFlag(NetProtocol.Tcp) || DataCacheProtocol.HasFlag(NetProtocol.Tcp));
                EnablePipeBundle = PipeBundleFormatter != BundleFormatter.NA && (RemoteCacheProtocol.HasFlag(NetProtocol.Pipe) || SyncCacheProtocol.HasFlag(NetProtocol.Pipe) || SessionCacheProtocol.HasFlag(NetProtocol.Pipe) || DataCacheProtocol.HasFlag(NetProtocol.Pipe));
                EnableHttpBundle = HttpBundleFormatter != BundleFormatter.NA && (RemoteCacheProtocol.HasFlag(NetProtocol.Http) || SyncCacheProtocol.HasFlag(NetProtocol.Http) || SessionCacheProtocol.HasFlag(NetProtocol.Http) || DataCacheProtocol.HasFlag(NetProtocol.Http));
            }

            //CacheDefaults.MaxSessionTimeout = MaxSessionTimeout;
            //CacheDefaults.SessionTimeout = SessionTimeout;
            //CacheDefaults.DefaultExpiration = DefaultExpiration;
            //CacheDefaults.EnableLog = EnableLog;
        }

        /// <summary>
        /// LoadPipeConfigServer
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static PipeSettings LoadPipeConfigServer(string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                throw new ArgumentNullException("PipeCacheSettings.LoadPipeConfigServer name");
            }

            var config = CacheConfigServer.GetConfig();

            var settings = config.FindPipeServer(hostName);
            if (settings == null)
            {
                throw new ArgumentException("Invalid PipeCacheSettings with PipeName:" + hostName);
            }

           //~Console.WriteLine("Debuger-LoadPipeConfigServer.IsAsync: " + settings.IsAsync.ToString());

           return new PipeSettings()
           {
               HostName = settings.HostName,
               PipeName = settings.PipeName,
               PipeDirection = EnumExtension.Parse<PipeDirection>(settings.PipeDirection, PipeDirection.InOut),
               PipeOptions = EnumExtension.Parse<PipeOptions>(settings.PipeOptions, PipeOptions.None),
               VerifyPipe = settings.VerifyPipe,
               ConnectTimeout = settings.ConnectTimeout,
               ReceiveBufferSize = settings.ReceiveBufferSize,
               SendBufferSize = settings.SendBufferSize,
               MaxServerConnections = settings.MaxServerConnections,
               MaxAllowedServerInstances = settings.MaxAllowedServerInstances,
               IsAsync= settings.IsAsync

            };
        }
        /// <summary>
        /// LoadTcpConfigServer
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static TcpSettings LoadTcpConfigServer(string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                throw new ArgumentNullException("TcpCacheSettings.LoadTcpConfigServer name");
            }

            var config = CacheConfigServer.GetConfig();

            var settings = config.FindTcpServer(hostName);
            if (settings == null)
            {
                throw new ArgumentException("Invalid TcpCacheSettings with TcpName:" + hostName);
            }

            return new TcpSettings()
            {
                HostName = settings.HostName,
                Address = TcpSettings.EnsureHostAddress(settings.Address),
                Port = settings.Port,
                IsAsync = settings.IsAsync,
                ReceiveBufferSize = settings.ReceiveBufferSize,
                SendBufferSize = settings.SendBufferSize,
                ConnectTimeout = settings.ConnectTimeout,
                ReadTimeout= settings.ReadTimeout,
                //ProcessTimeout=settings.ProcessTimeout,
                MaxSocketError = settings.MaxSocketError,
                MaxServerConnections = Math.Max(1, settings.MaxServerConnections)
            };

       }
       
    }
  
}
