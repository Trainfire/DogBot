using System.Collections.Generic;
using System.Linq;
using SteamKit2;

namespace DogBot
{
    /// <summary>
    /// Attempts to parse and execute a command based on the content of a message. The command may return a string.
    /// </summary>
    class MessageHandler
    {
        readonly MessageParser parser;

        /// <summary>
        /// The result of the processed message. If a command is found and executed, this string may have a value. Otherwise it will be null.
        /// </summary>
        public CommandRecord Record { get; private set; }

        public MessageHandler(DogBot bot, SteamFriends.ChatMsgCallback callback)
        {
            parser = new MessageParser(callback.Message);
            Parse(bot, callback.ChatterID, callback.Message);
        }

        public MessageHandler(DogBot bot, SteamID sid, string message)
        {
            parser = new MessageParser(message);
            Parse(bot, sid, message);
        }

        void Parse(DogBot bot, SteamID caller, string message)
        {
            if (parser.IsValid)
            {
                // Find a matching command using the parsed alias   
                var command = CommandRegistry.Commands.FirstOrDefault(x => x.Alias == parser.Command);

                if (command != null)
                {
                    if (command.UsersOnly)
                    {
                        if (bot.IsUser(caller))
                        {
                            Record = command.Execute(bot, caller, parser);
                        }
                        else
                        {
                            Record = new CommandRecord(null, caller, new CommandResult(Strings.NoPermission), parser);
                        }
                    }
                    else
                    {
                        Record = command.Execute(bot, caller, parser);
                    }
                }
            }
        }
    }
}
