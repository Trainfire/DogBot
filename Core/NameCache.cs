using System.Collections.Generic;
using SteamKit2;

namespace Core
{
    public class Names
    {
        readonly SteamFriends friends;
        readonly NameCache storage;

        public Names(SteamFriends friends, NameCache storage)
        {
            this.friends = friends;
            this.storage = storage;
        }

        public string GetFriendName(string steamID3)
        {
            return GetFriendName(new SteamID(steamID3));
        }

        public string GetFriendName(SteamID id)
        {
            Cache(id);
            return storage.Retrieve(id);
        }

        public void Cache(SteamID id)
        {
            var name = friends.GetFriendPersonaName(id);

            if (name != "[unknown]")
                storage.Store(id, name);
        }

        public void Cache(List<SteamID> names)
        {
            names.ForEach(x => Cache(x));
        }
    }

    public class NameCache : FileStorage<NameCacheData>
    {
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
