using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace OBSNotifier
{
    public static class UtilsWinApi
    {
        public static IntPtr GetHandle(this System.Windows.Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }

        #region WinAPI

        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WS_EX_TRANSPARENT = 0x20;
        const int GWL_EXSTYLE = -20;

        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOACTIVATE = 0x0010;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool WA_SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern int WA_SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern int WA_GetWindowLong(IntPtr hWnd, int nIndex);

        #endregion

        static void UpdateWindowLong(IntPtr hwnd, bool isEnabled, int updateWith)
        {
            if (isEnabled)
                WA_SetWindowLong(hwnd, GWL_EXSTYLE, WA_GetWindowLong(hwnd, GWL_EXSTYLE) | updateWith);
            else
                WA_SetWindowLong(hwnd, GWL_EXSTYLE, WA_GetWindowLong(hwnd, GWL_EXSTYLE) & ~updateWith);
        }

        /// <summary>
        /// Remove a window from the window switching menu (Alt+Tab, Win + Tab)<br/>
        /// For XAML it is better to use <c>ShowInTaskbar="False"</c><br/>
        /// https://stackoverflow.com/a/551847/8980874
        /// </summary>
        public static void SetWindowHideFromAltTab(IntPtr hwnd, bool isEnabled)
        {
            UpdateWindowLong(hwnd, isEnabled, WS_EX_TOOLWINDOW);
        }

        /// <summary>
        /// Make the mouse pass through the window
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="isEnabled"></param>
        public static void SetWindowIgnoreMouse(IntPtr hwnd, bool isEnabled)
        {
            UpdateWindowLong(hwnd, isEnabled, WS_EX_TRANSPARENT);
        }

        /// <summary>
        /// Allows the game window not to lose focus when clicking on <paramref name="hwnd"/> window.<br/>
        /// Parameters in XAML don't have the same effect.
        /// <br/><br/>
        /// For example, in Crysis 3, when a window is clicked,<br/>
        /// the sound is interrupted if only the parameters in XAML are used.<br/>
        /// But if use this method, then the parameters in XAML are not needed.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="isEnabled"></param>
        public static void SetWindowIgnoreFocus(IntPtr hwnd, bool isEnabled)
        {
            UpdateWindowLong(hwnd, isEnabled, WS_EX_NOACTIVATE);
        }

        /// <summary>
        /// Set the window as topmost permanently
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="fAlwaysTop"></param>
        public static void SetWindowTopmost(IntPtr hwnd, bool fAlwaysTop)
        {
            IntPtr hwndIdx = fAlwaysTop ? HWND_TOPMOST : HWND_NOTOPMOST;
            WA_SetWindowPos(hwnd, hwndIdx, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }
    }
}
