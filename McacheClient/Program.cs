﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using Nistec.IO;
using Nistec.Channels.RemoteCache;
using Nistec.Generic;
using Nistec.Serialization;
using System.Diagnostics;
using System.Linq;


namespace Nistec.Caching.Demo
{

    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            //Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WindowHeight = (int)(Console.LargestWindowHeight * 0.70);
            Console.WindowWidth = (int)(Console.LargestWindowWidth * 0.70);
            Console.Title = "Nistec cache client";



            Console.WriteLine("Welcome to: Nistec Cache commander...");
            Console.WriteLine("=====================================");

            Controller.EnableLog = Nistec.Generic.NetConfig.Get<bool>("EnableLog", false);
            Controller.EnableJsonController = Nistec.Generic.NetConfig.Get<bool>("EnableJsonController", false);

            Controller.Run(args);

            //RunTest();

            Console.WriteLine("Finished...");
            Console.ReadLine();

        }

        static void RunTest()
        {
            Nistec.Channels.NetProtocol netprotocol = Nistec.Channels.NetProtocol.Tcp;

            do
            {
                Console.WriteLine("start...protocol: " + netprotocol.ToString());
                string protocol = Console.ReadLine();
                if (protocol != null && protocol.Length > 0)
                    netprotocol = protocol == "pipe" ? Nistec.Channels.NetProtocol.Pipe : Nistec.Channels.NetProtocol.Tcp;
                if (Controller.EnableJsonController)
                    CmdController.DoCommandSyncJson(netprotocol, "printentityvalues", "contactEntity", "","");
                else
                    CmdController.DoCommandSync(netprotocol, "printentityvalues", "contactEntity", "","","");
                Console.WriteLine("end...");

            } while (Console.ReadLine() != "q");
        }

    }
}
