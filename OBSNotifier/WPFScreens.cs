using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Media3D;

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

        public static Size GetMaxSize()
        {
            var s = AllScreens();
            return new Size(s.Max(s => s.DeviceBounds.Width), s.Max(s => s.DeviceBounds.Height));
        }

        public static WPFScreens GetScreenFrom(Window window)
        {
            var windowInteropHelper = new WindowInteropHelper(window);
            Screen screen = Screen.FromHandle(windowInteropHelper.Handle);
            var wpfScreen = new WPFScreens(screen);
            return wpfScreen;
        }

        public static WPFScreens GetScreenFrom(Point point)
        {
            int x = (int)Math.Round(point.X);
            int y = (int)Math.Round(point.Y);

            // are x,y device-independent-pixels ??
            var drawingPoint = new System.Drawing.Point(x, y);
            Screen screen = Screen.FromPoint(drawingPoint);
            var wpfScreen = new WPFScreens(screen);

            return wpfScreen;
        }

        public static WPFScreens Primary
        {
            get { return new WPFScreens(Screen.PrimaryScreen ?? throw new NullReferenceException("Screen.PrimaryScreen")); }
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

        private static Rect GetRect(System.Drawing.Rectangle value)
        {
            return new Rect
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
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
