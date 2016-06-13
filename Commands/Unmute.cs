using System;
using SteamKit2;

namespace DogBot
{
    class Unmute : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            bot.Unmute();
            return new CommandResult();
        }
    }
}
