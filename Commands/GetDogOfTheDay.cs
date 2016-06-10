using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace DogBot
{
    class GetDogOfTheDay : CommandAction
    {
        public override string Execute(BotData botData, SteamID caller, string message)
        {
            return "Get dog of the day here! *woof*";
        }
    }
}
