using System;
using Core;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    class SubmitDogOfTheDay : DogOfTheDayCommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            var dotdParser = new DotdSetParser(caller, bot.Token, parser.Message);

            if (dotdParser.IsValid)
            {
                if (dotdParser.Dog != null)
                    DogBot.Data.EnqueueDog(dotdParser.Dog);
                return new CommandResult(DogOfTheDay.Strings.SubmitDogOfTheDay);
            }
            else
            {
                return new CommandResult(dotdParser.WhyInvalid);
            }
        }
    }
}
