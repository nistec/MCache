using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Nistec.Data.Common;
using Nistec.Data;
using Nistec.Data.Factory;

namespace Nistec.Caching.Data
{

	/// <summary>
	/// Summary description for ActiveWatcher.
	/// </summary>
    internal class ActiveWatcher : Nistec.Data.Entities.EntityView// ActiveView
    {

        #region constans

        public const string DefaultWatcherName = "Mc_Watcher";
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
        /// Sql text to check if table watcher all ready exists 
        /// Parameters are 0=Database 1=TabelWatcher name
        /// </summary>
        public const string SqlIsTableWatcherExists =
@"USE {0}

IF EXISTS (select * from 
dbo.sysobjects where id = object_id(N'[dbo].[{1}]') 
and OBJECTPROPERTY(id, N'IsUserTable') = 1)

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
        /// Sql Text to Create TableWatcherName
        /// Parameters are 0=Database name 1=TableWatcherName
        /// </summary>
        public const string SqlCreateTableWatcher =
@"USE {0}
CREATE TABLE [dbo].[{1}] (
	[ClientId] [varchar] (50) NOT NULL ,
	[TableName] [varchar] (50) NOT NULL ,
	[Edited] [bit] NOT NULL ,
	[LastUpdate] [timestamp] NOT NULL 
) ON [PRIMARY]
--GO

ALTER TABLE [dbo].[{1}] WITH NOCHECK ADD 
	CONSTRAINT [PK_{1}] PRIMARY KEY  CLUSTERED 
	(
		[ClientId],
		[TableName]
	)  ON [PRIMARY] 
--GO

ALTER TABLE [dbo].[{1}] ADD 
	CONSTRAINT [DF_{1}_Edited] DEFAULT (0) FOR [Edited]
--GO
";

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

        #endregion

        #region members
        //string m_TableWatcher = "Mc_Watcher";
        //string m_ClientId;
        DalCache Owner;
        #endregion

        #region Ctor

        internal ActiveWatcher(DalCache dal)
        {
            this.Owner = dal;
            //this.m_ClientId = dal.ClientId;
            //m_TableWatcher = dal.TableWatcherName;
            //Refresh();

        }
		#endregion


        public new int Select(string tableName)
        {
            int indx = base.Find(tableName);
            if (indx > -1)
            {
                base.Position = indx;
            }
            return indx;
        }

        public bool IsExists(string tableName)
        {
            int indx = base.Find(tableName);
            return (indx > -1) ? true : false;
        }
   
        public DataTable TableWatcherName
        {
            get { return base.DataSource; }
        }

        public DataRow GetRow(string tableName)
        {
            DataRowView[] drv = base.View.FindRows(tableName);
            if (drv != null && drv.Length > 0)
                return drv[0].Row;
            return null;
        }

 		#region Properties


        public string ClientID
		{
            get { return GetValue<string>("ClientID"); }
		}
        public string TableName
		{
            get { return GetValue<string>("TableName"); }
		}
        public bool Edited
        {
            get { return GetValue<bool>("Edited"); }
        }
        public DateTime LastUpdate
        {
            get { return GetValue<DateTime>("LastUpdate"); }
        }

		#endregion

		#region Methods

        public bool GetEdited(string tableName)
        {
            try
            {
                
                int index = Select(tableName);
                if (index < 0)
                {
                    CacheLogger.Error("Select " + tableName +" not found");
                    return false;
                }
                return Edited;
            }
            catch(Exception ex) 
            {
                CacheLogger.Error("GetEdited Error: " + ex.Message);
            }
            return false;
        }

        public int UpdateEdited(string tableName)
        {
            try
            {
                return Owner.dbCmd.ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}' and TableName='{2}'", Owner.TableWatcherName, Owner.ClientId,tableName));
            }
            catch { }
            return 0;
        }

        public int Register(string tableName)
        {
            try
            {
                return Owner.dbCmd.ExecuteNonQuery(string.Format(SqlAddTableToWatcherTable, Owner.TableWatcherName, Owner.ClientId, tableName));
            }
            catch { }
            return 0;
        }

        public void Register(string[] tables)
        {
            RegisterTablesTrigger(Owner.dbCmd, tables, Owner.ClientId, Owner.TableWatcherName);
        }

        public void Register(string[] tables, bool createTriggers)
        {
            if (createTriggers)
            {
                CreateTablesTrigger(Owner.dbCmd, tables, Owner.TableWatcherName);
            }
            RegisterTablesTrigger(Owner.dbCmd, tables, Owner.ClientId, Owner.TableWatcherName);
        }

