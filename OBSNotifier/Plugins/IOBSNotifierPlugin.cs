using System;
using System.Windows;

namespace OBSNotifier.Plugins
{
    public struct OBSNotifierPluginSettings
    {
        public uint OnScreenTime;
        public Enum Option;
        public Point Offset;
        public string AdditionalData;
    }

    public interface IOBSNotifierPlugin
    {
        string PluginName { get; }
        string PluginAuthor { get; }
        string PluginDescription { get; }
        /// <summary>
        /// Define which default settings will be available for this plugin
        /// </summary>
        DefaultPluginSettings AvailableDefaultSettings { get; }
        /// <summary>
        /// The type of enumeration of items for settings. Can be null
        /// </summary>
        Type EnumOptionsType { get; }
        /// <summary>
        /// A set of common settings for the plugin
        /// </summary>
        OBSNotifierPluginSettings PluginSettings { get; set; }

        /// <summary>
        /// Called during plugin initialization
        /// </summary>
        /// <param name="logWriter">Simple action for logging to application file</param>
        /// <returns></returns>
        bool PluginInit(Action<string> logWriter);
        /// <summary>
        /// Calling before closing the application
        /// </summary>
        void PluginDispose();
        /// <summary>
        /// Calling when switching to another plugin to delete unused resources such as windows
        /// </summary>
        void ForceCloseAllRelativeToPlugin();
        /// <summary>
        /// Show or Update preview window with new settings
        /// </summary>
        void ShowPreview();
        /// <summary>
        /// Hide preview window or dispose it
        /// </summary>
        void HidePreview();

        /// <summary>
        /// Show or update notification window
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="title">Title</param>
        /// <param name="description">Description</param>
        /// <returns></returns>
        bool ShowNotification(NotificationType type, string title, string description);
    }
}
