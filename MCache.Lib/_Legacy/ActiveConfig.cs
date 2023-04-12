using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using System.Collections;
//using Nistec.Data.Common._Dal;
using System.Threading;
using Nistec.Data;

namespace Nistec.Legacy
{
   

    #region SyncDataEventsArgs

    public delegate void SyncDataEventHandler(object sender, SyncDataEventArgs e);

    public class SyncDataEventArgs : EventArgs
    {
        private DataTable data;

        public SyncDataEventArgs(DataTable data)
        {
            this.data = data;
        }

        #region Properties Implementation

        /// <summary>
        /// Get Table
        /// </summary>
        public DataTable Table
        {
            get { return this.data; }
        }
         #endregion

    }

    #endregion


    /// <summary>
    /// ActiveConfig base on DataTable ,
    /// include columns ConfigKey,ConfigValue,ConfigSection when ConfigKey is a Primary key
    /// </summary>
    public class ActiveConfig : ActiveCommandBase, IActiveConfig//,IDisposable
    {
        #region Active command

        //private ActiveCommand _Command;

        //protected ActiveCommand Command
        //{
        //    get
        //    {
        //        if (_Command == null)
        //        {
        //            _Command = new ActiveCommand();
        //            _Command.CommandCompleted += new EventHandler(_Command_CommandCompleted);
        //        }
        //        return _Command;
        //    }
        //}

        //void _Command_CommandCompleted(object sender, EventArgs e)
        //{
        //    OnSyncData(new SyncDataEventArgs(_Command.DataSource));

        //    InitDictionary();
        //}

        protected override void OnAsyncCompleted(Nistec.Threading.AsyncDataResultEventArgs e)
        {

            base.OnAsyncCompleted(e);

        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);

            InitDictionary();
        }

        #endregion

        #region memebers and ctor

        //private DataTable dtSource;
        private int rowChanges;
        private Hashtable hashAsync;
        private bool dirty;
        private string keyName="ConfigKey";
        private string valueName="ConfigValue";
        private string sectionName = "ConfigSection";

        //private string connectionString;
        //private string mappingName;

        public const string DefaultSection = "General";

        /// <summary>
        /// ActiveConfig ctor
        /// </summary>
        /// <param name="dt"></param>
        public ActiveConfig(DataTable dt)
        {
            rowChanges = 0;
            dirty = false;
            //dtSource = dt;
            base.SyncTable(dt,false);
            InitDictionary();
        }

        /// <summary>
        /// ActiveConfig ctor
        /// </summary>
        /// <param name="dalBase"></param>
        /// <param name="mappingName"></param>
        public ActiveConfig(IAutoBase dalBase, string mappingName)
            : this(dalBase.Connection, mappingName)
        {
        }

        /// <summary>
        /// ActiveConfig ctor
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="mappingName"></param>
        public ActiveConfig(IDbConnection cnn, string mappingName)
            : this(cnn, mappingName, "ConfigKey", "ConfigValue")
        { }
        /// <summary>
        /// ActiveConfig ctor
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="mappingName"></param>
        public ActiveConfig(IDbConnection cnn,string mappingName,string keyName,string valueName)
        {
            rowChanges = 0;
            dirty = false;
            this.keyName = keyName;
            this.valueName = valueName;
            base.Init(cnn, mappingName);
            //this.connectionString = cnn.ConnectionString;
            //this.mappingName = mappingName;
            base.AsyncExecute();
            //InitDictionary();

            //using (IDBCmd cmd = DBFactory.Create(cnn))
            //{
            //    dtSource = cmd.ExecuteDataTable("Config", "Select * from " + mappingName);
            //    InitDictionary();
            //}
        }

