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
   /// Represent a utility functions for session cache.
   /// </summary>
    public class SessionUtil
    {
        /// <summary>
        /// Get session key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static string SessionKey(string key, string sessionId)
        {

            int index = key.IndexOf('$');
            if (index > 0)
                return key;

            if (string.IsNullOrEmpty(sessionId))
            {
                return key;
            }
            return string.Format("{0}${1}", sessionId, key);
        }
        /// <summary>
        /// Get session from key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string SessionFromKey(string key)
        {
            int index = key.IndexOf('$');
            return (index < 0) ? "" : key.Substring(0, index);
        }
    }
}
