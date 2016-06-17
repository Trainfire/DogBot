using System;
using System.Linq;
using System.Collections.Generic;

namespace DogBot
{
    /// <summary>
    /// Parses a message into a command and arguments.
    /// </summary>
    public class MessageParser
    {
        public string Message { get; private set; }
        public string Command { get; private set; }
        public List<string> Args { get; private set; }
        public bool IsValid { get; private set; }
        public string WhyInvalid { get; private set; }

        public MessageParser(string message)
        {
            Message = message;
            IsValid = true;
            Args = new List<string>();

            // Remove trailing whitespace
            message = message.Trim();

            // Split input using space as delimiter
            var args = message.Split(' ').ToList();

            if (args[0].StartsWith(CommandRegistry.COMMAND_TOKEN) && args[0].Length > 1)
            {
                // Remove the initial "!" character token
                Command = args[0].Substring(1);
                args.RemoveAt(0);
            }
            else
            {
                Invalidate("Command does not start with '" + CommandRegistry.COMMAND_TOKEN + "' token");
                return;
            }

            // Process arguments
            for (int i = 0; i < args.Count; i++)
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
