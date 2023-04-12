using System;
using System.Data;
using System.Collections;
using System.Threading;
using Nistec.Data;
using System.Collections.Generic;

using Nistec.Caching;
//using Nistec.Data.Common;
using Nistec.Xml;
using System.Xml;
using Nistec.Data.Factory;
//using Nistec.Sys;

namespace Nistec.Legacy
{

    /// <summary>
    /// Summary description for DalCache utility.
    /// </summary>
    public class DalCache : System.ComponentModel.Component//, IDataCache
    {
        #region memebers
        public const string DefaultCachename = "McDataCache";

        private int _size;
        private DataSet m_ds;
        private string _storageName;
        private bool _disposed;
        private int _tableCounts;
        //private Thread _ThreadSetting;
        private int _sleepTime;
        private DataCacheState _state;
        //private Nistec.Threading.ThreadTimer _timer;  
        //private SyncTimer _syncTime;
        //private DateTime _lastSyncTime;
        //private DateTime _nextSyncTime;
        //private CacheSyncState _syncState;
        private bool initilized;
        private SyncSourceCollection _SyncTables;
        private SyncOption _SyncOption;
        private bool suspend;

        //private ActiveWatcher watcher;
        private string _ClientId;
        private string _TableWatcherName;
        private CacheSynchronize _CacheSynchronize;
        public IDbCmd dbCmd;
        //internal string connectionString;
        //internal DBProvider provider;

        static DalCache dbHash;


        /// <summary>
        /// CacheStateChanged
        /// </summary>
        public event EventHandler CacheStateChanged;
        /// <summary>
        /// DataCacheChanging 
        /// </summary>
        public event EventHandler DataCacheChanging;
        /// <summary>
        /// DataCacheChanged
        /// </summary>
        public event EventHandler DataCacheChanged;
        /// <summary>
        /// DataValueChanged
        /// </summary>
        public event EventHandler DataValueChanged;
        /// <summary>
        /// DalException
        /// </summary>
        public event DalCacheExceptionEventHandler DalException;
        /// <summary>
        /// SyncStateChange
        /// </summary>
        public event EventHandler SyncTimeState;
        /// <summary>
        /// SyncDataSourceEventHandler
        /// </summary>
        public event SyncDataSourceChangedEventHandler SyncDataSourceChanged;


        #endregion

        #region Ctor

        ///// <summary>
        ///// DalCache
        ///// </summary>
        ///// <param name="pk"></param>
        //public DalCache(string pk)
        //{
        //    Nistec.Net.DalNet.NetFram(pk, "SRV");
        //    DalCacheInternal();
        //}

        /// <summary>
        /// DalCache Ctor
        /// </summary>
        private DalCache(string cacheName)
        {
            _storageName = cacheName;// "McDataCache";
            //Nistec.Net.DalNet.NetFram("DalCache", "Ctl");
            _ClientId = Environment.MachineName + "$" + _storageName;
            _TableWatcherName = ActiveWatcher.DefaultWatcherName;// "Mc_TableWatcher";

            suspend = false;
            initilized = false;
            m_ds = new DataSet();
            _SyncTables = new SyncSourceCollection(this);
            _CacheSynchronize = new CacheSynchronize(this);
            _size = 0;
            _disposed = false;
            _tableCounts = 0;
            _sleepTime = 60000;
            _state = DataCacheState.Closed;
            //_syncState = CacheSyncState.Idle;
            //_syncTime = SyncTimer.Empty;
            //_lastSyncTime = DateTime.Now;
            //_nextSyncTime = DateTime.Now;
            _SyncOption = SyncOption.Manual;

            //_timer = new Nistec.Threading.ThreadTimer(_sleepTime);
            //_timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            m_ds.Disposed += new EventHandler(_ds_Disposed);
        }
        /// <summary>
        /// DalCache Ctor 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="providerDb"></param>
        public DalCache(string cacheName,string connection, DBProvider providerDb)
            : this(cacheName)
        {
            //this.connectionString = connection;
            //this.provider = providerDb;
            dbCmd = DbFactory.Create(connection, providerDb);
            //connectionString = connection;
            //provider = providerDb;
        }
        /// <summary>
        /// DalCache Ctor 
        /// </summary>
        /// <param name="dalBase"></param>
        public DalCache(string cacheName, IDalBase dalBase)
            : this(cacheName)
        {
            //this.connectionString = dalBase.IConnection.ConnectionString;
            //this.provider = dalBase.DBProvider;
            dbCmd = DbFactory.Create(dalBase.Connection.ConnectionString, dalBase.DBProvider);
            //connectionString = dalBase.IConnection.ConnectionString;
            //provider = dalBase.DBProvider;
        }

        /// <summary>
        /// DalCache Ctor 
        /// </summary>
        /// <param name="dalBase"></param>
        public DalCache(string cacheName, AutoDb dalDB)
            : this(cacheName)
        {
            //this.connectionString = dalDB.Connection.ConnectionString;
            //this.provider = dalDB.DBProvider;
            dbCmd = DbFactory.Create(dalDB.Connection.ConnectionString, dalDB.DBProvider);
            //connectionString = dalDB.Connection.ConnectionString;
            //provider = dalDB.DBProvider;
        }

        /// <summary>
        /// Synchronize DalCache
        /// </summary>
        /// <param name="cache"></param>
        public void Synchronize(DalCache cache)
        {
            if (initilized)
                return;

            this.dbCmd = cache.dbCmd;
            this.DS = cache.DataSource;

            this._storageName = cache._storageName;
            this._tableCounts = cache._tableCounts;
            this._sleepTime = cache._sleepTime;

            this._SyncTables = cache._SyncTables;
            this._SyncOption = cache._SyncOption;

            this._ClientId = cache._ClientId;
            this._TableWatcherName = cache._TableWatcherName;
            this._CacheSynchronize = cache._CacheSynchronize;
        }

        static DalCache()
        {
            //initilized=false;
            //dbHash=new DalCache();
        }
        /// <summary>
        /// Get static Dal storage
        /// </summary>
        public static DalCache Cache(string cacheName)
        {
                 if (dbHash == null)
                {
                    dbHash = new DalCache(cacheName);
                }
                return dbHash;
        }


        /// <summary>
        /// DalCache
        /// </summary>
        ~DalCache()
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_ds.Dispose();
                Stop();

