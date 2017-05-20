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
using System.IO.Pipes;
using System.Text;
using System.IO;
using Nistec.Caching.Remote;
using Nistec.Caching;
using Nistec.Channels;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Runtime;
using System.Runtime.Serialization;
using System.Threading;
using Nistec.Channels.Tcp;
using System.Net.Sockets;
using Nistec.Caching.Config;


namespace Nistec.Caching.Channels
{

    /// <summary>
    /// Represent tcp client channel
    /// </summary>
    public class TcpClientCache : TcpClient<CacheMessage>, IDisposable
    {
        #region static send methods
        /// <summary>
        /// Send Duplex
        /// </summary>
        /// <param name="request"></param>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="readTimeout"></param>
        /// <param name="IsAsync"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static object SendDuplex(CacheMessage request, string hostAddress, int port,int readTimeout, bool IsAsync, bool enableException = false)
        {
            Type type = request.BodyType;
            request.IsDuplex = true;
            using (TcpClientCache client = new TcpClientCache(hostAddress, port, readTimeout, IsAsync))
            {
                return client.Execute(request, type, enableException);
            }
        }
        /// <summary>
        /// Send Duplex
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="readTimeout"></param>
        /// <param name="IsAsync"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T SendDuplex<T>(CacheMessage request, string hostAddress, int port, int readTimeout, bool IsAsync, bool enableException = false)
        {
            request.IsDuplex = true;
            using (TcpClientCache client = new TcpClientCache(hostAddress, port, readTimeout, IsAsync))
            {
                return client.Execute<T>(request, enableException);
            }
        }
        /// <summary>
        /// Send message one way.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="readTimeout"></param>
        /// <param name="IsAsync"></param>
        /// <param name="enableException"></param>
        public static void SendOut(CacheMessage request, string hostAddress, int port,int readTimeout, bool IsAsync, bool enableException = false)
        {
            Type type = request.BodyType;
            request.IsDuplex = false;
            using (TcpClientCache client = new TcpClientCache(hostAddress, port, readTimeout, IsAsync))
            {
                client.Execute(request, type, enableException);
            }
        }
        /// <summary>
        /// Send Duplex
        /// </summary>
        /// <param name="request"></param>
        /// <param name="hostName"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static object SendDuplex(CacheMessage request, string hostName, bool enableException = false)
        {
            Type type = request.BodyType;
            request.IsDuplex = true;
            using (TcpClientCache client = new TcpClientCache(hostName))
            {
                return client.Execute(request, type, enableException);
            }
        }
        /// <summary>
        /// Send Duplex
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="hostName"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T SendDuplex<T>(CacheMessage request, string hostName, bool enableException = false)
        {
            request.IsDuplex = true;
            using (TcpClientCache client = new TcpClientCache(hostName))
            {
                return client.Execute<T>(request, enableException);
            }
        }
        /// <summary>
        /// Send message one way.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="hostName"></param>
        /// <param name="enableException"></param>
        public static void SendOut(CacheMessage request, string hostName, bool enableException = false)
        {
            Type type = request.BodyType;
            request.IsDuplex = false;
            using (TcpClientCache client = new TcpClientCache(hostName))
            {
                client.Execute(request, type, enableException);
            }
        }

        #endregion

        #region ctor

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="readTimeout"></param>
        public TcpClientCache(string hostAddress, int port, int readTimeout)
            : base(hostAddress, port,readTimeout, false)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="readTimeout"></param>
        /// <param name="isAsync"></param>
        public TcpClientCache(string hostAddress, int port, int readTimeout, bool isAsync)
            : base(hostAddress, port, readTimeout, isAsync)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="readTimeout"></param>
        /// <param name="inBufferSize"></param>
        /// <param name="outBufferSize"></param>
        /// <param name="isAsync"></param>
        public TcpClientCache(string hostAddress, int port, int readTimeout, int inBufferSize, int outBufferSize,bool isAsync)
            : base(hostAddress, port, readTimeout,inBufferSize, outBufferSize, isAsync)
        {

        }

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHost"></param>
        public TcpClientCache(string configHost)
        {
            Settings = TcpClientCacheSettings.GetTcpClientSettings(configHost);
        }

        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        public TcpClientCache(TcpSettings settings)
            : base(settings)
        {

        }

        #endregion

        #region override
        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        protected override void ExecuteMessage(NetworkStream stream, CacheMessage message)
        {
            // Send a request from client to server
            message.EntityWrite(stream, null);

            if (message.IsDuplex == false)
            {
                return;
            }

            // Receive a response from server.
            message.ReadAck(stream, Settings.ReadTimeout, Settings.ReceiveBufferSize);
        }
        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override object ExecuteMessage(NetworkStream stream, CacheMessage message, Type type)
        {
            object response = null;

            // Send a request from client to server
            message.EntityWrite(stream, null);


            if (message.IsDuplex == false)
            {
                return response;
            }

            // Receive a response from server.
            response = message.ReadAck(stream, type, Settings.ReadTimeout, Settings.ReceiveBufferSize);

            return response;
        }
        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TResponse ExecuteMessage<TResponse>(NetworkStream stream, CacheMessage message)
        {
            TResponse response = default(TResponse);

            // Send a request from client to server
            message.EntityWrite(stream, null);


            if (message.IsDuplex == false)
            {
                return response;
            }

            // Receive a response from server.
            response = message.ReadAck<TResponse>(stream, Settings.ReadTimeout, Settings.ReceiveBufferSize);

            return response;
        }
        
        /// <summary>
        /// connect to the tcp channel and execute request.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public new MessageAck Execute(CacheMessage message, bool enableException = false)
        {
            return Execute<MessageAck>(message, enableException);
        }



        #endregion

    }

}