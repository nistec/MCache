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
using Nistec.Caching.Remote;
using Nistec.Channels;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Nistec.Caching.Channels
{
    /// <summary>
    /// Represent pipe client channel
    /// </summary>
    public class PipeClientCache : Nistec.Channels.PipeClient<CacheMessage>, IDisposable
    {

        #region static send methods with enableException

        /// <summary>
        /// Send Duplex message with return value.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PipeName"></param>
        /// <param name="enableException"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string SendJsonDuplex(string request, string PipeName, bool enableException = false, PipeOptions option = PipeOptions.None)
        {
            CacheMessage msg = new CacheMessage();
            msg.EntityRead(request, null);
            using (PipeClientCache client = new PipeClientCache(PipeName, true, option))
            {
                client.PipeDirection = PipeDirection.InOut;
                var o= client.Execute(msg, enableException);
                if (o == null)
                    return null;
                return JsonSerializer.Serialize(o);
            }
        }
        /// <summary>
        /// Send Out message with no return value.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PipeName"></param>
        /// <param name="enableException"></param>
        /// <param name="option"></param>
        public static void SendJsonOut(string request, string PipeName, bool enableException = false, PipeOptions option = PipeOptions.None)
        {
            CacheMessage msg = new CacheMessage();
            msg.EntityRead(request, null);
            using (PipeClientCache client = new PipeClientCache(PipeName, false, option))
            {
                client.PipeDirection = PipeDirection.Out;
                client.Execute(msg, enableException);
            }
        }

        /// <summary>
        /// Send Duplex message with return value.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PipeName"></param>
        /// <param name="enableException"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static object SendDuplex(CacheMessage request, string PipeName, bool enableException = false, PipeOptions option = PipeOptions.None)
        {
            //Type type = request.BodyType;
            request.IsDuplex = true;
            PipeDirection direction = request.IsDuplex ? PipeDirection.InOut : PipeDirection.Out;
            using (PipeClientCache client = new PipeClientCache(PipeName, true, option))
            {
                return client.Execute(request, enableException);
            }
        }
        /// <summary>
        /// Send Duplex message with return value.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PipeName"></param>
        /// <param name="enableException"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static TransStream SendDuplexStream(CacheMessage request, string PipeName, bool enableException = false, PipeOptions option = PipeOptions.None)
        {
            request.IsDuplex = true;
            //request.TransformType = TransformType.Stream;
            using (PipeClientCache client = new PipeClientCache(PipeName, true, option))
            {
                return client.Execute<TransStream>(request, enableException);
            }
        }

        /// <summary>
        /// Send Duplex message with return value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="PipeName"></param>
        /// <param name="enableException"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static T SendDuplex<T>(CacheMessage request, string PipeName, bool enableException = false, PipeOptions option = PipeOptions.None)
        {
            request.IsDuplex = true;
            using (PipeClientCache client = new PipeClientCache(PipeName, true, option))
            {
                return client.Execute<T>(request, enableException);
            }
        }
        /// <summary>
        /// Send Out message with no return value.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PipeName"></param>
        /// <param name="enableException"></param>
        /// <param name="option"></param>
        public static void SendOut(CacheMessage request, string PipeName, bool enableException = false, PipeOptions option = PipeOptions.None)
        {
            request.IsDuplex = false;
            using (PipeClientCache client = new PipeClientCache(PipeName, false, option))
            {
                client.Execute(request, enableException);
            }
        }
        /// <summary>
        /// Get message from server.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PipeName"></param>
        /// <param name="enableException"></param>
        /// <param name="option"></param>
        public static void SendIn(CacheMessage request, string PipeName, bool enableException = false, PipeOptions option = PipeOptions.None)
        {
            request.IsDuplex = false;
            using (PipeClientCache client = new PipeClientCache(PipeName, false, option))
            {
                client.PipeDirection = PipeDirection.In;
                client.Execute(request, enableException);
            }
        }
        #endregion

        #region ctor

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="isDuplex"></param>
        /// <param name="option"></param>
        public PipeClientCache(string pipeName, bool isDuplex, PipeOptions option= PipeOptions.None)
            : base(pipeName, isDuplex, option)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="inBufferSize"></param>
        /// <param name="outBufferSize"></param>
        /// <param name="isDuplex"></param>
        /// <param name="option"></param>
        public PipeClientCache(string pipeName, int inBufferSize, int outBufferSize, bool isDuplex, PipeOptions option = PipeOptions.None)
            : base(pipeName, inBufferSize, outBufferSize, isDuplex, option)
        {

        }


        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHostName"></param>
        /// <param name="direction"></param>
        public PipeClientCache(string configHostName, PipeDirection direction)
            : base(configHostName, direction)
        {

        }

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHostName"></param>
        public PipeClientCache(string configHostName)
            : base(configHostName, System.IO.Pipes.PipeDirection.InOut)
        {
        }

        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        public PipeClientCache(PipeSettings settings)
            : base(settings)
        {

        }

        #endregion

        #region override

        protected override void ExecuteOneWay(CacheMessage message)
        {
            // Send a request from client to server
            message.EntityWrite(pipeClientStream, null);
        }

        /// <summary>
        /// Execute Message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override object ExecuteMessage(CacheMessage message)//, Type type)
        {
            object response = null;

            if (PipeDirection != System.IO.Pipes.PipeDirection.In)
            {
                // Send a request from client to server
                message.EntityWrite(pipeClientStream, null);
            }

            if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
            {
                return response;
            }

            // Receive a response from server.
            response = message.ReadResponse(pipeClientStream, Settings.ReceiveBufferSize, message.TransformType, false);

            return response;
        }
        /// <summary>
        /// Execute Message
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TResponse ExecuteMessage<TResponse>(CacheMessage message)
        {
            TResponse response = default(TResponse);

            if (PipeDirection != System.IO.Pipes.PipeDirection.In)
            {
                // Send a request from client to server
                message.EntityWrite(pipeClientStream, null);
            }

            if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
            {
                return response;
            }

            // Receive a response from server.
            response = message.ReadResponse<TResponse>(pipeClientStream, Settings.ReceiveBufferSize);

            return response;
        }
        //protected override TransStream ExecuteMessageStream(CacheMessage message)
        //{
        //    TransStream response = null;

        //    if (PipeDirection != System.IO.Pipes.PipeDirection.In)
        //    {
        //        // Send a request from client to server
        //        message.EntityWrite(pipeClientStream, null);
        //    }

        //    if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
        //    {
        //        return response;
        //    }

        //    // Receive a response from server.
        //    response = new TransStream(pipeClientStream, message.TransformType, Settings.ReceiveBufferSize);// message.ReadAck(pipeClientStream, message.TransformType, Settings.ReceiveBufferSize);

        //    return response;
        //}
    

        ///// <summary>
        ///// connect to the named pipe and execute request.
        ///// </summary>
        //public MessageAck Execute(CacheMessage message, bool enableException = false)
        //{
        //    return Execute<MessageAck>(message, enableException);
        //}



        #endregion

    }
}

