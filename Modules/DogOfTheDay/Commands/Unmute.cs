using Core;
using Modules.CommandHandler;

namespace Modules.DogOfTheDay
{
    class Unmute : DogOfTheDayCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            Bot.GetModule<DogOfTheDay>().Muted = false;
            return new CommandResult();
        }
    }
}
