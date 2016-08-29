using System;
using System.Collections.Generic;
using System.Linq;
using SteamKit2;
using Core;

namespace Modules.MapModule
{
    class Map
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public SteamID Setter { get; set; }
    }

    class MapListData
    {
        public List<Map> Maps { get; set; }

        public MapListData()
        {
            if (Maps == null)
                Maps = new List<Map>();
        }
    }

    class MapList : FileStorage<MapListData>
    {
        public enum AddResult
        {
            Okay,
            MapExists,
        }

        public enum RemoveResult
        {
            Okay,
            MapDoesNotExist,
            NoPermission,
        }

        public enum UpdateResult
        {
            Okay,
            MapDoesNotExist,
            NoPermission,
        }

        private MapModule mapModule;

        /// <summary>
        /// Returns a copy of the map list.
        /// </summary>
        public List<Map> Maps
        {
            get { return Data.Maps.ToList(); }
        }

        public MapList(MapModule mapModule)
        {
            this.mapModule = mapModule;
        }

        public AddResult Add(Map map)
        {
            if (Data.Maps.Any(x => x.Name.ToLower() == map.Name.ToLower()))
            {
                return AddResult.MapExists;
            }
            else
            {
                Data.Maps.Add(map);
                Save();
                return AddResult.Okay;
            }
        }

        public RemoveResult Remove(SteamID caller, string mapName)
        {
            var map = Data.Maps.FirstOrDefault(x => x.Name.ToLower() == mapName);
            if (map == null)
            {
                return RemoveResult.MapDoesNotExist;
            }
            else if (map.Setter != caller && !mapModule.IsAdmin(caller))
            {
                return RemoveResult.NoPermission;
            }
            else
            {
                Data.Maps.Remove(map);
                Save();
                return RemoveResult.Okay;
            }
        }

        public UpdateResult Update(SteamID caller, string mapName, string URL)
        {
            var map = GetMap(mapName);
            if (map == null)
            {
                return UpdateResult.MapDoesNotExist;
            }
            else if (caller != map.Setter && !mapModule.IsAdmin(caller))
            {
                return UpdateResult.NoPermission;
            }
            else
            {
                map.URL = URL;
                Save();
                return UpdateResult.Okay;
            }
        }

        public Map GetMap(string mapName)
        {
            return Data.Maps.FirstOrDefault(x => x.Name.ToLower() == mapName);
        }
    }

    class MapModuleConfig : FileStorage<MapModuleData> { }

    class MapModuleData
    {
        public int MaxMaps { get; set; }
        public List<string> Admins { get; set; }
    }

    class MapModule : Module
    {
        private MapList mapList;
        private MapModuleConfig config;
        private CommandParser commandProcessor;
        private CommandListener commandListener;

        #region Commands
        private const string ADD = "!~addmap";
        private const string DELETE = "!~deletemap";
        private const string GET = "!~fetchmaps";
        private const string UPDATE = "!~updatemap";
        #endregion

        public MapList MapList
        {
            get { return mapList; }
        }

        public MapModuleData Config
        {
            get { return config.Data; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            mapList = new MapList(this);
            config = new MapModuleConfig();

            CommandListener.AddCommand<MapAdd>(ADD);
            CommandListener.AddCommand<MapRemove>(DELETE);
            CommandListener.AddCommand<MapGet>(GET);
            CommandListener.AddCommand<MapUpdate>(UPDATE);
        }

        public int MapOverflow()
        {
            if (MapList.Maps.Count > Config.MaxMaps)
            {
                return MapList.Maps.Count - Config.MaxMaps;
            }
            else
            {
                return 0;
            }
        }

        public bool IsAdmin(SteamID id)
        {
            return config.Data.Admins != null ? config.Data.Admins.Contains(id.ToString()) : false;
        }
    }

    class MapCommand : ChatCommand
    {
        protected MapModule MapModule { get; private set; }

        public override void Initialize(Bot bot)
        {
            base.Initialize(bot);
            MapModule = bot.Modules.Get<MapModule>();
        }
    }

    class MapAdd : MapCommand
    {
        public override string Execute(CommandSource source)
        {
            if (source.Parser.Args.Count == 0)
            {
                return "Invalid arguments. Use: !addmap <name> (URL)";
            }
            else
            {
                var map = new Map();
                map.Name = source.Parser.Args[0];
                map.Setter = source.Caller;
                map.URL = source.Parser.Args.Count > 1 ? source.Parser.Args[1] : "Uploaded";

                var result = MapModule.MapList.Add(map);

                switch (result)
                {
                    case MapList.AddResult.MapExists:
                        return string.Format("The map '{0}' already exists.", map.Name);
                    default:
                        return string.Format("Map '{0}' was added.", map.Name);
                }
            }
        }
    }

    class MapRemove : MapCommand
    {
        public override string Execute(CommandSource source)
        {
            if (source.Parser.Args.Count == 0)
                return "Invalid arguments. Use: !removemap <name>";

            var result = MapModule.MapList.Remove(source.Caller, source.Parser.Args[0]);

            switch (result)
            {
                case MapList.RemoveResult.MapDoesNotExist:
                    return string.Format("The map '{0}' cannot be DELETED as it does not exist.", source.Parser.Args[0]);
                case MapList.RemoveResult.NoPermission:
                    return string.Format("You do not have permission to remove the map '{0}'!", source.Parser.Args[0]);
                default:
                    return string.Format("Map '{0}' was DELETED.", source.Parser.Args[0]);
            }
        }
    }

    class MapUpdate : MapCommand
    {
        public override string Execute(CommandSource source)
        {
            if (source.Parser.Args.Count < 2)
                return string.Format("Invalid arguments. Use: !updatemap <name> <url>");

            var result = MapModule.MapList.Update(source.Caller, source.Parser.Args[0], source.Parser.Args[1]);

            switch (result)
            {
                case MapList.UpdateResult.MapDoesNotExist:
                    return string.Format("The map '{0}' cannot be updated as it does not exist.", source.Parser.Args[0]);
                case MapList.UpdateResult.NoPermission:
                    return string.Format("*You do not have permission to update the map '{0}'!", source.Parser.Args[0]);
                default:
                    return string.Format("Map '{0}' was updated.", source.Parser.Args[0]);
            }
        }
    }

    class MapGet : MapCommand
    {
        public override string Execute(CommandSource source)
        {
            if (MapModule.MapList.Maps.Count != 0)
                Bot.Connection.SayToFriend(source.Caller, GetFullMapList());

            return GetShortMapList();
        }

        string GetFullMapList()
        {
            var maps = MapModule.MapList.Maps;
            var str = "Maps:";

            for (int i = 0; i < maps.Count; i++)
            {
                str += "\n" + maps[i].Name;

                if (!string.IsNullOrEmpty(maps[i].URL))
                    str += "\n" + maps[i].URL;

                if (i != maps.Count - 1)
                    str += "\n---";
            }

            return str;
        }

        string GetShortMapList()
        {
            if (MapModule.MapList.Maps.Count == 0)
                return "The map list is empty.";

            var mapList = MapModule
                .MapList
                .Maps
                .Take(MapModule.Config.MaxMaps);

            var str = "Maps: " + string.Join(", ", mapList.Select(x => x.Name));

            if (MapModule.MapOverflow() != 0)
                str += string.Format(" (and {0} more...)", MapModule.MapOverflow());

            return str;
        }
    }
}
