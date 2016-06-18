using SteamKit2;
using System.Collections.Generic;

namespace DogBot
{
    class Stats : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            var stats = new List<string>();

            // Total records
            stats.Add(Strings.StatsTotalAdded + bot.Data.HistoryStats.TotalRecords);

            // Highest contributer
            var highestContributer = bot.Data.HistoryStats.HighestContributer;
            if (highestContributer != null)
            {
                var name = bot.GetFriendName(new SteamID(highestContributer.SteamID));
                stats.Add(string.Format("{0}{1} ({2})", Strings.StatsHighestContributer, name, highestContributer.Contributions));
            }

            // Include caller's contributions if they have any.
            var callerContributions = bot.Data.HistoryStats.GetUserContributions(caller);
            if (callerContributions.Count != 0)
            {
                var callerName = bot.GetFriendName(caller);
                stats.Add(string.Format("{0}'s total submissions: {1}", callerName, callerContributions.Count));
            }

            return new CommandResult(string.Join(" // ", stats.ToArray()));
        }
    }
}
