using Core;
using Modules.CommandHandler;

namespace Modules.DogOfTheDay
{
    public class DogOfTheDayCommand : ChatCommand
    {
        protected DogOfTheDay DogOfTheDay { get; private set; }

        public override void Initialize(Bot bot)
        {
            base.Initialize(bot);
            DogOfTheDay = bot.GetModule<DogOfTheDay>();
        }
    }
}
