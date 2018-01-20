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
using Nistec.Caching.Sync;

namespace Nistec.Caching
{
 

    /// <summary>
	/// CacheErrors
	/// </summary>
	public enum CacheErrors
	{
		/// <summary>
		/// ErrorUnexpected
		/// </summary>
		ErrorUnexpected=-1000,
		/// <summary>
		/// ErrorInitialized
		/// </summary>
		ErrorInitialized=-1001,
		/// <summary>
		/// ErrorCreateStorage
		/// </summary>
		ErrorCreateStorage=-1002,
		/// <summary>
		/// ErrorStoreData
		/// </summary>
		ErrorStoreData=-1003,
		/// <summary>
		/// ErrorFileNotFound
		/// </summary>
		ErrorFileNotFound=-1004,
		/// <summary>
		/// ErrorReadFromXml
		/// </summary>
		ErrorReadFromXml=-1005,
		/// <summary>
		/// ErrorWriteToXml
		/// </summary>
		ErrorWriteToXml=-1006,
		/// <summary>
		/// ErrorSyncStorage
		/// </summary>
		ErrorSyncStorage=-1007,
		/// <summary>
		/// ErrorSetValue
		/// </summary>
		ErrorSetValue=-1008,
		/// <summary>
		/// ErrorReadValue
		/// </summary>
		ErrorReadValue=-1009,
		/// <summary>
		/// ErrorTableNotExist
		/// </summary>
		ErrorTableNotExist=-1010,
		/// <summary>
		/// ErrorColumnNotExist
		/// </summary>
		ErrorColumnNotExist=-1011,
		/// <summary>
		/// ErrorInFilterExspression
		/// </summary>
		ErrorInFilterExspression=-1012,
		/// <summary>
		/// ErrorCastingValue
		/// </summary>
		ErrorCastingValue=-1013,
		/// <summary>
		/// ErrorGetValue
		/// </summary>
		ErrorGetValue=-1014,
        /// <summary>
        /// ErrorNotSupportedItem
        /// </summary>
        ErrorNotSupportedItem = -1015

	}

    #region CacheEntryEventsArgs
    /// <summary>
    /// CacheItemChangedEventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CacheEntryChangedEventHandler(object sender, CacheEntryChangedEventArgs e);

    /// <summary>
    /// CacheEventArgs
    /// </summary>
    public class CacheEntryChangedEventArgs : EventArgs
    {
        int _Size;
        string _Key;
        CacheAction action;

        /// <summary>
        /// CacheEntryChanged EventArgs
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cacheKey"></param>
        /// <param name="size"></param>
        public CacheEntryChangedEventArgs(CacheAction action, string cacheKey, int size)
        {
            _Key = cacheKey;
            _Size = size;
            this.action = action;
        }

        #region Properties Implementation
        /// <summary>
        /// CacheItem Key
        /// </summary>
        public string Key
        {
            get { return _Key; }
        }
        /// <summary>
        /// CacheItem Size
        /// </summary>
        public int Size
        {
            get { return _Size; }
        }
        /// <summary>
        /// Get <see cref="CacheAction"/> action.
        /// </summary>
        public CacheAction Action
        {
            get { return this.action; }
        }
  
        #endregion

    }

    #endregion

    #region SyncTimeCompleted
    /// <summary>
    /// SyncTimeCompletedEventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SyncTimeCompletedEventHandler(object sender, SyncTimeCompletedEventArgs e);

    /// <summary>
    /// CacheEventArgs
    /// </summary>
    public class SyncTimeCompletedEventArgs : EventArgs
    {
        string[] items;
 
        /// <summary>
        /// SyncTimeCompletedEventArgs
        /// </summary>
        /// <param name="items"></param>
        public SyncTimeCompletedEventArgs(string[] items)
        {
            this.items = items;
        }

        #region Properties Implementation
        /// <summary>
        /// Items
        /// </summary>
        public string[] Items
        {
            get { return this.items; }
        }
     
        #endregion

    }

    #endregion

    #region SyncTimerItemEventHandler
    /// <summary>
    /// SyncTimeCompletedEventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SyncTimerItemEventHandler(object sender, SyncTimerItemEventArgs e);

    /// <summary>
    /// CacheEventArgs
    /// </summary>
    public class SyncTimerItemEventArgs : EventArgs
    {
        Dictionary<string, TimerItem> items;

        /// <summary>
        /// SyncTimeCompletedEventArgs
        /// </summary>
        /// <param name="items"></param>
        public SyncTimerItemEventArgs(Dictionary<string, TimerItem> items)
        {
            this.items = items;
        }

        #region Properties Implementation
        /// <summary>
        /// Items
        /// </summary>
        public Dictionary<string, TimerItem> Items
        {
            get { return this.items; }
        }

        #endregion

    }

    #endregion

    #region SyncEntityTimeCompleted
    /// <summary>
    /// SyncTimeCompletedEventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SyncEntityTimeCompletedEventHandler(object sender, SyncEntityTimeCompletedEventArgs e);

    /// <summary>
    /// CacheEventArgs
    /// </summary>
    public class SyncEntityTimeCompletedEventArgs : EventArgs
    {
        SyncBoxTask item;

        /// <summary>
        /// SyncTimeCompletedEventArgs
        /// </summary>
        /// <param name="item"></param>
        internal SyncEntityTimeCompletedEventArgs(SyncBoxTask item)
        {
            this.item = item;
        }

        #region Properties Implementation
        /// <summary>
        /// Items
        /// </summary>
        internal SyncBoxTask Item
        {
            get { return this.item; }
        }

        #endregion

    }

    #endregion


    #region CacheException
    /// <summary>
	/// CacheExceptionEventHandler
	/// </summary>
	public delegate void CacheExceptionEventHandler(object sender, CacheExceptionEventArgs e);

	/// <summary>
	/// Summary description for CacheExceptionEventArgs.
	/// </summary>
	public class CacheExceptionEventArgs:EventArgs
	{
		/// <summary>
		/// Get ErrorMessage
		/// </summary>
		public readonly string ErrorMessage;
		/// <summary>
		/// Get CacheErrors
		/// </summary>
		public readonly CacheErrors Error;

		/// <summary>
		/// CacheExceptionEventArgs
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="error"></param>
		public CacheExceptionEventArgs(string msg,CacheErrors error)
		{
			ErrorMessage=msg;
			Error=error;
		}
	}
    #endregion

    #region QueueItemEventsArgs
    /// <summary>
    /// SyncDataSourceEventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SyncCacheEventHandler(object sender, SyncCacheEventArgs e);

    /// <summary>
    /// SyncDataSourceEventArgs
    /// </summary>
    public class SyncCacheEventArgs : EventArgs
    {
        private string sourceName;
        /// <summary>
        /// SyncDataSourceEventArgs
        /// </summary>
        /// <param name="name"></param>
        public SyncCacheEventArgs(string name)
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

    #endregion

}

