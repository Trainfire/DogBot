using System;
using SteamKit2;

namespace Core
{
    class PopulateNameCache : CommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            bot.PopulateNameCache();
            return new CommandResult("Name cache repopulated!");
        }
    }
}
