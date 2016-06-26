using System;
using SteamKit2;

namespace DogBot
{
    class GetDogOfTheDayCount : CommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            return new CommandResult(Strings.TotalMessages + bot.Data.QueueCount);
        }
    }
}
