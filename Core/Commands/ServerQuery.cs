using System.Threading.Tasks;
using System.Collections.Generic;
using Steam.Query;
using Extensions.SteamQuery;
using System;
using System.Net;
using System.Net.Sockets;

namespace Core
{
    class ServerQuery : ChatCommand
    {
        public override bool IsAsync { get { return true; } }

        public string Hostname { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string ServerName { get; set; }

        public override async Task<CommandResult> ExecuteAsync(CommandSource source)
        {
            // Lazy hack, but w/e.
            if (!string.IsNullOrEmpty(Hostname))
            {
                IPAddress = GetIPFromHostname(Hostname).AddressList[0].ToString();
            }

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

        IPHostEntry GetIPFromHostname(string hostname)
        {
            return Dns.GetHostEntry(hostname);
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
}
