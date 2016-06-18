using System.Collections.Generic;
using System.Linq;

namespace DogBot
{
    public class History : FileLogger<HistoryData>
    {
        public HistoryStats Stats { get; private set; }

        public History()
        {
            Stats = new HistoryStats(Data);
        }

        public void Write(HistoryRecord record)
        {
            Data.History.Add(record);
        }

        public override string Filename
        {
            get
            {
                return "history.json";
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

        public List<DogData> GetUserContributions(SteamKit2.SteamID steamID)
        {
            return data.History.Where(x => x.Dog.Setter == steamID).Select(x => x.Dog).ToList();
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
