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
using System.Data;

namespace Nistec.Caching.Data
{
    /// <summary>
    /// Represent a utility functions for data cache.
    /// </summary>
    public class DataCacheUtil
    {
        /// <summary>
        /// Get <see cref="DataSet"/> size in bytes.
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static long DataSetSize(DataSet ds)
        {
            long length = 0;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ds.WriteXml(ms);
                ms.Flush();
                length = ms.Length;
                ms.Close();
            }


            return length;

        }

        /// <summary>
        /// Get <see cref="DataTable"/> size in bytes.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long DataTableSize(DataTable dt)
        {
            long length = 0;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                dt.WriteXml(ms);
                ms.Flush();
                length = ms.Length;
                ms.Close();
            }


            return length;

        }

    }
}
