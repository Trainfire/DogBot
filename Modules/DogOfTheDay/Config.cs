using System;
using Core;

namespace Modules.DogOfTheDay
{
    public class Config : FileStorage<ConfigData> { }

    public class ConfigData
    {
        /// <summary>
        /// Set to true to sync data with a Google Spreadsheet.
        /// </summary>
        public bool SyncEnabled { get; set; }

        /// <summary>
        /// The ID of the Google Spreadsheet to sync with.
        /// </summary>
        public string SpreadsheetID { get; set; }
    }
}
