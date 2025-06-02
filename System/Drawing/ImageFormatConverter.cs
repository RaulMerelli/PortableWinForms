namespace System.Drawing
{

    using System.Diagnostics;
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Drawing.Imaging;

    public class ImageFormatConverter : TypeConverter
    {
        private StandardValuesCollection values;

        public ImageFormatConverter()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {

            string strValue = value as string;

            if (strValue != null)
            {

                string text = strValue.Trim();
                PropertyInfo[] props = GetProperties();
                for (int i = 0; i < props.Length; i++)
                {
                    PropertyInfo prop = props[i];
                    if (string.Equals(prop.Name, text, StringComparison.OrdinalIgnoreCase))
                    {
                        object[] tempIndex = null;
                        return prop.GetValue(null, tempIndex);
                    }
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (value is ImageFormat)
            {
                PropertyInfo targetProp = null;

                PropertyInfo[] props = GetProperties();
                foreach (PropertyInfo p in props)
                {
                    if (p.GetValue(null, null).Equals(value))
                    {
                        targetProp = p;
                        break;
                    }
                }

                if (targetProp != null)
                {
                    if (destinationType == typeof(string))
                    {
                        return targetProp.Name;
                    }
                    else if (destinationType == typeof(InstanceDescriptor))
                    {
                        return new InstanceDescriptor(targetProp, null);
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private PropertyInfo[] GetProperties()
        {
            return typeof(ImageFormat).GetProperties(BindingFlags.Static | BindingFlags.Public);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (values == null)
            {
                ArrayList list = new ArrayList();
                PropertyInfo[] props = GetProperties();
                for (int i = 0; i < props.Length; i++)
                {
                    PropertyInfo prop = props[i];
                    object[] tempIndex = null;
                    Debug.Assert(prop.GetValue(null, tempIndex) != null, "Property " + prop.Name + " returned NULL");
                    list.Add(prop.GetValue(null, tempIndex));
                }

                values = new StandardValuesCollection(list.ToArray());
            }

            return values;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}