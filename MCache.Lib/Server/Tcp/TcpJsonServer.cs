﻿//licHeader
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
//using Nistec.Caching.Channels;
using Nistec.Caching.Config;
using Nistec.Channels.Tcp;
using System.Net.Sockets;
using Nistec.Serialization;
using Nistec.Runtime;

namespace Nistec.Caching.Server.Tcp
{
    /// <summary>
    /// Represent a cache tcp server listner.
    /// </summary>
    public class TcpJsonBundleServer : TcpJsonServer//TcpServerPool<CacheMessage>
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
                if(!AgentManager.Cache.Initialized) AgentManager.Cache.Start();
            if (isDataCache)
                if (!AgentManager.DbCache.Initialized) AgentManager.DbCache.Start();
            if (isSyncCache)
                if (!AgentManager.SyncCache.Initialized) AgentManager.SyncCache.Start();// CacheSettings.EnableSyncFileWatcher, CacheSettings.ReloadSyncOnChange);
            if (isSession)
                if (!AgentManager.Session.Initialized) AgentManager.Session.Start();

            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "TcpJsonBundleServer.OnStart : " + Settings.HostName);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            if (isCache)
                if (AgentManager.Cache.Initialized) AgentManager.Cache.Stop();
            if (isDataCache)
                if (AgentManager.DbCache.Initialized) AgentManager.DbCache.Stop();
            if (isSyncCache)
                if (AgentManager.SyncCache.Initialized) AgentManager.SyncCache.Stop();
            if (isSession)
                if (AgentManager.Session.Initialized) AgentManager.Session.Stop();

            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Debug, "TcpJsonBundleServer.OnStop : " + Settings.HostName);
        }
        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            
        }

        protected override void OnFault(string message, Exception ex)
        {
            //base.OnFault(message, ex);
            CacheLogger.Logger.LogAction(CacheAction.General, CacheActionState.Error, "TcpJsonBundleServer.OnFault : " + this.Settings.HostName + ", " + message + " " + ex.Message);
        }

        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="hostName"></param>
        public TcpJsonBundleServer(string hostName) 
            : base(hostName)
        {
            //Settings = CacheSettings.LoadTcpConfigServer(hostName);

            isCache = CacheSettings.RemoteCacheProtocol.HasFlag(NetProtocol.Tcp);
            isDataCache = CacheSettings.DataCacheProtocol.HasFlag(NetProtocol.Tcp);
            isSyncCache = CacheSettings.SyncCacheProtocol.HasFlag(NetProtocol.Tcp);
            isSession = CacheSettings.SessionCacheProtocol.HasFlag(NetProtocol.Tcp);

        }

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public TcpJsonBundleServer(TcpSettings settings)
        : base(settings)
        {
            Settings = settings;

            isCache = CacheSettings.RemoteCacheProtocol.HasFlag(NetProtocol.Tcp);
            isDataCache = CacheSettings.DataCacheProtocol.HasFlag(NetProtocol.Tcp);
            isSyncCache = CacheSettings.SyncCacheProtocol.HasFlag(NetProtocol.Tcp);
            isSession = CacheSettings.SessionCacheProtocol.HasFlag(NetProtocol.Tcp);

   
        }

        #endregion

        #region abstract methods

        protected override TransStream ExecRequset(TransString message)
        {
            var cm = JsonSerializer.Deserialize<CacheMessage>(message.Body);
            var ack = AgentManager.ExecCommand(cm);
            if(ack==null)
            {
                return null;
            }
            string json = TransStream.ReadJson(ack.GetStream());
            return TransStream.Write(json,TransType.Json);

            //string json = TransReader.ReadJson(ack);
            ////string json = ack.ToJson();
            //if(json[0]=='[' && json[json.Length-1]!=']')
            //{
            //    Console.WriteLine("Incorrect json");
            //}
            //NetStream nstream = new NetStream();
            //StringMessage.WriteString(json, nstream);
            //return nstream;

        }

        #endregion
    }
}
