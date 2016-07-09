using Steam.Query;

namespace Steam.Query
{
    public static class SteamQueryEx
    {
        public static string GetMapInfo(this ServerInfoResult info)
        {
            var mapName = info.Map;

            if (mapName.Contains("workshop/"))
                mapName = mapName.Replace("workshop/", "");

            if (mapName.Contains("."))
                mapName = mapName.Remove(mapName.IndexOf('.'));

            return mapName;
        }

        public static bool IsWorkshopMap(this ServerInfoResult info)
        {
            return info.Map != null && info.Map.Contains("workshop");
        }
    }
}
