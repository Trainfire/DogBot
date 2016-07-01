using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core
{
    public class Config : FileStorage<ConfigData>
    {
        const string FILENAME = "configuration.json";

        public override string Filename
        {
            get
            {
                return "config.json";
            }
        }
    }

    public class ConfigData
    {
        public Connection.ConnectionInfo ConnectionInfo { get; set; }

        public string Token { get; set; }
        public string CommandPrefix { get; set; }

        /// <summary>
        /// Which chat room to connect to.
        /// </summary>
        public string ChatRoomId { get; set; }

        /// <summary>
        /// How long to wait for no activity before automatically rejoining chat.
        /// </summary>
        public double RejoinInterval { get; set; }

        public List<string> Admins { get; set; }
        public List<string> Users { get; set; }

        public ConfigData()
        {
            ConnectionInfo = new Connection.ConnectionInfo();
            Admins = new List<string>();
            Users = new List<string>();
            RejoinInterval = 300;
            Token = "";
            CommandPrefix = "";
        }
    }
}
