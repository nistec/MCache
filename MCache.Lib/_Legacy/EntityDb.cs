using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using System.Xml.Serialization;
using Nistec.Data;
using Nistec.Data.Factory;
using System.Globalization;
using Nistec.Generic;
using Nistec.Data.Entities;

namespace Nistec.Legacy
{
      

     /// <summary>
     /// Represent Entity mapping properties in DB include ConnectionContext
     /// </summary>
     [Serializable]
     public class EntityDb
     {
         #region static 

         /// <summary>
         /// Create EntityDb using new instance of  <see cref="DbContext"/>
         /// </summary>
         /// <typeparam name="Dbc"></typeparam>
         /// <param name="entityName"></param>
         /// <param name="mappingName"></param>
         /// <param name="sourceType"></param>
         /// <param name="keys"></param>
         /// <returns></returns>
         public static EntityDb Create<Dbc>(string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys) where Dbc : IDbContext
         {
             IDbContext db = DbContext.Create<Dbc>();//Activator.CreateInstance<Dbc>();
             return new EntityDb(db, entityName, mappingName, sourceType,keys);
         }

         /// <summary>
         /// Create EntityDb using new instance of  <see cref="DbContext"/>
         /// </summary>
         /// <typeparam name="Dbc"></typeparam>
         /// <param name="mappingName"></param>
         /// <param name="keys"></param>
         /// <returns></returns>
         public static EntityDb Create<Dbc>(string mappingName, EntityKeys keys) where Dbc : IDbContext
         {
             IDbContext db = DbContext.Create<Dbc>();//Activator.CreateInstance<Dbc>();
             return new EntityDb(db, mappingName, mappingName, EntitySourceType.Table, keys);
         }

         /// <summary>
         /// Get EntityDb using current instance of  <see cref="DbContext"/>
         /// </summary>
         /// <typeparam name="Dbc"></typeparam>
         /// <param name="entityName"></param>
         /// <param name="mappingName"></param>
         /// <param name="sourceType"></param>
         /// <param name="keys"></param>
         /// <returns></returns>
         public static EntityDb Get<Dbc>(string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys) where Dbc : IDbContext
         {
             IDbContext db = DbContext.Get<Dbc>();
             return new EntityDb(db, entityName, mappingName, sourceType, keys);
         }

         /// <summary>
         /// Get EntityDb using current instance of  <see cref="DbContext"/>
         /// </summary>
         /// <typeparam name="Dbc"></typeparam>
         /// <param name="mappingName"></param>
         /// <param name="keys"></param>
         /// <returns></returns>
         public static EntityDb Get<Dbc>(string mappingName, EntityKeys keys) where Dbc : IDbContext
         {
             IDbContext db = DbContext.Get<Dbc>();
             return new EntityDb(db, mappingName, mappingName, EntitySourceType.Table, keys);
         }

       

         ///// <summary>
         ///// Create EntityDb
         ///// </summary>
         ///// <typeparam name="Dbc"></typeparam>
         ///// <param name="entityName"></param>
         ///// <returns></returns>
         //public static EntityDb Create<Dbc>(string entityName) where Dbc : IDbContext
         //{
         //    IDbContext db = DbContext.Get<Dbc>();
         //    return db[entityName];
         //}
         #endregion

         #region Dispose
         /*
        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //if (m_DbContext != null)
                    //{
                    //    m_DbContext.Dispose();
                    //    m_DbContext = null;
                    //}
                 }
                DisposeInner(disposing);
                //dispose unmanaged resources here.
                disposed = true;
            }
        }

        protected virtual void DisposeInner(bool disposing)
        {

        }
        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);     
         
            // take this object off the finalization queue     
            GC.SuppressFinalize(this); 
        }

        ~EntityDb()
        {
            Dispose(false);
        }
*/
        #endregion
 
         #region ctor

         /// <summary>
         /// ctor
         /// </summary>
         public EntityDb()
         {
             // m_DbContext = db;
         }


