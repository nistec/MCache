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

namespace Nistec.Legacy
{


     /// <summary>
    /// SyncSourceCollection
    /// </summary>
    public class SyncSourceCollection : List<SyncSource>
    {
        internal DalCache Owner;

        public SyncSourceCollection(DalCache owner)
        {
            Owner = owner;
        }

        /// <summary>Appends the specified <see cref="T:Nistec.Caching.Data.SyncSource"></see> object to the end of the collection.</summary>
        /// <returns>The index value of the added item.</returns>
        /// <param name="syncsource">The <see cref="T:Nistec.Caching.Data.SyncSource"></see> to append to the collection. </param>
        public new int Add(SyncSource syncsource)
        {
            if (Contains(syncsource.TableName))
            {
               base.Remove(this[syncsource.TableName]);
            }
            
            syncsource.Owner = this.Owner;
            base.Add(syncsource);
            return this.Count - 1;
        }

        /// <summary>Creates a <see cref="T:Nistec.Caching.Data.SyncSource"></see> object with the specified name and default value, and appends it to the end of the collection.</summary>
        /// <returns>The index value of the added item.</returns>
        /// <param name="name">The name of the syncsource. </param>
        /// <param name="mappingName">A string that serves a mappingName for the syncsource. </param>
        /// <param name="sourceName">A string that serves a sourceName for the syncsource. </param>
        /// <param name="syncTime">A string that serves a syncTime for the syncsource. </param>
        public int Add(string name, string mappingName,string sourceName, SyncTimer syncTime)
        {
            return Add(new SyncSource(name, mappingName, sourceName, syncTime));
        }

        /// <summary>Creates a <see cref="T:Nistec.Caching.Data.SyncSource"></see> object with the specified name and default value, and appends it to the end of the collection.</summary>
        /// <returns>The index value of the added item.</returns>
        /// <param name="name">The name of the syncsource. </param>
        /// <param name="mappingName">A string that serves a mappingName for the syncsource. </param>
        /// <param name="syncTime">A string that serves a syncTime for the syncsource. </param>
        public int Add(string name, string mappingName, SyncTimer syncTime)
        {
            return Add(new SyncSource(name, mappingName, mappingName, syncTime));
        }

