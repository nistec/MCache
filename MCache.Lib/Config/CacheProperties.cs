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
using System.Text;
using System.Collections;
using System.Xml;
using Nistec.Caching;
using Nistec.Xml;
using Nistec.Caching.Data;
using System.Configuration;
using Nistec.Generic;
using System.Collections.Specialized;

namespace Nistec.Caching.Config
{
    /// <summary>
    /// Represent a cache initialization properties .
    /// </summary>
    public class CacheProperties
    {

        string _CacheName;
        /// <summary>
        /// Get or Set cache name
        /// </summary>
        public string CacheName
        {
            get { return _CacheName; }
            set { if (!string.IsNullOrEmpty(value)) _CacheName = value; }
        }

        long _MaxSize=CacheDefaults.DefaultCacheMaxSize;
        /// <summary>
        /// Get or Set max size of cache in bytes.
        /// </summary>
        public long MaxSize
        {
            get { return _MaxSize; }
            set { _MaxSize=CacheDefaults.GetValidCacheMaxSize(value); }
        }
        int _AutoResetIntervalHours = CacheDefaults.DefaultAutoResetIntervalHours;
        /// <summary>
        /// Get the interval in hours for auto reset performance counter.
        /// </summary>
        public int AutoResetIntervalHours
        {
            get { return _AutoResetIntervalHours; }
            set { if (value >= 0) _AutoResetIntervalHours = value; }
        }
        int _SyncInterval=CacheDefaults.DefaultIntervalSeconds;
        /// <summary>
        /// Get or Set the synchronization interval of cache.
        /// </summary>
        public int SyncIntervalSeconds
        {
            get { return _SyncInterval; }
            set { _SyncInterval=CacheDefaults.GetValidIntervalSeconds(value); }
        }
        int _InitialCapacity;
        /// <summary>
        /// Get or Set the initial capacity of cache memory.
        /// </summary>
        public int InitialCapacity
        {
            get { return _InitialCapacity; }
            set { if (value > 0 && value < 101000) _InitialCapacity = value; }
        }
       
        int _SessionTimeout=CacheDefaults.DefaultSessionTimeout;
        /// <summary>
        /// Get or Set the default session time out.
        /// </summary>
        public int SessionTimeout
        {
            get { return _SessionTimeout; }
            set
            {
                if (value > 0)
                {
                    _SessionTimeout = value;
                }
            }
        }

        bool _EnableLog = false;
        /// <summary>
        /// Get or Set if cache enable to use cache logger.
        /// </summary>
        public bool EnableLog
        {
            get { return _EnableLog; }
            set { _EnableLog = value; }
        }

        int _DefaultExpiration = 30;
        /// <summary>
        /// Get or Set the default expiration in minutes for item in cache.
        /// </summary>
        public int DefaultExpiration
        {
            get { return _DefaultExpiration; }
            set { if (value > 0) _DefaultExpiration = value; }
        }

        bool _RemoveExpiredItemOnSync = false;
        /// <summary>
        /// Get or Set if remove expired items from cache immediately.
        /// </summary>
        public bool RemoveExpiredItemOnSync
        {
            get { return _RemoveExpiredItemOnSync; }
            set { _RemoveExpiredItemOnSync = value; }
        }


        /// <summary>
        /// Get default properties.
        /// </summary>
        public static CacheProperties Default
        {
            get { return new CacheProperties(); }
        }

