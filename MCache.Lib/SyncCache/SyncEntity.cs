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
using System.Data;
using Nistec.Generic;
using Nistec.Data.Entities;
using Nistec.Runtime;
using Nistec.Caching.Data;
using System.Xml;
using Nistec.Xml;
using Nistec.Data.Factory;
using Nistec.Caching.Sync.Embed;
using Nistec.Serialization;
using System.Collections;

namespace Nistec.Caching.Sync
{
   
    /// <summary>
    /// Represent Synchronize Entity, Hold all the Synchronization settings.
    /// </summary>
    [Serializable]
    public class SyncEntity:  IDisposable
    {

        #region members
        
        /// <summary>
        /// Default entity type.
        /// </summary>
        public const string DefaultEntityType = "GenericEntity";
         
        /// <summary>
        /// Get or Set ConnectionKey
        /// </summary>
        public string ConnectionKey { get; set; }
        /// <summary>
        /// Get EntityName
        /// </summary>
        public string EntityName { get; internal set; }
        /// <summary>
        /// Get ViewName
        /// </summary>
        public string ViewName { get; internal set; }
         /// <summary>
        /// Get SourceName
        /// </summary>
        public string[] SourceName { get; internal set; }
        /// <summary>
        /// Get PreserveChanges
        /// </summary>
        public bool PreserveChanges { get; internal set; }
        /// <summary>
        /// Get MissingSchemaAction
        /// </summary>
        public MissingSchemaAction MissingSchemaAction { get; internal set; }
       
        /// <summary>
        /// Get syncTime
        /// </summary>
        public TimeSpan Interval { get; internal set; }
        /// <summary>
        /// Get syncTime
        /// </summary>
        public SyncType SyncType { get; internal set; }

        /// <summary>
        /// Get SourceType
        /// </summary>
        public EntitySourceType SourceType { get; internal set; }
         /// <summary>
        /// Get EntityKeys
        /// </summary>
        public string[] EntityKeys { get; internal set; }
        /// <summary>
        /// Get EntityType such as (GenericEntity,EntityContext)
        /// </summary>
        public string EntityType { get; internal set; }

        /// <summary>
        /// Get indicate whether cache should use with nolock statement.
        /// </summary>
        public bool EnableNoLock{ get; internal set; }

        /// <summary>
        /// Get the command timeout in seconds.
        /// </summary>
        public int CommandTimeout { get; internal set; }

        #endregion

        #region ctor
        /// <summary>
        /// Initialize a new instance of sync entity.
        /// </summary>
        public SyncEntity()
        {
            EntityType = DefaultEntityType;
            SyncType = Caching.SyncType.None;
            Interval = TimeSpan.Zero;
            MissingSchemaAction = System.Data.MissingSchemaAction.Add;
            PreserveChanges = false;
            SourceType = EntitySourceType.Table;
            EnableNoLock = false;
            CommandTimeout = 0;
        }

        internal SyncEntity(XmlTable xml)
        {

            ConnectionKey = xml.Get<string>("ConnectionKey");
            EntityName = xml.Get<string>("EntityName");
            ViewName = xml.Get<string>("MappingName");
            SourceName = StrToArray(xml.Get<string>("SourceName"));
            Interval = ParseSyncTime(xml.Get<string>("SyncTime"));
            SyncType = ParseSyncType(xml.Get<string>("SyncType"));
            SourceType = ParseSourceType(xml.Get<string>("SourceType"));
            EntityKeys = StrToArray(xml.Get<string>("EntityKeys"));
            EntityType = xml.Get<string>("EntityType", DefaultEntityType);
            EnableNoLock = xml.Get<bool>("EnableNoLock", false);
            CommandTimeout = xml.Get<int>("CommandTimeout", 0);
            
            MissingSchemaAction = System.Data.MissingSchemaAction.Add;
            PreserveChanges = false;

        }

