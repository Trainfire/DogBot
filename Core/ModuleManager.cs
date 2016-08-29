using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class ModuleManager
    {
        Bot bot;
        List<Module> modules;

        public ModuleManager(Bot bot)
        {
            this.bot = bot;
            modules = new List<Module>();
        }

        public T Add<T>() where T : Module
        {
            var instance = Activator.CreateInstance<T>();
            Add(instance);
            return instance;
        }

        public void Add(Module module)
        {
            if (modules.Any(x => x.GetType() == module.GetType()))
            {
                bot.Logger.Error("Module of type '{0}' has already been added. Only one module of each type is allowed.", module.GetType());
            }
            else
            {
                bot.Logger.Info("Registering module '{0}'", module.GetType().Name);
                modules.Add(module);
                module.Initialize(bot);
            }
        }

        public T Get<T>() where T : Module
        {
            var module = modules.Find(x => x.GetType() == typeof(T));
            if (module != null)
                return module as T;
            return null;
        }

        public T GetOrAddModule<T>() where T : Module
        {
            var module = Get<T>();
            if (module == null)
                module = Add<T>();
            return module;
        }
    }
}
