using System.Collections.Generic;
using SteamKit2;

namespace DogBot
{
    public class NameCache : FileLogger<NameCacheData>
    {
        public override string Filename
        {
            get
            {
                return "names.json";
            }
        }

        public void Store(SteamID steamID, string name)
        {
            var account = steamID.ToString();

            if (!Data.Cache.ContainsKey(account))
            {
                Data.Cache.Add(account, name);
            }
            else
            {
                Data.Cache[account] = name;
            }
            Save();
        }

        public string Retrieve(SteamID steamID)
        {
            var account = steamID.ToString();

            if (Data.Cache.ContainsKey(account))
                return Data.Cache[account];
            return "";
        }
    }

    public class NameCacheData
    {
        public Dictionary<string, string> Cache { get; set; }

        public NameCacheData()
        {
            Cache = new Dictionary<string, string>();
        }
    }
}
