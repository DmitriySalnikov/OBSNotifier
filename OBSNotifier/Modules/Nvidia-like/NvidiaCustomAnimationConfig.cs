using System;
using System.Windows.Media;

namespace OBSNotifier.Modules.NvidiaLike
{
    class NvidiaCustomAnimationConfig
    {
        uint duration;
        double iconHeight;
        double scale;
        double lineWidth;
        double openFileOffset;

        [ConfigIgnore]
        public uint Duration { get { return duration; } set { duration = Math.Max(0, value); } }
        [ConfigIgnore]
        public bool IsOnRightSide;
        [ConfigIgnore]
        public bool IsPreviewNotif;

        public SolidColorBrush BackgroundColor { get; set; }
        public SolidColorBrush ForegroundColor { get; set; }
        public SolidColorBrush TextColor { get; set; }

        public uint SlideDuration { get; set; }
        public uint SlideOffset { get; set; }

        public double LineWidth { get => lineWidth; set => lineWidth = Math.Max(0, value); }
        public double Scale { get => scale; set => scale = Math.Max(0.001, value); }

        public uint MaxPathChars { get; set; }
        public bool ShowQuickActions { get; set; }
        public bool ShowQuickActionsColoredLine { get; set; }
        public double QuickActionsOffset { get => openFileOffset; set => openFileOffset = Math.Max(0, value); }

        public double IconHeight { get => iconHeight; set => iconHeight = Math.Max(0, value); }
        public string IconPath { get; set; }

        public NvidiaCustomAnimationConfig()
        {
            Duration = 2500;
            IsOnRightSide = false;
            IsPreviewNotif = false;

            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E48BD"));
            ForegroundColor = new SolidColorBrush(Colors.Black);
            TextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4E4E4"));

            SlideDuration = 400;
            SlideOffset = 180;

            LineWidth = 6;
            Scale = 1.0;

            MaxPathChars = 32;
            ShowQuickActions = true;
            ShowQuickActionsColoredLine = true;
            QuickActionsOffset = 8.0;

            IconHeight = 64;
            IconPath = "INVALID_PATH";
        }

        public bool IsAnimParamsEqual(NvidiaCustomAnimationConfig b)
        {
            if (b == null)
                return false;

            return IsPreviewNotif.Equals(b.IsPreviewNotif) &&
                Duration.Equals(b.Duration) &&
                SlideDuration.Equals(b.SlideDuration) &&
                SlideOffset.Equals(b.SlideOffset) &&
                IsOnRightSide.Equals(b.IsOnRightSide) &&
                LineWidth.Equals(b.LineWidth);
        }

        public NvidiaCustomAnimationConfig Duplicate()
        {
            return new NvidiaCustomAnimationConfig()
            {
                Duration = this.Duration,
                IsOnRightSide = this.IsOnRightSide,
                IsPreviewNotif = this.IsPreviewNotif,

                BackgroundColor = this.BackgroundColor,
                ForegroundColor = this.ForegroundColor,
                TextColor = this.TextColor,

                SlideDuration = this.SlideDuration,
                SlideOffset = this.SlideOffset,

                LineWidth = this.LineWidth,
                Scale = this.Scale,

                MaxPathChars = this.MaxPathChars,
                ShowQuickActions = this.ShowQuickActions,
                ShowQuickActionsColoredLine = this.ShowQuickActionsColoredLine,
                QuickActionsOffset = this.QuickActionsOffset,

                IconHeight = this.IconHeight,
                IconPath = this.IconPath,
            };
        }
    }
}
