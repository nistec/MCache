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
using System.IO;
using System.IO.Pipes;
using Nistec.Channels;
using Nistec.Caching.Remote;
using System.Diagnostics;
using Nistec.IO;
using Nistec.Caching.Channels;
using Nistec.Caching.Config;

namespace Nistec.Caching.Server.Pipe
{
    /// <summary>
    /// Represent a sync pipe server listner.
    /// </summary>
    public class PipeSyncServer : PipeServer<CacheMessage>//PipeServerCache
    {
   
        #region override

        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            AgentManager.SyncCache.Start(CacheSettings.EnableSyncFileWatcher, CacheSettings.ReloadSyncOnChange);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            AgentManager.SyncCache.Stop();
        }
        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();


        }
        #endregion

        #region ctor

        /// <summary>
        /// Constractor default
        /// </summary>
        public PipeSyncServer()
            : base(CacheDefaults.DefaultSyncCacheHostName, true)
        {
            
        }

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loadFromSettings"></param>
        public PipeSyncServer(string name, bool loadFromSettings)
            : base(name, loadFromSettings)
        {
           
        }

        #endregion
 
        #region abstract methods
        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override NetStream ExecRequset(CacheMessage message)
        {
            return AgentManager.SyncCache.ExecRemote(message);
        }
        /// <summary>
        /// ReadRequest
        /// </summary>
        /// <param name="pipeServer"></param>
        /// <returns></returns>
        protected override CacheMessage ReadRequest(NamedPipeServerStream pipeServer)
        {
            return CacheMessage.ReadRequest(pipeServer, InBufferSize);
        }
        ///// <summary>
        ///// Write Response
        ///// </summary>
        ///// <param name="pipeServer"></param>
        ///// <param name="bResponse"></param>
        ///// <param name="message"></param>
        //protected override void WriteResponse(NamedPipeServerStream pipeServer, NetStream bResponse, CacheMessage message)
        //{
        //    if (message.IsDuplex)
        //    {
        //        WriteResponse(pipeServer, bResponse);
        //    }
        //}
        #endregion
    }
}
