using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;

namespace Nistec.Legacy
{

    /// <summary>
    /// Summary description for IActiveRecord.
    /// </summary>
    public interface IActiveRecord : IActiveValue
    {

        object this[int Index] { get; set; }

        bool Initilaized { get; }

        void CancelEdit();

        void BeginEdit();

        void ClearErrors();

        void Delete();

        void EndEdit();

        void AcceptChanges();

        void RejectChanges();

        bool HasErrors { get; }

        DataRowState RowState { get; }

        string RowError { get; set; }

        DataTable DataSource { get; set; }

        int Update();
        //int UpdateChanges(IDbConnection cnn);
        //int UpdateChanges(string connectionString, DBProvider provider);
    }

    /// <summary>
    /// Summary description for IActiveValue.
    /// </summary>
    public interface IActiveValue
    {
        bool IsEmpty { get; }

        object this[string field] { get; set; }

        object[] ItemArray { get; set; }

        int Count { get; }

        int Position { get; set; }

        void Refresh();

        //string Serilaize();

        //void Deserilaize(string base64);

        //void ClearChanges();

        //Dictionary<string, object> GetFieldsChanged();

        //bool IsDirty { get;}


        #region Values

        /// <summary>
        /// GetValue int
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>int,if null or error return defaultValue<</returns>
        int GetValue(string field, int defaultValue);
        /// <summary>
        /// GetValue decimal
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>decimal,if null or error return defaultValue</returns>
        decimal GetValue(string field, decimal defaultValue);
        /// <summary>
        /// GetValue double
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>double,if null or error return defaultValue</returns>
        double GetValue(string field, double defaultValue);
        /// <summary>
        /// GetValue bool
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>bool,if null or error return defaultValue</returns>
        bool GetValue(string field, bool defaultValue);
        /// <summary>
        /// GetValue string
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>string,if null or error return defaultValue</returns>
        string GetValue(string field, string defaultValue);
        /// <summary>
        /// GetValue DateTime
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>DateTime,if null or error return defaultValue</returns>
        DateTime GetValue(string field, DateTime defaultValue);

        #endregion

    }

    public interface IActiveConfig : IDisposable
    {


        /// <summary>
        /// Get Contains
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Contains(object key);
        /// <summary>
        /// Copy To Array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        void CopyTo(System.Array array, int index);
        /// <summary>
        /// Get Count
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Get or Set ActiveConfig
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section">the section name in data row</param>
        /// <returns></returns>
        object this[object key, string section] { get; }
        /// <summary>
        /// Get or Set ActiveConfig
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[object key] { get; }

        void Refresh();

        DataTable DataSource { get; set; }

        //DataTable Table { get; set;}

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <returns>object</returns>
        void SetValue(object key, object value, string section);

        #region Values

        /// <summary>
        /// GetValue int
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>int,if null or error return defaultValue<</returns>
        int GetValue(string key, string section, int defaultValue);

        /// <summary>
        /// GetValue decimal
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>decimal,if null or error return defaultValue</returns>
        decimal GetValue(string key, string section, decimal defaultValue);

        /// <summary>
        /// GetValue double
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>double,if null or error return defaultValue</returns>
        double GetValue(string key, string section, double defaultValue);

        /// <summary>
        /// GetValue bool
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>bool,if null or error return defaultValue</returns>
        bool GetValue(string key, string section, bool defaultValue);

        /// <summary>
        /// GetValue string
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>string,if null or error return defaultValue</returns>
        string GetValue(string key, string section, string defaultValue);

        /// <summary>
        /// GetValue DateTime
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>DateTime,if null or error return defaultValue</returns>
        DateTime GetValue(string key, string section, DateTime defaultValue);
        #endregion
    }


    public interface IRemoteSession
    {
        string Reply(string text);
        void AddSession(string sessionId, string userId, int timeout, string args);
        void RemoveSession(string sessionId);
        void Refresh(string sessionId);
        void Log(string message);
        SessionItem GetExistingSession(string sessionId);
        SessionItem GetSession(string sessionId);
        void ClearSessionItems(string sessionId);

        //string[] GetAllKeys();

