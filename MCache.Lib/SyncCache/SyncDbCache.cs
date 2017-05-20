﻿//licHeader
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
using System.Collections;
using Nistec.Caching.Data;
using Nistec.Data;
using System.Configuration;
using Nistec.Generic;
using Nistec.Caching.Config;

namespace Nistec.Caching.Sync
{
    /// <summary>
    /// Represents a list of multi database Synchronization cache.
    /// Each <see cref="SyncDb"/> item holds multiple tables from specific database.  
    /// </summary>
    internal class SyncDbCache 
    {
        internal Action<string> FunctionSyncChanged;

        internal void Reload(SyncDbCache copy)
        {
            lock (SyncRoot)
            {
                m_data = copy.m_data;
            }
        }

        bool m_copy=false;

        Dictionary<string, SyncDb> m_data;
        
        public object SyncRoot
        {
            get { return ((ICollection)m_data).SyncRoot; }
        }

        public SyncDb[] GetItems()
        {
            lock (SyncRoot)
            {
                return m_data.Values.ToArray();
            }
        }

        public string[] GetKeys()
        {
            lock (SyncRoot)
            {
                return m_data.Keys.ToArray();
            }
        }

        /// <summary>
        /// Sync and store item <see cref="IDataCache"/> to storage
        /// </summary>
        /// <param name="dc">data cache item</param>
        public void SyncAndStore(IDataCache dc)
        {
            try
            {
                if (dc == null)
                {
                    throw new ArgumentNullException("SyncAndStore.dc");
                }
                dc.WaitForReadySyncState(2000);
                ((SyncDb)dc).SyncState = CacheSyncState.Started;
                lock (SyncRoot)
                {
                    m_data[dc.ConnectionKey] = (SyncDb)dc;
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncItem, CacheActionState.Error, "SyncAndStore error " + ex.Message);
            }
            finally
            {
                if (dc != null)
                    ((SyncDb)dc).SyncState = CacheSyncState.Idle;
            }
        }
       

        #region members

        private bool _initialized = false;
        public string CacheName { get; internal set; }

        public ICollection<SyncDb> Items
        {
            get
            {
                return GetItems();
            }
        }

        /// <summary>
        /// Get All DataCache keys
        /// </summary>
        public ICollection<string> DataKeys
        {
            get { return GetKeys(); }
        }

        #endregion

        #region ctor

        /// <summary>
        /// Initilaize new instance of <see cref="SyncDbCache"/>
        /// </summary>
        /// <param name="cacheName"></param>
        public SyncDbCache(string cacheName)
        {
            m_data = new Dictionary<string, SyncDb>();
            CacheName = cacheName;
         }

        internal SyncDbCache(string cacheName, bool copy)
        {
            m_copy = copy;
            m_data = new Dictionary<string, SyncDb>();
            CacheName = cacheName;

        }

        /// <summary>
        /// Start Cache Synchronization.
        /// </summary>
        /// <param name="intervalSeconds"></param>
        public void Start(int intervalSeconds)
        {
             if (m_copy)
                return;

            foreach (SyncDb dc in this.GetItems())
            {
                dc.Start(intervalSeconds);
            }
            _initialized = true;
        }

        /// <summary>
        /// Stop Cache Synchronization.
        /// </summary>
        public void Stop()
        {
            if (m_copy)
                return;
            if (_initialized)
            {

                foreach (SyncDb dc in this.GetItems())
                {
                    dc.Stop();
                    
                    dc.SyncDataSourceChanged -= new SyncDataSourceChangedEventHandler(_SyncData_SyncDataSourceChanged);
                }

            }

            _initialized = false;
        }

        /// <summary>
        /// Releases all resources used by the System.ComponentModel.Component.
        /// </summary>
        public void Dispose()
        {
            Stop();

            foreach (SyncDb dc in this.GetItems())
            {
                 dc.Dispose();
            }
            if (m_data != null)
            {
                m_data.Clear();
            }

        }
        #endregion

        #region events

        

        /// <summary>
        /// OnSyncChanged
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncChanged(GenericEventArgs<string> e)
        {
            CacheLogger.Debug("SyncDbCache OnSyncChanged : " + e.Args);


            if (FunctionSyncChanged != null)
            {
                FunctionSyncChanged(e.Args);
            }
        }

     

        #endregion

        #region RemoteData


        /// <summary>
        /// Add sync source to db list.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="sc"></param>
        /// <param name="intervalSeconds"></param>
        /// <param name="replaceExists"></param>
        public void AddSyncSource(string connectionKey, DataSyncEntity sc, int intervalSeconds, bool replaceExists = true)
        {
            intervalSeconds = CacheDefaults.GetValidIntervalSeconds(intervalSeconds);
            SyncDb rdc = null;

            lock (this.SyncRoot)
            {

                if (!m_data.TryGetValue(connectionKey, out rdc))
                {
                    ConnectionStringSettings cn = NetConfig.ConnectionSettings(connectionKey);
                    rdc = new SyncDb(connectionKey, cn.ConnectionString, DBProvider.SqlServer);
                    rdc.SyncOption = SyncOption.Auto;

                    //evt-
                    rdc.SyncDataSourceChanged += new SyncDataSourceChangedEventHandler(_SyncData_SyncDataSourceChanged);
                    m_data[connectionKey] = rdc;
                }

            }

            if (rdc != null)
            {
                //sc.FunctionSyncChanged = FunctionSyncChanged;
                rdc.SyncTables.AddSafe(sc, replaceExists);
                if (!m_copy)
                {
                    rdc.Start(intervalSeconds);
                }
            }

        }

