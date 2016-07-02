using System;
using SteamKit2;

namespace Core.Commands
{
    class ShowPermissions : CommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            string hasAdmin = bot.IsUser(caller) ? "Aye" : "Nay";
            return new CommandResult(string.Format("Does {0} have admin? {1}", bot.GetFriendName(caller), hasAdmin));
        }
    }
}
