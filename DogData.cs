using SteamKit2;

namespace DogBot
{
    public class DogData
    {
        SteamFriends friends;

        public SteamID Setter { get; set; }
        public string URL { get; set; }
        public string Message { get; set; }

        public string SetterName
        {
            get
            {
                return Setter != null ? friends.GetFriendPersonaName(Setter) : "Unknown";
            }
        }

        /// <summary>
        /// Returns a DoTD message based on the setter, url, and message.
        /// </summary>
        public string DogOfTheDay
        {
            get
            {
                if (!string.IsNullOrEmpty(URL))
                {
                    if (!string.IsNullOrEmpty(Message))
                    {
                        return string.Format("{0} // {1} // {2} said: '{3}'", Strings.DogOfTheDay, URL, SetterName, Message);
                    }
                    else
                    {
                        return string.Format("{0} // {1}", Strings.DogOfTheDay, URL);
                    }
                }
                else
                {
                    return Strings.NoDog;
                }
            }
        }

        public DogData(SteamFriends friends)
        {
            this.friends = friends;
        }
    }
}
