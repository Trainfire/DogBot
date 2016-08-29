using System;
using Core;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    class GetDogOfTheDayCount : DogOfTheDayCommand
    {
        public override string Execute(CommandSource source)
        {
            return DogOfTheDay.Strings.TotalMessages + DogOfTheDay.Data.Queue.Count;
        }
    }
}
