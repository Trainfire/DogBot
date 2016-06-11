using System;
using SteamKit2;

namespace DogBot
{
    class Mute : CommandAction
    {
        public override string Execute(DogBot bot, SteamID caller, string message)
        {
            bot.Mute();
            return "";
        }
    }
}
