using Nistec.Data;
using Nistec.Data.Factory;
using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;


namespace Nistec.Legacy
{

    [Serializable]
    public class ActiveCommand:ActiveCommandBase
    {
        public ActiveCommand()
        {
           
        }

        #region override

        //protected virtual bool ValidCurrent()
        //{
        //    if (_DataSource == null)
        //    {
        //        return false;
        //    }
        //    return (_currentIndex >= 0 && _currentIndex < Count);
        //}

        //protected virtual bool ValidCurrent(bool raisError)
        //{
        //    if (!ValidCurrent())
        //    {
        //        if (raisError)
        //            throw new ArgumentException("Invalid Data or index out of range");
        //        else
        //            return false;
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// Get Count
        ///// </summary>
        ///// <returns>int</returns>
        //public override int Count
        //{
        //    get
        //    {
        //        if (IsEmpty)
        //            return 0;
        //        return _DataView.Count;
        //    }
        //}

        //private int _currentIndex = -1;
        ///// <summary>
        ///// Get or Set the current position
        ///// </summary>
        //public override int Position
        //{
        //    get
        //    {
        //        //if (_DataView == null)
        //        //{
        //        //    return -1;
        //        //}
        //        return _currentIndex;
        //    }
        //    set
        //    {
        //        //if (_DataView == null)
        //        //{
        //        //    return;
        //        //}
        //        //if (value > Count)
        //        //{
        //        //    throw new ArgumentException("Index out of range ", value.ToString());
        //        //}
        //        _currentIndex = value;
        //    }
        //}

        ///// <summary>
        ///// Get or Set ItemArray
        ///// </summary>
        //public override object[] ItemArray
        //{
        //    get
        //    {
        //        if (!ValidCurrent()) return null;
        //        return _DataView[_currentIndex].Row.ItemArray;
        //    }
        //    set
        //    {
        //        if (!ValidCurrent()) return;
        //        _DataView[_currentIndex].Row.ItemArray = value;
        //    }
        //}
        #endregion

    }

    [Serializable]
    public abstract class ActiveCommandBase
    {

        #region abstract

        ///// <summary>
        ///// Get Count
        ///// </summary>
        ///// <returns>int</returns>
        //public abstract int Count { get;}
        ///// <summary>
        ///// Get or Set the current position
        ///// </summary>
        //public abstract int Position { get;set;}
        ///// <summary>
        ///// Get or Set ItemArray
        ///// </summary>
        //public abstract object[] ItemArray { get;set;}

        #endregion

        #region override

        protected virtual bool ValidCurrent()
        {
            if (_DataSource == null)
            {
                return false;
            }
            return (_currentIndex >= 0 && _currentIndex < Count);
        }
        protected virtual bool ValidCurrent(bool validateEdit, bool raisError)
        {
            if (validateEdit && ReadOnly)
            {
                if (raisError)
                    throw new ArgumentException("Data is ReadOnly");
                else
                    return false;
            }
            return ValidCurrent(raisError);
        }

        protected virtual bool ValidCurrent(bool raisError)
        {
            if (!ValidCurrent())
            {
                if (raisError)
                    throw new ArgumentException("Invalid Data or index out of range");
                else
                    return false;
            }
            return true;
        }

        //protected virtual bool ValidCurrent()
        //{
        //    if (_DataSource == null)
        //    {
        //        return false;
        //    }
        //    return (_currentIndex >= 0 && _currentIndex < Count);
        //}

        //protected virtual bool ValidCurrent(bool raisError)
        //{
        //    if (!ValidCurrent())
        //    {
        //        if (raisError)
        //            throw new ArgumentException("Invalid Data or index out of range");
        //        else
        //            return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// Get Count
        /// </summary>
        /// <returns>int</returns>
        [DalProperty(DalPropertyType.NA)]
        public virtual int Count
        {
            get
            {
                if (IsEmpty)
                    return 0;
                return _DataSource.Rows.Count;
            }
        }

