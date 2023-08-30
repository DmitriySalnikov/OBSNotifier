using System;
using System.Windows;

namespace OBSNotifier.Modules.Default
{
    internal partial class DefaultNotificationWindow : Window
    {
        DefaultNotification owner = null;
        int addDataHash = -1;
        bool isPreviewNotif = false;

        DeferredAction hide_delay;
        DefaultCustomNotifBlockSettings currentNotifBlockSettings;

        bool IsPositionedOnTop { get => (DefaultNotification.Positions)owner.ModuleSettings.Option == DefaultNotification.Positions.TopLeft || (DefaultNotification.Positions)owner.ModuleSettings.Option == DefaultNotification.Positions.TopRight; }

        public DefaultCustomNotifBlockSettings CurrentNotifBlockSettings { get => currentNotifBlockSettings; private set => currentNotifBlockSettings = value; }
        readonly DefaultCustomNotifBlockSettings defaultNotifBlockSettings = new DefaultCustomNotifBlockSettings();

        public DefaultNotificationWindow(DefaultNotification module)
        {
            InitializeComponent();

            hide_delay = new DeferredAction(() => Hide(), 200, this);
            sp_main_panel.Children.Clear();
            CurrentNotifBlockSettings = defaultNotifBlockSettings;
            owner = module;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.RemoveWindowFromAltTab(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            RemoveUnusedBlocks();
            owner = null;
            hide_delay.Dispose();

            base.OnClosed(e);
        }

        void UpdateParameters()
        {
            // Additional Params
            if (owner.ModuleSettings.AdditionalData != null && owner.ModuleSettings.AdditionalData.GetHashCode() != addDataHash)
            {
                addDataHash = owner.ModuleSettings.AdditionalData.GetHashCode();

                CurrentNotifBlockSettings = defaultNotifBlockSettings;
                Utils.ConfigParseString(owner.ModuleSettings.AdditionalData, ref currentNotifBlockSettings);

                RemoveUnusedBlocks();
            }

            CurrentNotifBlockSettings.Duration = owner.ModuleSettings.OnScreenTime;
            Height = CurrentNotifBlockSettings.Height * CurrentNotifBlockSettings.Blocks;
            Width = CurrentNotifBlockSettings.Width;

            // Position
            var pe = (DefaultNotification.Positions)owner.ModuleSettings.Option;
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), pe.ToString());
            Point pos = Utils.GetWindowPosition(anchor, new Size(Width, Height), owner.ModuleSettings.Offset);

            sp_main_panel.VerticalAlignment = IsPositionedOnTop ? VerticalAlignment.Stretch : VerticalAlignment.Bottom;

            Left = pos.X;
            Top = pos.Y;
        }

        public void ShowNotif(NotificationType type, string title, string desc)
        {
            if (isPreviewNotif)
                return;

            if (sp_main_panel.Children.Count < CurrentNotifBlockSettings.Blocks)
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
                nb = sp_main_panel.Children[sp_main_panel.Children.Count - 1] as DefaultNotificationBlock;
            else
                nb = sp_main_panel.Children[0] as DefaultNotificationBlock;

            sp_main_panel.Children.Remove(nb);
            if (IsPositionedOnTop)
                sp_main_panel.Children.Insert(0, nb);
            else
                sp_main_panel.Children.Add(nb);

            UpdateParameters();
            nb.SetupNotif(CurrentNotifBlockSettings, type, title, desc);
            ShowWithLocationFix();
        }

        public void ShowPreview()
        {
            isPreviewNotif = true;
            UpdateParameters();
            CreateMissingBlocks();

            if (IsPositionedOnTop)
                for (int i = 0; i < sp_main_panel.Children.Count; i++)
                    (sp_main_panel.Children[i] as DefaultNotificationBlock).ShowPreview(CurrentNotifBlockSettings, (sp_main_panel.Children.Count - (double)i) / sp_main_panel.Children.Count);
            else
                for (int i = sp_main_panel.Children.Count - 1; i >= 0; i--)
                    (sp_main_panel.Children[i] as DefaultNotificationBlock).ShowPreview(CurrentNotifBlockSettings, ((double)i + 1) / sp_main_panel.Children.Count);

            ShowWithLocationFix();
        }

        public void HidePreview()
        {
            if (isPreviewNotif)
            {
                isPreviewNotif = false;

                foreach (DefaultNotificationBlock c in sp_main_panel.Children)
                    c.HidePreview();
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

        void RemoveUnusedBlocks()
        {
            while (sp_main_panel.Children.Count > CurrentNotifBlockSettings.Blocks)
            {
                DefaultNotificationBlock nb;
                if (IsPositionedOnTop)
                    nb = sp_main_panel.Children[sp_main_panel.Children.Count - 1] as DefaultNotificationBlock;
                else
                    nb = sp_main_panel.Children[0] as DefaultNotificationBlock;

                sp_main_panel.Children.Remove(nb);
                nb.Finished -= NotificationBlock_Animation_Finished;
                nb.Dispose();
            }
        }

        void CreateMissingBlocks()
        {
            while (sp_main_panel.Children.Count < CurrentNotifBlockSettings.Blocks)
            {
                var nnb = new DefaultNotificationBlock();
                nnb.Finished += NotificationBlock_Animation_Finished;
                sp_main_panel.Children.Add(nnb);
            }
        }

        private void NotificationBlock_Animation_Finished(object sender, EventArgs e)
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
