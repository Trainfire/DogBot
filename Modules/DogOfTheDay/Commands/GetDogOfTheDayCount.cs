using System;
using Core;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    class GetDogOfTheDayCount : DogOfTheDayCommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            return new CommandResult(DogOfTheDay.Strings.TotalMessages + DogBot.Data.QueueCount);
        }
    }
}
