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
using System.Threading.Tasks;
using Nistec.Caching.Config;

namespace Nistec.Caching.Data
{

    /// <summary>
    /// Represent the data sync executer in <see cref="DataSyncList"/>. 
    /// </summary>
    public class DataSyncEntity : IDataSyncEntity,IDisposable
    {
        public DataSyncEntity Copy()
        {
            return new DataSyncEntity()
            {
                SyncEntity = this.SyncEntity,
                m_Edited = this.m_Edited,
                m_LastSync = this.m_LastSync,
                SyncTime = this.SyncTime
            };
        }


        public string GetPrimaryKey()
        {
            if (SyncEntity == null || SyncEntity.EntityKeys==null)
                return null;
            
            // clean characters
            //string[] fieldsKey = SyncEntity.GetCleanKeys();
            //if (fieldsKey == null)
            //    return null;
            //return string.Join(",", fieldsKey);
            
            return string.Join(",", SyncEntity.EntityKeys);
        }

        public string GetSourceName()
        {
            if (SourceName == null)
                return null;
            return string.Join(",", SourceName);
        }


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

        ///// <summary>
        ///// Get EntitySyncKey
        ///// </summary>
        //public string EntitySyncKey { get { return SyncEntity == null ? null : SyncEntity.EntityName + "@" + SyncEntity.ConnectionKey ; } }

       
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

        /// <summary>
        /// Get or Set ConnectionKey
        /// </summary>
        public string ConnectionKey { get; set; }

        internal string ClientId { get; set; }

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
            //CacheLogger.Debug("DataSyncEntity OnSyncSourceChanged : " + e.SourceName);

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

        public bool IsEquals(DataSyncEntity dse)
        {
            if (dse == null)
                return false;

            if (!(this.ClientId == dse.ClientId &&
                this.ConnectionKey == dse.ClientId &&
                this.EnableNoLock == dse.EnableNoLock &&
                this.EntityName == dse.ClientId &&
                this.Interval == dse.Interval &&
                this.SourceType == dse.SourceType &&
                this.SourceName == dse.SourceName &&
                this.SyncEntity == dse.SyncEntity &&
                this.SyncTime == dse.SyncTime &&
                this.SyncType == dse.SyncType &&
                this.ViewName == dse.ViewName))
                return false;

            if (!((this.SourceName != null && dse.SourceName != null) && Strings.IsEqual(this.SourceName, dse.SourceName)))
                    return false;

            if (!((this.SyncEntity != null && dse.SyncEntity != null) && this.SyncEntity.IsEquals(dse.SyncEntity)))
                return false;

            if (!((this.SyncTime != null && dse.SyncTime != null) && this.SyncTime.IsEquals(dse.SyncTime)))
                return false;

            return true;
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
        /// Parameters are 0=TableWatcherName, 1=ClientId, 2=tableName
        /// </summary>
        public const string SqlAddTableToWatcherTable =
@"
if not exists(
Select ClientId FROM {0} 
WHERE ClientId='{1}' AND TableName='{2}') 

INSERT INTO {0}(ClientId,TableName,Edited)
VALUES('{1}','{2}',0)";

        /// <summary>
        /// Sql text to remove table from wather table
        /// Parameters are 0=TableWatcherName, 1=tableName
        /// </summary>
        public const string SqlRemoveTableFromWatcherTable =
@"
DELETE FROM {0}
WHERE TableName='{1}'";

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


        /// <summary>
        /// Sql text to drop Trigger
        /// Parameters are 0=Database name 1=TriggerName
        /// </summary>
        public const string SqlRemoveTrigger =
@"USE {0}
DROP TRIGGER trgw_{1}
--GO";
        #endregion

        #region ctor

        private DataSyncEntity()
        {
        }

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
            if (entity.SyncType == SyncType.Event && entity.Interval == TimeSpan.Zero)
                entity.Interval = TimeSpan.FromSeconds(60);
            SyncTime = new SyncTimer(entity.Interval, entity.SyncType);
            ConnectionKey = entity.ConnectionKey;
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
            ConnectionKey = entity.ConnectionKey;
        }

        /// <summary>
        /// Initialize a new instance of data sync entity.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="interval"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public DataSyncEntity(string connectionKey, string entityName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan interval, bool enableNoLock, int commandTimeout)
        {
            SyncEntity = new SyncEntity(entityName, mappingName, sourceName, syncType, interval, enableNoLock, commandTimeout);
            if (syncType == SyncType.Event && interval == TimeSpan.Zero)
                interval = TimeSpan.FromSeconds(60);
            SyncTime = new SyncTimer(interval, syncType);
            ConnectionKey = connectionKey;
        }

