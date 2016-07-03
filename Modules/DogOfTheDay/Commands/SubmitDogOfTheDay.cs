using System;
using Core;
using SteamKit2;
using Modules.CommandHandler;

namespace Modules.DogOfTheDay
{
    class SubmitDogOfTheDay : DogOfTheDayCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            var dotdParser = new DotdSetParser(source.Caller, source.Parser.Token, source.Parser.OriginalMessage);

            if (dotdParser.IsValid)
            {
                if (dotdParser.Dog != null)
                    DogOfTheDay.Data.EnqueueDog(dotdParser.Dog);
                return new CommandResult(DogOfTheDay.Strings.SubmitDogOfTheDay);
            }
            else
            {
                return new CommandResult(dotdParser.WhyInvalid);
            }
        }
    }
}
