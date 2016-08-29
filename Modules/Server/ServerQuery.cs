using Core;
using Extensions.SteamQuery;
using Steam.Query;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Modules.Server
{
    class ServerInfo
    {
        public string Alias { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    class ServerList
    {
        public List<ServerInfo> Servers { get; set; }
    }

    class Config : FileStorage<ServerList>
    {

    }

    class Server : Module
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            // Get Servers.
            var config = new Config();

            if (config.Data.Servers == null)
            {
                Logger.Warning("No Servers found in config...");
            }
            else
            {
                foreach (var server in config.Data.Servers)
                {
                    CommandListener.AddCommand<ServerQuery>(server.Alias, (command) =>
                    {
                        command.ServerName = server.Name;
                        command.Hostname = server.Host;
                        command.Port = server.Port;
                    });
                }
            }
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
