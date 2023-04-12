using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using Nistec.Data;
using Nistec.Data.Entities;
using Nistec.Generic;

namespace Nistec.Legacy
{
    /// <summary>
    /// DalProvider
    /// </summary>
    public class DalProvider : IAutoBase //IDalBase
    {

        #region IDisposable implementation

        /// <summary>
        /// Disposed flag.
        /// </summary>
        protected bool m_disposed = false;
        private bool m_initilaized = false;
        /// <summary>
        /// Permit
        /// </summary>
        internal bool m_Permit = false;

        /// <summary>
        /// Implementation of method of IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method with a boolean parameter indicating the source of calling.
        /// </summary>
        /// <param name="calledbyuser">Indicates from whare the method is called.</param>
        protected void Dispose(bool calledbyuser)
        {
            if (!m_disposed)
            {
                if (calledbyuser)
                {
                    InnerDispose();
                }
                m_disposed = true;
                m_initilaized = false;
            }
        }

        /// <summary>
        /// Inner implementation of Dispose method.
        /// </summary>
        protected void InnerDispose()
        {
            if (m_connection != null)
            {
                if ((m_connection.State != ConnectionState.Closed) && m_ownsConnection)
                {
                    try
                    {
                        m_connection.Close();
                    }
                    catch { }
                }
            }
            m_connection = null;
            m_transaction = null;
            m_initilaized = false;
            //UpdateAllBaseObjects();
        }

        /// <summary>
        /// Class destructor.
        /// </summary>
        ~DalProvider()
        {
            Dispose(false);
        }
        #endregion

        #region private members
        /// <summary>
        /// Connection object.
        /// </summary>
        protected IDbConnection m_connection = null;

        /// <summary>
        /// Transaction object.
        /// </summary>
        protected IDbTransaction m_transaction = null;

        /// <summary>
        /// Indicates that <see cref="DalProvider"/> object owns the connection.
        /// </summary>
        protected bool m_ownsConnection = false;

        /// <summary>
        /// Indicates that the connection must be closed each time after a command execution.
        /// </summary>
        protected bool m_autoCloseConnection = false;

        /// <summary>
        /// Contains DalSchema as dataset.
        /// </summary>
        protected DalSchema m_DataSet = null;

        private DBProvider dbProvider;

        private string m_connectionString;

        #endregion

 
        /// <summary>
        /// DalBaseProvider Ctor
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="connectionString">connection string parameter.</param>
        public DalProvider(DBProvider provider, string connectionString)
        {
            m_Permit = true;// Nistec.Net.DalNet.NetFram("Data.Provider", "CTL");

            dbProvider = provider;
            InitInternal(connectionString, true,true,null);
        }
        /// <summary>
        /// DalBaseProvider Ctor
        /// </summary>
        /// <param name="connectionID">Connection id from dalServer xml.</param>
        public DalProvider(string connectionID)
        {
            //m_Permit = Nistec.Net.DalNet.NetFram("Data.Provider", "CTL");
            //CONNECTION con = Connections.GetConnection(connectionID);
            
            DbContext con = new DbContext(connectionID);
            dbProvider = con.Provider;
            InitInternal(con.ConnectionString, true, true, null);
        }

        /// <summary>
        /// Initializes the object from DalServer XML. 
        /// </summary>
        /// <param name="connectionName">Connection name from dalServer xml.</param>
        /// <param name="path">mck file path.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        public void InitFromXML(string connectionName, string path, bool autoCloseConnection)
        {
            //string con = Connections.GetConnectionString(connectionName);
            //m_Permit = Nistec.Net.DalNet.NetFram(path, "SERVER");

            string connectionString= NetConfig.ConnectionString(connectionName);

            InitInternal(connectionString, autoCloseConnection, true, null);
        }

        /// <summary>
        /// Initializes the object. 
        /// </summary>
        /// <param name="connectionString">connection string parameter.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        public void Init(string connectionString, bool autoCloseConnection)
        {
            m_Permit = true;// Nistec.Net.DalNet.NetFram("Data.Provider", "CTL");
            
            InitInternal(connectionString, autoCloseConnection, true, null);
        }


        /// <summary>
        /// Initializes the object. 
        /// </summary>
        /// <param name="connectionString">Connection parameter.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="pk">key</param>
        public void Init(string connectionString, bool autoCloseConnection, string pk)
        {

            m_Permit = true;// Nistec.Net.DalNet.NetFram(pk, "SRV");
            InitInternal(connectionString, autoCloseConnection, true, null);
        }

