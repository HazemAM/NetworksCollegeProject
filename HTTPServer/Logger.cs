using System;
using System.IO;

namespace HTTPServer
{
    class Logger
    {
        public static void LogException(Exception ex)
        {
            StreamWriter writer = new StreamWriter("log.txt", true);
            writer.WriteLine("DateTime: " + DateTime.Now);
            writer.WriteLine("Message: " + ex.Message);
            writer.WriteLine("-----------------------------");
            writer.Close();

        }
    }
}
