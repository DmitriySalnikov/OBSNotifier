using OBSNotifier.Modules.Event.Default;
using OBSNotifier.Modules.Event.NvidiaLike;
using System;
using System.Collections.Generic;
using System.Linq;
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

        Logger logger = new Logger("logs/module_manager_log.txt");

        // TODO add support for persistent modules
        public List<ModuleData> LoadedModules { get; } = new List<ModuleData>();
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
                (Application.Current as App).gc_collect.CallDeferred();

                CurrentModule = moduleData;
                UpdateCurrentModuleSettings();

                return true;
            }

            return false;
        }

        public void UpdateCurrentModuleSettings()
        {
            var moduleSetting = Settings.Instance.CurrentModuleSettings;

            if (CurrentModule.instance.Settings != null)
                moduleSetting.Data = CurrentModule.instance.Settings.Clone();

            // Validate current module option
            //    Enum module_option;
            //    try
            //    {
            //        module_option = (Enum)Enum.Parse(CurrentModule.instance.EnumOptionsType, moduleSetting.SelectedOption);
            //    }
            //    catch
            //    {
            //        module_option = CurrentModule.defaultSettings.Option;
            //    }

            // Set notification types to default
            if (moduleSetting.ActiveNotificationTypes == null)
            {
                moduleSetting.ActiveNotificationTypes = CurrentModule.instance.DefaultActiveNotifications;
            }

            // Save defaults
            //  if (moduleSetting.FirstLoad)
            //  {
            //      moduleSetting.FirstLoad = false;
            //      moduleSetting.UseSafeDisplayArea = CurrentModule.defaultSettings.UseSafeDisplayArea;
            //      moduleSetting.AdditionalData = CurrentModule.defaultSettings.AdditionalData;
            //      moduleSetting.CustomSettings = CurrentModule.defaultSettings.CustomSettings;
            //      moduleSetting.Offset = CurrentModule.defaultSettings.Offset;
            //      moduleSetting.OnScreenTime = CurrentModule.defaultSettings.OnScreenTime;
            //      moduleSetting.SelectedOption = module_option.ToString();
            //      moduleSetting.Data = CurrentModule.defaultSettings.Settings.Clone();
            //  }

            // TODO load?
            // CurrentModule.instance.ModuleSettings = new OBSNotifierModuleSettings()
            {
                //        UseSafeDisplayArea = moduleSetting.UseSafeDisplayArea,
                //        AdditionalData = moduleSetting.AdditionalData,
                //        CustomSettings = moduleSetting.CustomSettings,
                //        Offset = new Point(moduleSetting.Offset.X, moduleSetting.Offset.Y),
                //        OnScreenTime = Math.Min(moduleSetting.OnScreenTime, 30000),
                //        Option = module_option,
                //    Settings = CurrentModule.instance.ModuleSettings.Settings, // TODO not copy?
            };
            Settings.Instance.Save();
        }

        void WriteLog(string txt)
        {
            logger?.Write(txt);
        }

        void WriteLog(Exception ex)
        {
            logger?.Write(ex);
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
            logger?.Dispose();
            logger = null;
        }
    }
}
