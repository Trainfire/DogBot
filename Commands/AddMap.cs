using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace DogBot
{
    class AddMap : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            var mapParser = new AddMapParser(caller, parser.Message);

            if (mapParser.IsValid)
            {
                bot.Data.Maps.Add(mapParser.Map);
                return new CommandResult(string.Format("Added map '{0}' to the queue.", mapParser.Map.MapName));
            }
            else
            {
                return new CommandResult(mapParser.WhyInvalid);
            }
        }
    }
}
