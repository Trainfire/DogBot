using System;
using Core;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    class MoveNext : DogOfTheDayCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override CommandResult Execute(CommandSource source)
        {
            DogOfTheDay.Data.MoveToNextDog();

            if (DogOfTheDay.Data.CurrentDog != null )
            {
                return new CommandResult(string.Format("Dog of the Day is now: {0}", DogOfTheDay.Data.CurrentDog.URL));
            }
            else
            {
                return new CommandResult(string.Format("Queue is now empty."));
            }
        }
    }
}
