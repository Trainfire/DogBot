using System.Linq;
using SteamKit2;

namespace DogBot
{
    public class SetDogOfTheDay : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            // Arg 0 is URL, Arg 1 is an optional Message.
            // TODO: Probably want to make this more obvious somehow...
            if (parser.Args.Count > 0 && IsURL(parser.Args[0]))
            {
                var dog = new DogData();

                dog.Setter = caller;
                dog.URL = parser.Args[0];

                // Any other arguments are considered to be a comment.
                if (parser.Args.Count > 0)
                {
                    var comment = parser.Args.Skip(1).ToList();
                    dog.Message = string.Join(" ", comment);
                }

                return OnCreateDog(bot, dog);
            }
            else
            {
                return new CommandResult(Strings.UrlInvalid);
            }
        }

        protected virtual CommandResult OnCreateDog(DogBot bot, DogData dog)
        {
            bot.Data.SetDog(dog);
            return new CommandResult(Strings.SetDogOfTheDay + dog.URL);
        }

        bool IsURL(string message)
        {
            // TODO: Rubbish validation
            return message.StartsWith("http://") || message.StartsWith("https://") || message.StartsWith("www.");
        }
    }
}