         /// <summary>
         /// ctor
         /// </summary>
         /// <param name="db"></param>
         /// <param name="mappingName"></param>
         /// <param name="keys"></param>
         public EntityDb(IDbContext db, string mappingName, EntityKeys keys)
         {
             //this.m_EntityType = type;
             this.MappingName = mappingName;
             this.EntityName = mappingName;
             m_DbContext = db;

             //m_ConnectionContext = new ConnectionContext(connectionKey);
             m_EntityKeys = keys;// new EntityKeys(keys);
             
         }

         /// <summary>
         /// ctor
         /// </summary>
         /// <param name="db"></param>
         /// <param name="entityName"></param>
         /// <param name="mappingName"></param>
         /// <param name="sourceType"></param>
         /// <param name="keys"></param>
         public EntityDb(IDbContext db, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys)
         {
             //this.m_EntityType = type;
             this.MappingName = mappingName;
             this.EntityName = entityName;
             this.DbSourceType = sourceType;
             m_DbContext = db;
             //m_ConnectionContext = new ConnectionContext(connectionKey);
             m_EntityKeys = keys;// new EntityKeys(keys);
         }


         /// <summary>
         /// ctor
         /// </summary>
         /// <param name="connectionKey"></param>
         /// <param name="entityName"></param>
         /// <param name="mappingName"></param>
         /// <param name="sourceType"></param>
         /// <param name="keys"></param>
         public EntityDb(string connectionKey, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys)
         {
             //this.m_EntityType = type;
             this.MappingName = mappingName;
             this.EntityName = entityName;
             this.DbSourceType = sourceType;
             //m_ConnectionContext = new ConnectionContext(connectionName, connectionString, provider);
             m_EntityKeys = keys;// new EntityKeys(keys);
             if (!string.IsNullOrEmpty(connectionKey))
             {
                 m_DbContext = new DbContext(connectionKey);
             }
         }

         /// <summary>
         /// ctor
         /// </summary>
         /// <param name="connectionName"></param>
         /// <param name="connectionString"></param>
         /// <param name="provider"></param>
         /// <param name="entityName"></param>
         /// <param name="mappingName"></param>
         /// <param name="sourceType"></param>
         /// <param name="keys"></param>
         public EntityDb(string connectionName, string connectionString, DBProvider provider, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys)
         {
             //this.m_EntityType = type;
             this.MappingName = mappingName;
             this.EntityName = entityName;
             this.DbSourceType = sourceType;
             m_EntityKeys = keys;// new EntityKeys(keys);

             if (!string.IsNullOrEmpty(connectionString))
             {
                 m_DbContext = new DbContext(connectionString, provider);
             }
         }

        
         #endregion

         #region properties

         /// <summary>
         /// Get or Set TableName
         /// </summary>
         public string EntityName
         {
             get;
             set;
         }

         /// <summary>
         /// Get or Set MappingName
         /// </summary>
         public string MappingName
         {
             get;
             set;
         }

         private EntitySourceType _DbSourceType = EntitySourceType.Table;
         /// <summary>
         /// Get or Set DbSourceType
         /// </summary>
         public EntitySourceType DbSourceType
         {
             get { return _DbSourceType; }
             set { _DbSourceType = value; }
         }

         /// <summary>
         /// Get <see cref="CommandType"/> acording to <see cref="DbSourceType"/>
         /// </summary>
         internal CommandType CmdType
         {
             get
             {
                 if (DbSourceType == EntitySourceType.Procedure)
                 {
                     return CommandType.StoredProcedure;
                 }
                 return CommandType.Text;
             }
         }

         CultureInfo m_CultureInfo;

         /// <summary>
         /// Get or Set current culture
         /// </summary>
         [EntityProperty(EntityPropertyType.NA)]
         public virtual CultureInfo EntityCulture
         {
             get
             {
                 if (m_CultureInfo == null)
                     return EntityLang.DefaultCulture;
                 return m_CultureInfo;
             }
             set { m_CultureInfo = value; }
         }

         internal EntityProperties m_ControlAttributes;

         /// <summary>
         /// Get EntityProperties
         /// </summary>
         public EntityProperties EntityProperties<T>()
         {

             if (m_ControlAttributes == null)
             {
                 T instance = Activator.CreateInstance<T>();
                 //CultureInfo culture = CultureInfo.CurrentCulture;
                 //if (typeof(IEntityDb).IsAssignableFrom(typeof(T)))
                 //{
                 //    culture = ((IEntityDb)instance).EntityCulture;
                 //}
                 m_ControlAttributes = new EntityProperties(instance, Localization, EntityCulture);
             }
             //CreateEntityAttributes();
             return m_ControlAttributes;
         }

