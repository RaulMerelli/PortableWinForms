namespace System.Drawing
{
    using System.IO;
    using System.ComponentModel;
    using System.Globalization;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Versioning;

    public class IconConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(byte[]))
            {
                return true;
            }

            if (sourceType == typeof(InstanceDescriptor))
            {
                return false;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Image) || destinationType == typeof(Bitmap))
            {
                return true;
            }

            if (destinationType == typeof(byte[]))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is byte[])
            {
                MemoryStream ms = new MemoryStream((byte[])value);
                return new Icon(ms);
            }

            return base.ConvertFrom(context, culture, value);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(Image) || destinationType == typeof(Bitmap))
            {
                Icon icon = value as Icon;
                if (icon != null)
                {
                    return icon.ToBitmap();
                }
            }
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    return value.ToString();
                }
                else
                {
                    return "toStringNone";
                }
            }
            if (destinationType == typeof(byte[]))
            {
                if (value != null)
                {
                    MemoryStream ms = null;
                    try
                    {
                        ms = new MemoryStream();
                        Icon icon = value as Icon;
                        if (icon != null)
                        {
                            icon.Save(ms);
                        }
                    }
                    finally
                    {
                        if (ms != null)
                        {
                            ms.Close();
                        }
                    }
                    if (ms != null)
                    {
                        return ms.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return new byte[0];
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
