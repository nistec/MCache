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

namespace Nistec.Caching.Data
{

    /// <summary>
    /// Data Cache Error Type
    /// </summary>
    public enum DataCacheError
    {
        /// <summary>
        /// ErrorUnexpected
        /// </summary>
        ErrorUnexpected = 1000,
        /// <summary>
        /// ErrorInitialized
        /// </summary>
        ErrorInitialized = 1001,
        /// <summary>
        /// ErrorCreateCache
        /// </summary>
        ErrorCreateCache = 1002,
        /// <summary>
        /// ErrorStoreData
        /// </summary>
        ErrorStoreData = 1003,
        /// <summary>
        /// ErrorFileNotFound
        /// </summary>
        ErrorFileNotFound = 1004,
        /// <summary>
        /// ErrorReadFromXml
        /// </summary>
        ErrorReadFromXml = 1005,
        /// <summary>
        /// ErrorWriteToXml
        /// </summary>
        ErrorWriteToXml = 1006,
        /// <summary>
        /// ErrorSyncCachee
        /// </summary>
        ErrorSyncCache = 1007,
        /// <summary>
        /// ErrorSetValue
        /// </summary>
        ErrorSetValue = 1008,
        /// <summary>
        /// ErrorReadValue
        /// </summary>
        ErrorReadValue = 1009,
        /// <summary>
        /// ErrorTableNotExist
        /// </summary>
        ErrorTableNotExist = 1010,
        /// <summary>
        /// ErrorColumnNotExist
        /// </summary>
        ErrorColumnNotExist = 1011,
        /// <summary>
        /// ErrorInFilterExspression
        /// </summary>
        ErrorInFilterExspression = 1012,
        /// <summary>
        /// ErrorCastingValue
        /// </summary>
        ErrorCastingValue = 1013,
        /// <summary>
        /// ErrorGetValue
        /// </summary>
        ErrorGetValue = 1014,
        /// <summary>
        /// ErrorMergeData
        /// </summary>
        ErrorMergeData = 1015,
        /// <summary>
        /// ErrorUpdateChanges
        /// </summary>
        ErrorUpdateChanges = 1016

    }
	/// <summary>
	/// Data Cache Exception Event Handler
	/// </summary>
	public delegate void DataCacheExceptionEventHandler(object sender, DataCacheExceptionEventArgs e);

	/// <summary>
	/// Data Cache Exception Event Args.
	/// </summary>
	public class DataCacheExceptionEventArgs:EventArgs
	{
		/// <summary>
		/// Get ErrorMessage
		/// </summary>
		public readonly string ErrorMessage;
		/// <summary>
		/// Get DataErrors
		/// </summary>
        public readonly DataCacheError Error;

		/// <summary>
        /// DataCacheExceptionEventArgs
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="error"></param>
        public DataCacheExceptionEventArgs(string msg, DataCacheError error)
		{
			ErrorMessage=msg;
			Error=error;
		}
	}
}
