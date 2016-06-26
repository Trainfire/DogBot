using System.Collections.Generic;
using System.Linq;
using SteamKit2;

namespace DogBot
{
    public class Maps : FileLogger<MapsData>
    {
        public override string Filename
        {
            get
            {
                return "maps.json";
            }
        }

        public void Add(Map map)
        {
            Data.Maps.Add(map);
            Save();
        }

        public void Remove(string mapName)
        {
            var map = Data.Maps.FirstOrDefault(x => x.MapName == mapName);

            if (map != null)
                Remove(map);
        }

        public void Remove(Map map)
        {
            if (Data.Maps.Contains(map))
            {
                Data.Maps.Remove(map);
                Save();
            }
        }

        public int Count
        {
            get
            {
                return Data.Maps.Count;
            }
        }

        public List<string> MapList
        {
            get
            {
                return Data.Maps.Select(x => x.MapName).ToList();
            }
        }

        public List<string> URLS
        {
            get
            {
                return Data.Maps.Select(x => x.URL).ToList();
            }
        }

        public Map Get(string mapName)
        {
            return Data.Maps.FirstOrDefault(x => x.MapName == mapName);
        }
    }

    public class MapsData
    {
        public List<Map> Maps { get; set; }

        public MapsData()
        {
            Maps = new List<Map>();
        }
    }

    public class Map
    {
        public string MapName { get; set; }
        public SteamID Setter { get; set; }
        public string URL { get; set; }
    }
}
