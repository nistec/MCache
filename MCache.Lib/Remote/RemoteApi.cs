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
using Nistec.Channels;
using Nistec.Caching.Channels;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Serialization;

namespace Nistec.Caching.Remote
{
    public abstract class RemoteApi
    {
        protected NetProtocol Protocol;
        protected string RemoteHostName;
        protected bool EnableRemoteException;



        /// <summary>
        /// CConvert stream to json format.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToJson(NetStream stream, JsonFormat format)
        {
            using (BinaryStreamer streamer = new BinaryStreamer(stream))
            {
                var obj = streamer.Decode();
                if (obj == null)
                    return null;
                else
                    return JsonSerializer.Serialize(obj, null, format);
            }
        }


        #region internal

        internal T SendDuplex<T>(CacheMessage message)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);
        }

        internal T SendDuplex<T>(string command, string key)
        {

            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                Key = key,
                TypeName = typeof(T).FullName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);
            }
        }

        internal T SendDuplex<T>(string command, string key, string id)
        {

            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                Key = key,
                TypeName = typeof(T).FullName,
                Id = id
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex<T>(message, RemoteHostName, EnableRemoteException);
            }
        }
        internal object SendDuplex(CacheMessage message)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientCache.SendDuplex(message, RemoteHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientCache.SendDuplex(message, RemoteHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientCache.SendDuplex(message, RemoteHostName, EnableRemoteException);
        }
        internal object SendDuplex(string command, string key, string typeName)
        {

            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                Key = key,
                TypeName = typeName
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        return HttpClientCache.SendDuplex(message, RemoteHostName, EnableRemoteException);

                    case NetProtocol.Pipe:
                        return PipeClientCache.SendDuplex(message, RemoteHostName, EnableRemoteException);

                    case NetProtocol.Tcp:
                        break;
                }
                return TcpClientCache.SendDuplex(message, RemoteHostName, EnableRemoteException);
            }
        }

        internal void SendOut(CacheMessage message)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClientCache.SendOut(message, RemoteHostName, EnableRemoteException);
                    break;
                case NetProtocol.Pipe:
                    PipeClientCache.SendOut(message, RemoteHostName, EnableRemoteException);
                    break;
                case NetProtocol.Tcp:
                default:
                    TcpClientCache.SendOut(message, RemoteHostName, EnableRemoteException);
                    break;
            }
        }

        internal void SendOut(string command, string key)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                Key = key
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        HttpClientCache.SendOut(message, RemoteHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        PipeClientCache.SendOut(message, RemoteHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        TcpClientCache.SendOut(message, RemoteHostName, EnableRemoteException);
                        break;
                }
            }
        }

        #endregion
    }
}
