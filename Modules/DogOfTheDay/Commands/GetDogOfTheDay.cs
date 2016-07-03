using System;
using Core;
using Modules.CommandHandler;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    class GetDogOfTheDay : ChatCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            var dotd = Bot.GetModule<DogOfTheDay>();

            if (dotd.Data.HasDog)
            {
                if (!string.IsNullOrEmpty(dotd.Data.CurrentDog.Message))
                {
                    return new CommandResult(string.Format("{0} // {1} said: '{2}'", GetDoTDInfo(dotd.Data.CurrentDog), Bot.GetFriendName(dotd.Data.CurrentDog.Setter), dotd.Data.CurrentDog.Message));
                }
                else
                {
                    return new CommandResult(string.Format("{0} // Courtesy of {1}", GetDoTDInfo(dotd.Data.CurrentDog), Bot.GetFriendName(dotd.Data.CurrentDog.Setter)));
                }
            }
            else
            {
                return new CommandResult(DogOfTheDay.Strings.UrlInvalid);
            }
        }

        string GetDoTDInfo(DogData dog)
        {
            return string.Format("{0}'s {1} // {2}", DateTime.Now.DayOfWeek.ToString(), DogOfTheDay.Strings.DogOfTheDay, dog.URL);
        }
    }
}
