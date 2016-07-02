using SteamKit2;

namespace Modules.DogOfTheDay
{
    public class DogData
    {
        public string TimeStamp { get; set; }
        public SteamID Setter { get; set; }
        public string URL { get; set; }
        public string Message { get; set; }
    }
}
