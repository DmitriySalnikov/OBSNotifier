using System;
using System.Windows;
using System.Windows.Media;

namespace OBSNotifier.Modules.Event.Default
{
    internal class DefaultCustomNotifBlockSettings : OBSModuleSettings
    {
        double onScreenDuration = 4.0;
        uint blocks = 3;
        double borderRadius = 4;
        double borderThickness = 1;
        uint width = 180;
        uint height = 52;

        [SettingsItemStringDisplayID]
        public string DisplayID { get; set; } = string.Empty;
        public bool UseSafeDisplayArea { get; set; }
        [SettingsItemNumberRange(0, 30, 0.1)]
        public double OnScreenTime
        {
            get => onScreenDuration;
            set => onScreenDuration = Utils.Clamp(value, 0, 30);
        }
        public DefaultNotification.Positions Option { get; set; }

        [SettingsItemNumberRange(0, 1, 0.01)]
        public Point Offset { get; set; }

        public Color BackgroundColor { get; set; } = (Color)ColorConverter.ConvertFromString("#4C4C4C");
        public Color OutlineColor { get; set; } = (Color)ColorConverter.ConvertFromString("#59000000");
        public Color TextColor { get; set; } = (Color)ColorConverter.ConvertFromString("#D8D8D8");

        [SettingsItemNumberRange(1, 24)]
        public uint Blocks
        {
            get => blocks;
            set => blocks = (uint)Utils.Clamp(value, 1, 24);
        }

        [SettingsItemNumberRange(1, 2048)]
        public uint Width
        {
            get => width;
            set => width = (uint)Utils.Clamp(value, 1, 2048);
        }
        [SettingsItemNumberRange(1, 2048)]
        public uint Height
        {
            get => height;
            set => height = (uint)Utils.Clamp(value, 1, 2048);
        }

        [SettingsItemNumberRange(0, 500)]
        public double BorderRadius
        {
            get => borderRadius;
            set => borderRadius = Utils.Clamp(value, 0, 500);
        }

        [SettingsItemNumberRange(0, 64, 0.5)]
        public double BorderThickness
        {
            get => borderThickness;
            set => borderThickness = Utils.Clamp(value, 0, 64);
        }

        [SettingsItemNumberRange(0, 1024, 0.1)]
        public Thickness Margin { get; set; } = new Thickness(4);
        public bool ClickThrough { get; set; } = false;

        [SettingsItemNumberRange(0, 256)]
        public uint MaxPathChars { get; set; } = 32;
        public bool ShowQuickActions { get; set; } = true;

        public override OBSModuleSettings Clone()
        {
            return new DefaultCustomNotifBlockSettings()
            {
                DisplayID = DisplayID,
                UseSafeDisplayArea = UseSafeDisplayArea,
                OnScreenTime = OnScreenTime,
                Option = Option,
                Offset = Offset,

                BackgroundColor = BackgroundColor,
                OutlineColor = OutlineColor,
                TextColor = TextColor,

                Blocks = Blocks,
                BorderRadius = BorderRadius,
                Width = Width,
                Height = Height,
                Margin = Margin,

                ClickThrough = ClickThrough,

                MaxPathChars = MaxPathChars,
                ShowQuickActions = ShowQuickActions,
            };
        }
    }
}
