using System;
using SteamKit2;

namespace DogBot
{
    class Unmute : CommandAction
    {
        public override string Execute(DogBot bot, SteamID caller, string message)
        {
            bot.Unmute();
            return "";
        }
    }
}
