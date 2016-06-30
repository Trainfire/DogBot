using Newtonsoft.Json;
using System;
using System.IO;

namespace Core
{
    public abstract class FileStorage<T>
    {
        public T Data { get; private set; }

        public abstract string Filename { get; }

        string Path
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public FileStorage()
        {
            Data = Load();
        }

        public void Save()
        {
            var sw = File.CreateText(Path + Filename);
            sw.Write(JsonConvert.SerializeObject(Data, Formatting.Indented));
            sw.Close();
        }

        public T Load()
        {
            if (File.Exists(Path + Filename))
            {
                var file = File.ReadAllText(Path + Filename);
                return JsonConvert.DeserializeObject<T>(file);
            }
            else
            {
                return Activator.CreateInstance<T>();
            }
        }
    }
}
