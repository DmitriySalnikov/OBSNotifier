using Microsoft.Win32;

namespace OBSNotifier
{
    static class AutostartManager
    {
        const string OBSNotifierPathReplacePattern = "&OBS_NOTIFIER_PATH&";
        const string OBSNotifierOsReplacePattern = "&OBS_NOTIFIER_OS&";
        const string scriptName = "obs_notifier_autostart.lua";
        const string autostartKeyName = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        readonly static string scriptText = GetScriptCode();

        public readonly static string ScriptPath = Path.Combine(App.AppDataFolder, scriptName);
        public static string ProgramPath
        {
            get
            {
                var exe = Assembly.GetExecutingAssembly().Location.Replace('\\', '/');
                if (exe.EndsWith(".dll"))
                {
                    return exe.Replace(".dll", ".exe");
                }
                return exe;
            }
        }

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
                    using var f = File.Open(ScriptPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var sr = new StreamReader(f);
                    var isOutdated = sr.ReadToEnd() != scriptText;
                    if (!suppressLogs)
                        App.Log($"The autorun script was read and compared with the current version: {(isOutdated ? "The file needs to be updated." : "The file is up-to-date.")}");
                    return isOutdated;
                }
                catch (Exception ex)
                {
                    App.LogError("The autorun script file cannot be opened.");
                    App.Log(ex);
                    return false;
                }
            }

            return true;
        }

        public static bool CreateScript()
        {
            try
            {
                using var f = File.Open(ScriptPath, FileMode.Create, FileAccess.Write, FileShare.Write);
                using var sw = new StreamWriter(f);
                sw.Write(scriptText);
                App.Log($"The autorun script has been written. File path: {ScriptPath}");
                return true;
            }
            catch (Exception ex)
            {
                App.LogError("The autorun script file cannot be written.");
                App.Log(ex);
            }
            return false;
        }

        static string GetScriptCode()
        {
            return AppResources.obs_notifier_autostart
                .Replace(OBSNotifierPathReplacePattern, ProgramPath)
                .Replace(OBSNotifierOsReplacePattern, Utils.GetOsName());
        }

        /// <summary>
        /// Get the autostart path from the registry
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static string GetAutostartPath(string appName)
        {
            using var rkApp = Registry.CurrentUser.OpenSubKey(autostartKeyName, true);
            if (rkApp != null)
                return (string)(rkApp.GetValue(appName) ?? string.Empty);
            else
                throw new NullReferenceException(nameof(rkApp));
        }

        /// <summary>
        /// Check if the current application path and the autostart path match
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="exeName"></param>
        /// <returns></returns>
        public static bool IsAutostartPathUpdated(string appName, string? exeName = null)
        {
            return GetAutostartPath(appName).Replace("\\", "/") == (exeName?.Replace("\\", "/") ?? ProgramPath);
        }

        /// <summary>
        /// Create autostart in the registry
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="exeName"></param>
        public static void SetAutostart(string appName, string? exeName = null)
        {
            using var rkApp = Registry.CurrentUser.OpenSubKey(autostartKeyName, true);
            if (rkApp != null)
                rkApp.SetValue(appName, exeName?.Replace("\\", "/") ?? ProgramPath);
            else
                throw new NullReferenceException(nameof(rkApp));
        }

        /// <summary>
        /// Remove autostart from the registry
        /// </summary>
        /// <param name="appName"></param>
        public static void RemoveAutostart(string appName)
        {
            using var rkApp = Registry.CurrentUser.OpenSubKey(autostartKeyName, true);
            if (rkApp != null)
                rkApp.DeleteValue(appName, false);
            else
                throw new NullReferenceException(nameof(rkApp));
        }
    }
}
