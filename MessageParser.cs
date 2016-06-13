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
        public string Command { get; private set; }
        public List<string> Args { get; private set; }
        public bool IsValid { get; private set; }

        // TODO: Move this into a config file.
        

        public MessageParser(string message)
        {
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
                IsValid = false;
                return;
            }

            // Process arguments
            for (int i = 0; i < args.Count; i++)
            {
                Args.Add(args[i]);
            }
        }

        public virtual void OnParse() { }
    }
}
