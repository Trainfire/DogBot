using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DogBot
{
    class Logger
    {
        readonly string path;
        readonly string prefix;

        public Logger(string path, string prefix = "")
        {
            this.path = path;
            this.prefix = prefix;
        }

        public void Info(string message, params object[] args)
        {
            Log(message, args);
        }

        public void Warning(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log("[WARNING] " + message, args);
            Console.ResetColor();
        }

        public void Error(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log("[ERROR] " + message, args);
            Console.ResetColor();
        }

        void Log(string message, params object[] args)
        {
            string str = "";
            if (!string.IsNullOrEmpty(prefix))
            {
                str = prefix + " | " + string.Format(message, args);
            }
            else
            {
                str = string.Format(message, args);
            }

            str = DateTime.Now.ToString("d/M/y h:mm:ss tt") + " | " + str;
            Console.WriteLine(str);

            using (var fa = File.AppendText(path))
            {
                fa.WriteLine(str);
                fa.Close();
            }
        }
    }
}
