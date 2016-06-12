using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace DogBot
{
    public class ServerCacheData
    {
        public class EndPoint
        {
            [JsonConverter(typeof(IPAddressConverter))]
            public IPAddress Address { get; set; }
            public int Port { get; set; }

            public EndPoint(IPAddress address, int port)
            {
                Address = address;
                Port = port;
            }
        }

        public List<EndPoint> EndPoints { get; set; };

        public ServerCacheData()
        {
            EndPoints = new List<EndPoint>();
        }
    }
}