        /// <summary>
        /// ActiveConfig ctor
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="mappingName"></param>
        public ActiveConfig(string connectionString,DBProvider provider, string mappingName, string keyName, string valueName)
        {
            rowChanges = 0;
            dirty = false;
            this.keyName = keyName;
            this.valueName = valueName;
            base.Init(connectionString, provider,mappingName);
            base.AsyncExecute();
        }
        ~ActiveConfig()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            if (hashAsync != null)
            {
                hashAsync.Clear();
                hashAsync = null;
            }
            base.Dispose();

            //if (_Command != null)
            //{
            //    _Command.Dispose();
            //    _Command = null;
            //} 
            //if (dtSource != null)
            //{
            //    dtSource.Dispose();
            //    dtSource = null;
            //}

        }

        ///// <summary>
        ///// IsEmpty
        ///// </summary>
        //public bool IsEmpty
        //{
        //    get { return Command.IsEmpty; }
        //}
        /// <summary>
        /// IsDirty
        /// </summary>
        public bool IsDirty
        {
            get { return dirty; }
        }

        /// <summary>
        /// Get Copy of Data table source
        /// </summary>
        public DataTable Copy
        {
            get 
            {
                if (IsEmpty)
                    return null;
                return base.DataSource.Copy(); 
            }
        }
        /// <summary>
        /// Get KeyName
        /// </summary>
        public string KeyName
        {
            get { return keyName; }
        }
        /// <summary>
        /// Get ValueName
        /// </summary>
        public string ValueName
        {
            get { return valueName; }
        }

        /// <summary>
        /// Get SectionName
        /// </summary>
        public string SectionName
        {
            get { return sectionName; }
        }
        ///// <summary>
        ///// Get MappingName
        ///// </summary>
        //public string MappingName
        //{
        //    get { return mappingName; }
        //}

        ///// <summary>
        ///// Get ConnectionString
        ///// </summary>
        //internal string ConnectionString
        //{
        //    get { return connectionString; }
        //}
        #endregion

        #region auto sync

        //private int interval;
        //private bool initilized = false;
        //private Thread thSync;
        public event SyncDataEventHandler SyncData;
        System.Timers.Timer aTimer;

        /// <summary>
        /// Get indicator if sync are enabled 
        /// </summary>
        public bool SyncEnabled
        {
            get
            {
                if (aTimer == null)
                    return false;
                return aTimer.Enabled;
            }
        }

        /// <summary>
        /// Start Async config Background multi thread Listner 
        /// </summary>
        protected virtual void StartAsync(TimeSpan  interval)
        {
            if (SyncEnabled)
                return;
            //this.interval = interval.TotalMilliseconds;
   
            try
            {

                aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);

                aTimer.Interval = interval.TotalMilliseconds;
                aTimer.Enabled = true;
                //initilized = true;
                // Keep the timer alive until the end of Main.
                GC.KeepAlive(aTimer);

                //thSync = new Thread(new ThreadStart(ConfigListner));
                //thSync.IsBackground = true;
                //initilized = true;
                //thSync.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
        }


        /// <summary>
        /// Stop AsyncQueue Background multi thread Listner 
        /// </summary>
        protected virtual void StopAsync()
        {
            Console.WriteLine("Stop Async config ");
            if (aTimer != null)
            {
                aTimer.Stop();
            }
            //initilized = false;
            //thSync.Abort();
        }

        ///// <summary>
        ///// Message Queue Listner worker thread
        ///// </summary>
        //private void ConfigListner()
        //{
        //    Console.WriteLine("Create ConfigListner...");

        //    while (initilized)
        //    {
        //        OnSyncData();
               
        //        Thread.Sleep(interval);
        //    }
        //}


        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Start sync ...");
            //OnSyncData();
            base.AsyncExecute();

        }



        /// <summary>
        /// OnSyncData
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncData(SyncDataEventArgs e)
        {
            //LoadData(e.Table);

            if (SyncData != null)
            {
                SyncData(this, e);
            }
        }

        //private void OnSyncData()
        //{
        //    DataTable dt = null;

        //    using (IDBCmd cmd = DBFactory.Create(connectionString, DBProvider.SqlServer))
        //    {
        //        dt = cmd.ExecuteDataTable("Config", "Select * from " + mappingName);
        //    }
        //    if (dt != null && dt.Rows.Count > 0)
        //    {
        //        OnSyncData(new SyncDataEventArgs(dt));
        //    }
        //}

 
        //protected void LoadData(DataTable table)
        //{
        //    if (table == null || table.Rows.Count == 0)
        //        return;
        //    dtSource = table;
        //    InitDictionary();
        //}

        //private void LoadData()
        //{
        //    using (IDBCmd cmd = DBFactory.Create(connectionString, DBProvider.SqlServer))
        //    {
        //        dtSource = cmd.ExecuteDataTable("Config", "Select * from " + mappingName);
        //        InitDictionary();
        //    }
        //}

        #endregion

        #region hashAsync

        static object syncRoot = new object();

        private Hashtable HashAsync
        {
            get
            {
                if (this.hashAsync == null)
                {
                    lock (syncRoot)
                    {
                        if (this.hashAsync == null)
                        {
                            Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
                            System.Threading.Thread.MemoryBarrier();
                            this.hashAsync = hashtable;
                        }
                    }
                }
                return this.hashAsync;
            }
        }
        private void SyncHashtable(Hashtable hash)
        {

            if (this.hashAsync == null)
            {
                Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
                System.Threading.Thread.MemoryBarrier();
                this.hashAsync = hash;
            }

            else //if (this.hashAsync != null)
            {
                lock (hashAsync.SyncRoot)// syncRoot)
                {
                    hashAsync.Clear();
                    this.hashAsync = Hashtable.Synchronized(hash);
                }
            }

        }
        private void InitDictionary()//string keyName, string valueName)
        {
            //if (!string.IsNullOrEmpty(keyName))
            //    this.keyName = keyName;
            //if (!string.IsNullOrEmpty(valueName))
            //    this.valueName = valueName;

            //if (hashAsync != null)
            //{
            //    hashAsync.Clear();
            //    hashAsync = null;
            //}
            //hashAsync = new Hashtable();

            DataRow[] drs = base.Select(null);
            if (drs == null || drs.Length == 0)
                return;

            Hashtable hashtable = Hashtable.Synchronized(new Hashtable());

            foreach (DataRow dr in drs)
            {
                string hashKey = string.Format("{0}_{1}", dr[keyName], dr[sectionName]);
                hashtable[hashKey] = dr[valueName];
            }
            SyncHashtable(hashtable);

            dirty = false;
        }
        #endregion

        #region public methods

        /// <summary>
        /// Add new item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(object key,object value)
        {
            Add(key, value, DefaultSection);
        }
        /// <summary>
        /// Add new item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="section"></param>
        public void Add(object key, object value,string section)
        {
            try
            {
                if (IsEmpty)
                    return;
                DataTable dt = base.DataSource;

                string hashKey = string.Format("{0}_{1}", key, section);
                hashAsync.Add(hashKey, value);
                DataRow dr = dt.NewRow();
                dr["ConfigKey"] = key;
                dr["ConfigValue"] = value;
                dr["ConfigSection"] = section;
                dr["ConfigId"] = 0;
                base.DataSource.Rows.Add(dr);
                dirty = true;
            }
            catch(Exception exception)
            {
                throw exception;
            }
        }
        /// <summary>
        /// Get Contains
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(object key)
        {
            return hashAsync.Contains(key);
        }
        /// <summary>
        /// Get ContainsKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(object key)
        {
            return hashAsync.ContainsKey(key);
        }
        /// <summary>
        /// Get ContainsValue
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsValue(object value)
        {
            return hashAsync.ContainsValue(value);
        }
        /// <summary>
        /// Copy To Array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(System.Array array,int index)
        {
            hashAsync.CopyTo(array,index);
        }
        /// <summary>
        /// Get Count
        /// </summary>
        public override int Count
        {
            get { return hashAsync.Count; }
        }
        /// <summary>
        /// Get Keys
        /// </summary>
        public ICollection Keys
        {
            get { return hashAsync.Keys; }
        }
        /// <summary>
        /// Get Values
        /// </summary>
        public ICollection Values
        {
            get { return hashAsync.Values; }
        }
        /// <summary>
        /// Get SyncRoot
        /// </summary>
        public object SyncRoot
        {
            get { return hashAsync.SyncRoot; }
        }
        /// <summary>
        /// Get IsSynchronized
        /// </summary>
        public bool IsSynchronized
        {
            get { return hashAsync.IsSynchronized; }
        }
        /// <summary>
        /// Get or Set ActiveConfig
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section">the section name in data row</param>
        /// <returns></returns>
        public object this[object key,string section]
        {
            get 
            {
                string hashKey = string.Format("{0}_{1}", key, section);
                return hashAsync[hashKey]; 
            }
            set 
            {
                SetValue(key, value, section);
            }
        }
        /// <summary>
        /// Get or Set ActiveConfig
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                return this[key,DefaultSection];
            }
            set
            {
                this[key, DefaultSection] = value;
            }
        }

        /// <summary>
        /// Refresh
        /// </summary>
        public virtual void Refresh()
        {

        }

        /// <summary>
        /// ActiveConfig ctor
        /// </summary>
        /// <param name="dt"></param>
        private void Refresh(DataTable dt)
        {
            rowChanges = 0;
            dirty = false;
            base.SyncTable(dt,false);
            //dtSource = dt;
            InitDictionary();
        }

        //public DataTable Table
        //{
        //    get { return base.DataSource; }
        //    set { Refresh(value); }
        //}
        #endregion

        #region set Values

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <returns>object</returns>
        public void SetValue(object key, object value,string section)
        {
            //if (dtSource == null || dtSource.Rows.Count == 0)
            //    return;
            if (IsEmpty)
                return;
            bool found = false;
            foreach (DataRow dr in base.DataSource.Rows)
            {
                if (dr[keyName] == key && dr[sectionName].ToString() == section)
                {
                    dr[valueName] = value;
                    rowChanges ++;
                    dirty = true;
                    found = true;

                    string hashKey = string.Format("{0}_{1}", key, section);
                    hashAsync[hashKey] = value;

                    break;
                }
            }

            if (!found)
            {
                Add(key, value, section);
            }
        }


        ///// <summary>
        ///// UpdateChanges 
        ///// </summary>
        ///// <returns></returns>
        //public int UpdateChanges()
        //{
        //        return base.Update();
        //}

        ///// <summary>
        ///// UpdateChanges
        ///// </summary>
        ///// <param name="connectionString"></param>
        ///// <param name="provider"></param>
        ///// <returns></returns>
        //public int UpdateChanges(string connectionString, DBProvider provider)
        //{
        //    using (IDBCmd cmd = DBFactory.Create(connectionString, provider))
        //    {
        //        return UpdateChanges(cmd);
        //    }
        //}

        ///// <summary>
        ///// UpdateChanges 
        ///// </summary>
        ///// <returns></returns>
        //public int UpdateChanges(IDbConnection cnn)
        //{
        //    using (IDBCmd cmd = DBFactory.Create(cnn))
        //    {
        //        return UpdateChanges(cmd);
        //    }
        //}
        ///// <summary>
        ///// UpdateChanges
        ///// </summary>
        ///// <param name="cmd"></param>
        ///// <returns></returns>
        //public int UpdateChanges(IDBCmd cmd)
        //{
        //    //IDBCmd cmd = DBUtil.Create(cnn);
        //    if (cmd == null || cmd.Connection == null)
        //    {
        //        throw new Exception("IDBCmd not initilaized");
        //    }
        //    if (rowChanges == 0)
        //        return 0;
        //    int res = 0;
        //    try
        //    {
        //        res = cmd.UpdateChanges(dtSource);
        //        rowChanges = 0;
        //        InitDictionary();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    return res;
        //}

        #endregion

        #region Values

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="section">the section name in data row</param>
        /// <returns></returns>
        public object GetValue(string key,string section)
        {
            DataRow[] drs = base.Select(string.Format("{0}='{1}' And {2}='{3}'", keyName, key, sectionName, section));
            if (drs == null || drs.Length == 0)
                return null;
            return drs[0][valueName];
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue(string key)
        {
            return GetValue(key,DefaultSection);
        }

        /// <summary>
        /// GetValues
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Record[] GetValues(string section)
        {
            DataRow[] drs = base.Select(string.Format("{0}='{1}'", sectionName, section));
            if (drs == null || drs.Length == 0)
                return null;

            Record[] records = new Record[drs.Length];
            int i = 0;
            foreach (DataRow dr in drs)
            {
                records[i] = new Record(dr[keyName], dr[valueName]);
                i++;
            }
            return records;
        }
        /// <summary>
        /// GetValue int
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <returns>int,if null or error return 0<</returns>
        public int GetIntValue(string key, string section)
        {
            return (int)GetValue(key, section, (int)0);
        }
        /// <summary>
        /// GetValue decimal
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <returns>decimal ,if null or error return 0</returns>
        public decimal GetDecimalValue(string key, string section)
        {
            return (decimal)GetValue(key, section, (decimal)0);
        }
        /// <summary>
        /// GetValue double
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <returns>double ,if null or error return 0<</returns>
        public double GetDoubleValue(string key, string section)
        {
            return (double)GetValue(key, section, (double)0);
        }
        /// <summary>
        /// GetValue bool
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <returns>bool,if null or error return false<</returns>
        public bool GetBoolValue(string key, string section)
        {
            return (bool)GetValue(key, section, (bool)false);
        }
        /// <summary>
        /// GetValue string
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <returns>string,if null or error return ""<</returns>
        public string GetStringValue(string key, string section)
        {
            return (string)GetValue(key, section, (string)"");
        }
        /// <summary>
        /// GetValue DateTime
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <returns>DateTime ,if null or error return Now<</returns>
        public DateTime GetDateValue(string key, string section)
        {
            return (DateTime)GetValue(key, section, DateTime.Now);
        }


        /// <summary>
        /// GetValue int
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>int,if null or error return defaultValue<</returns>
        public int GetValue(string key, string section, int defaultValue)
        {
            return (int)Types.NZ(GetValue(key, section), (int)defaultValue);
        }
        /// <summary>
        /// GetValue decimal
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>decimal,if null or error return defaultValue</returns>
        public decimal GetValue(string key, string section, decimal defaultValue)
        {
            return (decimal)Types.NZ(GetValue(key, section), (decimal)defaultValue);
        }
        /// <summary>
        /// GetValue double
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>double,if null or error return defaultValue</returns>
        public double GetValue(string key, string section, double defaultValue)
        {
            return (double)Types.NZ(GetValue(key, section), (double)defaultValue);
        }
        /// <summary>
        /// GetValue bool
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>bool,if null or error return defaultValue</returns>
        public bool GetValue(string key, string section, bool defaultValue)
        {
            return (bool)Types.NZ(GetValue(key, section), (bool)defaultValue);
        }
        /// <summary>
        /// GetValue string
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>string,if null or error return defaultValue</returns>
        public string GetValue(string key,string section, string defaultValue)
        {
            return (string)Types.NZ(GetValue(key,section), (string)defaultValue);
        }
        /// <summary>
        /// GetValue DateTime
        /// </summary>
        /// <param name="key">the column name in data row</param>
        /// <param name="section">the section name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>DateTime,if null or error return defaultValue</returns>
        public DateTime GetValue(string key, string section, DateTime defaultValue)
        {
            return (DateTime)Types.NZ(GetValue(key, section), defaultValue);
        }

        #endregion

    }
}