        /// <summary>
        /// Initializes the object. 
        /// </summary>
        /// <param name="connectionString">Connection parameter.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="dsSchema">dsSchema parameter.</param>
        /// <param name="pk">key</param>
        public void Init(string connectionString, bool autoCloseConnection, DalSchema dsSchema, string pk)
        {
            m_Permit = true;// Nistec.Net.DalNet.NetFram(pk, "SRV");
            InitInternal(connectionString, autoCloseConnection, true, null);
        }

        /// <summary>
        /// Initializes the object. 
        /// </summary>
        /// <param name="connection">Connection parameter.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="ownsConnection">OwnsConnection parameter.</param>
        /// <param name="dsSchema">dsSchema parameter.</param>
        private void InitInternal(string connection, bool autoCloseConnection, bool ownsConnection, DalSchema dsSchema)
        {


            InnerDispose();
            if (dbProvider == DBProvider.OleDb)
                m_connection = new OleDbConnection(connection);
            else
                m_connection = new SqlConnection(connection);
  
            //m_connection = connection;
            m_connectionString = connection;
            m_autoCloseConnection = autoCloseConnection;
            m_ownsConnection = ownsConnection;
            m_DataSet = dsSchema;

            //GenerateAllObjects();
            //UpdateAllBaseObjects();
            m_initilaized = true;
        }


        #region Public members

        /// <summary>
        /// DalPermit
        /// </summary>
        public bool Permit { get { return m_Permit; } }
 
        /// <summary>
        /// DBProvider
        /// </summary>
        public DBProvider DBProvider { get { return dbProvider; } }
        /// <summary>
        /// IDbConnection property.
        /// </summary>
        public IDbConnection IConnection { get { return m_connection as IDbConnection; } }
        /// <summary>
        /// IDbTransaction property.
        /// </summary>
        public IDbTransaction ITransaction { get { return m_transaction as IDbTransaction; } }
        /// <summary>
        /// Get Initilaized
        /// </summary>
        public bool Initilaized { get { return m_initilaized; } }
        /// <summary>
        /// It true then the object owns its connection
        /// and disposes it on its own disposal.
        /// </summary>
        public bool OwnsConnection { get { return m_ownsConnection; } }

        /// <summary>
        /// If true then the object's connection is closed each time 
        /// after command execution.
        /// </summary>
        public bool AutoCloseConnection { get { return m_autoCloseConnection; } }

        /// <summary>
        /// connection property.
        /// </summary>
        public IDbConnection Connection { get { return m_connection; } }

        /// <summary>
        /// transaction property.
        /// </summary>
        public IDbTransaction Transaction { get { return m_transaction; } }

        /// <summary>
        /// DalSchema Data set schema property.
        /// </summary>
        public DalSchema DBSchema { get { return m_DataSet; } }

        /// <summary>
        /// Begins transaction with a default (<see cref="IsolationLevel.ReadCommitted"/>) isolation level.
        /// </summary>
        /// <returns></returns>
        public IDbTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Begins transaction with a specified isolation level.
        /// </summary>
        /// <param name="iso"></param>
        /// <returns></returns>
        public IDbTransaction BeginTransaction(IsolationLevel iso)
        {
            if (m_transaction != null)
            {
                throw new ApplicationException("A previous transaction is not closed");
            }
            m_transaction = m_connection.BeginTransaction(iso);
            //UpdateAllBaseObjects();
            return m_transaction;
        }

        /// <summary>
        /// Begins transaction with a specified isolation level.
        /// </summary>
        /// <param name="iso"></param>
        /// <returns></returns>
        public IDbTransaction IBeginTransaction(IsolationLevel iso)
        {
            return (IDbTransaction)BeginTransaction(iso) as IDbTransaction;
        }

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            if (m_transaction == null)
            {
                throw new ApplicationException("A transaction has not been opened");
            }
            m_transaction.Rollback();
            m_transaction = null;
            //UpdateAllBaseObjects();
        }

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        public void CommitTransaction()
        {
            if (m_transaction == null)
            {
                throw new ApplicationException("A transaction has not been started");
            }
            m_transaction.Commit();
            m_transaction = null;
            //UpdateAllBaseObjects();
        }
        #endregion

    }
}
