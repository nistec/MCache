using System;

namespace Nistec.Legacy
{
	/// <summary>
    /// Dal Cache Error Type
	/// </summary>
	public enum DalCacheError
	{
		/// <summary>
		/// ErrorUnexpected
		/// </summary>
		ErrorUnexpected=1000,
		/// <summary>
		/// ErrorInitilaized
		/// </summary>
		ErrorInitilaized=1001,
		/// <summary>
        /// ErrorCreateCache
		/// </summary>
		ErrorCreateCache=1002,
		/// <summary>
		/// ErrorStoreData
		/// </summary>
		ErrorStoreData=1003,
		/// <summary>
		/// ErrorFileNotFound
		/// </summary>
		ErrorFileNotFound=1004,
		/// <summary>
		/// ErrorReadFromXml
		/// </summary>
		ErrorReadFromXml=1005,
		/// <summary>
		/// ErrorWriteToXml
		/// </summary>
		ErrorWriteToXml=1006,
		/// <summary>
        /// ErrorSyncCachee
		/// </summary>
		ErrorSyncCache=1007,
		/// <summary>
		/// ErrorSetValue
		/// </summary>
		ErrorSetValue=1008,
		/// <summary>
		/// ErrorReadValue
		/// </summary>
		ErrorReadValue=1009,
		/// <summary>
		/// ErrorTableNotExist
		/// </summary>
		ErrorTableNotExist=1010,
		/// <summary>
		/// ErrorColumnNotExist
		/// </summary>
		ErrorColumnNotExist=1011,
		/// <summary>
		/// ErrorInFilterExspression
		/// </summary>
		ErrorInFilterExspression=1012,
		/// <summary>
		/// ErrorCastingValue
		/// </summary>
		ErrorCastingValue=1013,
		/// <summary>
		/// ErrorGetValue
		/// </summary>
		ErrorGetValue=1014,
		/// <summary>
        /// ErrorMergeData
		/// </summary>
		ErrorMergeData=1015,
		/// <summary>
        /// ErrorUpdateChanges
		/// </summary>
		ErrorUpdateChanges=1016

	}

	/// <summary>
	/// Dal Cache Exception Event Handler
	/// </summary>
	public delegate void DalCacheExceptionEventHandler(object sender, DalCacheExceptionEventArgs e);

	/// <summary>
	/// Dal Cache Exception Event Args.
	/// </summary>
	public class DalCacheExceptionEventArgs:EventArgs
	{
		/// <summary>
		/// Get ErrorMessage
		/// </summary>
		public readonly string ErrorMessage;
		/// <summary>
		/// Get DalErrors
		/// </summary>
        public readonly DalCacheError Error;

		/// <summary>
        /// DalCacheExceptionEventArgs
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="error"></param>
        public DalCacheExceptionEventArgs(string msg, DalCacheError error)
		{
			ErrorMessage=msg;
			Error=error;
		}
	}
}
