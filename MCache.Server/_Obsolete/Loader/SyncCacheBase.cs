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
using System.Text;
using System.Configuration;
using System.Data;
using System.Collections;
using System.Xml;
using System.IO;
using Nistec.Data;
using Nistec.Generic;
using Nistec.Data.Entities;
using Nistec.Data.Entities.Cache;
using Nistec.Xml;
using Nistec.Threading;
using Nistec.Runtime;
using Nistec.Caching.Server;
using Nistec.Caching.Data;
using System.Threading.Tasks;
using System.Threading;
using Nistec.Caching.Config;
using Nistec.Caching.Sync.Embed;

namespace Nistec.Caching.Loader
{
    /// <summary>
    /// Represents a base class for a multi database Synchronization cache
    /// which manages them using <see cref="SyncDbCache"/>.  
    /// It is usefull for remote and embeded cache as well.
    /// The synchronization properties are configurable using "SyncFile",
    /// that uses <see cref="SysFileWatcher"/> which Listens to the file system change notifications and raises events when a
    /// file is changed.
    /// </summary>
    public abstract class SyncBase : IDisposable
    {
        #region members

        const string DefaultCacheName = "SyncCache";

        internal SyncEntity[] SyncEntityItems;


        internal SyncDbCache _DataCache;
        internal SysFileWatcher _SyncFileWatcher;
        //internal SyncBox _SyncBox;

        internal bool _initialized = false;
        internal bool _reloadOnChange = false;
        internal bool _enableSyncFileWatcher = false;
        internal static object ThreadLock = new object();
        internal bool IsWebHosted = false;
        int _IntervalSeconds = CacheDefaults.DefaultIntervalSeconds;
        #endregion

        #region copy

        internal SyncDbCache DataCacheCopy()
        {
            var db = new SyncDbCache(m_cacheName, true);
            db.FunctionSyncChanged = OnFunctionSyncChanged;
            return db;
        }

        #endregion

        #region ctor

        /// <summary>
        /// Initilaize new instance of <see cref="SyncCache"/>
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="isWebHosted"></param>
        public SyncBase(string cacheName, bool isWebHosted)
        {
            _IntervalSeconds = CacheDefaults.GetValidIntervalSeconds(CacheSettings.SyncInterval);
            IsWebHosted = isWebHosted;
             m_cacheName = cacheName;
            _DataCache = new SyncDbCache(cacheName);
            _DataCache.FunctionSyncChanged = OnFunctionSyncChanged;

            //_SyncBox = SyncBox.Instance;
            //_SyncBox.SyncAccepted += _SyncBox_SyncAccepted;

            //_Timer = new TimerSyncDispatcher(CacheSettings.SyncInterval,10,true);
            //_Timer.SyncCompleted += _Timer_SyncCompleted;
            //_Timer.SyncStarted += _Timer_SyncStarted;
        }

        void _SyncBox_SyncAccepted(object sender, SyncEntityTimeCompletedEventArgs e)
        {
            e.Item.DoAsync();
        }


        void _DataCache_SyncChanged(object sender, GenericEventArgs<string> e)
        {
            CacheLogger.Debug("SyncCacheBase _DataCache_SyncChanged : " + e.Args);

            Refresh(e.Args);
            OnSyncChanged(new GenericEventArgs<string>(e.Args));
        }


        long _Started=0;

        /// <summary>
        /// Start Cache Synchronization.
        /// </summary>
        public void Start(bool enableSyncFileWatcher=true, bool reloadOnChange = true)
        {
            if (Interlocked.Read(ref _Started)==1)
                return;
            Interlocked.Exchange(ref _Started, 1);
           
            if (_initialized)
            {
               
                return;
            }
            _DataCache.Start(IntervalSeconds);

            _enableSyncFileWatcher = enableSyncFileWatcher;
            _reloadOnChange = reloadOnChange;
            if (enableSyncFileWatcher)
            {
                _SyncFileWatcher = new SysFileWatcher(CacheSettings.SyncConfigFile,true);
                _SyncFileWatcher.FileChanged += new FileSystemEventHandler(_SyncFileWatcher_FileChanged);

                OnSyncFileChange(new FileSystemEventArgs(WatcherChangeTypes.Created, _SyncFileWatcher.SyncPath, _SyncFileWatcher.Filename));
            }
            
            _initialized = true;
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "SyncCache Started!");
        }

