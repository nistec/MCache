using System;
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
              Console.WindowHeight =(int) (Console.LargestWindowHeight*0.70);
              Console.WindowWidth = (int)(Console.LargestWindowWidth * 0.70);
              Console.Title = "Nistec cache console";
              
              

              Console.WriteLine("Welcome to: Nistec Cache commander...");
              Console.WriteLine("=====================================");
              Controller.Run(args);
              Console.WriteLine("Finished...");
              Console.ReadLine();

          }

         
    }
}
