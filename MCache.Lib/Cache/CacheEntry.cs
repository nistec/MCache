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
using System.Data;
using System.Xml;
using System.Collections;
using Nistec.Channels;
using Nistec.Generic;
using Nistec.Data.Entities;
using Nistec.Serialization;
using Nistec.IO;
using Nistec.Caching.Session;
using Nistec.Caching.Config;
using Nistec.Data;
using Nistec.Runtime;

namespace Nistec.Caching
{

    /// <summary>
    /// Represent Cache Entry item in cache memory.
    /// </summary>
    [Serializable]
    public sealed class CacheEntry : EntityStream,IDisposable
    {
       

        #region ctor
        /// <summary>
        /// Default constructor
        /// </summary>
        public CacheEntry()
            : base()
        {
        }

        internal CacheEntry(EntityStreamState state)
            : base(state)
        {

        }

        internal CacheEntry(string cacheKey, object value, string sessionId, int expiration, bool isRemote)
            : base()
        {
            Id = cacheKey;
            Expiration = expiration;
            GroupId = sessionId;
            IsRemote = isRemote;
            if (value != null)
            {
                if (isRemote)
                {
                    base.SetBody(value);
                }
                else
                {
                    TypeName = value.GetType().FullName;
                    Value = value;
                    size = CacheUtil.GetItemSize(value);
                }
            }
            else
            {
                base.SetBody(value);
            }
        }

        internal CacheEntry(SessionEntry entity, bool isRemote)
            : base()
        {
            Id = entity.Id;
            GroupId = entity.GroupId;
            Expiration = entity.Expiration;
            TypeName = entity.TypeName;
            Label = entity.Label;
            IsRemote = isRemote;
            if (isRemote)
            {
                SetBody(entity.GetStream(), entity.TypeName);
            }
            else
            {
                Value = GetValue();
                size = entity.Size;
            }
        }

        /// <summary>
        /// Constructor using <see cref="MessageStream"/> message.
        /// </summary>
        /// <param name="m"></param>
        public CacheEntry(MessageStream m)
            : this()
        {
            Id = m.Id;
            Expiration = m.Expiration;
            Label = m.Label;
            GroupId = m.GroupId;
            IsRemote = true;
            SetBody(m.GetStream(), m.TypeName);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Value =null;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Properties

        int size;

        object Value { get; set; }
        /// <summary>
        /// Get indicate whether the item is Remote object.
        /// </summary>
        public bool IsRemote { get; internal set; }

        /// <summary>
        /// Get indicate whether the item is empty 
        /// </summary>
        public override bool IsEmpty
        {
            get
            {
                return Value == null && base.IsEmpty;
            }
        }
        /// <summary>
        /// Get Id Icon
        /// </summary>
        /// <returns></returns>
        public string GetKeyIcon()
        {
            int icon = 0;
            return icon.ToString() + "_" + Id;
        }
        #endregion
               
        

        internal void SetEntry(object value)
        {
            if (value != null)
            {
                if (IsRemote)
                {
                    base.SetBody(value);
                }
                else
                {
                    TypeName = value.GetType().FullName;
                    Value = value;
                    size = CacheUtil.GetItemSize(value);
                }
            }
            else
            {
                base.SetBody(value);
            }
        }

        #region Extended properties

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
        /// <summary>
        /// Get or Set extra arguments
        /// </summary>
        public Dictionary<string,string> Args
        {
            get;
            set;
        }


        internal bool IsMatchArgs(Dictionary<string,string> keyValueArgs)
        {
            if (keyValueArgs == null || Args == null)
                return false;

            int count = keyValueArgs.Count;
            int matches = 0;
            foreach(var entry in keyValueArgs)
            {
                string val;
                if (Args.TryGetValue(entry.Key, out val))
                {
                    if (entry.Value == val)
                    {
                        matches++;
                        continue;
                    }
                    else
                        return false;
                }
                else
                    return false;

            }
            return matches == count;
        }

 
        internal static Dictionary<string, string> ArgsToDictionary(params string[] keyValueArgs)
        {
            if (keyValueArgs == null)
                return null;

            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter is not correct, Not match key value arguments");
            }
            Dictionary<string, string> list = new Dictionary<string, string>();
            for (int i = 0; i < count; i++)
            {
                list[keyValueArgs[i]]= keyValueArgs[++i];
            }

            return list;
        }
        #endregion

