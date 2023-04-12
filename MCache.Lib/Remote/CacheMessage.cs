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
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;
using System.Collections;
using Nistec.Runtime;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Channels;
using System.Net.Sockets;
using Nistec.Serialization;
using Nistec.Channels.Http;
using System.Net;
using Nistec.Caching.Config;

namespace Nistec.Caching.Remote
{
    /// <summary>
    /// Represent a cache message for pipe communications.
    /// </summary>
    [Serializable]
    public class CacheMessage : MessageStream
    {

        #region ctor
        /// <summary>
        /// Initialize a new instance of cache message.
        /// </summary>
        public CacheMessage() : base() { Formatter = MessageStream.DefaultFormatter; }

        //public static CacheMessage CreateWithBody(object body)
        //{
        //    var message = new CacheMessage();
        //    message.Formatter = MessageStream.DefaultFormatter;
        //    if (body is NetStream)
        //        message.EntityRead((NetStream)body, null);
        //    else
        //        message.SetBody(body);
        //    return message;
        //}
        ///// <summary>
        ///// Initialize a new instance of cache message.
        ///// </summary>
        ///// <param name="body"></param>
        //public CacheMessage(NetStream body) : base(body)
        //{
        //    Formatter = MessageStream.DefaultFormatter;
        //    if (body != null)
        //        EntityRead((NetStream)body, null);
        //}
        public CacheMessage(HttpRequestInfo request) : base(request)
        {
        }

        public CacheMessage(Stream stream, IBinaryStreamer streamer):base(stream, streamer)
        {
            
        }

        ///// <summary>
        ///// Initialize a new instance of cache message.
        ///// </summary>
        ///// <param name="body"></param>
        //public CacheMessage(object body) : this()
        //{
        //    Formatter = MessageStream.DefaultFormatter;
        //    if (body is NetStream)
        //        EntityRead((NetStream)body, null);
        //    else
        //        SetBody(body);
        //}

        ///// <summary>
        ///// Initialize a new instance of cache message.
        ///// </summary>
        ///// <param name="typeName"></param>
        ///// <param name="bodyStream"></param>
        //public CacheMessage(string typeName, NetStream bodyStream) : base(bodyStream,null)//(typeName, bodyStream)
        //{
        //    //TypeName = typeName;
        //    Formatter = MessageStream.DefaultFormatter;
        //}



        /// <summary>
        /// Initialize a new instance of cache message.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public CacheMessage(string command, string id, object value, int expiration)
            : this()
        {
            Command = command;
            CustomId = id;
            Expiration = expiration;
            SetBody(value);
        }
        ///// <summary>
        ///// Initialize a new instance of cache message.
        ///// </summary>
        ///// <param name="command"></param>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        ///// <param name="expiration"></param>
        ///// <param name="detail"></param>
        //public CacheMessage(string command, string key, object value, int expiration, string detail)
        //    : this()
        //{
        //    Command = command;
        //    Id = key;
        //    Expiration = expiration;
        //    Label = detail;
        //    SetBody(value);
        //}

        internal CacheMessage(MessageStream message)
            : base(message)
        {
            //if(message==null)
            //{
            //    throw new ArgumentNullException("message");
            //}
            //Command = message.Command;
            //CustomId = message.CustomId;
            //SessionId = message.SessionId;
            //Expiration = message.Expiration;
            //Label = message.Label;
            //BodyStream = message.BodyStream;
            //TypeName = message.TypeName;
            //Args = message.Args;
            //Formatter = message.Formatter;
            ////IsDuplex = message.IsDuplex;
            //DuplexType = message.DuplexType;
            //Modified = message.Modified;
            //Sender = message.Sender;
            ////Size = message.Size;
            //TransformType = message.TransformType;
        }
        #endregion   

#if(false)
        #region Read/Write pipe

        internal string ReadResponseAsJson(NamedPipeClientStream stream, int ReceiveBufferSize, TransformType transformType, bool isTransStream)
        {// = 8192

            if (isTransStream)
            {
                using (TransStream ts = TransStream.CopyFrom(stream, ReceiveBufferSize))
                {
                    return ts.ReadJson();
                }
            }

            using (TransStream ack = new TransStream(stream, ReceiveBufferSize, transformType, isTransStream))
            {
                return ack.ReadJson();
            }
        }

        internal object ReadResponse(NamedPipeClientStream stream, int ReceiveBufferSize, TransformType transformType, bool isTransStream)
        {
            if (isTransStream)//transformType == TransformType.Stream)
            {
                return TransStream.CopyFrom(stream, ReceiveBufferSize);
                //return new TransStream(stream, ReceiveBufferSize);
            }
            using (TransStream ack = new TransStream(stream,  ReceiveBufferSize, TransformType.Object,isTransStream))
            {
                return ack.ReadValue();
            }
        }

        internal TResponse ReadResponse<TResponse>(NamedPipeClientStream stream, int ReceiveBufferSize = 8192)
        {
            if (TransReader.IsTransStream(typeof(TResponse)))
            {
                TransStream ts = TransStream.CopyFrom(stream, ReceiveBufferSize);
                //TransStream ack = new TransStream(stream, ReceiveBufferSize);
                return GenericTypes.Cast<TResponse>(ts, true);
            }
            using (TransStream ack = new TransStream(stream, ReceiveBufferSize,TransformType.Object,false))
            {
                return ack.ReadValue<TResponse>();
            }
        }


