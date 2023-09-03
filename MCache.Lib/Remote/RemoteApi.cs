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
//using Nistec.Caching.Channels;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Serialization;
using Nistec.Channels.Tcp;
using Nistec.Channels.Http;
using Nistec.Runtime;

namespace Nistec.Caching.Remote
{
    public abstract class RemoteApi
    {
        //public const int DefaultExpiration = 0;

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

        public string ToJson(object obj, bool pretty = true)
        {
            if (obj == null)
                return null;
            return JsonSerializer.Serialize(obj, null, pretty ? JsonFormat.Indented : JsonFormat.None);
        }

        //public static string GetKey(string name, params string[] keys)
        //{
        //   return string.Format("{0}:{1}", name, string.Join("_", keys));
        //}

        //public static string PrintJsonRequest(NetProtocol protocol, string hostname, int port, string command, string key, object value = null, int expiration = 0, string sessionId = null)
        //{
        //    var response = JsonRequest(protocol, hostname, port, command, key, value, expiration, sessionId);
        //    if (response != null)
        //        response = JsonSerializer.Print(response);
        //    return response;
        //}
        //public static string JsonRequest(NetProtocol protocol, string hostname, int port,string command, string key, object value=null, int expiration=0, string sessionId=null)
        //{
        //    if (string.IsNullOrWhiteSpace(command))
        //    {
        //        throw new ArgumentNullException("command is required");
        //    }

        //    CacheMessage message = new CacheMessage(command, key, value, expiration, sessionId);
        //    string json = JsonSerializer.Serialize(message);
        //    string response = null;

        //    switch (protocol)
        //    {
        //        case NetProtocol.Pipe:
        //            response = PipeJsonClient.SendDuplex(json, hostname, false);
        //            break;
        //        case NetProtocol.Tcp:
        //            response = TcpJsonClient.SendDuplex(json, hostname, port, 0, false);
        //            break;
        //        case NetProtocol.Http:
        //            response = HttpJsonClient.SendDuplex(json, hostname, false);
        //            break;
        //    }
        //    return response;
        //}


        #region SendJson

        //public string SendJsonDuplex(string command, ComplexKey ck, bool pretty = false)
        //{
        //    if(command==null)
        //    {
        //        throw new ArgumentNullException("command");
        //    }
        //    if (ck == null)
        //    {
        //        throw new ArgumentNullException("ck");
        //    }

        //    CacheMessage message = new CacheMessage()//command, ck.Prefix, null, 0, ck.Suffix);
        //    {
        //        Command=command,
        //        Id=ck.Prefix,
        //        Label=ck.Suffix
        //    };
        //    string json = JsonSerializer.Serialize(message);
        //    string response = SendJsonDuplex(json, pretty);
        //    return response;
        //}
        //public string SendJsonDuplex(string command, ComplexKey ck, NameValueArgs keyValueArgs, bool pretty = false)
        //{
        //    if (command == null)
        //    {
        //        throw new ArgumentNullException("command");
        //    }
        //    if (ck == null)
        //    {
        //        throw new ArgumentNullException("ck");
        //    }

        //    CacheMessage message = new CacheMessage(command, ck.Prefix, null, 0, ck.Suffix);
        //    message.Args = keyValueArgs;
        //    string json = JsonSerializer.Serialize(message);
        //    string response = SendJsonDuplex(json, pretty);
        //    return response;
        //}
        //public string SendJsonDuplex(string command, ComplexKey ck, string[] keyValueArgs, bool pretty = false)
        //{
        //    if (command == null)
        //    {
        //        throw new ArgumentNullException("command");
        //    }
        //    if (ck == null)
        //    {
        //        throw new ArgumentNullException("ck");
        //    }

        //    CacheMessage message = new CacheMessage(command, ck.Prefix, null, 0, ck.Suffix);
        //    message.Args = CacheMessage.CreateArgs(keyValueArgs);
        //    string json = JsonSerializer.Serialize(message);
        //    string response = SendJsonDuplex(json, pretty);
        //    return response;
        //}
        //public string SendJsonDuplex(string command, ComplexKey ck, object value, int expiration, bool pretty = false)
        //{
        //    if (command == null)
        //    {
        //        throw new ArgumentNullException("command");
        //    }
        //    if (ck == null)
        //    {
        //        throw new ArgumentNullException("ck");
        //    }

        //    CacheMessage message = new CacheMessage(command, ck.Prefix, value, expiration, ck.Suffix);
        //    string json = JsonSerializer.Serialize(message);
        //    string response = SendJsonDuplex(json, pretty);
        //    return response;
        //}
        //public string SendJsonDuplex(string command, string key, object value , int expiration, string detail = null, bool pretty=false)
        //{
        //    CacheMessage message = new CacheMessage(command, key, value, expiration, detail);
        //    string json = JsonSerializer.Serialize(message);
        //    string response = SendJsonDuplex(json, pretty);
        //    return response;
        //}
        //public string SendJsonDuplex(string command, string key, object value, bool pretty = false)
        //{
        //    CacheMessage message = new CacheMessage(command, key, value,0);
        //    string json = JsonSerializer.Serialize(message);
        //    string response = SendJsonDuplex(json, pretty);
        //    return response;
        //}

