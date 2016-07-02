using System;
using SteamKit2;
using Modules.Messenger;

namespace Core.Commands
{
    class Mute : CommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            bot.GetModule<Messenger>().Muted = true;
            return new CommandResult();
        }
    }
}
