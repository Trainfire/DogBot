using System;
using Core;
using SteamKit2;

namespace BotDogBot
{
    class GetRandomDog : DogBotCommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            var rnd = new Random().Next(0, DogBot.Data.HistoryStats.Dogs.Count);
            var dog = DogBot.Data.HistoryStats.Dogs[rnd];
            return new CommandResult(dog.URL);
        }
    }
}
