﻿namespace System.Windows.Forms
{
    using System;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;

    public class ListBindingConverter : TypeConverter
    {

        private static Type[] ctorTypes = null;  // the list of type of our ctor parameters.
        private static string[] ctorParamProps = null; // the name of each property to check to see if we need to init with a ctor.

        private static Type[] ConstructorParamaterTypes
        {
            get
            {
                if (ctorTypes == null)
                {
                    ctorTypes = new Type[] { typeof(string), typeof(object), typeof(string), typeof(bool), typeof(DataSourceUpdateMode), typeof(object), typeof(string), typeof(IFormatProvider) };
                }
                return ctorTypes;
            }
        }


        private static string[] ConstructorParameterProperties
        {
            get
            {
                if (ctorParamProps == null)
                {
                    ctorParamProps = new string[] { null, null, null, "FormattingEnabled", "DataSourceUpdateMode", "NullValue", "FormatString", "FormatInfo", };
                }
                return ctorParamProps;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(InstanceDescriptor) && value is Binding)
            {
                Binding b = (Binding)value;
                return GetInstanceDescriptorFromValues(b);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            try
            {
                return new Binding((string)propertyValues["PropertyName"],
                                           propertyValues["DataSource"],
                                   (string)propertyValues["DataMember"]);
            }
            catch (InvalidCastException invalidCast)
            {
                //throw new ArgumentException(SR.GetString(SR.PropertyValueInvalidEntry), invalidCast);
                throw new ArgumentException("PropertyValueInvalidEntry: invalidCast");
            }
            catch (NullReferenceException nullRef)
            {
                //throw new ArgumentException(SR.GetString(SR.PropertyValueInvalidEntry), nullRef);
                throw new ArgumentException("PropertyValueInvalidEntry nullRef");
            }
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private InstanceDescriptor GetInstanceDescriptorFromValues(Binding b)
        {

            // The BindingFormattingDialog turns on Binding::FormattingEnabled property.
            // however, when the user data binds a property using the PropertyBrowser, Binding::FormattingEnabled is set to false
            // The Binding class is not a component class, so we don't have the ComponentInitialize method where we can set FormattingEnabled to true
            // so we set it here. VsWhidbey 241361
            b.FormattingEnabled = true;

            bool isComplete = true;
            int lastItem = ConstructorParameterProperties.Length - 1;

            for (; lastItem >= 0; lastItem--)
            {

                // null means no prop is available, we quit here.
                //
                if (ConstructorParameterProperties[lastItem] == null)
                {
                    break;
                }

                // get the property and see if it needs to be serialized.
                //
                PropertyDescriptor prop = TypeDescriptor.GetProperties(b)[ConstructorParameterProperties[lastItem]];
                if (prop != null && prop.ShouldSerializeValue(b))
                {
                    break;
                }
            }

            // now copy the type array up to the point we quit.
            //
            Type[] ctorParams = new Type[lastItem + 1];
            Array.Copy(ConstructorParamaterTypes, 0, ctorParams, 0, ctorParams.Length);

            // Get the ctor info.
            //
            ConstructorInfo ctor = typeof(Binding).GetConstructor(ctorParams);
            Debug.Assert(ctor != null, "Failed to find Binding ctor for types!");
            if (ctor == null)
            {
                isComplete = false;
                ctor = typeof(Binding).GetConstructor(new Type[] {
                   typeof(string),
                   typeof(object),
                   typeof(string)});
            }

            // now fill in the values.
            //
            object[] values = new object[ctorParams.Length];

            for (int i = 0; i < values.Length; i++)
            {
                object val = null;
                switch (i)
                {
                    case 0:
                        val = b.PropertyName;
                        break;
                    case 1:
                        val = b.BindToObject.DataSource;
                        break;
                    case 2:
                        val = b.BindToObject.BindingMemberInfo.BindingMember;
                        break;
                    default:
                        val = TypeDescriptor.GetProperties(b)[ConstructorParameterProperties[i]].GetValue(b);
                        break;
                }
                values[i] = val;
            }
            return new InstanceDescriptor(ctor, values, isComplete);
        }
    }
}