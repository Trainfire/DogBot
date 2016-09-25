using System;
using System.IO;
using Newtonsoft.Json;

namespace Modules.ThingOfTheDay
{
    class Storage<T>
    {
        private string _fileName;
        private T _data;

        public T Data { get { return _data; } }

        public Storage(string filename)
        {
            _fileName = filename;
            Load();
        }

        public void Load()
        {
            if (File.Exists(Path))
            {
                using (var file = File.OpenText(Path))
                {
                    _data = JsonConvert.DeserializeObject<T>(file.ReadToEnd());
                }
            }
            else
            {
                _data = default(T);
            }
        }

        public void Save()
        {
            using (var sw = File.CreateText(Path))
            {
                sw.Write(JsonConvert.SerializeObject(_data, Formatting.Indented));
            }
        }

        public void Save(T data)
        {
            _data = data;
            Save();
        }

        string Path
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + _fileName; }
        }
    }
}
