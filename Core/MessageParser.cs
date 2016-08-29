using System;
using System.Linq;
using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// Parses a message into a command and arguments.
    /// </summary>
    public class MessageParser
    {
        public string OriginalMessage { get; private set; }
        public string Command { get; private set; }
        public List<string> Args { get; private set; }
        public bool IsValid { get; private set; }
        public string WhyInvalid { get; private set; }

        public MessageParser(string message)
        {
            OriginalMessage = message;
            IsValid = true;
            Args = new List<string>();

            // Remove trailing whitespace
            message = message.Trim();

            // Split input using space as delimiter
            var args = message.Split(' ').ToList();

            Command = args[0];

            for (int i = 1; i < args.Count; i++)
            {
                Args.Add(args[i]);
            }
        }

        protected void Invalidate(string reason)
        {
            IsValid = false;
            WhyInvalid = reason;
        }

        public virtual void OnParse() { }
    }
}
