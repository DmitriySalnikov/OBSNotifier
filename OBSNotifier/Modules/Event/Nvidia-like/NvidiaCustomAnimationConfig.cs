using System.Windows;
using System.Windows.Media;

namespace OBSNotifier.Modules.Event.NvidiaLike
{
    internal class NvidiaCustomAnimationConfig : OBSModuleSettings
    {
        bool isOnRightSide = false;
        bool isPreviewNotif = false;
        NotificationType activeNotifications = NotificationType.All;
        string displayID = string.Empty;
        bool useSafeDisplayArea = false;
        NvidiaNotification.Positions option = NvidiaNotification.Positions.TopRight;
        Point offset = new(0, 0.1);
        bool clickThrough = false;

        // colors
        Color backgroundColor = (Color)ColorConverter.ConvertFromString("#2E48BD");
        Color foregroundColor = Colors.Black;
        Color textColor = (Color)ColorConverter.ConvertFromString("#E4E4E4");

        // animation
        double onScreenTime = 5.0;
        double slideDuration = 0.4;
        double slideOffset = 0.18;
        double lineWidth = 6;
        double scale = 1.0;

        // quick actions
        uint maxPathChars = 32;
        bool showQuickActions = true;
        bool showQuickActionsColoredLine = true;
        double quickActionsOffset = 8.0;

        // icon
        double iconHeight = 64;
        string iconPath = "INVALID_PATH";

        [SettingsItemIgnore]
        public bool IsOnRightSide { get => isOnRightSide; set => isOnRightSide = value; }
        [SettingsItemIgnore]
        public bool IsPreviewNotif { get => isPreviewNotif; set => isPreviewNotif = value; }

        public NotificationType ActiveNotifications { get => activeNotifications; set => activeNotifications = value; }

        [SettingsItemStringDisplayID]
        public string DisplayID { get => displayID; set => displayID = value; }

        public NvidiaNotification.Positions Option { get => option; set => option = value; }

        [SettingsItemNumberRange(0, 1, 0.01)]
        public Point Offset { get => offset; set => offset = value; }
        public bool UseSafeDisplayArea { get => useSafeDisplayArea; set => useSafeDisplayArea = value; }

        [SettingsItemHint("module_common_setting_clickthrough_hint")]
        public bool ClickThrough { get => clickThrough; set => clickThrough = value; }

        [SettingsItemCategory("module_common_setting_category_colors")]
        public Color BackgroundColor { get => backgroundColor; set => backgroundColor = value; }
        public Color ForegroundColor { get => foregroundColor; set => foregroundColor = value; }
        public Color TextColor { get => textColor; set => textColor = value; }

        [SettingsItemCategory("module_common_setting_category_animation")]
        [SettingsItemNumberRange(0, 30, 0.1)]
        public double OnScreenTime { get => onScreenTime; set => onScreenTime = Utils.Clamp(value, 0, 30); }

        [SettingsItemNumberRange(0, 15, 0.1)]
        public double SlideDuration { get => slideDuration; set => slideDuration = Utils.Clamp(value, 0, 15); }

        [SettingsItemNumberRange(0, 5, 0.1)]
        public double SlideOffset { get => slideOffset; set => slideOffset = Utils.Clamp(value, 0, 5); }

        [SettingsItemNumberRange(0, 100)]
        public double LineWidth { get => lineWidth; set => lineWidth = Utils.Clamp(value, 0, 100); }

        [SettingsItemNumberRange(0.001, 5, 0.1)]
        public double Scale { get => scale; set => scale = Utils.Clamp(value, 0.001, 5); }

        [SettingsItemCategory("module_common_setting_category_quick_actions")]
        [SettingsItemNumberRange(0, 256)]
        public uint MaxPathChars { get => maxPathChars; set => maxPathChars = (uint)Utils.Clamp(value, 0, 256); }
        public bool ShowQuickActions { get => showQuickActions; set => showQuickActions = value; }
        public bool ShowQuickActionsColoredLine { get => showQuickActionsColoredLine; set => showQuickActionsColoredLine = value; }

        [SettingsItemNumberRange(0, 2048)]
        public double QuickActionsOffset { get => quickActionsOffset; set => quickActionsOffset = Utils.Clamp(value, 0, 2048); }

        [SettingsItemCategory("module_common_setting_category_icon")]
        [SettingsItemNumberRange(0, 192)]
        public double IconHeight { get => iconHeight; set => iconHeight = Utils.Clamp(value, 0, 192); }

        [SettingsItemStringPath(DefaultExt = ".png", FileFilter = "Images|*.png; *.jpg; *.jpeg; *.bmp; *.tif; *.gif; *.ico|All Files|*.*", IsFile = true)]
        public string IconPath { get => iconPath; set => iconPath = value; }

        public bool IsAnimParamsEqual(NvidiaCustomAnimationConfig? b)
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
                isOnRightSide = isOnRightSide,
                isPreviewNotif = isPreviewNotif,
                activeNotifications = activeNotifications,
                displayID = displayID,
                useSafeDisplayArea = useSafeDisplayArea,
                onScreenTime = onScreenTime,
                option = option,
                offset = offset,
                clickThrough = clickThrough,

                backgroundColor = backgroundColor,
                foregroundColor = foregroundColor,
                textColor = textColor,

                slideDuration = slideDuration,
                slideOffset = slideOffset,
                lineWidth = lineWidth,
                scale = scale,

                maxPathChars = maxPathChars,
                showQuickActions = showQuickActions,
                showQuickActionsColoredLine = showQuickActionsColoredLine,
                quickActionsOffset = quickActionsOffset,

                iconHeight = iconHeight,
                iconPath = iconPath,
            };
        }

        public override NotificationType GetActiveNotifications()
        {
            return ActiveNotifications;
        }
    }
}