                if (_CacheSynchronize != null)
                {
                    _CacheSynchronize.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        ///// <summary>
        ///// Dispose
        ///// </summary>
        //public new void Dispose()
        //{
        //    _ds.Dispose();
        //    Stop();
        //    if (_CacheSynchronize != null)
        //    {
        //        _CacheSynchronize.Dispose();
        //    }
        //    //if(_timer!=null)
        //    //{
        //    //  _timer.Elapsed -= new System.Timers.ElapsedEventHandler(_timer_Elapsed);
        //    //  _timer.Dispose();
        //    //}
        //}

        private void _ds_Disposed(object sender, EventArgs e)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        #region Load xml config

        public void LoadXmlConfigFile(string file)
        {

            if (string.IsNullOrEmpty(file))
                return;
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(file);
            LoadXmlConfig(doc);
        }

        public void LoadXmlConfig(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            LoadXmlConfig(doc);
        }

        //public void LoadXmlConfig(System.Xml.XmlDocument doc)
        //{
        //    if (doc==null)
        //        return;

        //    System.Xml.XmlNode node=null;
        //    System.Xml.XmlAttribute attrib = null;
        //    System.Xml.XmlNodeList nodeList = null;
        //    try
        //    {

        //        node = doc.SelectSingleNode("//CacheName");
        //        if (node != null)
        //        {
        //            this._storageName = node.InnerText;
        //        }

        //        node = doc.SelectSingleNode("//SyncOption");
        //        if (node != null)
        //        {
        //            this._SyncOption = (SyncOption)Enum.Parse(typeof(SyncOption), node.InnerText, true);
        //        }
        //        node = doc.SelectSingleNode("//ConnectionString");
        //        string connection = null;
        //        if (node != null)
        //        {
        //            dbCmd = DbFactory.Create(connection, DBProvider.SqlServer);
        //        }

        //        node = doc.SelectSingleNode("//DataSource");
        //        if (node != null)
        //            nodeList = node.ChildNodes;
        //        if (nodeList != null)
        //        {
        //            string tableName = null;
        //            string mappingName = null;

        //            foreach (System.Xml.XmlNode n in nodeList)
        //            {
        //                attrib = node.Attributes["Name"];
        //                if (attrib != null)
        //                    tableName = attrib.Value;
        //                attrib = node.Attributes["MappingName"];
        //                if (attrib != null)
        //                    mappingName = attrib.Value;
        //                if (!(string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(mappingName)))
        //                {
        //                    DataTable dt = dbCmd.ExecuteDataTable(tableName, "SELECT * FROM " + mappingName);
        //                    this.Add(dt, tableName);
        //                }
        //            }
        //        }
        //        nodeList = null;

        //        node = doc.SelectSingleNode("//SyncSource");
        //        if (node != null)
        //            nodeList = node.ChildNodes;
        //        if (nodeList != null)
        //        {
        //            string tableName = null;
        //            string mappingName = null;
        //            string syncType = null;
        //            string syncTime = null;
        //            foreach (System.Xml.XmlNode n in nodeList)
        //            {
        //                attrib = node.Attributes["Name"];
        //                if (attrib != null)
        //                    tableName = attrib.Value;
        //                attrib = node.Attributes["MappingName"];
        //                if (attrib != null)
        //                    mappingName = attrib.Value;
        //                if (!(string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(mappingName)))
        //                {
        //                    SyncType type = SyncType.None;
        //                    int days = 0;
        //                    int hour = 0;
        //                    int minute = 0;
        //                    TimeSpan ts = TimeSpan.Zero;

        //                    attrib = node.Attributes["SyncType"];
        //                    if (attrib != null)
        //                    {
        //                        syncType = attrib.Value;
        //                        type = (SyncType)Enum.Parse(typeof(SyncType), syncType, true);
        //                    }
        //                    attrib = node.Attributes["SyncTime"];
        //                    if (attrib != null)
        //                    {
        //                        syncTime = attrib.Value;
        //                        string[] s = syncTime.Split(';', ',');
        //                        if (s != null && s.Length == 2)
        //                        {
        //                            hour = Types.ToInt(s[0], 0);
        //                            minute = Types.ToInt(s[1], 0);
        //                            ts = new TimeSpan(hour, minute, 0);
        //                        }
        //                        if (s != null && s.Length >= 3)
        //                        {
        //                            days = Types.ToInt(s[0], 0);
        //                            hour = Types.ToInt(s[1], 0);
        //                            minute = Types.ToInt(s[2], 0);
        //                            ts = new TimeSpan(days,hour, minute, 0);
        //                        }

        //                    }
        //                    if (type != SyncType.None && hour + minute > 0)
        //                        this.SyncTables.Add(tableName, mappingName, new SyncTimer(ts, type));
        //                    else
        //                        this.SyncTables.Add(new SyncSource(tableName,mappingName));

        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        OnDalException(ex.Message, DalCacheError.ErrorReadFromXml);
        //    }

        //}

        public void LoadXmlConfig(System.Xml.XmlDocument doc)
        {

            //  <RemoteData>
            //  <Settings>
            //    <ConnectionString value ="Data Source=MCONTROL; Initial Catalog=Northwind; uid=sa;password=tishma; Connection Timeout=30"/>
            //    <Provider value ="SqlServer"/>
            //    <DataCacheName value ="McRemoteData"/>
            //    <LoadRemoteSettings value ="true"/>
            //  </Settings>

            //  <DataSource>
            //    <Table Name="Customers">
            //      <MappingName value="Customers"/>
            //      <SyncType value="Interval"/>
            //      <SyncTime value="0:20:0"/>
            //    </Table>
            //  </DataSource>
            //</RemoteData>

            if (doc == null)
                return;

            System.Xml.XmlNode root = null;
            System.Xml.XmlNode settings = null;
            System.Xml.XmlNode dataSource = null;
            try
            {
                XmlParser parser = new XmlParser(doc);
                root = parser.SelectSingleNode("//RemoteData", true);
                settings = parser.SelectSingleNode(root, "//Settings", true);
                string connection = parser.GetAttributeValue(settings, "ConnectionString", "value", "");
                DBProvider provider = DbFactory.GetProvider(parser.GetAttributeValue(settings, "Provider", "value", "SqlServer"));
                this.CacheName = parser.GetAttributeValue(settings, "DataCacheName", "value", DalCache.DefaultCachename);
                if (string.IsNullOrEmpty(connection))
                    return;
                bool useWatcher = Types.ToBool(parser.GetAttributeValue(settings, "UseTableWatcher", "value", "false"), false);


                this.dbCmd = DbFactory.Create(connection, provider);



                dataSource = parser.SelectSingleNode(root, "//DataSource", false);
                if (dataSource == null)
                    return;
                XmlNodeList list = dataSource.ChildNodes;
                if (list == null)
                    return;
                foreach (XmlNode n in list)
                {
                    LoadDataCacheItem(new DataCacheItemProperties(n));
                }
                if (useWatcher)
                {
                    ActiveWatcher.CreateTableWatcher(dbCmd);
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorReadFromXml);
            }
        }


        public void LoadXmlConfig(DataCacheProperties prop)
        {
            dbCmd = DbFactory.Create(prop.ConnectionString, prop.Provider);
            this.CacheName = prop.DataCacheName;
            if (prop.UseTableWatcher)
            {
                CreateTableWatcher(TableWatcherName);
            }
            if (!string.IsNullOrEmpty(prop.Xmlsettings))
            {
                LoadDataCacheItems(prop.GetItemsSettings());
            }
        }

        public virtual void LoadDataCacheItem(DataCacheItemProperties item)
        {
            try
            {

                DataTable dt = dbCmd.ExecuteDataTable(item.TableName, "SELECT * FROM " + item.MappingName, false);
                this.Add(dt, item.TableName);
                if (item.SyncType != SyncType.None)
                {
                    this.SyncTables.Add(item.TableName, item.MappingName, item.SourceName, new SyncTimer(item.SyncTime, item.SyncType));
                }

            }
            catch { }
        }

        public void LoadDataCacheItems(DataCacheItemProperties[] items)
        {
            if (items == null)
                return;

            try
            {
                foreach (DataCacheItemProperties item in items)
                {
                    LoadDataCacheItem(item);
                }

            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorReadFromXml);
            }

        }
        #endregion load xml config

