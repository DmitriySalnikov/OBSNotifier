using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace OBSNotifier
{
    // https://stackoverflow.com/a/2118993/8980874

    public class WPFScreens
    {
        public static IEnumerable<WPFScreens> AllScreens()
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                yield return new WPFScreens(screen);
            }
        }

        public static WPFScreens GetScreenFrom(Window window)
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
            Screen screen = Screen.FromHandle(windowInteropHelper.Handle);
            WPFScreens wpfScreen = new WPFScreens(screen);
            return wpfScreen;
        }

        public static WPFScreens GetScreenFrom(Point point)
        {
            int x = (int)Math.Round(point.X);
            int y = (int)Math.Round(point.Y);

            // are x,y device-independent-pixels ??
            System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
            Screen screen = Screen.FromPoint(drawingPoint);
            WPFScreens wpfScreen = new WPFScreens(screen);

            return wpfScreen;
        }

        public static WPFScreens Primary
        {
            get { return new WPFScreens(Screen.PrimaryScreen); }
        }

        private readonly Screen screen;

        internal WPFScreens(Screen screen)
        {
            this.screen = screen;
        }

        public Rect DeviceBounds
        {
            get { return GetRect(screen.Bounds); }
        }

        public Rect WorkingArea
        {
            get { return GetRect(screen.WorkingArea); }
        }

        // https://stackoverflow.com/questions/1927540/how-to-get-the-size-of-the-current-screen-in-wpf#comment64493527_2118993
        private Rect GetRect(System.Drawing.Rectangle value)
        {
            var pixelWidthFactor = SystemParameters.WorkArea.Width / screen.WorkingArea.Width;
            var pixelHeightFactor = SystemParameters.WorkArea.Height / screen.WorkingArea.Height;

            return new Rect
            {
                X = value.X * pixelWidthFactor,
                Y = value.Y * pixelHeightFactor,
                Width = value.Width * pixelWidthFactor,
                Height = value.Height * pixelHeightFactor
            };
        }

        public bool IsPrimary
        {
            get { return screen.Primary; }
        }

        public string DeviceName
        {
            get { return screen.DeviceName; }
        }
    }
}
