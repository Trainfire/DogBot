using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace DogBot
{
    class ShowRepo : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            return new CommandResult(Strings.Repo);
        }
    }
}