        private int _currentIndex = -1;
        /// <summary>
        /// Get or Set the current position
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public virtual int Position
        {
            get
            {
                //if (_DataView == null)
                //{
                //    return -1;
                //}
                return _currentIndex;
            }
            set
            {
                //if (_DataView == null)
                //{
                //    return;
                //}
                //if (value > Count)
                //{
                //    throw new ArgumentException("Index out of range ", value.ToString());
                //}
                _currentIndex = value;
            }
        }

        /// <summary>
        /// Go to next record
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void Next()
        {
            GoTo(_currentIndex + 1);
        }
        /// <summary>
        /// Go To Position 
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void GoTo(int index)
        {
            if (Count <= index)
            {
                throw new IndexOutOfRangeException("Index out of range "+ index.ToString());
            }
            _currentIndex=index;
        }

        /// <summary>
        /// Get or Set ItemArray
        /// </summary>
        public virtual object[] ItemArray
        {
            get
            {
                if (!ValidCurrent()) return null;
                return _DataSource.Rows[_currentIndex].ItemArray;
            }
            set
            {
                if (!ValidCurrent()) return;
                _DataSource.Rows[_currentIndex].ItemArray = value;
            }
        }
        #endregion

        #region memebers

        DBProvider _DBProvider = DBProvider.SqlServer;
        /// <summary>
        /// Get or Set DBProvider
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public DBProvider ActiveProvider { get { return _DBProvider; } set { _DBProvider = value; } }
        string _ConnectionString;
        /// <summary>
        /// Get or Set ConnectionString
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public string ActiveConnection { get { return _ConnectionString; } set { _ConnectionString = value; } }
        //IDbConnection _Connection;
        ///// <summary>
        ///// Get or Set Connection
        ///// </summary>
        //public IDbConnection Connection { get { return _Connection; } set { _Connection = value; } }
        string _TableName;
        /// <summary>
        /// Get or Set TableName
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public string ActiveTableName { get { return _TableName; } set { _TableName = value; } }
        string _MappingName;
        /// <summary>
        /// Get or Set MappingName
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public string ActiveMappingName { get { return _MappingName; } set { _MappingName = value; } }
        string _CommandText;
        /// <summary>
        /// Get or Set CommandText
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public string ActiveCommandText { get { return _CommandText; } set { _CommandText = value; } }
        
        
        internal DataTable _DataSource;
        /// <summary>
        /// Get or Set DataSource
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public DataTable DataSource
        {
            get { return _DataSource; }
            set { this.SyncTable(value,false); } 
        }
 
        bool _ReadOnly = false;
        /// <summary>
        /// Get or Set ReadOnly
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public bool ReadOnly
        {
            get { return _ReadOnly; }
            set
            {
                _ReadOnly = value;
                OnReadOnlyChanged(EventArgs.Empty);
            }
        }

        private string _Message;
        /// <summary>
        /// Get Execute message
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public string ActiveMessage
        {
            get { return _Message; }
        }
        uint _Timeout = 100000;
        /// <summary>
        /// Get or Set the Timeout in milliseconds,the minimum is 1000 milliseconds
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public uint ActiveTimeout
        {
            get { return _Timeout; }
            set
            {
                if (value > 1000)
                    _Timeout = value;
            }
        }
        bool _IsRunning = false;
        /// <summary>
        /// Get the value indicating that command is running
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        internal bool IsRunning
        {
            get { return _IsRunning; }
        }
        /// <summary>
        /// Get the value indicating that data source IsEmpty
        /// </summary>
        [DalProperty(DalPropertyType.NA)]
        public bool IsEmpty
        {
            get { return _DataSource == null || _DataSource.Rows.Count == 0; }
        }
        #endregion

        /// <summary>
        /// CompareRecord
        /// </summary>
        /// <param name="index"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool CompareRecord(int index, string column, object value)
        {
            return _DataSource.Rows[index][column].Equals(value);
        }


        /// <summary>
        /// Find
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual int Find(string columnName, object value)
        {
            if (IsEmpty)
                return -1;
            int ordinal = _DataSource.Columns[columnName].Ordinal;
            return FindRecord(ordinal, value);
        }

