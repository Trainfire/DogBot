using System;
using SteamKit2;

namespace DogBot
{
    class SubmitDogOfTheDay : SetDogOfTheDay
    {
        protected override CommandResult OnCreateDog(DogBot bot, DogData dog)
        {
            bot.Data.AddDog(dog);
            return new CommandResult(Strings.SetDogOfTheDay + dog.URL);
        }
    }
}
