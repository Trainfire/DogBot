using Core;

namespace Modules.DogOfTheDay
{
    class Unmute : DogOfTheDayCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override string Execute(CommandSource source)
        {
            Bot.GetModule<DogOfTheDay>().Muted = false;
            return string.Empty;
        }
    }
}
