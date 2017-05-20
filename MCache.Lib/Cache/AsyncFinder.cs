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
 
namespace Nistec.Caching
{

  
    /// <summary>
    /// IFinder interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFinder<T>
    {
        /// <summary>
        /// End find.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        ICollection<T> EndFind(IAsyncResult asyncResult);
    }

   
    /// <summary>
    /// EntryFindType
    /// </summary>
    public enum EntryFindType
    {
        /// <summary>
        /// Find by Key
        /// </summary>
        Key,
        /// <summary>
        /// Find by SessionId
        /// </summary>
        SessionId,
        /// <summary>
        /// Find by Timeout
        /// </summary>
        Timeout,
        /// <summary>
        /// Find by part of Key
        /// </summary>
        InKey,
        /// <summary>
        /// Find by Type
        /// </summary>
        Type
    }

    #region  Find delegate

    /// <summary>
    /// Find Completed EventHandler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void FindCompletedEventHandler<T>(object sender, FindCompletedEventArgs<T> e);

    /// <summary>
    /// Find Completed EventArgs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FindCompletedEventArgs<T> : EventArgs
    {
        // Fields
        private ICollection<T> item;
        private IAsyncResult result;
        private AsyncFinder<T> sender;
        //private ItemState state;

        // Methods
        internal FindCompletedEventArgs(AsyncFinder<T> sender, IAsyncResult result)
        {
            this.result = result;
            this.sender = sender;
        }

        /// <summary>
        /// Get or Set AsyncResult
        /// </summary>
        public IAsyncResult AsyncResult
        {
            get
            {
                return this.result;
            }
            set
            {
                this.result = value;
            }
        }
        /// <summary>
        /// Get items collection.
        /// </summary>
        public ICollection<T> Items
        {
            get
            {
                if (this.item == null)
                {
                    try
                    {
                        this.item = this.sender.EndFind(this.result);
                    }
                    catch
                    {
                        throw;
                    }
                }
                return this.item;
            }
        }


    }
    #endregion

   /// <summary>
    /// Find Item Callback delegate
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="timeout"></param>
   /// <param name="findType"></param>
   /// <param name="key"></param>
   /// <returns></returns>
    public delegate ICollection<T> FindItemCallback<T>(TimeSpan timeout, string findType, object key);

    /// <summary>
    /// Async Finder class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncFinder<T> : IFinder<T>
    {

        #region  asyncInvoke
        /// <summary>
        /// DefaultMaxTimeout
        /// </summary>
        public static readonly TimeSpan DefaultTimeOut = TimeSpan.FromMilliseconds(4294967295);

        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;

       /// <summary>
        /// Find Completed event
       /// </summary>
        public event FindCompletedEventHandler<T> FindCompleted;
        

        /// <summary>
        /// OnFindCompleted
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFindCompleted(FindCompletedEventArgs<T> e)
        {
            if (FindCompleted != null)
                FindCompleted(this, e);
        }

        #region find types

        internal static ICollection<T> NewCollection
        {
            get {return new List<T>(); }
        }
        #endregion
        /// <summary>
        ///  Find using arguments.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="key"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static ICollection<T> Find(string findType, object key, FindItemCallback<T> caller)
        {
            AsyncFinder<T> finder = new AsyncFinder<T>();
            return finder.AsyncFind(findType, key, caller);
        }
        /// <summary>
        /// ctor.
        /// </summary>
        public AsyncFinder()
        {
            //this.owner = owner;
            resetEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// Async Find.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="key"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public ICollection<T> AsyncFind(string findType, object key, FindItemCallback<T> caller)
        {

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(DefaultTimeOut,findType, key, CreateCallBack(), caller);
            //Thread.Sleep(10);

            //result.AsyncWaitHandle.WaitOne();
            while (!result.IsCompleted)
            {
                Thread.Sleep(10);
            }
            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            ICollection<T> item = caller.EndInvoke(result);
            //AsyncCompleted(item);
            return item;

        }

        /// <summary>Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.</summary>
        /// <param name="caller"></param>
        /// <param name="findType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public IAsyncResult BeginFind(FindItemCallback<T> caller, string findType, object key)
        {
            return BeginFind(caller,DefaultTimeOut, findType, key);
        }
        /// <summary>Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.</summary>
        /// <param name="caller"></param>
        /// <param name="timeout"></param>
        /// <param name="findType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public IAsyncResult BeginFind(FindItemCallback<T> caller, TimeSpan timeout, string findType, object key)
        {
            return BeginFind(caller,timeout, findType, key);
        }


        /// <summary>
        /// Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="timeout"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="findType"></param>
        /// <param name="key"></param>
        /// <returns>The <see cref="T:System.IAsyncResult"></see> that identifies the posted asynchronous request.</returns>
        public IAsyncResult BeginFind(FindItemCallback<T> caller, TimeSpan timeout, object state, AsyncCallback callback, string findType, object key)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 4294967295L))
            {
                throw new ArgumentException("InvalidParameter", "timeout");
            }

            if (callback == null)
            {
                callback = CreateCallBack();
            }

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(timeout,findType, key, callback, caller);
            this.resetEvent.Set();
            return result;
        }


        /// <summary>Completes the specified asynchronous receive operation.</summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public ICollection<T> EndFind(IAsyncResult asyncResult)
        {
            // Retrieve the delegate.
            FindItemCallback<T> caller = (FindItemCallback<T>)asyncResult.AsyncState;

            // Call EndInvoke to retrieve the results.
            ICollection<T> item = (ICollection<T>)caller.EndInvoke(asyncResult);

            this.resetEvent.WaitOne();
            return item;
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
            OnFindCompleted(new FindCompletedEventArgs<T>(this, asyncResult));
        }

         #endregion
    
    }

}