        /// <summary>
        /// Get EntityProperties
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
         public EntityProperties EntityProperties(object instance)
         {

             if (m_ControlAttributes == null)
             {

                 //CultureInfo culture = CultureInfo.CurrentCulture;
                 //if (typeof(IEntityDb).IsAssignableFrom(instance.GetType()))
                 //{
                 //    culture = ((IEntityDb)instance).EntityCulture;
                 //}

                 m_ControlAttributes = new EntityProperties(instance, Localization, EntityCulture);
             }
             //CreateEntityAttributes();
             return m_ControlAttributes;
         }

         /*
         IEntity m_Entity;
         /// <summary>
         /// Get or Set Entity
         /// </summary>
         [XmlIgnore()]
         public IEntity Entity
         {
             get
             {
                 //if (m_Entity == null)
                 //{
                 //    m_Entity = new ActiveEntity();
                 //}
                 return m_Entity;
             }
             internal set { m_Entity = value; }
         }
         */

         EntityKeys m_EntityKeys;
         /// <summary>
         /// Get or Set EntityType
         /// </summary>
         public EntityKeys EntityKeys
         {
             get
             {
                 if (m_EntityKeys == null)
                 {
                     m_EntityKeys = new EntityKeys();
                 }
                 return m_EntityKeys;
             }
             set { m_EntityKeys = value; }
         }

         //CommandType m_CommandType;
         ///// <summary>
         ///// Get or Set Entity CommandType
         ///// </summary>
         //public CommandType EntityCommandType
         //{
         //    get
         //    {
         //        //if (m_CommandType == null)
         //        //{
         //        //    m_EntityKeys = new EntityKeys();
         //        //}
         //        return m_CommandType;
         //    }
         //    set { m_CommandType = value; }
         //}

         IDbContext m_DbContext;
         /// <summary>
         /// Get or Set DbContext
         /// </summary>
         [XmlIgnore]
         public IDbContext Db //Context
         {
             get
             {
                 ValidateConnection();
                 return m_DbContext;
             }
             set { m_DbContext = value; }
         }

         ///// <summary>
         ///// Get <see cref="IEntityLang"/> from <see cref="DbContext"/> which usful for multi language,
         ///// if  EntityDb not define or DbContext not define return null
         ///// </summary>
         //[XmlIgnore]
         //public IEntityLang LangManager
         //{
         //    get
         //    {
         //        if (m_DbContext != null)
         //        {
         //           return m_DbContext.LangManager;//.GetLangManager();
         //        }
         //        return null;
         //    }

         //}
        [XmlIgnore]
        public ILocalizer Localization
        {
            get
            {
                if (m_DbContext != null)
                {
                    return m_DbContext.Localization;//.LangManager;//.GetLangManager();
                }
                return null;
            }

        }

        //internal void SetLangManager(IEntity instance, string resource)
        // {
        //     if (m_DbContext != null)
        //     {
        //         ((DbContext)m_DbContext)..SetLangManager(instance, resource);
        //     }
        // }

        #endregion

        #region List

        public T EntityItem<T>(params object[] keys)
        {
            EntityContext<T> entity = new EntityContext<T>();
            entity.EntityDb = new EntityDbContext(this.Db, this.MappingName, this.EntityKeys);
            entity.SetEntity(keys);
            return entity.Entity;
        }

         public T EntityItem<T>(DataFilter filter)
         {
             EntityContext<T> entity = new EntityContext<T>();
             entity.EntityDb = new EntityDbContext(this.Db, this.MappingName, this.EntityKeys);
             entity.Set(filter);
             return entity.Entity;
         }

         public T EntityItem<T>(string commandText, CommandType cmdType, params IDbDataParameter[] parameters)
         {
             EntityContext<T> entity = new EntityContext<T>();
             entity.EntityDb = new EntityDbContext(this.Db,this.MappingName,this.EntityKeys);
             entity.Set(commandText, parameters, cmdType);
             return entity.Entity;
         }