        #region Get Values

        /// <summary>
        /// Get value as T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>()
        {
            return GenericTypes.Cast<T>(GetValue(), true);
        }

        /// <summary>
        /// Get value as object
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return this.DecodeBody();
        }
        /// <summary>
        /// Get value as json.
        /// </summary>
        /// <returns></returns>
        public string GetValueJson(bool pretty=false)
        {
            object o = GetValue();
            return JsonSerializer.Serialize(o, pretty);
        }

        /// <summary>
        /// Get value as stream.
        /// </summary>
        /// <returns></returns>
        public NetStream GetValueSream()
        {
            var copy=GetCopy();
            if (copy != null)
            {
                return copy;
            }

            byte[] b = ValueToBytes();
            if (b == null)
                return null;
            return new NetStream(b);
        }

        /// <summary>
        /// Get Value as byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetValueBinary()
        {
            byte[] b = GetBinary();
            if (b == null)
            {
                return ValueToBytes();
            }
            return b;
        }

        byte[] ValueToBytes()
        {
            if (Value == null)
                return null;
            return BinarySerializer.SerializeToBytes(Value,false);

        }
        /// <summary>
        /// Get Value as base64 string.
        /// </summary>
        /// <returns></returns>
        public string GetValueAsBase64String()
        {
            var stream = GetValueSream();
            if (stream == null)
                return null;
            return stream.ToBase64String();
        }

        #endregion

        #region Methods

        internal void SetStatistic()
        {
            Modified = DateTime.Now;
        }
        /// <summary>
        /// Get indicate whether the item is expired.
        /// </summary>
        /// <returns></returns>
        public bool IsExpired()
        {
            if (!AllowExpires)
                return false;
            return DateTime.Now.Subtract(Modified) > TimeOut;
        }
        /// <summary>
        /// Convert item to <see cref="DataRow"/>.
        /// </summary>
        /// <returns></returns>
        public object[] ToDataRow(bool noBody=false)
        {
            string val = null;

            if(noBody)
            {
                val = this.ToJson(true);
                val = "<Body>";
            }
            else if (BodyStream != null)//(IsRemote)
            {
                 val = this.BodyToBase64();
            }
            else
            {
                val = BinarySerializer.SerializeToBase64(Value);
            }

            return new object[] { Id, val, Expiration, Size, TypeName, GroupId, Modified };
        }

        /// <summary>
        /// Get item from <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="isRemote"></param>
        /// <returns></returns>
        public static CacheEntry ItemFromDataRow(DataRow dr, bool isRemote)
        {
            CacheEntry item = new CacheEntry();

            item.Id = dr.Get<string>("Id");//.ToString();
            item.TypeName = Types.NZ(dr["TypeName"], "System.String");

            object val = dr["Value"];

            if (val != null)
            {
                if (isRemote)
                {
                    item.BodyFromBase64(val.ToString());
                }
                else
                {
                    val = BinarySerializer.DeserializeFromBase64(val.ToString());
                }
            }
            else
            {
                item.Value = val;
            }
            item.Expiration = Types.NZ(dr["Expiration"], CacheDefaults.DefaultCacheExpiration);
            item.GroupId = Types.NZ(dr["SessionId"], null);
            item.Modified = Types.ToDateTime(dr["Modified"], DateTime.Now);
            return item;
        }

