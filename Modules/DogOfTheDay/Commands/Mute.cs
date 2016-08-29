using Core;

namespace Modules.DogOfTheDay
{
    class Mute : DogOfTheDayCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override string Execute(CommandSource source)
        {
            Bot.GetModule<DogOfTheDay>().Muted = true;
            return string.Empty;
        }
    }
}