        #endregion ctor

        #region Setting

        public int CreateTableWatcher(string tableWatcherName)
        {
            _TableWatcherName = tableWatcherName;
            return ActiveWatcher.CreateTableWatcher(dbCmd, tableWatcherName);
        }

        public void CreateTablesTrigger(params string[] Tables)
        {
            ActiveWatcher.CreateTablesTrigger(dbCmd, Tables, TableWatcherName);
        }

        public void CreateTablesTrigger()
        {
            string[] Tables = this._SyncTables.GetTablesTrigger();
            ActiveWatcher.CreateTablesTrigger(dbCmd, Tables, TableWatcherName);
        }


        /// <summary>
        /// Start storage ThreadSetting
        /// </summary>
        public void Start()//string lockKey)
        {
            if (initilized)
                return;

            //if (!Encryption.Delock(lockKey))
            //{
            //    throw new ArgumentException("Invalid Lock Key");
            //}

            //Thread.AllocateNamedDataSlot("sleeptime");		

            //watcher = new ActiveWatcher(this);
            //RegisteredTablesEvent();
            initilized = true;
            //_ThreadSetting = new Thread(new ThreadStart(CheckSetting));
            //_ThreadSetting.IsBackground=true;
            //_ThreadSetting.Start();
            //_timer.Interval = IntervalSetting;
            //_timer.Start();

            if (_SyncOption == SyncOption.Auto)
            {
                int res=CreateTableWatcher(_TableWatcherName);
                if (res > 0)
                {
                    CreateTablesTrigger();
                }

                foreach (SyncSource source in _SyncTables)
                {
                    source.SyncSourceChanged += new SyncDataSourceChangedEventHandler(source_SyncSourceChanged);
                }
                _CacheSynchronize.Start(IntervalSetting);
            }
            OnCacheStateChanged(EventArgs.Empty);
        }

        void source_SyncSourceChanged(object sender, SyncDataSourceChangedEventArgs e)
        {
            OnSyncDataSourceChanged(e);
        }


