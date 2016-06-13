using System;
using SteamKit2;

namespace DogBot
{
    class GetRandomDog : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            var rnd = new Random().Next(0, bot.Data.HistoryStats.Dogs.Count);
            var dog = bot.Data.HistoryStats.Dogs[rnd];
            return new CommandResult(dog.URL);
        }
    }
}
