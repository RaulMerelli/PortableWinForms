namespace System.Windows.Forms
{
    using System;
    using System.Reflection;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Collections;

    [
        TypeConverterAttribute(typeof(LinkArea.LinkAreaConverter)),
        Serializable
    ]
    public struct LinkArea
    {
        int start;
        int length;

        public LinkArea(int start, int length)
        {
            this.start = start;
            this.length = length;
        }

        public int Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsEmpty
        {
            get
            {
                return length == start && start == 0;
            }
        }
        public override bool Equals(object o)
        {
            if (!(o is LinkArea))
            {
                return false;
            }

            LinkArea a = (LinkArea)o;
            return this == a;
        }

        public override string ToString()
        {
            return "{Start=" + Start.ToString(CultureInfo.CurrentCulture) + ", Length=" + Length.ToString(CultureInfo.CurrentCulture) + "}";
        }

        public static bool operator ==(LinkArea linkArea1, LinkArea linkArea2)
        {
            return (linkArea1.start == linkArea2.start) && (linkArea1.length == linkArea2.length);
        }

        public static bool operator !=(LinkArea linkArea1, LinkArea linkArea2)
        {
            return !(linkArea1 == linkArea2);
        }

        public override int GetHashCode()
        {
            return start << 4 | length;
        }

        public class LinkAreaConverter : TypeConverter
        {

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

                if (value is string)
                {

                    string text = ((string)value).Trim();

                    if (text.Length == 0)
                    {
                        return null;
                    }
                    else
                    {

                        // Parse 2 integer values.
                        //
                        if (culture == null)
                        {
                            culture = CultureInfo.CurrentCulture;
                        }
                        char sep = culture.TextInfo.ListSeparator[0];
                        string[] tokens = text.Split(new char[] { sep });
                        int[] values = new int[tokens.Length];
                        TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = (int)intConverter.ConvertFromString(context, culture, tokens[i]);
                        }

                        if (values.Length == 2)
                        {
                            return new LinkArea(values[0], values[1]);
                        }
                        else
                        {
                            throw new ArgumentException("TextParseFailedFormat");
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

                if (destinationType == typeof(string) && value is LinkArea)
                {
                    LinkArea pt = (LinkArea)value;

                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    string sep = culture.TextInfo.ListSeparator + " ";
                    TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
                    string[] args = new string[2];
                    int nArg = 0;

                    args[nArg++] = intConverter.ConvertToString(context, culture, pt.Start);
                    args[nArg++] = intConverter.ConvertToString(context, culture, pt.Length);

                    return string.Join(sep, args);
                }
                if (destinationType == typeof(InstanceDescriptor) && value is LinkArea)
                {
                    LinkArea pt = (LinkArea)value;

                    ConstructorInfo ctor = typeof(LinkArea).GetConstructor(new Type[] { typeof(int), typeof(int) });
                    if (ctor != null)
                    {
                        return new InstanceDescriptor(ctor, new object[] { pt.Start, pt.Length });
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                return new LinkArea((int)propertyValues["Start"],
                                 (int)propertyValues["Length"]);
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(LinkArea), attributes);
                return props.Sort(new string[] { "Start", "Length" });
            }


            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

        }
    }
}