﻿namespace OBSNotifier.Modules.Event.NvidiaLike
{
    internal partial class NvidiaNotification : IOBSNotifierModule
    {
        internal enum Positions
        {
            TopLeft, TopRight,
        }

        OBSPerModuleAppInfo? moduleInfo = null;
        NvidiaNotificationWindow? window = null;

        public string ModuleID => "Nvidia-Like";
        public string ModuleName => Utils.Tr("module_nvidia_like_name");

        public string ModuleAuthor => "Dmitriy Salnikov";

        public string ModuleDescription => Utils.Tr("module_nvidia_like_desc");

        public NotificationType DefaultActiveNotifications => NotificationType.All;

        public NvidiaCustomAnimationConfig SettingsTyped { get; set; } = new NvidiaCustomAnimationConfig();

        public OBSModuleSettings Settings
        {
            get => SettingsTyped;
            set
            {
                if (value.GetType() != typeof(NvidiaCustomAnimationConfig))
                    throw new ArgumentException($"'value' is not a {typeof(NvidiaCustomAnimationConfig)}");
                SettingsTyped = (NvidiaCustomAnimationConfig)value;
            }
        }


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
                window = new NvidiaNotificationWindow(this);
                window.Closing += Window_Closing;
            }

            window.ShowNotif(type, title, description ?? "");

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

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is NvidiaNotificationWindow s)
                s.Closing -= Window_Closing;

            if (window == sender)
                window = null;
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

