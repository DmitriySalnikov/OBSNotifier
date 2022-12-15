using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using OBSNotifier.Plugins;

namespace OBSNotifier.ConfigUI
{
    public class VariableInfo
    {
        public readonly MemberInfo OriginalInfo;
        public readonly object Owner;
        readonly bool isField = false;

        public UIParamAttribute argUI = null;
        public ParamRangeAttribute argRange = null;
        public ParamTextAttribute argText = null;

        public Type VarType
        {
            get
            {
                if (isField)
                    return (OriginalInfo as FieldInfo).FieldType;
                else
                    return (OriginalInfo as PropertyInfo).PropertyType;
            }
        }

        public VariableInfo(object owner, MemberInfo member)
        {
            if (!(member is FieldInfo || member is PropertyInfo))
                throw new ArgumentException($"Expected FieldInfo or PropertyInfo, but got {member.GetType()}");

            OriginalInfo = member;
            Owner = owner;
            isField = OriginalInfo is FieldInfo;
        }

        public static implicit operator PropertyInfo(VariableInfo a)
        {
            if (!a.isField)
                return (PropertyInfo)a;
            return null;
        }

        public static implicit operator FieldInfo(VariableInfo a)
        {
            if (a.isField)
                return (FieldInfo)a;
            return null;
        }

        public void SetValue(object value)
        {
            if (isField)
                (OriginalInfo as FieldInfo).SetValue(Owner, value);
            else
                (OriginalInfo as PropertyInfo).SetValue(Owner, value);
        }

        public object GetValue()
        {
            if (isField)
                return (OriginalInfo as FieldInfo).GetValue(Owner);
            else
                return (OriginalInfo as PropertyInfo).GetValue(Owner);
        }

        public T GetValue<T>()
        {
            if (isField)
                return (T)(OriginalInfo as FieldInfo).GetValue(Owner);
            else
                return (T)(OriginalInfo as PropertyInfo).GetValue(Owner);
        }


        public string GetName()
        {
            if (argUI != null)
                return argUI.Name;
            return OriginalInfo.Name;
        }
    }

    static class ConfigMenuGenerator
    {
        static readonly Dictionary<Type, VariableInfo[]> cachedMembers = new Dictionary<Type, VariableInfo[]>();
        public static BaseGrid GenerateMenu(object data)
        {
            var type = data.GetType();
            VariableInfo[] typeDescs;

            // write arrays of members to the cache to avoid random sorting as much as possible.
            if (cachedMembers.ContainsKey(type))
            {
                typeDescs = cachedMembers[type];
            }
            else
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                    .Where((p) => p.GetCustomAttribute<ParamIgnoreAttribute>() == null)
                    .Cast<MemberInfo>();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField)
                    .Where((f) => f.GetCustomAttribute<ParamIgnoreAttribute>() == null)
                    .Cast<MemberInfo>();

                typeDescs = props.Concat(fields)
                    .Select((p) => new VariableInfo(data, p)
                    {
                        argUI = p.GetCustomAttribute<UIParamAttribute>(),
                        argRange = p.GetCustomAttribute<ParamRangeAttribute>(),
                        argText = p.GetCustomAttribute<ParamTextAttribute>(),
                    })
                    .ToArray();

                cachedMembers.Add(type, typeDescs);
            }

            var grid = new BaseGrid();
            foreach (var desc in typeDescs)
            {
                var ctrl = GetControlForMember(desc);

                if (ctrl != null)
                {
                    grid.Children.Add(ctrl);
                }
                else
                {
                    App.Log($"No suitable control found for member \"{desc.OriginalInfo.Name}\" with type \"{desc.OriginalInfo.DeclaringType}\"", Logger.ErrorLevel.Warning);
                }
            }


            return grid;
        }

        static UserControl GetControlForMember(VariableInfo desc)
        {
            var type = desc.VarType;
            if (type == typeof(bool))
            {
                return new BoolParam(desc);
            }
            else if (type == typeof(string))
            {
                return new StringParam(desc);
            }

            return null;
        }
    }
}
