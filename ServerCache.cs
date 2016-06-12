using Newtonsoft.Json;
using SteamKit2.Internal;
using System;
using System.IO;
using System.Net;

namespace DogBot
{
    class ServerCache
    {
        const string PATH = "servers.bin";

        public bool CacheExists { get; private set; }

        public void Load()
        {
            if (File.Exists(PATH))
            {
                var file = File.ReadAllText(PATH);
                var data = JsonConvert.DeserializeObject<ServerCacheData>(file);

                CMClient.Servers.Clear();
                data.EndPoints.ForEach(x =>
                {
                    CMClient.Servers.TryAdd(new IPEndPoint(x.Address, x.Port));
                });

                Console.WriteLine("Loaded {0} servers from server list cache.", CMClient.Servers.GetAllEndPoints().Length);

                CacheExists = true;
            }
            else
            {
                // since we don't have a list of servers saved, load the latest list of Steam servers
                // from the Steam Directory.
                var loadServersTask = SteamKit2.SteamDirectory.Initialize(0u);
                loadServersTask.Wait();

                if (loadServersTask.IsFaulted)
                {
                    Console.WriteLine("Error loading server list from directory: {0}", loadServersTask.Exception.Message);
                }
            }
        }

        public void Save()
        {
            var serverData = new ServerCacheData();
            foreach (var endPoint in CMClient.Servers.GetAllEndPoints())
            {
                serverData.EndPoints.Add(new ServerCacheData.EndPoint(endPoint.Address, endPoint.Port));
            }
            File.WriteAllText(PATH, JsonConvert.SerializeObject(serverData, Formatting.Indented));
        }
    }
}
