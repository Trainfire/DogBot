using Core;

namespace Modules.DogOfTheDay
{
    class Unmute : DogOfTheDayCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override string Execute(CommandSource source)
        {
            Bot.Modules.Get<DogOfTheDay>().Muted = false;
            return string.Empty;
        }
    }
}
