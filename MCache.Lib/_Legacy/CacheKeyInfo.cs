using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Runtime.Serialization;

namespace Nistec.Legacy
{

    /// <summary>
    /// Represent the item key info for each item in cache
    /// </summary>
    [Serializable]
    public class CacheKeyInfo 
    {

        public static CacheKeyInfo Get(string name, string[] keys)
        {
            return new CacheKeyInfo() { ItemName = name, ItemKeys = keys };
        }
                      

        #region properties

        public string ItemName
        {
            get;
            set;
        }
       
        public string[] ItemKeys
        {
            get;
            set;
        }

        public string CacheKey
        {
            get { return string.Join("_", ItemKeys); }
        }

               
        #endregion
    }
}
