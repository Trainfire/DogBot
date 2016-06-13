using System.Linq;
using SteamKit2;

namespace DogBot
{
    class Help : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            // Returns a list of each command and it's arguments if specified.
            return new CommandResult(Strings.Repo + " | " + Strings.Help + " " + string.Join(" // ", CommandRegistry.Commands.Select(x => x.Help).ToArray()));
        }
    }
}