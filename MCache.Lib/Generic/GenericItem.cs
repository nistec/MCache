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
using Nistec.Caching.Remote;
using System.Data;
using Nistec.Serialization;

namespace Nistec.Caching
{

    [Serializable]
    public class GenericItem 
    {
        public object Value { get; set; }
        int _size;
        //Type _type;

        public Type ItemType
        {
            get { return Value.GetType(); }
        }
        public int Size
        {
            get { return _size; }
        }

        public bool IsEmpty
        {
            get { return Value == null; }
        }

        public T Get<T>()
        {
            return GenericTypes.ConvertObject<T>(Value);
            //return MControl.Runtime.Serialization.DeserializeFromBase64<T>(SerializedValue);
        }

        // public object Value()
        //{
        //    return MControl.Runtime.Serialization.DeserializeFromBase64(SerializedValue);
        //}

        //public GenericItem(string serializedValue, Type type, int size)
        //{
        //    SerializedValue = serializedValue;
        //    _size = size;
        //}
        public GenericItem()
        {
        }
        public GenericItem(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("GenericItem.value");
            }
            //_type = value.GetType();
            _size = CacheUtil.GetItemSize(value);
            Value = value;
            //SerializedValue = MControl.Runtime.Serialization.SerializeToBase64(value);
        }

        //internal static string Create(object value)
        //{
        //    return new GenericItem(value).Serialize();
        //}

        public string Serialize()
        {
            return BinarySerializer.SerializeToBase64(this);
        }
         
        //public static string Serialize(object value)
        //{
        //    return MControl.Runtime.Serialization.SerializeToBase64(value);
        //}
        public static T Deserialize<T>(string base64)
        {
            return BinarySerializer.DeserializeFromBase64<T>(base64);
        }
        public static object Deserialize(string base64)
        {
            return BinarySerializer.DeserializeFromBase64(base64);
        }
    }

    #region Generic Item
    /*
    [Serializable]
    public class GenericItem<T>
    {
        //public string Name { get; set; }
        public T Value { get; set; }
        int _size;
        
        public int Size
        {
            get { return _size; }
        }

        public bool IsEmpty
        {
            get { return Value == null; }
        }

        public GenericItem(T value)
        {
            Value = value;
            _size = CacheUtil.GetItemSize(value);
        }

        public Type ItemType
        {
            get { return Value == null ? typeof(object) : typeof(T); }
        }
       
        public string Serialize()
        {
            return MControl.Runtime.Serialization.SerializeToBase64(this);
        }

        public static T Deserialize(string base64)
        {
            return MControl.Runtime.Serialization.DeserializeFromBase64<T>(base64);
        }
             */
    
    #endregion

  
}