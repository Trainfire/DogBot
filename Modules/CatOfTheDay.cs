using Core;
using DogBot.Extensions;

namespace Modules.CatOfTheDay
{
    class CatData
    {

    }

    class CatOfTheDay : Module
    {
        private ContentQueue<CatData> _contentQueue;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _contentQueue = new ContentQueue<CatData>();
        }
    }
}