         /// <summary>
         /// Create Entity collection using Entity Keys
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="keys"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentNullException"></exception>
         public List<T> EntityList<T>(params object[] keys) //where T : IEntityItem
         {
             if (HasConnection)
             {
                 DataTable dt = GetEntityTable(keys);
                 return EntityList<T>(dt);
             }

             return null;
         }

         /// <summary>
         /// Create Entity collection using Entity Keys
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="top"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentNullException"></exception>
         public List<T> EntityList<T>(int top = 0) //where T : IEntityItem
         {
             if (HasConnection)
             {
                 DataTable dt = GetEntityTable(top);
                 return EntityList<T>(dt);
             }

             return null;
         }

         /// <summary>
         /// Create Entity collection using <see cref="DataFilter"/> filter with parameters
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="filter"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentNullException"></exception>
         public List<T> EntityList<T>(DataFilter filter) //where T : IEntityItem
         {
             if (HasConnection)
             {
                 DataTable dt = GetEntityTable(filter);
                 return EntityList<T>(dt);
             }

             return null;
         }

         /// <summary>
         /// Create Entity collection using command
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="commandText"></param>
         /// <param name="parameters"></param>
         /// <param name="cmdType"></param>
         /// <returns></returns>
         public List<T> EntityList<T>(string commandText, IDbDataParameter[] parameters, CommandType cmdType)
         {
             if (HasConnection)
             {
                 DataTable dt = GetEntityTable(commandText, parameters, cmdType);
                 return EntityList<T>(dt);
             }

             return null;
         }

         /// <summary>
         /// Create Entity collection from <see cref="DataTable"/>
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="dt"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentNullException"></exception>
         public static List<T> EntityList<T>(DataTable dt) //where T : IEntityItem
         {
             if (dt == null)
             {
                 throw new ArgumentNullException("CreateList.dt");
             }
             var records = GenericRecord.ParseList(dt);//.CreateRecords(dt);
             List<T> list = new List<T>();
             if (records != null)
             {
                
                 foreach (GenericRecord gr in records)
                 {
                     T item = System.Activator.CreateInstance<T>();
                     EntityPropertyBuilder.SetEntityContext(item, gr);
                     list.Add(item);
                 }

                 return list;
             }
             return null;
         }


 
         /// <summary>
         /// Create Entity collection from <see cref="DataTable"/>
         /// </summary>
         /// <param name="dt"></param>
         /// <param name="type"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentNullException"></exception>
         public static object[] EntityList(DataTable dt, Type type) //where T : IEntityItem
         {
             if (dt == null)
             {
                 throw new ArgumentNullException("CreateList.dt");
             }
            var records = GenericRecord.ParseList(dt);
            List<object> list = new List<object>();
             if (records != null)
             {
                 foreach (GenericRecord gr in records)
                 {
                     object item = System.Activator.CreateInstance(type);
                     EntityPropertyBuilder.SetEntityContext(item, gr);
                     list.Add(item);
                 }

                 return list.ToArray();
             }
             return null;
         }

         /// <summary>
         /// Create Entity from <see cref="DataRow"/>
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="dr"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentNullException"></exception>
         public static T EntityItem<T>(DataRow dr) //where T : IEntityItem
         {
             if (dr == null)
             {
                 throw new ArgumentNullException("CreateEntity.dr");
             }
             GenericRecord record = new GenericRecord(dr);

             if (record != null)
             {
                 T item = System.Activator.CreateInstance<T>();
                 EntityPropertyBuilder.SetEntityContext(item, record);
                 return item;
             }
             return default(T);
         }

         /// <summary>
         /// Create Entity from <see cref="DataRow"/>
         /// </summary>
         /// <param name="dr"></param>
         /// <param name="type"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentNullException"></exception>
         public static object EntityItem(DataRow dr, Type type)
         {
             if (dr == null)
             {
                 throw new ArgumentNullException("CreateEntity.dr");
             }
             GenericRecord record = new GenericRecord(dr);

