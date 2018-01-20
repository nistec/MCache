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
using Nistec.Channels.Config;

namespace Nistec.Caching.Config
{
    /// <summary>
    /// Represent cache config section in <see cref="ConfigurationSection"/>
    /// </summary>
    public class CacheConfigServer : ConfigurationSection
    {

        static CacheConfigServer Config;

        /// <summary>
        /// Get <see cref="CacheConfigServer"/>.
        /// </summary>
        /// <returns></returns>
        public static CacheConfigServer GetConfig()
        {
            if (Config == null)
                Config = (CacheConfigServer)System.Configuration.ConfigurationManager.GetSection("MCache") ?? new CacheConfigServer();
            return Config;
        }


        /// <summary>
        /// Get tcp server item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static TcpServerConfigItem GetTcpServer(string hostName)
        {
            var config = GetConfig();
            if (config == null)
            {
                throw new Exception("Tcp CacheConfigServer not found");
            }
            return config.FindTcpServer(hostName); 
        }
        /// <summary>
        /// Get pipe server item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static PipeServerConfigItem GetPipeServer(string hostName)
        {
            var config = GetConfig();
            if (config == null)
            {
                throw new Exception("Pipe CacheConfigServer not found");
            }
            return config.FindPipeServer(hostName);
        }
        /// <summary>
        /// Get http server item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static HttpServerConfigItem GetHttpServer(string hostName)
        {
            var config = GetConfig();
            if (config == null)
            {
                throw new Exception("Http CacheConfigServer not found");
            }
            return config.FindHttpServer(hostName);
        }

        ///// <summary>
        ///// Get cache api settings.
        ///// </summary>
        ///// <returns></returns>
        //public static NetConfigItems GetCacheApiSettings()
        //{
        //    var config = GetConfig();
        //    if (config == null)
        //    {
        //        throw new Exception("GetCacheApiSettings not found");
        //    }
        //    return config.CacheApiSettings;
        //}

        /// <summary>
        /// Get Cache Settings items.
        /// </summary>
        [System.Configuration.ConfigurationProperty("CacheSettings")]
        [ConfigurationCollection(typeof(NetConfigItems), AddItemName = "add")]
        public NetConfigItems CacheSettings
        {
            get
            {
                object o = this["CacheSettings"];
                return o as NetConfigItems;
            }
        }
        //public void ResetSettings()
        //{
        //    this.Reset(this.CacheSettings);
        //}

        ///// <summary>
        ///// Get Cache Api Settings.
        ///// </summary>
        //[System.Configuration.ConfigurationProperty("CacheApiSettings")]
        //[ConfigurationCollection(typeof(NetConfigItems), AddItemName = "add")]
        //public NetConfigItems CacheApiSettings
        //{
        //    get
        //    {
        //        object o = this["CacheApiSettings"];
        //        return o as NetConfigItems;
        //    }
        //}

        #region Pipe

        /// <summary>
        /// Get <see cref="PipeServerConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("PipeServerSettings")]
        [ConfigurationCollection(typeof(PipeServerConfigItem), AddItemName = "host")]
        public PipeServerConfigItems PipeServerSettings
        {
            get
            {
                object o = this["PipeServerSettings"];
                return o as PipeServerConfigItems;
            }
        }
        /// <summary>
        /// Find pipe server item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public PipeServerConfigItem FindPipeServer(string hostName)
        {
            return PipeServerSettings[hostName];
        }
        
    
        #endregion

        #region Tcp
        
        /// <summary>
        /// Get <see cref="TcpServerConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("TcpServerSettings")]
        [ConfigurationCollection(typeof(TcpServerConfigItem), AddItemName = "host")]
        public TcpServerConfigItems TcpServerSettings
        {
            get
            {
                object o = this["TcpServerSettings"];
                return o as TcpServerConfigItems;
            }
        }

        /// <summary>
        /// Find tcp server item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public TcpServerConfigItem FindTcpServer(string hostName)
        {
            return TcpServerSettings[hostName];
        }

        #endregion

        #region Http

        /// <summary>
        /// Get <see cref="HttpServerConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("HttpServerSettings")]
        [ConfigurationCollection(typeof(HttpServerConfigItem), AddItemName = "host")]
        public HttpServerConfigItems HttpServerSettings
        {
            get
            {
                object o = this["HttpServerSettings"];
                return o as HttpServerConfigItems;
            }
        }

        /// <summary>
        /// Find Http server item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public HttpServerConfigItem FindHttpServer(string hostName)
        {
            return HttpServerSettings[hostName];
        }

        #endregion
    }

}