        /// <summary>
        /// Cache Item Schema as <see cref="DataTable"/> class.
        /// </summary>
        /// <returns></returns>
        public static DataTable CacheItemSchema()
        {
            DataTable dt = new DataTable("CacheEntry");
            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("Body", typeof(string));
            dt.Columns.Add("Expiration", typeof(int));
            dt.Columns.Add("Size", typeof(int));
            dt.Columns.Add("TypeName", typeof(string));
            dt.Columns.Add("SessionId", typeof(string));
            dt.Columns.Add("Modified", typeof(string));
            return dt.Clone();
        }

       /// <summary>
        /// Print Header
       /// </summary>
       /// <returns></returns>
        public string PrintHeader()
        {
            return string.Format("<CacheEntry Id='{0}' Type='{1}' Size='{2}' IsTimeOut={3} />",
                Id, TypeName, Size, IsTimeOut);

        }
        /// <summary>
        /// Print Details
        /// </summary>
        /// <returns></returns>
        public string PrintDetails()
        {
            return string.Format("<CacheEntry Id='{0}' Size='{1}' TimeOut='{2}' AllowExpires='{3}' ItemType='{4}' Modified='{5}' />",
                Id, Size, TimeOut, AllowExpires, TypeName, Modified);
        }
        /// <summary>
        /// Print Tool Tip
        /// </summary>
        /// <returns></returns>
        public string PrintToolTip()
        {
            return string.Format("<CacheEntry Id='{0}' TypeName='{1}' Size='{2}' />", Id, TypeName, Size);
        }
        
        #endregion

        #region Load copy and mereg
        /// <summary>
        /// Get item copy without value
        /// </summary>
        /// <returns></returns>
        public CacheEntry Clone()
        {
            return Copy(false);
        }

        /// <summary>
        /// Get item copy with argument.
        /// </summary>
        /// <param name="valueAswell"></param>
        /// <returns></returns>
        public new CacheEntry Copy(bool valueAswell)
        {
            CacheEntry item = new CacheEntry();
            item.GroupId = GroupId;
            item.Modified = Modified;
            
            item.Id = Id;
            item.Value = valueAswell ? Value : "<Copy>";
            item.BodyStream = valueAswell ? GetCopy(): null;
            item.TypeName = TypeName;
            item.IsRemote = item.IsRemote;
            return item;
        }
        /// <summary>
        /// Load Item
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="isRemote"></param>
        internal void LoadItem(CacheObjType objType, Type type, string source, bool isRemote)
        {
            object val = null;
            byte[] bin = null;

             this.IsRemote = isRemote;


            switch (objType)
            {
                case CacheObjType.ImageFile:
                case CacheObjType.BinaryFile:
                case CacheObjType.TextFile:
                case CacheObjType.HtmlFile:
                    if (isRemote)
                    {
                        bin = CacheUtil.LoadFileSourceToBytes(objType, source);
                        if (type == null)
                        {
                            if (objType == Caching.CacheObjType.ImageFile)
                                type = typeof(System.Drawing.Image);
                            else if (objType == Caching.CacheObjType.BinaryFile)
                                type = typeof(object);
                            else if (objType == Caching.CacheObjType.TextFile)
                                type = typeof(string);
                            else if (objType == Caching.CacheObjType.HtmlFile)
                                type = typeof(string);
                        }
                    }
                    else
                    {
                        val = CacheUtil.LoadFileSource(objType, source);
                        if (val != null)
                        {
                            bin = BinarySerializer.SerializeToBytes(val);
                            type = val.GetType();
                        }
                    }
                    break;
                case CacheObjType.XmlDocument:
                    if (isRemote)
                    {
                        bin = CacheUtil.LoadFileSourceToBytes(objType, source);
                   }
                   else
                    {
                        val = CacheUtil.LoadFileSource(objType, source);
                        if (val != null)
                        {
                            bin = BinarySerializer.SerializeXmlToBytes(val);
                            type = val.GetType();
                        }
                    }
                    if (type == null)
                        type = typeof(XmlDocument);
                    break;
                case CacheObjType.RemotingData:
                case CacheObjType.SerializeClass:
                case CacheObjType.Default:
                default:
                    
                    break;
            }

            if (type == null)
                type = typeof(object);
            this.TypeName = type.ToString();

            if (isRemote)
            {
                this.BodyStream = new IO.NetStream(bin);
            }
            else
            {
                this.Value = val;
            }
        }

