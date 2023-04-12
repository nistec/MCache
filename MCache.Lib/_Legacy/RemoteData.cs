using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Data;


using Nistec.Data;
using Nistec.Collections;
using Nistec.Threading;
using Nistec.Caching;
using Nistec.Caching.Data;
using System.Collections.Specialized;
using Nistec.Data.Factory;

namespace Nistec.Legacy
{
    [Serializable]
    public class RemoteData : DalCache, IRemoteData
    {
        private const string DefaultXmlConfig = "McRemoteData.xml";
        //private int _size;

        //private static readonly RemoteData cache;

        ///// <summary>
        ///// Get RemoteCache
        ///// </summary>
        //public static RemoteData Storage
        //{
        //    get { return RemoteData.cache; }
        //} 

        //static RemoteData()
        //{
        //    cache = new RemoteData("McRemoteData");
        //}


        public string Reply(string text)
        {
            return text;
        }

        #region Data cache

        private Nistec.Data.IDalBase _DalBase;

        ///// <summary>
        ///// Get IDalBase
        ///// </summary>
        //public IDalBase DalBase
        //{
        //    get
        //    {
        //        if (_DalBase == null)
        //        {
        //            _DalBase = new Nistec.Data.Common.DalProvider(Nistec.Data.DBProvider.SqlServer, Config.ConnectionString);
        //        }
        //        return _DalBase;
        //    }
        //}
 

 
        /// <summary>
        /// Create Data Source with copy of storage
        /// </summary>
        /// <param name="dalBase"></param>
        /// <param name="storage"></param>
        public void CreateDataSource(IDalBase dalBase, Nistec.Caching.Data.DalCache storage)
        {
            if (!base.Initilized)
            {
                base.CacheName = storage.CacheName;
                _DalBase = dalBase;
                base.Synchronize( storage);
                //_size = DataSetUtil.DataSetToByteCount(this.DataSource, true); ;

            }
        }

        /// <summary>
        /// Create Data Source with connectionString and DataSet
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dataSource"></param>
        /// <param name="cahceName"></param>
        public void CreateDataSource(string connectionString, DataSet dataSource, string cahceName)
        {
            IDalBase dal= new DalProvider(Nistec.Data.DBProvider.SqlServer, connectionString);
            CreateDataSource(dal,dataSource, cahceName);
        }
        /// <summary>
        /// Create Data Source with IDalBase and DataSet
        /// </summary>
        /// <param name="dalBase"></param>
        /// <param name="cahceName"></param>
        public void CreateDataSource(IDalBase dalBase, DataSet dataSource, string cahceName)
        {
           
            if (!base.Initilized)
            {
                _DalBase = dalBase;
                base.Synchronize(new Nistec.Caching.Data.DalCache(cahceName,dalBase));
                base.CreateCache(dataSource, cahceName);
                //_size = DataSetUtil.DataSetToByteCount(this.DataSource, true); ;
            }
        }


        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        public bool AddDataItem(DataTable dt, string tableName)
        {
            return base.Add(dt, tableName);
        }

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="syncSource"></param>
        public void AddDataItem(DataTable dt, string tableName, Nistec.Caching.Data.SyncSource syncSource)
        {
            if (AddDataItem(dt, tableName))
            {
                base.SyncTables.Add(syncSource);
            }
        }
        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        public void AddDataItem(DataTable dt, string tableName, string mappingName,string sourceName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
            if (AddDataItem(dt, tableName))
            {
                base.SyncTables.Add(new Nistec.Caching.Data.SyncSource(tableName, mappingName,sourceName, new SyncTimer(ts, syncType)));
            }
        }

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        public void AddDataItem(DataTable dt, string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
            if (AddDataItem(dt, tableName))
            {
                base.SyncTables.Add(new Nistec.Caching.Data.SyncSource(tableName, mappingName, mappingName, new SyncTimer(ts, syncType)));
            }
        }

        /// <summary>
        /// Add Item to SyncTables
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="syncType"></param>
        /// <param name="ts"></param>
        public void AddSyncItem(string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
                base.SyncTables.Add(new Nistec.Caching.Data.SyncSource(tableName, mappingName, mappingName, new SyncTimer(ts, syncType)));
        }

