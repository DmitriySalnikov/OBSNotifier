using System;

namespace OBSNotifier.Modules
{
    // TODO add Attribute for custom control type! Useful, for example, for configuring sound notifications

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class SettingsItemAttribute(string name) : Attribute
    {
        public string Name = name;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class SettingsItemCategoryAttribute(string categoryName = "") : Attribute
    {
        public string CategoryName = categoryName;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class SettingsItemIgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class SettingsItemStringPathAttribute : Attribute
    {
        public bool IsFile = true;
        public string DefaultExt = "*.*";
        public string FileFilter = "All Files|*.*";
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class SettingsItemStringDisplayIDAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class SettingsItemNumberRangeAttribute(double min, double max, double step = -1) : Attribute
    {
        public double Min = min;
        public double Max = max;
        public double Step = step;
    }
}
