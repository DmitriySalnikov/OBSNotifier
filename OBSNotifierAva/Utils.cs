using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

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
            if (Application.Current == null) throw new NullReferenceException(nameof(Application.Current));

            var is_spec_dict = specificResDict != null;
            IResourceDictionary dict = specificResDict ?? Application.Current.Resources;

            if (dict.ContainsKey(id))
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

            throw new NotImplementedException();
            // TODO
            /*
            // Second try to fallback
            var dicts = Application.Current?.Resources.MergedDictionaries.Where((i) => i.Source != null && i.Source.OriginalString.StartsWith("Localization/lang.")).ToArray() ?? throw new NullReferenceException(nameof(Application.Current));
            loc_str = Tr(id, dicts[0]);

            return string.Format(loc_str, args);
             */
        }

        public static string TrErrorTitle()
        {
            return $"{App.AppNameSpaced}: {OBSNotifier.Tr.MessageBox.ErrorTitle}";
        }

        public static string GetOsName()
        {
            if (OperatingSystem.IsWindows())
            {
                return "Windows";
            }
            else if (OperatingSystem.IsLinux())
            {
                return "Linux";
            }
            else if (OperatingSystem.IsMacOS())
            {
                return "MacOS";
            }
            else if (OperatingSystem.IsAndroid())
            {
                return "Android";
            }
            return "Unknown";
        }

        public enum AnchorPoint
        {
            TopLeft = 0, TopRight = 1, BottomRight = 2, BottomLeft = 3,
            Center = 4,
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
            if (string.IsNullOrWhiteSpace(encrypted))
                return null;

            var encryptedBytes = Convert.FromBase64String(encrypted);
            if (encryptedBytes == null || encryptedBytes.Length == 0)
                return null;

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
        /// Fix the position of a window that goes beyond the boundaries of the screen
        /// </summary>
        /// <param name="wnd"></param>
        public static void FixWindowLocation(Window wnd)
        {
            var rect = wnd.Screens.ScreenFromWindow(wnd)?.Bounds ?? new PixelRect();

            if (rect.Size.Width < wnd.Width || rect.Size.Height < wnd.Height)
            {
                wnd.Position = new PixelPoint(
                    rect.Size.Width < wnd.Width ? rect.X : wnd.Position.X,
                    rect.Size.Height < wnd.Height ? rect.Y : wnd.Position.Y);
                return;
            }

            if (wnd.Position.X < rect.X || wnd.Position.Y < rect.Y)
            {
                wnd.Position = new PixelPoint(
                   wnd.Position.X < rect.X ? rect.X : wnd.Position.X,
                   wnd.Position.Y < rect.Y ? rect.Y : wnd.Position.Y);
            }

            int wndWidth = (int)(wnd.FrameSize?.Width ?? wnd.ClientSize.Width);
            int wndHeight = (int)(wnd.FrameSize?.Height ?? wnd.ClientSize.Height);

            if (wnd.Position.X + wndWidth > rect.Right || wnd.Position.Y + wndHeight > rect.Bottom)
            {
                wnd.Position = new PixelPoint(
                   wnd.Position.X + wndWidth > rect.Right ? rect.Right - wndWidth : wnd.Position.X,
                   wnd.Position.Y + wndHeight > rect.Bottom ? rect.Bottom - wndHeight : wnd.Position.Y);
            }
        }

        /// <summary>
        /// Returns the <see cref="WPFScreens"/> by the display ID
        /// </summary>
        /// <returns></returns>
        public static Screen GetScreenById(Window wnd, uint displayID)
        {
            if (displayID < wnd.Screens.ScreenCount)
            {
                return wnd.Screens.All[(int)displayID];
            }
            return wnd.Screens.Primary ?? wnd.Screens.All[0]; // ???
        }

        /// <summary>
        /// Returns the bounds of the <see cref="Screen"/> based on the selected settings
        /// </summary>
        /// <returns></returns>
        public static PixelRect GetCurrentDisplayBounds(Screen screen, bool useSafeArea)
        {
            return useSafeArea ? screen.WorkingArea : screen.Bounds;
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
        public static PixelPoint GetWindowPosition(Window wnd, uint displayID, AnchorPoint anchor, PixelSize size, PixelPoint offset, bool useSafeArea)
        {
            return GetWindowPosition(GetScreenById(wnd, displayID), anchor, size, offset, useSafeArea);
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
        public static PixelPoint GetWindowPosition(Screen screen, AnchorPoint anchor, PixelSize size, PixelPoint offset, bool useSafeArea)
        {
            if (screen == null)
                return new PixelPoint();

            var rect = GetCurrentDisplayBounds(screen, useSafeArea);

            var boundsPos = rect.TopLeft;
            var boundsPosEnd = new PixelPoint(rect.BottomRight.X - size.Width, rect.BottomRight.Y - size.Height);
            var offsetX = (rect.Width - size.Width) * offset.X;
            var offsetY = (rect.Height - size.Height) * offset.Y;

            switch (anchor)
            {
                case AnchorPoint.TopLeft:
                    return new PixelPoint(boundsPos.X + offsetX, boundsPos.Y + offsetY);
                case AnchorPoint.TopRight:
                    return new PixelPoint(boundsPosEnd.X - offsetX, boundsPos.Y + offsetY);
                case AnchorPoint.BottomRight:
                    return new PixelPoint(boundsPosEnd.X - offsetX, boundsPosEnd.Y - offsetY);
                case AnchorPoint.BottomLeft:
                    return new PixelPoint(boundsPos.X + offsetX, boundsPosEnd.Y - offsetY);
                case AnchorPoint.Center:
                    return new PixelPoint(boundsPos.X + offsetX, boundsPos.Y + offsetY);
            };

            return new PixelPoint();
        }

        public static PixelPoint GetModuleWindowPosition(Window window, uint displayID, AnchorPoint anchor, PixelSize size, PixelPoint offset, bool useSafeArea)
        {
            return GetModuleWindowPosition(window, GetScreenById(window, displayID), anchor, size, offset, useSafeArea);
        }

        public static PixelPoint GetModuleWindowPosition(Window window, Screen screen, AnchorPoint anchor, PixelSize size, PixelPoint offset, bool useSafeArea)
        {
            return GetWindowPosition(screen, anchor, new PixelSize((int)(size.Width * window.DesktopScaling), (int)(size.Height * window.DesktopScaling)), offset, useSafeArea);
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
        public static IImage GetBitmapImage(string path, Assembly? assembly = null)
        {
            var rel_path = Path.Combine(Path.GetDirectoryName(assembly?.Location) ?? string.Empty, path);
            if (File.Exists(rel_path))
            {
                return new Bitmap(rel_path);
            }
            if (File.Exists(path))
            {
                return new Bitmap(path);
            }

            return new DrawingImage();
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
