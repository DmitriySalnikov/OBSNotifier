using OBSNotifier.Modules.UserControls;
using System.Windows;

namespace OBSNotifier.Modules.Event.Default
{
    internal partial class DefaultNotificationWindow : ModuleWindow
    {
        readonly DefaultNotification owner;
        bool isPreviewNotif = false;

        readonly DeferredActionWPF hide_delay;

        bool IsPositionedOnTop { get => owner.SettingsTyped.Option == DefaultNotification.Positions.TopLeft || owner.SettingsTyped.Option == DefaultNotification.Positions.TopRight; }

        public DefaultNotificationWindow(DefaultNotification module) : base()
        {
            InitializeComponent();

            owner = module;
            hide_delay = new DeferredActionWPF(() => Hide(), 200, this);
            sp_main_panel.Children.Clear();
        }

        protected override void OnModuleDpiChanged()
        {
            UpdateParameters();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = this.GetHandle();
            UtilsWinApi.SetWindowIgnoreFocus(hwnd, true);
            UtilsWinApi.SetWindowTopmost(hwnd, true);
        }

        protected override void OnClosed(EventArgs e)
        {
            hide_delay.Dispose();

            base.OnClosed(e);
        }

        void UpdateParameters()
        {
            UtilsWinApi.SetWindowIgnoreMouse(this.GetHandle(), owner.SettingsTyped.ClickThrough && !owner.SettingsTyped.ShowQuickActions);

            var maxSize = WPFScreens.GetScreenFrom(this).DeviceBounds;
            var newSize = new Size(
               Utils.Clamp(owner.SettingsTyped.BlockSize.Width, 1, maxSize.Width),
               Utils.Clamp(owner.SettingsTyped.BlockSize.Height * owner.SettingsTyped.Blocks, 1, maxSize.Height));
            owner.SettingsTyped.OnScreenTime = owner.SettingsTyped.OnScreenTime;

            // Position
            var pe = owner.SettingsTyped.Option;
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), pe.ToString());
            Point pos = Utils.GetModuleWindowPosition(this, owner.SettingsTyped.DisplayID, anchor, new Size(newSize.Width, newSize.Height), owner.SettingsTyped.Offset, owner.SettingsTyped.UseSafeDisplayArea);

            sp_main_panel.VerticalAlignment = IsPositionedOnTop ? VerticalAlignment.Stretch : VerticalAlignment.Bottom;

            sp_main_panel.Width = newSize.Width;
            sp_main_panel.Height = newSize.Height;

            if (!this.MoveModuleWindow(owner.SettingsTyped.DisplayID, pos.X, pos.Y, newSize.Width, newSize.Height, true))
            {
                Left = pos.X;
                Top = pos.Y;
                Width = newSize.Width;
                Height = newSize.Height;
            }
        }

        public void ShowNotif(NotificationType type, string title, string desc)
        {
            if (isPreviewNotif)
                return;

            if (sp_main_panel.Children.Count < owner.SettingsTyped.Blocks)
            {
                var nnb = new DefaultNotificationBlock();
                nnb.Finished += NotificationBlock_Animation_Finished;

                if (IsPositionedOnTop)
                    sp_main_panel.Children.Add(nnb);
                else
                    sp_main_panel.Children.Insert(0, nnb);
            }

            DefaultNotificationBlock nb;
            if (IsPositionedOnTop)
                nb = (DefaultNotificationBlock)sp_main_panel.Children[^1];
            else
                nb = (DefaultNotificationBlock)sp_main_panel.Children[0];

            sp_main_panel.Children.Remove(nb);
            if (IsPositionedOnTop)
                sp_main_panel.Children.Insert(0, nb);
            else
                sp_main_panel.Children.Add(nb);

            UpdateParameters();
            nb.SetupNotif(owner.SettingsTyped, type, title, desc);
            ShowWithLocationFix();
        }

        public void ShowPreview()
        {
            isPreviewNotif = true;
            UpdateParameters();
            UpdatePreviewBlocksCount();

            if (IsPositionedOnTop)
                for (int i = 0; i < sp_main_panel.Children.Count; i++)
                    ((DefaultNotificationBlock)sp_main_panel.Children[i]).ShowPreview(owner.SettingsTyped, (sp_main_panel.Children.Count - (double)i) / sp_main_panel.Children.Count);
            else
                for (int i = sp_main_panel.Children.Count - 1; i >= 0; i--)
                    ((DefaultNotificationBlock)sp_main_panel.Children[i]).ShowPreview(owner.SettingsTyped, ((double)i + 1) / sp_main_panel.Children.Count);

            ShowWithLocationFix();
        }

        public void HidePreview()
        {
            if (isPreviewNotif)
            {
                isPreviewNotif = false;

                foreach (DefaultNotificationBlock c in sp_main_panel.Children)
                {
                    c.Dispose();
                }
                sp_main_panel.Children.Clear();

                hide_delay.CallDeferred();
            }
        }

        void ShowWithLocationFix()
        {
            hide_delay.Cancel();
            Show();
            if (!IsPositionedOnTop)
            {
                var delta = Height - ActualHeight;
                if (delta > 0)
                    Top += delta;
            }
        }

        void UpdatePreviewBlocksCount()
        {
            while (sp_main_panel.Children.Count < owner.SettingsTyped.Blocks)
            {
                var nnb = new DefaultNotificationBlock();
                nnb.Finished += NotificationBlock_Animation_Finished;
                sp_main_panel.Children.Add(nnb);
            }

            while (sp_main_panel.Children.Count > owner.SettingsTyped.Blocks)
            {
                DefaultNotificationBlock nb;
                if (IsPositionedOnTop)
                    nb = (DefaultNotificationBlock)sp_main_panel.Children[^1];
                else
                    nb = (DefaultNotificationBlock)sp_main_panel.Children[0];

                sp_main_panel.Children.Remove(nb);
                nb.Finished -= NotificationBlock_Animation_Finished;
                nb.Dispose();
            }
        }

        private void NotificationBlock_Animation_Finished(object? sender, EventArgs e)
        {
            // hide window if no visible blocks
            foreach (DefaultNotificationBlock c in sp_main_panel.Children)
            {
                if (c.Visibility == Visibility.Visible)
                    return;
            }
            hide_delay.CallDeferred();
        }
    }
}
