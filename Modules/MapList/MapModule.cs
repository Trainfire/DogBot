﻿using System;
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

    class MapList
    {
        private List<Map> maps;

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

        /// <summary>
        /// Returns a copy of the map list.
        /// </summary>
        public List<Map> Maps
        {
            get { return maps.ToList(); }
        }

        public MapList()
        {
            maps = new List<Map>();
        }

        public AddResult Add(Map map)
        {
            if (maps.Any(x => x.Name.ToLower() == map.Name.ToLower()))
            {
                return AddResult.MapExists;
            }
            else
            {
                maps.Add(map);
                return AddResult.Okay;
            }
        }

        public RemoveResult Remove(SteamID caller, string mapName)
        {
            var map = maps.FirstOrDefault(x => x.Name.ToLower() == mapName);
            if (map == null)
            {
                return RemoveResult.MapDoesNotExist;
            }
            else if (map.Setter != caller)
            {
                return RemoveResult.NoPermission;
            }
            else
            {
                maps.Remove(map);
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
            else if (caller != map.Setter)
            {
                return UpdateResult.NoPermission;
            }
            else
            {
                map.URL = URL;
                return UpdateResult.Okay;
            }
        }

        public Map GetMap(string mapName)
        {
            return maps.FirstOrDefault(x => x.Name.ToLower() == mapName);
        }
    }

    /// <summary>
    /// TODO: Serialize into JSON.
    /// </summary>
    class MapModuleConfig
    {
        public int MaxMaps { get; set; }
    }

    class MapModule : Module, ICommandHandler
    {
        private MapList mapList;
        private MapModuleConfig config;
        private ChatCommandProcessor commandProcessor;
        private CommandListener commandListener;

        #region Commands
        private const string ADD = "addmap";
        private const string DELETE = "deletemap";
        private const string GET = "fetchmaps";
        private const string UPDATE = "updatemap";
        #endregion

        public MapList MapList
        {
            get { return mapList; }
        }

        public MapModuleConfig Config
        {
            get { return config; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            mapList = new MapList(); // TODO: Load from file.

            config = new MapModuleConfig()
            {
                MaxMaps = 1,
            };

            commandProcessor = new ChatCommandProcessor(Bot);

            commandListener = new CommandListener(Bot);
            commandListener.AddCommand<MapAdd>(ADD, this);
            commandListener.AddCommand<MapRemove>(DELETE, this);
            commandListener.AddCommand<MapGet>(GET, this);
            commandListener.AddCommand<MapUpdate>(UPDATE, this);
        }

        void ICommandHandler.OnCommandTriggered(CommandEvent commandEvent)
        {
            commandProcessor.ProcessCommand(commandEvent);
        }

        public int MapOverflow()
        {
            if (MapList.Maps.Count > config.MaxMaps)
            {
                return MapList.Maps.Count - config.MaxMaps;
            }
            else
            {
                return 0;
            }
        }
    }

    class MapCommand : ChatCommand
    {
        protected MapModule MapModule { get; private set; }

        public override void Initialize(Bot bot)
        {
            base.Initialize(bot);
            MapModule = bot.GetModule<MapModule>();
        }
    }

    class MapAdd : MapCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            if (source.Parser.Args.Count == 0)
            {
                return new CommandResult("*bark!* Invalid arguments. Use: !addmap <name> (URL)");
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
                    case MapList.AddResult.Okay:
                        return new CommandResult("Map '{0}' was added.", map.Name);
                    case MapList.AddResult.MapExists:
                        return new CommandResult("*whines* The map '{0}' already exists.", map.Name);
                    default:
                        break;
                }
            }

            return new CommandResult("*whines* This shouldn't happen!");
        }
    }

    class MapRemove : MapCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            if (source.Parser.Args.Count == 0)
                return new CommandResult("*bark!* Invalid arguments. Use: !removemap <name>");

            var result = MapModule.MapList.Remove(source.Caller, source.Parser.Args[0]);

            switch (result)
            {
                case MapList.RemoveResult.MapDoesNotExist:
                    return new CommandResult("*whines* The map '{0}' cannot be DELETED as it does not exist.", source.Parser.Args[0]);
                case MapList.RemoveResult.NoPermission:
                    return new CommandResult("*bark!* You do not have permission to remove the map '{0}'!", source.Parser.Args[0]);
                default:
                    return new CommandResult("Map '{0}' was DELETED.", source.Parser.Args[0]);
            }
        }
    }

    class MapUpdate : MapCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            if (source.Parser.Args.Count < 2)
                return new CommandResult("*bark!* Invalid arguments. Use: !updatemap <name> <url>");

            var result = MapModule.MapList.Update(source.Caller, source.Parser.Args[0], source.Parser.Args[1]);

            switch (result)
            {
                case MapList.UpdateResult.MapDoesNotExist:
                    return new CommandResult("*whines* The map '{0}' cannot be updated as it does not exist.", source.Parser.Args[0]);
                case MapList.UpdateResult.NoPermission:
                    return new CommandResult("*bark!* You do not have permission to update the map '{0}'!", source.Parser.Args[0]);
                default:
                    return new CommandResult("Map '{0}' was updated.", source.Parser.Args[0]);
            }
        }
    }

    class MapGet : MapCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            if (MapModule.MapList.Maps.Count != 0)
                Bot.SayToFriend(source.Caller, GetFullMapList());

            return new CommandResult(GetShortMapList());
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
                return "*whines* The map list is empty.";

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