        /// <summary>
        /// Add Item to SyncTables
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="ts"></param>
        public void AddSyncItem(string tableName, string mappingName, string sourceName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
                base.SyncTables.Add(new Nistec.Caching.Data.SyncSource(tableName, mappingName, sourceName, new SyncTimer(ts, syncType)));
        }

        #region Active record

 
        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        public bool AddDataItem(IActiveConfig dt, string tableName)
        {
            return AddDataItem(dt.DataSource,tableName);
            //AddRemotingDataItemInternal(dt, tableName, size);
        }

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="syncSource"></param>
        public void AddDataItem(IActiveConfig dt, string tableName, Nistec.Caching.Data.SyncSource syncSource)
        {
            if (AddDataItem(dt, tableName))
            {
                base.SyncTables.Add(syncSource);
            }
        }
        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        public void AddDataItem(IActiveConfig dt, string tableName, string mappingName,string sourceName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
            if (AddDataItem(dt, tableName))
            {
                base.SyncTables.Add(new Nistec.Caching.Data.SyncSource(tableName, mappingName,sourceName, new SyncTimer(ts, syncType)));
            }
        }
        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        public void AddDataItem(IActiveConfig dt, string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts)
        {
            if (AddDataItem(dt, tableName))
            {
                base.SyncTables.Add(new Nistec.Caching.Data.SyncSource(tableName, mappingName, mappingName, new SyncTimer(ts, syncType)));
            }
        }

        #endregion

        #endregion

        #region ctor

        //public RemoteData(string cacheName)
        //{
        //    base.CacheName=cacheName;
        //}

        /// <summary>
        /// RemoteData ctor
        /// </summary>
        /// <param name="prop"></param>
        public RemoteData(DataCacheProperties prop)
            :base(prop.DataCacheName, prop.ConnectionString,prop.Provider)
        {
            base.CacheName=prop.DataCacheName;
            base.SyncOption = prop.DataSyncOption;
            if (!string.IsNullOrEmpty(prop.Xmlsettings))
            {
                LoadDataCacheItems(prop.GetItemsSettings());
            }

        }

        /// <summary>
        /// RemoteData Ctor 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="providerDb"></param>
        public RemoteData(string cacheName, string connection, DBProvider provider)
            : base(cacheName,connection, provider)
        {
            base.CacheName="McRemoteData";
        }
        /// <summary>
        /// RemoteData Ctor 
        /// </summary>
        /// <param name="dalBase"></param>
        public RemoteData(string cacheName, IDalBase dalBase)
            : base(cacheName,dalBase)
        {
            base.CacheName = "McRemoteData";
        }

        /// <summary>
        /// RemoteData Ctor 
        /// </summary>
        /// <param name="dalBase"></param>
        public RemoteData(string cacheName,Nistec.Data.Factory.AutoDb dalDB)
            : base(cacheName,dalDB)
        {
            base.CacheName = "McRemoteData";
        }
  
        /// <summary>
        /// Start storage
        /// </summary>
        public void Start(string XmlConigFile)//, string lockKey)
        {
            if (base.Initilized)
                return;

            //string xmlConfig = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string xmlConfig = Environment.CurrentDirectory + "\\" + XmlConigFile;
            if (string.IsNullOrEmpty(xmlConfig))
                return;
            if (!File.Exists(xmlConfig))
                return;

            base.LoadXmlConfigFile(xmlConfig);
            base.Start();//lockKey);
        }

  
        #endregion


        #region Statistic

//<?xml version="1.0" encoding="utf-8" ?> 
//<DalCache>

//  <CacheName></CacheName>
//  <SyncOption></SyncOption>
//  <ConnectionString></ConnectionString>
  
//  <DataSource>
//    <Table Name="" MappingName=""></Table>
//  </DataSource>

//  <SyncSource>
//    <Table Name="" MappingName="" SyncType="" SyncTime=""></Table>
//  </SyncSource>
  
//</DalCache>

