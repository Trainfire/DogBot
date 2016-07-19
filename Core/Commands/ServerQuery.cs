using System.Threading.Tasks;
using System.Collections.Generic;
using Steam.Query;
using Extensions.SteamQuery;
using System;

namespace Core
{
    abstract class ServerQuery : ChatCommand
    {
        public override bool IsAsync { get { return true; } }

        public abstract string IPAddress { get; }
        public abstract int Port { get; }
        public abstract string ServerName { get; }

        public override async Task<CommandResult> ExecuteAsync(CommandSource source)
        {
            var result = await new SteamQuery().GetServerInfo(IPAddress, Port);

            if (result == null)
            {
                return new CommandResult("Failed to query the server.");
            }
            else
            {
                return new CommandResult(Format(result, IPAddress, Port));
            }
        }

        string Format(ServerInfoResult info, string ipAddress, int port)
        {
            var str = new List<string>();
            str.Add(string.Format("{0}{1}:{2}", "steam://connect/", ipAddress, port));
            str.Add(string.Format("Players: {0} / {1}", info.Players, info.MaxPlayers));
            str.Add("Map: " + info.GetMapInfo());
            return ServerName + " // " + string.Join(" // ", str);
        }
    }

    // Really dumb implementation...
    // Ideally, these properties would be exposed in ServerQuery.
    // But the current implementation of commands prevents this due to the way commands are registered.
    class EUServerQuery : ServerQuery
    {
        public override string ServerName { get { return "EU Server"; } }
        public override string IPAddress { get { return "91.121.155.109"; } }
        public override int Port { get { return 27015; } }
    }

    class USServerQuery : ServerQuery
    {
        public override string ServerName { get { return "US Server"; } }
        public override string IPAddress { get { return "70.42.74.31"; } }
        public override int Port { get { return 27015; } }
    }
}
