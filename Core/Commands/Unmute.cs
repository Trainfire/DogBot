using System;
using SteamKit2;
using Modules.Messenger;

namespace Core.Commands
{
    class Unmute : CommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            bot.GetModule<Messenger>().Muted = false;
            return new CommandResult();
        }
    }
}
