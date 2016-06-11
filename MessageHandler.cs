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
        /// <summary>
        /// The result of the processed message. If a command is found and executed, this string may have a value. Otherwise it will be null.
        /// </summary>
        public string Result { get; private set; }

        public MessageHandler(DogBot bot, SteamFriends.ChatMsgCallback callback)
        {
            var parser = new MessageParser(callback.Message);

            if (parser.IsValid)
            {
                // Find a matching command using the parsed alias   
                var command = CommandRegistry.Commands.FirstOrDefault(x => x.Alias == parser.Command);

                if (command != null)
                {
                    if (command.AdminOnly)
                    {
                        //if (IsOfficerOrModerator(callback.ChatterID))
                        //{
                            Result = command.Execute(bot, callback.ChatterID, callback.Message);
                        //}
                        //else
                        //{
                            //Result = Strings.NoPermission;
                        //}
                    }
                    else
                    {
                        Result = command.Execute(bot, callback.ChatterID, callback.Message);
                    }
                }
            }
        }

        bool IsOfficerOrModerator(SteamID id)
        {
            // TODO: Move to JSON
            var users = new List<string>();
            return users.Contains(id.ToString());
        }
    }
}
