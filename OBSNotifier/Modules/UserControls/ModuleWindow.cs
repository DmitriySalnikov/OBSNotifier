using System.Windows;

namespace OBSNotifier.Modules.UserControls
{
    public struct ModuleWindowDpi
    {
        public Size CurrentDpiScale;
        public Size CurrentInvertedDpiScale;
        public Size DefaultDPI;

        public ModuleWindowDpi()
        {
            CurrentDpiScale = new(1, 1);
            CurrentInvertedDpiScale = new(1, 1);
            DefaultDPI = new(96, 96);
        }
    }

    public class ModuleWindow : Window
    {
        public ModuleWindowDpi DpiData { get; private set; } = new();

        public ModuleWindow()
        {
            DpiChanged += ModuleWindow_DpiChanged;
        }

        protected virtual void OnModuleDpiChanged() { }

        private void ModuleWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            ModuleWindowDpi tmp;
            tmp.DefaultDPI = new(e.NewDpi.PixelsPerInchX / e.NewDpi.DpiScaleX, e.NewDpi.PixelsPerInchY / e.NewDpi.DpiScaleY);
            tmp.CurrentDpiScale = new(e.NewDpi.DpiScaleX, e.NewDpi.DpiScaleY);
            tmp.CurrentInvertedDpiScale = new(tmp.DefaultDPI.Width / e.NewDpi.PixelsPerInchX, tmp.DefaultDPI.Height / e.NewDpi.PixelsPerInchY);

            DpiData = tmp;

            // Must be called 'async' or it will place window in wrong position
            Dispatcher.BeginInvoke(() => OnModuleDpiChanged());
        }
    }
}
