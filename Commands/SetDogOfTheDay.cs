using System.Linq;
using SteamKit2;

namespace DogBot
{
    public class SetDogOfTheDay : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            var dotdParser = new DotdSetParser(caller, parser.Message);

            if (dotdParser.IsValid)
            {
                if (dotdParser.Dog != null)
                    bot.Data.SetDog(dotdParser.Dog);
                return new CommandResult(Strings.SetDogOfTheDay + dotdParser.Dog.URL);
            }
            else
            {
                return new CommandResult(dotdParser.WhyInvalid);
            }
        }

        protected virtual CommandResult OnCreateDog(DogBot bot, DogData dog)
        {
            bot.Data.SetDog(dog);
            return new CommandResult(Strings.SetDogOfTheDay + dog.URL);
        }
    }
}
