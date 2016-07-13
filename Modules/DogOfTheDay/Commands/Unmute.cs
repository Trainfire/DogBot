using Core;

namespace Modules.DogOfTheDay
{
    class Unmute : DogOfTheDayCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override CommandResult Execute(CommandSource source)
        {
            Bot.GetModule<DogOfTheDay>().Muted = false;
            return new CommandResult();
        }
    }
}