        /// <summary>
        /// Initialize a new instance of data sync entity.
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="syncType"></param>
        /// <param name="interval"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public DataSyncEntity(string connectionKey, string entityName, string mappingName, SyncType syncType, TimeSpan interval, bool enableNoLock=false, int commandTimeout=0)
        {
            SyncEntity = new SyncEntity(entityName, mappingName, new string[] { mappingName }, syncType, interval, enableNoLock, commandTimeout);
            if (syncType == SyncType.Event && interval == TimeSpan.Zero)
                interval = TimeSpan.FromSeconds(60);
            SyncTime = new SyncTimer(interval, syncType);
            ConnectionKey = connectionKey;
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
        /// Register item to <see cref="DbWatcher"/> table async.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public int RegisterAsync(IDataCache Owner)
        {
            return Task.Factory.StartNew<int>(() => Register(Owner, true)).Result;
        }

        /// <summary>
        /// Register item to <see cref="DbWatcher"/> table.
        /// </summary>
        /// <param name="Owner"></param>
        /// <param name="ensureTableWatcher"></param>
        /// <returns></returns>
        public int Register(IDataCache Owner, bool ensureTableWatcher)
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

                //if(DataSyncList.Global.Contains(SyncEntity.EntityName))
                //{
                //    return 0;
                //}

                using (var db = Owner.Db())
                {
                    db.OwnsConnection = true;

                    //ensure table watcher exists
                    if (ensureTableWatcher)
                    {
                        var result = DbWatcher.CreateTableWatcher(db.NewCmd());
                        if (result <= 0)
                        {
                            throw new Exception("CreateTableWatcher failed, " + Owner.ConnectionKey);
                        }
                    }

                    foreach (string sn in SyncEntity.SourceName)
                    {
                        try
                        {
                            res += db.ExecuteCommandNonQuery(string.Format(SqlAddTableToWatcherTable, Owner.TableWatcherName, Owner.ClientId, sn), null);
                            Thread.Sleep(10);
                        }
                        catch (Exception ex)
                        {
                            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity Register item error: " + ex.Message);
                        }
                    }
                    db.OwnsConnection = false;
                }
                if(res>0)
                {
                    //this.ClientId = Owner.ClientId;
                    //DataSyncList.Global.AddSafe(this, true);
                }

                //using (IDbCmd dbCmd = Owner.Db.NewCmd())
                //{
                //    foreach (string sn in SyncEntity.SourceName)
                //    {
                //        res += dbCmd.ExecuteNonQuery(string.Format(SqlAddTableToWatcherTable, Owner.TableWatcherName, Owner.ClientId, sn));
                //        Thread.Sleep(10);
                //    }
                //}
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity Register error: " + ex.Message);
            }
            return res;
        }

        /// <summary>
        /// Remobe registered item to <see cref="DbWatcher"/> table.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public int RegisterRemove(IDataCache Owner)
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

                using (var db = Owner.Db())
                {
                    db.OwnsConnection = true;

                    foreach (string sn in SyncEntity.SourceName)
                    {
                        try
                        {
                            res += db.ExecuteCommandNonQuery(string.Format(SqlRemoveTableFromWatcherTable, Owner.TableWatcherName, sn), null);
                            Thread.Sleep(10);
                        }
                        catch (Exception ex)
                        {
                            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity Register item error: " + ex.Message);
                        }
                    }
                    db.OwnsConnection = false;
                }
                if (res > 0)
                {
                    //this.ClientId = Owner.ClientId;
                    DataSyncList.Global.Remove(this);

                    CacheLogger.DebugFormat("RegisterRemove Removed DataSyncEntity from DataSyncList: {0} ", EntityName);
                }

