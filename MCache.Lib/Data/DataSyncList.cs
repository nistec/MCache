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

namespace Nistec.Caching.Data
{
    /// <summary>
    /// Represent the  HashSet of <see cref="DataSyncEntity"/>  data sync entities in cache.
    /// </summary>
    public class DataSyncList :IDisposable
    {
        static object SyncRoot = new object();

        HashSet<DataSyncEntity> m_data;

        internal IDataCache Owner;

        /// <summary>
        /// Initialize a new instance of dtaa sync list.
        /// </summary>
        /// <param name="owner"></param>
        public DataSyncList(IDataCache owner)
        {
            m_data = new HashSet<DataSyncEntity>();
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
            DataSyncEntity[] items = null;

            lock (SyncRoot)
            {
                items = m_data.ToArray();
            }
            return items;
        }
        /// <summary>
        /// Returns a number that represents how many elements in the specified sequence satisfy a condition.
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public int GetItemsCount(SyncType st)
        {
            lock (SyncRoot)
            {
                return m_data.Count(p => p.SyncType == st);
            }
        }
        /// <summary>
        /// Get all items with SyncType.Event as array of <see cref="DataSyncEntity"/>.
        /// </summary>
        /// <returns></returns>
        public DataSyncEntity[] GetEventsItems()
        {
            DataSyncEntity[] items = null;

            lock (SyncRoot)
            {
                items = m_data.Where(p => p.SyncType == SyncType.Event).ToArray();
            }
            return items;
        }
        /// <summary>
        /// Get all items with SyncType.Daily or SyncType.Interval as array of <see cref="DataSyncEntity"/>.
        /// </summary>
        /// <returns></returns>
        public DataSyncEntity[] GetIntervalItems()
        {
            DataSyncEntity[] items = null;

            lock (SyncRoot)
            {
                items = m_data.Where(p => p.SyncType == SyncType.Daily || p.SyncType == SyncType.Interval).ToArray();
            }
            return items;
        }
        /// <summary>
        /// Get specified item in list with entity name.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public DataSyncEntity Get(string entityName)
        {

            DataSyncEntity[] items = GetItems();
            if (items == null)
            {
                return null;
            }
            foreach (DataSyncEntity o in items)
            {
                if (o.EntityName.Equals(entityName))
                    return o;
            }
            return null;
        }
        /// <summary>
        /// Returns a number that represents how many elements in the list.
        /// </summary>
        public int Count
        {
            get { lock (SyncRoot) { return m_data.Count; } }
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

            lock (SyncRoot)
            {
                if (m_data.Contains(syncsource))
                {
                    m_data.Remove(syncsource);
                }

                m_data.Add(syncsource);

                return m_data.Count - 1;
            }

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
                    o.Register(Owner);
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

            if (items != null)
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


            lock (SyncRoot)
            {
                if (!m_data.Contains(entity))
                {
                    m_data.Add(entity);
                }
               else if (replaceExists)
                {
                    m_data.Add(entity);
                }
            }
        }
        /// <summary>
        /// Remove item from list.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Remove(DataSyncEntity entity)
        {
            lock (SyncRoot)
            {
                if (m_data.Contains(entity))
                {
                   return m_data.Remove(entity);
                }
            }
            return false;
        }

        /// <summary>
        /// Removes all elements that match the entityName, and return the number of elements were removed.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public int Remove(string entityName)
        {
            lock (SyncRoot)
            {
                return m_data.RemoveWhere(d => d.EntityName == entityName);
            }
        }

        /// <summary>
        /// Determines whether a HashSet list contains the specified element.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Contains(DataSyncEntity entity)
        {
            lock (SyncRoot)
            {
                return m_data.Contains(entity);
            }
        }
        

        /// <summary>
        /// Determines whether a HashSet list contains the specified element.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public bool Contains(string entityName)
        {
            if (m_data.Count == 0)
                return false;
            DataSyncEntity[] items = GetItems();
            
            foreach (DataSyncEntity o in items)
            {
                if (o.EntityName.Equals(entityName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get indicate whether the list contains the specified item in list.
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public bool IsExists(string viewName)
        {
            if (this.Count == 0)
                return false;
            DataSyncEntity[] items = GetItems();

            foreach (DataSyncEntity o in items)
            {
                if (o.ViewName.Equals(viewName))
                    return true;
            }
            return false;
        }

     
        /// <summary>
        /// Get item from list using the DataSyncEntity.ViewName.
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public DataSyncEntity GetItemByView(string viewName)
        {
            if (this.Count == 0)
                return null;
            DataSyncEntity[] items = GetItems();

            foreach (DataSyncEntity o in items)
            {
                if (o.ViewName.Equals(viewName))
                    return o;
            }
            return null;
        }
    }
}
