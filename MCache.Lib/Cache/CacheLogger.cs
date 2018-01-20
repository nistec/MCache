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
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Linq;
using Nistec.Generic;
using Nistec.Threading;
using Nistec.Logging;
using Nistec.Caching.Config;

namespace Nistec.Caching
{

    #region  Log delegate

    /// <summary>
    /// Log Message EventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void LogMessageEventHandler(object sender, LogMessageEventArgs e);

    /// <summary>
    /// Represent log message event arguments.
    /// </summary>
    public class LogMessageEventArgs : EventArgs
    {
        // Fields
        private string message;
        private IAsyncResult result;
        private CacheLogger sender;

        // Methods
        internal LogMessageEventArgs(CacheLogger sender, IAsyncResult result)
        {
            this.result = result;
            this.sender = sender;
        }

        /// <summary>
        /// Get message.
        /// </summary>
        public string Message
        {
            get
            {
                if (this.message == null)
                {
                    try
                    {
                        this.message = this.sender.EndLog(this.result);
                    }
                    catch
                    {
                        throw;
                    }
                }
                return this.message;
            }
        }
    }

    #endregion

    /// <summary>
    /// Represent cache logger.
    /// </summary>
    public class CacheLogger : System.ComponentModel.Component
    {

        internal static int logCapacity = 1000;
        internal static bool debugEnabled = true;
        #region  members

        //int logCapacity = 1000;
        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;
        /// <summary>
        /// Log Item Callback delegate
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public delegate string LogItemCallback(string text);
        /// <summary>
        /// Log Completed event
        /// </summary>
        public event LogMessageEventHandler LogMessage;


        static readonly Queue<string> log = new Queue<string>();

        static CacheLogger()
        {
            logCapacity = CacheSettings.LogMonitorCapacityLines;
            debugEnabled = CacheSettings.LogMonitorDebugEnabled;
        }
        /// <summary>
        /// Read log as string array.
        /// </summary>
        /// <returns></returns>
        public static string[] ReadLog()
        {
            List<string> copy = null;
            lock (((ICollection)log).SyncRoot)
            {
                copy = log.ToList<string>();
            }
            if (copy == null)
                return new string[] { "" };
            copy.Reverse();
            return copy.ToArray();
        }


        #endregion


        ILogger _ILogger = Nistec.Logging.Logger.Instance;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger ILog { get { return _ILogger; } set { if (value != null) _ILogger = value; } }


        static CacheLogger _Logger;
        /// <summary>
        /// Get <see cref="CacheLogger"/> instance.
        /// </summary>
        public static CacheLogger Logger
        {
            get
            {
                if (_Logger == null)
                {
                    _Logger = new CacheLogger();
                }
                return _Logger;
            }
        }

        /// <summary>
        /// OnLogCompleted
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnLogMessage(LogMessageEventArgs e)
        {
            if (LogMessage != null)
                LogMessage(this, e);
        }


        private string LogItemWorker(string text)
        {
            string msg = string.Format("{0}: {1}", DateTime.Now, text);
            lock (((ICollection)log).SyncRoot)
            {
                if (log.Count > logCapacity)
                {
                    log.Dequeue();
                }
                log.Enqueue(msg);
            }
            return msg;
        }
        /// <summary>
        /// Clear log.
        /// </summary>
        public void Clear()
        {
            lock (((ICollection)log).SyncRoot)
            {
                log.Clear();
            }
        }
        /// <summary>
        /// Get log as long string.
        /// </summary>
        /// <returns></returns>
        public string CacheLog()
        {
            string[] array = ReadLog();
            StringBuilder sb = new StringBuilder();

            foreach (string s in array)
            {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Initialize a new instance of cache logger.
        /// </summary>
        public CacheLogger()
        {
            resetEvent = new ManualResetEvent(false);
        }
        /// <summary>
        /// Release all resource fro cache logger.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// AsyncLog
        /// </summary>
        /// <returns></returns>
        void Log(string text)
        {
            LogItemCallback caller = new LogItemCallback(LogItemWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(text, CreateCallBack(), caller);

            while (!result.IsCompleted)
            {
                Thread.Sleep(10);
            }
            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            caller.EndInvoke(result);
        }

        /// <summary>
        /// Begin write to cache logger async.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IAsyncResult BeginLog(string text)
        {
            return BeginLog(text);
        }
        /// <summary>
        /// Begin write to cache logger async.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public IAsyncResult BeginLog(object state, AsyncCallback callback, string text)
        {

            LogItemCallback caller = new LogItemCallback(LogItemWorker);

            if (callback == null)
            {
                callback = CreateCallBack();
            }

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(text, callback, caller);
            //this.resetEvent.Set();
            return result;
        }


        /// <summary>Completes the specified asynchronous receive operation.</summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public string EndLog(IAsyncResult asyncResult)
        {
            // Retrieve the delegate.
            LogItemCallback caller = (LogItemCallback)asyncResult.AsyncState;

            // Call EndInvoke to retrieve the results.
            string msg = (string)caller.EndInvoke(asyncResult);

            return msg;
        }

        private AsyncCallback CreateCallBack()
        {
            if (this.onRequestCompleted == null)
            {
                this.onRequestCompleted = new AsyncCallback(this.OnRequestCompleted);
            }
            return this.onRequestCompleted;
        }


        private void OnRequestCompleted(IAsyncResult asyncResult)
        {
            OnLogMessage(new LogMessageEventArgs(this, asyncResult));
        }
        /// <summary>
        /// Write new line to cache logger as info.
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            Logger.WriteLog(LoggerLevel.Info, message);
        }
        /// <summary>
        /// Write new line to cache logger as info.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void InfoFormat(string message, params object[] args)
        {
            Logger.WriteLog(LoggerLevel.Info, message);
        }
        /// <summary>
        /// Write new line to cache logger as debug.
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            Logger.WriteLog(LoggerLevel.Debug, message);
        }
        /// <summary>
        ///  Write new line to cache logger as debug.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void DebugFormat(string message, params object[] args)
        {
            Logger.WriteLog(LoggerLevel.Debug, message, args);
        }
        /// <summary>
        ///  Write new line to cache logger as error.
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            Logger.WriteLog(LoggerLevel.Error, message);
        }
        /// <summary>
        ///  Write new line to cache logger as error.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void ErrorFormat(string message, params object[] args)
        {
            Logger.WriteLog(LoggerLevel.Error, message, args);
        }

