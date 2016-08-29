using Core;
using Extensions.SteamQuery;
using Steam.Query;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Modules.Server
{
    class Server : Module
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            CommandListener.AddCommand<ServerQuery>("!~euserver", (command) =>
            {
                command.ServerName = "EU Server";
                command.Hostname = "geit.uk";
                command.Port = 27015;
            });

            CommandListener.AddCommand<ServerQuery>("!~usserver", (command) =>
            {
                command.ServerName = "US Server";
                command.IPAddress = "70.42.74.31";
                command.Port = 27015;
            });
        }
    }

    class ServerQuery : ChatCommand
    {
        public override bool IsAsync { get { return true; } }

        public string Hostname { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string ServerName { get; set; }

        public override async Task<string> ExecuteAsync(CommandSource source)
        {
            // Lazy hack, but w/e.
            if (!string.IsNullOrEmpty(Hostname))
            {
                IPAddress = GetIPFromHostname(Hostname).AddressList[0].ToString();
            }

            var result = await new SteamQuery().GetServerInfo(IPAddress, Port);

            if (result == null)
            {
                return "Failed to query the server.";
            }
            else
            {
                return Format(result, IPAddress, Port);
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
