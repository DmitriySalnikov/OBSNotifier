using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.Threading;
using System.Windows;

namespace OBSNotifier.Plugins
{
    internal class PluginManager : IDisposable
    {
        public struct PluginData
        {
            public IOBSNotifierPlugin plugin;
            public OBSNotifierPluginSettings defaultSettings;
            public Type pluginClass;
        }

        public static string PluginsDir { get; } = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        public static string PluginsExt { get; } = "*.dll";
        public static string TempDir { get; } = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "OBSNotifier");

        static string LogFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugin_manager_log.txt");
        static TextWriter LogWriter = null;

        public List<PluginData> LoadedPlugins { get; } = new List<PluginData>();
        public PluginData CurrentPlugin { get; private set; }

        const int PluginLoadingTimeout = 5000;

        public PluginManager()
        {
            try
            {
                if (File.Exists(LogFile))
                    File.Delete(LogFile);
                LogWriter  = new StreamWriter(File.OpenWrite(LogFile));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The log file cannot be opened for writing. {ex.Message}");
            }

            CreateTempDirectory();

            WriteLog("============Plugin Manager Loading Begin===========");

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            // Create plugins directory if it does not exists
            try
            {
                // Try to create directory or throw exception and stop loading plugins
                Directory.CreateDirectory(PluginsDir);

                AggregateCatalog agc = new AggregateCatalog();

                // Load default plugins
                {
                    var ac = new AssemblyCatalog(Assembly.GetAssembly(typeof(PluginManager)));
#pragma warning disable S1481 // "Assembly.Load" should be used
                    // may throws ReflectionTypeLoadException
                    // but only when developing default plugins.
                    // Probably this cant happen.
                    var parts = ac.Parts.ToArray();
#pragma warning restore S1481 // Assembly.Load, not comment, unused variable

                    agc.Catalogs.Add(ac);
                }

                // List of all dlls in all sub folders and root plugins directory
                List<string> allFiles = new List<string>();
                allFiles.AddRange(Directory.GetFiles(PluginsDir, PluginsExt));

                foreach (string dir in Directory.GetDirectories(PluginsDir))
                {
                    allFiles.AddRange(Directory.GetFiles(dir, PluginsExt));
                }

                // Works on list of all plugins in plugin directory
                foreach (string file in allFiles)
                {
                    try
                    {
                        var ac = new AssemblyCatalog(LoadFromFile(file));

#pragma warning disable S2201 // Return values from functions without side effects should not be ignored
                        ac.Parts.ToArray(); // throws ReflectionTypeLoadException 
#pragma warning restore S2201

                        agc.Catalogs.Add(ac);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        WriteLog(ex.Message);
                    }
                }

                // End of loading
                CompositionContainer cc = new CompositionContainer(agc);

                // Processing loaded plugins
                foreach (Lazy<IOBSNotifierPlugin> plugins in cc.GetExports<IOBSNotifierPlugin>())
                {
                    // Plugins list marked as Lazy so we need to wait
                    // when plugin was fully loaded
                    int elapsedtime = 0;
                    bool timeout = false;
                    const int pauseTime = 5;
                    while (plugins.Value == null)
                    {
                        Thread.Sleep(pauseTime);
                        elapsedtime += pauseTime;

                        if (elapsedtime > PluginLoadingTimeout)
                        {
                            WriteLog("Plugin can't be loaded. Timed out.");
                            timeout = true;
                            break;
                        }
                    }

                    // If all Ok initialize plugins and add it to list
                    if (!timeout)
                    {
                        try
                        {
                            IOBSNotifierPlugin pp = plugins.Value;

                            if (string.IsNullOrWhiteSpace(pp.PluginName))
                            {
                                WriteLog("Plugin name cannot be empty.");
                                continue;
                            }

                            if (LoadedPlugins.FindIndex((ppp) => ppp.plugin.PluginName == pp.PluginName) != -1)
                            {
                                WriteLog("A plugin with the same name already exists..");
                                continue;
                            }

                            WriteLog(pp.PluginName);

                            if (!pp.PluginInit((s) => WriteLog($"{pp.PluginName}: {s}")))
                            {
                                WriteLog("Plugin can't be initialized.");
                                continue;
                            }

                            LoadedPlugins.Add(new PluginData()
                            {
                                plugin = pp,
                                defaultSettings = pp.PluginSettings,
                                pluginClass = plugins.Value.GetType()
                            });
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                WriteLog($"{plugins.Value?.PluginName}. Plugin can't be initialized. {ex.Message}\n{ex.StackTrace}");
                            }
                            catch
                            {
                                WriteLog("Plugin can't be initialized.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }

            WriteLog("============Plugin Manager Loading End===========");

            WriteLog("##### Loaded Plugins:");
            foreach (var p in LoadedPlugins)
            {
                WriteLog($"Name: {p.plugin.PluginName}, Class: {p.pluginClass}");
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string DllName = new AssemblyName(args.Name).Name + ".dll";

            if (args.RequestingAssembly!= null)
                return LoadFromFile(Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), DllName));
            else
                return null;
        }

        private Assembly LoadFromFile(string file)
        {
            return Assembly.LoadFile(file);
        }

        public static bool IsTempDirectoryExists()
        {
            if (Directory.Exists(TempDir))
            {
                return true;
            }
            else
            {
                if (CreateTempDirectory())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CreateTempDirectory()
        {
            try
            {
                Directory.CreateDirectory(TempDir);
                return true;
            }
            catch (Exception)
            {
                WriteLog("Cant create temp directory: " + TempDir);
                return false;
            }
        }

        static void WriteLog(string txt)
        {
            Console.WriteLine(txt);
            LogWriter?.WriteLine(txt);
            //LogWriter?.Flush();
        }

        public bool SelectCurrent(string name)
        {
            var p = LoadedPlugins.FirstOrDefault((pp) => pp.plugin.PluginName == name);
            return SelectCurrent(p);
        }

        public bool SelectCurrent(PluginData pluginData)
        {
            if (pluginData.plugin != null)
            {
                CurrentPlugin.plugin?.ForceCloseWindow();

                CurrentPlugin = pluginData;
                UpdateCurrentPluginSettings();

                return true;
            }

            return false;
        }

        public void UpdateCurrentPluginSettings()
        {
            CurrentPlugin.plugin.PluginSettings = new OBSNotifierPluginSettings()
            {
                AdditionalData = Settings.Instance.AdditionalData,
                Offset = new Point(Settings.Instance.NotificationOffset.X, Settings.Instance.NotificationOffset.Y),
                OnScreenTime = Math.Min((uint)Settings.Instance.NotificationFadeDelay, 30000),
                Position = (Enum)Enum.Parse(CurrentPlugin.plugin.EnumPositionType, Settings.Instance.NotificationPosition),
            };
        }

        public void Dispose()
        {
            foreach (var pp in LoadedPlugins)
            {
                try
                {
                    pp.plugin.PluginDispose();
                }
                catch (Exception ex)
                {
                    WriteLog($"{pp.plugin.PluginName}. Can't close plugin. {ex.Message}");
                }
            }

            LoadedPlugins.Clear();
            LogWriter?.Close();
            LogWriter = null;

            try
            {
                Directory.Delete(TempDir, true);
            }
            catch (Exception ex)
            {
                WriteLog("Cant delete directory: " + TempDir + "\nException: " + ex.Message);
            }
        }
    }
}