             if (record != null)
             {
                 object item = System.Activator.CreateInstance(type);
                 EntityPropertyBuilder.SetEntityContext(item, record);
                 return item;
             }
             return null;
         }
         #endregion

         #region IEntity

         /*
         public GenericDataTable GetEntityDataSource()
         {
             GenericDataTable dt = GenericDataTable.Convert(GetDataTable());
             if (dt == null)
             {
                 return null;
             }
             return dt;// new GenericData(dt);
         }

         public GenericDataTable GetEntityDataSource(string filter)
         {
             if (string.IsNullOrEmpty(filter))
             {
                 throw new ArgumentNullException("filter");
             }
             
             return GenericDataTable.Convert(GetDataTable("select * from " + MappingName + " where " + filter));
             //if (dt == null)
             //{
             //    return null;
             //}
             //dt.TableName = MappingName;
             //return dt;//ew GenericData(dt);
         }

         public GenericDataTable GetEntityDataSource(object[] keys)
         {
             GenericDataTable dt = GenericDataTable.Convert(GetEntityTable(keys));
             if (dt == null)
             {
                 return null;
             }
             return dt;// new GenericData(dt);
         }
         */

  
         /*
         public IEntity GetEntity(DataRow dr)
         {
             ActiveEntity entity = null;
             try
             {
                 entity = new ActiveEntity(dr);
                 entity.InitEntity(this);
                 m_Entity = entity;
                 return entity;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }

         public IEntity GetEntity(params object[] keys)
         {
             ActiveEntity entity = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     DataRow dr = GetEntityRow(keys);

                     entity = new ActiveEntity(dr);
                     entity.InitEntity(this);
                     m_Entity = entity;
                 }

                 return entity;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }
         

         public IDataEntity GetEntity(DataParameter[] prm)
         {
             ActiveEntity entity = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     string commandText = GetCommandText(true);
                     DataRow dr = null;

                     //if (EntityCommandType == CommandType.StoredProcedure)
                     //    dr = cmd.ExecuteCommand<DataRow>(commandText, EntityCommandType, 0, prm, true);
                     //else
                         dr = cmd.ExecuteCommand<DataRow>(commandText, CmdType,0, prm, true);

                     entity = new ActiveEntity(dr);
                     entity.InitEntity(this);
                     m_Entity = entity;
                 }

                 return entity;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }
         */
         #endregion

         #region internal methods

         public GenericDataTable EntityDataSource(params object[] keys)
         {

             DataTable dt = GetEntityTable(keys);

             return new GenericDataTable(dt);

         }

         internal DataRow GetEntityRow(object[] keys)
         {
             DataRow dr = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     string commandText = GetCommandText(keys != null);
                     DataParameter[] prm = GetCommandParam(keys);
                     //if (EntityCommandType == CommandType.StoredProcedure)
                     //    dr = cmd.ExecuteCommand<DataRow>(commandText, EntityCommandType, 0, prm, true);
                     //else
                     dr = cmd.ExecuteCommand<DataRow>(commandText, prm, CmdType, 0, true);

                 }

                 return dr;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }

         internal GenericRecord EntityRrecord(object[] keys)
         {
             DataRow dr = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     string commandText = GetCommandText(keys != null);
                     DataParameter[] prm = GetCommandParam(keys);
                     //if (EntityCommandType == CommandType.StoredProcedure)
                     //    dr = cmd.ExecuteCommand<DataRow>(commandText, EntityCommandType, 0, prm, true);
                     //else
                     dr = cmd.ExecuteCommand<DataRow>(commandText, prm, CmdType, 0, true);

                 }

