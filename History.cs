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
        public string TimeStamp { get; set; }
        public string URL { get; set; }
        public string SetterSteamID { get; set; }
    }

    public class HistoryStats
    {
        readonly HistoryData data;

        public int TotalRecords { get { return data.History.Count; } }

        public HistoryContributor HighestContributer
        {
            get
            {
                var highest = data.History.OrderBy(x => x.SetterSteamID).FirstOrDefault();
                if (highest != null)
                {
                    var contributions = data.History.Where(x => x.SetterSteamID == highest.SetterSteamID).ToList().Count;
                    return new HistoryContributor(highest.SetterSteamID, contributions);
                }
                return null;
            }
        }

        public List<DogData> Dogs
        {
            get
            {
                var dogs = new List<DogData>();
                foreach (var dog in data.History)
                {
                    dogs.Add(new DogData()
                    {
                        Setter = new SteamKit2.SteamID(dog.SetterSteamID),
                        URL = dog.URL,
                    });
                }
                return dogs;
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
