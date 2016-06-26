using SteamKit2;

namespace DogBot
{
    class GetMaps : CommandAction
    {
        public override CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser)
        {
            if (bot.Data.Maps.Count != 0)
            {
                var maps = string.Join(", ", bot.Data.Maps.MapList);

                return new CommandResult()
                {
                    FeedbackMessage = "Maps: " + maps,
                    PrivateMessage = "Upload Status: " + string.Join(", ", bot.Data.Maps.URLS)
                };
            }
            else
            {
                return new CommandResult(string.Format("No maps have been added. Use '{0}{1}' to add a map", CommandRegistry.COMMAND_TOKEN, CommandRegistry.ADDMAP));
            }
        }
    }
}
