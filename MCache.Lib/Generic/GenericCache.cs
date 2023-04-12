using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;

namespace Nistec.Caching
{

     /// <summary>
    /// GenericCache
    /// </summary>
    [Serializable]
    public abstract class GenericCache<K, T> : Dictionary<K, T>
    {

        public const int DefaultInitialCapacity = 100;
        public const float DefaultLoadFactor = 0.5F;

        //Hashtable m_cache;
        int m_options;
        string m_cacheName;

        List<string> m_keys;
        /// <summary>
        /// Get Keys items
        /// </summary>
        public List<string> DataKeys
        {
            get 
            { 
                if (m_keys == null)
                {
                    m_keys = new List<string>();
                }
                return m_keys; 
            }
        }

        public GenericCache()
            : this("", 1)
        {
        }
        public GenericCache(int options)
            : this("", options)
        {
        }
        public GenericCache(string cacheName, int options)
        {
            this.m_cacheName = cacheName;
            this.m_options = options;
            //this.m_cache = Hashtable.Synchronized(new Hashtable(DefaultInitialCapacity, DefaultLoadFactor));
        }

        /// <summary>
        /// InitCache 
        /// </summary>
        protected abstract void InitCache();

        //public abstract string CreateKey(params string[] item);

        ///// <summary>
        ///// CreateKey from string
        ///// </summary>
        ///// <param name="dr"></param>
        ///// <returns></returns>
        //public abstract K CreateKey(object key);

        /// <summary>
        /// GetKey with number of options
        /// </summary>
        /// <param name="option"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual K GetKey(int option, params string[] item)
        {
            return default(K);
        }

        /// <summary>
        /// Get Key default
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual K GetKey(params string[] item)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < item.Length; i++)
            {
                sb.Append(string.Format("{0}_", item[i]));
            }

            return CreateKey(sb.ToString().TrimEnd('_')); 
        }

        /// <summary>
        /// Reset cache
        /// </summary>
        public void Reset()
        {
            this.Clear();
        }

        /// <summary>
        /// Refresh cache
        /// </summary>
        public void Refresh()
        {
            this.Clear();
            InitCache();
        }

        protected virtual K CreateKey(object key)
        {
            return (K)key;
        }

        protected K CreateDataKey(DataRow dr)
        {
            int count= DataKeys.Count;
            if(count<=0)
                return CreateKey(dr);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append( string.Format( "{0}_", dr[DataKeys[i]]));
            }

            return CreateKey(sb.ToString().TrimEnd('_')); 
        }
      
        /// <summary>
        /// CreateCacheItem 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual T CreateCacheItem(object item)
        {
            return (T)item;
        }

        /// <summary>
        /// CreateCache from data table
        /// </summary>
        /// <param name="dt"></param>
        protected virtual void CreateCache(DataTable dt)
        {
            Reset();

            if (dt == null || dt.Rows.Count == 0)
                return;
            if (string.IsNullOrEmpty(m_cacheName))
                m_cacheName = dt.TableName;
            int count = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                K key = CreateDataKey(dr);
                this[key] = CreateCacheItem(dr);
            }
        }

        /// <summary>
        /// Create cache from IDictionary
        /// </summary>
        /// <param name="d"></param>
        protected virtual void CreateCache(IDictionary d)
        {
            Reset();

            if (d == null || d.Count == 0)
                return;

            int count = d.Count;
            foreach (DictionaryEntry entry in d)
            {
                this[(K)entry.Key] = (T)entry.Value;
            }
        }

 

        /// <summary>
        /// Get Item by key with number of options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public T GetItem(params string[] key)
        {
            if (key == null || key.Length == 0)
            {
                throw new ArgumentException("null Key item in Cache: " + m_cacheName);
            }
            if (Count <= 0)
            {
                InitCache();
            }
            K ky = default(K);

            try
            {
                if (DataKeys.Count <= 1)
                {
                    ky = CreateKey(key[0]);
                    return this[ky];
                }
                else if (m_options <= 1)
                {
                    ky = GetKey(key);
                    return this[ky];
                }
                else
                {
                    for (int i = 0; i < m_options; i++)
                    {
                        ky = GetKey(i, key);
                        if (ky!=null && this.ContainsKey(ky))
                            return this[ky];
                    }
                }

                throw new ArgumentException("Invalid item in Cache: " + m_cacheName + " for key: " + ky.ToString());
                //return null;
            }
            catch (ArgumentException mex)
            {
                throw mex;
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid item in Cache: " + m_cacheName + " for key: " + ky.ToString());
            }

        }


        /// <summary>
        /// Get Item by key with number of options
        /// </summary>
        /// <param name="key"></param>
        /// <returns>return null or empty if not exists</returns>
        public T FindItem(params string[] key)
        {
            if (key == null || key.Length == 0)
            {
                return default(T);
            }
            if (Count <= 0)
            {
                InitCache();
            }
            K ky = default(K);

            if (DataKeys.Count <= 1)
            {
                ky = CreateKey(key[0]);
                if (ky!=null && this.ContainsKey(ky))
                    return this[ky];
            }
            else if (m_options <= 1)
            {
                ky = GetKey(key);
                return this[ky];
            }
            else
            {
                for (int i = 0; i < m_options; i++)
                {
                    ky = GetKey(i, key);
                    if (ky!=null && this.ContainsKey(ky))
                        return this[ky];

                }
            }
            return default(T);
        }

        
    }
}
