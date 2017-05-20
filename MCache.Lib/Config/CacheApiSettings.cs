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

namespace Nistec.Caching.Config
{
    /// <summary>
    /// Represent the cache api settings as read only.
    /// </summary>
    public class CacheApiSettings
    {

        static string _RemoteCacheHostName = CacheDefaults.DefaultCacheHostName;
        static string _RemoteSyncCacheHostName = CacheDefaults.DefaultSyncCacheHostName;
        static string _RemoteSessionHostName = CacheDefaults.DefaultSessionHostName;
        static string _RemoteDataCacheHostName = CacheDefaults.DefaultDataCacheHostName;
        static string _RemoteCacheManagerHostName = CacheDefaults.DefaultCacheManagerHostName;
        static string _RemoteBundleHostName = CacheDefaults.DefaultBundleHostName;


        /// <summary>RemoteCacheHostName.</summary>
        public static string RemoteCacheHostName { get { return _RemoteCacheHostName; } }
        /// <summary>RemoteSyncCacheHostName.</summary>
        public static string RemoteSyncCacheHostName { get { return _RemoteSyncCacheHostName; } }
        /// <summary>RemoteSessionHostName.</summary>
        public static string RemoteSessionHostName { get { return _RemoteSessionHostName; } }
        /// <summary>RemoteDataCacheHostName.</summary>
        public static string RemoteDataCacheHostName { get { return _RemoteDataCacheHostName; } }
        /// <summary>RemoteCacheManagerHostName.</summary>
        public static string RemoteCacheManagerHostName { get { return _RemoteCacheManagerHostName; } }

        /// <summary>RemoteCacheBundleHostName.</summary>
        public static string RemoteBundleHostName { get { return _RemoteBundleHostName; } }

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

            _RemoteCacheHostName = table.Get<string>("RemoteCacheHostName", CacheDefaults.DefaultCacheHostName);
            _RemoteSyncCacheHostName = table.Get<string>("RemoteSyncCacheHostName", CacheDefaults.DefaultSyncCacheHostName);
            _RemoteSessionHostName = table.Get<string>("RemoteSessionHostName", CacheDefaults.DefaultSessionHostName);
            _RemoteDataCacheHostName = table.Get<string>("RemoteDataCacheHostName", CacheDefaults.DefaultDataCacheHostName);
            _RemoteCacheManagerHostName = table.Get<string>("RemoteCacheManagerHostName", CacheDefaults.DefaultCacheManagerHostName);

            _RemoteBundleHostName = table.Get<string>("RemoteBundleHostName", CacheDefaults.DefaultBundleHostName);

            _Protocol = GenericTypes.ConvertEnum<NetProtocol>(table.Get<string>("Protocol", CacheDefaults.DefaultProtocol.ToString()), CacheDefaults.DefaultProtocol);

        }

    }
   
}
