namespace OBSNotifier.Forms
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            this.Title = Utils.TrFormat("tray_menu_about", AssemblyTitle);
            this.labelProductName.Content = AssemblyProduct;
            this.labelVersion.Content = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Content = AssemblyCopyright;
            this.labelCompanyName.Content = AssemblyCompany;
            this.textBoxDescription.Content = AssemblyDescription;

            link_source_code.Content = Utils.Tr("about_window_view_source_code");
            // TODO RightToLeft = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
        }

        #region Assembly Attribute Accessors
        public static string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "-1.-1.-1";
            }
        }

        public static string AssemblyDescription
        {
            get
            {
                return Utils.Tr("about_window_description");
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void Label_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Utils.ProcessStartShell("https://github.com/DmitriySalnikov/OBSNotifier");
        }
    }
}
