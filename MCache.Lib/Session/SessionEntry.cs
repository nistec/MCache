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
using System.IO;
using Nistec.Channels;
using Nistec.Caching.Remote;
using Nistec.Generic;
using System.Data;
using System.Xml;
using System.Collections;
using Nistec.Data.Entities;

namespace Nistec.Caching.Session
{
    /// <summary>
    /// Represent a session cache entry in session bag.
    /// </summary>
    [Serializable]
    public class SessionEntry: EntityStream, IDisposable
    {
        #region properties

        /// <summary>
        /// Get item expiration
        /// </summary>
        public DateTime ExpirationTime
        {
            get { return AllowExpires ? Modified.AddMinutes(Expiration) : Modified.AddYears(1); }
        }

        /// <summary>
        /// Get if item Allow expired
        /// </summary>
        public bool AllowExpires
        {
            get { return Expiration > 0; }
        }

        /// <summary>
        /// Get indicate whether the item is timeout 
        /// </summary>
        public bool IsTimeOut
        {
            get { return AllowExpires && TimeOut < DateTime.Now.Subtract(Modified); }
        }

        /// <summary>
        /// Get or Set the item time out
        /// </summary>
        public TimeSpan TimeOut
        {
            get { return TimeSpan.FromMinutes(Expiration); }
         }

        #endregion

        #region ctor
         /// <summary>
         /// ctor
         /// </summary>
        public SessionEntry()
            : base()
        {

        }
         /// <summary>
         /// ctor
         /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
         /// <param name="value"></param>
         /// <param name="expiration"></param>
        public SessionEntry(string sessionId,string key, object value, int expiration)
            : this()
        {
            Id = sessionId;
            Key = key;
            Expiration = expiration;
            SetBody(value);
           
        }
       
         /// <summary>
         /// ctor
         /// </summary>
         /// <param name="key"></param>
         /// <param name="value"></param>
         /// <param name="type"></param>
         /// <param name="expiration"></param>
        public SessionEntry(string key, byte[] value, Type type, int expiration)
            : this()
        {
            Key = key;
            Expiration = expiration;
            SetBody(value, type);
          
        }
         /// <summary>
        /// Initialize a new instance of session entry using <see cref="MessageStream"/>.
         /// </summary>
         /// <param name="m"></param>
        public SessionEntry(MessageStream m)
            : this()
        {
            Key = m.Key;
            Expiration = m.Expiration;
            Id = m.Id;
            SetBody(m.BodyStream, m.TypeName);
        }
        #endregion

        #region Dispose
         /// <summary>
         /// Destructor.
         /// </summary>
        ~SessionEntry()
        {
            Dispose(false);
        }

       
        #endregion

    }

}
