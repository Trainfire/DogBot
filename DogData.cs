using SteamKit2;

namespace DogBot
{
    public class DogData
    {
        public SteamID Setter { get; set; }
        public string URL { get; set; }
        public string Message { get; set; }

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
