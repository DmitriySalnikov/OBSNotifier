using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSNotifier.Plugins
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class UIParamAttribute : Attribute
    {
        public string Name;
        public object DefaultValue = null;
        public bool HasDefaultValue = false;

        public UIParamAttribute()
        {
        }

        public UIParamAttribute(string name)
        {
            Name = name;
            HasDefaultValue = false;
        }

        public UIParamAttribute(string name, object defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ParamRangeAttribute : Attribute
    {
        public double Min;
        public double Max;
        public double? Step;

        public ParamRangeAttribute(double min, double max, double? step = null)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ParamTextAttribute : Attribute
    {
        public uint MaxLength;
        public bool Multiline;

        public ParamTextAttribute(uint maxLength, bool multiline = false)
        {
            MaxLength = maxLength;
            Multiline = multiline;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ParamIgnoreAttribute : Attribute { }
}
