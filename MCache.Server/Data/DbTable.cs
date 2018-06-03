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
using System.Data;
using System.Collections;
using System.Threading;
using Nistec.Data;
using System.Collections.Generic;
using System.Linq;
using Nistec.Caching;
using Nistec.Xml;
using System.Xml;
using Nistec.Data.Factory;
using Nistec.Runtime;
using Nistec.Data.Entities;
using Nistec.Caching.Sync;
using System.Threading.Tasks;
using Nistec.Caching.Config;
using System.Collections.Concurrent;
using Nistec.Generic;
using Nistec.Serialization;
using System.IO;
using System.Diagnostics;
using Nistec.Data.Ado;

namespace Nistec.Caching.Data
{

   

    /// <summary>
    ///  Represent an synchronized Data set of tables as a data cache for specific database.
    ///The <see cref="DataSynchronizer"/> "Synchronizer" manages the synchronization for each item
    ///in  <see cref="DataSyncList"/> items.
    ///Thru <see cref="IDbContext"/> connector.
    /// </summary>
    public class DbTable : IDisposable,ISerialEntity
    {

        public static DbTable Creat(DataTable dt,string mappingName, EntitySourceType sourceType, string [] primaryKey)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("Creat.dt");
            }
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("Creat.mappingName");
            }
            if (primaryKey == null || primaryKey.Length == 0)
            {
                throw new ArgumentException("Invalid Primary key for data table");
            }

            if (string.IsNullOrEmpty(dt.TableName))
                dt.TableName = mappingName;

            DbTable table = new DbTable();
            table.BeginEdit();
            int size = 0;
            foreach(DataRow row in dt.Rows)
            {
                var entity=new GenericEntity(row, primaryKey,true);
                string key = entity.PrimaryKey.ToString();
                table[key] = entity;
                size += entity.Size();
            }
            table.Name = dt.TableName;
            table.MappingName = mappingName;
            table.PrimaryKey = primaryKey;
            table.SourceType = sourceType;
            table._size = size;
            table.EndEdit();
            return table;
        }

        public static DbTable CreatWithKey(DataTable dt, string mappingName, EntitySourceType sourceType)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("CreatWithKey.dt");
            }
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("Creat.mappingName");
            }

            if (dt.PrimaryKey == null || dt.PrimaryKey.Length == 0)
            {
                throw new ArgumentException("Invalid Primary key in data table");
            }
            if (string.IsNullOrEmpty(dt.TableName))
                dt.TableName = mappingName;
            DbTable table = new DbTable();
            table.BeginEdit();
            int size = 0;
            foreach (DataRow row in dt.Rows)
            {
                var entity = new GenericEntity(row);
                string key = entity.PrimaryKey.ToString();
                table[key] = entity;
                size += entity.Size();
            }
            table.Name = dt.TableName;
            table.MappingName = mappingName;
            table.PrimaryKey = EntityKeys.ColumnsToArray(dt.PrimaryKey);
            table.SourceType = sourceType;
            table._size = size;
            table.EndEdit();
            return table;
        }

        public static DbTable Load(DbTable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Load.source");
            }
            if (string.IsNullOrEmpty(source.MappingName))
            {
                throw new ArgumentException("Invalid MappingName");
            }
            if (source.Owner == null)
            {
                throw new ArgumentException("Invalid DbSet owner");
            }
            if (string.IsNullOrEmpty(source.Owner.ConnectionKey))
            {
                throw new ArgumentException("Invalid DbSet owner ConnectionKey");
            }

            DataTable dt = null;

            using (var cmd = DbContext.Create(source.Owner.ConnectionKey, 0, true))
            {
                dt = cmd.QueryDataTable(source.MappingName, null);
            }

            if (dt == null)
            {
                throw new ArgumentNullException("Reload failed, could not load table " + source.MappingName);
            }

            if (dt.PrimaryKey == null || dt.PrimaryKey.Length == 0)
            {
                throw new ArgumentException("Invalid Primary key in data table");
            }

            DbTable table = CreatWithKey(dt, source.MappingName, source.SourceType);
            table.Owner = source.Owner;
            return table;
        }

        //public static DbTable LoadAsync(string ConnectionKey, string MappingName, EntitySourceType sourceType, int timeout, params object[] keyValueParameters)
        //{
        //    Task<DbTable> t = new Task<DbTable>(() =>
        //    {
        //        return Load(ConnectionKey, MappingName, sourceType, timeout, keyValueParameters);

        //    });
        //    {
        //        t.Start();
        //        t.Wait(timeout);
        //        if (t.IsCompleted)
        //        {
        //            return t.Result;
        //        }
        //    }
        //    t.TryDispose();
        //    return null;
        //}
        //public static DbTable Load(string ConnectionKey, string MappingName, EntitySourceType sourceType, int timeout,params object[] keyValueParameters)
        //{
        //    //if (source == null)
        //    //{
        //    //    throw new ArgumentNullException("Load.source");
        //    //}
        //    //if (source.Owner == null)
        //    //{
        //    //    throw new ArgumentException("Invalid DbSet owner");
        //    //}
        //    if (string.IsNullOrEmpty(MappingName))
        //    {
        //        throw new ArgumentException("Invalid MappingName");
        //    }
        //    if (string.IsNullOrEmpty(ConnectionKey))
        //    {
        //        throw new ArgumentException("Invalid DbSet owner ConnectionKey");
        //    }

        //    DataTable dt = null;

        //    using (var cmd = DbContext.Create(ConnectionKey, timeout, true))
        //    {
        //        dt = cmd.ExecuteCommand<DataTable>(MappingName, sourceType== EntitySourceType.Procedure ? CommandType.StoredProcedure: CommandType.Text, keyValueParameters);
        //    }

        //    if (dt == null)
        //    {
        //        throw new ArgumentNullException("Reload failed, could not load table " + MappingName);
        //    }

        //    if (dt.PrimaryKey == null || dt.PrimaryKey.Length == 0)
        //    {
        //        throw new ArgumentException("Invalid Primary key in data table");
        //    }

        //    DbTable table = CreatWithKey(dt,MappingName,sourceType);
        //    //table.Owner = source.Owner;
        //    return table;
        //}

        //public static DbTable LoadAsync(string ConnectionKey, string MappingName, EntitySourceType sourceType, int timeout, string[] primaryKey, params object[] keyValueParameters)
        //{
        //    Task<DbTable> t = new Task<DbTable>(() =>
        //    {
        //        return Load(ConnectionKey, MappingName, sourceType, timeout, primaryKey, keyValueParameters);

        //    });
        //    {
        //        t.Start();
        //        t.Wait(timeout);
        //        if (t.IsCompleted)
        //        {
        //            return t.Result;
        //        }
        //    }
        //    t.TryDispose();
        //    return null;
        //}

        //public static DbTable Load(string ConnectionKey, string MappingName, EntitySourceType sourceType, int timeout, string[] primaryKey, params object[] keyValueParameters)
        //{

        //    if (string.IsNullOrEmpty(MappingName))
        //    {
        //        throw new ArgumentException("Invalid MappingName");
        //    }
        //    if (string.IsNullOrEmpty(ConnectionKey))
        //    {
        //        throw new ArgumentException("Invalid DbSet owner ConnectionKey");
        //    }

        //    DataTable dt = null;

        //    using (var cmd = DbContext.Create(ConnectionKey, timeout, true))
        //    {
        //        dt = cmd.ExecuteCommand<DataTable>(MappingName, sourceType== EntitySourceType.Procedure ? CommandType.StoredProcedure : CommandType.Text, keyValueParameters);
        //    }

        //    if (dt == null)
        //    {
        //        throw new ArgumentNullException("Reload failed, could not load table " + MappingName);
        //    }

        //    if (dt.PrimaryKey == null || dt.PrimaryKey.Length == 0)
        //    {
        //        throw new ArgumentException("Invalid Primary key in data table");
        //    }

        //    DbTable table = Creat(dt, MappingName, sourceType, primaryKey);

        //    //table.Owner = source.Owner;
        //    return table;
        //}

        public static DbTable LoadTableAsync(string ConnectionKey, string MappingName, EntitySourceType sourceType, int timeout, string[] primaryKey, params object[] keyValueParameters)
        {
            Task<DbTable> t = new Task<DbTable>(() =>
            {
                return LoadTable(ConnectionKey, MappingName, sourceType, timeout, primaryKey, keyValueParameters);

            });
            {
                t.Start();
                t.Wait(timeout);
                if (t.IsCompleted)
                {
                    return t.Result;
                }
            }
            t.TryDispose();
            return null;
        }
        public static DbTable LoadTable(string ConnectionKey, string MappingName, EntitySourceType sourceType, int timeout, string[] primaryKey, params object[] keyValueParameters)
        {

            if (string.IsNullOrEmpty(MappingName))
            {
                throw new ArgumentException("Invalid MappingName");
            }
            if (string.IsNullOrEmpty(ConnectionKey))
            {
                throw new ArgumentException("Invalid DbSet owner ConnectionKey");
            }

            DataTable dt = null;

            using (var cmd = DbContext.Create(ConnectionKey, timeout, true, CacheSettings.EnableConnectionProvider))
            {
                string sql = SqlFormatter.GetCommandText(MappingName, keyValueParameters);

                dt = cmd.ExecuteCommand<DataTable>(sql, sourceType == EntitySourceType.Procedure ? CommandType.StoredProcedure : CommandType.Text, keyValueParameters);
            }

            if (dt == null)
            {
                throw new ArgumentNullException("Reload failed, could not load table " + MappingName);
            }

            DbTable table = null;
            if (primaryKey == null)
                table = CreatWithKey(dt, MappingName, sourceType);
            else
                table = Creat(dt, MappingName, sourceType, primaryKey);

            //table.Owner = source.Owner;
            return table;
        }

        public DbTable Copy()
        {
            return new DbTable()
            {
                Owner = this.Owner,
                _size = this._size,
                Name=this.Name,
                MappingName=this.MappingName,
                PrimaryKey=this.PrimaryKey,
                SourceType=this.SourceType,
                Modified=this.Modified,
                m_ds = new ConcurrentDictionary<string, GenericEntity>(this.m_ds.ToArray())
            };
        }

        #region memebers

        private long _size;
        private ConcurrentDictionary<string, GenericEntity> m_ds;
        private bool suspend;
        internal IDbSet Owner;

   
         #endregion

        #region Events

        /// <summary>
        /// CacheStateChanged
        /// </summary>
        public event EventHandler CacheStateChanged;
        /// <summary>
        /// DataCacheChanging 
        /// </summary>
        public event EventHandler DataCacheChanging;
        /// <summary>
        /// DataCacheChanged
        /// </summary>
        public event EventHandler DataCacheChanged;
        /// <summary>
        /// DataValueChanged
        /// </summary>
        public event EventHandler DataValueChanged;
        /// <summary>
        /// DataException
        /// </summary>
        public event DataCacheExceptionEventHandler DataException;


        #endregion

        #region Ctor

        public DbTable()
        {
            suspend = false;
            m_ds = new ConcurrentDictionary<string, GenericEntity>();
            _size = 0;
            Modified = DateTime.Now;
            SourceType = EntitySourceType.Table;
        }

        /// <summary>
        /// Initialize a new instance of data cache.
        /// </summary>
        public DbTable(DbSet db, string mappingName) : this()
        {
            Owner = db;
            MappingName = mappingName;
            if (Name == null || Name == "")
                Name = MappingName;
        }

        #endregion ctor

        #region IDispose

        /// <summary>
        /// Destructor.
        /// </summary>
        ~DbTable()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(false);
        }

       /// <summary>
       /// Dispose
       /// </summary>
       /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_ds.Clear();   
            }
            
        }

        #endregion

        #region Keys

        /// <summary>
        /// Get all keys from data cache.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllKeys()
        {
           return m_ds.Keys.ToArray();
        }
        #endregion

        #region override

        /// <summary>
        /// Get indicate if it is a copy of DataSource.
        /// </summary>
        public bool IsCopy
        {
            get
            {
                return Owner==null;
            }
        }

        /// <summary>
        /// Get the items (Tables) count of data cache.
        /// </summary>
        public int Count
         {
             get
             {
                 return m_ds.Count;
             }
         }

        public int FieldsCount
        {
            get
            {
                return (DataSource == null || DataSource.Count == 0) ? 0 : DataSource.Values.ElementAt(0).Count;
            }
        }

        /// <summary>
        /// Get the size of data cache in bytes
        /// </summary>
        public long Size
        {
            get
            {
               
                return _size;
            }
        }

        private void OnSizeChanged(GenericEntity currentValue,GenericEntity newValue, bool changeOwner)
        {
            if (isEdit || IsCopy)
                return;

            int cursize = SizeOf(currentValue);
            int newsize = SizeOf(newValue);
            _size += (newsize - cursize);
            Owner.Owner.SizeExchage(cursize, newsize, currentValue == null ? 0 : 1, 1, true);

            if(changeOwner)
            {
                Owner.ChangeSizeInternal(newsize - cursize);
            }
            Modified = DateTime.Now;
            //Task<int[]> task = new Task<int[]>(() => SizeOf(currentValue,newValue));
            //{
            //    task.Start();
            //    task.Wait(120000);
            //    if (task.IsCompleted)
            //    {
            //        int[] result = task.Result;
            //        Owner.Owner.SizeExchage(result[0], result[1], currentValue == null?0:1, 1, true);
            //        _size += (result[1]- result[0]);
            //    }
            //}
            //task.TryDispose();
        }
        private void OnSizeChanged(long currentSize, long newSize, bool changeOwner)
        {
            if (isEdit || IsCopy)
                return;
            _size += (newSize - currentSize);
            Owner.Owner.SizeExchage(currentSize, newSize, 1, 1, true);
            if (changeOwner)
            {
                Owner.ChangeSizeInternal((int)(newSize - currentSize));
            }
            Modified = DateTime.Now;
            //Task task = new Task(() => Owner.Owner.SizeExchage(currentSize, newSize, 1, 1, true));
            //{
            //    task.Start();
            //    task.Wait(120000);
            //    if (task.IsCompleted)
            //    {
            //        _size += (newSize - currentSize);
            //    }
            //}
            //task.TryDispose();
        }
        private void OnSizeChanged(GenericEntity ge, int oprator, bool changeOwner)
        {
            if (isEdit || IsCopy)
                return;
            int size = SizeOf(ge);
            _size += (size * oprator);

            if (oprator < 0)
                Owner.Owner.SizeExchage(size, 0, 1, 0, true);
            else
                Owner.Owner.SizeExchage(0, size, 0, 1, true);

            if (changeOwner)
            {
                Owner.ChangeSizeInternal(size * oprator);
            }
            Modified = DateTime.Now;
            //Task<long> task = new Task<long>(() => SizeOf(ge));
            //{
            //    task.Start();
            //    task.Wait(120000);
            //    if (task.IsCompleted)
            //    {
            //        long size = task.Result;
            //        if (oprator < 0)
            //            Owner.Owner.SizeExchage(size, 0, 1, 0, true);
            //        else
            //            Owner.Owner.SizeExchage(0, size, 0, 1, true);
            //        _size += (size * oprator);

            //    }
            //}
            //task.TryDispose();
        }

        //private void OnSizeChanged(GenericEntity ge, int currentSize, int currentCount)
        //{
        //    if (isEdit)
        //        return;

        //    Task<long> task = new Task<long>(() => SizeOf(ge));
        //    {
        //        task.Start();
        //        task.Wait(120000);
        //        if (task.IsCompleted)
        //        {
        //            long newSize = task.Result;
        //            Owner.Owner.SizeExchage(currentSize, newSize, currentCount, 1, true);
        //            _size += newSize;
                   
        //        }
        //    }
        //    task.TryDispose();
        //}

        /// <summary>
        /// On Cache ThreadSetting State Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCacheStateChanged(EventArgs e)
        {
            if (isEdit || IsCopy)
                return;

            if (CacheStateChanged != null)
                CacheStateChanged(this, e);
        }
       
        /// <summary>
        /// On Data Value Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataValueChanged(EventArgs e)
        {
            if (isEdit || IsCopy)
                return;

            if (DataValueChanged != null)
                DataValueChanged(this, e);
        }

        /// <summary>
        /// On DataException
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataException(DataCacheExceptionEventArgs e)
        {
            if (isEdit)
                return;

            if (DataException != null)
                DataException(this, e);
        }
        /// <summary>
        /// Raise exception event.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="err"></param>
        public void RaiseException(string msg, DataCacheError err)
       {
            if (isEdit)
                return;

            if (DataException != null)
                DataException(this, new DataCacheExceptionEventArgs(msg, err));

            CacheLogger.Logger.LogAction(CacheAction.DataCacheError, CacheActionState.Error, msg);
 
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get table name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Get table name in Database.
        /// </summary>
        public string MappingName { get; internal set; }
        /// <summary>
        /// Get copy of storage dataset
        /// </summary>
        public IDictionary<string, GenericEntity> DataSource
        {
            get { return m_ds; }
        }

        /// <summary>
        /// Get PrimaryKey as EntityKeys
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public string[] PrimaryKey
        {
            get;
            internal set; 
        }

        /// <summary>
        /// Get Modified time.
        /// </summary>
        public DateTime Modified { get; private set; }

        /// <summary>
        /// Get SourceType.
        /// </summary>
        public EntitySourceType SourceType { get; set; }
        #endregion

        #region public methods

        /// <summary>
        /// Suspend layout when Store Data
        /// </summary>
        public void Suspend()
        {
            this.suspend = true;
        }

        #endregion

        #region Add / remove
              

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="value">data table to add into the storage</param>
        public bool Add(GenericEntity value)
        {

            if (value == null)
            {
                throw new ArgumentNullException("Set.value");
            }
            if (value.PrimaryKey == null)
            {
                throw new ArgumentException("Invalid primary key in GenericEntity");
            }


            try
            {
                string primaryKey = value.PrimaryKey.ToString();

                if (m_ds.TryAdd(primaryKey, value))
                {
                    OnSizeChanged(value, 1,true);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataValueChanged(EventArgs.Empty);
            }
        }


        /// <summary>
        /// Set Value into local data table  
        /// </summary>
        /// <param name="value">value to set</param>
        public bool Set(GenericEntity value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Set.value");
            }
            if (value.PrimaryKey == null)
            {
                throw new ArgumentException("Invalid primary key in GenericEntity");
            }
            try
            {
                string primaryKey = value.PrimaryKey.ToString();
                GenericEntity cur = null;
                if (m_ds.TryGetValue(primaryKey, out cur))
                {
                    m_ds[primaryKey] = value;
                    OnSizeChanged(cur, value, true);
                    return true;
                }

                m_ds[primaryKey] = value;
                OnSizeChanged(value, 1, true);
                return true;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataValueChanged(EventArgs.Empty);
            }
        }
        /// <summary>
        /// Add Value into local data table  
        /// </summary>
        /// <param name="primaryKey"</param>
        /// <param name="field"></param>
        /// <param name="value">value to set</param>
        public bool Add(string primaryKey, string field, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Set.value");
            }
            if (primaryKey == null)
            {
                throw new ArgumentException("Invalid primary key in GenericEntity");
            }
            try
            {
                GenericEntity cur = null;
                if (m_ds.TryGetValue(primaryKey, out cur))
                {
                    int oldSize = SizeOf(cur);
                    if (cur.Add(field, value, false))
                    {
                        int newSize = SizeOf(cur);
                        OnSizeChanged(oldSize, newSize, true);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataValueChanged(EventArgs.Empty);
            }
        }
        /// <summary>
        /// Set Value into local data table  
        /// </summary>
        /// <param name="primaryKey"</param>
        /// <param name="field"></param>
        /// <param name="value">value to set</param>
        public bool Set(string primaryKey, string field, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Set.value");
            }
            if (primaryKey == null)
            {
                throw new ArgumentException("Invalid primary key in GenericEntity");
            }
            try
            {
                GenericEntity cur = null;
                if (m_ds.TryGetValue(primaryKey, out cur))
                {
                    int oldSize = SizeOf(cur);
                    cur[field] = value;
                    int newSize = SizeOf(cur);
                    OnSizeChanged(oldSize, newSize, true);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataValueChanged(EventArgs.Empty);
            }
        }
        /// <summary>
        /// Remove data table  from storage
        /// </summary>
        /// <param name="key">table name</param>
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("Remove.key");
            }
            GenericEntity ge;
            try
            {
                if (key == null)
                    return false;

                lock (m_ds)
                {
                    if (m_ds.TryRemove(key,out ge))
                    {
                        OnSizeChanged(ge, -1, true);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataValueChanged(EventArgs.Empty);
            }
          
        }

        #endregion

        #region get and set values


        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">table name</param>
        /// <param name="column">column name</param>
        /// <returns></returns>
        public T GetValue<T>(string key, string column)
        {

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            try
            {
                GenericEntity value;
                if (m_ds.TryGetValue(key, out value))
                {
                    return value.GetValue<T>(column);
                }
                return default(T);
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorColumnNotExist);
                return default(T);
            }
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="key">table name</param>
        /// <param name="column">column name</param>
        /// <returns>object value</returns>
        public object GetValue(string key, string column)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            try
            {
                GenericEntity value;
                if (m_ds.TryGetValue(key, out value))
                {
                    return value.GetValue(column);
                }
                return null;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorColumnNotExist);
                return null;
            }
        }

        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="key">table name</param>
        /// <returns>Hashtable object</returns>
        public GenericEntity GetRow(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            try
            {
                GenericEntity value;
                if (m_ds.TryGetValue(key, out value))
                {
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                RaiseException(ex.Message, DataCacheError.ErrorColumnNotExist);
                return null;
            }
        }

        #endregion

        [NoSerialize]
        [EntityProperty(EntityPropertyType.NA)]
        public GenericEntity this[string primaryKey]
        {
            get
            {
                GenericEntity val;
                m_ds.TryGetValue(primaryKey, out val);
                return val;
            }
            set {
                Set(value);
                //m_ds[primaryKey] = value;
            }
        }

        bool isEdit = false;
        public void BeginEdit()
        {
            isEdit = true;
        }
        public void EndEdit()
        {
            isEdit = false;
        }

       
        //internal static int[] SizeOf(GenericEntity objA, GenericEntity objB)
        //{
        //    return new int[] { SizeOf(objA), SizeOf(objB) };
        //}
        internal static int SizeOf(GenericEntity ge)
        {
            try
            {
                if (ge == null)
                    return 0;

                Task<int> task = new Task<int>(() => ge.Size());
                {
                    task.Start();
                    task.Wait(10000);
                    if (task.IsCompleted)
                    {
                        return task.Result;
                    }
                }
                task.TryDispose();

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #region IserialEntity

        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteValue(PrimaryKey);
            streamer.WriteValue(SourceType);
            streamer.WriteValue(Size);
            streamer.WriteValue(m_ds);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {

            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            PrimaryKey = streamer.ReadValue<string[]>();
            SourceType =(EntitySourceType) streamer.ReadValue<int>();
            _size = streamer.ReadValue<int>();
           
           var sw=Stopwatch.StartNew();

            m_ds = new ConcurrentDictionary<string, GenericEntity>();
            try
            {
                ((BinaryStreamer)streamer).TryReadToGenericDictionary<string, GenericEntity>(m_ds, true);
                //m_ds = (ConcurrentDictionary<string, GenericEntity>)((BinaryStreamer)streamer).ReadToDictionary(new ConcurrentDictionary<string,GenericEntity>(), true);
            }
            finally
            {
                sw.Stop();
                Console.WriteLine("EntityRead table Elapsed : {0}", sw.ElapsedMilliseconds);
            }
            //var dictionary=(Dictionary<string,GenericEntity>) streamer.ReadValue();
            //m_ds=new ConcurrentDictionary<string, GenericEntity>(dictionary.ToArray());
        }
        #endregion


        /// <summary>
        /// Get entity as <see cref="DataTable"/>
        /// </summary>
        /// <returns></returns>
        public DataTable ToDataTable()
        {
            var entity = DataSource.FirstOrDefault();

            if (entity.Value == null)
                return null;

            DataTable dt = new DataTable(Name);
            foreach (var item in entity.Value.Record)
            {
                Type type = item.Value.GetType();
                dt.Columns.Add(item.Key, type);
            }
            foreach (var item in DataSource.Values.ToArray())
            {
                dt.Rows.Add(item.Record.ToDataRow());
            }
            return dt;
        }

        /// <summary>
        /// Get entity items Report
        /// </summary>
        /// <returns></returns>
        public DataTable GetItemsReport()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("TypeName");
            dt.Columns.Add("Label");
            dt.Columns.Add("Modified");
            dt.Columns.Add("Expiration");

            string typeName = typeof(GenericRecord).FullName;

            foreach (var item in DataSource.ToArray())
            {
                dt.Rows.Add(item.Key, item.Value.EntityType.Name, "", item.Value.Modified.ToString(), 0);
                //dt.Rows.Add(item.Key, item.TypeName, item.Id, item.Modified, item.Expiration);
            }
            return dt;

        }

        /// <summary>
        /// Get if db table contains spesific item by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return m_ds.ContainsKey(key);
        }


    }


}

