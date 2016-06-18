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
                    return new CommandResult(string.Format("{0} // {1} said: '{2}'", GetDoTDInfo(bot.Data.CurrentDog), bot.GetFriendName(bot.Data.CurrentDog.Setter), bot.Data.CurrentDog.Message));
                }
                else
                {
                    return new CommandResult(string.Format("{0} // Courtesy of {1}", GetDoTDInfo(bot.Data.CurrentDog), bot.GetFriendName(bot.Data.CurrentDog.Setter)));
                }
            }
            else
            {
                return new CommandResult(Strings.NoDog);
            }
        }

        string GetDoTDInfo(DogData dog)
        {
            return string.Format("{0}'s {1} // {2}", DateTime.Now.DayOfWeek.ToString(), Strings.DogOfTheDay, dog.URL);
        }
    }
}
