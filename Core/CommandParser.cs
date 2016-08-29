using System;
using Core;

namespace Core
{
    /// <summary>
    /// Process a chat command and relays it to the caller.
    /// </summary>
    public class CommandParser
    {
        readonly Bot Bot;

        public string NoPermissionMessage { get; set; }
        public bool Muted { get; set; }

        public CommandParser(Bot bot)
        {
            this.Bot = bot;
        }

        public async void ProcessCommand(CommandEvent commandEvent)
        {
            if (!commandEvent.Source.HadPermission)
            {
                if (string.IsNullOrEmpty(NoPermissionMessage))
                    return;

                if (commandEvent.Source.Context == MessageContext.Chat)
                {
                    Bot.Connection.SayToChat(Bot.CurrentChatRoomID, NoPermissionMessage);
                }
                else
                {
                    Bot.Connection.SayToFriend(commandEvent.Source.Caller, NoPermissionMessage);
                }
            }
            else
            {
                string result = null;
                if (commandEvent.Command.IsAsync)
                {
                    result = await commandEvent.Command.ExecuteAsync(commandEvent.Source);
                }
                else
                {
                    result = commandEvent.Command.Execute(commandEvent.Source);
                }

                if (!string.IsNullOrEmpty(result) && !Muted)
                {
                    if (commandEvent.Source.Context == MessageContext.Chat)
                    {
                        Bot.Connection.SayToChat(Bot.CurrentChatRoomID, result);
                    }
                    else
                    {
                        Bot.Connection.SayToFriend(commandEvent.Source.Caller, result);
                    }
                }
            }
        }
    }
}
