using System;
using Core;

namespace Modules.DogOfTheDay
{
    public class Config : FileStorage<ConfigData> { }

    public class ConfigData
    {
        /// <summary>
        /// The ID of the Google Spreadsheet to sync with.
        /// </summary>
        public string SpreadsheetID { get; set; }
    }
}