        /// <summary>
        /// FindRecord
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int FindRecord(int column, object value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_DataSource.Rows[i][column].Equals(value))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Gets the index of the specified Row
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual int FindRow(object[] keys)
        {
            if (IsEmpty)
                return -1;
            DataRow dr= GetActiveRow(keys);
            if (dr == null)
                return -1;
            return _DataSource.Rows.IndexOf(dr);
        }

        /// <summary>
        /// Gets the row that contains the specified primary key values.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual DataRow GetActiveRow(object[] keys)
        {
            if (IsEmpty)
                return null;
            return _DataSource.Rows.Find(keys);
        }

        /// <summary>
        /// Gets the current row .
        /// </summary>
        /// <returns></returns>
        public virtual DataRow GetActiveRow()
        {
            if (IsEmpty)
                return null;
            return _DataSource.Rows[Position];
        }

        /// <summary>
        /// Select DataRows array from DataSource by filter
        /// <param name="filter"></param>
        /// <returns></returns>
        public DataRow[] Select(string filter)
        {
            if (IsEmpty)
                return null;
            if (string.IsNullOrEmpty(filter))
                return _DataSource.Select();
            return _DataSource.Select(filter);
        }
        /// <summary>
        /// Select DataRows array from DataSource by filter and sort fields
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public DataRow[] Select(string filter, string sort)
        {
            if (IsEmpty)
                return null;
            return _DataSource.Select(filter, sort);
        }


        AsyncCommand cmd;
        static object syncRoot = new object();

        public event EventHandler CommandCompleted;

        public void Init(string connectioString, DBProvider provider, string mappingName)
        {
            _ConnectionString = connectioString;
            _DBProvider = provider;
            _MappingName = mappingName;
        }
        public void Init(IDbConnection cnn, string mappingName)
        {
            if (cnn == null)
            {
                throw new ArgumentNullException("cnn");
            }

            _ConnectionString = cnn.ConnectionString;
            _DBProvider =DbFactory.GetProvider(cnn);
            _MappingName = mappingName;
        }

        private bool Validate()
        {

            if (string.IsNullOrEmpty(_ConnectionString))
            {
                throw new Exception("Invalid connection");
            }
            if (string.IsNullOrEmpty(_CommandText))
            {
                if (string.IsNullOrEmpty(_MappingName))
                {
                    throw new Exception("Invalid MappingName");
                }
                _CommandText = "select * from " + _MappingName;
            }
            if (string.IsNullOrEmpty(_TableName))
            {
                if (string.IsNullOrEmpty(_MappingName))
                {
                    throw new Exception("Invalid MappingName");
                }
                _TableName = _MappingName;
            }

            return Wait();
        }

        private bool ValidateUpdate()
        {

            if (ReadOnly)
            {
                throw new Exception("DataSource is ReadOnly");
            }
            if (string.IsNullOrEmpty(_ConnectionString))
            {
                throw new Exception("Invalid connection");
            }
            if (IsEmpty)
            {
                throw new Exception("Invalid DataSource");
            }
            if (string.IsNullOrEmpty(ActiveMappingName) && string.IsNullOrEmpty(ActiveTableName))
            {
                throw new Exception("Invalid MappingName");
            }
            return Wait();
        }

        internal bool ValidateReadOnly()
        {
            if (ReadOnly)
            {
                throw new Exception("DataSource is ReadOnly");
            }
            return true;
        }

        private string GetMappingName()
        {
            if (!string.IsNullOrEmpty(ActiveMappingName))
            {
                return ActiveMappingName;
            }
            if (!string.IsNullOrEmpty(ActiveTableName))
            {
                return ActiveTableName;
            }
            return "";
        }

        private bool Wait()
        {
            int count = 0;
            while (_IsRunning)// || cmd.AsyncState == Nistec.Threading.AsyncState.Started)
            {
                System.Threading.Thread.Sleep(10);
                count++;
                if (count > (_Timeout / 10))
                    return false;
            }
            return true;
        }

        public void AsyncExecute()
        {
            if (!Validate()) return;

            if (cmd == null)
            {
                cmd = new AsyncCommand(_ConnectionString, _DBProvider);
                cmd.AsyncProgress += new Nistec.Threading.AsyncProgressEventHandler(cmd_AsyncProgress);
                cmd.AsyncCompleted += new Nistec.Threading.AsyncDataResultEventHandler(cmd_AsyncCompleted);

            }
            lock (syncRoot)
            {
                cmd.AsyncBeginInvoke(_CommandText, _TableName);
                _IsRunning = true;
            }
        }

