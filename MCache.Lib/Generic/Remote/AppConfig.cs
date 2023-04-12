using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Collections.Specialized;

namespace Nistec.Caching.Remote
{
  
    /// <summary>
    /// ActiveConfig base on HybridDictionary ,
    /// </summary>
    public class AppConfig:IDisposable
    {
        #region memebers and ctor

        private HybridDictionary hash;

        /// <summary>
        /// ActiveConfig ctor
        /// </summary>
        public AppConfig()
        {
            InitDictionary();
        }

        ~AppConfig()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            if (hash != null)
            {
                hash.Clear();
                hash = null;
            }
        }

        /// <summary>
        /// IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            get { return hash == null || hash.Count == 0; }
        }
 
        ///// <summary>
        ///// Get Copy of Data table source
        ///// </summary>
        //public Hashtable Copy
        //{
        //    get { return hash.Clone(); }
        //}
 
        #endregion

        #region hash

        private void InitDictionary()//string keyName, string valueName)
        {
            //if (!string.IsNullOrEmpty(keyName))
            //    this.keyName = keyName;
            //if (!string.IsNullOrEmpty(valueName))
            //    this.valueName = valueName;

            if (hash != null)
            {
                hash.Clear();
                hash = null;
            }
            hash = new HybridDictionary();
        }


        /// <summary>
        /// Add new item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(object key, object value)
        {
            try
            {
                hash.Add(key, value);
            }
            catch(Exception exception)
            {
                throw exception;
            }
        }
        /// <summary>
        /// Get Contains
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(object key)
        {
            return hash.Contains(key);
        }

        /// <summary>
        /// Copy To Array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(System.Array array,int index)
        {
            hash.CopyTo(array,index);
        }
        /// <summary>
        /// Get Count
        /// </summary>
        public int Count
        {
            get { return hash.Count; }
        }
        /// <summary>
        /// Get Keys
        /// </summary>
        public ICollection Keys
        {
            get { return hash.Keys; }
        }
        /// <summary>
        /// Get Values
        /// </summary>
        public ICollection Values
        {
            get { return hash.Values; }
        }
        /// <summary>
        /// Get SyncRoot
        /// </summary>
        public object SyncRoot
        {
            get { return hash.SyncRoot; }
        }
        /// <summary>
        /// Get IsSynchronized
        /// </summary>
        public bool IsSynchronized
        {
            get { return hash.IsSynchronized; }
        }
        /// <summary>
        /// Get or Set ActiveConfig
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[object key]
        {
            get { return hash[key]; }
            set 
            {
                hash[key] = value;
            }
        }
  
        #endregion

        #region Values

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue(string key)
        {
            return hash[key];
        }


        /// <summary>
        /// GetValue int
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <returns>int,if null or error return 0<</returns>
        public int GetIntValue(string key)
        {
            return (int)GetValue(key, (int)0);
        }
        /// <summary>
        /// GetValue decimal
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <returns>decimal ,if null or error return 0</returns>
        public decimal GetDecimalValue(string key)
        {
            return (decimal)GetValue(key, (decimal)0);
        }
        /// <summary>
        /// GetValue double
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <returns>double ,if null or error return 0<</returns>
        public double GetDoubleValue(string key)
        {
            return (double)GetValue(key, (double)0);
        }
        /// <summary>
        /// GetValue bool
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <returns>bool,if null or error return false<</returns>
        public bool GetBoolValue(string key)
        {
            return (bool)GetValue(key, (bool)false);
        }
        /// <summary>
        /// GetValue string
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <returns>string,if null or error return ""<</returns>
        public string GetStringValue(string key)
        {
            return (string)GetValue(key, (string)"");
        }
        /// <summary>
        /// GetValue DateTime
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <returns>DateTime ,if null or error return Now<</returns>
        public DateTime GetDateValue(string key)
        {
            return (DateTime)GetValue(key, DateTime.Now);
        }


        /// <summary>
        /// GetValue int
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>int,if null or error return defaultValue<</returns>
        public int GetValue(string key, int defaultValue)
        {
            return (int)Types.NZ(GetValue(key), (int)defaultValue);
        }
        /// <summary>
        /// GetValue decimal
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>decimal,if null or error return defaultValue</returns>
        public decimal GetValue(string key, decimal defaultValue)
        {
            return (decimal)Types.NZ(GetValue(key), (decimal)defaultValue);
        }
        /// <summary>
        /// GetValue double
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>double,if null or error return defaultValue</returns>
        public double GetValue(string key, double defaultValue)
        {
            return (double)Types.NZ(GetValue(key), (double)defaultValue);
        }
        /// <summary>
        /// GetValue bool
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>bool,if null or error return defaultValue</returns>
        public bool GetValue(string key, bool defaultValue)
        {
            return (bool)Types.NZ(GetValue(key), (bool)defaultValue);
        }
        /// <summary>
        /// GetValue string
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>string,if null or error return defaultValue</returns>
        public string GetValue(string key, string defaultValue)
        {
            return (string)Types.NZ(GetValue(key), (string)defaultValue);
        }
        /// <summary>
        /// GetValue DateTime
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>DateTime,if null or error return defaultValue</returns>
        public DateTime GetValue(string key, DateTime defaultValue)
        {
            return (DateTime)Types.NZ(GetValue(key), defaultValue);
        }

        #endregion

    }
}
