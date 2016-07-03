using System;
using Core;
using SteamKit2;
using Modules.CommandHandler;

namespace Modules.DogOfTheDay
{
    class GetDogOfTheDayCount : DogOfTheDayCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            return new CommandResult(DogOfTheDay.Strings.TotalMessages + DogOfTheDay.Data.QueueCount);
        }
    }
}