                //using (IDbCmd dbCmd = Owner.Db.NewCmd())
                //{
                //    foreach (string sn in SyncEntity.SourceName)
                //    {
                //        res += dbCmd.ExecuteNonQuery(string.Format(SqlAddTableToWatcherTable, Owner.TableWatcherName, Owner.ClientId, sn));
                //        Thread.Sleep(10);
                //    }
                //}
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity Remove register error: " + ex.Message);
            }
            return res;
        }
        /// <summary>
        /// Create table trigger in table watcher async.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public int CreateTableTriggerAsync(IDataCache Owner)
        {
            return Task.Factory.StartNew<int>(() => CreateTableTrigger(Owner)).Result;
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

                if (SyncEntity.SourceName == null)
                    return -1;

                using (var db = Owner.Db())
                {

                    string Database = db.Connection.Database;

                    db.OwnsConnection = true;

                    foreach (string sn in SyncEntity.SourceName)
                    {
                        try
                        {
                            string tgw = sn.Replace(".", "_");
                            int exists = db.QueryScalar<int>(string.Format(SqlIsTriggerExists, Database, tgw),0);
                            if (exists == 0)
                            {
                                res += db.ExecuteCommandNonQuery(string.Format(SqlCreateTrigger, Database, sn, Owner.TableWatcherName, tgw),null);
                            }
                        }
                        catch (Exception ex)
                        {
                            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity CreateTableTrigger item error: " + ex.Message);
                        }
                    }
                    db.OwnsConnection = false;
                }

                //using (IDbCmd dbCmd = Owner.Db.NewCmd())
                //{
                //    string Database = dbCmd.Connection.Database;
                //    if (SyncEntity.SourceName == null)
                //        return -1;
                //    foreach (string sn in SyncEntity.SourceName)
                //    {
                //        string tgw = sn.Replace(".", "_");
                //        int exists = dbCmd.ExecuteScalar<int>(string.Format(SqlIsTriggerExists, Database, tgw));
                //        if (exists == 0)
                //        {
                //            res += dbCmd.ExecuteNonQuery(string.Format(SqlCreateTrigger, Database, sn, Owner.TableWatcherName, tgw));
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity CreateTableTrigger error: " + ex.Message);
            }

            return res;
        }
        /// <summary>
        /// Remove table trigger in table watcher.
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        public int RemoveTableTrigger(IDataCache Owner)
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

                using (var db = Owner.Db())
                {

                    string Database = db.Connection.Database;

                    db.OwnsConnection = true;

                    foreach (string sn in SyncEntity.SourceName)
                    {
                        try
                        {
                            string tgw = sn.Replace(".", "_");
                            int exists = db.QueryScalar<int>(string.Format(SqlIsTriggerExists, Database, tgw), 0);
                            if (exists>0)
                            {
                                res += db.ExecuteCommandNonQuery(string.Format(SqlRemoveTrigger, Database, tgw), null);
                            }
                            if(res>0)
                                CacheLogger.DebugFormat("RemoveTableTrigger Removed Table Trigger from Table: {0}.{1} ", Database,sn);
                        }
                        catch (Exception ex)
                        {
                            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity RemoveTableTrigger item error: " + ex.Message);
                        }
                    }
                    db.OwnsConnection = false;
                }

            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity RemoveTableTrigger error: " + ex.Message);
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

                using (var dbCmd = DbContext.Create(Owner.ConnectionKey, CacheSettings.EnableConnectionProvider))//Owner.Db.NewCmd())
                {
                    res = dbCmd.NewCmd().ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}'", Owner.TableWatcherName, Owner.ClientId));
                }

                //using (IDbCmd dbCmd = DbFactory.Create(Owner.ConnectionKey))//Owner.Db.NewCmd())
                //{
                //    res = dbCmd.ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}'", Owner.TableWatcherName, Owner.ClientId));
                //}

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

                using (var db = Owner.Db())
                {
                    db.OwnsConnection=true;

                    foreach (string sn in SyncEntity.SourceName)
                    {
                        try
                        {
                            res += db.ExecuteCommandNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}' and TableName='{2}'", Owner.TableWatcherName, Owner.ClientId, sn), null);
                        }
                        catch(Exception ex)
                        {
                            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "DataSyncEntity ResetEdited item error: " + ex.Message);
                        }
                    }
                    db.OwnsConnection = false;
                }

                //using (IDbCmd dbCmd = Owner.Db.NewCmd())
                //{
                //    foreach (string sn in SyncEntity.SourceName)
                //    {
                //        res += dbCmd.ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}' and TableName='{2}'", Owner.TableWatcherName, Owner.ClientId, sn));
                //    }
                //}

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
           //~Console.WriteLine("Debuger-DataSyncEntity.SyncAndStore...");

            try
            {
                CacheLogger.Debug("SyncAndStore Start, Entity: " + this.ToString());

                if (Owner == null)
                {
                    throw new Exception("Invalid DataCache Owner");
                }
                if (SyncEntity == null)
                {
                    throw new Exception("SyncEntity is null or disposed");
                }
                ((ISyncronizer)Owner.Parent).Refresh(SyncEntity.EntityName);

                //if (Owner.EnableDataSource)
                //{
                //    Owner.Store(GetTableSource(Owner), SyncEntity.EntityName);
                //}
                //else
                //{
                //    Owner.Refresh(SyncEntity.EntityName);
                //}
                if (SyncEntity.SyncType == SyncType.Event)
                {
                    ResetEdited(Owner);
                }
             
                OnSyncSourceChanged(new SyncDataSourceChangedEventArgs(SyncEntity.EntityName));
            }
            catch (Exception ex)
            {
                if (Owner == null)
                    CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncAndStore TableSource error: " + ex.Message);
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
            using (var dbCmd = DbContext.Create(Owner.ConnectionKey, CacheSettings.EnableConnectionProvider))//Owner.Db.NewCmd())
            {
                return dbCmd.NewCmd().ExecuteDataTable(SyncEntity.EntityName, string.Format("SELECT * FROM {0}{1}", SyncEntity.ViewName, SyncEntity.EnableNoLock ? " with(nolock)" : ""), SyncEntity.CommandTimeout, SyncEntity.MissingSchemaAction == System.Data.MissingSchemaAction.AddWithKey);
            }

            //using (IDbCmd dbCmd = DbFactory.Create(Owner.ConnectionKey))//Owner.Db.NewCmd())
            //{
            //    return dbCmd.ExecuteDataTable(SyncEntity.EntityName, string.Format("SELECT * FROM {0}{1}", SyncEntity.ViewName, SyncEntity.EnableNoLock ? " with(nolock)" : ""), SyncEntity.CommandTimeout, SyncEntity.MissingSchemaAction == System.Data.MissingSchemaAction.AddWithKey);
            //}
        }

        #endregion
    }

}
