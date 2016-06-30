using System;
using SteamKit2;

namespace Core
{
    class Unmute : CommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            bot.Unmute();
            return new CommandResult();
        }
    }
}
