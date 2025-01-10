namespace OBSNotifier.Modules.Event.Default
{
    public partial class DefaultNotification : IOBSNotifierModule
    {
        public enum Positions
        {
            TopLeft, TopRight, BottomLeft, BottomRight,
            //Left, Right, Top, Bottom,
            Center
        }

        OBSPerModuleAppInfo? moduleInfo = null;
        DefaultNotificationWindow? window = null;

        public string ModuleID => "Default";
        public string ModuleName => Utils.Tr("module_default_name");

        public string ModuleAuthor => "Dmitriy Salnikov";

        public string ModuleDescription => Utils.Tr("module_default_desc");

        public DefaultCustomNotifBlockSettings SettingsTyped { get; set; } = new DefaultCustomNotifBlockSettings();
        public OBSModuleSettings Settings
        {
            get => SettingsTyped;
            set
            {
                if (value.GetType() != typeof(DefaultCustomNotifBlockSettings))
                    throw new ArgumentException($"'value' is not a {typeof(DefaultCustomNotifBlockSettings)}");
                SettingsTyped = (DefaultCustomNotifBlockSettings)value;
            }
        }

        public NotificationType DefaultActiveNotifications => NotificationType.All;

        public bool ModuleInit(OBSPerModuleAppInfo moduleInfo)
        {
            this.moduleInfo = moduleInfo;
            return true;
        }

        public void ModuleDispose()
        {
            window?.Close();
            window = null;
        }

        public bool ShowNotification(NotificationType type, string title, string? description = null, object[]? originalData = null)
        {
            if (window == null)
            {
                window = new DefaultNotificationWindow(this);
                window.Closing += Window_Closing;
            }

            window.ShowNotif(type, title, description ?? "");
            return true;
        }

        public void ShowPreview()
        {
            if (window == null)
            {
                window = new DefaultNotificationWindow(this);
                window.Closing += Window_Closing;
            }

            window.ShowPreview();
        }

        public void HidePreview()
        {
            window?.HidePreview();
        }

        public void ModuleDeactivate()
        {
            if (window != null)
            {
                window.Closing -= Window_Closing;
                window.Close();
            }
            window = null;
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is DefaultNotificationWindow s)
                s.Closing -= Window_Closing;

            if (window == sender)
                window = null;
        }

        public void Log(string txt)
        {
            moduleInfo?.Log(txt);
        }

        public void Log(Exception ex)
        {
            moduleInfo?.Log($"Exception:\n{ex.Message}\nStackTrace:\n{ex.StackTrace}");
        }
    }
}

