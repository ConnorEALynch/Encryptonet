using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Encryptonet_Sockets
{
    /*
     * CLASS: public static class Logger
     * 
     * DESCRIPTION: creates log file in local directory
     * 
     * keep improving and I will have a reliable log class
     * 
     */
    public static class Logger
    {
        //ude string builder as this gets more advanced
        private static string header = System.DateTime.Now.ToString() + ">>";
        public static void LogMessage(string message)
        {
            Write(header + message);
        }

        public static void LogError(Exception exception)
        {
            Write(header + exception.ToString() + ": " + exception.Message);
        }

        private static void Write(string formattedMessage)
        {
            using (StreamWriter file =
            new StreamWriter(@"./Log.txt", true))
            {
                file.WriteLine(formattedMessage);
            }
        }
    }
}
