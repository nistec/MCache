using Nistec.Caching.Config;
using Nistec.Caching.Data;
using Nistec.Caching.Server;
using Nistec.Caching.Sync.Remote;
using Nistec.Data.Ado;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Nistec.Caching.Sync
{
    public class SyncConfig
    {
        #region members
        public const string fileName = "SyncCache.config";
        public const string RootSync = "SyncCache";
        public const string RootConnection = "connectionStrings";

        int synchronizedSync;
        int synchronizedCP;

        //bool _reloadOnChange;
        //bool _enableSyncFileWatcher;
        //long _Started;
        bool _initialized;
        SysFileWatcher _SyncFileWatcher;
        #endregion

        #region properties

        public string FilePath { get; private set; }
        //public bool EnableAsyncTask { get; set; }
        //public bool ReloadAllItemsOnChange { get; set; }
        public bool EnableSyncFileWatcher { get; set; }
        public Action<SyncEntity> OnSyncItem { get; set; }

        #endregion
        /// <summary>
        /// Start Cache Synchronization.
        /// </summary>
        public void Start(bool enableSyncFileWatcher)//, bool reloadOnChange = true)
        {
            //if (Interlocked.Read(ref _Started) == 1)
            //    return;
            //Interlocked.Exchange(ref _Started, 1);

            if (_initialized)
            {
                return;
            }
            EnableSyncFileWatcher = enableSyncFileWatcher;
            //_reloadOnChange = reloadOnChange;
            if (enableSyncFileWatcher)
            {
                _SyncFileWatcher = new SysFileWatcher(CacheSettings.SyncConfigFile, true);
                _SyncFileWatcher.FileChanged += new FileSystemEventHandler(_SyncFileWatcher_FileChanged);

                OnSyncFileChange(new FileSystemEventArgs(WatcherChangeTypes.Created, _SyncFileWatcher.SyncPath, _SyncFileWatcher.Filename));
            }
            else
            {
                LoadSyncConfig();
            }
            _initialized = true;
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "SyncConfig Started!");
        }

        /// <summary>
        /// Stop Cache Synchronization.
        /// </summary>
        public void Stop()
        {
            _initialized = false;
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "SyncConfig Stoped!");
        }


        void _SyncFileWatcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            Task task = Task.Factory.StartNew(() => OnSyncFileChange(e));
        }

        void OnSyncFileChange(object args)//GenericEventArgs<string> e)
        {
            FileSystemEventArgs e = (FileSystemEventArgs)args;

            LoadSyncConfigFile(e.FullPath);//, 3);
        }


        #region events

        ///// <summary>
        ///// Sync Changed Event Handler
        ///// </summary>
        //public event EventHandler<GenericEventArgs<string>> SyncChanged;

        ///// <summary>
        ///// OnSyncChanged
        ///// </summary>
        ///// <param name="e"></param>
        //protected virtual void OnSyncChanged(GenericEventArgs<string> e)
        //{
        //    if (SyncChanged != null)
        //        SyncChanged(this, e);
        //}

        ///// <summary>
        ///// Occured On Sync Changed
        ///// </summary>
        ///// <param name="entity"></param>
        //protected virtual void OnFunctionSyncChanged(string entity)
        //{
        //    CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.Debug, "OnSync Refresh started :" + entity);

        //    Owner.Refresh(entity);

        //    OnSyncChanged(new GenericEventArgs<string>(entity));
        //}


        ///// <summary>
        ///// Sync Reload Event Handler.
        ///// </summary>
        //public event EventHandler<GenericEventArgs<string>> SyncReload;

        ///// <summary>
        ///// Occured On Sync Reload.
        ///// </summary>
        ///// <param name="e"></param>
        //protected virtual void OnSyncReload(GenericEventArgs<string> e)
        //{
        //    if (SyncReload != null)
        //        SyncReload(this, e);

        //    CacheLogger.Logger.LogAction(CacheAction.SyncTime, CacheActionState.None, "OnSyncReload :" + e.Args);

        //}

        /// <summary>
        /// Sync Error Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SyncError;
        /// <summary>
        /// Sync LoadCompleted Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<SyncEntity[]>> LoadCompleted;

        /// <summary>
        /// On Error Occured
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnError(string e)
        {
            if (SyncError != null)
                SyncError(this, new GenericEventArgs<string>(e));
        }

        protected virtual void OnLoadCompleted(SyncEntity[] e)
        {
            if (LoadCompleted != null)
                LoadCompleted(this, new GenericEventArgs<SyncEntity[]>(e));
        }

        #endregion

        #region Load xml config
        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        public void LoadSyncConfig()
        {

            string file = CacheSettings.SyncConfigFile;
            LoadSyncConfigFile(file);//, 3);
        }

        //public void LoadSyncConfigFile(string file, int retrys)
        //{

        //    int counter = 0;
        //    bool reloaded = false;
        //    while (!reloaded && counter < retrys)
        //    {
        //        reloaded = LoadSyncConfigFile(file);
        //        counter++;
        //        if (!reloaded)
        //        {
        //            CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Failed, "LoadSyncConfigFile retry: " + counter);
        //            Thread.Sleep(100);
        //        }
        //    }
        //    if (reloaded)
        //    {
        //        OnSyncReload(new GenericEventArgs<string>(file));
        //    }
        //}

        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        /// <param name="file"></param>
        public bool LoadSyncConfigFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                return false;
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

            XmlNode items = doc.SelectSingleNode("//" + RootSync);

            XmlNode cpItems = doc.SelectSingleNode("//" + RootConnection);

            if (cpItems != null)
                LoadSyncConnections(cpItems);// Task.Factory.StartNew(()=> LoadSyncConnections(cpItems));

            if (items != null)
                Task.Factory.StartNew(() => LoadSyncTables(items));

        }

        //internal abstract void LoadSyncTables(XmlNode node);//, bool EnableAsyncTask, bool enableLoader);

        internal virtual void LoadSyncTables(XmlNode node)
        {
            if (node == null)
                return;

            try
            {
                if (0 == Interlocked.Exchange(ref synchronizedSync, 1))
                {
                    XmlNodeList list = node.ChildNodes;
                    if (list == null)
                    {
                        CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, "LoadSyncTables is empty");
                        return;
                    }

                    var newSyncEntityItems = SyncEntity.GetItems(list);

                    if (newSyncEntityItems == null || newSyncEntityItems.Length == 0)
                    {
                        throw new Exception("Can not LoadSyncTables, SyncEntity Items not found");
                    }

                    //if(EnableAsyncTask && OnSyncItem!=null)
                    //{
                    //    foreach(var item in newSyncEntityItems)
                    //    {
                    //        AgentManager.Tasker.Add(OnSyncItem, item);
                    //    }
                    //}

                    OnLoadCompleted(newSyncEntityItems);
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, string.Format("LoadSyncTables error: {0}", ex.Message));

                OnError("LoadSyncTables error " + ex.Message);

            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronizedSync, 0);
            }
        }

        #endregion


        #region load connection

        internal virtual void LoadSyncConnections(XmlNode node)
        {
            if (node == null)
                return;

            try
            {
                if (0 == Interlocked.Exchange(ref synchronizedCP, 1))
                {
                    XmlNodeList list = node.ChildNodes;
                    if (list == null)
                    {
                        CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, "LoadSyncTables is empty");
                        return;
                    }

                    var newItems = ConnectionSettings.GetItems(list);

                    if (newItems == null || newItems.Length == 0)
                    {
                        throw new Exception("Can not Load connection items, Items not found");
                    }

                    AgentManager.Connections.LoadConfigItems(newItems);

                    //OnLoadCompleted(newSyncEntityItems);
                }
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Error, string.Format("LoadSyncTables error: {0}", ex.Message));

                OnError("LoadSyncTables error " + ex.Message);

            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronizedCP, 0);
            }
        }
        #endregion load xml config


    }
}
