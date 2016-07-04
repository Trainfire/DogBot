using Core;

namespace Modules.Twitter
{
    public class Twitter : Module
    {
        TinyTwitter tinyTwitter;

        protected override void OnInitialize()
        {
            var config = new Config();
            tinyTwitter = new TinyTwitter(config.Data);
        }

        public void UpdateStatus(string message)
        {
            tinyTwitter.UpdateStatus(message);
        }
    }
}
