using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //for testing:
            CreateRedirectionRulesFile();
            
            //Start server
            Server server = new Server(1000, "redirectionRules.txt");
            server.StartServer();
        }

        //Create the simple redirection rules:
        static void CreateRedirectionRulesFile()
        {
            StreamWriter writer = new StreamWriter("redirectionRules.txt");
            writer.WriteLine("aboutus.html,aboutus2.html");
            writer.Close();
        }
         
    }
}