        public int CreateTableTrigger(string tableName)
        {
            try
            {
                IDbCmd dbCmd = Owner.dbCmd;
                string Database = dbCmd.Connection.Database;
                int res= dbCmd.ExecuteScalar<int>(string.Format(SqlIsTriggerExists, Database, tableName));
                if (res == 1)
                    return 0;
                //if (o == null || o.ToString() == "1")
                //    return 0;
                return dbCmd.ExecuteNonQuery(string.Format(SqlCreateTigger, Database, tableName, Owner.TableWatcherName));
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 0;
            }
        }

        public override void Refresh()
        {
            try
            {
                DataTable dt = Owner.dbCmd.ExecuteDataTable(Owner.TableWatcherName, string.Format("SELECT * FROM {0} WHERE ClientId='{1}'", Owner.TableWatcherName, Owner.ClientId),false);
               
                base.Init(dt, false);
                if (base.IsEmpty)
                {
                    CacheLogger.Error("TableWatcher is emtpy");
                    return;
                }
                base.View.Sort="TableName";
            }
            catch (Exception ex)
            {
                Owner.OnDalException(ex.Message, DalCacheError.ErrorSyncCache);
                //throw new Exception("Could not load  ActiveWatcher" + ex.Message);
            }
        }


		#endregion

        public static int CreateTableWatcher(IDbCmd dbCmd)
        {
          return  CreateTableWatcher(dbCmd, DefaultWatcherName);
        }
        public static int CreateTableWatcher(IDbCmd dbCmd, string TableWatcherName)
        {
            try
            {
                //IDbCmd dbCmd = DBFactory.Create(connectionString, MControl.Data.DBProvider.SqlServer);
                string Database = dbCmd.Connection.Database;
                int res= dbCmd.ExecuteScalar<int>(string.Format(SqlIsTableWatcherExists, Database, TableWatcherName));
                if (res == 1)
                    return res;
                //if (o == null )
                //    return 0;
                //if (o.ToString() == "1")
                //    return 1;
                string cmd = string.Format(SqlCreateTableWatcher, Database, TableWatcherName);
                return dbCmd.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 0;
            }
        }

 
   
        //public static int CreateTableTrigger(IDbCmd dbCmd, string[] TablesName, string TableWatcherName)
        //{
        //    //IDbCmd dbCmd = DBFactory.Create(connectionString, MControl.Data.DBProvider.SqlServer);
        //    string Database = dbCmd.Connection.Database;
        //    int count = 0;
        //    foreach (string table in TablesName)
        //    {
        //        object o = dbCmd.ExecuteScalar(string.Format(SqlIsTriggerExists, Database, table));
        //        if (o == null || o.ToString() == "1")
        //            continue;
        //        count+= dbCmd.ExecuteNonQuery(string.Format(SqlCreateTigger, Database, table, TableWatcherName));
        //    }
        //    return count;
        //}

        public static void CreateTablesTrigger(IDbCmd dbCmd, string[] tables, string tableWatcher)
        {
   
            try
            {
                //IDbCmd dbCmd = m_DalCache.dbCmd;
                object[] results = null;
                string[] exists = new string[tables.Length];
                List<string> commands = new List<string>();

                string Database = dbCmd.Connection.Database;
                for (int i = 0; i < tables.Length; i++)
                {
                    exists[i] = string.Format(SqlIsTriggerExists, Database, tables[i]);
                }

                results = dbCmd.MultiExecuteScalar(exists,false);
                int index = -1;
                foreach (object o in results)
                {
                    index++;
                    if (o == null || o.ToString() == "1")
                    {
                        continue;
                    }
                    commands.Add(string.Format(SqlCreateTigger, Database, tables[index], tableWatcher));
                }
                if (commands.Count > 0)
                {
                    dbCmd.MultiExecuteNonQuery(commands.ToArray(),false);
                }
            }
            catch { }
        }

        public static void RegisterTablesTrigger(IDbCmd dbCmd, string[] tables, string clientId, string tableWatcher)
        {

            try
            {
                string[] registers = new string[tables.Length];
                for (int i = 0; i < tables.Length; i++)
                {
                    registers[i] = string.Format(SqlAddTableToWatcherTable, tableWatcher, clientId, tables[i]);
                }

                dbCmd.MultiExecuteNonQuery(registers,false);
            }
            catch { }
        }

    }
}
