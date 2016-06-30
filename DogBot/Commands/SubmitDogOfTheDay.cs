using System;
using Core;
using SteamKit2;

namespace BotDogBot
{
    class SubmitDogOfTheDay : DogBotCommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            var dotdParser = new DotdSetParser(caller, bot.Token, parser.Message);

            if (dotdParser.IsValid)
            {
                if (dotdParser.Dog != null)
                    DogBot.Data.EnqueueDog(dotdParser.Dog);
                return new CommandResult(DogBot.Strings.SubmitDogOfTheDay);
            }
            else
            {
                return new CommandResult(dotdParser.WhyInvalid);
            }
        }
    }
}
