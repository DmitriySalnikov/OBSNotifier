using System;
using System.Windows.Media;

namespace OBSNotifier.Modules.NvidiaLike
{
    class CustomAnimationConfig
    {
        uint duration;
        double iconHeight;
        double scale;
        double lineWidth;

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
        public bool ShowQuickActionsOnFileSave { get; set; }

        public double IconHeight { get => iconHeight; set => iconHeight = Math.Max(0, value); }
        public string IconPath { get; set; }

        public CustomAnimationConfig()
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
            ShowQuickActionsOnFileSave = false;

            IconHeight = 64;
            IconPath = "INVALID_PATH";
        }

        public bool IsAnimParamsEqual(CustomAnimationConfig b)
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

        public CustomAnimationConfig Duplicate()
        {
            return new CustomAnimationConfig()
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
                ShowQuickActionsOnFileSave = this.ShowQuickActionsOnFileSave,

                IconHeight = this.IconHeight,
                IconPath = this.IconPath,
            };
        }
    }
}
