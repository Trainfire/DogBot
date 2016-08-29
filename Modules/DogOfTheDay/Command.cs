using Core;

namespace Modules.DogOfTheDay
{
    public class DogOfTheDayCommand : ChatCommand
    {
        protected DogOfTheDay DogOfTheDay { get; private set; }

        public override string NoPermissionMessage
        {
            get { return "*bark* You do not have permission to do that!"; }
        }

        public override void Initialize(Bot bot)
        {
            base.Initialize(bot);
            DogOfTheDay = bot.GetModule<DogOfTheDay>();
        }
    }
}
