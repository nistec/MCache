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
using Nistec.Data;
using System.Collections.Generic;

using Nistec.Caching;
using System.Xml;
using Nistec.Xml;
using Nistec.Data.Advanced;
using Nistec.Caching.Sync;

namespace Nistec.Caching.Data
{

    /// <summary>
    /// Represent a runtime data cache entry item in <see cref="DataCache"/>.
    /// This is usefull for data transfer and reporting.
    /// </summary>
    [Serializable]
    public class DataCacheItem //: SyncSource
    {
        /// <summary>
        /// Initialize a new instance of data cache item.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        public DataCacheItem(DataTable dt, string tableName)
        {
            _EntityName = tableName;
            _ViewName = tableName;
            _SourceName = new string[]{ tableName};
            _IsSync = false;

            _PreserveChanges = false;
            _MissingSchemaAction = MissingSchemaAction.Ignore;
            _SyncTime =TimeSpan.Zero;
            _SyncType = SyncType.None;
            _LastSync = "";

            SetProperties(dt);
        }
        /// <summary>
        /// Initialize a new instance of data cache item.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="source"></param>
        public DataCacheItem(DataTable dt, DataSyncEntity source)
        {
            _EntityName = source.EntityName;
            _ViewName = source.ViewName;
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

        /// <summary>
        /// Get item as object array.
        /// </summary>
        public object[] ItemArray
        {
            get
            {
                string sourceName = string.Join(",", _SourceName);
                return new object[] { _EntityName, _ViewName, sourceName, _IsSync, _PreserveChanges, _MissingSchemaAction, _SyncType, _SyncTime, _LastSync, _RecordCount, _ColumnCount, _Size };
            }
        }

        #region memebers

        private int _ColumnCount;
        /// <summary>
        /// Get columns count.
        /// </summary>
        public int ColumnCount
        {
            get { return _ColumnCount; }
        }

        private int _RecordCount;
        /// <summary>
        /// Get record count.
        /// </summary>
        public int RecordCount
        {
            get { return _RecordCount; }
        }
        private int _Size;
        /// <summary>
        /// Get size of object.
        /// </summary>
        public int Size
        {
            get { return _Size; }
        }
        /// <summary>
        /// ViewName
        /// </summary>
        private string _ViewName;
        /// <summary>
        /// Get the view name of current item.
        /// </summary>
        public string ViewName
        {
            get { return _ViewName; }
        }  
        /// <summary>
        /// EntityName
        /// </summary>
        private string _EntityName;
        /// <summary>
        /// Get the entity name of current item.
        /// </summary>
        public string EntityName
        {
            get { return _EntityName; }
        }
        /// <summary>
        /// SourceName
        /// </summary>
        private string[] _SourceName;
        /// <summary>
        /// Get the list of source names of current item.
        /// </summary>
        public string[] SourceName
        {
            get { return _SourceName; }
        }  

        private bool _IsSync;
        /// <summary>
        /// Get whether the current item use async.
        /// </summary>
        public bool IsSync
        {
            get { return _IsSync; }
        }
        /// <summary>
        /// PreserveChanges
        /// </summary>
        private bool _PreserveChanges;
        /// <summary>
        /// Get indicate whether the current item use PreserveChanges.
        /// </summary>
        public bool PreserveChanges
        {
            get { return _PreserveChanges; }
        }  
        /// <summary>
        /// MissingSchemaAction
        /// </summary>
        private MissingSchemaAction _MissingSchemaAction;
        /// <summary>
        /// Get the <see cref="MissingSchemaAction"/> of current item.
        /// </summary>
        public MissingSchemaAction MissingSchemaAction
        {
            get { return _MissingSchemaAction; }
        }
        /// <summary>
        /// syncTime
        /// </summary>
        private TimeSpan _SyncTime;
        /// <summary>
        /// Get the sync time for synchronization as <see cref="TimeSpan"/>.
        /// </summary>
        public TimeSpan SyncTime
        {
            get { return _SyncTime; }
        }  
        /// <summary>
        /// syncTime
        /// </summary>
        private SyncType _SyncType;
        /// <summary>
        /// Get the <see cref="SyncType"/> of current item.
        /// </summary>
        public SyncType SyncType
        {
            get { return _SyncType; }
        }
        /// <summary>
        /// Get Last Sync
        /// </summary>
        private string _LastSync;
        /// <summary>
        /// Get the last synchronization time of current item.
        /// </summary>
        public string LastSync
        {
            get { return _LastSync; }
        }


        #endregion

      
    }



}
