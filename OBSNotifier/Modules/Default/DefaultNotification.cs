using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace OBSNotifier.Modules.Default
{
    [Export(typeof(IOBSNotifierModule))]
    internal partial class DefaultNotification : IOBSNotifierModule
    {
        internal enum Positions
        {
            TopLeft, TopRight, BottomLeft, BottomRight,
            //Left, Right, Top, Bottom,
            Center
        }

        Action<string> logWriter = null;
        DefaultNotificationWindow window = null;

        public string ModuleID => "Default";
        public string ModuleName => Utils.Tr("default_module_name");

        public string ModuleAuthor => "Dmitriy Salnikov";

        public string ModuleDescription => Utils.Tr("default_module_desc");

        public AvailableModuleSettings DefaultAvailableSettings => AvailableModuleSettings.AllNoCustomSettings;

        OBSNotifierModuleSettings _moduleSettings = new OBSNotifierModuleSettings()
        {
            UseSafeDisplayArea = true,
            AdditionalData = "BackgroundColor = #FF4C4C4C\nOutlineColor = #59000000\nTextColor = #FFD8D8D8\nBlocks = 3\nRadius = 4.0\nWidth = 180.0\nHeight = 52.0\nMargin = 4, 4, 4, 4\nMaxPathChars = 32\nShowQuickActionsOnFileSave = True",
            Option = Positions.BottomRight,
            Offset = new Point(0, 0),
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
                window = new DefaultNotificationWindow(this);
                window.Closing += Window_Closing;
            }

            window.ShowNotif(type, title, description);
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

        public void OpenCustomSettings() { }

        public string GetCustomSettingsDataToSave() => null;

        public string GetFixedAdditionalData()
        {
            return Utils.ConfigFixString<DefaultCustomNotifBlockSettings>(_moduleSettings.AdditionalData);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (sender as DefaultNotificationWindow).Closing -= Window_Closing;

            if (window == sender)
                window = null;
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

