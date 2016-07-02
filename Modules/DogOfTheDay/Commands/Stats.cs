using SteamKit2;
using System.Collections.Generic;
using Core;

namespace Modules.DogOfTheDay
{
    class Stats : DogOfTheDayCommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            var stats = new List<string>();

            // Total records
            stats.Add(DogOfTheDay.Strings.StatsTotalAdded + DogBot.Data.TotalDogsAdded);

            // Dogs Shown
            stats.Add(DogOfTheDay.Strings.StatsTotalShown + DogBot.Data.TotalDogsShown);

            // Highest contributer
            var highestContributor = DogBot.Data.HighestContributor;
            if (highestContributor != null)
            {
                var name = bot.GetFriendName(highestContributor);
                stats.Add(string.Format("{0}{1} ({2})", DogOfTheDay.Strings.StatsHighestContributer, name, DogBot.Data.GetUserContributions(highestContributor).Count));
            }

            // Include caller's contributions if they have any.
            var callerContributions = DogBot.Data.GetUserContributions(caller);
            if (callerContributions.Count != 0)
            {
                var callerName = bot.GetFriendName(caller);
                stats.Add(string.Format("{0}'s total submissions: {1}", callerName, callerContributions.Count));
            }

            return new CommandResult(string.Join(" // ", stats.ToArray()));
        }
    }
}