        /// <summary>
        /// Registered All Tables that has sync option by event.
        /// </summary>
        private void RegisteredTablesEvent()
        {
            if (this.Count == 0)
                return;
            foreach (SyncSource o in this)
            {
                if (o.SyncType == SyncType.Event)
                {
                    o.Register();
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
            foreach (SyncSource o in this)
            {
                if (o.SyncType == SyncType.Event)
                {
                    list.Add(o.SourceName);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get All Tables or Views that has sync option by event.
        /// </summary>
        public string[] GetTablesEvent()
        {
            if (this.Count == 0)
                return null;
            List<string> list = new List<string>();
            foreach (SyncSource o in this)
            {
                if (o.SyncType == SyncType.Event)
                {
                    list.Add(o.MappingName);
                }
            }
            return list.ToArray();
        }

        ///// <summary>
        ///// Sync All Tables in SyncTables list.
        ///// </summary>
        //public void SyncRegisteredTables()
        //{
        //    if (this.Count == 0)
        //        return;
        //    foreach (SyncSource o in this)
        //    {
        //        if (o.syncTime.SyncType == SyncType.Event)
        //        {
        //            if (o.Edited())
        //            {
        //                o.MergeTableSource();
        //                //Owner.Merge(o.GetTableSource(), o.TableName);
        //                //o.ResetEdited();
        //            }
        //        }
        //        //else if (o.syncTime.SyncType == SyncType.None)
        //        //    SyncTableSource(o);
        //        else if (o.syncTime.HasTimeToRun())
        //        {
        //            o.MergeTableSource();
        //            //Owner.Merge(o.GetTableSource(), o.TableName);
        //        }
        //    }
        //}

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="mappingName"></param>
        /// <returns></returns>
        public bool Contains(string tableName)
        {
            if (this.Count == 0)
                return false;
            foreach (SyncSource o in this)
            {
                if (o.TableName.Equals(tableName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// IsExists
        /// </summary>
        /// <param name="mappingName"></param>
        /// <returns></returns>
        public bool IsExists(string mappingName)
        {
            if (this.Count == 0)
                return false;
            foreach (SyncSource o in this)
            {
                if (o.MappingName.Equals(mappingName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// SyncSource item
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public SyncSource this[string tableName]
        {
            get
            {
                if (this.Count == 0)
                    return null;
                foreach (SyncSource o in this)
                {
                    if (o.TableName.Equals(tableName))
                        return o;
                }
                return null;
            }
        }
        /// <summary>
        /// SyncSource item
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public SyncSource GetItem(string mappingName)
        {
                if (this.Count == 0)
                    return null;
                foreach (SyncSource o in this)
                {
                    if (o.MappingName.Equals(mappingName))
                        return o;
                }
                return null;
        }
    }

  
    /// <summary>
    /// SyncSource
    /// </summary>
    public class SyncSource
    {
        /// <summary>
        /// MappingName
        /// </summary>
        public readonly string MappingName;
        /// <summary>
        /// TableName
        /// </summary>
        public readonly string TableName;
        /// <summary>
        /// SourceName
        /// </summary>
        public readonly string SourceName;
        /// <summary>
        /// PreserveChanges
        /// </summary>
        public readonly bool PreserveChanges;
        /// <summary>
        /// MissingSchemaAction
        /// </summary>
        public readonly MissingSchemaAction MissingSchemaAction;
        /// <summary>
        /// syncTime
        /// </summary>
        public readonly SyncTimer SyncTime;
        /// <summary>
        /// syncTime
        /// </summary>
        public readonly SyncType SyncType;
        /// <summary>
        /// syncTime
        /// </summary>
        internal DalCache Owner;

        private bool m_Edited;
        private string m_LastSync;

        public event SyncDataSourceChangedEventHandler SyncSourceChanged;

        public override bool Equals(object obj)
        {
            if (!(obj is SyncSource))
                return false;
            return base.Equals(((SyncSource)obj).TableName == this.TableName);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return TableName;
        }

        internal void SetEdited(bool value)
        {
            m_Edited = value;
            //if (value)
            //{
            //    OnSyncSourceChanged(new SyncDataSourceChangedEventArgs(this.TableName));
            //}
        }

 
        protected virtual void OnSyncSourceChanged(SyncDataSourceChangedEventArgs e)
        {
            if (SyncSourceChanged != null)
            {
                this.SyncSourceChanged(this, e);
            }
        }


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
        /// Parameters are 0=Database name  1=TableName 2=TableWatcherName 
        /// </summary>
        public const string SqlCreateTigger =
@"CREATE   TRIGGER trgw_{1}
ON {1}
FOR DELETE, INSERT, UPDATE 
AS
BEGIN
 UPDATE {2} SET Edited=1 where TableName='{1}'
END
--GO";

        /// <summary>
        /// SyncSource
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        public SyncSource(string tableName, string mappingName, string sourceName)
        {
            TableName = tableName;
            MappingName = mappingName;
            SourceName = sourceName;
            PreserveChanges = false;
            MissingSchemaAction = MissingSchemaAction.Add;
            SyncTime = SyncTimer.Empty;
            SyncType = SyncType.None;
        }

        /// <summary>
        /// SyncSource
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="synctime"></param>
        public SyncSource(string tableName, string mappingName, string sourceName, SyncTimer synctime)
        {
            TableName = tableName;
            MappingName = mappingName;
            SourceName = sourceName;
            PreserveChanges = false;
            MissingSchemaAction = MissingSchemaAction.Add;
            SyncTime = synctime;
            SyncType = synctime.SyncType;
        }

        /// <summary>
        /// SyncSource
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="preserveChanges"></param>
        /// <param name="missingSchemaAction"></param>
        public SyncSource(string tableName, string mappingName, string sourceName, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            TableName = tableName;
            MappingName = mappingName;
            SourceName = sourceName;
            PreserveChanges = preserveChanges;
            MissingSchemaAction = missingSchemaAction;
            SyncTime = SyncTimer.Empty;
            SyncType = SyncType.None;
        }
        /// <summary>
        /// SyncSource
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="preserveChanges"></param>
        /// <param name="missingSchemaAction"></param>
        /// <param name="synctime"></param>
        public SyncSource(string tableName, string mappingName, string sourceName, bool preserveChanges, MissingSchemaAction missingSchemaAction, SyncTimer synctime)
        {
            TableName = tableName;
            MappingName = mappingName;
            SourceName = sourceName;
            PreserveChanges = preserveChanges;
            MissingSchemaAction = missingSchemaAction;
            SyncTime = synctime;
            SyncType = synctime.SyncType;
        }
 
        public int Register()
        {
            try
            {
                return Owner.dbCmd.ExecuteNonQuery(string.Format(SqlAddTableToWatcherTable, Owner.TableWatcherName, Owner.ClientId, this.SourceName));
            }
            catch { }
            return 0;
        }

        public int CreateTableTrigger()
        {
            try
            {

                IDbCmd dbCmd = Owner.dbCmd;// DBFactory.Create(connectionString, Nistec.Data.DBProvider.SqlServer);
                string Database = dbCmd.Connection.Database;
                int res = dbCmd.ExecuteScalar<int>(string.Format(SqlIsTriggerExists, Database, this.SourceName));
                if (res == 1)
                    return 0;
                //if (o == null || o.ToString() == "1")
                //    return 0;
                return dbCmd.ExecuteNonQuery(string.Format(SqlCreateTigger, Database, this.SourceName, Owner.TableWatcherName));
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return 0;
            }
        }
        /// <summary>
        /// Get if is edited
        /// </summary>
        public bool Edited
        {
            get{return m_Edited;}
        }

        /// <summary>
        /// Get Last Sync
        /// </summary>
        public string LastSync
        {
            get { return m_LastSync; }
        }

        public int ResetAll()
        {
            try
            {
                int res = Owner.dbCmd.ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}'", Owner.TableWatcherName, Owner.ClientId));
                m_Edited = false;
                m_LastSync = DateTime.Now.ToString("s");
                return res;
            }
            catch { }
            return 0;
        }

        public int ResetEdited()
        {
            try
            {
                int res= Owner.dbCmd.ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}' and TableName='{2}'", Owner.TableWatcherName,Owner.ClientId,this.SourceName));
                m_Edited = false;
                m_LastSync = DateTime.Now.ToString("s");
                return res;
            }
            catch { }
            return 0;
        }

        public void StoreTableSource()
        {
            try
            {
                Nistec.Caching.CacheLogger.LogWrite("Store Edited : " + this.TableName);

                Owner.Store(GetTableSource(), this.TableName);
                ResetEdited();
                OnSyncSourceChanged(new SyncDataSourceChangedEventArgs(this.TableName));
            }
            catch (Exception ex)
            {
                Owner.OnDalException(ex.Message, DalCacheError.ErrorMergeData);
            }
        }

        public DataTable GetTableSource()
        {
            return Owner.dbCmd.ExecuteDataTable(this.TableName, "SELECT * FROM " + this.MappingName, this.MissingSchemaAction== System.Data.MissingSchemaAction.AddWithKey);
        }

        public int UpdateChanges()
        {
             return UpdateChanges(Owner.DataSource.Tables[this.TableName]);
        }

        public int UpdateChanges(DataTable dt)
        {
            try
            {
                return Owner.dbCmd.Adapter.UpdateChanges(dt, this.MappingName);
            }
            catch (Exception ex)
            {
                Owner.OnDalException(ex.Message, DalCacheError.ErrorUpdateChanges);
                return 0;
            }
        }


    }
}
