using System.Linq;
using SteamKit2;

namespace DogBot
{
    public class SetDogOfTheDay : CommandAction
    {
        public DogData Dog { get; private set; }

        public override string Execute(BotData botData, SteamID caller, string message)
        {
            Dog = new DogData(null);

            Dog.Setter = caller;

            var parser = new MessageParser(message);

            // Arg 0 is URL, Arg 1 is an optional Message.
            // TODO: Probably want to make this more obvious somehow...
            if (parser.Args.Count > 0 && IsURL(parser.Args[0]))
            {
                Dog.Setter = caller;
                Dog.URL = parser.Args[0];

                if (parser.Args.Count > 0)
                {
                    var comment = parser.Args.Skip(1).ToList();
                    Dog.Message = string.Join(" ", comment);
                }

                return Strings.SetDogOfTheDay + Dog.Setter;
            }
            else
            {
                return Strings.UrlInvalid;
            }
        }

        bool IsURL(string message)
        {
            // TODO: Rubbish validation
            return message.StartsWith("http://") || message.StartsWith("https://") || message.StartsWith("www.");
        }
    }
}
