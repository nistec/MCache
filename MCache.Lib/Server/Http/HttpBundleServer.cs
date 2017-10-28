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
using Nistec.IO;
using System.Threading.Tasks;
using Nistec.Caching.Channels;
using Nistec.Caching.Config;
using Nistec.Channels.Http;
using System.Net.Sockets;


namespace Nistec.Caching.Server.Http
{
    /// <summary>
    /// Represent a cache Http server listner.
    /// </summary>
    public class HttpBundleServer : HttpServer<CacheMessage>
    {
        bool isCache=false;
        bool isDataCache=false;
        bool isSyncCache=false;
        bool isSession=false;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            if (isCache)
                AgentManager.Cache.Start();
            if (isDataCache)
                AgentManager.DbCache.Start();
            if (isSyncCache)
                AgentManager.SyncCache.Start();
            if (isSession)
                AgentManager.Session.Start();

            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "HttpBundleServer.OnStart : " + Settings.HostName);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            if (isCache)
                AgentManager.Cache.Stop();
            if (isDataCache)
                AgentManager.DbCache.Stop();
            if (isSyncCache)
                AgentManager.SyncCache.Stop();
            if (isSession)
                AgentManager.Session.Stop();

            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "HttpBundleServer.OnStop : " + Settings.HostName);
        }
        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            if (isCache)
                AgentManager.Cache.Start();
            if (isDataCache)
                AgentManager.DbCache.Start();
            if (isSyncCache)
                AgentManager.SyncCache.Start(CacheSettings.EnableSyncFileWatcher, CacheSettings.ReloadSyncOnChange);
            if (isSession)
                AgentManager.Session.Start();
        }

        protected override void OnFault(string message, Exception ex)
        {
            //base.OnFault(message, ex);
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Error, "HttpBundleServer.OnFault : " + this.Settings.Address + ", " + message + " " + ex.Message);
        }
        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="hostName"></param>
        public HttpBundleServer(string hostName)
            : base(hostName)
        {

            isCache = CacheSettings.RemoteCacheProtocol.HasFlag(NetProtocol.Http);
            isDataCache = CacheSettings.DataCacheProtocol.HasFlag(NetProtocol.Http);
            isSyncCache = CacheSettings.SyncCacheProtocol.HasFlag(NetProtocol.Http);
            isSession = CacheSettings.SessionCacheProtocol.HasFlag(NetProtocol.Http);

        }

        /// <summary>
        /// Constractor using <see cref="HttpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public HttpBundleServer(HttpSettings settings)
            : base(settings)
        {

            isCache = CacheSettings.RemoteCacheProtocol.HasFlag(NetProtocol.Http);
            isDataCache = CacheSettings.DataCacheProtocol.HasFlag(NetProtocol.Http);
            isSyncCache = CacheSettings.SyncCacheProtocol.HasFlag(NetProtocol.Http);
            isSession = CacheSettings.SessionCacheProtocol.HasFlag(NetProtocol.Http);

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
            return AgentManager.ExecCommand(message);
        }
        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override CacheMessage ReadRequest(HttpRequestInfo request)
        {
            return CacheMessage.ReadRequest(request);
        }
       
        #endregion
    }
}
