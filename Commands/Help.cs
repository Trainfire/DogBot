using System.Linq;
using SteamKit2;

namespace DogBot
{
    class Help : CommandAction
    {
        public override string Execute(DogBot bot, SteamID caller, string message)
        {
            return Strings.Help + " " + string.Join(" // ", CommandRegistry.Commands.Select(x => x.Help).ToArray());
        }
    }
}
