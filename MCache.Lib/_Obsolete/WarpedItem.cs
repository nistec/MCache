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
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Web.SessionState;

using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Nistec.Caching;
using Nistec.Caching.Remote;
using System.Data;
using Nistec.Runtime;
using Nistec.Serialization;


namespace Nistec.Caching
{
    #region Warped Item

    /// <summary>
    /// Represent a wrapped object item.
    /// </summary>
    [Serializable]
    public struct WarpedItem
    {
        /// <summary>
        /// Get item name.
        /// </summary>
        public readonly string Name;
        GenericHashtable _Item;
        /// <summary>
        /// Get indicate if item is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _Item == null || _Item.Count == 0; }
        }
        /// <summary>
        /// Initialize a new instance of wrapped item.
        /// </summary>
        /// <param name="name"></param>
        public WarpedItem(string name)
        {
            Name = name;
            _Item = new GenericHashtable();
        }
       

        /// <summary>
        /// Get item.
        /// </summary>
        public GenericHashtable Item
        {
            get
            {
                if (_Item == null)
                {
                    _Item = new GenericHashtable();
                }
                return _Item;
            }
            set { _Item = value; }
        }
        /// <summary>
        /// Add a new item.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, object value)
        {
            Item[key] = value;
        }
        /// <summary>
        /// Serialize item tobase 64 string.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return NetSerializer.SerializeToBase64(this);
        }
        /// <summary>
        /// Desrialize item from base 64 string.
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static WarpedItem Deserialize(string base64)
        {
            object o = NetSerializer.DeserializeFromBase64(base64);
            return (WarpedItem)o;
        }
        /// <summary>
        /// Create data source.
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="columns"></param>
        /// <param name="timeoutMinute"></param>
        /// <returns></returns>
        public static WarpedItem CreateDataSource(object ds, object columns, int timeoutMinute)
        {
           if (ds.GetType() == typeof(System.Data.DataView))
            {
                ds = ((System.Data.DataView)ds).Table;
            }
            WarpedItem wi = new WarpedItem("DS");
            wi.Add("DATA", ds);
            wi.Add("COLUMNS", columns);

            return wi;
        }
        
    }
    #endregion

  
}