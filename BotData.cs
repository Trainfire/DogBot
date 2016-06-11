using System;
using SteamKit2;

namespace DogBot
{
    /// <summary>
    /// Information about the current state of the Bot.
    /// </summary>
    public class BotData
    {
        public DogData Dog { get; private set; }

        public BotData(SteamFriends friends)
        {
            Dog = new DogData();
        }

        public void SetDog(DogData dog)
        {
            Dog = dog;
        }
    }
}
