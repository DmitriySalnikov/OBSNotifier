using OBSNotifier.Modules.Event.Default;
using OBSNotifier.Modules.Event.NvidiaLike;
using System.Windows;

namespace OBSNotifier.Modules.Event
{
    internal class ModuleManager : IDisposable
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
        public ModuleData CurrentModule { get; private set; }

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

        public bool SelectCurrent(string name)
        {
            var p = LoadedModules.FirstOrDefault((pp) => pp.instance.ModuleID == name);
            return SelectCurrent(p);
        }

        public bool SelectCurrent(ModuleData moduleData)
        {
            if (moduleData.instance != null)
            {
                if (moduleData.instance == CurrentModule.instance)
                    return true;

                CurrentModule.instance?.ForceCloseAllRelativeToModule();
                ((App)Application.Current).gc_collect.CallDeferred();

                CurrentModule = moduleData;

                return true;
            }

            return false;
        }

        static bool IsActive(IOBSNotifierModule module, NotificationType type)
        {
            var notifs = module.Settings.GetActiveNotifications();
            return notifs.HasFlag(type);
        }

        public void ShowNotification(NotificationType type, string title, string? description = null, object[]? originalData = null)
        {
            foreach (var m in LoadedModules)
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
