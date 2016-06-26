using SteamKit2;
using System.Linq;

namespace DogBot
{
    public class AddMapParser : MessageParser
    {
        public Map Map{ get; private set; }

        public AddMapParser(SteamID setter, string message) : base(message)
        {
            var mapName = "";
            var URL = "";

            if (Args.Count > 0)
            {
                // First arg is map name
                mapName = Args[0];

                if (Args.Count > 1)
                {
                    // Second is URL
                    URL = Args[1];
                }
                else
                {
                    URL = "Uploaded";
                }
            }
            else
            {
                Invalidate(Strings.UrlInvalid);
            }

            if (IsValid)
            {
                Map = new Map();
                Map.MapName = mapName;
                Map.URL = URL;
                Map.Setter = setter;
            }
        }
    }
}