        /// <summary>
        /// Remove sync source from db list.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="sc"></param>
        public void RemoveSyncSource(string connectionKey, DataSyncEntity sc)
        {
            SyncDb rdc = null;

            lock (this.SyncRoot)
            {
                if (m_data.TryGetValue(connectionKey, out rdc))
                {
                    if (rdc.SyncTables.Contains(sc))
                    {
                        rdc.SyncTables.Remove(sc);
                        if (!m_data.ContainsKey(connectionKey))
                        {
                            //evt-
                            rdc.SyncDataSourceChanged -= new SyncDataSourceChangedEventHandler(_SyncData_SyncDataSourceChanged);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove sync source from db list.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        public void RemoveSyncSource(string connectionKey, string entityName)
        {
            SyncDb rdc = null;

            lock (this.SyncRoot)
            {
                if (m_data.TryGetValue(connectionKey, out rdc))
                {
                    if (rdc.SyncTables.Contains(entityName))
                    {
                        rdc.SyncTables.Remove(entityName);

                        if (!m_data.ContainsKey(connectionKey))
                        {
                            //evt-
                            rdc.SyncDataSourceChanged -= new SyncDataSourceChangedEventHandler(_SyncData_SyncDataSourceChanged);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add Db To Sync Cache
        /// </summary>
        /// <param name="connectionKey"></param>
       /// <param name="intervalSeconds"></param>
        public void AddDb(string connectionKey, int intervalSeconds)
        {
            if (string.IsNullOrEmpty(connectionKey))
            {
                throw new ArgumentNullException("AddDb.connectionKey");
            }
            intervalSeconds = CacheDefaults.GetValidIntervalSeconds(intervalSeconds);

            lock (SyncRoot)
            {
                ConnectionStringSettings cn = NetConfig.ConnectionSettings(connectionKey);
                SyncDb rdc = new SyncDb(connectionKey, cn.ConnectionString, DBProvider.SqlServer);
                //evt-
                rdc.SyncDataSourceChanged += new SyncDataSourceChangedEventHandler(_SyncData_SyncDataSourceChanged);
                m_data[connectionKey] = rdc;
                if (!m_copy)
                {
                    rdc.Start(intervalSeconds);
                }
            }
        }

        /// <summary>
        /// Remove Db from Sync Cache
        /// </summary>
        /// <param name="connectionKey"></param>
        public void RemoveDb(string connectionKey)
        {
            if (string.IsNullOrEmpty(connectionKey))
            {
                throw new ArgumentNullException("RemoveDb.connectionKey");
            }
            lock (this.SyncRoot)
            {
                if (m_data.ContainsKey(connectionKey))
                {
                    SyncDb rdc = m_data[connectionKey];
                    if (rdc != null)
                    {
                        //evt-
                        rdc.SyncDataSourceChanged -= new SyncDataSourceChangedEventHandler(_SyncData_SyncDataSourceChanged);
                        rdc.Stop();
                    }
                    m_data.Remove(connectionKey);
                }
            }

        }

        void _SyncData_SyncDataSourceChanged(object sender, SyncDataSourceChangedEventArgs e)
        {
            OnSyncChanged(new GenericEventArgs<string>(e.SourceName));
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public void ClearSafe()
        {
            lock (this.SyncRoot)
            {
                m_data.Clear();
            }

        }
        #endregion

        /// <summary>
        /// Get <see cref="SyncDb"/> from Db.
        /// </summary>
        /// <param name="connectionKey"></param>
        public SyncDb GetDb(string connectionKey)
        {
            if (string.IsNullOrEmpty(connectionKey))
            {
                throw new ArgumentNullException("GetDb.connectionKey");
            }
            SyncDb rdc;
            lock (SyncRoot)
            {
                if (m_data.TryGetValue(connectionKey, out rdc))
                {
                    return rdc;
                }
            }

            return null;
        }

        /// <summary>
        /// Get if <see cref="SyncDb"/> cache contains item using connectionKey.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <returns></returns>
        public bool ContainsSyncDb(string connectionKey)
        {
            lock (this.SyncRoot)
            {
                return m_data.ContainsKey(connectionKey);
            }
        }

        /// <summary>
        /// Get if <see cref="SyncDb"/> cache contains item using connectionKey and <see cref="DataSyncEntity"/>. 
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="sc"></param>
        /// <returns></returns>
        public bool ContainsSyncSource(string connectionKey, DataSyncEntity sc)
        {

            lock (this.SyncRoot)
            {
                SyncDb rdc = null;

                if (m_data.TryGetValue(connectionKey, out rdc))
                {
                    return rdc.SyncTables.Contains(sc);
                }

            }
            return false;
        }

        /// <summary>
        /// Get if <see cref="SyncDb"/> cache contains item using connectionKey and table name.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool ContainsSyncSource(string connectionKey, string tableName)
        {

            lock (this.SyncRoot)
            {
                SyncDb rdc = null;

                if (m_data.TryGetValue(connectionKey, out rdc))
                {
                    return rdc.SyncTables.Contains(tableName);
                }

            }
            return false;
        }

    }
}
