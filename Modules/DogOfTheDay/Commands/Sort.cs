using SteamKit2;
using System.Collections.Generic;
using Core;
using System.Threading.Tasks;
using System;

namespace Modules.DogOfTheDay
{
    class Sort : DogOfTheDayCommand
    {
        public override bool AdminOnly
        {
            get
            {
                return true;
            }
        }

        public override CommandResult Execute(CommandSource source)
        {
            DogOfTheDay.Data.Queue.Sort();
            return new CommandResult("Queue has been sorted.");
        }
    }
}
