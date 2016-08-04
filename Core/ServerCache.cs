using Newtonsoft.Json;
using SteamKit2.Internal;
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Core
{
    class ServerCache
    {
        readonly Logger logger;

        const string PATH = "servers.bin";

        public bool CacheExists { get; private set; }

        public ServerCache(Logger logger)
        {
            this.logger = logger;
        }

        public void Load()
        {
            if (File.Exists(PATH))
            {
                using (var fr = new StreamReader(PATH))
                {
                    var data = JsonConvert.DeserializeObject<ServerCacheData>(fr.ReadToEnd());

                    CMClient.Servers.Clear();
                    data.EndPoints.ForEach(x =>
                    {
                        CMClient.Servers.TryAdd(new IPEndPoint(x.Address, x.Port));
                    });

                    logger.Info("Loaded {0} servers from server list cache.", CMClient.Servers.GetAllEndPoints().Length);

                    CacheExists = true;
                }
            }
            else
            {
                logger.Error("Failed to find the server cache! Forcing a refresh...");
                Refresh();
            }
        }

        public void Refresh()
        {
            // since we don't have a list of servers saved, load the latest list of Steam servers
            // from the Steam Directory.
            var loadServersTask = SteamKit2.SteamDirectory.Initialize(0u);
            loadServersTask.Wait();

            if (loadServersTask.IsFaulted)
            {
                Console.WriteLine("Error loading server list from directory: {0}", loadServersTask.Exception.Message);
            }
            else
            {
                Console.WriteLine("Successfully loaded server list.");
            }

            Save();
        }

        public void Save()
        {
            var serverData = new ServerCacheData();
            foreach (var endPoint in CMClient.Servers.GetAllEndPoints())
            {
                serverData.EndPoints.Add(new ServerCacheData.EndPoint(endPoint.Address, endPoint.Port));
            }

            using (var fw = File.CreateText(PATH))
            {
                fw.Write(JsonConvert.SerializeObject(serverData, Formatting.Indented));
            }
        }
    }
}
