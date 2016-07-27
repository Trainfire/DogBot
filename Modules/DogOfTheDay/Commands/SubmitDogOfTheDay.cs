using System;
using Core;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    class SubmitDogOfTheDay : DogOfTheDayCommand
    {
        public override bool UsersOnly { get { return true; } }

        public override CommandResult Execute(CommandSource source)
        {
            var dotdParser = new DotdSetParser(source.Caller, source.Parser.Token, source.Parser.OriginalMessage);

            if (dotdParser.IsValid)
            {
                if (DogOfTheDay.Data.Queue.HasURL(dotdParser.Dog.URL))
                    return new CommandResult(DogOfTheDay.Strings.SubmitURLExists);

                DogOfTheDay.Data.Queue.Enqueue(dotdParser.Dog);
                return new CommandResult(DogOfTheDay.Strings.SubmitDogOfTheDay);
            }
            else
            {
                return new CommandResult(dotdParser.WhyInvalid);
            }
        }
    }
}
