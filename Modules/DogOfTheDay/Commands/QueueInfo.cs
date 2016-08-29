using SteamKit2;
using System.Collections.Generic;
using Core;
using System.Linq;

namespace Modules.DogOfTheDay
{
    class QueueInfo : DogOfTheDayCommand
    {
        public override string Execute(CommandSource source)
        {
            var users = DogOfTheDay.Data.Queue.GetUsers();
            var usersInfo = users.Select(x => GetInfo(x)).ToList();

            return string.Format("Users in queue: " + string.Join(", ", usersInfo));
        }

        string GetInfo(SteamID steamID)
        {
            string name = Bot.GetFriendName(steamID);
            int contributions = DogOfTheDay.Data.Queue.GetUserContributions(steamID).Count;
            return string.Format("{0} ({1})", name, contributions);
        }
    }
}
