using System;
using Core;
using SteamKit2;

namespace BotDogBot
{
    class GetDogOfTheDayCount : DogBotCommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            return new CommandResult(DogBot.Strings.TotalMessages + DogBot.Data.QueueCount);
        }
    }
}
