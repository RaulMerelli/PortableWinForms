namespace System.Windows.Forms
{
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class LinkConverter : TypeConverter
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
            if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
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

                    // Parse 2 integer values - Start & Length of the Link.
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
                        return new LinkLabel.Link(values[0], values[1]);
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

            if (value is LinkLabel.Link)
            {
                if (destinationType == typeof(string))
                {
                    LinkLabel.Link link = (LinkLabel.Link)value;

                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    string sep = culture.TextInfo.ListSeparator + " ";
                    TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
                    string[] args = new string[2];
                    int nArg = 0;

                    args[nArg++] = intConverter.ConvertToString(context, culture, link.Start);
                    args[nArg++] = intConverter.ConvertToString(context, culture, link.Length);

                    return string.Join(sep, args);
                }

                if (destinationType == typeof(InstanceDescriptor))
                {
                    LinkLabel.Link link = (LinkLabel.Link)value;
                    MemberInfo info;
                    if (link.LinkData == null)
                    {
                        info = typeof(LinkLabel.Link).GetConstructor(new Type[] { typeof(int), typeof(int) });
                        if (info != null)
                        {
                            return new InstanceDescriptor(info, new object[] { link.Start, link.Length }, true);
                        }
                    }
                    else
                    {
                        info = typeof(LinkLabel.Link).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(object) });
                        if (info != null)
                        {
                            return new InstanceDescriptor(info, new object[] { link.Start, link.Length, link.LinkData }, true);
                        }
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}