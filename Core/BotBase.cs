namespace Core
{
    abstract class BotBase
    {
        public Utils Bot { get; private set; }

        public BotBase(Utils bot)
        {
            Bot = bot;
        }
    }
}
