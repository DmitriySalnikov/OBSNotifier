using System;
using System.Text;
using System.Windows;

namespace OBSNotifier
{
    internal static class Utils
    {
        public static void InvokeAction(this Window wnd, Action act)
        {
            wnd.Dispatcher.BeginInvoke(act);
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
    }
}