        //CacheSession
        /*
        string[] GetCacheSessionKeys(string sessionId);
        void RemoveCacheSessionAsync(string sessionId);
        CacheItem[] CloneCacheSessionItems(string sessionId);
        int CacheSessionTimeout { get; }
        int RemoveCacheSessionItems(string sessionId, bool isAsync);
        */

        //void KeepAliveSession(string sessionId, bool isAsync);

        ICollection<string> GetAllSessionsKeys();
        ICollection<string> GetAllSessionsStateKeys(SessionState state);
        ICollection<SessionItem> GetAllSessions();
        ICollection<SessionItem> GetActiveSessions();
        ICollection<string> GetSessionsItemsKeys(string sessionId);

        //object this[string sessionId, string key] { get; set; }

        int AddItem(string sessionId, string key, object value, bool validateExisting = false);
        bool RemoveItem(string sessionId, string key);
        object GetItem(string sessionId, string key);
        object FetchItem(string sessionId, string key);

        int CopyTo(string sessionId, string key, string targetKey, int expiration, bool addToCache = false);
        int FetchTo(string sessionId, string key, string targetKey, int expiration, bool addToCache = false);
        bool Exists(string sessionId, string key);

    }

    public interface IRemoteCache
    {
        string Reply(string text);

        Hashtable CacheProperties();

        int Timeout { get; set; }
        object this[string key] { get; set; }
        CacheItem GetItem(string key);
        CacheItem FetchItem(string key);
        CacheItem ViewItem(string key, bool isClone);

        //object GetItemValue(string key);
        string[] GetAllKeys();
        string[] GetAllKeysIcons();
        CacheItem[] CloneItems(CloneType type);

        object LoadItem(string key, object defaultValue);
        object LoadItem(string source, CacheObjType type);//, Type itemType);
        object LoadItem(string key, string source, CacheObjType type);//, Type itemType);
        object LoadItem(string key, string source, CacheObjType type, /*Type itemType,*/ bool allowExpires);

        object LoadImage(string source);
        object LoadTextFile(string source);
        object LoadXmlDocument(string source);

        //int AddItem(string key, object value, CacheObjType cacheObjType, string sessionId, /*string type,*/ /*int size,*/ int expiration);
        int AddItem(string base64);
        int AddItem(byte[] bytes);
        int CopyItem(string source, string dest, int expiration);
        int CutItem(string source, string dest, int expiration);
        int MergeItem(string key, string base64);
        int MergeRemoveItem(string key, string base64);

        bool RemoveItem(string key);
        void RemoveItemAsync(string key);
        //void RemoveSessionAsync(string sessionId);
        int RemoveCacheSessionItems(string sessionId, bool isAsync);
        int SessionTimeout { get; }

        bool KeepAliveItem(string key);
        void KeepAliveItemAsync(string key);

        CacheStatistic GetStatistic();

        //void PrintCache();
        string CacheToXml();
        void CacheToXml(string fileName);
        void XmlToCache(string filename);

        string CacheLog();
        void Log(string message);
        void Reset();

    }

    public interface IRemoteData
    {
        string Reply(string text);

        string[] GetAllKeys();

        int Size { get; }

        DataCacheItem GetItemProperties(string tableName);

        DataCacheStatistic GetStatistic();

        void CacheToXmlConfig(string fileName);
        void CacheToXml(string fileName, XmlWriteMode mode);
        void LoadXmlConfigFile(string file);

        /// <summary>
        /// SyncDataSourceEventHandler
        /// </summary>
        event Caching.Data.SyncDataSourceChangedEventHandler SyncDataSourceChanged;

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
        void AddDataItem(DataTable dt, string tableName, string mappingName, Caching.SyncType syncType, TimeSpan ts);

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        void AddDataItem(DataTable dt, string tableName, string mappingName, string sourceName, Caching.SyncType syncType, TimeSpan ts);

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
        void AddDataItem(IActiveConfig ac, string tableName, string mappingName, Nistec.Caching.SyncType syncType, TimeSpan ts);

        /// <summary>
        /// Add Remoting Data Item to cache include SyncTables
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="minute"></param>
        void AddDataItem(IActiveConfig ac, string tableName, string mappingName, string sourceName, Nistec.Caching.SyncType syncType, TimeSpan ts);
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

}
