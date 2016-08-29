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

        public override string Execute(CommandSource source)
        {
            DogOfTheDay.Data.Queue.Sort();
            return "Queue has been sorted.";
        }
    }
}
