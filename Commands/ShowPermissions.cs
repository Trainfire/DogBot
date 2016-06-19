using System;
using SteamKit2;

namespace DogBot
{
    class ShowPermissions : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            string hasAdmin = bot.IsAdmin(caller) ? "Aye" : "Nay";
            return new CommandResult(string.Format("Does {0} have admin? {1}", bot.GetFriendName(caller), hasAdmin));
        }
    }
}
