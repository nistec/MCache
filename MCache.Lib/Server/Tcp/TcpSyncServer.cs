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
using Nistec.Channels.Tcp;
using System.Net.Sockets;

namespace Nistec.Caching.Server.Tcp
{
    /// <summary>
    /// Represent a sync tcp server listner.
    /// </summary>
    public class TcpSyncServer : TcpServer<CacheMessage>
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
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="hostName"></param>
        public TcpSyncServer(string hostName)
            : base()
        {
            Settings = CacheSettings.LoadTcpConfigServer(hostName);
        }
        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public TcpSyncServer(TcpSettings settings)
            :base(settings)
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
        /// <param name="stream"></param>
        /// <returns></returns>
        protected override CacheMessage ReadRequest(NetworkStream stream)
        {
            return CacheMessage.ReadRequest(stream, Settings.ReceiveBufferSize);
        }

        #endregion
    }
}
