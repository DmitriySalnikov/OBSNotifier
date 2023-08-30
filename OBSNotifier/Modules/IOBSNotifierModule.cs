using System;
using System.Windows;

namespace OBSNotifier.Modules
{
    public struct OBSNotifierModuleSettings
    {
        public bool UseSafeDisplayArea;
        public uint OnScreenTime;
        public Enum Option;
        public Point Offset;
        public string AdditionalData;
        public string CustomSettings;
    }

    public interface IOBSNotifierModule
    {
        string ModuleID { get; }
        string ModuleName { get; }
        string ModuleAuthor { get; }
        string ModuleDescription { get; }
        /// <summary>
        /// Define which default settings will be available for this module
        /// </summary>
        AvailableModuleSettings DefaultAvailableSettings { get; }
        /// <summary>
        /// The type of enumeration of items for settings. Can be null
        /// </summary>
        Type EnumOptionsType { get; }
        /// <summary>
        /// A set of common settings for the module
        /// </summary>
        OBSNotifierModuleSettings ModuleSettings { get; set; }
        /// <summary>
        /// Default types of active notifications
        /// </summary>
        NotificationType DefaultActiveNotifications { get; }

        /// <summary>
        /// Called during module initialization
        /// </summary>
        /// <param name="logWriter">Simple action for logging to application file</param>
        /// <returns></returns>
        bool ModuleInit(Action<string> logWriter);
        /// <summary>
        /// Calling before closing the application
        /// </summary>
        void ModuleDispose();
        /// <summary>
        /// Calling when switching to another module to delete unused resources such as windows
        /// </summary>
        void ForceCloseAllRelativeToModule();
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
        /// <param name="originalData">The original OBS callback arguments, so you can show them as you wish</param>
        /// <returns></returns>
        bool ShowNotification(NotificationType type, string title, string description = null, object[] originalData = null);
        /// <summary>
        /// Calling on Custom Settings Button pressed
        /// </summary>
        void OpenCustomSettings();
        /// <summary>
        /// Default mechanism for saving module settings data into <see cref="OBSNotifierModuleSettings.CustomSettings"/>
        /// </summary>
        /// <returns>Any string with your settings or null</returns>
        string GetCustomSettingsDataToSave();
        /// <summary>
        /// This method is needed to format or correct user's errors in <see cref="OBSNotifierModuleSettings.AdditionalData"/>
        /// </summary>
        /// <returns>Any string with fixed settings or null</returns>
        string GetFixedAdditionalData();
    }
}
