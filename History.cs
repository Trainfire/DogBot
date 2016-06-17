using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DogBot
{
    public class History
    {
        HistoryData data;

        const string FILENAME = "history.json";

        public HistoryStats Stats { get; private set; }

        string Path
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public History()
        {
            data = Load();
            Stats = new HistoryStats(data);
        }

        public void Write(HistoryRecord record)
        {
            data.History.Add(record);
        }

        public void Save()
        {
            var sw = File.CreateText(Path + FILENAME);
            sw.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
            sw.Close();
        }

        public HistoryData Load()
        {
            if (File.Exists(Path + FILENAME))
            {
                var file = File.ReadAllText(Path + FILENAME);
                return JsonConvert.DeserializeObject<HistoryData>(file);
            }
            else
            {
                return new HistoryData();
            }
        }
    }

    public class HistoryData
    {
        public List<HistoryRecord> History { get; set; }

        public HistoryData()
        {
            if (History == null)
                History = new List<HistoryRecord>();
        }
    }

    public class HistoryRecord
    {
        public DogData Dog { get; set; }
    }

    public class HistoryStats
    {
        readonly HistoryData data;

        public int TotalRecords { get { return data.History.Count; } }

        public HistoryContributor HighestContributer
        {
            get
            {
                var highest = data.History.OrderBy(x => x.Dog.Setter.ToString()).FirstOrDefault();
                if (highest != null)
                {
                    var contributions = data.History.Where(x => x.Dog.Setter == highest.Dog.Setter).ToList().Count;
                    return new HistoryContributor(highest.Dog.Setter.ToString(), contributions);
                }
                return null;
            }
        }

        public List<DogData> Dogs
        {
            get
            {
                return data.History.Select(x => x.Dog).ToList();
            }
        }

        public DogData LatestDog
        {
            get
            {
                if (data.History != null && data.History.Count != 0)
                {
                    var record = data.History.Last();
                    if (record != null)
                        return record.Dog;
                }
                return null;
            }
        }

        public int Unshown
        {
            get
            {
                return data.History.Select(x => !x.Dog.Shown).ToList().Count;
            }
        }

        /// <summary>
        /// Returns the next dog in the list that hasn't been shown. Otherwise, returns the last shown dog.
        /// </summary>
        public DogData NextDog
        {
            get
            {
                var record = data.History.FirstOrDefault(x => !x.Dog.Shown);
                if (record != null)
                {
                    return record.Dog;
                }
                else
                {
                    return data.History.Last().Dog;
                }
            }
        }

        public class HistoryContributor
        {
            public string SteamID { get; private set; }
            public int Contributions { get; private set; }

            public HistoryContributor(string steamID, int contributions)
            {
                SteamID = steamID;
                Contributions = contributions;
            }
        }

        public HistoryStats(HistoryData data)
        {
            this.data = data;
        }
    }
}