        internal CacheEntry CopyTo(string cacheKey, int expiration)
        {
            CacheEntry item = new CacheEntry();
            item.Label = Label;
            item.GroupId = GroupId;
            item.Modified = DateTime.Now;
            item.Id = cacheKey;
            item.Value = Value;
            //if (BodyStream != null)
            //{
            //    var stream = GetCopy();
            //    item.BodyStream = stream;
            //}
            item.BodyStream = GetCopy();
            item.TypeName = TypeName;
            item.IsRemote = IsRemote;
            return item;
        }

        internal CacheEntry Merge(string cacheKey, object value)
        {
            if (value==null)
            {
                throw new ArgumentNullException("CacheEntry.Merge value");
            }

            int count = 0;
            int size = Size;
            Type type = SerializeTools.GetQualifiedType(TypeName);
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                IDictionary src = (IDictionary)GetValue();
                IDictionary dest = (IDictionary)ActivatorUtil.CreateInstance(type);
                if (dest == null || src == null)
                    return this;

                IEnumerator en = dest.Keys.GetEnumerator();
                while (en.MoveNext())
                {
                    src[en.Current] = dest[en.Current];
                }
                count = src.Count;

                SetEntry(dest);
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                IList src = (IList)GetValue();
                IList dest = (IList)ActivatorUtil.CreateInstance(type);
                if (dest == null || src == null)
                    return this;

                IEnumerator en = dest.GetEnumerator();
                while (en.MoveNext())
                {
                    src.Add(en.Current);
                }
                count = src.Count;
                SetEntry(dest);
            }
            else if (typeof(DataTable) ==type)
            {
                DataTable src = (DataTable)GetValue();
                DataTable dest = (DataTable)value;
                if (dest == null || src == null)
                    return this;

                src.Merge(dest);
                count = src.Rows.Count;
                SetEntry(dest);
            }
             else
            {
                throw new NotSupportedException("item type not supported");
            }

            Modified = DateTime.Now;

            

            return this;
        }

        internal CacheEntry MergeRemove(string cacheKey, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("CacheEntry.MergeRemove value");
            }

            int count = 0;
            int size = Size;
            Type type = SerializeTools.GetQualifiedType(TypeName);
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                IDictionary src = (IDictionary)GetValue();
                IDictionary dest = (IDictionary)ActivatorUtil.CreateInstance(type);
                if (dest == null || src == null)
                    return this;

                IEnumerator en = dest.Keys.GetEnumerator();
                while (en.MoveNext())
                {
                    src.Remove(en.Current);
                }
                count = src.Count;
                SetEntry(dest);
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                IList src = (IList)GetValue();
                IList dest = (IList)ActivatorUtil.CreateInstance(type);
                if (dest == null || src == null)
                    return this;

                IEnumerator en = dest.GetEnumerator();
                while (en.MoveNext())
                {
                    src.Remove(en.Current);
                }
                count = src.Count;
                SetEntry(dest);
            }
            else if (typeof(DataTable) == type)
            {
                DataTable src = (DataTable)GetValue();
                DataTable dest = (DataTable)value;
                if (dest == null || src == null)
                    return this;
                
                foreach (DataRow dr in dest.Rows)
                {
                    src.Rows.Remove(dr);
                }

                count = src.Rows.Count;
                size = CacheUtil.GetItemSize(src);
                SetEntry(dest);
            }
            else
            {
                throw new NotSupportedException("item type not supported");
            }

            Modified = DateTime.Now;

            return this;
        }

        #endregion

    }

}
