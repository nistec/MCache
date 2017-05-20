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

using Nistec.Caching;
using System.Xml;
using Nistec.Xml;
using Nistec.Data.Factory;
using Nistec.Caching.Sync;
using Nistec.Threading;
using Nistec.Caching.Server;
using Nistec.Data.Entities;

namespace Nistec.Caching.Data
{

    /// <summary>
    /// Represent the data sync executer in <see cref="DataSyncList"/>. 
    /// </summary>
    public class DataSyncEntity : IDataSyncEntity,IDisposable
    {
        #region members

        /// <summary>
        /// syncTime
        /// </summary>
        public SyncEntity SyncEntity { get; private set; }
       
        /// <summary>
        /// syncTime
        /// </summary>
        public SyncTimer SyncTime { get; private set; }
       
        private bool m_Edited;
        private string m_LastSync;

        #endregion

        #region properties

       
        /// <summary>
        /// Get EntityName
        /// </summary>
        public string EntityName { get { return SyncEntity == null ? null : SyncEntity.EntityName; } }
        /// <summary>
        /// Get ViewName
        /// </summary>
        public string ViewName { get { return SyncEntity == null ? null : SyncEntity.ViewName; } }
        /// <summary>
        /// Get SourceName
        /// </summary>
        public string[] SourceName { get { return SyncEntity == null ? null : SyncEntity.SourceName; } }
        /// <summary>
        /// Get PreserveChanges
        /// </summary>
        public bool PreserveChanges { get { return SyncEntity == null ? false : SyncEntity.PreserveChanges; } }
        /// <summary>
        /// Get MissingSchemaAction
        /// </summary>
        public MissingSchemaAction MissingSchemaAction { get { return SyncEntity == null ? MissingSchemaAction.Add : SyncEntity.MissingSchemaAction; } }

        /// <summary>
        /// Get syncTime
        /// </summary>
        public TimeSpan Interval { get { return SyncEntity == null ? TimeSpan.Zero : SyncEntity.Interval; } }
        /// <summary>
        /// Get syncTime
        /// </summary>
        public SyncType SyncType { get { return SyncEntity == null ? SyncType.None : SyncEntity.SyncType; } }
        /// <summary>
        /// Get SourceType
        /// </summary>
        public EntitySourceType SourceType { get { return SyncEntity == null ? EntitySourceType.Table : SyncEntity.SourceType; } }
       
        /// <summary>
        /// Get indicate whether cache should use with nolock statement.
        /// </summary>
        public bool EnableNoLock { get { return SyncEntity == null ? false : SyncEntity.EnableNoLock; } }

        #endregion

        #region events
        /// <summary>
        /// Sync Source Changed Event Handler
        /// </summary>
        public event SyncDataSourceChangedEventHandler SyncSourceChanged;
        /// <summary>
        /// On SyncSource Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncSourceChanged(SyncDataSourceChangedEventArgs e)
        {
            CacheLogger.Debug("DataSyncEntity OnSyncSourceChanged : " + e.SourceName);

            if (SyncSourceChanged != null)
            {
                this.SyncSourceChanged(this, e);
            }
        }

        #endregion

        #region override

        /// <summary>
        ///  Determines whether the specified System.Object is equal to the current DataSyncEntity.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DataSyncEntity))
                return false;
            if (SyncEntity==null)
                return ((DataSyncEntity)obj).SyncEntity==null;
            if (((DataSyncEntity)obj).SyncEntity == null)
                return SyncEntity == null;
            return base.Equals(((DataSyncEntity)obj).SyncEntity.EntityName == SyncEntity.EntityName);
        }
        /// <summary>
        /// Get Serves as a hash function for a particular type.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Get data sync entity name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (SyncEntity == null)
                return "(null)";
            return SyncEntity.EntityName;
        }
        #endregion

        #region sql args

        /// <summary>
        /// Sql text to Add table to wather table
        /// Parameters are 0=dbTableName, 1=ClientId, 2=tableName
        /// </summary>
        public const string SqlAddTableToWatcherTable =
@"
if not exists(
Select ClientId FROM {0} 
WHERE ClientId='{1}' AND TableName='{2}') 

