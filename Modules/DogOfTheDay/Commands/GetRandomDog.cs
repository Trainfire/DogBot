using System;
using Core;
using Modules.CommandHandler;

namespace Modules.DogOfTheDay
{
    class GetRandomDog : DogOfTheDayCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            var rnd = new Random().Next(0, DogOfTheDay.Data.HistoryStats.Dogs.Count);
            var dog = DogOfTheDay.Data.HistoryStats.Dogs[rnd];
            return new CommandResult(dog.URL);
        }
    }
}
