using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace OBSNotifier
{
    internal static class Utils
    {
        public enum AnchorPoint
        {
            //Left, Top, Right, Bottom,
            TopLeft, TopRight, BottomRight, BottomLeft,
            Center
        }

        public static void InvokeAction(this DispatcherObject disp, Action act)
        {
            disp.Dispatcher.BeginInvoke(act);
        }

        public static string EncryptString(string plainText)
        {
            if (plainText == null) throw new ArgumentNullException("plainText");
            if (plainText == string.Empty) return "";
            var data = Encoding.Unicode.GetBytes(plainText);
            return Convert.ToBase64String(data);
        }

        public static string DecryptString(string encrypted)
        {
            if (encrypted == null) throw new ArgumentNullException("encrypted");
            if (encrypted  == string.Empty) return "";
            byte[] data = Convert.FromBase64String(encrypted);
            return Encoding.Unicode.GetString(data);
        }

        public static Point GetWindowPosition(AnchorPoint anchor, Point size, Point offset)
        {
            WPFScreens screen = null;
            foreach (var s in WPFScreens.AllScreens())
            {
                if (s.DeviceName == Settings.Instance.DisplayID)
                    screen = s;
            }

            if (screen == null)
                return new Point();

            var rect = Settings.Instance.UseSafeDisplayArea ? screen.WorkingArea : screen.DeviceBounds;

            var boundsPos = rect.TopLeft;
            var boundsPosEnd = new Point(rect.BottomRight.X - size.X, rect.BottomRight.Y - size.Y);
            var offsetX = (rect.Width - size.X) * offset.X;
            var offsetY = (rect.Height - size.Y) * offset.Y;

            switch (anchor)
            {
                case AnchorPoint.TopLeft:
                    return new Point(boundsPos.X + offsetX, boundsPos.Y + offsetY);
                case AnchorPoint.TopRight:
                    return new Point(boundsPosEnd.X - offsetX, boundsPos.Y + offsetY);
                case AnchorPoint.BottomRight:
                    return new Point(boundsPosEnd.X - offsetX, boundsPosEnd.Y - offsetY);
                case AnchorPoint.BottomLeft:
                    return new Point(boundsPos.X + offsetX, boundsPosEnd.Y - offsetY);
                case AnchorPoint.Center:
                    return new Point(boundsPos.X + offsetX, boundsPos.Y + offsetY);
            }
            return new Point();
        }
    }
}
