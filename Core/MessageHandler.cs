using System.Collections.Generic;
using System.Linq;
using SteamKit2;

namespace Core
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

        public MessageHandler(Bot bot, SteamFriends.ChatMsgCallback callback)
        {
            parser = new MessageParser(bot.Token, callback.Message);
            Parse(bot, callback.ChatterID, callback.Message);
        }

        public MessageHandler(Bot bot, SteamID sid, string message)
        {
            parser = new MessageParser(bot.Token, message);
            Parse(bot, sid, message);
        }

        void Parse(Bot bot, SteamID caller, string message)
        {
            if (parser.IsValid)
            {
                // Find a matching command using the parsed alias   
                var command = bot.Commands.FirstOrDefault(x => x.Alias == parser.Command);

                if (command != null)
                {
                    // Cache the name of the caller.
                    bot.CacheName(caller);

                    if (command.UsersOnly || command.AdminOnly)
                    {
                        if (command.UsersOnly && bot.IsUser(caller) || command.AdminOnly && bot.IsAdmin(caller))
                        {
                            Record = command.Execute(bot, caller, parser);
                        }
                        else
                        {
                            Record = new CommandRecord(null, caller, new CommandResult(bot.CoreStrings.NoPermission), parser);
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