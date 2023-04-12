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
using Nistec.IO;
using Nistec.Caching.Config;

namespace Nistec.Caching.Session
{
    /// <summary>
    /// Represent a session cache entry in session bag.
    /// </summary>
    [Serializable]
    public class SessionEntry : EntityStream, IDisposable
    {

        #region properties

        /// <summary>
        /// Get item expiration
        /// </summary>
        public DateTime ExpirationTime
        {
            get { return AllowExpires ? Modified.AddMinutes(Expiration) : Modified.AddMonths(12); }
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
            get { return AllowExpires && DateTime.Now.Subtract(Modified) > TimeOut; }
        }

        /// <summary>
        /// Get or Set the item time out
        /// </summary>
        public TimeSpan TimeOut
        {
            get { return TimeSpan.FromMinutes(Expiration); }
        }

        public object Body { get; set; }

        #endregion

        #region ctor
        /// <summary>
        /// ctor
        /// </summary>
        public SessionEntry()
            : base()
        {
            Expiration = CacheSettings.SessionTimeout;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public SessionEntry(string sessionId, string key, object value, int expiration)
            : this()
        {
            SessionId = sessionId;
            Id = key;
            Expiration = CacheSettings.GetValidSessionTimeout(expiration);
            SetBody(value);

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="expiration"></param>
        public SessionEntry(string sessionId, string key, byte[] value, Type type, int expiration)
            : this()
        {
            SessionId = sessionId;
            Id = key;
            Expiration = CacheSettings.GetValidSessionTimeout(expiration);
            SetBody(value, type);

        }
        /// <summary>
        /// Initialize a new instance of session entry using <see cref="MessageStream"/>.
        /// </summary>
        /// <param name="m"></param>
        public SessionEntry(MessageStream m)
            : this()
        {
            SessionId = m.SessionId;
            Id = m.Identifier;
            Expiration = CacheSettings.GetValidSessionTimeout(m.Expiration);
            Label = m.Label;
            SetBody(m.GetStream(), m.TypeName);
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

        public virtual SessionEntry Copy()
        {
            return new SessionEntry()
            {
                BodyStream = GetCopy(),
                Expiration = this.Expiration,
                Label = this.Label,
                Formatter = this.Formatter,
                Id = this.Id,
                SessionId = this.SessionId,
                Modified = this.Modified,
                TypeName = this.TypeName,
                TransformType = this.TransformType
            };
        }
  
        internal object[] ToDataRow(SessionBag bag, bool noBody = false)
        {
            string val = null;

            if (noBody)
            {
                val = "<Body>";
            }
            else if (BodyStream != null)
            {
                val = this.ToJson(true);
                //val = this.BodyToBase64();
            }

            string sessionKey = string.Format("{0}{1}{2}", SessionId, KeySet.Separator, Id);
            if (noBody)
                return new object[] { sessionKey, val, TypeName, bag.State.ToString(), SessionId, bag.Creation, bag.Timeout, Size, bag.LastUsed, bag.UserId };
            else
                return new object[] { sessionKey, val, TypeName, bag.State.ToString(), SessionId, bag.Creation, bag.Timeout, Size, bag.LastUsed, bag.UserId };

        }

    }
}
