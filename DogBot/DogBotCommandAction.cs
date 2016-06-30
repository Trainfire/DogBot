using Core;
using SteamKit2;

namespace BotDogBot
{
    public abstract class DogBotCommandAction : CommandAction
    {
        public DogBot DogBot { get; private set; }

        public override void Initialize(Bot bot, SteamID caller, MessageParser parser)
        {
            // TODO: Would be nice if there was automated *somehow*.
            DogBot = bot as DogBot;
        }
    }
}
