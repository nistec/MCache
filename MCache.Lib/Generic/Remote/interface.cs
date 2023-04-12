using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;

using Nistec.Caching;
using Nistec.Data;
using Nistec.Caching.Data;

namespace Nistec.Caching.Remote
{

     

    public interface IRemoteData
    {
        string Reply(string text);

        string[] GetAllKeys();

        int Size{get;}

        DataCacheItem GetItemProperties(string tableName);

        DataCacheStatistic GetStatistic();

        void CacheToXmlConfig(string fileName);
        void CacheToXml(string fileName, XmlWriteMode mode);
        void LoadXmlConfigFile(string file);

        ///// <summary>
        ///// Add Item to SyncTables
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <param name="mappingName"></param>
        ///// <param name="syncType"></param>
        ///// <param name="ts"></param>
        //void AddSyncItem(string tableName, string mappingName, MControl.Caching.SyncType syncType, TimeSpan ts);


        /// <summary>
        /// SyncDataSourceEventHandler
        /// </summary>
        event SyncDataSourceChangedEventHandler SyncDataSourceChanged;

        #region Add remove items

        /// <summary>
		/// Remove data table  from storage
		/// </summary>
		/// <param name="tableName">table name</param>
		void Remove(string tableName);


        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        bool AddDataItem(DataTable dt, string tableName);

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        void AddDataItem(DataTable dt, string tableName, string mappingName, MControl.Caching.SyncType syncType, TimeSpan ts);

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        void AddDataItem(DataTable dt, string tableName, string mappingName,string sourceName, MControl.Caching.SyncType syncType, TimeSpan ts);

        #endregion

        #region Active record


        /// <summary>
        /// Add Remoting Data Item to cache
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="tableName"></param>
        bool AddDataItem(IActiveConfig ac, string tableName);


        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        void AddDataItem(IActiveConfig ac, string tableName, string mappingName, MControl.Caching.SyncType syncType, TimeSpan ts);

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        void AddDataItem(IActiveConfig ac, string tableName, string mappingName,string sourceName, MControl.Caching.SyncType syncType, TimeSpan ts);
        #endregion

        #region get and set values

        /// <summary>
        /// Set Value into specific row and column in local data table  
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="value">value to set</param>
        void SetValue(string tableName, string column, string filterExpression, object value);
    

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>string value</returns>
        string GetValue(string tableName, string column, string filterExpression, string defaultValue);
    
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>DateTime value</returns>
        DateTime GetValue(string tableName, string column, string filterExpression, DateTime defaultValue);
    
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>bool value</returns>
        bool GetValue(string tableName, string column, string filterExpression, bool defaultValue);
     
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>double value</returns>
        double GetValue(string tableName, string column, string filterExpression, double defaultValue);
  
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>decimal value</returns>
        decimal GetValue(string tableName, string column, string filterExpression, decimal defaultValue);
        
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>int value</returns>
        int GetValue(string tableName, string column, string filterExpression, int defaultValue);
  
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>object value</returns>
        object GetValue(string tableName, string column, string filterExpression);
 
        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>Hashtable object</returns>
        System.Collections.IDictionary GetRow(string tableName, string filterExpression);
   
      

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        DataTable GetDataTable(string tableName);
 
        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="index">table index</param>
        /// <returns>DataTable</returns>
        DataTable GetDataTable(int index);
  
 

        #endregion


    }

    //public interface IRemoteTask
    //{
    //    string Reply(string text);
    //    void AddTask(RemoteTaskItem item);
    //    void AddTask(RemoteTaskKey key,string name, string command,string subject, object param);
    //    //object AsyncTask(QueueTaskItem item);
    //}

  

}

    
