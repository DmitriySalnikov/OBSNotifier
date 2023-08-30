using System;
using System.Windows;
using System.Windows.Media;

namespace OBSNotifier.Modules.Default
{
    public class DefaultCustomNotifBlockSettings
    {
        uint blocks;
        uint duration;
        double radius;
        double width;
        double height;

        [ConfigIgnore]
        public uint Duration { get { return duration; } set { duration = Math.Max(0, value); } }

        public Color BackgroundColor { get; set; }
        public Color OutlineColor { get; set; }
        public Color TextColor { get; set; }
        public uint Blocks { get => blocks; set { blocks = Math.Max(1, value); } }
        public double Radius { get { return radius; } set { radius = Math.Max(0, value); } }
        public double Width { get { return width; } set { width = Math.Max(1, value); } }
        public double Height { get { return height; } set { height = Math.Max(1, value); } }
        public Thickness Margin { get; set; }
        public uint MaxPathChars { get; set; }
        public bool ShowQuickActionsOnFileSave { get; set; }

        public DefaultCustomNotifBlockSettings()
        {
            Blocks = 1;
            BackgroundColor = (Color)ColorConverter.ConvertFromString("#4C4C4C");
            TextColor = (Color)ColorConverter.ConvertFromString("#D8D8D8");
            OutlineColor = (Color)ColorConverter.ConvertFromString("#59000000");
            Duration = 2000;
            Radius = 4;
            Width = 180;
            Height = 52;
            Margin = new Thickness(4);
            MaxPathChars = 32;
            ShowQuickActionsOnFileSave = true;
        }
    }
}
