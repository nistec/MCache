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
    /// Represent cache config for client.
    /// </summary>
    public class CacheConfigClient : ConfigurationSection
    {
        static CacheConfigClient Config;

        /// <summary>
        /// Get <see cref="CacheConfigClient"/>.
        /// </summary>
        /// <returns></returns>
        public static CacheConfigClient GetConfig()
        {
            if (Config == null)
                Config = (CacheConfigClient)System.Configuration.ConfigurationManager.GetSection("RemoteCache") ?? new CacheConfigClient();
            return Config;
        }

        /// <summary>
        /// Get Cache Api Settings.
        /// </summary>
        [System.Configuration.ConfigurationProperty("CacheApiSettings")]
        [ConfigurationCollection(typeof(NetConfigItems), AddItemName = "add")]
        public NetConfigItems CacheApiSettings
        {
            get
            {
                object o = this["CacheApiSettings"];
                return o as NetConfigItems;
            }
        }

        /// <summary>
        /// Get <see cref="PipeClientConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("PipeClientSettings")]
        [ConfigurationCollection(typeof(PipeClientConfigItems), AddItemName = "host")]
        public PipeClientConfigItems PipeClientSettings
        {
            get
            {
                object o = this["PipeClientSettings"];
                return o as PipeClientConfigItems;
            }
        }
        /// <summary>
        ///  Find pipe client item.
        /// </summary>
        /// <param name="pipeName"></param>
        /// <returns></returns>
        public PipeConfigItem FindPipeClient(string pipeName)
        {
            return PipeClientSettings[pipeName];
        }

        
        /// <summary>
        /// Get <see cref="TcpClientConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("TcpClientSettings")]
        [ConfigurationCollection(typeof(TcpClientConfigItems), AddItemName = "host")]
        public TcpClientConfigItems TcpClientSettings
        {
            get
            {
                object o = this["TcpClientSettings"];
                return o as TcpClientConfigItems;
            }
        }
        /// <summary>
        ///  Find tcp client item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public TcpConfigItem FindTcpClient(string hostName)
        {
            return TcpClientSettings[hostName];
        }

        /// <summary>
        /// Get <see cref="HttpClientConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("HttpClientSettings")]
        [ConfigurationCollection(typeof(HttpClientConfigItems), AddItemName = "host")]
        public HttpClientConfigItems HttpClientSettings
        {
            get
            {
                object o = this["HttpClientSettings"];
                return o as HttpClientConfigItems;
            }
        }
        /// <summary>
        ///  Find http client item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public HttpConfigItem FindHttpClient(string hostName)
        {
            return HttpClientSettings[hostName];
        }

   
    }

    /// <summary>
    /// Represent pipe client configuration element collection.
    /// </summary>
    public class PipeClientConfigItems : ConfigurationElementCollection
    {
        /// <summary>
        /// Get or Set <see cref="PipeConfigItem"/> item by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PipeConfigItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as PipeConfigItem;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
        /// <summary>
        /// Get or Set <see cref="PipeConfigItem"/> item by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new PipeConfigItem this[string key]
        {
            get { return (PipeConfigItem)BaseGet(key); }
            set
            {
                if (BaseGet(key) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                }
                BaseAdd(value);
            }
        }
        /// <summary>
        /// Create New Element
        /// </summary>
        /// <returns></returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new PipeConfigItem();
        }
        /// <summary>
        /// Get Element Key
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((PipeConfigItem)element).HostName;
        }
    }

    /// <summary>
    /// Represent tcp client configuration element collection.
    /// </summary>
    public class TcpClientConfigItems : ConfigurationElementCollection
    {
        /// <summary>
        /// Get or Set <see cref="TcpConfigItem"/> item by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TcpConfigItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as TcpConfigItem;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
        /// <summary>
        /// Get or Set <see cref="TcpConfigItem"/> item by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new TcpConfigItem this[string key]
        {
            get { return (TcpConfigItem)BaseGet(key); }
            set
            {
                if (BaseGet(key) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                }
                BaseAdd(value);
            }
        }
        /// <summary>
        /// Create New Element
        /// </summary>
        /// <returns></returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new TcpConfigItem();
        }
        /// <summary>
        /// Get Element Key
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((TcpConfigItem)element).HostName;
        }
    }

    /// <summary>
    /// Represent http client configuration element collection.
    /// </summary>
    public class HttpClientConfigItems : ConfigurationElementCollection
    {
        /// <summary>
        /// Get or Set <see cref="HttpConfigItem"/> item by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public HttpConfigItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as HttpConfigItem;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
        /// <summary>
        /// Get or Set <see cref="HttpConfigItem"/> item by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new HttpConfigItem this[string key]
        {
            get { return (HttpConfigItem)BaseGet(key); }
            set
            {
                if (BaseGet(key) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                }
                BaseAdd(value);
            }
        }
        /// <summary>
        /// Create New Element
        /// </summary>
        /// <returns></returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new HttpConfigItem();
        }
        /// <summary>
        /// Get Element Key
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((HttpConfigItem)element).HostName;
        }
    }
}