        //public string SendJsonDuplex(string command, string key, bool pretty = false)
        //{
        //    CacheMessage message = new CacheMessage(command, key, null, 0);
        //    string json = JsonSerializer.Serialize(message);
        //    string response = SendJsonDuplex(json, pretty);
        //    return response;
        //}

        //public string SendJsonDuplex(CacheMessage message, bool pretty = false)
        //{
        //    string json = JsonSerializer.Serialize(message);
        //    string response = SendJsonDuplex(json, pretty);
        //    return response;
        //}
        //public void SendJsonOut(CacheMessage message)
        //{
        //    string json = JsonSerializer.Serialize(message);
        //    SendJsonOut(json);
        //}
        //public string SendJsonDuplex(string json, bool pretty = false)
        //{
        //    string response = null;

        //    switch (Protocol)
        //    {
        //        case NetProtocol.Pipe:
        //            response = PipeJsonClient.SendDuplex(json, RemoteHostName, false);
        //            break;
        //        case NetProtocol.Tcp:
        //            response = TcpJsonClient.SendDuplex(json, RemoteHostName, false);
        //            break;
        //        case NetProtocol.Http:
        //            response = HttpJsonClient.SendDuplex(json, RemoteHostName, false);
        //            break;
        //    }

        //    if (pretty)
        //    {
        //        if (response != null)
        //            response = JsonSerializer.Print(response);
        //    }
        //    return response;
        //}

        //public void SendJsonOut(string json)
        //{
        //    switch (Protocol)
        //    {
        //        case NetProtocol.Pipe:
        //            PipeJsonClient.SendOut(json, RemoteHostName, false);
        //            break;
        //        case NetProtocol.Tcp:
        //            TcpJsonClient.SendOut(json, RemoteHostName, false);
        //            break;
        //        case NetProtocol.Http:
        //            HttpJsonClient.SendOut(json, RemoteHostName, false);
        //            break;
        //    }
        //}
        #endregion

        #region cache message json

        public string SendHttpJsonDuplex(CacheMessage message, bool pretty = false)
        {
            string response = null;

            message.TransformType = TransformType.Json;
            //message.IsDuplex = true;
            message.DuplexType = DuplexTypes.Respond;
            response = HttpClient.SendDuplexJson(message, RemoteHostName, false);
            //response = HttpClientCache.SendDuplexJson(message, RemoteHostName, false);

            if (pretty)
            {
                if (response != null)
                    response = JsonSerializer.Print(response);
            }
            return response;
        }

        public void SendHttpJsonOut(CacheMessage message)
        {
            HttpClient.SendOutJson(message, RemoteHostName, false);
            //HttpClientCache.SendOut(message, RemoteHostName, false);
        }

        #endregion

        #region Send Stream

        public T SendDuplexStream<T>(CacheMessage message, Action<string> onFault)
        {
            TransStream ts = SendDuplexStream(message);
            if (ts == null)
                onFault(message.Command + " return null");
            return ts.ReadValue<T>(onFault);
        }
        public object SendDuplexStreamValue(CacheMessage message, Action<string> onFault)
        {
            TransStream ts = SendDuplexStream(message);
            if (ts == null)
                onFault(message.Command + " return null");
            return ts.ReadValue(onFault);
        }
        public CacheState SendDuplexState(CacheMessage message)
        {
            TransStream ts = SendDuplexStream(message);
            if (ts == null)
                return CacheState.UnKnown;
            return (CacheState)ts.ReadState();
        }
        public TransStream SendDuplexStream(CacheMessage message)
        {
            message.TransformType = TransformType.Stream;
            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClient.SendDuplexStream(message, RemoteHostName, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClient.SendDuplexStream(message, RemoteHostName, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpStreamClient.SendDuplexStream(message, RemoteHostName, EnableRemoteException);
        }

        public void SendOut(CacheMessage message)
        {
            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClient.SendOut(message, RemoteHostName, EnableRemoteException);
                    break;
                case NetProtocol.Pipe:
                    PipeClient.SendOut(message, RemoteHostName,EnableRemoteException);
                    break;
                case NetProtocol.Tcp:
                default:
                    TcpStreamClient.SendOut(message, RemoteHostName, EnableRemoteException);
                    break;
            }
        }
        #endregion

        #region Send internal
        /*
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
                Id = key,
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
                Id = key,
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

        internal object SendDuplex(string command, string key, TransformType transformType= TransformType.Message)//, string typeName)
        {

            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                Id = key,
                //TypeName = typeName,
                 TransformType= transformType
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
         */
        internal void SendOut(string command, string key)
        {
            using (CacheMessage message = new CacheMessage()
            {
                Command = command,
                CustomId = key
            })
            {
                switch (Protocol)
                {
                    case NetProtocol.Http:
                        HttpClient.SendOut(message, RemoteHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Pipe:
                        PipeClient.SendOut(message, RemoteHostName, EnableRemoteException);
                        break;
                    case NetProtocol.Tcp:
                    default:
                        TcpStreamClient.SendOut(message, RemoteHostName, EnableRemoteException);
                        break;
                }
            }
        }
       
        #endregion
    }
}
