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

namespace Nistec.Caching.Data
{

    
    /// <summary>
    /// SyncDataSourceChangedEventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SyncDataSourceChangedEventHandler(object sender, SyncDataSourceChangedEventArgs e);

    /// <summary>
    /// SyncDataSourceEventArgs
    /// </summary>
    public class SyncDataSourceChangedEventArgs : EventArgs
    {
        private string sourceName;
        /// <summary>
        /// SyncDataSourceChangedEventArgs
        /// </summary>
        /// <param name="name"></param>
        public SyncDataSourceChangedEventArgs(string name)
        {
            this.sourceName = name;
        }

        #region Properties Implementation
        /// <summary>
        /// SourceName
        /// </summary>
        public string SourceName
        {
            get { return this.sourceName; }
        }

        #endregion

    }

   

}
