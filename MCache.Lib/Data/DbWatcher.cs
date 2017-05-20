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
using System.Collections.Generic;
using System.ComponentModel;
using Nistec.Data;
using Nistec.Data.Factory;
using Nistec.Data.Entities;

namespace Nistec.Caching.Data
{

	/// <summary>
	/// Summary description for ActiveWatcher.
	/// </summary>
    internal class DbWatcher : EntityView //ActiveView
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
@"CREATE   TRIGGER trgw_{3}
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
        IDataCache Owner;
        #endregion

        #region Ctor

        internal DbWatcher(IDataCache dal)
        {
            this.Owner = dal;
            
        }
		#endregion


        public new int Select(string tableName)
        {
            int indx = base.Find(new object[]{tableName});
            if (indx > -1)
            {
                base.EntityDataSource.Index = indx;
            }
            return indx;
        }

        public bool IsExists(string tableName)
        {
            int indx = base.Find(new object[] { tableName });
            return (indx > -1) ? true : false;
        }
   
        public DataTable TableWatcherName
        {
            get { return base.EntityDataSource; }
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
            get { return base.GetValue<string>("ClientID"); }
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

        public bool GetEdited(string[] tables)
        {
            try
            {
                foreach (string sn in tables)
                {
                    int index = Select(sn);
                    if (index < 0)
                    {
                        CacheLogger.Info("Select " + sn + " not found");
                    }
                    else
                    {
                        if (Edited)
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("DbWatcher GetEdited Error: " + ex.Message);
            }
            return false;
        }

        public bool GetEdited(string tableName)
        {
            try
            {
                
                int index = Select(tableName);
                if (index < 0)
                {
                    CacheLogger.Info("Select " + tableName +" not found");
                    return false;
                }
                return Edited;
            }
            catch(Exception ex) 
            {
                CacheLogger.Error("DbWatcher GetEdited Error: " + ex.Message);
            }
            return false;
        }

        public int UpdateEdited(string tableName)
        {
            try
            {
                using (IDbCmd dbCmd = Owner.Db.NewCmd())
                {
                    return dbCmd.ExecuteNonQuery(string.Format("UPDATE {0} SET Edited=0 WHERE ClientId='{1}' and TableName='{2}'", Owner.TableWatcherName, Owner.ClientId, tableName));
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("DbWatcher UpdateEdited Error: " + ex.Message);
            }
            return 0;
        }

        public int Register(string tableName)
        {
            try
            {
                using (IDbCmd dbCmd = Owner.Db.NewCmd())
                {
                    return dbCmd.ExecuteNonQuery(string.Format(SqlAddTableToWatcherTable, Owner.TableWatcherName, Owner.ClientId, tableName));
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("DbWatcher Register Error: " + ex.Message);
            }
            return 0;
        }

        public void Register(string[] tables)
        {
            RegisterTablesTrigger(Owner.Db, tables, Owner.ClientId, Owner.TableWatcherName);
        }

        public void Register(string[] tables, bool createTriggers)
        {
            if (createTriggers)
            {
                CreateTablesTrigger(Owner.Db, tables, Owner.TableWatcherName);
            }
            RegisterTablesTrigger(Owner.Db, tables, Owner.ClientId, Owner.TableWatcherName);
        }

        public int CreateTableTrigger(string tableName)
        {
            try
            {
                using (IDbCmd dbCmd = Owner.Db.NewCmd())
                {
                    string Database = dbCmd.Connection.Database;
                    int res = dbCmd.ExecuteScalar<int>(string.Format(SqlIsTriggerExists, Database, tableName));
                    if (res == 1)
                        return 0;
                    return dbCmd.ExecuteNonQuery(string.Format(SqlCreateTigger, Database, tableName, Owner.TableWatcherName));
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Error("DbWatcher CreateTableTrigger Error: " + ex.Message);
            }

            return 0;
        }

        public override void Refresh()
        {
            try
            {
                DataTable dt = null; ;
                using (IDbCmd dbCmd = Owner.Db.NewCmd())
                {
                    dt = dbCmd.ExecuteDataTable(Owner.TableWatcherName, string.Format("SELECT * FROM {0} WHERE ClientId='{1}'", Owner.TableWatcherName, Owner.ClientId), false);
                }
                if (dt == null)
                {
                    throw new Exception("Could not load data table watche: " + Owner.TableWatcherName);
                }
                base.Init(dt, false);
                if (base.IsEmpty)
                {
                    CacheLogger.Info("TableWatcher is empty");
                    return;
                }
                base.View.Sort="TableName";
            }
            catch (Exception ex)
            {
                Owner.RaiseException(ex.Message, DataCacheError.ErrorSyncCache);
            }
        }


		#endregion

        #region static

        public static int CreateTableWatcher(IDbContext db)
        {
            return CreateTableWatcher(db, DefaultWatcherName);
        }
        public static int CreateTableWatcher(IDbContext db, string TableWatcherName)
        {
            try
            {
                using (IDbCmd dbCmd = db.NewCmd())
                {
                    string Database = dbCmd.Connection.Database;
                    int res = dbCmd.ExecuteScalar<int>(string.Format(SqlIsTableWatcherExists, Database, TableWatcherName));
                    if (res == 1)
                        return res;
                    string cmd = string.Format(SqlCreateTableWatcher, Database, TableWatcherName);
                    return dbCmd.ExecuteNonQuery(cmd);
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 0;
            }
        }

        public static int CreateTableWatcher(IDbCmd dbCmd)
        {
          return  CreateTableWatcher(dbCmd, DefaultWatcherName);
        }

        public static int CreateTableWatcher(IDbCmd dbCmd, string TableWatcherName)
        {
            try
            {
                string Database = dbCmd.Connection.Database;
                int res= dbCmd.ExecuteScalar<int>(string.Format(SqlIsTableWatcherExists, Database, TableWatcherName));
                if (res == 1)
                    return res;
                string cmd = string.Format(SqlCreateTableWatcher, Database, TableWatcherName);
                return dbCmd.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 0;
            }
        }

 
        public static void CreateTablesTrigger(IDbContext db, string[] tables, string tableWatcher)
        {

            List<string> commands = new List<string>();
            IDbCmd dbCmd = null;
            try
            {
                dbCmd = db.NewCmd();
                object[] results = null;
                string[] exists = new string[tables.Length];


                string Database = dbCmd.Connection.Database;
                for (int i = 0; i < tables.Length; i++)
                {
                    exists[i] = string.Format(SqlIsTriggerExists, Database, tables[i]);
                }

                results = dbCmd.MultiExecuteScalar(exists, false);
                int index = -1;
                foreach (object o in results)
                {
                    index++;
                    if (o == null || o.ToString() == "1")
                    {
                        continue;
                    }
                    string triggerName = tables[index].Replace(".", "_");
                    string cmd = string.Format(SqlCreateTigger, Database, tables[index], tableWatcher, triggerName);
                    if (commands.Contains(cmd))
                        continue;
                    commands.Add(cmd);
                }

                if (commands.Count > 0)
                {
                    dbCmd.MultiExecuteNonQuery(commands.ToArray(), false);
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.DataCacheError, CacheActionState.Error, "CreateTablesTrigger error:" + ex.Message);
            }
            finally
            {
                if (dbCmd != null)
                {
                    dbCmd.Dispose();
                    dbCmd = null;
                }
            }

        }

        public static void RegisterTablesTrigger(IDbContext db, string[] tables, string clientId, string tableWatcher)
        {
            try
            {
                string[] registers = new string[tables.Length];
                for (int i = 0; i < tables.Length; i++)
                {
                    registers[i] = string.Format(SqlAddTableToWatcherTable, tableWatcher, clientId, tables[i]);
                }
                using (IDbCmd dbCmd = db.NewCmd())
                {
                    dbCmd.MultiExecuteNonQuery(registers, false);
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.DataCacheError, CacheActionState.Error, "RegisterTablesTrigger error:" + ex.Message);
            }
        }


        public static void CreateTablesTrigger(IDbCmd dbCmd, string[] tables, string tableWatcher)
        {

            List<string> commands = new List<string>();

            try
            {
                object[] results = null;
                string[] exists = new string[tables.Length];
                

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
                    string triggerName = tables[index].Replace(".","_");
                    string cmd = string.Format(SqlCreateTigger, Database, tables[index], tableWatcher, triggerName);
                    if (commands.Contains(cmd))
                        continue;
                    commands.Add(cmd);
                }

                if (commands.Count > 0)
                {
                    dbCmd.MultiExecuteNonQuery(commands.ToArray(), false);
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

        #endregion

    }
}
