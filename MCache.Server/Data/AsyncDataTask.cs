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
using System.Data;
using Nistec.Data;


namespace Nistec.Caching.Data
{

  

    #region  DataTask delegate
    /// <summary>
    /// Data Task Completed Event Handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DataTaskCompletedEventHandler(object sender, DataTaskCompletedEventArgs e);

    /// <summary>
    /// Data Task Completed Event Args
    /// </summary>
    public class DataTaskCompletedEventArgs : EventArgs
    {
        // Fields
        private object value;
        private IAsyncResult result;
        private AsyncDataTask sender;

        // Methods
        internal DataTaskCompletedEventArgs(AsyncDataTask sender, IAsyncResult result)
        {
            this.result = result;
            this.sender = sender;
        }

 
        /// <summary>
        /// Get value invoked.
        /// </summary>
        public object Value
        {
            get
            {
                if (this.value == null)
                {
                    try
                    {
                        this.value = this.sender.EndInvoke(this.result);
                    }
                    catch
                    {
                        throw;
                    }
                }
                return this.value;
            }
        }
    }

    #endregion

    /// <summary>
    /// Represent async data task.
    /// </summary>
    public class AsyncDataTask 
    {
        #region  members

        object syncRoot;
        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;
        /// <summary>
        /// DataTask Item Callback delegate
        /// </summary>
        /// <param name="task"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public delegate object DataTaskItemCallback(string task, object data);
        /// <summary>
        /// DataTask Completed event
        /// </summary>
        public event DataTaskCompletedEventHandler DataTaskCompleted;


   

        
        #endregion

        /// <summary>
        /// OnDataTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataTaskCompleted(DataTaskCompletedEventArgs e)
        {
            if (DataTaskCompleted != null)
                DataTaskCompleted(this, e);
        }


        private object DataTaskItemWorker(string task, object data)
        {
            switch (task.ToLower())
            {
                case "sizeofdataset":
                    return DataCacheUtil.DataSetSize((DataSet)data);

                case "sizeofdatatable":
                    {
                        return DataCacheUtil.DataTableSize((DataTable)data);
                    }
            }
            return null;
        }

        private int DataTableHandler(DataTable ds)
        {
            if (ds == null || ds.Rows.Count==0)
                return 0;

            int colsize = 0;

            DataRow dr = ds.Rows[0];
            for (int i = 0; i < ds.Columns.Count;i++ )
            {
                object o = dr[i];
                if (o == null)
                    colsize += 5;
                colsize += o.ToString().Length;
            }
           

            int rowsize = ds.Rows.Count;
            return (rowsize * colsize)/1024;

        }

        private int DataSetHandler(DataSet ds)
        {
            lock (syncRoot)
            {
                long size = 0;
                //string xmlDataSet = null;
               
                string tempFile = System.IO.Path.GetTempFileName();

                System.IO.FileInfo fi = new System.IO.FileInfo(tempFile);
                if (fi.Exists)
                {

                    ds.WriteXml(tempFile);
                   
                    size = fi.Length;

                }

                fi.Delete();

                return(int) size / 1024;
            }
        }
        /// <summary>
        /// Async Invoke
        /// </summary>
        /// <param name="task"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object AsyncTask(string task, object data)
        {
            AsyncDataTask taskdata = new AsyncDataTask();
            return taskdata.AsyncInvoke(task, data);
        }
        /// <summary>
        /// Create a new instance of async data task.
        /// </summary>
        public AsyncDataTask()
        {
            syncRoot = new object();
            resetEvent = new ManualResetEvent(false);
        }

      

        /// <summary>
        /// AsyncDataTask
        /// </summary>
        /// <returns></returns>
        public object AsyncInvoke(string task, object data)
        {
            DataTaskItemCallback caller = new DataTaskItemCallback(DataTaskItemWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(task,data, CreateCallBack(), caller);
            
           
            
            while (!result.IsCompleted)
            {
                Thread.Sleep(10);
            }

            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
           return caller.EndInvoke(result);
        }

        /// <summary>
        /// Task Begin Invoke.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public IAsyncResult BeginInvoke(string task, object data)
        {
            return BeginInvoke(task, data);
        }
        /// <summary>
        /// Task Begin Invoke.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="task"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public IAsyncResult BeginInvoke(object state, AsyncCallback callback, string task, object data)
        {
          
            DataTaskItemCallback caller = new DataTaskItemCallback(DataTaskItemWorker);

            if (callback == null)
            {
                callback = CreateCallBack();
            }

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(task, data, callback, caller);
            this.resetEvent.Set();
            return result;
        }


        /// <summary>Completes the specified asynchronous receive operation.</summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public object EndInvoke(IAsyncResult asyncResult)
        {
            // Retrieve the delegate.
            DataTaskItemCallback caller = (DataTaskItemCallback)asyncResult.AsyncState;

            // Call EndInvoke to retrieve the results.
            object o=(string)caller.EndInvoke(asyncResult);

           
            this.resetEvent.WaitOne();
            return o;
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
            OnDataTaskCompleted(new DataTaskCompletedEventArgs(this, asyncResult));
        }
    
    }

}
