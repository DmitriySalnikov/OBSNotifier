using OBSNotifier.Modules.Event.Default;
using OBSNotifier.Modules.Event.NvidiaLike;
using System.Windows;

namespace OBSNotifier.Modules.Event
{
    public class ModuleManager : IDisposable
    {
        public struct ModuleData
        {
            public IOBSNotifierModule instance;
            public OBSModuleSettings defaultSettings;

            public ModuleData(IOBSNotifierModule module) : this()
            {
                this.instance = module;
                this.defaultSettings = module.Settings.Clone();
            }
        }

        readonly Logger logger = new("logs/module_manager_log.txt");

        // TODO add support for persistent modules
        public List<ModuleData> LoadedModules { get; } = [];
        public IEnumerable<ModuleData> ActiveModules
        {
            get
            {
                foreach (var mod in LoadedModules)
                {
                    if (Settings.Instance.ActiveModules.Contains(mod.instance.ModuleID))
                    {
                        yield return mod;
                    }
                }
            }
        }

        public ModuleManager()
        {
            WriteLog(":Module Manager Loading Begin");

            LoadedModules.Add(new ModuleData(new DefaultNotification()));
            LoadedModules.Add(new ModuleData(new NvidiaNotification()));

            foreach (var pd in LoadedModules)
            {
                if (!pd.instance.ModuleInit((s) => WriteLog($"{pd.instance.ModuleID}: {s}")))
                {
                    throw new Exception($"{pd.instance.GetType()} is broken!");
                }
            }

            WriteLog("\n:Module Manager Loading End");

            WriteLog("# Loaded Module:");
            foreach (var p in LoadedModules)
            {
                WriteLog($"Name: {p.instance.ModuleID}, Class: {p.instance.GetType()}");
            }
            WriteLog("# End");
            WriteLog("\n:Module logs will be shown below");
        }

        public ModuleData? GetModuleById(string moduleId)
        {
            foreach (var mod in LoadedModules)
            {
                if (mod.instance.ModuleID == moduleId)
                {
                    return mod;
                }
            }

            return null;
        }

        public void UpdateActiveModules()
        {
            foreach (var mod in LoadedModules)
            {
                if (!Settings.Instance.ActiveModules.Contains(mod.instance.ModuleID))
                {
                    mod.instance.ForceCloseAllRelativeToModule();
                }
            }

                ((App)Application.Current).gc_collect.CallDeferred();
        }

        static bool IsActive(IOBSNotifierModule module, NotificationType type)
        {
            var notifs = module.Settings.GetActiveNotifications();
            return notifs.HasFlag(type);
        }

        public void ShowNotification(NotificationType type, string title, string? description = null, object[]? originalData = null)
        {
            foreach (var m in ActiveModules)
            {
                if (IsActive(m.instance, type))
                {
                    try
                    {
                        m.instance.ShowNotification(type, title, description, originalData);
                    }
                    catch (Exception ex)
                    {
                        App.Log(ex);
                    }
                }
            }
        }

        void WriteLog(string txt)
        {
            logger.Write(txt);
        }

        void WriteLog(Exception ex)
        {
            logger.Write(ex);
        }

        public void Dispose()
        {
            foreach (var pp in LoadedModules)
            {
                try
                {
                    pp.instance.ModuleDispose();
                }
                catch (Exception ex)
                {
                    WriteLog($"{pp.instance.ModuleID}. Can't close module. {ex.Message}");
                }
            }

            LoadedModules.Clear();
            logger.Dispose();
        }
    }
}