        internal static CacheMessage ReadRequest(NamedPipeServerStream pipeServer, int ReceiveBufferSize = 8192)
        {
            CacheMessage message = new CacheMessage();
            message.EntityRead(pipeServer, null);
            return message;
        }

        internal static void WriteResponse(NamedPipeServerStream pipeServer, NetStream bResponse)
        {
            if (bResponse == null)
            {
                return;
            }
            pipeServer.Write(bResponse.ToArray(), 0, bResponse.iLength);

            pipeServer.Flush();
        }

        #endregion

        #region Read/Write tcp
        //= 8192
        //internal string ReadResponseAsJson(NetworkStream stream, int ReceiveBufferSize , TransformType transformType)
        //{
        //    using (TransStream ack = new TransStream(stream, 0, ReceiveBufferSize, transformType))
        //    {
        //        return ack.ReadJson();
        //    }
        //}

        //internal object ReadResponse(NetworkStream stream, int readTimeout, int ReceiveBufferSize, TransformType transformType)
        //{
        //    if (transformType == TransformType.Stream)
        //    {
        //        return new TransStream(stream,  0, ReceiveBufferSize);
        //    }
        //    using (TransStream ack = new TransStream(stream,  0,ReceiveBufferSize, TransformType.Object))
        //    {
        //        return ack.ReadValue();
        //    }
        //}

        //internal TResponse ReadResponse<TResponse>(NetworkStream stream, int readTimeout, int ReceiveBufferSize = 8192)
        //{
        //    if (TransReader.IsTransStream(typeof(TResponse)))
        //    {
        //        var ts = new TransStream(stream,  0, ReceiveBufferSize);
        //        return GenericTypes.Cast<TResponse>(ts, true);
        //    }
        //    using (TransStream ack = new TransStream(stream,  0,ReceiveBufferSize, TransformType.Object))
        //    {
        //        return ack.ReadValue<TResponse>();
        //    }
        //}

        internal static NetStream FaultStream(string faultDescription)
        {
            var message = new CacheMessage("Fault", "Fault", faultDescription, 0);
            return message.Serialize();
        }

        internal static CacheMessage ReadRequest(NetworkStream streamServer, int ReceiveBufferSize = 8192)
        {
            var message = new CacheMessage();
            message.EntityRead(streamServer, null);
            return message;
        }

        internal static void WriteResponse(NetworkStream streamServer, NetStream bResponse)
        {
            if (bResponse == null)
            {
                return;
            }

            int cbResponse = bResponse.iLength;

            streamServer.Write(bResponse.ToArray(), 0, cbResponse);

            streamServer.Flush();

        }


        #endregion

        #region Read/Write http

        internal static CacheMessage ReadRequest(HttpRequestInfo request)
        {
            if (request.BodyStream != null)
            {
                var msg = MessageStream.ParseStream(request.BodyStream, NetProtocol.Http);
                return new CacheMessage(msg);
            }
            else
            {

                var message = new CacheMessage();
                if (request.BodyType == HttpBodyType.QueryString)
                    message.EntityRead(request.QueryString, null);
                else if (request.Body != null)
                    message.EntityRead(request.Body, null);
                else if (request.Url.LocalPath != null && request.Url.LocalPath.Length > 1)
                    message.EntityRead(request.Url.LocalPath.TrimStart('/').TrimEnd('/'), null);

                return message;
            }
        }

        internal static void WriteResponse(HttpListenerContext context, NetStream bResponse)
        {
            var response = context.Response;
            if (bResponse == null)
            {
                response.StatusCode = (int)HttpStatusCode.NoContent;
                response.StatusDescription = "No response";
                return;
            }

            int cbResponse = bResponse.iLength;
            byte[] buffer = bResponse.ToArray();

            

            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = HttpStatusCode.OK.ToString();
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();

        }


        #endregion
#endif

        #region extension

        [NoSerialize]
        public string CacheKey { get { return CustomId; } internal set { CustomId = value; } }
        [NoSerialize]
        public string TableName { get { return Label; } internal set { Label = value; } }
        [NoSerialize]
        public string DbName { get { return Args[KnownArgs.DbName]; } internal set { Args[KnownArgs.DbName] = value; } }
        [NoSerialize]
        public string MappingName { get { return Args[KnownArgs.MappingName]; } internal set { Args[KnownArgs.MappingName] = value; } }

        [NoSerialize]
        internal string CommandType
        {
            get
            {
                return Command.Substring(0, 5);
            }
        }
        /*
        /// <summary>
        /// Convert <see cref="IDictionary"/> to <see cref="MessageStream"/>.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static MessageStream ConvertFrom(IDictionary dict)
        {
            CacheMessage message = new CacheMessage()
            {
                Command = dict.Get<string>("Command"),
                Sender = dict.Get<string>("Sender"),
                Identifier = dict.Get<string>("Identifier"),
                Args = dict.Get<NameValueArgs>("Args"),
                BodyStream = dict.Get<NetStream>("Body", null),//, ConvertDescriptor.Implicit),
                Expiration = dict.Get<int>("Expiration", 0),
                IsDuplex = dict.Get<bool>("IsDuplex", true),
                Modified = dict.Get<DateTime>("Modified", DateTime.Now),
                TypeName = dict.Get<string>("TypeName"),
                Label = dict.Get<string>("Label"),
                TransformType= (TransformType)dict.Get<byte>("TransformType")
            };

            return message;
        }
        */
        /// <summary>
        /// Convert stream to json.
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
        #endregion
 
    }

}
