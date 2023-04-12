using Nistec.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Legacy
{
    public class DbCache<T> : Dictionary<string, T>
    {

    }

    public class EntityDbCache : DbCache<EntityDb>
    {
        IDbContext context;

        public EntityDbCache(IDbContext context)
        {
            this.context = context;
        }


        /// <summary>
        /// Get or Create <see cref="EntityDb"/>
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="entityKeys"></param>
        /// <param name="enableCache"></param>
        /// <returns></returns>
        public EntityDb Get(string entityName, string mappingName, EntitySourceType sourceType, EntityKeys entityKeys, bool enableCache=true)
        {
            EntityDb db = null;
            if (enableCache)
            {
                if (this.TryGetValue(entityName, out db))
                {
                    return db;
                }
                db = new EntityDb(this.context, entityName, mappingName, sourceType, entityKeys);
                this[entityName] = db;
                return db;
            }
            return new EntityDb(this.context, entityName, mappingName, sourceType, entityKeys); ;
        }

        /// <summary>
        /// Get or Create <see cref="EntityDb"/> using EntitySourceType.Table
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="entityKeys"></param>
        /// <param name="enableCache"></param>
        /// <returns></returns>
        public EntityDb Get(string mappingName, EntityKeys entityKeys, bool enableCache = true)
        {
            EntityDb db = null;
            if (enableCache)
            {
                if (this.TryGetValue(mappingName, out db))
                {
                    return db;
                }
                db = new EntityDb(this.context, mappingName, mappingName, EntitySourceType.Table, entityKeys);
                this[mappingName] = db;
                return db;
            }
            return new EntityDb(this.context, mappingName, mappingName, EntitySourceType.Table, entityKeys); ;
        }

        /// <summary>
        /// Set Entity using <see cref="EntityAttribute"/>
        /// </summary>
        /// <typeparam name="Dbe"></typeparam>
        public void Set<Dbe>() where Dbe : IEntity
        {
            EntityDb Db = CacheDataExtention.GetEntityDb<Dbe>(EntityLang.DefaultCulture);
            if (Db == null)
            {
                throw new EntityException("Could not create EntityDb, Entity attributes is incorrect");
            }
            Set(Db);

            //IEntity entity = System.Activator.CreateInstance<Dbe>();
            //SetEntity(entity);
        }

        public void Set(IEntity entity)//, string tableName, string mappingName)
        {
            //EntityDb db = entity.EntityDb;//(tableName, mappingName);
            //db.EntityType = type;
            Set(entity.EntityDb);
        }

        public void Set(EntityDb entity)
        {
            this[entity.EntityName] = entity;
        }
        public void Set(EntityDbContext entity)
        {
            this[entity.EntityName] = new EntityDb(this.context, entity.EntityName, entity.MappingName, entity.SourceType, entity.EntityKeys); ;
        }
        /// <summary>
        /// Set EntityDb
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourseType"></param>
        /// <param name="keys"></param>
        public void Set(string tableName, string mappingName, EntitySourceType sourseType, EntityKeys keys)
        {
            this[tableName] = new EntityDb(this.context, tableName, mappingName, sourseType, keys);
        }

        /// <summary>
        /// Set EntityDb with EntitySourceType.Table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        public void Set(string tableName, string mappingName, EntityKeys keys)
        {
            this[tableName] = new EntityDb(this.context, tableName, mappingName, EntitySourceType.Table, keys);
        }

        public void RemoveEntity(EntityDb entity)
        {
            if (this.ContainsKey(entity.EntityName))
                this.Remove(entity.EntityName);
        }

    }

    
}
