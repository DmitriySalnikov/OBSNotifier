using OBSNotifier.Plugins;
using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OBSNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        public static OBSWebsocket obs;
        public static PluginManager plugins;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            obs = new OBSWebsocket();

            Settings.Load();
            plugins = new PluginManager();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            obs.Disconnect();
            Settings.Instance.Save();
            plugins?.Dispose();
            plugins = null;
        }
    }
}