        public void CacheToXmlConfig(string fileName)
        {

            //  <RemoteData>
            //  <Settings>
            //    <ConnectionString value ="Data Source=MCONTROL; Initial Catalog=Northwind; uid=sa;password=tishma; Connection Timeout=30"/>
            //    <Provider value ="SqlServer"/>
            //    <DataCacheName value ="McRemoteData"/>
            //    <LoadRemoteSettings value ="true"/>
            //  </Settings>

            //  <DataSource>
            //    <Table Name="Customers">
            //      <MappingName value="Customers"/>
            //      <SyncType value="Interval"/>
            //      <SyncTime value="0:20:0"/>
            //    </Table>
            //  </DataSource>
            //</RemoteData>

            try
            {
                Nistec.Xml.XmlBuilder builder = new Nistec.Xml.XmlBuilder();
                builder.AppendXmlDeclaration();
                builder.AppendEmptyElement("RemoteData", 0);
                builder.AppendElement(0,"Settings","", 1);

                //builder.Attributes.Add("value", this.dbCmd == null ? "" : this.dbCmd.ConnectionString);
                builder.AppendElementAttributes(1, "ConnectionString", "", "value", this.dbCmd.ConnectionString);
                builder.AppendElementAttributes(1, "Provider", "", "value", this.dbCmd.DBProvider.ToString());
                builder.AppendElementAttributes(1, "DataCacheName", "", "value", this.CacheName);

                builder.AppendEmptyElement(0, "DataSource", 2);

                foreach (DataTable dt in DataSource.Tables)
                {
                    string tableName = dt.TableName;

                    System.Xml.XmlNode node = builder.AppendElement(2, "Table", "");
                    builder.AppendAttribute(node, "Name", tableName);

                    if (this.SyncTables.Contains(tableName))
                    {
                        SyncSource s = this.SyncTables[tableName];
                        builder.AppendAttribute(node, "MappingName", s.MappingName);
                        builder.AppendAttribute(node, "SyncType", s.SyncType.ToString());
                        builder.AppendAttribute(node, "SyncTime", s.SyncTime.ToString());
                        //builder.AppendElementAttributes(node, "MappingName","","value", s.MappingName);
                        //builder.AppendElementAttributes(node, "SyncType", "", "value", s.SyncType.ToString());
                        //builder.AppendElementAttributes(node, "SyncTime", "", "value", s.SyncTime.ToString());

                    }
                }



                //builder.AppendElement(0, "CacheName", this.CacheName);
                //builder.AppendElement(0, "SyncOption", this.SyncOption.ToString());
                //builder.AppendElement(0, "ConnectionString",this.dbCmd==null?"": this.dbCmd.ConnectionString);
                //builder.AppendEmptyElement(0, "DataSource", 1);
                //foreach (DataTable dt in DataSource.Tables)
                //{
                //    System.Xml.XmlNode node = builder.AppendElement(1, "Table", "");
                //    builder.AppendAttribute(node, "Name", dt.TableName);
                //    builder.AppendAttribute(node, "MappingName", dt.TableName);
                //}

                //builder.AppendEmptyElement(0, "SyncSource", 2);
                //foreach (SyncSource s in this.SyncTables)
                //{
                //    System.Xml.XmlNode node = builder.AppendElement(2, "Table", "");
                //    builder.AppendAttribute(node, "Name", s.TableName);
                //    builder.AppendAttribute(node, "MappingName", s.MappingName);
                //    builder.AppendAttribute(node, "SyncType", s.SyncType.ToString());
                //    builder.AppendAttribute(node, "SyncTime", s.SyncTime.ToString());
                //}
                builder.Document.Save(fileName);
                //System.Xml.XmlDataDocument doc = CacheToXmlData();
                //doc.Save(fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CacheToXml()
        {
            return DataSource.GetXml();
        }

    
        //public static void XmlToRemoteData(string cacheName, string filName)
        //{
        //    RemoteData cache = new RemoteData(cacheName);
        //    cache.LoadXmlConfigFile(filName);
        //}

        public void PrintCache()
        {
            Console.Write(CacheToXml());
        }


        /// <summary>
        /// GetStatistic
        /// </summary>
        /// <returns></returns>
        public DataCacheStatistic GetStatistic()
        {
            return new DataCacheStatistic(this);
        }
        #endregion

  
    }



}


