using System.Threading.Tasks;
using System.Collections.Generic;
using Core;
using Steam.Query;

namespace Modules.CommandHandler
{
    class ServerQuery : ChatCommand
    {
        public override bool AdminOnly { get { return true; } }
        public override bool IsAsync { get { return true; } }

        public override async Task<CommandResult> ExecuteAsync(CommandSource source)
        {
            var sQuery = Bot.GetOrAddModule<SteamQuery.SteamQuery>();

            // DEBUG
            string ipAddress = "91.121.155.109";
            int port = 27015;
            var result = await sQuery.GetServerInfo(ipAddress, port);

            if (result == null)
            {
                return new CommandResult("Failed to query the server.");
            }
            else
            {
                return new CommandResult(Format(result, ipAddress, port));
            }
        }

        string Format(ServerInfoResult info, string ipAddress, int port)
        {
            var str = new List<string>();
            str.Add(string.Format("{0}{1}:{2}", "steam://connect/", ipAddress, port));
            str.Add(string.Format("Players: {0} / {1}", info.Players, info.MaxPlayers));
            str.Add("Map: " + info.GetMapInfo());
            return "EU Server // " + string.Join(" // ", str);
        }
    }
}
