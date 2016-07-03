using Core;
using Modules.CommandHandler;

namespace Modules.DogOfTheDay
{
    class Mute : DogOfTheDayCommand
    {
        public override CommandResult Execute(CommandSource source)
        {
            Bot.GetModule<DogOfTheDay>().Muted = true;
            return new CommandResult();
        }
    }
}
