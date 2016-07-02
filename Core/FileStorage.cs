using Newtonsoft.Json;
using System;
using System.IO;

namespace Core
{
    public abstract class FileStorage<T>
    {
        public T Data { get; private set; }

        public virtual string Filename { get { return GetType().FullName; } }

        string Path
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public FileStorage()
        {
            Load();
        }

        public void Save()
        {
            var sw = File.CreateText(Path + Filename);
            sw.Write(JsonConvert.SerializeObject(Data, Formatting.Indented));
            sw.Close();
        }

        public void Load()
        {
            if (File.Exists(Path + Filename))
            {
                var file = File.ReadAllText(Path + Filename);
                Data = JsonConvert.DeserializeObject<T>(file);
            }
            else
            {
                Data = Activator.CreateInstance<T>();
                Save();
            }
        }
    }
}