        internal SyncEntity(string json)
        {
            var nameValue = (IDictionary)JsonSerializer.Deserialize(json);

            //var nameValue= json.Deserialize();
            ConnectionKey = nameValue.Get<string>("ConnectionKey");
            EntityName = nameValue.Get<string>("EntityName");
            ViewName = nameValue.Get<string>("MappingName");
            SourceName = StrToArray(nameValue.Get<string>("SourceName"));
            Interval = ParseSyncTime(nameValue.Get<string>("SyncTime"));
            SyncType = ParseSyncType(nameValue.Get<string>("SyncType"));
            SourceType = ParseSourceType(nameValue.Get<string>("SourceType"));
            EntityKeys = StrToArray(nameValue.Get<string>("EntityKeys"));
            EntityType = nameValue.Get<string>("EntityType", DefaultEntityType);
            EnableNoLock = nameValue.Get<bool>("EnableNoLock", false);
            CommandTimeout = nameValue.Get<int>("CommandTimeout", 0);

            MissingSchemaAction = System.Data.MissingSchemaAction.Add;
            PreserveChanges = false;

        }

       
         /// <summary>
        /// Initialize a new instance of data cache entity.
        /// </summary>
        /// <param name="node"></param>
        internal SyncEntity(XmlNode node)
        {
            //LoadDataCacheEntity(node);

            if (node == null)
            {
                throw new ArgumentException("Inavlid Xml Root, 'Table' ");
            }

            XmlParser parser = new XmlParser(node.OuterXml);

            EntityName = parser.GetAttributeValue(node, "Name", true);
            ViewName = parser.GetAttributeValue(node, "MappingName", EntityName);
            string sourceName = parser.GetAttributeValue(node, "SourceName", EntityName);
            SyncType = SyncTimer.SyncTypeFromString(parser.GetAttributeValue(node, "SyncType", "None"));
            Interval = SyncTimer.TimeSpanFromString(parser.GetAttributeValue(node, "SyncTime", "0"));
            SourceName = CacheUtil.SplitStrTrim(sourceName);
            //======================
            SourceType = (EntitySourceType)EnumExtension.Parse<EntitySourceType>(parser.GetAttributeValue(node, "SourceType", EntitySourceType.Table.ToString()), EntitySourceType.Table);
            EntityKeys = CacheUtil.SplitStrTrim(parser.GetAttributeValue(node, "EntityKeys", ""));
            EntityType = parser.GetAttributeValue(node, "EntityType", DefaultEntityType);

            EnableNoLock = GenericTypes.Convert<bool>( parser.GetAttributeValue(node, "EnableNoLock", "false"),false);
            CommandTimeout =  parser.GetAttributeValue(node, "CommandTimeout", 0);

            MissingSchemaAction = System.Data.MissingSchemaAction.Add;
            PreserveChanges = false;
        }

        /// <summary>
        /// Initialize a new instance of sync entity.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="syncType"></param>
        /// <param name="interval"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public SyncEntity(string entityName, string mappingName, string[] sourceName, SyncType syncType, TimeSpan interval, bool enableNoLock, int commandTimeout)
        {
            EntityName = entityName;
            ViewName = mappingName;
            SourceName = sourceName;
            PreserveChanges = false;
            MissingSchemaAction = MissingSchemaAction.Add;
            SyncType = syncType;
            Interval = interval;
            EnableNoLock = enableNoLock;
            CommandTimeout = commandTimeout;
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// Destructor.
        /// </summary>
        ~SyncEntity()
        {
             Dispose(false);
        }
        /// <summary>
        /// Realease all resources from current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            this.ConnectionKey = null;
            this.EntityKeys = null;
            this.EntityName = null;
            this.SourceName = null;
            this.ViewName = null;
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
            if (!(obj is SyncEntity))
                return false;
            return base.Equals(((SyncEntity)obj).EntityName == this.EntityName);
        }

        /// <summary>
        ///  Determines whether the specified System.Object is equal to the current DataSyncEntity.
        /// </summary>
        /// <param name="syncEntity"></param>
        /// <returns></returns>
        public bool IsEquals(SyncEntity syncEntity)
        {

            var syncKey=string.Join(",", syncEntity.EntityKeys);
            var thisKey=string.Join(",", this.EntityKeys);

            var syncSourceName = string.Join(",", syncEntity.SourceName);
            var thisSourceName = string.Join(",", this.SourceName);

            var isequal= (syncEntity.EntityName == this.EntityName &&
                syncEntity.ConnectionKey == this.ConnectionKey &&
                syncKey == thisKey &&
                syncEntity.EntityType == this.EntityType &&
                syncEntity.Interval == this.Interval &&
                syncSourceName == thisSourceName &&
                syncEntity.SourceType == this.SourceType &&
                syncEntity.SyncType == this.SyncType &&
                syncEntity.ViewName == this.ViewName);

            return isequal;
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
            return this.EntityName;
        }
        #endregion

        #region Methods

        //-GenericRecord
        Type GetDefaultEntityType() { return typeof(GenericRecord); }
        /// <summary>
        /// Get type of current entity.
        /// </summary>
        /// <returns></returns>
        public Type GetEntityType()
        {
            if (string.IsNullOrEmpty(EntityType))
                return GetDefaultEntityType();
            if (EntityType.Equals(DefaultEntityType))
                return GetDefaultEntityType();
            if (EntityType.Equals("GenericRecord"))
                return typeof(GenericRecord);

            Type type = SerializeTools.GetQualifiedType(EntityType); //Type.GetType(EntityType, false);

            if (type == null)
            {
                type = GetDefaultEntityType();
                CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Error, "GetEntityType error not found: " + EntityType);
            }

