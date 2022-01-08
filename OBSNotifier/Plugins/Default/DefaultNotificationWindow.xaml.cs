using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OBSNotifier.Plugins.Default
{
    public partial class DefaultNotificationWindow : Window
    {
        public struct NotifBlockSettings
        {
            public Color Background;
            public Color Foreground;
            public Color Outline;
            public double Radius;
            public double Width;
            public double Height;
            public Thickness Margin;
        }

        public DefaultNotification owner = null;
        int addDataHash = 0;

        public bool IsPreviewNotif = false;
        int VerticalBlocksCount = 1;

        public static readonly RoutedEvent FadeInOutEvent = EventManager.RegisterRoutedEvent("FadeInOut", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(DefaultNotificationWindow));
        public event RoutedEventHandler FadeInOut { add => AddHandler(FadeInOutEvent, value); remove => RemoveHandler(FadeInOutEvent, value); }

        //DeferredAction gc_collect = new DeferredAction(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }, 1000);
        bool IsPositionedOnTop { get => (DefaultNotification.Positions)owner.PluginSettings.Option == DefaultNotification.Positions.TopLeft || (DefaultNotification.Positions)owner.PluginSettings.Option == DefaultNotification.Positions.TopRight; }

        NotifBlockSettings CurrentNotifBlockSettings;
        readonly NotifBlockSettings DefaultNotifBlockSettings = new NotifBlockSettings()
        {
            Background = (Color)ColorConverter.ConvertFromString("#4C4C4C"),
            Foreground = (Color)ColorConverter.ConvertFromString("#D8D8D8"),
            Outline = (Color)ColorConverter.ConvertFromString("#59000000"),
            Radius = 4,
            Width = 180,
            Height = 52,
            Margin = new Thickness(4),
        };

        public DefaultNotificationWindow(DefaultNotification plugin)
        {
            InitializeComponent();

            sp_main_panel.Children.Clear();
            CurrentNotifBlockSettings = DefaultNotifBlockSettings;
            owner = plugin;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        void UpdateWindow()
        {
            // Additional Params
            if (owner.PluginSettings.AdditionalData.GetHashCode() != addDataHash)
            {
                addDataHash = owner.PluginSettings.AdditionalData.GetHashCode();

                var lines = owner.PluginSettings.AdditionalData.Replace("\r", "").Split('\n');
                foreach (var line in lines)
                {
                    var args = line.Split('=');
                    if (args.Length == 2)
                    {
                        switch (args[0].Trim())
                        {
                            case "Blocks":
                                {
                                    if (int.TryParse(args[1].Trim(), out int val))
                                    {
                                        VerticalBlocksCount = val;
                                        Height = val * CurrentNotifBlockSettings.Height;
                                    }
                                    else
                                    {
                                        VerticalBlocksCount = 3;
                                        Height = CurrentNotifBlockSettings.Height * VerticalBlocksCount;
                                    }
                                    break;
                                }
                            case "BackgroundColor":
                                {
                                    try
                                    {
                                        CurrentNotifBlockSettings.Background = (Color)ColorConverter.ConvertFromString(args[1].Trim());
                                    }
                                    catch
                                    {
                                        CurrentNotifBlockSettings.Background = DefaultNotifBlockSettings.Background;
                                    }
                                    break;
                                }
                            case "ForegroundColor":
                                {
                                    try
                                    {
                                        CurrentNotifBlockSettings.Foreground = (Color)ColorConverter.ConvertFromString(args[1].Trim());
                                    }
                                    catch
                                    {
                                        CurrentNotifBlockSettings.Foreground = DefaultNotifBlockSettings.Foreground;
                                    }
                                    break;
                                }
                            case "OutlineColor":
                                {
                                    try
                                    {
                                        CurrentNotifBlockSettings.Outline = (Color)ColorConverter.ConvertFromString(args[1].Trim());
                                    }
                                    catch
                                    {
                                        CurrentNotifBlockSettings.Outline = DefaultNotifBlockSettings.Outline;
                                    }
                                    break;
                                }
                            case "Radius":
                                {
                                    if (double.TryParse(args[1].Trim(), out double val))
                                    {
                                        CurrentNotifBlockSettings.Radius = val;
                                    }
                                    else
                                    {
                                        CurrentNotifBlockSettings.Radius = DefaultNotifBlockSettings.Radius;
                                    }
                                    break;
                                }
                            case "Width":
                                {
                                    if (int.TryParse(args[1].Trim(), out int val))
                                    {
                                        CurrentNotifBlockSettings.Width = val;
                                        Width = val;
                                    }
                                    else
                                    {
                                        CurrentNotifBlockSettings.Width = DefaultNotifBlockSettings.Width;
                                        Width = CurrentNotifBlockSettings.Width;
                                    }
                                    break;
                                }
                            case "Height":
                                {
                                    if (int.TryParse(args[1].Trim(), out int val))
                                    {
                                        CurrentNotifBlockSettings.Height = val;
                                        Height = val * VerticalBlocksCount;
                                    }
                                    else
                                    {
                                        CurrentNotifBlockSettings.Height = DefaultNotifBlockSettings.Height;
                                        Height = DefaultNotifBlockSettings.Height * VerticalBlocksCount;
                                    }
                                    break;
                                }
                            case "Margin":
                                {
                                    var split = args[1].Trim().Split(',');
                                    if (split.Length == 1)
                                    {
                                        if (double.TryParse(split[0], out double val))
                                            CurrentNotifBlockSettings.Margin = new Thickness(val);
                                    }
                                    else if (split.Length == 4)
                                    {
                                        if (double.TryParse(split[0], out double val1) &&
                                            double.TryParse(split[1], out double val2) &&
                                            double.TryParse(split[2], out double val3) &&
                                            double.TryParse(split[3], out double val4))
                                        {
                                            CurrentNotifBlockSettings.Margin = new Thickness(val1, val2, val3, val4);
                                        }
                                    }
                                    else
                                    {
                                        CurrentNotifBlockSettings.Margin = DefaultNotifBlockSettings.Margin;
                                    }
                                    break;
                                }
                        }
                    }
                }

                RemoveUnusedBlocks();
            }

            // Position
            var pe = (DefaultNotification.Positions)owner.PluginSettings.Option;
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), pe.ToString());
            Point pos = Utils.GetWindowPosition(anchor, new Size(Width, Height), owner.PluginSettings.Offset);

            sp_main_panel.VerticalAlignment = IsPositionedOnTop ? VerticalAlignment.Stretch : VerticalAlignment.Bottom;

            Left = pos.X;
            Top = pos.Y;
        }

        public void ShowNotif(string title, string desc)
        {
            IsPreviewNotif = false;

            if (sp_main_panel.Children.Count < VerticalBlocksCount)
            {
                var nnb = new DefaultNotificationBlock(this);
                nnb.Finished += Nb_Finished;

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

            UpdateWindow();
            nb.SetupNotif(CurrentNotifBlockSettings, title, desc);
            Show();
        }

        void RemoveUnusedBlocks()
        {
            while (sp_main_panel.Children.Count > VerticalBlocksCount)
            {
                DefaultNotificationBlock nb;
                if (IsPositionedOnTop)
                    nb = sp_main_panel.Children[sp_main_panel.Children.Count - 1] as DefaultNotificationBlock;
                else
                    nb = sp_main_panel.Children[0] as DefaultNotificationBlock;

                sp_main_panel.Children.Remove(nb);
                nb.Finished -= Nb_Finished;
                nb.Dispose();
            }
        }

        private void Nb_Finished(object sender, EventArgs e)
        {
            var nb = sender as DefaultNotificationBlock;
            nb.HideNotif();

            // hide window if no visible blocks
            foreach (DefaultNotificationBlock c in sp_main_panel.Children)
            {
                if (c.Visibility == Visibility.Visible)
                    return;
            }
            Hide();
        }

        public void ShowPreview()
        {
            IsPreviewNotif = true;
            UpdateWindow();
            Show();
        }
    }
}
