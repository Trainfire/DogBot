using SteamKit2;
using System.Collections.Generic;
using Core;
using System.Threading.Tasks;
using System;

namespace Modules.DogOfTheDay
{
    class Sync : DogOfTheDayCommand
    {
        public override bool AdminOnly
        {
            get
            {
                return true;
            }
        }

        public override bool IsAsync
        {
            get
            {
                return true;
            }
        }

        public override async Task<string> ExecuteAsync(CommandSource source)
        {
            try
            {
                await DogOfTheDay.Data.Sync();
            }
            catch(Exception ex)
            {
                return string.Format("Failed to sync. Reason: " + ex.Message);
            }

            return "Synced successfully!";
        }
    }
}
