using Core;
using Steam.Query;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Modules.SteamQuery
{
    class SteamQuery : Module
    {
        public async Task<ServerInfoResult> GetServerInfo(string ipAddress, int port)
        {
            IPAddress ip = null;
            IPAddress.TryParse(ipAddress, out ip);

            if (ip == null)
            {
                Logger.Error("Failed to parse IP '{0}'", ipAddress);
                return null;
            }

            var endPoint = new IPEndPoint(ip, port);

            var result = await TestConnection(endPoint);

            if (result != null)
            {
                Logger.Error("Failed to connect to IP '{0}'. Reason: {1}", ipAddress, result.Message);
                return null;
            }

            var server = new Server(endPoint);
            var info = await server.GetServerInfo();

            var data = new List<string>();
            data.Add(string.Format("Players: {0} / {1}", info.Players, info.MaxPlayers));
            data.Add(string.Format("Map: {0}", info.Map));

            Logger.Info(string.Join(" // ", data));

            return info;
        }

        async Task<Exception> TestConnection(IPEndPoint endPoint)
        {
            var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await Task.Run(() => s.Connect(endPoint));
            }

            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }
    }
}