INSERT INTO {0}(ClientId,TableName,Edited)
VALUES('{1}','{2}',0)";


        /// <summary>
        /// Sql text to check if trigger all ready exists for specific table
        /// Parameters are 0=Database 1=Tabel name
        /// </summary>
        public const string SqlIsTriggerExists =
@"USE {0}
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = N'trgw_{1}' 
	   AND 	  type = 'TR')

select 1 as IsExists
else
select 0 as IsExists";


        /// <summary>
        /// Sql text to create Watcher Trigger
        /// Parameters are 0=Database name  1=TableName 2=TableWatcherName  3=TriggerName
        /// </summary>
        public const string SqlCreateTrigger =
@"CREATE   TRIGGER trgw_{3}
ON {1}
FOR DELETE, INSERT, UPDATE 
AS
BEGIN
 UPDATE {2} SET Edited=1 where TableName='{1}'
END
--GO";
        #endregion

        #region ctor

        /// <summary>
        /// Initilaize a new instance of <see cref="DataSyncEntity"/>
        /// </summary>
        /// <param name="entity"></param>
        public DataSyncEntity(SyncEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("DataSyncEntity.ctor entity");
            }
            SyncEntity = entity;
            SyncTime = new SyncTimer(entity.Interval, entity.SyncType);
        }

        /// <summary>
        /// Initilaize a new instance of <see cref="DataSyncEntity"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="syncTimer"></param>
        public DataSyncEntity(SyncEntity entity, SyncTimer syncTimer)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("DataSyncEntity.ctor entity");
            }
            SyncEntity = entity;
            SyncTime = syncTimer;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Destructor.
        /// </summary>
        ~DataSyncEntity()
        {
            Dispose(false);
        }
        /// <summary>
        /// Release all resource from current instance.
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
            if (disposed)
                return;
            if (disposing)
            {

                if (SyncEntity != null)
                {
                    SyncEntity.Dispose();
                    SyncEntity = null;
                }
                if (SyncTime != null)
                {
                    SyncTime = null;
                }
            }
            m_LastSync = null;
            disposed = true;
        }

        bool disposed = false;
        /// <summary>
        /// Get whether this item is disposed;
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        
        #endregion

        #region sync

        internal void SetEdited(bool value)
        {
            m_Edited = value;
        }

        /// <summary>
        /// Register item to <see cref="DbWatcher"/> table.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public int Register(IDataCache Owner)
        {
            int res = 0;
            try
            {
                if (Owner == null)
                {
                    throw new Exception("Invalid DataCache Owner");
                }
                if (SyncEntity == null)
                {
                    throw new Exception("SyncEntity is null or disposed");
                }
                if (SyncEntity.SourceName == null)
                    return -1;
                using (IDbCmd dbCmd = Owner.Db.NewCmd())
                {
                    foreach (string sn in SyncEntity.SourceName)
                    {
                        res += dbCmd.ExecuteNonQuery(string.Format(SqlAddTableToWatcherTable, Owner.TableWatcherName, Owner.ClientId, sn));
                    }
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity Register error: " + ex.Message);
            }
            return res;
        }

        /// <summary>
        /// Create table trigger in table watcher.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public int CreateTableTrigger(IDataCache Owner)
        {
            int res = 0;
            try
            {
                if (Owner == null)
                {
                    throw new Exception("Invalid DataCache Owner");
                }
                if (SyncEntity == null)
                {
                    throw new Exception("SyncEntity is null or disposed");
                }

                using (IDbCmd dbCmd = Owner.Db.NewCmd())
                {
                    string Database = dbCmd.Connection.Database;
                    if (SyncEntity.SourceName == null)
                        return -1;
                    foreach (string sn in SyncEntity.SourceName)
                    {
                        string tgw = sn.Replace(".", "_");
                        int exists = dbCmd.ExecuteScalar<int>(string.Format(SqlIsTriggerExists, Database, tgw));
                        if (exists == 0)
                        {
                            
                            res += dbCmd.ExecuteNonQuery(string.Format(SqlCreateTrigger, Database, sn, Owner.TableWatcherName, tgw));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity CreateTableTrigger error: " + ex.Message);
            }

            return res;
        }

        /// <summary>
        /// Get if is edited
        /// </summary>
        public bool Edited
        {
            get { return m_Edited; }
        }

        /// <summary>
        /// Get Last Sync
        /// </summary>
        public string LastSync
        {
            get { return m_LastSync; }
        }
        /// <summary>
        /// Reset all edited item in table watcher.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public int ResetAll(IDataCache Owner)
        {
            int res = 0;
            try
            {
                if (Owner == null)
                {
                    throw new Exception("Invalid DataCache Owner");
                }
                using (IDbCmd dbCmd = Owner.Db.NewCmd())
                {
                    res = dbCmd.ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}'", Owner.TableWatcherName, Owner.ClientId));
                }
                m_Edited = false;
                m_LastSync = DateTime.Now.ToString("s");
                return res;
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity ResetAll error: " + ex.Message);
            }
            return 0;
        }
        /// <summary>
        /// Reset sepcified item in table watcher.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public int ResetEdited(IDataCache Owner)
        {
            int res = 0;

            try
            {
               
                if (Owner == null)
                {
                    throw new Exception("Invalid DataCache Owner");
                }
                if (SyncEntity == null)
                {
                    throw new Exception("SyncEntity is null or disposed");
                }
                if (SyncEntity.SourceName == null)
                    return -1;
                using (IDbCmd dbCmd = Owner.Db.NewCmd())
                {
                    foreach (string sn in SyncEntity.SourceName)
                    {
                        res += dbCmd.ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}' and TableName='{2}'", Owner.TableWatcherName, Owner.ClientId, sn));
                    }
                }
                m_Edited = false;
                m_LastSync = DateTime.Now.ToString("s");
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity ResetEdited error: " + ex.Message);
            }
            return res;
        }
        /// <summary>
        /// Refresh the current sync entity and raise the <see cref="SyncDataSourceChangedEventArgs"/> event.
        /// </summary>
        /// <param name="state"></param>
        public void Refresh(object state)
        {
            IDataCache Owner = (IDataCache)state;
            SyncAndStore(Owner);
        }

        /// <summary>
        /// Store data source to data cache and raise the <see cref="SyncDataSourceChangedEventArgs"/> event.
        /// </summary>
        /// <param name="Owner"></param>
        public void SyncAndStore(IDataCache Owner)
        {
            try
            {
                CacheLogger.Debug("Store Edited : " + this.ToString());
                if (Owner == null)
                {
                    throw new Exception("Invalid DataCache Owner");
                }
                if (SyncEntity == null)
                {
                    throw new Exception("SyncEntity is null or disposed");
                }
                if (Owner.EnableDataSource)
                {
                    Owner.Store(GetTableSource(Owner), SyncEntity.EntityName);
                }
                if (SyncEntity.SyncType == SyncType.Event)
                {
                    ResetEdited(Owner);
                }
                OnSyncSourceChanged(new SyncDataSourceChangedEventArgs(SyncEntity.EntityName));
            }
            catch (Exception ex)
            {
                if (Owner == null)
                    CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "StoreTableSource error: " + ex.Message);
                else
                    Owner.RaiseException(ex.Message, DataCacheError.ErrorMergeData);
            }
        }
        /// <summary>
        /// Get table source from database.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public DataTable GetTableSource(IDataCache Owner)
        {
            if (Owner == null)
            {
                throw new Exception("Invalid DataCache Owner");
            }
            if (SyncEntity == null)
            {
                throw new Exception("SyncEntity is null or disposed");
            }
            using (IDbCmd dbCmd = Owner.Db.NewCmd())
            {
                return dbCmd.ExecuteDataTable(SyncEntity.EntityName, string.Format("SELECT * FROM {0}{1}", SyncEntity.ViewName, SyncEntity.EnableNoLock ? " with(nolock)" : ""), SyncEntity.CommandTimeout, SyncEntity.MissingSchemaAction == System.Data.MissingSchemaAction.AddWithKey);
            }
        }

        #endregion
    }

}