        /// <summary>
        /// Stop storage ThreadSetting
        /// </summary>
        public void Stop()
        {
            if (!initilized)
                return;
            initilized = false;

            if (_SyncOption == SyncOption.Auto)
            {
                foreach (SyncSource source in _SyncTables)
                {
                    source.SyncSourceChanged -= new SyncDataSourceChangedEventHandler(source_SyncSourceChanged);
                }
                _CacheSynchronize.Stop();
            }

            //_timer.Stop();
            //Thread.FreeNamedDataSlot("sleeptime");
            //if (_ThreadSetting != null)
            //{
            //    _ThreadSetting.Abort();
            //}
            OnCacheStateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Restart storage ThreadSetting
        /// </summary>
        internal void RestartThreadSetting()
        {
            Stop();
            Start();//Encryption.Enlock());
        }

        //void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    if (initilized)
        //    {
        //        if (_SyncOption == SyncOption.Auto)
        //        {
        //            //DetermineSyncTime();
        //            SyncRegisteredTables();
        //        }
        //    }
        //}


        //private void DetermineSyncTime()
        //{
        //    if (_syncTime.IsEmpty)
        //        return;

        //    if (NextSyncTime.CompareTo(DateTime.Now) < 0)
        //    {
        //        _nextSyncTime = _syncTime.GetNextValidTime(_lastSyncTime);
        //        _syncState = CacheSyncState.Started;
        //        OnSyncTimeState(EventArgs.Empty);
        //    }

        //}

        private void DetermineSleepTime()
        {
            //Thread.AllocateNamedDataSlot("sleeptime");		

            LocalDataStoreSlot myData;
            myData = Thread.GetNamedDataSlot("sleeptime");

            // Set the named data slot equal to the random number created above
            Thread.SetData(myData, _sleepTime);

            // We need to pull the value from TLS here
            LocalDataStoreSlot myTLSValue;

            // We call GetNamedDataSlot to retrieve the value from TLS
            myTLSValue = Thread.GetNamedDataSlot("sleeptime");
            // This returns an object so we need to cast it to an int
            _sleepTime = (int)Thread.GetData(myTLSValue);

            //Thread.FreeNamedDataSlot("sleeptime");			

        }

        /// <summary>
        /// Get storage ThreadSetting
        /// </summary>
        public CacheSettingState ThreadSettingState()
        {
            if (initilized)
            {
                return CacheSettingState.Started;
            }
            return CacheSettingState.Stoped;
        }


        ///// <summary>
        ///// Sync All Tables in SyncTables list.
        ///// </summary>
        //public void SyncRegisteredTables()
        //{
        //    if (_SyncTables == null || _SyncTables.Count == 0)
        //        return;
        //    foreach (SyncSource o in _SyncTables)
        //    {
        //        if (o.syncTime.SyncType == SyncType.Event)
        //        {
        //            if (watcher.GetEdited(o.MappingName))
        //            {
        //                SyncTableSource(o);
        //                watcher.UpdateEdited(o.MappingName);
        //            }
        //        }
        //        //else if (o.syncTime.SyncType == SyncType.None)
        //        //    SyncTableSource(o);
        //        else if (o.syncTime.HasTimeToRun())
        //        {
        //            SyncTableSource(o);
        //        }
        //    }
        //}

        ///// <summary>
        ///// SyncContains
        ///// </summary>
        ///// <param name="mappingName"></param>
        ///// <returns></returns>
        //public bool SyncContains(string mappingName)
        //{
        //    if (_SyncTables == null || _SyncTables.Count == 0)
        //        return false;
        //    foreach (SyncSource o in _SyncTables)
        //    {
        //        if (o.MappingName.Equals(mappingName))
        //            return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// SyncTableSource
        ///// </summary>
        ///// <param name="s"></param>
        //public void SyncTableSource(SyncSource s)
        //{
        //    try
        //    {
        //        //IDbCmd cmd = DbFactory.Create(ConnectionString, Provider);
        //        //SyncTableSource(cmd, s);
        //        SyncTableSource(s);
        //    }
        //    catch(Exception ex)
        //    {
        //        OnDalException(ex.Message, DalCacheError.ErrorCreateCache);
        //    }
        //}

        /// <summary>
        /// Synchronize All Table in Data Source
        /// </summary>
        public void RefreshDataSource()
        {
            try
            {
                //IDbCmd cmd = DbFactory.Create(ConnectionString, Provider);

                foreach (DataTable dt in DS.Tables)
                {
                    DataTable dtSource = dbCmd.ExecuteDataTable(dt.TableName);
                    if (dtSource != null)
                    {
                        this.Store(dtSource, dt.TableName);
                        //SyncTableSource(/*cmd,*/ new SyncSource(dt.TableName,dt.TableName));
                    }
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorSyncCache);
            }
        }


        //private void SyncTableSource(/*IDbCmd cmd,*/SyncSource s)
        //{
        //    try
        //    {

        //        //IDbCmd cmd = Nistec.Data.DBUtil.Create(ConnectionString, Provider);
        //        DataTable dtSource = dbCmd.ExecuteDataTable(s.TableName, "SELECT * FROM " + s.MappingName, s.MissingSchemaAction);
        //        if (!_ds.Tables.Contains(s.TableName))
        //            return;

        //        DataTable dtLocal = _ds.Tables[s.TableName];
        //        if (dtSource.Rows.Count != dtLocal.Rows.Count)
        //        {

        //            this.StoreData(dtSource, s.TableName);
        //            OnSyncDataSource(new SyncDataSourceEventArgs(s.MappingName));
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        OnDalException(ex.Message, DalCacheError.ErrorCreateCache);
        //    }
        //}

        #endregion

        #region AsyncTask


        //object syncRoot;
        //private AsyncCallback onRequestCompleted;
        //private ManualResetEvent resetEvent;
        private delegate object DataTaskItemCallback(string task, object data);


        private object DataTaskItemWorker(string task, object data)
        {
            try
            {
                switch (task.ToLower())
                {
                    case "sizeofdataset":
                        DataSetSize();
                        return 0;
                    case "sizeofdatatable":
                        DataTableSize((DataTable)data);
                        return 0;
                    case "remove":
                        this.RemoveInternal(data.ToString());
                        return 0;
                    case "updatechanges":
                        UpdateChangesInternal(data.ToString());
                        return 0;
                    case "updatechangesdataset":
                        UpdateChangesInternal();
                        return 0;
                    case "setallkeys":
                        SetAllKeysInternal();
                        return 0;
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorUnexpected);
            }
            return 0;
        }


        //private int DataSetSize()
        //{
        //    long size = 0;
        //    //string xmlDataSet = null;

        //    string tempFile = System.IO.Path.GetTempFileName();

        //    System.IO.FileInfo fi = new System.IO.FileInfo(tempFile);
        //    if (fi.Exists)
        //    {

        //        DS.WriteXml(tempFile);
        //        fi.Refresh();
        //        size = fi.Length;

        //        //using(System.IO.StreamReader sr = fi.OpenText()) {

        //        //    xmlDataSet = sr.ReadToEnd();
        //        //    sr.Close();
        //    }

        //    fi.Delete();

        //    _size = (int)size / 1024;

        //    return 0;
        //}

        private void DataSetSize()
        {
            int length = 0;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                DS.WriteXml(ms);
                ms.Flush();
                length =(int) ms.Length;
                ms.Close();
            }


            _size = (int)(length / 1024);

        }

        private void DataTableSize(DataTable dt)
        {
            int length=0;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                dt.WriteXml(ms);
                ms.Flush();
                length = (int)ms.Length;
                ms.Close();
            }


            _size += (int)(length / 1024);

        }

        public static int DataSetSize(DataSet ds)
        {
            int length = 0;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ds.WriteXml(ms);
                ms.Flush();
                length = (int)ms.Length;
                ms.Close();
            }


