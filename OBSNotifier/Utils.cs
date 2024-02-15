using System.Windows;
using Forms = System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using OBSNotifier.Modules.UserControls;

namespace OBSNotifier
{
    public static class Utils
    {
        public readonly static string PreviewPathString = @"D:\Lorem\ipsum\dolor\sit\amet\consectetur\adipiscing\elit.\Donec\pharetra\lorem\turpis\nec\fringilla\leo\interdum\sit\amet.\Mauris\in\placerat\nulla\in\laoreet\Videos\OBS\01.01.01\Replay_01-01-01.mkv";
        readonly static int EncryptedMagic = 0x4f4e4544; // ONED - OBS Notifier Encrypted Data

        /// <summary>
        /// Get the translated string by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Tr(string id, ResourceDictionary? specificResDict = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"{nameof(id)} must not be null or an empty string");

            var is_spec_dict = specificResDict != null;
            ResourceDictionary dict = specificResDict ?? Application.Current.Resources;

            if (dict.Contains(id))
            {
                var obj = dict[id];
                if (obj.GetType() != typeof(string))
                {
                    App.LogError($"Resource with ID \"{id}\" is not a string." + (is_spec_dict ? " Using a specific dictionary" : ""));
                    return $"[{id}]";
                }

                return (string)obj;
            }

            App.LogError($"Resource ID ({id}) not found." + (is_spec_dict ? " Using a specific dictionary" : ""));
            return $"[{id}]";
        }

        /// <summary>
        /// Get the translated string by ID and apply formatting to it.
        /// If formatting fails, fallback to the default language and apply formatting to it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string TrFormat(string id, params object[] args)
        {
            var loc_str = Tr(id);

            try
            {
                return string.Format(loc_str, args);
            }
            catch (Exception ex)
            {
                App.LogError($"The localized string ({id}) cannot be formatted for {Thread.CurrentThread.CurrentUICulture}");
                App.Log(ex);
            }

            // Second try to fallback
            var dicts = Application.Current.Resources.MergedDictionaries.Where((i) => i.Source != null && i.Source.OriginalString.StartsWith("Localization/lang.")).ToArray();
            loc_str = Tr(id, dicts[0]);

            return string.Format(loc_str, args);
        }

        public static string TrErrorTitle()
        {
            return $"{App.AppNameSpaced}: {Tr("message_box_error_title")}";
        }

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

        /// <summary>
        /// Encrypt a string with AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptString(string plainText, string key)
        {
            var iv = new byte[16];
            Random.Shared.NextBytes(iv);

            var textUtf8 = Encoding.UTF8.GetBytes(plainText);
            using var mem = new MemoryStream();
            using var bw = new BinaryWriter(mem);
            bw.Write(EncryptedMagic);
            bw.Write(MD5.HashData(textUtf8));
            bw.Write(iv);

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = iv;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(textUtf8, 0, textUtf8.Length);

            bw.Write(encryptedBytes.Length);
            bw.Write(encryptedBytes);

            return Convert.ToBase64String(mem.ToArray());
        }

        /// <summary>
        /// Decrypt a string with AES
        /// </summary>
        /// <param name="encrypted">Encrypted base64 string</param>
        /// <param name="key"></param>
        /// <returns><see cref="null"/> on error</returns>
        public static string? DecryptString(string encrypted, string key)
        {
            var encryptedBytes = Convert.FromBase64String(encrypted);
            using var mem = new MemoryStream(encryptedBytes);
            using var br = new BinaryReader(mem);

            if (br.ReadInt32() != EncryptedMagic)
                return null;

            var md5 = br.ReadBytes(16);
            var iv = br.ReadBytes(16);

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = iv;

            var len = br.ReadInt32();
            var cipherText = br.ReadBytes(len);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, len);

            if (!MD5.HashData(decryptedBytes).SequenceEqual(md5))
                return null;

            return Encoding.UTF8.GetString(decryptedBytes);
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
        /// Returns the <see cref="WPFScreens"/> by the display ID
        /// </summary>
        /// <returns></returns>
        public static WPFScreens GetScreenById(string displayID)
        {
            foreach (var s in WPFScreens.AllScreens())
            {
                if (s.DeviceName == displayID)
                    return s;
            }
            return WPFScreens.Primary;
        }

