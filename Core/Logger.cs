using System;
using System.IO;

namespace Core
{
    public class Logger
    {
        readonly string path;
        readonly string prefix;
        
        public string Path
        {
            get { return path; }
        }

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
            var str = Format(message, args);

            try
            {
                using (var fa = File.AppendText(path))
                {
                    fa.WriteLine(str);
                    fa.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Format(ex.Message));
            }

            Console.WriteLine(str);
        }

        public string Format(string message, params object[] args)
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

            return DateTime.Now.ToString("d/M/y h:mm:ss tt") + " | " + str;
        }
    }
}