        /// <summary>
        /// Load propertis from app.config file
        /// </summary>
        /// <returns></returns>
        public static CacheProperties LoadProperties()
        {
            System.Configuration.Configuration config =
      ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            XmlDocument doc = new XmlDocument();
            doc.Load(config.FilePath);
            XmlNode node = doc.SelectSingleNode("//CacheSettings");

            CacheProperties prop = new CacheProperties(node);
            return prop;
        }
        /// <summary>
        /// Initialize a new instance of cache properties.
        /// </summary>
        public CacheProperties()
            : this("MyCache", CacheDefaults.DefaultCacheMaxSize)
        {
        }
        /// <summary>
        /// Initialize a new instance of cache properties.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="maxSize"></param>
        public CacheProperties(string name, long maxSize)
        {
            CacheName = name;
            MaxSize = maxSize;
            InitialCapacity = 100;
            

            DefaultExpiration = 30;
            RemoveExpiredItemOnSync = true;
            SyncIntervalSeconds = CacheDefaults.DefaultIntervalSeconds;
            SessionTimeout = CacheDefaults.DefaultSessionTimeout;
            AutoResetIntervalHours = CacheDefaults.DefaultAutoResetIntervalHours;
            EnableLog = false;
        }
        /// <summary>
        /// Initialize a new instance of cache properties using <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="prop"></param>
        public CacheProperties(NameValueCollection prop)
        {
            CacheName = Types.NZ(prop["CacheName"], "MyCache");
            MaxSize = (long)Types.ToLong(prop["MaxSize"], CacheDefaults.DefaultCacheMaxSize);
            DefaultExpiration = (int)Types.ToInt(prop["DefaultExpiration"], 30);
            RemoveExpiredItemOnSync = Types.ToBool(prop["RemoveExpiredItemOnSync"], true);
            SyncIntervalSeconds = (int)Types.ToInt(prop["SyncInterval"], CacheDefaults.DefaultIntervalSeconds);
            InitialCapacity = (int)Types.ToInt(prop["InitialCapacity"], CacheDefaults.InitialCapacity);
            SessionTimeout = (int)Types.ToInt(prop["SessionTimeout"], CacheDefaults.DefaultSessionTimeout);
            EnableLog = (bool)Types.ToBool(prop["EnableLog"], false);
            AutoResetIntervalHours = (int)Types.ToInt(prop["AutoResetIntervalHours"], CacheDefaults.DefaultAutoResetIntervalHours);
        }

        /// <summary>
        /// Initialize a new instance of cache properties using <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="node"></param>
        public CacheProperties(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentException("Inavlid Xml Root, 'CacheSettings' ");
            }

            XmlTable table = NetConfig.GetCustomConfig("CacheSettings");

            if (table == null)
            {
                throw new ArgumentException("Can not load XmlTable config");
            }

            CacheName = table.GetValue("name");
            MaxSize = table.Get<long>("MaxSize", CacheDefaults.DefaultCacheMaxSize);
            DefaultExpiration = table.Get<int>("DefaultExpiration", 30);
            RemoveExpiredItemOnSync = table.Get<bool>("RemoveExpiredItemOnSync", true);
            SyncIntervalSeconds = table.Get<int>("SyncInterval", CacheDefaults.DefaultIntervalSeconds);
            InitialCapacity = table.Get<int>("InitialCapacity", CacheDefaults.InitialCapacity);
            SessionTimeout = table.Get<int>("SessionTimeout", CacheDefaults.DefaultSessionTimeout);
            EnableLog = table.Get<bool>("EnableLog", false);
            AutoResetIntervalHours = table.Get<int>("AutoResetIntervalHours", CacheDefaults.DefaultAutoResetIntervalHours);
        }
        /// <summary>
        /// Get cache properties as dictionary
        /// </summary>
        /// <returns></returns>
        public IDictionary ToDictionary()
        {
            IDictionary prop = new Hashtable();
            prop["CacheName"] = CacheName;
            prop["MaxSize"] = MaxSize;
            prop["DefaultExpiration"] = DefaultExpiration;
            prop["RemoveExpiredItemOnSync"] = RemoveExpiredItemOnSync;
            prop["SyncInterval"] = SyncIntervalSeconds;
            prop["InitialCapacity"] = InitialCapacity;
            prop["SessionTimeout"] = SessionTimeout;
            prop["EnableLog"] = EnableLog;
            prop["AutoResetIntervalHours"] = AutoResetIntervalHours;
            return prop;
        }
        /// <summary>
        /// Create new cache properties from dictionary
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static CacheProperties Create(IDictionary prop)
        {

            CacheProperties cp = new CacheProperties();
            cp.CacheName = Types.NZ(prop["CacheName"], "MyCache");
            cp.MaxSize = (long)Types.ToLong(prop["MaxSize"], CacheDefaults.DefaultCacheMaxSize);
            cp.DefaultExpiration = (int)Types.ToInt(prop["DefaultExpiration"], 30);
            cp.RemoveExpiredItemOnSync = Types.ToBool(prop["RemoveExpiredItemOnSync"], true);
            cp.SyncIntervalSeconds = (int)Types.ToInt(prop["SyncInterval"], CacheDefaults.DefaultIntervalSeconds);
            cp.InitialCapacity = (int)Types.ToInt(prop["InitialCapacity"], CacheDefaults.InitialCapacity);
            cp.SessionTimeout = (int)Types.ToInt(prop["SessionTimeout"], CacheDefaults.DefaultSessionTimeout);
            cp.EnableLog = (bool)Types.ToBool(prop["EnableLog"], false);
            cp.AutoResetIntervalHours = (int)Types.ToInt(prop["AutoResetIntervalHours"], CacheDefaults.DefaultAutoResetIntervalHours);
            return cp;
        }
    }

    
}
