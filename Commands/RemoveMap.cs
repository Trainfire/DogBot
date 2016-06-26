using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace DogBot
{
    class RemoveMap : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            if (parser.Args.Count > 0)
            {
                var map = bot.Data.Maps.Get(parser.Args[0]);

                if (map != null)
                {
                    bot.Data.Maps.Remove(map);
                    return new CommandResult(string.Format("Removed map '{0}' from the queue.", map.MapName));
                }
            }

            return new CommandResult(string.Format("Invalid arguments. Use {0} <MapName> to remove a map", CommandRegistry.REMOVEMAP));
        }
    }
}
