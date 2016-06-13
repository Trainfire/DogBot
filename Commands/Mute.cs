﻿using System;
using SteamKit2;

namespace DogBot
{
    class Mute : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            bot.Mute();
            return new CommandResult();
        }
    }
}
