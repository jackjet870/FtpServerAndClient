using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SharpFtpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FtpServer server = new FtpServer())
            {
                if (!Directory.Exists("C:\\FtpServer"))
                    Directory.CreateDirectory("C:\\FtpServer");
                server.ServerPath = "C:\\FtpServer";
                try
                {
                    server.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.ToString());
                }
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
            }
        }
    }
}