            return type;
        }
       

        /// <summary>
        /// Create new instance of <see cref="ISyncItem"/>
        /// </summary>
        /// <returns></returns>
        internal ISyncItem CreateInstance()
        {
            Type type = GetEntityType();
            Type d1 = typeof(SyncItem<>);
            Type[] typeArgs = { type };
            Type constructed = d1.MakeGenericType(typeArgs);
            object o = Activator.CreateInstance(constructed);

            ((ISyncCacheItem)o).Set(this,false);

            return (ISyncItem)o;
        }

     
        
        /// <summary>
        /// Get SyncTimer
        /// </summary>
        public SyncTimer GetSyncTimer()
        {
            return new SyncTimer(Interval,SyncType);
        }

        /// <summary>
        /// Validate sync entity properties.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void ValidateSyncEntity()
        {
            if (ConnectionKey == null)
                throw new ArgumentNullException("SyncEntity.connectionKey");
            if (EntityName == null)
                throw new ArgumentNullException("SyncEntity.entityName");
            if (ViewName == null)
                throw new ArgumentNullException("SyncEntity.mappingName");
            if (SourceName == null)
                throw new ArgumentNullException("SyncEntity.sourceName");
            if (EntityKeys == null)
                throw new ArgumentNullException("SyncEntity.entityKeys");
        }
        
        

        #endregion

        #region static
        
        /// <summary>
        /// Parse <see cref="SyncType"/> from arguments.
        /// </summary>
        /// <param name="strSyncType"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        public static SyncType ParseSyncType(string strSyncType, SyncType defaultType = SyncType.None)
        {
            SyncType syncType = EnumExtension.Parse<SyncType>(strSyncType, defaultType);
            return syncType;
        }
        /// <summary>
        /// Parse <see cref="EntitySourceType"/> from arguments.
        /// </summary>
        /// <param name="strSourceType"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        public static EntitySourceType ParseSourceType(string strSourceType, EntitySourceType defaultType = EntitySourceType.Table)
        {
            EntitySourceType sourceType = EnumExtension.Parse<EntitySourceType>(strSourceType, defaultType);
            return sourceType;
        }

        /// <summary>
        /// Parse <see cref="TimeSpan"/> from arguments.
        /// </summary>
        /// <param name="strInterval"></param>
        /// <returns></returns>
        public static TimeSpan ParseSyncTime(string strInterval)
        {
            TimeSpan syncTime = TimeSpan.Zero;
            TimeSpan.TryParse(strInterval, out syncTime);
            return syncTime;
        }
        /// <summary>
        /// Split str to array of string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string[] StrToArray(string str)
        {
            string[] strArray = str.SplitTrim(',');

            return strArray;
        }

        public static SyncEntity[] GetItems(XmlNodeList list)
        {
            List<SyncEntity> items = new List<SyncEntity>();
            foreach (XmlNode n in list)
            {
                if (n.NodeType == XmlNodeType.Comment)
                    continue;
                SyncEntity sync = new SyncEntity(new XmlTable(n));
                if (items.Exists(s => s.EntityName == sync.EntityName && s.ConnectionKey==sync.ConnectionKey))
                {
                    CacheLogger.Logger.LogAction(CacheAction.SyncItem, CacheActionState.Debug, "Duplicate in SyncFile, entity: " + sync.EntityName);
                    continue;
                }
                items.Add(sync);
            }
            return items.ToArray();
        }

        public static IEnumerable<SyncEntity> GetItemsToSync(IEnumerable<SyncEntity> curItems, IEnumerable<SyncEntity> syncItems)
        {
           
            var result = (from sync in syncItems
                          from cur in curItems
                          where
                          ((sync.EntityType != cur.EntityType ||
                          sync.Interval != cur.Interval ||
                          sync.SourceType != cur.SourceType ||
                          sync.SyncType != cur.SyncType ||
                          sync.ViewName != cur.ViewName ||
                          string.Join(",", sync.EntityKeys)!= string.Join(",", cur.EntityKeys) ||
                          string.Join(",", sync.SourceName) != string.Join(",", cur.SourceName)
                          )
                          && (sync.EntityName == cur.EntityName &&
                          sync.ConnectionKey == cur.ConnectionKey
                           ))
                          select sync).Distinct();

            //List<SyncEntity> modifiedItems = new List<SyncEntity>(result);
            //return modifiedItems;

            return result;
            
            //syncItems.Where()
            //syncItems.Except(curItems);

            //foreach(var item in syncItems)
            //{
            //    curItems.Where(i=> i.IsEquals(..FirstOrDefault()
            //    //if()
            //}


        }


        #endregion


    }
}
