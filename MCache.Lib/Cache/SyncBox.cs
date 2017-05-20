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
using System.Linq;
using System.Text;
using System.Collections;
using Nistec.Threading;
using Nistec.Caching.Remote;
using System.Collections.Concurrent;
using Nistec.Caching.Sync;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Caching.Data;

namespace Nistec.Caching
{

    internal class SyncBox : IDisposable
    {
        #region memebers

        int synchronized;

        public static readonly SyncBox Instance = new SyncBox(true,true);
        private ConcurrentQueue<SyncBoxTask> m_SynBox;
        private bool KeepAlive = false;

        #endregion

        #region properties

        /// <summary>
        /// Get indicate whether the sync box is remote.
        /// </summary>
        public bool IsRemote
        {
            get;
            internal set;
        }

        /// <summary>
        /// Get indicate whether the sync box intialized.
        /// </summary>
        public bool Initialized
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of elements contained in the SyncBox. 
        /// </summary>
        public int Count
        {
            get { return m_SynBox.Count; }
        }
      
        #endregion

        #region ctor

        public SyncBox(bool autoStart, bool isRemote)
        {
            m_SynBox = new ConcurrentQueue<SyncBoxTask>();
            IsRemote = isRemote;
            this.Initialized = true;
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized SynBox");

            if (autoStart)
            {
                Start();
            }
        }

        public void Dispose()
        {
           
        }
        #endregion

        #region queue methods


        public void Add(SyncBoxTask item)
        {
            if (item == null)
            {

                this.LogAction(CacheAction.SyncTime, CacheActionState.Failed, "SyncBox can not add task null!");
                return;
            }
         
            m_SynBox.Enqueue(item);
            this.LogAction(CacheAction.SyncTime, CacheActionState.Debug, "SyncBox Added SyncBoxTask {0}", item.ItemName);
        }

        private SyncBoxTask Get()
        {
            SyncBoxTask res = null;
             m_SynBox.TryDequeue(out res);
            return res;
        }

        
        public void Clear()
        {
            while (m_SynBox.Count > 0)
            {
                Get();
            }
            
        }

        #endregion

        #region Timer Sync

       
        public void Start()
        {

            if (!this.Initialized)
            {
                throw new Exception("The SyncBox not initialized!");
            }

            if (KeepAlive)
                return;
            this.LogAction(CacheAction.General, CacheActionState.None, "SyncBox Started...");

            KeepAlive = true;
            Thread.Sleep(1000);
            Thread th = new Thread(new ThreadStart(InternalStart));
            th.IsBackground = true;
            th.Start();
        }

        public void Stop()
        {
            KeepAlive = false;
            this.Initialized = false;
            this.LogAction(CacheAction.General, CacheActionState.None, "SyncBox Stoped");
        }


        private void InternalStart()
        {
            while (KeepAlive)
            {
                DoSync();
                Thread.Sleep(1000);
            }
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized SyncBox Not keep alive");
        }

        public void DoSync()
        {
            OnSyncTask();
        }

        protected virtual void OnSyncTask()
        {
            try
            {
                //0 indicates that the method is not in use.
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    SyncBoxTask syncTask = null;
                    if (m_SynBox.TryDequeue(out syncTask))
                    {
                       
                        syncTask.DoSync();
                       
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "SyncBox OnSyncTask End error :" + ex.Message);

            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }

        #endregion

        #region LogAction
        protected virtual void LogAction(CacheAction action, CacheActionState state, string text)
        {
            if (IsRemote)
            {
                CacheLogger.Logger.LogAction(action, state, text);
            }
        }

        protected virtual void LogAction(CacheAction action, CacheActionState state, string text, params string[] args)
        {
            if (IsRemote)
            {
                CacheLogger.Logger.LogAction(action, state, text, args);
            }
        }
        #endregion

    }
}
