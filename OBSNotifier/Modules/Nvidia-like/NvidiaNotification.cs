using System;
using System.Windows;

namespace OBSNotifier.Modules.NvidiaLike
{
    public partial class NvidiaNotification : IOBSNotifierModule
    {
        internal enum Positions
        {
            TopLeft, TopRight,
        }

        Action<string> logWriter = null;
        NvidiaNotificationWindow window = null;

        public string ModuleID => "Nvidia-Like";
        public string ModuleName => Utils.Tr("nvidia_like_module_name");

        public string ModuleAuthor => "Dmitriy Salnikov";

        public string ModuleDescription => Utils.Tr("nvidia_like_module_desc");

        public AvailableModuleSettings DefaultAvailableSettings => AvailableModuleSettings.AllNoCustomSettings;

        OBSNotifierModuleSettings _moduleSettings = new OBSNotifierModuleSettings()
        {
            UseSafeDisplayArea = true,
            AdditionalData = "BackgroundColor = #2E48BD\nForegroundColor = #000000\nTextColor = #E4E4E4\nSlideDuration = 400\nSlideOffset = 180\nLineWidth = 6.0\nScale = 1.0\nMaxPathChars = 32\nShowQuickActions = True\nShowQuickActionsColoredLine = True\nQuickActionsOffset = 8.0\nIconHeight = 64.0\nIconPath = INVALID_PATH",
            Option = Positions.TopRight,
            Offset = new Point(0, 0.1),
            OnScreenTime = 3000,
        };

        public OBSNotifierModuleSettings ModuleSettings
        {
            get => _moduleSettings;
            set => _moduleSettings = value;
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

        public bool ShowNotification(NotificationType type, string title, string description = null, object[] originalData = null)
        {
            if (window == null)
            {
                window = new NvidiaNotificationWindow(this);
                window.Closing += Window_Closing;
            }

            window.ShowNotif(type, title, description);

            return true;
        }

        public void ShowPreview()
        {
            if (window == null)
            {
                window = new NvidiaNotificationWindow(this);
                window.Closing += Window_Closing;
            }

            window.ShowPreview();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (sender as NvidiaNotificationWindow).Closing -= Window_Closing;

            if (window == sender)
                window = null;
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

        public void OpenCustomSettings() { }

        public string GetCustomSettingsDataToSave() => null;

        public string GetFixedAdditionalData()
        {
            return Utils.ConfigFixString<NvidiaCustomAnimationConfig>(_moduleSettings.AdditionalData);
        }

        public void Log(string txt)
        {
            logWriter.Invoke(txt);
        }

        public void Log(Exception ex)
        {
            Log($"Exception:\n{ex.Message}\nStackTrace:\n{ex.StackTrace}");
        }
    }
}

