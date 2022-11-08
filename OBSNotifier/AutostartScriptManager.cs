using OBSNotifier.Properties;
using System;
using System.IO;
using System.Reflection;

namespace OBSNotifier
{
    static class AutostartScriptManager
    {
        const string OBSNotifierPathReplacePattern = "&OBS_NOTIFIER_PATH&";
        const string scriptName = "obs_notifier_autostart.lua";
        readonly static string scriptText = GetScriptCode();

        public readonly static string ScriptPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), App.AppName), scriptName);
        public static string ProgramPath { get => Assembly.GetExecutingAssembly().Location.Replace('\\', '/'); }

        public static bool IsScriptExists()
        {
            return File.Exists(ScriptPath);
        }

        public static bool IsFileNeedToUpdate(bool suppressLogs = false)
        {
            if (IsScriptExists())
            {
                try
                {
                    using (var f = File.Open(ScriptPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var sr = new StreamReader(f))
                        {
                            var isOutdated = sr.ReadToEnd() != scriptText;
                            if (!suppressLogs)
                                App.Log($"The autorun script was read and compared with the current version: {(isOutdated ? "The file needs to be updated." : "The file is up-to-date.")}");
                            return isOutdated;
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.Log($"The autorun script file cannot be opened: {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        public static bool CreateScript()
        {
            try
            {
                using (var f = File.Open(ScriptPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (var sw = new StreamWriter(f))
                    {
                        sw.Write(scriptText);
                        App.Log($"The autorun script has been written. File path: {ScriptPath}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log($"The autorun script file cannot be written: {ex.Message}");
            }
            return false;
        }

        static string GetScriptCode()
        {
            return Resources.obs_notifier_autostart.Replace(OBSNotifierPathReplacePattern, ProgramPath);
        }
    }
}