        public void StopExecution()
        {
            if (cmd != null)
            {
                cmd.StopCurrentExecution();
                _IsRunning = false;
            }
        }

        public void Execute()
        {
            if (!Validate()) return;
            _IsRunning = true;
            DataTable dt = null;
            using (IDbCmd dbCmd = DbFactory.Create(_ConnectionString, _DBProvider))
            {
                dt = dbCmd.ExecuteDataTable(_TableName, _CommandText, true);
            }
            _IsRunning = false;
            SyncTable(dt, false);
        }

        public int Update()//DataTable dt)
        {
            if (!ValidateUpdate())
                return 0;

            _IsRunning = true;
            int res = 0;
            string mappingName = GetMappingName();
            using (IDbCmd dbCmd = DbFactory.Create(_ConnectionString, _DBProvider))
            {
                res = dbCmd.Adapter.UpdateChanges(_DataSource, mappingName);
            }
            AcceptChanges();
            _IsRunning = false;
            return res;
        }

        public void AcceptChanges()
        {
            if (_DataSource == null) return;
            _DataSource.AcceptChanges();
            OnAcceptChanges(EventArgs.Empty);
        }


        public void RejectChanges()
        {
            if (_DataSource == null) return;
            _DataSource.RejectChanges();
        }

        void cmd_AsyncCompleted(object sender, Nistec.Threading.AsyncDataResultEventArgs e)
        {
            _IsRunning = false;
            OnAsyncCompleted(e);
            if (CommandCompleted != null)
                CommandCompleted(this, EventArgs.Empty);
        }

        void cmd_AsyncProgress(object sender, Nistec.Threading.AsyncProgressEventArgs e)
        {
            OnAsyncProgress(e);
            if (e.Level == Nistec.Threading.AsyncProgressLevel.Error)
            {
                _IsRunning = false;
            }
        }

        protected virtual void OnAsyncCompleted(Nistec.Threading.AsyncDataResultEventArgs e)
        {
            SyncTable(e.Table, false);
        }

        protected virtual void OnAsyncProgress(Nistec.Threading.AsyncProgressEventArgs e)
        {
            _Message = e.Message;
        }

        internal void SyncTable(DataTable dt, bool isCopy)
        {
            if (dt == null)// || dt.Rows.Count == 0)
                return;
            lock (syncRoot)
            {
                _DataSource = isCopy ? dt.Copy() : dt;
                OnDataSourceChanged(EventArgs.Empty);
                //AcceptChanges();
            }
        }
        internal void SyncTableSchema(DataTable dt)
        {
            if (dt == null)// || dt.Rows.Count == 0)
                return;
            lock (syncRoot)
            {
                _DataSource = dt.Clone();
                OnDataSourceChanged(EventArgs.Empty);
            }
        }

        protected virtual void SetConnection(DBProvider provider, string connectionString, string mappingName)
        {
            ActiveProvider=provider;
            ActiveConnection = connectionString;
            ActiveMappingName = mappingName;
        }

        protected virtual void OnDataSourceChanged(EventArgs e)
        {

        }
        protected virtual void OnReadOnlyChanged(EventArgs e)
        {

        }
        protected virtual void OnAcceptChanges(EventArgs e)
        {

        }
        protected virtual void OnDataChanged(EventArgs e)
        {

        }

        public virtual void Dispose()
        {
            //if (_Connection != null)
            //{
            //    _Connection.Dispose();
            //    _Connection=null;
            //}
            if (cmd != null)
            {
                cmd.AsyncProgress -= new Nistec.Threading.AsyncProgressEventHandler(cmd_AsyncProgress);
                cmd.AsyncCompleted -= new Nistec.Threading.AsyncDataResultEventHandler(cmd_AsyncCompleted);
                cmd.Dispose();
                cmd = null;
            }
            _Message = null;
            _ConnectionString = null;
            _TableName = null;
            _MappingName = null;
            _CommandText = null;

        }
    }


}
