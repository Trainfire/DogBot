using Newtonsoft.Json;
using System;
using System.IO;

namespace Core
{
    public abstract class FileStorage<T>
    {
        public T Data { get; private set; }

        public virtual string Filename { get { return GetType().FullName.ToLower() + ".json"; } }

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
            using (var sw = File.CreateText(Path + Filename))
            {
                sw.Write(JsonConvert.SerializeObject(Data, Formatting.Indented));
            }
        }

        public void Load()
        {
            if (File.Exists(Path + Filename))
            {
                using (var file = File.OpenText(Path + Filename))
                {
                    Data = JsonConvert.DeserializeObject<T>(file.ReadToEnd());
                }
            }
            else
            {
                Data = Activator.CreateInstance<T>();
                Save();
            }
        }
    }
}
