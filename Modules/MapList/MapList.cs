using System;
using System.Collections.Generic;
using Core;
using SteamKit2;
using Extensions.SteamQuery;
using Steam.Query;
using System.Threading.Tasks;
using System.Timers;

namespace Modules.MapList
{
    class MapList : Module, ICommandHandler
    {
        CommandListener commandListener;
        ChatCommandProcessor commandProcessor;
        List<string> maps;
        MapChangeNotifier mapChangeNotifier;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            commandListener = new CommandListener(Bot);
            commandListener.AddCommand<AddMap>("mapsadd", this);
            commandListener.AddCommand<GetMaps>("maps", this);
            commandListener.AddCommand<RemoveMap>("mapremove", this);

            commandProcessor = new ChatCommandProcessor(Bot);

            maps = new List<string>();

            mapChangeNotifier = new MapChangeNotifier();
            mapChangeNotifier.MapChanged += MapChangeNotifier_MapChanged;
        }

        private void MapChangeNotifier_MapChanged(object sender, string e)
        {
            Console.WriteLine("The map has now changed to: " + e);
        }

        void ICommandHandler.OnCommandTriggered(CommandEvent commandEvent)
        {
            commandProcessor.ProcessCommand(commandEvent);
        }

        public void AddMap(string mapName)
        {
            maps.Add(mapName);
        }

        public bool HasMap(string mapName)
        {
            return maps.Contains(mapName);
        }

        public void RemoveMap(string mapName)
        {
            maps.Remove(mapName);
        }

        public string GetMaps()
        {
            return string.Join(" , ", maps);
        }
    }

    class MapChangeNotifier
    {
        public event EventHandler<string> MapChanged;

        ServerInfoResult lastResult;;
        Timer timer;

        public MapChangeNotifier()
        {
            timer = new Timer(24000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            DoPing();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DoPing();
        }

        async void DoPing()
        {
            await PingServer();
        }

        public async Task PingServer()
        {
            try
            {
                var serverInfo = await new SteamQuery().GetServerInfo("70.42.74.31", 27015);

                if (lastResult != null && serverInfo.Map != lastResult.Map)
                {
                    if (MapChanged != null)
                        MapChanged(this, serverInfo.Map);
                }

                lastResult = serverInfo;
            }
            catch
            {

            }
        }
    }

    #region Commands
    class AddMap : ChatCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            var mapList = Bot.GetModule<MapList>();
            mapList.AddMap(source.Parser.Args[0]);
            return new CommandResult("Map Added:" + source.Parser.Args[0]);
        }
    }

    class GetMaps : ChatCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            var mapList = Bot.GetModule<MapList>();
            return new CommandResult(mapList.GetMaps());
        }
    }

    class RemoveMap : ChatCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            var mapList = Bot.GetModule<MapList>();

            var map = source.Parser.Args[0];

            if (mapList.HasMap(map))
            {
                mapList.RemoveMap(map);
                return new CommandResult("Removed map '" + map + "'");
            }
            else
            {
                return new CommandResult("Could not find map!");
            }
        }
    }
    #endregion
}
