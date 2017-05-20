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
using System.Threading;
using System.ServiceModel;
using System.Runtime.Remoting;
using System.ServiceProcess;
using Nistec.Caching.Remote;
using Nistec.Caching.Server;
using Nistec.Generic;
using Nistec.Caching;
using System.IO;
using Nistec.Caching.Config;
using Nistec.Caching.Server.Tcp;
using System.Threading.Tasks;
using Nistec.Channels;
using Nistec.Caching.Server.Http;
using Nistec.Caching.Server.Pipe;
using Nistec.Logging;

namespace Nistec.Services
{
    public class ServiceManager
    {

        private PipeCacheServer mcache;
        private PipeDataServer mdata;
        private PipeSessionServer msession;
        private PipeManagerServer mmanger;
        private PipeSyncServer msync;

        private TcpBundleServer tcpbundle;
        private PipeBundleServer pipebundle;
        private HttpBundleServer httpbundle;
        
        private TcpCacheServer tcpcache;
        private TcpDataServer tcpdata;
        private TcpSessionServer tcpsession;
        private TcpManagerServer tcpmanger;
        private TcpSyncServer tcpsync;
        private ConfigFileWatcher configWatcher;
        bool _loaded = false;

       public bool Loaded
       {
           get { return _loaded; }
       }

            public ServiceManager()
            {
            }

            public void Start()
            {
                Thread Th = new Thread(new ThreadStart(InternalStart));
                Th.Start();
            }

            private void InternalStart()
            {
                try
                {
                    Netlog.Debug(Settings.ServiceName + " start...");


                    var settings = CacheConfigServer.GetCacheApiSettings();

                    //Tcp
                    if (CacheSettings.EnableTcpBundle)
                    {
                        tcpbundle = new TcpBundleServer(settings.Get("RemoteBundleHostName"));
                        tcpbundle.Start();
                    }
                    else
                    {
                        //Tcp
                        if (CacheSettings.RemoteCacheProtocol.HasFlag(NetProtocol.Tcp))
                        {
                            tcpcache = new TcpCacheServer(settings.Get("RemoteCacheHostName"));
                            tcpcache.Start();
                        }
                        if (CacheSettings.SessionCacheProtocol.HasFlag(NetProtocol.Tcp))
                        {
                            tcpsession = new TcpSessionServer(settings.Get("RemoteSessionHostName"));
                            tcpsession.Start();
                        }
                        if (CacheSettings.SyncCacheProtocol.HasFlag(NetProtocol.Tcp))
                        {
                            tcpsync = new TcpSyncServer(settings.Get("RemoteSyncCacheHostName"));
                            tcpsync.Start();
                        }
                        if (CacheSettings.DataCacheProtocol.HasFlag(NetProtocol.Tcp))
                        {
                            tcpdata = new TcpDataServer(settings.Get("RemoteDataCacheHostName"));
                            tcpdata.Start();
                        }
                       
                    }
                    //Pipe
                    if (CacheSettings.EnablePipeBundle)
                    {
                        pipebundle = new PipeBundleServer(settings.Get("RemoteBundleHostName"));
                        pipebundle.Start();
                    }
                    else
                    {
                        //Pipe
                        if (CacheSettings.RemoteCacheProtocol.HasFlag(NetProtocol.Pipe))
                        {
                            mcache = new PipeCacheServer(settings.Get("RemoteCacheHostName"), true);
                            mcache.Start();
                        }
                        if (CacheSettings.SessionCacheProtocol.HasFlag(NetProtocol.Pipe))
                        {
                            msession = new PipeSessionServer(settings.Get("RemoteSessionHostName"), true);
                            msession.Start();
                        }
                        if (CacheSettings.SyncCacheProtocol.HasFlag(NetProtocol.Pipe))
                        {
                            msync = new PipeSyncServer(settings.Get("RemoteSyncCacheHostName"), true);
                            msync.Start();
                        }
                        if (CacheSettings.DataCacheProtocol.HasFlag(NetProtocol.Pipe))
                        {
                            mdata = new PipeDataServer(settings.Get("RemoteDataCacheHostName"), true);
                            mdata.Start();
                        }
                    }
                    //Http
                    if (CacheSettings.EnableHttpBundle)
                    {
                        httpbundle = new HttpBundleServer(settings.Get("RemoteBundleHostName"));
                        httpbundle.Start();
                    }

                    //manager
                    if (CacheSettings.CacheManagerProtocol.HasFlag(NetProtocol.Pipe))
                    {
                        mmanger = new PipeManagerServer(settings.Get("RemoteCacheManagerHostName"), true);
                        mmanger.Start();
                    }

                    if (CacheSettings.CacheManagerProtocol.HasFlag(NetProtocol.Tcp))
                    {
                        tcpmanger = new TcpManagerServer(settings.Get("RemoteCacheManagerHostName"));
                        tcpmanger.Start();
                    }

                    configWatcher = new ConfigFileWatcher();
                    configWatcher.Start(false);
                    Netlog.Debug(Settings.ServiceName + " started!");
                }
                catch (Exception ex)
                {
                    Netlog.Exception(Settings.ServiceName + " ", ex, true, true);

                }
            }

            public void Stop()
            {
                Netlog.Debug(Settings.ServiceName + " stop...");

                if (mcache != null)
                    mcache.Stop();
                if (mdata != null)
                    mdata.Stop();
                if (msession != null)
                    msession.Stop();
                if (mmanger != null)
                    mmanger.Stop();
                if (msync != null)
                    msync.Stop();

                if (tcpbundle != null)
                    tcpbundle.Stop();
                if (pipebundle != null)
                    pipebundle.Stop();
                if (httpbundle != null)
                    httpbundle.Stop();
                
                if (tcpcache != null)
                    tcpcache.Stop();
                if (tcpdata != null)
                    tcpdata.Stop();
                if (tcpsession != null)
                    tcpsession.Stop();
                if (tcpsync != null)
                    tcpsync.Stop();

                if (tcpmanger != null)
                    tcpmanger.Stop();

                if (configWatcher != null)
                    configWatcher.Stop();

                Netlog.Debug(Settings.ServiceName + " stoped.");
            }
     
    }
}
