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
    /// Represent a tcp client settings.
    /// </summary>
    public class TcpClientCacheSettings
    {
        static readonly Dictionary<string, TcpSettings> ClientSettingsCache = new Dictionary<string, TcpSettings>();

        /// <summary>
        /// Get Tcp Client Settings
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static TcpSettings GetTcpClientSettings(string hostName)
        {
            TcpSettings settings = null;
            if (ClientSettingsCache.TryGetValue(hostName, out settings))
            {
                return settings;
            }
            settings = LoadTcpConfigClient(hostName);
            if (settings == null)
            {
                throw new Exception("Invalid configuration for tcp cache client settings with host name:" + hostName);
            }
            ClientSettingsCache[hostName] = settings;
            return settings;
        }
        /// <summary>
        /// LoadTcpConfigClient
        /// </summary>
        /// <param name="configHost"></param>
        /// <returns></returns>
        public static TcpSettings LoadTcpConfigClient(string configHost)
        {
            if (string.IsNullOrEmpty(configHost))
            {
                throw new ArgumentNullException("TcpCacheSettings.LoadTcpConfigClient name");
            }

            var config = CacheConfigClient.GetConfig();

            var settings = config.FindTcpClient(configHost);
            if (settings == null)
            {
                throw new ArgumentException("Invalid TcpCacheSettings with TcpName:" + configHost);
            }

            return new TcpSettings()
            {
                HostName = settings.HostName,
                Address = TcpSettings.EnsureHostAddress(settings.Address),
                Port = settings.Port,
                IsAsync = settings.IsAsync,
                ReceiveBufferSize = settings.ReceiveBufferSize,
                SendBufferSize = settings.SendBufferSize,
                SendTimeout = settings.SendTimeout,
                ReadTimeout = settings.ReadTimeout,
            };
        }
   }
    /// <summary>
    /// Represent a pipe client settings.
    /// </summary>
    public class PipeClientCacheSettings
    {
        static readonly Dictionary<string, PipeSettings> ClientSettingsCache = new Dictionary<string, PipeSettings>();
        /// <summary>
        /// Get pipe client settings
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static PipeSettings GetPipeClientSettings(string hostName)
        {
            PipeSettings settings = null;
            if (ClientSettingsCache.TryGetValue(hostName, out settings))
            {
                return settings;
            }
            settings = LoadPipeConfigClient(hostName);
            if (settings == null)
            {
                throw new Exception("Invalid configuration for pipe cache client settings with host name:" + hostName);
            }
            ClientSettingsCache[hostName] = settings;
            return settings;
        }

        /// <summary>
        /// LoadPipeConfigClient
        /// </summary>
        /// <param name="configPipe"></param>
        /// <returns></returns>
        public static PipeSettings LoadPipeConfigClient(string configPipe)
        {
            if (string.IsNullOrEmpty(configPipe))
            {
                throw new ArgumentNullException("PipeCacheSettings.LoadPipeConfigClient name");
            }

            var config = CacheConfigClient.GetConfig();

            var settings = config.FindPipeClient(configPipe);
            if (settings == null)
            {
                throw new ArgumentException("Invalid PipeCacheSettings with PipeName:" + configPipe);
            }
            return new PipeSettings()
            {
                PipeName = settings.PipeName,
                PipeDirection = EnumExtension.Parse<PipeDirection>(settings.PipeDirection, PipeDirection.InOut),
                PipeOptions = EnumExtension.Parse<PipeOptions>(settings.PipeOptions, PipeOptions.None),
                VerifyPipe = settings.VerifyPipe,
                ConnectTimeout = (uint)settings.ConnectTimeout,
                InBufferSize = settings.InBufferSize
            };

        }

    }

    /// <summary>
    /// Represent the cache settings as read only.
    /// </summary>
    public class CacheSettings
    {
        /// <summary>EnableDynamic.</summary>
        public readonly static bool EnableDynamic = false;

        /// <summary>EnableSyncTypeEvent.</summary>
        public readonly static bool EnableSyncTypeEventTrigger = true;
        /// <summary>MaxSize.</summary>
        public readonly static long MaxSize = CacheDefaults.DefaultCacheMaxSize;
        /// <summary>DefaultExpiration.</summary>
        public readonly static int DefaultExpiration = 30;
        /// <summary>RemoveExpiredItemOnSync.</summary>
        public readonly static bool RemoveExpiredItemOnSync = true;
        /// <summary>Sync Interval in seconds.</summary>
        public readonly static int SyncInterval = CacheDefaults.DefaultIntervalSeconds;
        /// <summary>SyncBox Interval in seconds.</summary>
        public readonly static int SyncBoxInterval = CacheDefaults.DefaultIntervalSeconds;
        /// <summary>SyncOption.</summary>
        public readonly static string SyncOption = "Auto";
        /// <summary>SessionTimeout.</summary>
        public readonly static int SessionTimeout = CacheDefaults.DefaultSessionTimeout;
        /// <summary>MaxSessionTimeout.</summary>
        public readonly static int MaxSessionTimeout = 1440;
        /// <summary>EnableLog.</summary>
        public readonly static bool EnableLog = false;
        
         /// <summary>SyncConfigFile.</summary>
        public readonly static string SyncConfigFile = "";
        /// <summary>DbConfigFile.</summary>
        public readonly static string DbConfigFile = "";
        /// <summary>EnableSyncFileWatcher.</summary>
        public readonly static bool EnableSyncFileWatcher = false;
        /// <summary>ReloadSyncOnChange.</summary>
        public readonly static bool ReloadSyncOnChange = false;
        /// <summary>SyncTaskerTimeout.</summary>
        public readonly static int SyncTaskerTimeout = 60;
        /// <summary>EnableAsyncTask.</summary>
        public readonly static bool EnableAsyncTask = true;
        /// <summary>Get the interval in hours for auto reset performance counter.</summary>
        public readonly static int AutoResetIntervalHours = CacheDefaults.DefaultAutoResetIntervalHours;

        
        /// <summary>EnableSizeHandler.</summary>
        public readonly static bool EnableSizeHandler = false;
        /// <summary>EnablePerformanceCounter.</summary>
        public readonly static bool EnablePerformanceCounter = false;

         /// <summary>EnableRemoteCache.</summary>
        public readonly static NetProtocol RemoteCacheProtocol = NetProtocol.NA;
        /// <summary>EnableSyncCache.</summary>
        public readonly static NetProtocol SyncCacheProtocol = NetProtocol.NA;
        /// <summary>EnableSessionCache.</summary>
        public readonly static NetProtocol SessionCacheProtocol = NetProtocol.NA;
        /// <summary>EnableDataCache.</summary>
        public readonly static NetProtocol DataCacheProtocol = NetProtocol.NA;
        /// <summary>EnableCacheManager.</summary>
        public readonly static NetProtocol CacheManagerProtocol = NetProtocol.NA;

        /// <summary>EnablePipeBundle.</summary>
        public readonly static bool EnablePipeBundle = false;
        /// <summary>EnableTcpBundle.</summary>
        public readonly static bool EnableTcpBundle = false;
        ///// <summary>MaxTcpBundlePool.</summary>
        //public readonly static int MaxTcpBundlePool = 10;
        /// <summary>EnableHttpBundle.</summary>
        public readonly static bool EnableHttpBundle = false;
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
        static CacheSettings()
        {
            //XmlTable table = NetConfig.GetCustomConfig("CacheSettings");

            var section= CacheConfigServer.GetConfig();
            var table = section.CacheSettings;

            if (table == null)
            {
                throw new ArgumentException("Can not load XmlTable config");
            }

            MaxSize = table.Get<long>("MaxSize", CacheDefaults.DefaultCacheMaxSize);
            DefaultExpiration = table.Get<int>("DefaultExpiration", 30);
            RemoveExpiredItemOnSync = table.Get<bool>("RemoveExpiredItemOnSync", true);
            SyncInterval = table.Get<int>("SyncInterval", 60);
            SyncBoxInterval = table.Get<int>("SyncBoxInterval", 60);
            SessionTimeout = table.Get<int>("SessionTimeout", CacheDefaults.DefaultSessionTimeout);
            MaxSessionTimeout = table.Get<int>("MaxSessionTimeout", 1440);
            EnableLog = table.Get<bool>("EnableLog", false);
            SyncConfigFile = table.Get("SyncConfigFile");
            DbConfigFile = table.Get("DbConfigFile");
            EnableSyncFileWatcher = table.Get<bool>("EnableSyncFileWatcher", false);
            ReloadSyncOnChange = table.Get<bool>("ReloadSyncOnChange", false);
            SyncTaskerTimeout = table.Get<int>("SyncTaskerTimeout", 60);
            EnableAsyncTask = table.Get<bool>("EnableAsyncTask", true);
            AutoResetIntervalHours = table.Get<int>("AutoResetIntervalHours", CacheDefaults.DefaultAutoResetIntervalHours);

            EnableSizeHandler = table.Get<bool>("EnableSizeHandler", false);
            EnablePerformanceCounter = table.Get<bool>("EnablePerformanceCounter", false);

            EnableTcpBundle = table.Get<bool>("EnableTcpBundle", false);
            EnablePipeBundle = table.Get<bool>("EnablePipeBundle", false);
            EnableHttpBundle = table.Get<bool>("EnableHttpBundle", false);

            EnableSyncTypeEventTrigger = table.Get<bool>("EnableSyncTypeEventTrigger", true);

            //MaxTcpBundlePool = table.Get<int>("MaxTcpBundlePool", 0);

            RemoteCacheProtocol = GetNetProtocol(table.Get("RemoteCacheProtocol"));
            SyncCacheProtocol = GetNetProtocol(table.Get("SyncCacheProtocol"));
            SessionCacheProtocol = GetNetProtocol(table.Get("SessionCacheProtocol"));
            DataCacheProtocol = GetNetProtocol(table.Get("DataCacheProtocol"));
            CacheManagerProtocol = GetNetProtocol(table.Get("CacheManagerProtocol"));

            CacheDefaults.MaxSessionTimeout = MaxSessionTimeout;
            CacheDefaults.SessionTimeout = SessionTimeout;
            CacheDefaults.DefaultExpiration = DefaultExpiration;
            CacheDefaults.EnableLog = EnableLog;
        }

        /// <summary>
        /// LoadPipeConfigServer
        /// </summary>
        /// <param name="configPipe"></param>
        /// <returns></returns>
        public static PipeSettings LoadPipeConfigServer(string configPipe)
        {
            if (string.IsNullOrEmpty(configPipe))
            {
                throw new ArgumentNullException("PipeCacheSettings.LoadPipeConfigServer name");
            }

            var config = CacheConfigServer.GetConfig();

            var settings = config.FindPipeServer(configPipe);
            if (settings == null)
            {
                throw new ArgumentException("Invalid PipeCacheSettings with PipeName:" + configPipe);
            }
            return new PipeSettings()
           {
               PipeName = settings.PipeName,
               PipeDirection = EnumExtension.Parse<PipeDirection>(settings.PipeDirection, PipeDirection.InOut),
               PipeOptions = EnumExtension.Parse<PipeOptions>(settings.PipeOptions, PipeOptions.None),
               VerifyPipe = settings.VerifyPipe,
               ConnectTimeout = (uint)settings.ConnectTimeout,
               InBufferSize = settings.InBufferSize,
               OutBufferSize = settings.OutBufferSize,
               MaxServerConnections = settings.MaxServerConnections,
               MaxAllowedServerInstances = settings.MaxAllowedServerInstances
           };
        }
        /// <summary>
        /// LoadTcpConfigServer
        /// </summary>
        /// <param name="configHost"></param>
        /// <returns></returns>
        public static TcpSettings LoadTcpConfigServer(string configHost)
        {
            if (string.IsNullOrEmpty(configHost))
            {
                throw new ArgumentNullException("TcpCacheSettings.LoadTcpConfigServer name");
            }

            var config = CacheConfigServer.GetConfig();

            var settings = config.FindTcpServer(configHost);
            if (settings == null)
            {
                throw new ArgumentException("Invalid TcpCacheSettings with TcpName:" + configHost);
            }

            return new TcpSettings()
            {
                HostName = settings.HostName,
                Address = TcpSettings.EnsureHostAddress(settings.Address),
                Port = settings.Port,
                IsAsync = settings.IsAsync,
                ReceiveBufferSize = settings.ReceiveBufferSize,
                SendBufferSize = settings.SendBufferSize,
                SendTimeout = settings.SendTimeout,
                ProcessTimeout=settings.ProcessTimeout,
                ReadTimeout=settings.ReadTimeout,
                MaxSocketError = settings.MaxSocketError,
                MaxServerConnections = Math.Max(1, settings.MaxServerConnections)
            };

       }
       
    }
  
}