        public static WPFScreens GetWindowScreen(Window window)
        {
            var center = new Point(window.Left + window.Width * 0.5, window.Top + window.Height * 0.5);
            foreach (var s in WPFScreens.AllScreens())
            {
                if (s.DeviceBounds.Contains(center))
                    return s;
            }
            return WPFScreens.Primary;
        }

        /// <summary>
        /// Returns the bounds of the <see cref="WPFScreens"/> based on the selected settings
        /// </summary>
        /// <returns></returns>
        public static Rect GetCurrentDisplayBounds(WPFScreens screen, bool useSafeArea)
        {
            return useSafeArea ? screen.WorkingArea : screen.DeviceBounds;
        }

        /// <summary>
        /// Returns the location of the window inside the currently selected notification screen.
        /// The location of the window will depend on the selected <see cref="AnchorPoint"/> and the offset from it.
        /// </summary>
        /// <param name="anchor"></param>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        /// <param name="useSafeArea"></param>
        /// <returns></returns>
        public static Point GetWindowPosition(string displayID, AnchorPoint anchor, Size size, Point offset, bool useSafeArea)
        {
            return GetWindowPosition(GetScreenById(displayID), anchor, size, offset, useSafeArea);
        }

        /// <summary>
        /// Returns the location of the window inside the screen.
        /// The location of the window will depend on the selected <see cref="AnchorPoint"/> and the offset from it.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="anchor"></param>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        /// <param name="useSafeArea"></param>
        /// <returns></returns>
        public static Point GetWindowPosition(WPFScreens screen, AnchorPoint anchor, Size size, Point offset, bool useSafeArea)
        {
            if (screen == null)
                return new Point();

            var rect = GetCurrentDisplayBounds(screen, useSafeArea);

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
            };

            return new Point();
        }

        public static Point GetModuleWindowPosition(ModuleWindow window, string displayID, AnchorPoint anchor, Size size, Point offset, bool useSafeArea)
        {
            return GetModuleWindowPosition(window, GetScreenById(displayID), anchor, size, offset, useSafeArea);
        }

        public static Point GetModuleWindowPosition(ModuleWindow window, WPFScreens screen, AnchorPoint anchor, Size size, Point offset, bool useSafeArea)
        {
            return GetWindowPosition(screen, anchor, new Size(size.Width * window.DpiData.CurrentDpiScale.Width, size.Height * window.DpiData.CurrentDpiScale.Height), offset, useSafeArea);
        }

        /// <summary>
        /// Get a trimmed file path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string GetShortPath(string path, uint chars)
        {
            if (string.IsNullOrEmpty(path) || chars <= 0)
                return "";

            string short_name;
            if (path.Length > chars)
            {
                short_name = path[^((int)chars)..];
                var slash_pos = short_name.IndexOf(Path.DirectorySeparatorChar);
                if (slash_pos == -1)
                    slash_pos = short_name.IndexOf(Path.AltDirectorySeparatorChar);

                if (slash_pos == -1)
                    short_name = "..." + short_name;
                else
                    short_name = "..." + short_name[slash_pos..];
            }
            else
            {
                short_name = path;
            }
            return short_name;
        }

        public static bool ApproxEqual(double a, double b, double epsilon = 0.0001)
        {
            return Math.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// Get the calling file and line
        /// </summary>
        /// <param name="callingFilePath"></param>
        /// <param name="callingFileLineNumber"></param>
        /// <returns></returns>
        public static string GetCallerFileLine([CallerFilePath] string callingFilePath = "", [CallerLineNumber] int callingFileLineNumber = 0)
        {
            return $"{Path.GetFileName(callingFilePath)}:{callingFileLineNumber}";
        }

        /// <summary>
        /// Load WPF Image from resources or file path
        /// </summary>
        /// <param name="path">Image path</param>
        /// <param name="assembly">Assembly to perform search in relative path</param>
        /// <returns>Loaded image</returns>
        public static BitmapImage GetBitmapImage(string path, Assembly? assembly = null)
        {
            BitmapImage img = new();
            img.BeginInit();

            if (assembly != null)
                img.UriSource = new Uri(Path.Combine(Path.GetDirectoryName(assembly.Location) ?? string.Empty, path));
            else
                img.UriSource = new Uri(path);

            img.EndInit();

            return img;
        }

        public static void UpdateContextItemStyle(Forms.ToolStripItem item, System.Drawing.FontStyle style)
        {
            item.Font = new System.Drawing.Font(item.Font, style);
        }

        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static void ProcessStartShell(string path)
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
