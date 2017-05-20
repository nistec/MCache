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
using System.Collections;
using Nistec.Xml;
using System.Xml;
using Nistec.Caching.Sync;

namespace Nistec.Caching.Data
{
    /// <summary>
    /// Represent a data cache settings.
    /// </summary>
    public class DataCacheSettings
    {
        /// <summary>
        /// Connection satring.
        /// </summary>
        public string ConnectionString;
        /// <summary>
        /// Data cache name.
        /// </summary>
        public string DataCacheName;
        /// <summary>
        /// Provider.
        /// </summary>
        public Nistec.Data.DBProvider Provider;
        /// <summary>
        /// Xml settings.
        /// </summary>
        public string Xmlsettings;
        /// <summary>
        /// Use table watcher for sunchronization.
        /// </summary>
        public bool UseTableWatcher = false;
        /// <summary>
        /// Dat sync options.
        /// </summary>
        public SyncOption DataSyncOption;

        /// <summary>
        /// Get default data cache settings.
        /// </summary>
        public static DataCacheSettings Default
        {
            get { return new DataCacheSettings(); }
        }
        /// <summary>
        /// Initialize a new instance of data cache settings.
        /// </summary>
        public DataCacheSettings()
            : this("McRemoteData", "", Nistec.Data.DBProvider.SqlServer, false)
        {
        }
        /// <summary>
        /// Initialize a new instance of data cache settings.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connection"></param>
        /// <param name="provider"></param>
        /// <param name="useTableWatcher"></param>
        public DataCacheSettings(string name, string connection, Nistec.Data.DBProvider provider, bool useTableWatcher)
        {
            DataCacheName = name;
            ConnectionString = connection;
            Provider = provider;
            UseTableWatcher = useTableWatcher;
            DataSyncOption = SyncOption.Manual;
        }
        /// <summary>
        /// Initialize a new instance of data cache settings.
        /// </summary>
        /// <param name="prop"></param>
        public DataCacheSettings(System.Collections.Specialized.NameValueCollection prop)
        {
            DataCacheName = Types.NZ(prop["DataCacheName"], "McRemoteData");
            ConnectionString = Types.NZ(prop["ConnectionString"], "");
            Provider = Nistec.Data.Factory.DbFactory.GetProvider(Types.NZ(prop["Provider"], "SqlServer"));
            UseTableWatcher = Types.ToBool(prop["UseTableWatcher"], false);
            string syncOption = Types.NZ(prop["SyncOption"], "Manual");
            DataSyncOption = (SyncOption)Enum.Parse(typeof(SyncOption), syncOption, true);
        }
        /// <summary>
        /// Get items settings as <see cref="SyncEntity"/> array.
        /// </summary>
        /// <returns></returns>
        public SyncEntity[] GetItemsSettings()
        {
            if (string.IsNullOrEmpty(Xmlsettings))
                return null;

            List<SyncEntity> items = new List<SyncEntity>();
            XmlParser parser = new XmlParser(Xmlsettings);
            XmlNode root = parser.SelectSingleNode("RemoteData", true);
            XmlNodeList list = root.ChildNodes;
            foreach (XmlNode n in list)
            {
                try
                {
                    if (n.NodeType == XmlNodeType.Comment)
                        continue;

                    items.Add(new SyncEntity(n));

                }
                catch { }
            }
            return items.ToArray();
        }

        /// <summary>
        /// Get data cache settings as dictionary.
        /// </summary>
        /// <returns></returns>
        public IDictionary ToDictionary()
        {
            IDictionary prop = new Hashtable();
            prop["DataCacheName"] = DataCacheName;
            prop["ConnectionString"] = ConnectionString;
            prop["Provider"] = Provider;
            prop["UseTableWatcher"] = UseTableWatcher;
            prop["SyncOption"] = DataSyncOption.ToString();
            prop["Xmlsettings"] = Xmlsettings;

            return prop;
        }
        /// <summary>
        /// Create data cache settings from dictionary.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static DataCacheSettings Create(IDictionary prop)
        {
            DataCacheSettings cp = new DataCacheSettings();
            cp.DataCacheName = Types.NZ(prop["DataCacheName"], "McRemoteData");
            cp.ConnectionString = Types.NZ(prop["ConnectionString"], "");
            cp.Provider = Nistec.Data.Factory.DbFactory.GetProvider(Types.NZ(prop["Provider"], "SqlServer"));
            cp.UseTableWatcher = Types.ToBool(prop["UseTableWatcher"], false);
            string syncOption = Types.NZ(prop["SyncOption"], "Manual");
            cp.DataSyncOption = (SyncOption)Enum.Parse(typeof(SyncOption), syncOption, true);
            cp.Xmlsettings = Types.NZ(prop["Xmlsettings"], "");
            return cp;
        }


    }
}
