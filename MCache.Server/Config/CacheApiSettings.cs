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
using Nistec.Channels;
using Nistec.Channels.Tcp;
using System.IO.Pipes;
using Nistec.Channels.Http;

namespace Nistec.Caching.Config
{
    /// <summary>
    /// Represent the cache api settings as read only.
    /// </summary>
    public class CacheApiSettings
    {

        public static string RemoteManagerHostName = CacheDefaults.DefaultManagerHostName;
        public static string RemoteBundleHostName = CacheDefaults.DefaultBundleHostName;
        //public static string RemoteJsonHostName = CacheDefaults.DefaultJsonHostName;

        const bool DefaultIsAsync = false;
        const bool DefaultEnableException = true;

        static bool _IsRemoteAsync = DefaultIsAsync;
        /// <summary>IsRemoteAsync.</summary>
        public static bool IsRemoteAsync
        {
            get
            {
                return _IsRemoteAsync;
            }
        }

        static bool _EnableRemoteException = DefaultEnableException;

        /// <summary>EnableRemoteException.</summary>
        public static bool EnableRemoteException
        {
            get
            {
                return _EnableRemoteException;
            }

        }

        static NetProtocol _Protocol = CacheDefaults.DefaultProtocol;

        /// <summary>Protocol.</summary>
        public static NetProtocol Protocol
        {
            get
            {
                return _Protocol;
            }
            set { _Protocol = value; }
        }

        static int _CacheExpiration = CacheDefaults.DefaultCacheExpiration;

        public static int CacheExpiration
        {
            get
            {
                return _CacheExpiration;
            }
            set { _CacheExpiration = value; }
        }

        static int _SessionTimeout = CacheDefaults.DefaultSessionTimeout;

        public static int SessionTimeout
        {
            get
            {
                return _SessionTimeout;
            }
            set { _SessionTimeout = value; }
        }

        static int _TcpPort = CacheDefaults.DefaultTcpBundlePort;
        static int _HttpPort = CacheDefaults.DefaultHttpBundlePort;

        public static int TcpPort
        {
            get
            {
                return _TcpPort;
            }
        }
        public static int HttpPort
        {
            get
            {
                return _HttpPort;
            }
        }

        /// <summary>Port.</summary>
        public static int Port
        {
            get
            {
                switch(Protocol)
                {
                    case NetProtocol.Http:
                        return _HttpPort;
                    case NetProtocol.Tcp:
                        return _TcpPort;
                    default:
                        return 0;
                }
            }
        }

        static CacheApiSettings()
        {

            var section = CacheConfigClient.GetConfig();
            var table = section.CacheApiSettings;

            if (table == null)
            {
                throw new ArgumentException("Can not load Cache Api Settings");
            }

            _IsRemoteAsync = table.Get<bool>("IsRemoteAsync", DefaultIsAsync);
            _EnableRemoteException = table.Get<bool>("EnableRemoteException", DefaultEnableException);

            _Protocol = GenericTypes.ConvertEnum<NetProtocol>(table.Get<string>("Protocol", CacheDefaults.DefaultProtocol.ToString()), CacheDefaults.DefaultProtocol);
            _CacheExpiration = table.Get<int>("CacheExpiration", CacheDefaults.DefaultCacheExpiration);
            _SessionTimeout = table.Get<int>("SessionTimeout", CacheDefaults.DefaultSessionTimeout);
        }

    }

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
        public static TcpSettings GetClientSettings(string hostName)
        {
            TcpSettings settings = null;
            if (ClientSettingsCache.TryGetValue(hostName, out settings))
            {
                return settings;
            }
            settings = LoadConfigClient(hostName);
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
        public static TcpSettings LoadConfigClient(string configHost)
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
                ConnectTimeout = settings.ConnectTimeout,
                ProcessTimeout = settings.ProcessTimeout,
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
        public static PipeSettings GetClientSettings(string hostName)
        {
            PipeSettings settings = null;
            if (ClientSettingsCache.TryGetValue(hostName, out settings))
            {
                return settings;
            }
            settings = LoadConfigClient(hostName);
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
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static PipeSettings LoadConfigClient(string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                throw new ArgumentNullException("PipeCacheSettings.LoadPipeConfigClient name");
            }

            var config = CacheConfigClient.GetConfig();

            var settings = config.FindPipeClient(hostName);
            if (settings == null)
            {
                throw new ArgumentException("Invalid PipeCacheSettings with PipeName:" + hostName);
            }
            return new PipeSettings()
            {
                HostName = settings.HostName,
                PipeName = settings.PipeName,
                PipeDirection = EnumExtension.Parse<PipeDirection>(settings.PipeDirection, PipeDirection.InOut),
                PipeOptions = EnumExtension.Parse<PipeOptions>(settings.PipeOptions, PipeOptions.None),
                VerifyPipe = settings.VerifyPipe,
                ConnectTimeout = (uint)settings.ConnectTimeout,
                ProcessTimeout = settings.ProcessTimeout,
                ReceiveBufferSize = settings.ReceiveBufferSize,
                SendBufferSize=settings.SendBufferSize
            };

        }

    }

    /// <summary>
    /// Represent a http client settings.
    /// </summary>
    public class HttpClientCacheSettings
    {
        static readonly Dictionary<string, HttpSettings> ClientSettingsCache = new Dictionary<string, HttpSettings>();

        /// <summary>
        /// Get Tcp Client Settings
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static HttpSettings GetClientSettings(string hostName)
        {
            HttpSettings settings = null;
            if (ClientSettingsCache.TryGetValue(hostName, out settings))
            {
                return settings;
            }
            settings = LoadConfigClient(hostName);
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
        public static HttpSettings LoadConfigClient(string configHost)
        {
            if (string.IsNullOrEmpty(configHost))
            {
                throw new ArgumentNullException("TcpCacheSettings.LoadTcpConfigClient name");
            }

            var config = CacheConfigClient.GetConfig();

            var settings = config.FindHttpClient(configHost);
            if (settings == null)
            {
                throw new ArgumentException("Invalid TcpCacheSettings with HostName:" + configHost);
            }

            return new HttpSettings()
            {
                HostName = settings.HostName,
                Address = HttpSettings.EnsureHostAddress(settings.Address),
                Port = settings.Port,
                Method=settings.Method,
                //IsAsync = settings.IsAsync,
                //ReceiveBufferSize = settings.ReceiveBufferSize,
                //SendBufferSize = settings.SendBufferSize,
                ConnectTimeout = settings.ConnectTimeout,
                ProcessTimeout = settings.ProcessTimeout,
            };
        }
    }

}
