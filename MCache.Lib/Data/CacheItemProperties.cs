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
using System.Data;
using System.Collections;
using System.Threading;
using System.Linq;
using Nistec.Data;
using System.Collections.Generic;

using Nistec.Caching;
using System.Xml;
using Nistec.Xml;
using Nistec.Data.Advanced;
using Nistec.Caching.Sync;
using Nistec.Serialization;

namespace Nistec.Caching.Data
{

    public struct TableProperties
    {
        internal int RecordCount;
        internal int ColumnCount;
        internal long Size;
    }

    /// <summary>
    /// Represent a runtime data cache entry item in <see cref="DataCache"/>.
    /// This is usefull for data transfer and reporting.
    /// </summary>
    [Serializable]
    public class CacheItemProperties //: SyncSource
    {

        public CacheItemProperties()
        {

        }

        /// <summary>
        /// Initialize a new instance of data cache item.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        public CacheItemProperties(TableProperties dt, string tableName)
        {
            EntityName = tableName;
            PrimaryKey = null;
            ViewName = tableName;
            SourceName = new string[]{ tableName};
            IsSync = false;

            PreserveChanges = false;
            MissingSchemaAction = MissingSchemaAction.Ignore;
            SyncTime =TimeSpan.Zero;
            SyncType = SyncType.None;
            LastSync = "";

            //SetProperties(dt);
            RecordCount = dt.RecordCount;
            ColumnCount = dt.ColumnCount;
            Size = dt.Size;

        }
        /// <summary>
        /// Initialize a new instance of data cache item.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="source"></param>
        public CacheItemProperties(TableProperties dt, DataSyncEntity source)
        {
            EntityName = source.EntityName;
            PrimaryKey = source.GetPrimaryKey();
            ViewName = source.ViewName;
            SourceName = source.SourceName;
            PreserveChanges = source.PreserveChanges;
            MissingSchemaAction = source.MissingSchemaAction;
            SyncTime = source.SyncTime.Interval;
            SyncType = source.SyncType;
            IsSync = true;
            LastSync = source.LastSync;
            //SetProperties(dt);
            RecordCount = dt.RecordCount;
            ColumnCount = dt.ColumnCount;
            Size = dt.Size;
        }

        //private void SetProperties(DataTable dt)
        //{
        //    _RecordCount = dt.Rows.Count;
        //    _ColumnCount = dt.Columns.Count;
        //    DataSet ds=new DataSet();
        //    ds.Tables.Add(dt.Copy());
        //    _Size=DataSetUtil.DataSetToByteCount(ds,true);

        //}

        ///// <summary>
        ///// Initialize a new instance of data cache item.
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <param name="tableName"></param>
        //public DataCacheItem(TableProperties dt, string tableName)
        //{
        //    _EntityName = tableName;
        //    _ViewName = tableName;
        //    _SourceName = new string[] { tableName };
        //    _IsSync = false;

        //    _PreserveChanges = false;
        //    _MissingSchemaAction = MissingSchemaAction.Ignore;
        //    _SyncTime = TimeSpan.Zero;
        //    _SyncType = SyncType.None;
        //    _LastSync = "";

        //    //SetProperties(dt);
        //    _RecordCount = dt.RecordCount;
        //    _ColumnCount = dt.ColumnCount;
        //    _Size = dt.Size;

        //}
        ///// <summary>
        ///// Initialize a new instance of data cache item.
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <param name="source"></param>
        //public DataCacheItem(DbTable dt, DataSyncEntity source)
        //{
        //    _EntityName = source.EntityName;
        //    _ViewName = source.ViewName;
        //    _SourceName = source.SourceName;
        //    _PreserveChanges = source.PreserveChanges;
        //    _MissingSchemaAction = source.MissingSchemaAction;
        //    _SyncTime = source.SyncTime.Interval;
        //    _SyncType = source.SyncType;
        //    _IsSync = true;
        //    _LastSync = source.LastSync;
        //    SetProperties(dt);
        //}

        //private void SetProperties(DbTable dt)
        //{
        //    _RecordCount = dt.Count;
        //    _Size = dt.Size;
        //    _RecordCount = dt.Count;
        //    _ColumnCount = dt.DataSource.Values.ElementAt(0).Count;
        //    //_ColumnCount = dt.Columns.Count;
        //    //DbSet ds = new DbSet(dt.);
        //    //ds[dt.]=dt.Copy();
        //    //_Size = DataSetUtil.DataSetToByteCount(ds, true);

        //}
        /// <summary>
        /// Get item as object array.
        /// </summary>
        public object[] ItemArray
        {
            get
            {
                string sourceName = SourceName == null ? null : string.Join(",", SourceName);
                return new object[] { EntityName, ViewName, sourceName, IsSync, PreserveChanges, MissingSchemaAction, SyncType, SyncTime, LastSync, RecordCount, ColumnCount, Size };
            }
        }

        #region memebers

        //private int _ColumnCount;
        /// <summary>
        /// Get columns count.
        /// </summary>
        public int ColumnCount { get; set; }
        
        //private int _RecordCount;
        /// <summary>
        /// Get record count.
        /// </summary>
        public int RecordCount { get; set; }
        //{
        //    get { return _RecordCount; }
        //}
        //private long _Size;
        /// <summary>
        /// Get size of object.
        /// </summary>
        public long Size { get; set; }
        //{
        //    get { return _Size; }
        //}
    
        //private string _ViewName;
        /// <summary>
        /// Get the view name of current item.
        /// </summary>
        public string ViewName { get; set; }
        //{
        //    get { return _ViewName; }
        //}  
     
        //private string _EntityName;
        /// <summary>
        /// Get the entity name of current item.
        /// </summary>
        public string EntityName { get; set; }
        //{
        //    get { return _EntityName; }
        //}
        public string PrimaryKey { get; set; }

        //private string[] _SourceName;
        /// <summary>
        /// Get the list of source names of current item.
        /// </summary>
        public string[] SourceName { get; set; }
        //{
        //    get { return _SourceName; }
        //}  

       // private bool _IsSync;
        /// <summary>
        /// Get whether the current item use async.
        /// </summary>
        public bool IsSync { get; set; }
        //{
        //    get { return _IsSync; }
        //}
      
        //private bool _PreserveChanges;
        /// <summary>
        /// Get indicate whether the current item use PreserveChanges.
        /// </summary>
        public bool PreserveChanges { get; set; }
        //{
        //    get { return _PreserveChanges; }
        //}  
       
        //private MissingSchemaAction _MissingSchemaAction;
        /// <summary>
        /// Get the <see cref="MissingSchemaAction"/> of current item.
        /// </summary>
        public MissingSchemaAction MissingSchemaAction { get; set; }
        //{
        //    get { return _MissingSchemaAction; }
        //}
       
        //private TimeSpan _SyncTime;
        /// <summary>
        /// Get the sync time for synchronization as <see cref="TimeSpan"/>.
        /// </summary>
        public TimeSpan SyncTime { get; set; }
        //{
        //    get { return _SyncTime; }
        //}  
       
        //private SyncType _SyncType;
        /// <summary>
        /// Get the <see cref="SyncType"/> of current item.
        /// </summary>
        public SyncType SyncType { get; set; }
        //{
        //    get { return _SyncType; }
        //}
 
        //private string _LastSync;
        /// <summary>
        /// Get the last synchronization time of current item.
        /// </summary>
        public string LastSync { get; set; }
        //{
        //    get { return _LastSync; }
        //}


        #endregion

      
    }



}
