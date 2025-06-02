namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms.Design;

    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
    public class PropertiesTab : PropertyTab
    {


        public override string TabName
        {
            get
            {
                return "PBRSToolTipProperties";
            }
        }

        public override string HelpKeyword
        {
            get
            {
                return "vs.properties"; // do not localize.
            }
        }

        public override PropertyDescriptor GetDefaultProperty(object obj)
        {
            PropertyDescriptor def = base.GetDefaultProperty(obj);

            if (def == null)
            {
                PropertyDescriptorCollection props = GetProperties(obj);
                if (props != null)
                {
                    for (int i = 0; i < props.Count; i++)
                    {
                        if ("Name".Equals(props[i].Name))
                        {
                            def = props[i];
                            break;
                        }
                    }
                }
            }
            return def;
        }

        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            return GetProperties(null, component, attributes);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            if (attributes == null)
            {
                attributes = new Attribute[] { BrowsableAttribute.Yes };
            }

            if (context == null)
            {
                return TypeDescriptor.GetProperties(component, attributes);
            }
            else
            {
                TypeConverter tc = (context.PropertyDescriptor == null ? TypeDescriptor.GetConverter(component) : context.PropertyDescriptor.Converter);
                if (tc == null || !tc.GetPropertiesSupported(context))
                {
                    return TypeDescriptor.GetProperties(component, attributes);
                }
                else
                {
                    return tc.GetProperties(context, component, attributes);
                }
            }
        }
    }
}