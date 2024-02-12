namespace OBSNotifier.Modules.Event
{
    public class OBSModuleSettings
    {
        public virtual NotificationType GetActiveNotifications() => NotificationType.None;
        public virtual OBSModuleSettings Clone() => throw new NotImplementedException();
    }

    public interface IOBSNotifierModule
    {
        string ModuleID { get; }
        string ModuleName { get; }
        string ModuleAuthor { get; }
        string ModuleDescription { get; }
        /// <summary>
        /// The type of enumeration of items for settings. Can be null
        /// </summary>
        Type EnumOptionsType { get; }
        OBSModuleSettings Settings { get; set; }
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
        bool ShowNotification(NotificationType type, string title, string? description = null, object[]? originalData = null);
    }
}
