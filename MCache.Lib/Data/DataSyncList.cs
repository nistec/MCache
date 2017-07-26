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
using Nistec.Caching.Sync;
using System.Collections;
using System.Collections.Concurrent;

namespace Nistec.Caching.Data
{

    /// <summary>
    /// Represent the  HashSet of <see cref="DataSyncEntity"/>  data sync entities in cache.
    /// </summary>
    public class DataSyncList : IDisposable
    {
        //static object SyncRoot = new object();
  
        ConcurrentDictionary<string, DataSyncEntity> m_data;

        internal IDataCache Owner;

        /// <summary>
        /// Initialize a new instance of dtaa sync list.
        /// </summary>
        /// <param name="owner"></param>
        public DataSyncList(IDataCache owner)
        {
            m_data = new ConcurrentDictionary<string, DataSyncEntity>();
            Owner = owner;
        }

        #region IDisposable
        /// <summary>
        /// Destructor.
        /// </summary>
        ~DataSyncList()
        {
            Dispose(false);
        }
        /// <summary>
        /// Release all resources from current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose item.
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Owner != null)
                {
                    this.Owner = null;
                }
            }
            m_data = null;
        }

        #endregion

        /// <summary>
        /// Get all items as array of <see cref="DataSyncEntity"/>.
        /// </summary>
        /// <returns></returns>
        public DataSyncEntity[] GetItems()
        {
            return m_data.Values.ToArray();
        }
        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public int GetItemsCount(SyncType st)
        {
            return m_data.Values.Count(p => p.SyncType == st);
        }
        /// <summary>
        /// Get all items with SyncType.Event as array of <see cref="DataSyncEntity"/>.
        /// </summary>
        /// <returns></returns>
        public DataSyncEntity[] GetEventsItems()
        {
            var items = m_data.Values.Where(p => p.SyncType == SyncType.Event);
            return (items == null) ? null : items.ToArray();
        }
        /// <summary>
        /// Get all items with SyncType.Daily or SyncType.Interval as array of <see cref="DataSyncEntity"/>.
        /// </summary>
        /// <returns></returns>
        public DataSyncEntity[] GetIntervalItems()
        {
            var items= m_data.Values.Where(p => p.SyncType == SyncType.Daily || p.SyncType == SyncType.Interval);
            return (items == null) ? null : items.ToArray();
               
        }

        /// <summary>
        /// Get specified item in list with entity name.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public DataSyncEntity Get(string entityName)
        {

            DataSyncEntity entity;
            m_data.TryGetValue(entityName, out entity);
            return entity;

        }
        /// <summary>
        /// Returns a number that represents how many elements in the list.
        /// </summary>
        public int Count
        {
            get
            {
                return m_data.Count;
            }
        }

        internal int Add(SyncEntity entity)
        {
            DataSyncEntity syncsource = new DataSyncEntity(entity);//entity.EntityName, entity.ViewName, entity.SourceName, entity.GetSyncTimer());
            return Add(syncsource);
        }

        /// <summary>Appends the specified <see cref="T:Nistec.Caching.Data.DataSyncEntity"></see> object to the end of the collection.</summary>
        /// <returns>The index value of the added item.</returns>
        /// <param name="syncsource">The <see cref="T:Nistec.Caching.Data.DataSyncEntity"></see> to append to the collection. </param>
        public int Add(DataSyncEntity syncsource)
        {
            //syncsource.Owner = this.Owner;

            if (syncsource == null)
            {
                return 0;
            }
           

            m_data[syncsource.EntityName] = syncsource;

            return m_data.Count - 1;

        }

        /// <summary>
        /// Registered All Tables that has sync option by event.
        /// </summary>
        private void RegisteredTablesEvent()
        {
            if (this.Count == 0)
                return;

            DataSyncEntity[] items = GetItems();

            foreach (DataSyncEntity o in items)
            {
                if (o.SyncType == SyncType.Event)
                {
                    o.RegisterAsync(Owner);
                }
            }
        }

        /// <summary>
        /// Get All Tables that has trigger sync option by event.
        /// </summary>
        public string[] GetTablesTrigger()
        {
            if (this.Count == 0)
                return null;
            List<string> list = new List<string>();

            DataSyncEntity[] items = GetEventsItems();

            if (items != null && items.Length>0)
            {
                foreach (DataSyncEntity o in items)
                {
                    foreach (string sn in o.SourceName)
                    {
                        list.Add(sn);
                    }

                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get All Tables or Views that has sync option by event.
        /// </summary>
        public string[] GetViewEvent()
        {
            if (this.Count == 0)
                return null;
            List<string> list = new List<string>();
            DataSyncEntity[] items = GetEventsItems();
            if (items != null)
            {
                foreach (DataSyncEntity o in items)
                {
                    list.Add(o.ViewName);
                }
            }
            return list.ToArray();
        }


        /// <summary>
        /// Add a new <see cref="DataSyncEntity"/> item to the list.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="replaceExists"></param>
        public void AddSafe(DataSyncEntity entity, bool replaceExists)
        {

            //m_data.AddOrUpdate(entity.EntityName, entity, (key, oldValue) => entity);

            m_data[entity.EntityName] = entity;

            //lock (SyncRoot)
            //{
            //    if (!m_data.Contains(entity))
            //    {
            //        m_data.Add(entity);
            //    }
            //    else if (replaceExists)
            //    {
            //        m_data.Add(entity);
            //    }
            //}
        }
        /// <summary>
        /// Remove item from list.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Remove(DataSyncEntity entity)
        {
            DataSyncEntity syncentity;
            return m_data.TryRemove(entity.EntityName, out syncentity);

        }

        /// <summary>
        /// Removes all elements that match the entityName, and return the number of elements were removed.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public int Remove(string entityName)
        {
            DataSyncEntity syncentity;
            if( m_data.TryRemove(entityName, out syncentity))
            {
                return 1;
            }
            return 0;

        }

        /// <summary>
        /// Determines whether a HashSet list contains the specified element.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Contains(DataSyncEntity entity)
        {
            return m_data.ContainsKey(entity.EntityName);

            
        }

        /// <summary>
        /// Determines whether a HashSet list contains the specified element.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsExists(SyncEntity entity)
        {
            var item = Get(entity.EntityName);
            if (item == null)
                return false;

            return item.SyncEntity.IsEquals(entity);

        }

        /// <summary>
        /// Determines whether a HashSet list contains the specified element.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public bool Contains(string entityName)
        {
            return m_data.ContainsKey(entityName);

        }

        /// <summary>
        /// Get indicate whether the list contains the specified item in list.
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public bool IsExists(string viewName)
        {

            var items = m_data.Values.Where(p => p.ViewName == viewName);
            return (items == null) ? false : items.Count() >0 ;

        }


        /// <summary>
        /// Get item from list using the DataSyncEntity.ViewName.
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public DataSyncEntity GetItemByView(string viewName)
        {
            return m_data.Values.Where(p => p.ViewName == viewName).FirstOrDefault();

        }
    }

}
