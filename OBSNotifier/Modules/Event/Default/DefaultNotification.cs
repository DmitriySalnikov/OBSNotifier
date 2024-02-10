namespace OBSNotifier.Modules.Event.Default
{
    internal partial class DefaultNotification : IOBSNotifierModule
    {
        internal enum Positions
        {
            TopLeft, TopRight, BottomLeft, BottomRight,
            //Left, Right, Top, Bottom,
            Center
        }

        Action<string>? logWriter;
        DefaultNotificationWindow? window = null;

        public string ModuleID => "Default";
        public string ModuleName => Utils.Tr("default_module_name");

        public string ModuleAuthor => "Dmitriy Salnikov";

        public string ModuleDescription => Utils.Tr("default_module_desc");

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

        public Type EnumOptionsType => typeof(Positions);

        public NotificationType DefaultActiveNotifications => NotificationType.All;

        public bool ModuleInit(Action<string> logWriter)
        {
            this.logWriter = logWriter;
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

        public void ForceCloseAllRelativeToModule()
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
            var s = sender as DefaultNotificationWindow;
            if (s != null)
                s.Closing -= Window_Closing;

            if (window == sender)
                window = null;
        }

        public void Log(string txt)
        {
            logWriter?.Invoke(txt);
        }

        public void Log(Exception ex)
        {
            Log($"Exception:\n{ex.Message}\nStackTrace:\n{ex.StackTrace}");
        }
    }
}