        void _SyncFileWatcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            Task task = Task.Factory.StartNew(() => OnSyncFileChange(e));
        }

        void OnSyncFileChange(object args)//GenericEventArgs<string> e)
        {
            FileSystemEventArgs e = (FileSystemEventArgs)args;

            LoadSyncConfigFile(e.FullPath, 3);
        }


        /// <summary>
        /// Stop Cache Synchronization.
        /// </summary>
        public void Stop()
        {
            if (_DataCache != null)
            {
                _DataCache.Stop();
            }
            _initialized = false;
            Interlocked.Exchange(ref _Started, 0);
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "SyncCache Stoped!");
        }

        #endregion

        #region IDispose
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            Stop();

            if (disposing)
            {
                if (_DataCache != null)
                {
                    _DataCache.Dispose();
                    _DataCache = null;
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the System.ComponentModel.Component.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        #endregion

        #region abstract

        //internal abstract void LoadSyncBag(string cacheName);

        /// <summary>
        /// Add Item to sync cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceName"></param>
        /// <param name="sourceType"></param>
        /// <param name="entityKeys"></param>
        /// <param name="columns"></param>
        /// <param name="interval"></param>
        /// <param name="syncType"></param>
        /// <param name="enableNoLock"></param>
        /// <param name="commandTimeout"></param>
        public abstract void AddItem<T>(string connectionKey, string entityName, string mappingName, string[] sourceName, EntitySourceType sourceType, string[] entityKeys,string columns, TimeSpan interval, SyncType syncType, bool enableNoLock, int commandTimeout);

        internal abstract void AddItem(SyncEntity entity);
        /// <summary>
        /// Remove all items from sync cache.
        /// </summary>
        /// <param name="clearDataCache"></param>
        public abstract void Clear(bool clearDataCache);
        /// <summary>
        /// Refresh specific item in sync cache.
        /// </summary>
        /// <param name="syncName"></param>
        public abstract void Refresh(string syncName);


        /// <summary>
        /// Remove specific item from sync cache.
        /// </summary>
        /// <param name="syncName"></param>
        /// <returns></returns>
        public abstract bool RemoveItem(string syncName);
        #endregion

        #region internal methods

        /*
        /// <summary>
        /// Get the count of all items in all cref="ISyncItem"/> items in cache.
        /// </summary>
        /// <returns></returns>
        protected ICollection GetAllSyncValues<T>(ICollection<T> Col)
        {
            if (Col == null)
                return null;
            List<object> list = new List<object>();
            foreach (ISyncItemBase syncitem in Col)
            {
                foreach (object o in syncitem.Values)
                {
                    list.Add(o);
                }
            }
            return list;
        }
        */
        /// <summary>
        /// Get the count of all items in all cref="ISyncItem"/> items in cache.
        /// </summary>
        /// <returns></returns>
        protected int GetAllSyncCount<T>(ICollection<T> Col)
        {
            int count = 0;
            if (Col == null)
                return 0;
            foreach (ISyncItemBase syncitem in Col)
            {
                count += syncitem.Count;
            }
            return count;
        }
         
        #endregion

        #region Load xml config
        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        public void LoadSyncConfig()
        {

            string file = CacheSettings.SyncConfigFile;
            LoadSyncConfigFile(file, 3);
        }

        internal void LoadSyncConfigFile(string file,int retrys)
        {

            int counter = 0;
            bool reloaded = false;
            while (!reloaded && counter < retrys)
            {
                reloaded = LoadSyncConfigFile(file);
                counter++;
                if (!reloaded)
                {
                    CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Failed, "LoadSyncConfigFile retry: " + counter);
                }
            }
            if (reloaded)
            {
                OnSyncReload(new GenericEventArgs<string>(file));
            }
        }

        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        /// <param name="file"></param>
        public bool LoadSyncConfigFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                return true;
            Thread.Sleep(1000);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
               
                LoadSyncConfig(doc);
                return true;
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Error, "LoadSyncConfigFile error: " + ex.Message);
                OnError("LoadSyncConfigFile error " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Load sync cache from xml string argument.
        /// </summary>
        /// <param name="xml"></param>
        public bool LoadSyncConfig(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return true;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                LoadSyncConfig(doc);
                return true;
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Error, "LoadSyncConfig error: " + ex.Message);
                OnError("LoadSyncConfig error " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Load sync cache from <see cref="XmlDocument"/> document.
        /// </summary>
        /// <param name="doc"></param>
        public void LoadSyncConfig(XmlDocument doc)
        {
            if (doc == null)
                return;

            XmlNode items = doc.SelectSingleNode("//SyncCache");
            if (items == null)
                return;
            LoadSyncItems(items, CacheSettings.EnableAsyncTask);
        }

        internal abstract void LoadSyncItems(XmlNode node, bool EnableAsyncTask);
   
        #endregion load xml config

        #region Properties

        string m_cacheName;
        /// <summary>
        /// Get Cache name
        /// </summary>
        public string CacheName
        {
            get { return m_cacheName; }
        }

        /// <summary>
        /// Get indicate whether the cache was intialized.
        /// </summary>
        public bool Initialized
        {
            get { return _initialized; }
        }

        /// <summary>
        /// Get or set The interval in seconds for synchronization.
        /// </summary>
        public int IntervalSeconds
        {
            get { return _IntervalSeconds; }
            set
            {
                if (value > 0)
                {
                    _IntervalSeconds = value;
                }
            }
        }

        SyncDbCache DataCache
        {
            get { return _DataCache; }
        }

        #endregion

        #region events

        /// <summary>
        /// Sync Changed Event Handler
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SyncChanged;

        /// <summary>
        /// OnSyncChanged
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncChanged(GenericEventArgs<string> e)
        {
            if (SyncChanged != null)
                SyncChanged(this, e);
        }

        /// <summary>
        /// Occured On Sync Changed
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void OnFunctionSyncChanged(string entity)
        {
            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Debug, "OnSync Refresh started :" + entity);

            Refresh(entity);

            OnSyncChanged(new GenericEventArgs<string>(entity));
        }
        /// <summary>
        /// Sync Reload Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SyncReload;

        /// <summary>
        /// Occured On Sync Reload.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncReload(GenericEventArgs<string> e)
        {
            if (SyncReload != null)
                SyncReload(this, e);

            CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.None, "OnSyncReload :" + e.Args);

        }
        /// <summary>
        /// Sync Error Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SyncError;

        /// <summary>
        /// On Error Occured
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnError(string e)
        {
            if (SyncError != null)
                SyncError(this, new GenericEventArgs<string>(e));
        }

       

        #endregion
        
        #region Add items

        internal void AddItem(XmlTable xml, bool copy)
        {
            SyncEntity entity = new SyncEntity(xml);
            if (entity.SyncType != SyncType.Remove)
            {
                AddItem(entity);
            }
        }

        internal void AddItem(string entityType, string connectionKey, string entityName, string mappingName, string strSourceName, int iSourceType, string strEntityKeys, string columns, int intervalMinute, int iSyncType, bool enableNoLock, int commandTimeout)
        {
            EntitySourceType sourceType = (EntitySourceType)iSourceType;
            TimeSpan interval = TimeSpan.FromMinutes(intervalMinute);
            SyncType syncType = (SyncType)iSyncType;
            if (syncType != SyncType.Remove)
            {
                AddItem(entityType, connectionKey, entityName, mappingName, strSourceName, sourceType, strEntityKeys, columns,interval, syncType, enableNoLock, commandTimeout);
            }
        }


        void AddItem(string entityType, string connectionKey, string entityName, string mappingName, string strSourceName, EntitySourceType sourceType, string strEntityKeys, string columns, TimeSpan syncTime, SyncType syncType, bool enableNoLock, int commandTimeout)
        {

            if (strSourceName == null)
                throw new ArgumentNullException("AddItem.strSourceName");
            if (strEntityKeys == null)
                throw new ArgumentNullException("AddItem.strEntityKeys");

            string[] sourceName = strSourceName.SplitTrim(',');
            string[] entityKeys = strEntityKeys.SplitTrim(',');

            switch (entityType)
            {
                case "GenericRecord":
                    AddItem<GenericRecord>(connectionKey, entityName, mappingName, sourceName, sourceType, entityKeys, columns, syncTime, syncType, enableNoLock, commandTimeout);
                    break;
                case "EntityStream":
                    AddItem<EntityStream>(connectionKey, entityName, mappingName, sourceName, sourceType, entityKeys, columns, syncTime, syncType, enableNoLock, commandTimeout);
                    break;
                case "GenericEntity":
                default:
                    AddItem<GenericEntity>(connectionKey, entityName, mappingName, sourceName, sourceType, entityKeys, columns, syncTime, syncType, enableNoLock, commandTimeout);
                    break;
            }
        }

        #endregion

        #region Cache Stream
        /// <summary>
        /// Reset sync cache.
        /// </summary>
        public virtual void Reset()
        {
            Stop();
            Clear(true);
            Start(_enableSyncFileWatcher, _reloadOnChange);
        }

        #endregion
        
    }
}
