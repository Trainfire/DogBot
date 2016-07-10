using System.Threading.Tasks;
using System.Collections.Generic;
using Core;
using Steam.Query;

namespace Modules.CommandHandler
{
    class SetName : ChatCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override CommandResult Execute(CommandSource source)
        {
            if (source.Parser.Args.Count > 0)
                Bot.Friends.SetPersonaName(source.Parser.Args[0]);

            return new CommandResult();
        }
    }
}
