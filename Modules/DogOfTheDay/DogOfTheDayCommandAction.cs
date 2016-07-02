using Core;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    public abstract class DogOfTheDayCommandAction : CommandAction
    {
        public DogOfTheDay DogBot { get; private set; }

        public override void Initialize(Bot bot, SteamID caller, MessageParser parser)
        {
            // TODO: Would be nice if there was automated *somehow*.
            DogBot = bot.GetModule<DogOfTheDay>();
        }
    }
}
