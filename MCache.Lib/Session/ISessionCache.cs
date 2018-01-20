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

namespace Nistec.Caching.Session
{
    /// <summary>
    /// Interface for ISessionCache
    /// </summary>
    public interface ISessionCache
    {
        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="newSize"></param>
        /// <param name="currentCount"></param>
        /// <param name="newCount"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        void SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange);

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="CacheException"/> will thrown.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        void SizeExchage(ISessionBag oldItem, ISessionBag newItem);
       
        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        void SizeRefresh();
    }
    /// <summary>
    /// Interface for SessionBag
    /// </summary>
    public interface ISessionBag
    {
        #region properties
        /// <summary>
        /// Get session id.
        /// </summary>
        string SessionId { get; }
        /// <summary>
        /// Get Session Creation time.
        /// </summary>
        DateTime Creation { get; }
        /// <summary>
        /// Get session timeout.
        /// </summary>
        int Timeout { get; }
     
        /// <summary>
        /// Get the last used of current session.
        /// </summary>
        DateTime LastUsed { get; }
        /// <summary>
        /// Get Args of current session.
        /// </summary>
        string Args { get; }
        /// <summary>
        /// Get the user id who created the current session.
        /// </summary>
        string UserId { get; }
        
        /// <summary>
        /// Get the size in bytes of current session.
        /// </summary>
        long Size { get; }
        /// <summary>
        /// Get the items count of current session.
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Determines whether the session bag contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(string key);
       
        /// <summary>
        /// Get <see cref="SessionEntry"/> item from session bag by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        SessionEntry Get(string key);

        /// <summary>
        /// Add new <see cref="SessionEntry"/> to the session bag.
        /// </summary>
        /// <param name="value"></param>
        CacheState AddItem(SessionEntry value);

        /// <summary>
        /// Add new <see cref="SessionEntry"/> to the session bag.
        /// </summary>
        /// <param name="value"></param>
        CacheState SetItem(SessionEntry value);

        ///// <summary>
        ///// Add new <see cref="SessionEntry"/> to the session bag.
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        //void AddItem(string key, SessionEntry value);
        
  
        /// <summary>
        /// Print current session.
        /// </summary>
        /// <returns></returns>
        string Print();
       

        #endregion

    }
    /// <summary>
    /// Interface for SessionBagStream
    /// </summary>
    public interface ISessionBagStream
    {
        #region properties
        /// <summary>
        /// Get session id.
        /// </summary>
        string SessionId { get;}
        /// <summary>
        /// Get Session Creation time.
        /// </summary>
        DateTime Creation { get; }
        /// <summary>
        /// Get session timeout.
        /// </summary>
        int Timeout { get; }

        /// <summary>
        /// Get the last used of current session.
        /// </summary>
        DateTime LastUsed { get; }
        /// <summary>
        /// Get Args of current session.
        /// </summary>
        string Args { get; }
        /// <summary>
        /// Get the user id who created the current session.
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Get the size in bytes of current session.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Determines whether the session bag contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(string key);
        
        /// <summary>
        /// Get <see cref="SessionEntry"/> item from session bag by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        SessionEntry Get(string key);
        
        /// <summary>
        /// Print current session.
        /// </summary>
        /// <returns></returns>
        string Print();
       

        #endregion

    }
}
