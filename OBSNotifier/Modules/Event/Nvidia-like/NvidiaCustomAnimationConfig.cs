using System.Windows;
using System.Windows.Media;

namespace OBSNotifier.Modules.Event.NvidiaLike
{
    internal class NvidiaCustomAnimationConfig : OBSModuleSettings
    {
        double onScreenTime = 5.0;
        double iconHeight = 64;
        double scale = 1.0;
        double lineWidth = 6;
        double openFileOffset = 8.0;
        double slideDuration = 0.4;
        double slideOffset = 0.18;
        uint maxPathChars = 32;

        [SettingsItemIgnore]
        public bool IsOnRightSide { get; set; } = false;
        [SettingsItemIgnore]
        public bool IsPreviewNotif { get; set; } = false;

        [SettingsItemStringDisplayID]
        public string DisplayID { get; set; } = string.Empty;
        public bool UseSafeDisplayArea { get; set; }
        // TODO test loading with greater values
        [SettingsItemNumberRange(0, 30, 0.1)]
        public double OnScreenTime
        {
            get => onScreenTime;
            set => onScreenTime = Utils.Clamp(value, 0, 30);
        }
        public NvidiaNotification.Positions Option { get; set; }

        [SettingsItemNumberRange(0, 1, 0.01)]
        public Point Offset { get; set; }

        [SettingsItemCategory("Colors")]
        public Color BackgroundColor { get; set; } = (Color)ColorConverter.ConvertFromString("#2E48BD");
        public Color ForegroundColor { get; set; } = Colors.Black;
        public Color TextColor { get; set; } = (Color)ColorConverter.ConvertFromString("#E4E4E4");

        [SettingsItemCategory("Animation")]
        [SettingsItemNumberRange(0, 15, 0.1)]
        public double SlideDuration
        {
            get => slideDuration;
            set => slideDuration = Utils.Clamp(value, 0, 15);
        }
        [SettingsItemNumberRange(0, 5, 0.1)]
        public double SlideOffset
        {
            get => slideOffset;
            set => slideOffset = Utils.Clamp(value, 0, 5);
        }

        // TODO broken
        [SettingsItemNumberRange(0, 100)]
        public double LineWidth
        {
            get => lineWidth;
            set => lineWidth = Utils.Clamp(value, 0, 100);
        }
        [SettingsItemNumberRange(0.001, 5, 0.1)]
        public double Scale
        {
            get => scale;
            set => scale = Utils.Clamp(value, 0.001, 5);
        }

        [SettingsItemCategory("Quick Actions")]
        [SettingsItemNumberRange(0, 256)]
        public uint MaxPathChars
        {
            get => maxPathChars;
            set => maxPathChars = (uint)Utils.Clamp(value, 0, 256);
        }
        public bool ClickThrough { get; set; } = false;
        public bool ShowQuickActions { get; set; } = true;
        public bool ShowQuickActionsColoredLine { get; set; } = true;
        [SettingsItemNumberRange(0, 2048)]
        public double QuickActionsOffset
        {
            get => openFileOffset;
            set => openFileOffset = Utils.Clamp(value, 0, 2048);
        }

        [SettingsItemCategory("Icon")]
        [SettingsItemNumberRange(0, 192)]
        public double IconHeight
        {
            get => iconHeight;
            set => iconHeight = Utils.Clamp(value, 0, 192);
        }
        [SettingsItemStringPath(DefaultExt = ".png", FileFilter = "Images|*.png; *.jpg; *.jpeg; *.bmp; *.tif; *.gif; *.ico|All Files|*.*", IsFile = true)]
        public string IconPath { get; set; } = "INVALID_PATH";

        public bool IsAnimParamsEqual(NvidiaCustomAnimationConfig b)
        {
            if (b == null)
                return false;

            return IsPreviewNotif.Equals(b.IsPreviewNotif) &&
                OnScreenTime.Equals(b.OnScreenTime) &&
                SlideDuration.Equals(b.SlideDuration) &&
                SlideOffset.Equals(b.SlideOffset) &&
                IsOnRightSide.Equals(b.IsOnRightSide) &&
                LineWidth.Equals(b.LineWidth);
        }

        public override OBSModuleSettings Clone()
        {
            return new NvidiaCustomAnimationConfig()
            {
                OnScreenTime = OnScreenTime,
                IsOnRightSide = IsOnRightSide,
                IsPreviewNotif = IsPreviewNotif,

                BackgroundColor = BackgroundColor,
                ForegroundColor = ForegroundColor,
                TextColor = TextColor,

                SlideDuration = SlideDuration,
                SlideOffset = SlideOffset,

                LineWidth = LineWidth,
                Scale = Scale,

                MaxPathChars = MaxPathChars,
                ClickThrough = ClickThrough,
                ShowQuickActions = ShowQuickActions,
                ShowQuickActionsColoredLine = ShowQuickActionsColoredLine,
                QuickActionsOffset = QuickActionsOffset,

                IconHeight = IconHeight,
                IconPath = IconPath,
            };
        }
    }
}
