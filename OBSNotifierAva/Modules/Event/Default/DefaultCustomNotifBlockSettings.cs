namespace OBSNotifier.Modules.Event.Default
{
    public class DefaultCustomNotifBlockSettings : OBSModuleSettings
    {
        NotificationType activeNotifications = NotificationType.All;
        uint displayID = 0;
        bool useSafeDisplayArea = true;
        DefaultNotification.Positions option = DefaultNotification.Positions.BottomRight;
        double onScreenDuration = 4.0;
        PixelPoint offset = new();
        bool clickThrough = false;

        Color backgroundColor = Color.Parse("#4C4C4C");
        Color outlineColor = Color.Parse("#59000000");
        Color textColor = Color.Parse("#D8D8D8");

        uint blocks = 3;
        Size blockSize = new(180, 52);
        double borderRadius = 4;
        double borderThickness = 1;

        Thickness margin = new(4);
        uint maxPathChars = 32;
        bool showQuickActions = true;

        public NotificationType ActiveNotifications { get => activeNotifications; set => activeNotifications = value; }

        [SettingsItemStringDisplayID]
        public uint DisplayID { get => displayID; set => displayID = value; }

        public DefaultNotification.Positions Option { get => option; set => option = value; }

        [SettingsItemNumberRange(0, 30, 0.1)]
        public double OnScreenTime { get => onScreenDuration; set => onScreenDuration = Utils.Clamp(value, 0, 30); }

        [SettingsItemNumberRange(0, 1, 0.01)]
        public PixelPoint Offset { get => offset; set => offset = value; }
        public bool UseSafeDisplayArea { get => useSafeDisplayArea; set => useSafeDisplayArea = value; }

        [SettingsItemHint("module_common_setting_clickthrough_hint")]
        public bool ClickThrough { get => clickThrough; set => clickThrough = value; }

        [SettingsItemCategory("module_common_setting_category_colors")]
        public Color BackgroundColor { get => backgroundColor; set => backgroundColor = value; }
        public Color OutlineColor { get => outlineColor; set => outlineColor = value; }
        public Color TextColor { get => textColor; set => textColor = value; }

        [SettingsItemCategory("module_default_setting_category_blocks")]
        [SettingsItemNumberRange(1, 24)]
        public uint Blocks { get => blocks; set => blocks = (uint)Utils.Clamp(value, 1, 24); }

        [SettingsItemNumberRangeMaxDisplaySize]
        public Size BlockSize { get => blockSize; set => blockSize = new(Utils.Clamp(value.Width, 1, 1024 * 16), Utils.Clamp(value.Height, 1, 1024 * 16)); }

        [SettingsItemNumberRange(0, 500)]
        public double BorderRadius { get => borderRadius; set => borderRadius = Utils.Clamp(value, 0, 500); }

        [SettingsItemNumberRange(0, 64, 0.5)]
        public double BorderThickness { get => borderThickness; set => borderThickness = Utils.Clamp(value, 0, 64); }

        [SettingsItemNumberRange(0, 1024, 0.1)]
        public Thickness Margin { get => margin; set => margin = value; }

        [SettingsItemCategory("module_common_setting_category_quick_actions")]
        [SettingsItemNumberRange(0, 256)]
        public uint MaxPathChars { get => maxPathChars; set => maxPathChars = value; }
        public bool ShowQuickActions { get => showQuickActions; set => showQuickActions = value; }

        public override OBSModuleSettings Clone()
        {
            return new DefaultCustomNotifBlockSettings()
            {
                activeNotifications = activeNotifications,
                displayID = displayID,
                useSafeDisplayArea = useSafeDisplayArea,
                onScreenDuration = onScreenDuration,
                option = option,
                offset = offset,
                clickThrough = clickThrough,

                backgroundColor = backgroundColor,
                outlineColor = outlineColor,
                textColor = textColor,

                blocks = blocks,
                blockSize = blockSize,
                borderRadius = borderRadius,
                borderThickness = borderThickness,

                margin = margin,
                maxPathChars = maxPathChars,
                showQuickActions = showQuickActions,
            };
        }

        public override NotificationType GetActiveNotifications()
        {
            return ActiveNotifications;
        }
    }
}
