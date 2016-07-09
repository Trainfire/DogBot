using System;
using Core;
using Modules.CommandHandler;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    class GetDogOfTheDay : DogOfTheDayCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            if (DogOfTheDay.Data.HasDog)
            {
                if (!string.IsNullOrEmpty(DogOfTheDay.Data.CurrentDog.Message))
                {
                    return new CommandResult(string.Format("{0} // {1} said: '{2}'", GetDoTDInfo(DogOfTheDay.Data.CurrentDog), Bot.GetFriendName(DogOfTheDay.Data.CurrentDog.Setter), DogOfTheDay.Data.CurrentDog.Message));
                }
                else
                {
                    return new CommandResult(string.Format("{0} // Courtesy of {1}", GetDoTDInfo(DogOfTheDay.Data.CurrentDog), Bot.GetFriendName(DogOfTheDay.Data.CurrentDog.Setter)));
                }
            }
            else
            {
                return new CommandResult(DogOfTheDay.Strings.NoDog);
            }
        }

        string GetDoTDInfo(DogData dog)
        {
            return string.Format("{0}'s {1} // {2}", DateTime.Now.DayOfWeek.ToString(), DogOfTheDay.Strings.DogOfTheDay, dog.URL);
        }
    }
}
