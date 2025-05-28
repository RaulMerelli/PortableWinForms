namespace System.Windows.Forms
{

    using System.Diagnostics;
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.IO;

    public class CursorConverter : TypeConverter
    {

        private StandardValuesCollection values;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string) || sourceType == typeof(byte[]))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(byte[]))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {

            if (value is string)
            {
                string text = ((string)value).Trim();

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

            if (value is byte[])
            {
                MemoryStream ms = new MemoryStream((byte[])value);
                return new Cursor(ms);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(string) && value != null)
            {
                PropertyInfo[] props = GetProperties();
                int bestMatch = -1;

                for (int i = 0; i < props.Length; i++)
                {
                    PropertyInfo prop = props[i];
                    object[] tempIndex = null;
                    Cursor c = (Cursor)prop.GetValue(null, tempIndex);
                    if (c == (Cursor)value)
                    {
                        if (Object.ReferenceEquals(c, value))
                        {
                            return prop.Name;
                        }
                        else
                        {
                            bestMatch = i;
                        }
                    }
                }

                if (bestMatch != -1)
                {
                    return props[bestMatch].Name;
                }

                // We throw here because we cannot meaningfully convert a custom
                // cursor into a string. In fact, the ResXResourceWriter will use
                // this exception to indicate to itself that this object should
                // be serialized through ISeriazable instead of a string.
                //
                throw new FormatException("CursorCannotCovertToString");
            }

            if (destinationType == typeof(InstanceDescriptor) && value is Cursor)
            {
                PropertyInfo[] props = GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    if (prop.GetValue(null, null) == value)
                    {
                        return new InstanceDescriptor(prop, null);
                    }
                }
            }

            if (destinationType == typeof(byte[]))
            {
                if (value != null)
                {
                    MemoryStream ms = new MemoryStream();
                    Cursor cursor = (Cursor)value;
                    cursor.SavePicture(ms);
                    ms.Close();
                    return ms.ToArray();
                }
                else
                    return new byte[0];
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private PropertyInfo[] GetProperties()
        {
            return typeof(Cursors).GetProperties(BindingFlags.Static | BindingFlags.Public);
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