using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
namespace Kurisu.Framework.DataDriven
{
    public abstract class DataTableManager
    {
        private static bool isLoaded;
#if AF_INITIALIZE_DATATABLE_MANAGER_ON_LOAD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void Initialize()
        {
            if (isLoaded) return;
            isLoaded = true;
            var managerTypes = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(x => x.GetTypes())
                            .Where(x => typeof(DataTableManager).IsAssignableFrom(x) && !x.IsAbstract)
                            .ToArray();

            var args = new object[1] { null };
            foreach (var type in managerTypes)
            {
                var manager = Activator.CreateInstance(type, args) as DataTableManager;
                manager!.Initialize(true).Forget();
            }
        }
        /// <summary>
        /// Manual initialization api
        /// </summary>
        /// <returns></returns>
        public static async UniTask InitializeAsync()
        {
            if (isLoaded) return;
            isLoaded = true;
            var managerTypes = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(x => x.GetTypes())
                            .Where(x => typeof(DataTableManager).IsAssignableFrom(x) && !x.IsAbstract)
                            .ToArray();

            var args = new object[1] { null };
            using var parallel = UniParallel.Get();
            foreach (var type in managerTypes)
            {
                var manager = Activator.CreateInstance(type, args) as DataTableManager;
                parallel.Add(manager!.Initialize(false));
            }
            await parallel;
        }

        protected readonly Dictionary<string, DataTable> DataTables = new();

        protected void RegisterDataTable(string name, DataTable dataTable)
        {
            DataTables[name] = dataTable;
        }
        protected void RegisterDataTable(DataTable dataTable)
        {
            DataTables[dataTable.name] = dataTable;
        }
        
        public DataTable GetDataTable(string name)
        {
            if (DataTables.TryGetValue(name, out var table))
            {
                return table;
            }
            return null;
        }
        /// <summary>
        /// Async initiaize manager at start of game, loading your dataTables in this stage
        /// </summary>
        /// <param name="sync">Whether initialize in sync, useful when need blocking loading</param>
        /// <returns></returns>
        protected abstract UniTask Initialize(bool sync);
    }
    public abstract class DataTableManager<TManager> : DataTableManager where TManager : DataTableManager<TManager>
    {
        private static TManager instance;
        // Force implementation has this constructor
        public DataTableManager(object _)
        {
            instance = (TManager)this;
        }
        /// <summary>
        /// Get <see cref="{TManager}"/>
        /// </summary>
        /// <returns></returns>
        public static TManager Get()
        {
            if (instance == null)
            {
                Initialize(); /* Initialize in blocking mode */
            }
            return instance;
        }
    }
}
