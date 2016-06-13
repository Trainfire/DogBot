﻿using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DogBot
{
    public static class Config
    {
        const string FILENAME = "configuration.json";

        public static ConfigData Load()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;

            if (File.Exists(path + FILENAME))
            {
                var file = File.ReadAllText(path + FILENAME);
                return JsonConvert.DeserializeObject<ConfigData>(file);
            }

            Console.WriteLine("[ERROR] Failed to load configuration file");

            var config = new ConfigData();

            var sw = File.CreateText(path + FILENAME);
            sw.Write(JsonConvert.SerializeObject(config, Formatting.Indented));
            sw.Close();

            return config;
        }
    }

    public class ConfigData
    {
        public string User { get; set; }
        public string Pass { get; set; }
        public string SteamName { get; set; }
        public string ChatRoomId { get; set; }

        /// <summary>
        /// How often to make an announcement in seconds.
        /// </summary>
        public double AnnouncementInterval { get; set; }

        /// <summary>
        /// How long to wait for no activity before automatically rejoining chat.
        /// </summary>
        public double RejoinInterval { get; set; }

        public List<string> Admins { get; set; }
    }
}
