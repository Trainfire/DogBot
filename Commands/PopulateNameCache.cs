using System;
using SteamKit2;

namespace DogBot
{
    class PopulateNameCache : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            bot.PopulateNameCache();
            return new CommandResult("Name cache repopulated!");
        }
    }
}
