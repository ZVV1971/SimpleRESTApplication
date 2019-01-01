using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Threading.Tasks;

namespace SimpleRESTApplication.Alumni
{
    public static class Logger
    {
        private static string LogFileName;

        static Logger()
        {
            LogFileName = DateTime.Now.ToShortDateString().Replace(".", "");
        }

        public static void WriteToLog(string message)
        {
            if (!File.Exists(Path.Combine(HttpRuntime.AppDomainAppPath, LogFileName)))
            {
                File.Create(Path.Combine(HttpRuntime.AppDomainAppPath, LogFileName)).Close();
            }

            using (StreamWriter w = File.AppendText(Path.Combine(HttpRuntime.AppDomainAppPath, LogFileName)))
            {
                w.WriteLineAsync(message);
            }
        }
    }
}