        void WriteLog(LoggerLevel level, string text, params object[] args)
        {

            string msg = string.Format(text, args);
            WriteLog(level, msg);

            //Log(level.ToString() + "-" + msg);

            //switch (level)
            //{
            //    case LoggerLevel.Error:
            //        Log(level.ToString() + "-" + msg);
            //        if (CacheSettings.EnableLog)
            //            ILog.Error(msg); break;
            //    case LoggerLevel.Debug:
            //        if (debugEnabled)
            //            Log(level.ToString() + "-" + msg);
            //        if (CacheSettings.EnableLog)
            //            ILog.Debug(msg); break;
            //    case LoggerLevel.Info:
            //        Log(level.ToString() + "-" + msg);
            //        if (CacheSettings.EnableLog)
            //            ILog.Info(msg); break;
            //    case LoggerLevel.Warn:
            //        Log(level.ToString() + "-" + msg);
            //        if (CacheSettings.EnableLog)
            //            ILog.Warn(msg); break;
            //        //case LoggerLevel.Trace:
            //        //    Netlog.Trace(msg); break;
            //}


            //Console.WriteLine(msg);
        }

        void WriteLog(LoggerLevel level, string msg)
        {

            switch (level)
            {
                case LoggerLevel.Error:
                    Log(level.ToString() + "-" + msg);
                    if (CacheSettings.EnableLog)
                        ILog.Error(msg); break;
                case LoggerLevel.Debug:
                    if (debugEnabled)
                        Log(level.ToString() + "-" + msg);
                    if (CacheSettings.EnableLog)
                        ILog.Debug(msg); break;
                case LoggerLevel.Info:
                    Log(level.ToString() + "-" + msg);
                    if (CacheSettings.EnableLog)
                        ILog.Info(msg); break;
                case LoggerLevel.Warn:
                    Log(level.ToString() + "-" + msg);
                    if (CacheSettings.EnableLog)
                        ILog.Warn(msg); break;
                    //case LoggerLevel.Trace:
                    //    Netlog.Trace(msg); break;
            }
            Console.WriteLine(msg);
        }

        ///// <summary>
        /////  Write new line to cache logger using arguments.
        ///// </summary>
        ///// <param name="action"></param>
        ///// <param name="text"></param>
        ///// <param name="state"></param>
        ///// <param name="args"></param>
        //public void Log(CacheAction action, string text, CacheActionState state, params string[] args)
        //{

        //    // Log(string.Format(action + "-" + text, args));

        //    //if (CacheSettings.EnableLog)
        //    //{
        //    switch (state)
        //    {
        //        case CacheActionState.Error:
        //            WriteLog(LoggerLevel.Error, action + "-" + text, args);
        //            //ILog.Error(action + "-" + text, args);
        //            break;
        //        case CacheActionState.Debug:
        //            WriteLog(LoggerLevel.Debug, action + "-" + text, args);
        //            //ILog.Debug(action + "-" + text, args); 
        //            break;
        //        default:
        //            WriteLog(LoggerLevel.Info, action + "-" + text, args);
        //            //ILog.Info(action + "-" + text, args); 
        //            break;
        //    }
        //    //}
        //}

        /// <summary>
        /// Write new line to cache logger using arguments.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="text"></param>
        public void LogAction(CacheAction action, CacheActionState state, string text)
        {

            //Log(string.Format("{0}-{1}", action, text));
            //if (CacheSettings.EnableLog)
            //{
            switch (state)
            {
                case CacheActionState.Error:
                    WriteLog(LoggerLevel.Error, action + "-" + text);
                    //ILog.Error(action + "-" + text);
                    break;
                case CacheActionState.Debug:
                    WriteLog(LoggerLevel.Debug, action + "-" + text);
                    //ILog.Debug(action + "-" + text);
                    break;
                default:
                    WriteLog(LoggerLevel.Info, action + "-" + text);
                    //ILog.Info(action + "-" + text);
                    break;
            }
            // }
        }
        /// <summary>
        /// Write new line to cache logger using arguments.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public void LogAction(CacheAction action, CacheActionState state, string text, params string[] args)
        {
            //Log(string.Format(action + "-" + text, args));
            //if (CacheSettings.EnableLog)
            //{
            switch (state)
            {
                case CacheActionState.Error:
                    WriteLog(LoggerLevel.Error, action + "-" + text, args);
                    //ILog.Error(action + "-" + text, args);
                    break;
                case CacheActionState.Debug:
                    WriteLog(LoggerLevel.Debug, action + "-" + text, args);
                    //ILog.Debug(action + "-" + text, args);
                    break;
                default:
                    WriteLog(LoggerLevel.Info, action + "-" + text, args);
                    //ILog.Info(action + "-" + text, args);
                    break;
            }
            //}
        }
    }

}
