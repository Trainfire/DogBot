using System;
using Core;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    class GetDogOfTheDay : DogOfTheDayCommand
    {
        public override string Execute(CommandSource source)
        {
            if (DogOfTheDay.Data.HasDog)
            {
                if (!string.IsNullOrEmpty(DogOfTheDay.Data.CurrentDog.Message))
                {
                    return string.Format("{0} // {1} said: '{2}'", GetDoTDInfo(DogOfTheDay.Data.CurrentDog), Bot.Names.GetFriendName(DogOfTheDay.Data.CurrentDog.Setter), DogOfTheDay.Data.CurrentDog.Message);
                }
                else
                {
                    return string.Format("{0} // Courtesy of {1}", GetDoTDInfo(DogOfTheDay.Data.CurrentDog), Bot.Names.GetFriendName(DogOfTheDay.Data.CurrentDog.Setter));
                }
            }
            else
            {
                return DogOfTheDay.Strings.NoDog;
            }
        }

        string GetDoTDInfo(DogData dog)
        {
            if (DogOfTheDay.AnnouncementMode == AnnouncementMode.Hourly)
            {
                return string.Format("{0} // {1}", DogOfTheDay.Strings.DogOfTheHour, dog.URL);
            }
            else
            {
                return string.Format("{0}'s {1} // {2}", DateTime.Now.DayOfWeek.ToString(), DogOfTheDay.Strings.DogOfTheDay, dog.URL);
            }
        }
    }
}
