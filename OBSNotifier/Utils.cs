using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace OBSNotifier
{
    public static class Utils
    {
        public enum AnchorPoint
        {
            TopLeft = 0, TopRight = 1, BottomRight = 2, BottomLeft = 3,
            Center = 4,
        }

        /// <summary>
        /// Simple invoke wrapper
        /// </summary>
        /// <param name="disp"></param>
        /// <param name="act"></param>
        public static DispatcherOperation InvokeAction(this DispatcherObject disp, Action act)
        {
            return disp.Dispatcher.BeginInvoke(act);
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

        /// <summary>
        /// Fix the position of the window that goes beyond the borders of the window's screen
        /// </summary>
        /// <param name="wnd"></param>
        public static void FixWindowLocation(Window wnd)
        {
            FixWindowLocation(wnd, WPFScreens.GetScreenFrom(wnd));
        }

        /// <summary>
        /// Fix the position of a window that goes beyond the boundaries of the <paramref name="screen"/>
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="screen"></param>
        public static void FixWindowLocation(Window wnd, WPFScreens screen)
        {
            var rect = screen.WorkingArea;

            if (rect.Size.Width < wnd.Width || rect.Size.Height < wnd.Height)
            {
                if (rect.Size.Width < wnd.Width)
                    wnd.Left = rect.Left;
                if (rect.Size.Height < wnd.Height)
                    wnd.Top = rect.Top;
                return;
            }

            if (wnd.Left < rect.Left)
                wnd.Left = rect.Left;
            if (wnd.Top < rect.Top)
                wnd.Top = rect.Top;

            if (wnd.Left + wnd.Width > rect.Right)
                wnd.Left = rect.Right - wnd.Width;
            if (wnd.Top + wnd.Height > rect.Bottom)
                wnd.Top = rect.Bottom - wnd.Height;
        }

        /// <summary>
        /// Returns the current <see cref="WPFScreens"/> selected in the application settings
        /// </summary>
        /// <returns></returns>
        public static WPFScreens GetCurrentNotificationScreen()
        {
            foreach (var s in WPFScreens.AllScreens())
            {
                if (s.DeviceName == Settings.Instance.DisplayID)
                    return s;
            }
            return WPFScreens.Primary;
        }

        /// <summary>
        /// Returns the bounds of the <see cref="WPFScreens"/> based on the selected settings
        /// </summary>
        /// <returns></returns>
        public static Rect GetCurrentDisplayBounds(WPFScreens screen)
        {
            return Settings.Instance.UseSafeDisplayArea ? screen.WorkingArea : screen.DeviceBounds;
        }

        /// <summary>
        /// Returns the location of the window inside the currently selected notification screen.
        /// The location of the window will depend on the selected <see cref="AnchorPoint"/> and the offset from it.
        /// </summary>
        /// <param name="anchor"></param>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Point GetWindowPosition(AnchorPoint anchor, Size size, Point offset)
        {
            return GetWindowPosition(GetCurrentNotificationScreen(), anchor, size, offset);
        }

        /// <summary>
        /// Returns the location of the window inside the screen.
        /// The location of the window will depend on the selected <see cref="AnchorPoint"/> and the offset from it.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="anchor"></param>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Point GetWindowPosition(WPFScreens screen, AnchorPoint anchor, Size size, Point offset)
        {
            if (screen == null)
                return new Point();

            var rect = GetCurrentDisplayBounds(screen);

            var boundsPos = rect.TopLeft;
            var boundsPosEnd = new Point(rect.BottomRight.X - size.Width, rect.BottomRight.Y - size.Height);
            var offsetX = (rect.Width - size.Width) * offset.X;
            var offsetY = (rect.Height - size.Height) * offset.Y;

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

        /// <summary>
        /// Get a trimmed file path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string GetShortPath(string path, uint chars)
        {
            var short_name = "";
            if (path.Length > chars)
            {
                short_name = path.Substring(path.Length - (int)chars);
                var slash_pos = short_name.IndexOf(Path.DirectorySeparatorChar);
                if (slash_pos == -1)
                    slash_pos = short_name.IndexOf(Path.AltDirectorySeparatorChar);

                if (slash_pos == -1)
                    short_name = "..." + short_name;
                else
                    short_name = "..." + short_name.Substring(slash_pos);
            }
            else
            {
                short_name = path;
            }
            return short_name;
        }

        /// <summary>
        /// Remove a window from the window switching menu (Alt+Tab, Win + Tab)
        /// </summary>
        /// <param name="wnd"></param>
        public static void RemoveWindowFromAltTab(Window wnd)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(wnd);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        #region Native Windows Window styles

        // https://stackoverflow.com/a/551847/8980874

        [Flags]
        enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        static extern void SetLastError(int dwErrorCode);
        #endregion
    }
}