            return (int)(length / 1024);

        }

        public static int DataSetSize(DataTable dt)
        {
            int length = 0;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                dt.WriteXml(ms);
                ms.Flush();
                length = (int)ms.Length;
                ms.Close();
            }


            return (int)(length / 1024);

        }

        //private void StartAsyncDataTask()
        //{
        //    syncRoot = new object();
        //    resetEvent = new ManualResetEvent(false);
        //}

        object syncRoot = new object();

        /// <summary>
        /// AsyncDataTask
        /// </summary>
        /// <returns></returns>
        private void AsyncInvoke(string task, object data)
        {
            lock (syncRoot)
            {

                DataTaskItemCallback caller = new DataTaskItemCallback(DataTaskItemWorker);

                // Initiate the asychronous call.
                IAsyncResult result = caller.BeginInvoke(task, data, new AsyncCallback(CallbackMethod), caller);

                ////// Poll while simulating work.
                //while (result.IsCompleted == false)
                //{
                //    Thread.Sleep(10);
                //}

                //////result.AsyncWaitHandle.WaitOne();

                ////// Call EndInvoke to wait for the asynchronous call to complete,
                ////// and to retrieve the results.
                //return caller.EndInvoke(result);
            }
        }

        static void CallbackMethod(IAsyncResult ar)
        {
            // Retrieve the delegate.
            DataTaskItemCallback caller = (DataTaskItemCallback)ar.AsyncState;

            // Call EndInvoke to retrieve the results.
            object returnValue = caller.EndInvoke(ar);

            Console.WriteLine("The call executed return value \"{0}\".", returnValue);
        }

        //private IAsyncResult BeginInvoke(string task, object data)
        //{
        //    return BeginInvoke(task, data);
        //}

        //private IAsyncResult BeginInvoke(object state, AsyncCallback callback, string task, object data)
        //{

        //    DataTaskItemCallback caller = new DataTaskItemCallback(DataTaskItemWorker);

        //    if (callback == null)
        //    {
        //        callback = CreateCallBack();
        //    }

        //    // Initiate the asychronous call.  Include an AsyncCallback
        //    // delegate representing the callback method, and the data
        //    // needed to call EndInvoke.
        //    IAsyncResult result = caller.BeginInvoke(task, data, callback, caller);
        //    this.resetEvent.Set();
        //    return result;
        //}

        //private object EndInvoke(IAsyncResult asyncResult)
        //{
        //    // Retrieve the delegate.
        //    DataTaskItemCallback caller = (DataTaskItemCallback)asyncResult.AsyncState;

        //    // Call EndInvoke to retrieve the results.
        //    object o = (string)caller.EndInvoke(asyncResult);

        //    //AsyncCompleted(item);
        //    this.resetEvent.WaitOne();
        //    return o;
        //}

        //private AsyncCallback CreateCallBack()
        //{
        //    if (this.onRequestCompleted == null)
        //    {
        //        this.onRequestCompleted = new AsyncCallback(this.OnRequestCompleted);
        //    }
        //    return this.onRequestCompleted;
        //}

        //private void OnRequestCompleted(IAsyncResult asyncResult)
        //{
        //    this.EndInvoke(asyncResult);
        //}

        #endregion

        #region Keys

        List<string> m_TableList = new List<string>();

        public string[] GetAllKeys()
        {
           return m_TableList.ToArray();
        }
        private void AddKey(string name)
        {
            if (m_TableList.Contains(name))
                return;
            m_TableList.Add(name);
        }
        private void RemoveKey(string name)
        {
            m_TableList.Remove(name);
        }
        public void SetAllKeys()
        {
            AsyncInvoke("setallkeys", null);
        }
         private void SetAllKeysInternal()
        {
            int count = DS.Tables.Count;
            string[] list = new string[count];
            for (int i = 0; i < count; i++)
            {
                list[i] = DS.Tables[i].TableName;
            }
            m_TableList.Clear();
            m_TableList.AddRange( list);
        }
        #endregion

        #region override

        bool dataChanged;
        /// <summary>
        /// Get the size of cache in KB
        /// </summary>
        public int Size
        {
            get
            {
                if (dataChanged)
                {
                    //_size = DataSetSize();
                    AsyncInvoke("sizeofdataset", null);
                    //if (o != null)
                    //{
                    //    _size = (int)o;
                    dataChanged = false;
                    //}
                }
                //int size =  DataSetUtil.DataSetToByteCount(base.DataSource, true) / 1024;
                return _size;
            }
        }

        private void OnSizeChanged()
        {
            dataChanged = true;
        }

        private void OnSizeChanged(DataTable dt)
        {
            AsyncInvoke("sizeofdataset", dt);
            //AsyncDataTask.AsyncTask("sizeofdatatable", dt);
        }

        /// <summary>
        /// On Cache ThreadSetting State Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCacheStateChanged(EventArgs e)
        {
            if (CacheStateChanged != null)
                CacheStateChanged(this, e);
        }

        /// <summary>
        /// On Cache Sync State Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncTimeState(EventArgs e)
        {
            if (SyncTimeState != null)
                SyncTimeState(this, e);
        }

        /// <summary>
        /// On Synchronize Data Source Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncDataSourceChanged(SyncDataSourceChangedEventArgs e)
        {
            if (SyncDataSourceChanged != null)
                SyncDataSourceChanged(this, e);
        }



        /// <summary>
        /// On Data Cache Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataCacheChanged(EventArgs e)
        {
            //if (_ds == null)
            //{
            //    _tableCounts = 0;
            //    _state = DataCacheState.Closed;
            //    goto Label_01;
            //}
            DS.AcceptChanges();

            _tableCounts = DS.Tables.Count;
            if (_tableCounts > 0)
            {
                _state = DataCacheState.Open;
            }
            else
            {
                _state = DataCacheState.Closed;
            }
        //Label_01:
            //_lastSyncTime = DateTime.Now;
            //_syncState = CacheSyncState.Idle;

            if (DataCacheChanged != null)
                DataCacheChanged(this, e);
        }

        /// <summary>
        /// On Data Cache Changing
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataCacheChanging(EventArgs e)
        {
            _state = DataCacheState.Synch;
            if (DataCacheChanging != null)
                DataCacheChanging(this, e);
        }

        /// <summary>
        /// On Data Value Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataValueChanged(EventArgs e)
        {
            if (DataValueChanged != null)
                DataValueChanged(this, e);
        }

        /// <summary>
        /// On DalException
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDalException(DalCacheExceptionEventArgs e)
        {
            if (DalException != null)
                DalException(this, e);
        }

        internal void OnDalException(string msg, DalCacheError err)
        {
            if (DalException != null)
                DalException(this, new DalCacheExceptionEventArgs(msg, err));
        }

        #endregion

        #region data set methods

        /// <summary>
        /// Get copy of storage dataset in xml
        /// </summary>
        public string GetXml()
        {
            //if (_ds == null)
            //    return string.Empty;

            return DS.GetXml();
        }

        /// <summary>
        /// HasChanges
        /// </summary>
        public bool HasChanges()
        {
            //if (_ds == null)
            //    return false;

            return DS.HasChanges();
        }

        /// <summary>
        /// Get copy of storage dataset changes
        /// </summary>
        public DataSet GetChanges()
        {
            //if (_ds == null)
            //    return null;

            return DS.GetChanges();
        }

        /// <summary>
        /// Update all changes in data sorce
        /// </summary>
        /// <returns></returns>
        public void UpdateChanges()//IDbConnection cnn)
        {
           AsyncInvoke("UpdateChangesDataSet", null);
        }
        private int UpdateChangesInternal()//IDbConnection cnn)
        {
            //if (_ds == null)
            //    return 0;
            //IDbCmd cmd = DbFactory.Create(cnn);
            int res = 0;
            foreach (DataTable dt in DS.Tables)
            {
                DataTable dtChanges = dt.GetChanges();
                res += dbCmd.Adapter.UpdateChanges(dt);
                dt.AcceptChanges();
            }
            return res;
        }


        /// <summary>
        /// Update changes for Specific table in data sorce
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void UpdateChanges(/*IDbConnection cnn,*/ string tableName)
        {
            AsyncInvoke("UpdateChanges", tableName);
        }
        private int UpdateChangesInternal(/*IDbConnection cnn,*/ string tableName)
        {
            //if (_ds == null)
            //    return 0;
            DataTable dt = DS.Tables[tableName];
            if (dt == null)
                return 0;

            int res = 0;
            //IDbCmd cmd = DbFactory.Create(cnn);
            res = dbCmd.Adapter.UpdateChanges(dt);
            dt.AcceptChanges();
            return res;
        }

        /// <summary>
        /// Reject changes in data sorce
        /// </summary>
        /// <param name="tableName"></param>
        public void RejectChanges(string tableName)
        {
            //if (_ds == null)
            //    return;
            DataTable dt = DS.Tables[tableName];
            if (dt != null)
                dt.RejectChanges();
        }

        /// <summary>
        /// Reject all changes in data sorce
        /// </summary>
        public void RejectAllChanges()
        {
            //if (_ds == null)
            //    return;
            foreach (DataTable dt in DS.Tables)
            {
                dt.RejectChanges();
            }
        }
        #endregion

        #region Properties

        private DataSet DS
        {
            get 
            {
                if (m_ds == null)
                {
                    m_ds = new DataSet();
                }
                return m_ds; 
            }
            set 
            {
                m_ds = value;
                SetAllKeys();
            }
        }

        /// <summary>
        /// Get copy of storage dataset
        /// </summary>
        public DataSet DataSource
        {
            get { return DS.Copy(); }
        }

        /// <summary>
        /// Get indicating if DalCache are Initilized
        /// </summary>
        public bool Initilized
        {
            get { return this.initilized; }

        }
        /// <summary>
        /// Get or set SleepTime for sync ThreadSetting
        /// </summary>
        public int IntervalSetting
        {
            get { return _sleepTime; }
            set
            {
                if (value > 0)
                {
                    _sleepTime = value;
                }
            }
        }
        /// <summary>
        /// Get or Set SyncOption
        /// </summary>
        public SyncOption SyncOption
        {
            get { return _SyncOption; }
            set
            {
                if (initilized)
                {
                    throw new Exception("Cache was initilized, you can not change SyncOption");
                }
                _SyncOption = value;
            }
        }

        ///// <summary>
        ///// Get or set SyncTime for sync data set in houres
        ///// </summary>
        //public SyncTimer SyncTime
        //{
        //    get { return _syncTime; }
        //    set
        //    {
        //        if (!_syncTime.Equals(value))
        //        {
        //            _syncTime = value;
        //            _nextSyncTime = _syncTime.GetNextValidTime(_lastSyncTime);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Get  the last SyncTime for sync data set
        ///// </summary>
        //public DateTime LastSyncTime
        //{
        //    get { return _lastSyncTime; }

        //}

        ///// <summary>
        ///// Get  the next SyncTime for sync data set
        ///// </summary>
        //public DateTime NextSyncTime
        //{
        //    get 
        //    {
        //        return _nextSyncTime;// _lastSyncTime.AddHours(_syncTime.Hour).AddMinutes(_syncTime.Minute); 
        //    }
        //}

        /// <summary>
        /// Get or set Cache Name  
        /// </summary>
        public string CacheName
        {
            get { return _storageName; }
            set
            {
                if (value != null)
                {
                    _storageName = value;
                }
            }
        }

        /// <summary>
        /// Get DataCacheState  
        /// </summary>
        public DataCacheState DataCacheState
        {
            get { return _state; }
        }

        ///// <summary>
        ///// Get CacheSyncState  
        ///// </summary>
        //public CacheSyncState CacheSyncState
        //{
        //    get { return _syncState; }
        //}
        /// <summary>
        /// Get SyncTables collection
        /// </summary>
        public SyncSourceCollection SyncTables
        {
            get
            {
                return _SyncTables;
            }
        }
        ///// <summary>
        ///// ConnectionString
        ///// </summary>
        //public string ConnectionString
        //{
        //    get { return connectionString; }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            connectionString = value;
        //        }
        //    }
        //}
        ///// <summary>
        ///// DBProvider
        ///// </summary>
        //public DBProvider Provider
        //{
        //    get { return provider; }
        //    set
        //    {
        //        provider = value;
        //    }
        //}


        /// <summary>
        /// Get the ClientId  (MachineName)
        /// </summary>
        public string ClientId
        {
            get { return _ClientId; }
        }

        /// <summary>
        /// Get or set Table Watcher Name
        /// </summary>
        public string TableWatcherName
        {
            get { return _TableWatcherName; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _TableWatcherName = value;
                }
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Suspend layout when Store Data
        /// </summary>
        public void Suspend()
        {
            this.suspend = true;
        }
        /// <summary>
        /// Resume Store Data
        /// </summary>
        /// <param name="resumeCache"></param>
        public void Resume(bool resumeCache)
        {
            this.suspend = false;
            if (_disposed || !resumeCache)
            {
                return;
            }
            try
            {
                lock (DS)
                {
                    OnDataCacheChanging(EventArgs.Empty);
                    OnDataCacheChanged(EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorStoreData);
            }
        }

        /// <summary>
        /// Write Cache To Xml file
        /// </summary>
        /// <param name="fileName">full file name</param>
        /// <param name="mode">XmlWriteMode</param>
        public void CacheToXml(string fileName, XmlWriteMode mode)
        {
            if (fileName.Length == 0)
            {
                OnDalException("fileName Required", DalCacheError.ErrorFileNotFound);
                throw new ArgumentException("fileName Required");
            }
            if (!_disposed)
            {
                DS.WriteXml(fileName, mode);
            }
        }

        /// <summary>
        /// Write Cache To XmlSchema file
        /// </summary>
        /// <param name="fileName">full file name</param>
        public void CacheToXmlSchema(string fileName)
        {
            if (fileName.Length == 0)
            {
                OnDalException("fileName Required", DalCacheError.ErrorFileNotFound);
                throw new ArgumentException("fileName Required");
            }
            if (!_disposed)
            {
                DS.WriteXmlSchema(fileName);
            }
        }

        public DataCacheItem GetItemProperties(string tableName)
        {
            DataTable dt = DataSource.Tables[tableName];
            if (dt == null)
                return null;
            if (this._SyncTables.Contains(tableName))
            {
                return new DataCacheItem(dt.Copy(), this._SyncTables[tableName]);
            }
            else
            {
                return new DataCacheItem(dt.Copy(), tableName);
            }
        }

        public DataCacheItem[] GetCacheProperties()
        {
            List<DataCacheItem> items = new List<DataCacheItem>();
            for (int i = 0; i < DataSource.Tables.Count; i++)
            {
                DataTable dt = DataSource.Tables[i];
                if (dt == null)
                    continue;
                string tableName = dt.TableName;
                if (this._SyncTables.Contains(tableName))
                {
                    items.Add(new DataCacheItem(dt.Copy(), this._SyncTables[tableName]));
                }
                else
                {
                    items.Add(new DataCacheItem(dt.Copy(), tableName));
                }
            }
            return items.ToArray();
        }

        #endregion

        #region Create Cache
        /// <summary>
        /// Create Cache from xml file
        /// </summary>
        /// <param name="fileName">full file name</param>
        /// <param name="mode">XmlReadMode</param>
        /// <param name="storageName">storage Name</param>
        public void CreateCache(string fileName, XmlReadMode mode, string storageName)
        {
            try
            {
                if (fileName.Length == 0)
                {
                    throw new ArgumentException("fileName Required");
                }
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                lock (DS)
                {
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    _storageName = storageName;
                    DS.ReadXml(fileName, mode);
                    DS.DataSetName = _storageName;
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorCreateCache);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Create new storage from data set
        /// </summary>
        /// <param name="ds">data set</param>
        /// <param name="storageName">storage Name</param>
        public void CreateCache(DataSet ds, string storageName)
        {
            try
            {
                //if (ds == null)
                //    return;
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                lock (DS)
                {
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    _storageName = storageName;
                    DS = ds.Copy();
                    DS.DataSetName = _storageName;
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorCreateCache);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        ///// <summary>
        ///// Create Sync storage with data set source
        ///// </summary>
        ///// <param name="ds">data set source</param>
        ///// <param name="storageName">storage Name</param>
        //public void SyncCache(DataSet ds,string storageName)
        //{
        //    try
        //    {
        //        if(_disposed)
        //        {
        //            _ds=new DataSet();
        //        }
        //        lock(_ds)
        //        {
        //            if (!suspend)
        //                OnDataCacheChanging(EventArgs.Empty);
        //            _storageName=storageName;
        //            _ds=ds;
        //            _ds.DataSetName=_storageName;
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        OnDalException(ex.Message,DalCacheError.ErrorSyncCache);
        //    }
        //    finally
        //    {
        //        if (!suspend)
        //            OnDataCacheChanged(EventArgs.Empty);
        //    }		
        //}

        /// <summary>
        /// Store data to storage
        /// </summary>
        /// <param name="tables">array of data tables to add into storage</param>
        /// <param name="tablesName">array of table names</param>
        public bool Add(DataTable[] tables, string[] tablesName)
        {
            try
            {
                if (tables == null)
                    return false; ;
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                if (tables.Length != tablesName.Length)
                {
                    throw new ArgumentException("tables length must be equal to tablesName length");
                }
                lock (DS)
                {
                    int i = 0;
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    foreach (DataTable dt in tables)
                    {
                        StoreDataInternal(dt, tablesName[i]);
                        i++;
                    }
                    OnSizeChanged();

                    return true;
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }


        ///// <summary>
        ///// StoreData
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <param name="tableName"></param>
        ///// <param name="mappingName"></param>
        ///// <param name="sechemaAction"></param>
        ///// <param name="addToSyncTable"></param>
        //public void Add(DataTable dt, string tableName,string mappingName,MissingSchemaAction sechemaAction , bool addToSyncTable)
        //{
        //    Add(dt, tableName);
        //    if (addToSyncTable)
        //    {
        //        if (!SyncContains(mappingName))
        //        {
        //            SyncTables.Add(new SyncSource(tableName, mappingName, false, sechemaAction));
        //        }
        //    }
        //}

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        public bool Add(DataTable dt, string tableName)
        {
            return Add(dt, tableName, false);
        }

        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        /// <param name="force">should replace data table if exists</param>
        public bool Add(DataTable dt, string tableName, bool force)
        {
            try
            {
                if (dt == null)
                    return false;
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                lock (DS)
                {
                    if (DS.Tables.Contains(tableName))
                    {
                        if (force)
                        {
                            DS.Tables.Remove(tableName);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    //if(_ds.Tables.Contains(tableName))
                    //{
                    //    _ds.Tables.Remove(tableName);
                    //}
                    dt.TableName = tableName;
                    DS.Tables.Add(dt);
                    AddKey(tableName);
                    OnSizeChanged(dt);
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorStoreData);
                return false;
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Remove data table  from storage
        /// </summary>
        /// <param name="tableName">table name</param>
        public void Remove(string tableName)
        {
            AsyncInvoke("remove", tableName);
        }
        private void RemoveInternal(string tableName)
        {
            try
            {
                //if (_disposed)
                //{
                //    return;
                //}
                if (DS.Tables.Contains(tableName))
                {
                    lock (DS)
                    {
                        if (!suspend)
                            OnDataCacheChanging(EventArgs.Empty);
                        DS.Tables.Remove(tableName);
                        RemoveKey(tableName);
                        OnSizeChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorStoreData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        #endregion

        #region Stor Data

        /// <summary>
        /// Store data to storage
        /// </summary>
        /// <param name="tables">array of data tables to add into storage</param>
        /// <param name="tablesName">array of table names</param>
        public void Store(DataTable[] tables, string[] tablesName)
        {
            try
            {
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                if (tables.Length != tablesName.Length)
                {
                    throw new ArgumentException("tables length must be equal to tablesName length");
                }
                lock (DS)
                {
                    int i = 0;
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    foreach (DataTable dt in tables)
                    {
                        StoreDataInternal(dt, tablesName[i]);
                        i++;
                    }
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorStoreData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        private void StoreDataInternal(DataTable dt, string tableName)
        {
            if (dt == null)
                return;
            if (DS.Tables.Contains(tableName))
            {
                DS.Tables.Remove(tableName);
            }
            dt.TableName = tableName;
            DS.Tables.Add(dt);
            AddKey(tableName);
        }


        /// <summary>
        /// Store data table to storage
        /// </summary>
        /// <param name="dt">data table to add into the storage</param>
        /// <param name="tableName">table name</param>
        public void Store(DataTable dt, string tableName)
        {
            try
            {
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                if (dt == null)
                    return;

                lock (DS)
                {
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    if (DS.Tables.Contains(tableName))
                    {
                        DS.Tables.Remove(tableName);
                    }
                    dt.TableName = tableName;
                    DS.Tables.Add(dt);
                    AddKey(tableName);
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorStoreData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }


        #endregion

        #region Merge

        /// <summary>
        /// Merge Cache with data set source
        /// </summary>
        /// <param name="ds">data set source</param>
        /// <param name="storageName">storage Name</param>
        /// <param name="preserveChanges">preserving or discarding changes in the data set </param>
        public void Merge(DataSet ds, string storageName, bool preserveChanges)
        {
            try
            {
                //if (ds == null)
                //    return;
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                lock (DS)
                {
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    _storageName = storageName;
                    DS.Merge(ds.Copy(), preserveChanges);
                    DS.DataSetName = _storageName;
                    SetAllKeys();
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorMergeData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Merge Cache with data table source
        /// </summary>
        /// <param name="dt">merges data table into storage</param>
        /// <param name="tableName">tablename</param>
        /// <param name="preserveChanges">preserving or discarding changes in the data set </param>
        /// <param name="schemaAction">handling an incopatible schema accordingto the given arguments </param>
        public void Merge(DataTable dt, string tableName, bool preserveChanges, MissingSchemaAction schemaAction)
        {
            try
            {
                //if (dt == null)
                //    return;
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                lock (DS)
                {
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    dt.TableName = tableName;
                    DS.Merge(dt.Clone(), preserveChanges, schemaAction);
                    SetAllKeys();
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorMergeData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Merge Cache with data table source
        /// </summary>
        /// <param name="dt">merges data table into storage</param>
        /// <param name="tableName">tablename</param>
        /// <param name="preserveChanges">preserving or discarding changes in the data set </param>
        /// <param name="schemaAction">handling an incopatible schema accordingto the given arguments </param>
        public void Merge(DataTable dt, string tableName)
        {
            try
            {
                //if (dt == null)
                //    return;
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                lock (DS)
                {
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    dt.TableName = tableName;
                    DS.Merge(dt.Clone());
                    AddKey(tableName);
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorMergeData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Merge Cache with data rows source
        /// </summary>
        /// <param name="rows">merges an array of data rows source into storage </param>
        /// <param name="preserveChanges">preserving or discarding changes in the data set </param>
        /// <param name="schemaAction">handling an incopatible schema accordingto the given arguments </param>
        public void Merge(DataRow[] rows, bool preserveChanges, MissingSchemaAction schemaAction)
        {
            try
            {
                //if (rows == null)
                //    return;
                //if (_disposed)
                //{
                //    _ds = new DataSet();
                //}
                lock (DS)
                {
                    if (!suspend)
                        OnDataCacheChanging(EventArgs.Empty);
                    DS.Merge(rows, preserveChanges, schemaAction);
                    OnSizeChanged();
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorMergeData);
            }
            finally
            {
                if (!suspend)
                    OnDataCacheChanged(EventArgs.Empty);
            }
        }
        #endregion

        #region get and set values

        /// <summary>
        /// Set Value into specific row and column in local data table  
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="value">value to set</param>
        public void SetValue(string tableName, string column, string filterExpression, object value)
        {
            DataRow dr = GetDataRow(tableName, filterExpression);
            try
            {
                if (dr != null)
                {
                    lock (dr)
                    {
                        dr[column] = value;
                    }
                    DS.Tables[tableName].AcceptChanges();
                    OnDataValueChanged(EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorSetValue);
            }
        }

        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>string value</returns>
        public string GetValue(string tableName, string column, string filterExpression, string defaultValue)
        {
            object val = GetValue(tableName, column, filterExpression);
            if (val == null)
                return null;
            try
            {
                return (string)val.ToString();
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorCastingValue);
                return defaultValue;
            }
        }
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>DateTime value</returns>
        public DateTime GetValue(string tableName, string column, string filterExpression, DateTime defaultValue)
        {
            object val = GetValue(tableName, column, filterExpression);
            if (val == null)
                return defaultValue;
            try
            {
                return Types.ToDateTime(val);
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorCastingValue);
                return defaultValue;
            }
        }
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>bool value</returns>
        public bool GetValue(string tableName, string column, string filterExpression, bool defaultValue)
        {
            object val = GetValue(tableName, column, filterExpression);
            if (val == null)
                return defaultValue;
            try
            {
                return Types.ToBool(val, defaultValue);
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorCastingValue);
                return defaultValue;
            }
        }
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>double value</returns>
        public double GetValue(string tableName, string column, string filterExpression, double defaultValue)
        {
            object val = GetValue(tableName, column, filterExpression);
            if (val == null)
                return defaultValue;
            try
            {
                return Types.ToDouble(val, defaultValue);
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorCastingValue);
                return defaultValue;
            }
        }
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>decimal value</returns>
        public decimal GetValue(string tableName, string column, string filterExpression, decimal defaultValue)
        {
            object val = GetValue(tableName, column, filterExpression);
            if (val == null)
                return defaultValue;
            try
            {
                return Types.ToDecimal(val, defaultValue);
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorCastingValue);
                return defaultValue;
            }
        }
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="defaultValue">defaultValue to return if not found or error occured</param>
        /// <returns>int value</returns>
        public int GetValue(string tableName, string column, string filterExpression, int defaultValue)
        {
            object val = GetValue(tableName, column, filterExpression);
            if (val == null)
                return defaultValue;
            try
            {
                return Types.ToInt(val, defaultValue);
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorCastingValue);
                return defaultValue;
            }
        }
        /// <summary>
        /// Get single value from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="column">column name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>object value</returns>
        public object GetValue(string tableName, string column, string filterExpression)
        {
            DataRow dr = GetDataRow(tableName, filterExpression);
            if (dr == null)
                return null;
            try
            {
                return dr[column];
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorColumnNotExist);
                return null;
            }
        }

        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>Hashtable object</returns>
        public IDictionary GetRow(string tableName, string filterExpression)
        {
            DataRow dr = GetDataRow(tableName, filterExpression);
            if (dr == null)
                return null;
            return DataUtil.DataRowToHashtable(dr);
        }


        /// <summary>
        /// Get single data row from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <returns>Data row object</returns>
        public DataRow GetDataRow(string tableName, string filterExpression)
        {
            DataRow[] drs = GetDataRows(tableName, filterExpression, "");
            if (drs == null || drs.Length == 0)
                return null;
            return drs[0];
        }
        /// <summary>
        /// Get array of dataRows from storage by filter Expression,if no rows found by filter Expression return null.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="filterExpression">filter Expression</param>
        /// <param name="sort">sort fileld</param>
        /// <returns>array of data rows </returns>
        public DataRow[] GetDataRows(string tableName, string filterExpression, string sort)
        {
            if (_disposed || _tableCounts == 0)
            {
                OnDalException("DatSet is disposed", DalCacheError.ErrorInitilaized);
                return null;
            }
            DataRow[] drs = null;
            DataTable dt = null;

            dt = DS.Tables[tableName];
            if (dt == null)
            {
                OnDalException("Table " + tableName + " not found. ", DalCacheError.ErrorTableNotExist);
                return null;
            }
            try
            {
                drs = dt.Select(filterExpression, sort);
                return drs;
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorInFilterExspression);
                return null;
            }
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(string tableName)
        {
            if (_disposed || _tableCounts == 0)
            {
                OnDalException("DatSet is disposed", DalCacheError.ErrorInitilaized);
                return null;
            }
            try
            {
                return DS.Tables[tableName];
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorTableNotExist);
                return null;
            }
        }

        /// <summary>
        /// Get DataTable from storage by table name.
        /// </summary>
        /// <param name="index">table index</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(int index)
        {
            if (_disposed || _tableCounts == 0)
            {
                OnDalException("DatSet is disposed", DalCacheError.ErrorInitilaized);
                return null;
            }
            try
            {
                return DS.Tables[index];
            }
            catch (Exception ex)
            {
                OnDalException(ex.Message, DalCacheError.ErrorTableNotExist);
                return null;
            }
        }

        /// <summary>
        /// Get DataView from storage by table name.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns>DataView</returns>
        public DataView this[string tableName]
        {
            get
            {
                DataTable dt = GetDataTable(tableName);
                if (dt == null)
                    return null;
                return dt.DefaultView;
            }
        }

        /// <summary>
        /// Get DataView from storage by table index.
        /// </summary>
        /// <param name="index">table index</param>
        /// <returns>DataView</returns>
        public DataView this[int index]
        {
            get
            {
                DataTable dt = GetDataTable(index);
                if (dt == null)
                    return null;
                return dt.DefaultView;
            }
        }


        #endregion

    }
}
