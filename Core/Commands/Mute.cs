using System;
using SteamKit2;

namespace Core
{
    class Mute : CommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            bot.Mute();
            return new CommandResult();
        }
    }
}
