using System;
using System.Data;
using System.Collections;
using System.Threading;
using Nistec.Data;
using System.Collections.Generic;

using Nistec.Caching;
using System.Xml;
using Nistec.Xml;
using Nistec.Data.Advanced;

namespace Nistec.Legacy
{


 
    [Serializable]
    public class DataCacheItem //: SyncSource
    {

        public DataCacheItem(DataTable dt, string tableName)
        {
            _TableName = tableName;
            _MappingName = tableName;
            _SourceName = tableName;
            _IsSync = false;

            _PreserveChanges = false;
            _MissingSchemaAction = MissingSchemaAction.Ignore;
            _SyncTime =TimeSpan.Zero;
            _SyncType = SyncType.None;
            _LastSync = "";

            SetProperties(dt);
        }

        public DataCacheItem(DataTable dt, SyncSource source)
        {
            _TableName = source.TableName;
            _MappingName = source.MappingName;
            _SourceName = source.SourceName;
            _PreserveChanges = source.PreserveChanges;
            _MissingSchemaAction = source.MissingSchemaAction;
            _SyncTime = source.SyncTime.Interval;
            _SyncType = source.SyncType;
            _IsSync = true;
            _LastSync = source.LastSync;
            SetProperties(dt);
        }

        private void SetProperties(DataTable dt)
        {
            _RecordCount = dt.Rows.Count;
            _ColumnCount = dt.Columns.Count;
            DataSet ds=new DataSet();
            ds.Tables.Add(dt.Copy());
            _Size=DataSetUtil.DataSetToByteCount(ds,true);

        }

        public object[] ItemArray
        {
            get 
            {
                return new object[] { _TableName, _MappingName,_SourceName, _IsSync, _PreserveChanges, _MissingSchemaAction, _SyncType, _SyncTime,_LastSync, _RecordCount, _ColumnCount, _Size }; 
           
            }
        }

        #region memebers

        private int _ColumnCount;
        public int ColumnCount
        {
            get { return _ColumnCount; }
        }

        private int _RecordCount;
        public int RecordCount
        {
            get { return _RecordCount; }
        }
        private int _Size;
        public int Size
        {
            get { return _Size; }
        }
        /// <summary>
        /// MappingName
        /// </summary>
        private string _MappingName;
        public string MappingName
        {
            get { return _MappingName; }
        }  
        /// <summary>
        /// TableName
        /// </summary>
        private string _TableName;
        public string TableName
        {
            get { return _TableName; }
        }
        /// <summary>
        /// SourceName
        /// </summary>
        private string _SourceName;
        public string SourceName
        {
            get { return _SourceName; }
        }  

        private bool _IsSync;
        public bool IsSync
        {
            get { return _IsSync; }
        }
        /// <summary>
        /// PreserveChanges
        /// </summary>
        private bool _PreserveChanges;
        public bool PreserveChanges
        {
            get { return _PreserveChanges; }
        }  
        /// <summary>
        /// MissingSchemaAction
        /// </summary>
        private MissingSchemaAction _MissingSchemaAction;
        public MissingSchemaAction MissingSchemaAction
        {
            get { return _MissingSchemaAction; }
        }
        /// <summary>
        /// syncTime
        /// </summary>
        private TimeSpan _SyncTime;
        public TimeSpan SyncTime
        {
            get { return _SyncTime; }
        }  
        /// <summary>
        /// syncTime
        /// </summary>
        private SyncType _SyncType;
        public SyncType SyncType
        {
            get { return _SyncType; }
        }
        /// <summary>
        /// Get Last Sync
        /// </summary>
        private string _LastSync;
        public string LastSync
        {
            get { return _LastSync; }
        }


        #endregion
    }


    [Serializable]
    public class DataCacheItemProperties 
    {

        public DataCacheItemProperties(string tableName)
        {
            _TableName = tableName;
            _MappingName = tableName;
            _SyncType = SyncType.None;
        }

        public DataCacheItemProperties(string tableName, string mappingName,string sourceName, SyncType syncType, TimeSpan syncTime)
        {
            _TableName = tableName;
            _MappingName = mappingName;
            _SourceName = sourceName;
            _SyncTime = syncTime;
            _SyncType = syncType;
        }
        public DataCacheItemProperties(string tableName, string mappingName, SyncType syncType, TimeSpan syncTime)
        {
            _TableName = tableName;
            _MappingName = mappingName;
            _SourceName = mappingName;
            _SyncTime = syncTime;
            _SyncType = syncType;
        }
        public DataCacheItemProperties(XmlNode node)
        {
    //<Table Name="">
    //  <MappingName value="Customers"/>
    //  <SyncType value="Interval"/>
    //  <SyncTime value="0:20:0"/>
    //</Table>

            if (node == null)
            {
                throw new ArgumentException("Inavlid Xml Root, 'Table' ");
            }

            XmlParser parser = new XmlParser(node.OuterXml);
            
            _TableName = parser.GetAttributeValue(node, "Name", true);
            _MappingName = parser.GetAttributeValue(node, "MappingName", _MappingName);
            _SourceName = parser.GetAttributeValue(node, "SourceName", _MappingName);
            _SyncType = SyncTimer.SyncTypeFromString(parser.GetAttributeValue(node, "SyncType", "None"));
            _SyncTime = SyncTimer.TimeSpanFromString(parser.GetAttributeValue(node, "SyncTime", "0"));

            //_MappingName = parser.GetAttributeValue(node, "MappingName", "value", _TableName);
            //_SyncType =SyncTimer.SyncTypeFromString(parser.GetAttributeValue(node, "SyncType", "value", "None"));
            //_SyncTime =SyncTimer.TimeSpanFromString( parser.GetAttributeValue(node, "SyncTime", "value", "0"));

        }


        public object[] ItemArray
        {
            get
            {
                return new object[] { _TableName, _MappingName,_SourceName, _SyncType, _SyncTime };

            }
        }

        #region memebers

  
        /// <summary>
        /// MappingName
        /// </summary>
        private string _MappingName;
        public string MappingName
        {
            get { return _MappingName; }
        }  /// <summary>
        /// TableName
        /// </summary>
        private string _TableName;
        public string TableName
        {
            get { return _TableName; }
        }
        /// SourceName
        /// </summary>
        private string _SourceName;
        public string SourceName
        {
            get { return _SourceName; }
        }
        //private bool _IsSync;
        //public bool IsSync
        //{
        //    get { return _IsSync; }
        //}
        ///// <summary>
        ///// PreserveChanges
        ///// </summary>
        //private bool _PreserveChanges;
        //public bool PreserveChanges
        //{
        //    get { return _PreserveChanges; }
        //}  /// <summary>
        ///// MissingSchemaAction
        ///// </summary>
        //private MissingSchemaAction _MissingSchemaAction;
        //public MissingSchemaAction MissingSchemaAction
        //{
        //    get { return _MissingSchemaAction; }
        //}
        /// <summary>
        /// syncTime
        /// </summary>
        private TimeSpan _SyncTime;
        public TimeSpan SyncTime
        {
            get { return _SyncTime; }
        }  /// <summary>
        /// syncTime
        /// </summary>
        private SyncType _SyncType;
        public SyncType SyncType
        {
            get { return _SyncType; }
        }
        #endregion
    }


}
