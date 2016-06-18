using System;
using SteamKit2;

namespace DogBot
{
    class SubmitDogOfTheDay : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            var dotdParser = new DotdSetParser(caller, parser.Message);

            if (dotdParser.IsValid)
            {
                if (dotdParser.Dog != null)
                    bot.Data.EnqueueDog(dotdParser.Dog);
                return new CommandResult(Strings.SubmitDogOfTheDay);
            }
            else
            {
                return new CommandResult(dotdParser.WhyInvalid);
            }
        }
    }
}
