namespace OBSNotifier.Modules.Event
{
    public class OBSModuleSettings
    {
        public virtual NotificationType GetActiveNotifications() => NotificationType.None;
        public virtual OBSModuleSettings Clone() => throw new NotImplementedException();
    }

    public class OBSPerModuleAppInfo(Action<string> logWriter, string? moduleFolder)
    {
        public void Log(string message)
        {
            logWriter.Invoke(message);
        }

        public string? GetModuleFolder()
        {
            return moduleFolder;
        }
    }

    public interface IOBSNotifierModule
    {

        /// <summary>
        /// Unique module ID.
        /// It can also be used as the name of the module settings folder.
        /// </summary>
        string ModuleID { get; }
        string ModuleName { get; }
        string ModuleAuthor { get; }
        string ModuleDescription { get; }

        OBSModuleSettings Settings { get; set; }
        /// <summary>
        /// Default types of active notifications
        /// </summary>
        NotificationType DefaultActiveNotifications { get; }

        /// <summary>
        /// Called during module initialization
        /// </summary>
        /// <param name="moduleInfo">Some information for interacting with the application</param>
        /// <returns></returns>
        bool ModuleInit(OBSPerModuleAppInfo moduleInfo);
        /// <summary>
        /// Calling before closing the application
        /// </summary>
        void ModuleDispose();
        /// <summary>
        /// Called when the module is deactivated
        /// </summary>
        void ModuleDeactivate();
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
