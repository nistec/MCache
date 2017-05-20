﻿//licHeader
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
using Nistec.Serialization;
using Nistec.Channels;

namespace Nistec.Caching.Config
{
    /// <summary>
    /// Represent Cache Defaults Settings
    /// </summary>
    public class CacheDefaults
    {
        /// <summary>
        /// Get Default Formatter
        /// </summary>
        public static Formatters DefaultFormatter { get { return Formatters.BinarySerializer; } }
        /// <summary>
        /// Default protocol
        /// </summary>
        public static NetProtocol DefaultProtocol = NetProtocol.Tcp;

        #region pipe
        /// <summary>
        /// Default Cache HostName
        /// </summary>
        public const string DefaultCacheHostName = "nistec_cache";
        /// <summary>
        /// Default Data HostName
        /// </summary>
        public const string DefaultDataCacheHostName = "nistec_cache_data";
        /// <summary>
        /// Default Session HostName
        /// </summary>
        public const string DefaultSessionHostName = "nistec_cache_session";
        /// <summary>
        /// Default Sync HostName
        /// </summary>
        public const string DefaultSyncCacheHostName = "nistec_cache_sync";
        /// <summary>
        /// Default CacheManager HostName
        /// </summary>
        public const string DefaultCacheManagerHostName = "nistec_cache_manager";

        /// <summary>
        /// Default CacheManager HostName
        /// </summary>
        public const string DefaultBundleHostName = "nistec_cache_bundle";

        #endregion

        #region TCP

        /// <summary>
        /// DefaultReadTimeout
        /// </summary>
        public const int DefaultReadTimeout = 1000;

        /// <summary>
        /// Default Sync Address
        /// </summary>
        public const string DefaultBundleAddress = "localhost";
        /// <summary>
        /// DefaultCachePort
        /// </summary>
        public const int DefaultBundlePort = 13000;

        /// <summary>
        /// Default Sync Address
        /// </summary>
        public const string DefaultCacheAddress = "localhost";
        /// <summary>
        /// DefaultCachePort
        /// </summary>
        public const int DefaultCachePort = 13001;

        /// <summary>
        /// Default Data Address
        /// </summary>
        public const string DefaultDataCacheAddress = "localhost";
        /// <summary>
        /// DefaultDataCachePort
        /// </summary>
        public const int DefaultDataCachePort = 13002;

        /// <summary>
        /// Default Session Address
        /// </summary>
        public const string DefaultSessionAddress = "localhost";
        /// <summary>
        /// DefaultSessionPort
        /// </summary>
        public const int DefaultSessionPort = 13003;

        /// <summary>
        /// Default Sync Address
        /// </summary>
        public const string DefaultSyncCacheAddress = "localhost";
        /// <summary>
        /// DefaultSyncCachePort
        /// </summary>
        public const int DefaultSyncCachePort = 13004;

        /// <summary>
        /// Default CacheManager Address
        /// </summary>
        public const string DefaultCacheManagerAddress = "localhost";
        /// <summary>
        /// DefaultCacheManagerPort
        /// </summary>
        public const int DefaultCacheManagerPort = 13005;
        /// <summary>
        /// DefaultTaskTimeout
        /// </summary>
        public const int DefaultTaskTimeout = 240;

        #endregion


        //10 GB max size
        internal const long DefaultCacheMaxSize = 10737418240;
        //10 MB
        internal const long MinCacheMaxSize = 10485760;
        internal const int InitialCapacity = 100;
        internal const float LoadFactor = 0.5F;
        internal const int DefaultIntervalSeconds = 60;
        internal const int MinIntervalSeconds = 30;
        internal const int DefaultSessionTimeout = 30;
        internal const int DefaultAutoResetIntervalHours = 12;

        internal static int GetValidIntervalSeconds(int intervalSeconds)
        {
            return intervalSeconds < CacheDefaults.MinIntervalSeconds ? CacheDefaults.DefaultIntervalSeconds : intervalSeconds;

        }
        internal static long GetValidCacheMaxSize(long maxSize)
        {
            return maxSize < CacheDefaults.MinCacheMaxSize ? CacheDefaults.DefaultCacheMaxSize : maxSize;

        }

        /// <summary>
        /// Get Max Session Timeout in minutes
        /// </summary>
        public static int MaxSessionTimeout { get; internal set; }// = 1440;
        /// <summary>
        /// Get Session Timeout in minutes
        /// </summary>
        public static int SessionTimeout { get; internal set; }
        /// <summary>
        /// Get Default Expiration in minutes
        /// </summary>
        public static int DefaultExpiration { get; internal set; }
        /// <summary>
        /// Get if Enable Logging
        /// </summary>
        public static bool EnableLog { get; internal set; }


        static CacheDefaults()
        {
            MaxSessionTimeout = 1440;
            SessionTimeout = CacheDefaults.DefaultSessionTimeout;
            DefaultExpiration = 30;
            EnableLog = false;
        }

    }

}
