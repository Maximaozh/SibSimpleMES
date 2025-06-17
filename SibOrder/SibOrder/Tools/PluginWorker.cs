using ProductionLoader;
using ProductionOrdering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SibOrder
{
    public class PluginWorker
    {
        private static readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs");
        private readonly HashSet<string> loadedGuids = new HashSet<string>();

        public List<IProductionOrdering> OrderPlugins { get; } = new List<IProductionOrdering>();
        public List<IProductionDataProvider> LoaderPlugins { get; } = new List<IProductionDataProvider>();

        public int UpdateAll()
        {
            loadedGuids.Clear();
            OrderPlugins.Clear();
            LoaderPlugins.Clear();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return 0;
            }

            foreach (string dllFile in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dllFile);
                    ProcessAssembly(assembly);
                }
                catch (Exception ex)
                {
                }
            }

            OrderPlugins.Sort((x, y) => string.Compare(x.Name(), y.Name()));
            LoaderPlugins.Sort((x, y) => string.Compare(x.Name(), y.Name()));
            return OrderPlugins.Count + LoaderPlugins.Count;
        }

        private void ProcessAssembly(Assembly assembly)
        {
            var localGuids = new HashSet<string>();

            foreach (Type type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
            {
                try
                {
                    if (typeof(IProductionOrdering).IsAssignableFrom(type))
                    {
                        ProcessType<IProductionOrdering>(type, OrderPlugins, localGuids);
                    }
                    else if (typeof(IProductionDataProvider).IsAssignableFrom(type))
                    {
                        ProcessType<IProductionDataProvider>(type, LoaderPlugins, localGuids);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void ProcessType<T>(Type type, ICollection<T> plugins, HashSet<string> localGuids) where T : class
        {
            T instance = Activator.CreateInstance(type) as T;
            if (instance == null) return;

            string guid = GetGuid(instance);
            if (string.IsNullOrWhiteSpace(guid))
            {
                return;
            }

            if (loadedGuids.Contains(guid) || localGuids.Contains(guid))
            {
                return;
            }

            localGuids.Add(guid);
            loadedGuids.Add(guid);
            plugins.Add(instance);
        }

        private string GetGuid<T>(T plugin) where T : class
        {
            if (plugin is IProductionOrdering ordering)
            {
                return ordering.GUID();
            }
            else if (plugin is IProductionDataProvider provider)
            {
                return provider.GUID();
            }
            return string.Empty;
        }
    }
}