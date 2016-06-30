using System;
using Core;
using SteamKit2;

namespace BotDogBot
{
    class GetDogOfTheDay : DogBotCommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            if (DogBot.Data.HasDog)
            {
                if (!string.IsNullOrEmpty(DogBot.Data.CurrentDog.Message))
                {
                    return new CommandResult(string.Format("{0} // {1} said: '{2}'", GetDoTDInfo(DogBot.Data.CurrentDog), bot.GetFriendName(DogBot.Data.CurrentDog.Setter), DogBot.Data.CurrentDog.Message));
                }
                else
                {
                    return new CommandResult(string.Format("{0} // Courtesy of {1}", GetDoTDInfo(DogBot.Data.CurrentDog), bot.GetFriendName(DogBot.Data.CurrentDog.Setter)));
                }
            }
            else
            {
                return new CommandResult(DogBot.Strings.UrlInvalid);
            }
        }

        string GetDoTDInfo(DogData dog)
        {
            return string.Format("{0}'s {1} // {2}", DateTime.Now.DayOfWeek.ToString(), DogBot, dog.URL);
        }
    }
}
