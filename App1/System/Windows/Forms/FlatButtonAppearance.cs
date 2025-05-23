namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.ComponentModel;
    using System.Windows.Forms.Layout;

    [TypeConverter(typeof(FlatButtonAppearanceConverter))]
    public class FlatButtonAppearance
    {

        private ButtonBase owner;

        private int borderSize = 1;
        private Color borderColor = Color.Empty;
        private Color checkedBackColor = Color.Empty;
        private Color mouseDownBackColor = Color.Empty;
        private Color mouseOverBackColor = Color.Empty;

        internal FlatButtonAppearance(ButtonBase owner)
        {
            this.owner = owner;
        }

        [
        Browsable(true),
        ApplicableToButton(),
        NotifyParentProperty(true),
        EditorBrowsable(EditorBrowsableState.Always),
        DefaultValue(1),
        ]
        public int BorderSize
        {
            get
            {
                return borderSize;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("BorderSize", value, "InvalidLowBoundArgumentEx");

                if (borderSize != value)
                {
                    borderSize = value;
                    if (owner != null && owner.ParentInternal != null)
                    {
                        LayoutTransaction.DoLayoutIf(owner.AutoSize, owner.ParentInternal, owner, PropertyNames.FlatAppearanceBorderSize);
                    }
                    owner.Invalidate();
                }
            }
        }

        [
        Browsable(true),
        ApplicableToButton(),
        NotifyParentProperty(true),
        EditorBrowsable(EditorBrowsableState.Always),
        DefaultValue(typeof(Color), ""),
        ]
        public Color BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                if (value.Equals(Color.Transparent))
                {
                    throw new NotSupportedException("SButtonFlatAppearanceInvalidBorderColor");
                }

                if (borderColor != value)
                {
                    borderColor = value;
                    owner.Invalidate();
                }
            }
        }

        [
        Browsable(true),
        NotifyParentProperty(true),
        EditorBrowsable(EditorBrowsableState.Always),
        DefaultValue(typeof(Color), ""),
        ]
        public Color CheckedBackColor
        {
            get
            {
                return checkedBackColor;
            }
            set
            {
                if (checkedBackColor != value)
                {
                    checkedBackColor = value;
                    owner.Invalidate();
                }
            }
        }

        [
        Browsable(true),
        ApplicableToButton(),
        NotifyParentProperty(true),
        EditorBrowsable(EditorBrowsableState.Always),
        DefaultValue(typeof(Color), ""),
        ]
        public Color MouseDownBackColor
        {
            get
            {
                return mouseDownBackColor;
            }
            set
            {
                if (mouseDownBackColor != value)
                {
                    mouseDownBackColor = value;
                    owner.Invalidate();
                }
            }
        }

        [
        Browsable(true),
        ApplicableToButton(),
        NotifyParentProperty(true),
        EditorBrowsable(EditorBrowsableState.Always),
        DefaultValue(typeof(Color), ""),
        ]
        public Color MouseOverBackColor
        {
            get
            {
                return mouseOverBackColor;
            }
            set
            {
                if (mouseOverBackColor != value)
                {
                    mouseOverBackColor = value;
                    owner.Invalidate();
                }
            }
        }

    }

    internal sealed class ApplicableToButtonAttribute : Attribute
    {
        public ApplicableToButtonAttribute()
        {
        }
    }

    internal class FlatButtonAppearanceConverter : ExpandableObjectConverter
    {

        // Don't let the property grid display the full type name in the value cell
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        // Don't let the property grid display the CheckedBackColor property for Button controls
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (context != null && context.Instance is Button)
            {
                Attribute[] attributes2 = new Attribute[attributes.Length + 1];
                attributes.CopyTo(attributes2, 0);
                attributes2[attributes.Length] = new ApplicableToButtonAttribute();
                attributes = attributes2;
            }

            return TypeDescriptor.GetProperties(value, attributes);
        }

    }

}