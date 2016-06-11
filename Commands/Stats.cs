using SteamKit2;
using System.Collections.Generic;

namespace DogBot
{
    class Stats : CommandAction
    {
        public override string Execute(DogBot bot, SteamID caller, string message)
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

            return string.Join(" // ", stats.ToArray());
        }
    }
}
