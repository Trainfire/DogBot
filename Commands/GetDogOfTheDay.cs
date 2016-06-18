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
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            if (bot.Data.HasDog)
            {
                if (!string.IsNullOrEmpty(bot.Data.CurrentDog.Message))
                {
                    return new CommandResult(string.Format("{0} // {1} // {2} said: '{3}'", Strings.DogOfTheDay, bot.Data.CurrentDog.URL, bot.GetFriendName(bot.Data.CurrentDog.Setter), bot.Data.CurrentDog.Message));
                }
                else
                {
                    return new CommandResult(string.Format("{0} // {1} // Set by {2}", Strings.DogOfTheDay, bot.Data.CurrentDog.URL, bot.GetFriendName(bot.Data.CurrentDog.Setter)));
                }
            }
            else
            {
                return new CommandResult(Strings.NoDog);
            }
        }
    }
}
