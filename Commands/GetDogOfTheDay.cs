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
        public override string Execute(DogBot bot, SteamID caller, string message)
        {
            if (!string.IsNullOrEmpty(bot.Data.Dog.URL))
            {
                if (!string.IsNullOrEmpty(bot.Data.Dog.Message))
                {
                    return string.Format("{0} // {1} // {2} said: '{3}'", Strings.DogOfTheDay, bot.Data.Dog.URL, bot.GetFriendName(bot.Data.Dog.Setter), bot.Data.Dog.Message);
                }
                else
                {
                    return string.Format("{0} // {1}", Strings.DogOfTheDay, bot.Data.Dog.URL);
                }
            }
            else
            {
                return Strings.NoDog;
            }
        }
    }
}
