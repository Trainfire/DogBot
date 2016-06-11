using System.Linq;
using SteamKit2;

namespace DogBot
{
    class Help : CommandAction
    {
        public override string Execute(DogBot bot, SteamID caller, string message)
        {
            // Returns a list of each command and it's arguments if specified.
            return Strings.Help + " " + string.Join(" // ", CommandRegistry.Commands.Select(x => x.Help).ToArray());
        }
    }
}