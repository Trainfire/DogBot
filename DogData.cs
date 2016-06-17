using SteamKit2;

namespace DogBot
{
    public class DogData
    {
        public string TimeStamp { get; set; }
        public SteamID Setter { get; set; }
        public string URL { get; set; }
        public string Message { get; set; }
        public bool Shown { get; set; }

        /// <summary>
        /// Returns true if a dog has been set.
        /// </summary>
        public bool IsSet
        {
            get
            {
                return !string.IsNullOrEmpty(URL);
            }
        }
    }
}