                 return new GenericRecord(dr);
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }

         internal GenericRecord EntityRrecord(DataFilter filter)
         {
             DataRow dr = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     string strWhere = filter == null ? null : filter.Filter;
                     IDbDataParameter[] parameters = filter == null ? null : filter.Parameters;
                     string commandText = GetCommandText(strWhere);
                     dr = cmd.ExecuteCommand<DataRow>(commandText, parameters, CmdType, 0, true);
                 }

                 return new GenericRecord(dr) ;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }

         internal GenericRecord EntityRrecord(string commandText, IDbDataParameter[] parameters, CommandType cmdType)
         {
             DataRow dr = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     dr = cmd.ExecuteCommand<DataRow>(commandText, parameters, CmdType, 0, true);
                 }

                 return new GenericRecord(dr);
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }

         internal DataTable GetEntityTable(int top)
         {
             DataTable dt = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     string commandText = GetCommandText(top);
                     DataParameter[] prm = null;
                     dt = cmd.ExecuteCommand<DataTable>(commandText, prm, CmdType, 0, true);
                 }

                 return dt;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }
         internal DataTable GetEntityTable(object[] keys)
         {
             DataTable dt = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     string commandText = GetCommandText(keys != null);
                     DataParameter[] prm = GetCommandParam(keys);
                     //if (EntityCommandType == CommandType.StoredProcedure)
                     //    dt = cmd.ExecuteCommand<DataTable>(commandText, EntityCommandType, 0, prm, true);
                     //else
                     dt = cmd.ExecuteCommand<DataTable>(commandText, prm, CmdType, 0, true);
                 }

                 return dt;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }

         internal DataTable GetEntityTable(DataFilter filter)
         {
             DataTable dt = null;
             ValidateConnection();
             try
             {
                 string strWhere = filter == null ? null : filter.Filter;
                 IDbDataParameter[] parameters = filter == null ? null : filter.Parameters;
                 using (IDbCmd cmd = DbCmd())
                 {
                     string commandText = GetCommandText(strWhere);
                     dt = cmd.ExecuteCommand<DataTable>(commandText, parameters, CmdType, 0, true);
                 }

                 return dt;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }

         internal DataTable GetEntityTable(string commandText, IDbDataParameter[] parameters, CommandType cmdType)
         {
             DataTable dt = null;
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     dt = cmd.ExecuteCommand<DataTable>(commandText, parameters, CmdType, 0, true);
                 }

                 return dt;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }

         internal T GetEntityData<T>(object[] keys)
         {
             T dt = default(T);
             ValidateConnection();
             try
             {
                 using (IDbCmd cmd = DbCmd())
                 {
                     string commandText = GetCommandText(keys != null);
                     DataParameter[] prm = GetCommandParam(keys);
                     //if (EntityCommandType == CommandType.StoredProcedure)
                     //    dt = cmd.ExecuteCommand<DataTable>(commandText, EntityCommandType, 0, prm, true);
                     //else
                     dt = cmd.ExecuteCommand<T>(commandText, prm, CmdType, 0, true);
                 }

                 return dt;
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }
        #endregion

         #region IDbCmd methods

         /// <summary>
         /// Get indicate if entity has connection properties
         /// </summary>
         [EntityProperty(EntityPropertyType.NA)]
         public bool HasConnection
         {
             get
             {
                 if (m_DbContext != null && m_DbContext.HasConnection && !string.IsNullOrEmpty(MappingName))
                 {
                     return true;
                 }
                 return false;
             }
         }

         /// <summary>
         /// Validate if entity has connection properties
         /// </summary>
         /// <exception cref="EntityException"></exception>
         public void ValidateConnection()
         {
             if (m_DbContext == null || !m_DbContext.HasConnection && string.IsNullOrEmpty(MappingName))
             {
                 throw new EntityException("Invalid MappingName or ConnectionContext");
             }
         }

         /// <summary>
         /// Get Db command
         /// </summary>
         /// <returns></returns>
         public IDbCmd DbCmd()
         {
             ValidateConnection();
             return m_DbContext.NewCmd();
         }

         /// <summary>
         /// Get all entity values from DB as DataTable
         /// </summary>
         /// <returns></returns>
         public DataTable GetDataTable()
         {

             IDbCmd cmd = DbCmd();
             return cmd.ExecuteDataTable(MappingName, true);

         }

         
         /// <summary>
         /// Get entity values with sql expression from DB as DataTable
         /// </summary>
         /// <param name="sql"></param>
         /// <returns></returns>
         public DataTable GetDataTable(string sql)
         {
             IDbCmd cmd = DbCmd();
             return cmd.ExecuteDataTable(EntityName, sql, true);

         }

         /*
         /// <summary>
         /// Get entity values with filter expression from DB as DataRow
         /// </summary>
         /// <param name="filter"></param>
         /// <returns></returns>
         internal DataRow GetDataRow(string filter)
         {
             string sql = SqlFormatter.SelectString("*",MappingName, filter);
             DataTable dt = GetDataTable(sql);
             if (dt == null || dt.Rows.Count == 0)
             {
                 return null;
             }
             return dt.Rows[0];
         }
         */
         
         /// <summary>
         /// Get sql command text
         /// </summary>
         /// <returns></returns>
         internal string GetCommandText(bool addWhere)
         {
             if (string.IsNullOrEmpty(MappingName))
             {
                 throw new EntityException("Invalid MappingName");
             }
             if (CmdType == CommandType.StoredProcedure)
             {
                 return MappingName;
             }
             string where = "";
             if (addWhere)
             {
                 if (m_EntityKeys == null)
                 {
                     throw new EntityException("Invalid Entity Keys");
                 }
                 where = SqlFormatter.CommandWhere(m_EntityKeys.ToArray(), false);
             }
             return SqlFormatter.SelectString("*", MappingName, where);
         }

         internal string GetCommandText(int top)
         {
             if (string.IsNullOrEmpty(MappingName))
             {
                 throw new EntityException("Invalid MappingName");
             }
             if (CmdType == CommandType.StoredProcedure)
             {
                 return MappingName;
             }

             return SqlFormatter.SelectString(top, "*", MappingName, null);
         }
         
         internal string GetCommandText(string where)
         {
             if (string.IsNullOrEmpty(MappingName))
             {
                 throw new EntityException("Invalid MappingName");
             }
             if (CmdType == CommandType.StoredProcedure)
             {
                 return MappingName;
             }
             return SqlFormatter.SelectString("*", MappingName, where);
         }

         /// <summary>
         /// Get sql command parameters
         /// </summary>
         /// <returns></returns>
         private DataParameter[] GetCommandParam(params object[] keys)
         {
             if (string.IsNullOrEmpty(MappingName))
             {
                 throw new Exception("Invalid MappingName");
             }
             if (keys == null)
             {
                 return null;
             }
             int count = EntityKeys.Count;
             //Dictionary<string, object> keysValue = new Dictionary<string, object>();

             List<DataParameter> prm = new List<DataParameter>();
             //int index = 0;
             for (int i = 0; i < count; i++)
             {
                 prm.Add(new DataParameter(EntityKeys[i], keys[i]));
                 //keysValue[EntityKeys[i]] = keys[i];
                 //prm[i + index] = EntityKeys[i];
                 //prm[i + index + 1] = keys[i];
                 //index++;
             }

             return prm.ToArray();
         }

         #endregion

         #region internal static

         /// <summary>
         /// Create list of entities that implement IEntity
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <returns></returns>
         internal IEnumerable<T> ToEntities<T>() where T : IEntity
         {
             DataTable dt = GetDataTable();
             return GetEntities<T>(dt);
         }
         /// <summary>
         /// Create list of entities that implement IDataEntity
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="sql"></param>
         /// <returns></returns>
         internal IEnumerable<T> ToEntities<T>(string sql) where T : IEntity
         {
             DataTable dt = GetDataTable(sql);
             return GetEntities<T>(dt);
         }

         /// <summary>
         /// Create list of entities that implement IDataEntity
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="dt"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentNullException"></exception>
         /// <exception cref="EntityException"></exception>
         internal static IEnumerable<T> GetEntities<T>(DataTable dt) where T : IEntity
         {
             if (dt == null)
             {
                 throw new ArgumentNullException("dt");
             }

             try
             {

                 List<T> entities = new List<T>();

                 string[] fields = DataUtil.GetTableFields(dt);

                 if (fields == null)
                     return null;

                foreach (DataRow dr in dt.Rows)
                {

                    GenericRecord gv = new GenericRecord(dr, fields);//.Create(dr, fields);
                    T instance = System.Activator.CreateInstance<T>();
                    ((IEntity)instance).EntityRecord = gv;
                    entities.Add(instance);
                }

                 return entities;//.ToArray();
             }
             catch (Exception ex)
             {
                 throw new EntityException(ex.Message);
             }
         }


         #endregion

     }


}
