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
using Nistec.Channels.Http;
using System.Net.Sockets;
using Nistec.Caching.Config;
using Nistec.Serialization;


namespace Nistec.Caching.Channels
{

    /// <summary>
    /// Represent Http client channel
    /// </summary>
    public class HttpClientCache : HttpClient<CacheMessage>, IDisposable
    {
        #region static send methods
        /// <summary>
        /// Send Duplex
        /// </summary>
        /// <param name="request"></param>
        /// <param name="hostAddress"></param>
        /// <param name="method"></param>
        /// <param name="readTimeout"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static object SendDuplex(CacheMessage request, string hostAddress, string method,int readTimeout, bool enableException = false)
        {
            Type type = request.BodyType;
            request.IsDuplex = true;
            using (HttpClientCache client = new HttpClientCache(hostAddress, method, readTimeout))
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
        /// <param name="method"></param>
        /// <param name="readTimeout"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T SendDuplex<T>(CacheMessage request, string hostAddress, string method, int readTimeout, bool enableException = false)
        {
            request.IsDuplex = true;
            using (HttpClientCache client = new HttpClientCache(hostAddress, method, readTimeout))
            {
                return client.Execute<T>(request, enableException);
            }
        }
        /// <summary>
        /// Send message one way.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="hostAddress"></param>
        /// <param name="method"></param>
        /// <param name="readTimeout"></param>
        /// <param name="enableException"></param>
        public static void SendOut(CacheMessage request, string hostAddress, string method, int readTimeout, bool enableException = false)
        {
            Type type = request.BodyType;
            request.IsDuplex = false;
            using (HttpClientCache client = new HttpClientCache(hostAddress, method, readTimeout))
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
            using (HttpClientCache client = new HttpClientCache(hostName))
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
            using (HttpClientCache client = new HttpClientCache(hostName))
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
            using (HttpClientCache client = new HttpClientCache(hostName))
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
        /// <param name="method"></param>
        public HttpClientCache(string hostAddress, string method)
            : base(hostAddress, method)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="method"></param>
        /// <param name="readTimeout"></param>
        public HttpClientCache(string hostAddress, string method, int readTimeout)
            : base(hostAddress, method, readTimeout)
        {

        }

       

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHost"></param>
        public HttpClientCache(string configHost)
            : base(configHost)
        {
           
        }

        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        public HttpClientCache(HttpSettings settings)
            : base(settings)
        {

        }

        #endregion

        #region override

        /// <summary>
        /// Serialize json request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override string RequestToJson(CacheMessage message)
        {
            return message.EntityWrite(new JsonSerializer(JsonSerializerMode.Write, null));
        }
        /// <summary>
        ///  Deserialize json response
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        protected override TResponse ReadJsonResponse<TResponse>(string response)
        {
            return JsonSerializer.Deserialize<TResponse>(response);
        }
        /// <summary>
        ///  Deserialize json response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override object ReadJsonResponse(string response, Type type)
        {
            return JsonSerializer.Deserialize(response, type);
        }

        #endregion
        

    }

}