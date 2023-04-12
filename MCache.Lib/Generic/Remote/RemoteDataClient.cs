#define SERVICE

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Runtime.Remoting;  
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;
using System.Threading;

using MControl.Caching;

namespace MControl.Caching.Remote
{


    [SecurityPermission(SecurityAction.Assert)]
    public class RemoteDataClient 
    {

		#region <Members>

        const string host = "ipc://portRemoteMCache/RemoteDataServer.rem";

        //string m_CacheName;
        IRemoteData manager;
        ManualResetEvent resetEvent;

 		#endregion

		#region <Ctor>

        public RemoteDataClient(/*string cacheName*/)
         {
             //m_CacheName = "RemoteCache";// cacheName;
             manager = (IRemoteData)Activator.GetObject
             (
             typeof(IRemoteData),
             host
             );

             //object remoteObject = Activator.GetObject(typeof(ISharedInterface<string>), "ipc://test/stringRemoteObject.rem");
             //Console.WriteLine(manager.GetType().ToString());
             //Console.WriteLine(manager.GetType().IsAssignableFrom(typeof(IRemoteCache)));
             //Console.WriteLine(typeof(IRemoteCache).IsAssignableFrom(manager.GetType()));
            
             if (manager == null)
             {
                 Console.WriteLine("cannot locate server");
             }
             else
             {
                 //Console.WriteLine("Remote Object responds: " + manager.Reply("Remote queue activated"));
                 Console.WriteLine(manager.Reply("Remote data activated"));
             }

             resetEvent = new ManualResetEvent(false);
         }
 
		#endregion

#if SERVICE

        public static IRemoteData Instance
        {
            get { return new RemoteDataClient().manager; }
        }

#else
        static readonly RemoteCache instance= new RemoteCache();
        public static IRemoteData Instance
        {
            get {return instance;}
        }
#endif

        public IRemoteData Cache
        {
            get { return manager; }
        }

        public string Reply(string text)
        {
            return manager.Reply(text);
        }

	}
}

    
