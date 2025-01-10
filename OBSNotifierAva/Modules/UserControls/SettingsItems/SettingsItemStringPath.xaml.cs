using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemStringPath : SettingsItemModuleData
    {
        bool is_changed_by_code = true;

        readonly SettingsItemStringPathAttribute pathAttribute;

        public SettingsItemStringPath() : base() { pathAttribute = new(); InitializeComponent(); }

        public SettingsItemStringPath(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            InitializeComponent();
            tb_text.Text = GetPropertyName();

            ValueChanged += (s, e) =>
            {
                if (is_changed_by_code)
                    return;

                is_changed_by_code = true;
                tb_value.Text = (string)(Value ?? string.Empty);
                is_changed_by_code = false;
            };

            is_changed_by_code = true;
            tb_value.Text = (string)(Value ?? string.Empty);

            if (ValuePropertyInfo.GetCustomAttribute<SettingsItemStringPathAttribute>() is SettingsItemStringPathAttribute attr_path)
            {
                pathAttribute = attr_path;
            }
            else
            {
                throw new CustomAttributeFormatException($"Add {nameof(SettingsItemStringPathAttribute)} attribute to {ValuePropertyInfo.DeclaringType}.{ValuePropertyInfo.PropertyType}");
            }
            is_changed_by_code = false;
        }

        private void tb_value_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (is_changed_by_code)
                return;

            is_changed_by_code = true;
            Value = tb_value.Text;
            is_changed_by_code = false;
        }

        private void btn_browse_Click(object? sender, System.Windows.RoutedEventArgs e)
        {
            var asm_dir = Path.GetDirectoryName(GetType().Assembly.Location) ?? string.Empty;
            string getShortPath(string i)
            {
                return i.StartsWith(asm_dir) ? i[(asm_dir + Path.PathSeparator).Length..] : i;
            }

            if (pathAttribute.IsFile)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = pathAttribute.DefaultExt,
                    Filter = pathAttribute.FileFilter,
                    InitialDirectory = Path.GetDirectoryName(tb_value.Text) ?? string.Empty
                };

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    tb_value.Text = getShortPath(dialog.FileName);
                }
            }
            else
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    SelectedPath = Directory.Exists(tb_value.Text) ? Path.Combine(asm_dir, tb_value.Text) : asm_dir
                };
                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    tb_value.Text = getShortPath(dialog.SelectedPath);
                }
            }
        }
    }
}
