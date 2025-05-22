namespace System.Drawing
{
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;

    public class ColorConverter : TypeConverter
    {
        private static string ColorConstantsLock = "colorConstants";
        private static Hashtable colorConstants;
        private static string SystemColorConstantsLock = "systemColorConstants";
        private static Hashtable systemColorConstants;
        private static string ValuesLock = "values";
        private static StandardValuesCollection values;

        public ColorConverter()
        {
        }

        private static Hashtable Colors
        {
            get
            {
                if (colorConstants == null)
                {
                    lock (ColorConstantsLock)
                    {
                        if (colorConstants == null)
                        {
                            Hashtable tempHash = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            FillConstants(tempHash, typeof(Color));
                            colorConstants = tempHash;
                        }
                    }
                }

                return colorConstants;
            }
        }

        private static Hashtable SystemColors
        {
            get
            {
                if (systemColorConstants == null)
                {
                    lock (SystemColorConstantsLock)
                    {
                        if (systemColorConstants == null)
                        {
                            Hashtable tempHash = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            FillConstants(tempHash, typeof(System.Drawing.SystemColors));
                            systemColorConstants = tempHash;
                        }
                    }
                }

                return systemColorConstants;
            }
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

        internal static object GetNamedColor(string name)
        {
            object color = null;
            // First, check to see if this is a standard name.
            //
            color = Colors[name];
            if (color != null)
            {
                return color;
            }
            // Ok, how about a system color?
            //
            color = SystemColors[name];
            return color;
        }

        [SuppressMessage("Microsoft.Performance", "CA1808:AvoidCallsThatBoxValueTypes")]
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string strValue = value as string;
            if (strValue != null)
            {
                object obj = null;
                string text = strValue.Trim();

                if (text.Length == 0)
                {
                    obj = Color.Empty;
                }
                else
                {
                    // First, check to see if this is a standard name.
                    //
                    obj = GetNamedColor(text);

                    if (obj == null)
                    {
                        if (culture == null)
                        {
                            culture = CultureInfo.CurrentCulture;
                        }

                        char sep = culture.TextInfo.ListSeparator[0];
                        bool tryMappingToKnownColor = true;

                        TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));

                        // If the value is a 6 digit hex number only, then
                        // we want to treat the Alpha as 255, not 0
                        //
                        if (text.IndexOf(sep) == -1)
                        {

                            // text can be '' (empty quoted string)
                            if (text.Length >= 2 && (text[0] == '\'' || text[0] == '"') && text[0] == text[text.Length - 1])
                            {
                                // In quotes means a named value
                                string colorName = text.Substring(1, text.Length - 2);
                                obj = Color.FromName(colorName);
                                tryMappingToKnownColor = false;
                            }
                            else if ((text.Length == 7 && text[0] == '#') ||
                                     (text.Length == 8 && (text.StartsWith("0x") || text.StartsWith("0X"))) ||
                                     (text.Length == 8 && (text.StartsWith("&h") || text.StartsWith("&H"))))
                            {
                                // Note: ConvertFromString will raise exception if value cannot be converted.
                                obj = Color.FromArgb(unchecked((int)(0xFF000000 | (uint)(int)intConverter.ConvertFromString(context, culture, text))));
                            }
                        }

                        // Nope.  Parse the RGBA from the text.
                        //
                        if (obj == null)
                        {
                            string[] tokens = text.Split(new char[] { sep });
                            int[] values = new int[tokens.Length];
                            for (int i = 0; i < values.Length; i++)
                            {
                                values[i] = unchecked((int)intConverter.ConvertFromString(context, culture, tokens[i]));
                            }

                            // We should now have a number of parsed integer values.
                            // We support 1, 3, or 4 arguments:
                            //
                            // 1 -- full ARGB encoded
                            // 3 -- RGB
                            // 4 -- ARGB
                            //
                            switch (values.Length)
                            {
                                case 1:
                                    obj = Color.FromArgb(values[0]);
                                    break;

                                case 3:
                                    obj = Color.FromArgb(values[0], values[1], values[2]);
                                    break;

                                case 4:
                                    obj = Color.FromArgb(values[0], values[1], values[2], values[3]);
                                    break;
                            }
                            tryMappingToKnownColor = true;
                        }

                        if ((obj != null) && tryMappingToKnownColor)
                        {

                            // Now check to see if this color matches one of our known colors.
                            // If it does, then substitute it.  We can only do this for "Colors"
                            // because system colors morph with user settings.
                            //
                            int targetARGB = ((Color)obj).ToArgb();

                            foreach (Color c in Colors.Values)
                            {
                                if (c.ToArgb() == targetARGB)
                                {
                                    obj = c;
                                    break;
                                }
                            }
                        }
                    }

                    if (obj == null)
                    {
                        //throw new ArgumentException(SR.GetString(SR.InvalidColor, text));
                        throw new ArgumentException("InvalidColor");
                    }
                }
                return obj;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (value is Color)
            {
                if (destinationType == typeof(string))
                {
                    Color c = (Color)value;

                    if (c == Color.Empty)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        // If this is a known color, then Color can provide its own
                        // name.  Otherwise, we fabricate an ARGB value for it.
                        //
                        if (c.IsKnownColor)
                        {
                            return c.Name;
                        }
                        else if (c.IsNamedColor)
                        {
                            return "'" + c.Name + "'";
                        }
                        else
                        {
                            if (culture == null)
                            {
                                culture = CultureInfo.CurrentCulture;
                            }
                            string sep = culture.TextInfo.ListSeparator + " ";
                            TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
                            string[] args;
                            int nArg = 0;

                            if (c.A < 255)
                            {
                                args = new string[4];
                                args[nArg++] = intConverter.ConvertToString(context, culture, (object)c.A);
                            }
                            else
                            {
                                args = new string[3];
                            }

                            // Note: ConvertToString will raise exception if value cannot be converted.
                            args[nArg++] = intConverter.ConvertToString(context, culture, (object)c.R);
                            args[nArg++] = intConverter.ConvertToString(context, culture, (object)c.G);
                            args[nArg++] = intConverter.ConvertToString(context, culture, (object)c.B);

                            // Now slam all of these together with the fantastic Join 
                            // method.
                            //
                            return string.Join(sep, args);
                        }
                    }
                }
                if (destinationType == typeof(InstanceDescriptor))
                {
                    MemberInfo member = null;
                    object[] args = null;

                    Color c = (Color)value;

                    if (c.IsEmpty)
                    {
                        member = typeof(Color).GetField("Empty");
                    }
                    else if (c.IsSystemColor)
                    {
                        member = typeof(SystemColors).GetProperty(c.Name);
                    }
                    else if (c.IsKnownColor)
                    {
                        member = typeof(Color).GetProperty(c.Name);
                    }
                    else if (c.A != 255)
                    {
                        member = typeof(Color).GetMethod("FromArgb", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) });
                        args = new object[] { c.A, c.R, c.G, c.B };
                    }
                    else if (c.IsNamedColor)
                    {
                        member = typeof(Color).GetMethod("FromName", new Type[] { typeof(string) });
                        args = new object[] { c.Name };
                    }
                    else
                    {
                        member = typeof(Color).GetMethod("FromArgb", new Type[] { typeof(int), typeof(int), typeof(int) });
                        args = new object[] { c.R, c.G, c.B };
                    }

                    Debug.Assert(member != null, "Could not convert color to member.  Did someone change method name / signature and not update Colorconverter?");
                    if (member != null)
                    {
                        return new InstanceDescriptor(member, args);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static void FillConstants(Hashtable hash, Type enumType)
        {
            MethodAttributes attrs = MethodAttributes.Public | MethodAttributes.Static;
            PropertyInfo[] props = enumType.GetProperties();

            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo prop = props[i];
                if (prop.PropertyType == typeof(Color))
                {
                    MethodInfo method = prop.GetGetMethod();
                    if (method != null && (method.Attributes & attrs) == attrs)
                    {
                        object[] tempIndex = null;
                        hash[prop.Name] = prop.GetValue(null, tempIndex);
                    }
                }
            }
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (values == null)
            {
                lock (ValuesLock)
                {
                    if (values == null)
                    {

                        // We must take the value from each hashtable and combine them.
                        //
                        ArrayList arrayValues = new ArrayList();
                        arrayValues.AddRange(Colors.Values);
                        arrayValues.AddRange(SystemColors.Values);

                        // Now, we have a couple of colors that have the same names but
                        // are identical values.  Look for these and remove them.  Too
                        // bad this is n^2.
                        //
                        int count = arrayValues.Count;
                        for (int i = 0; i < count - 1; i++)
                        {
                            for (int j = i + 1; j < count; j++)
                            {
                                if (arrayValues[i].Equals(arrayValues[j]))
                                {
                                    // Remove this item!
                                    //
                                    arrayValues.RemoveAt(j);
                                    count--;
                                    j--;
                                }
                            }
                        }

                        // Sort the array.
                        //
                        arrayValues.Sort(0, arrayValues.Count, new ColorComparer());
                        values = new StandardValuesCollection(arrayValues.ToArray());
                    }
                }
            }

            return values;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private class ColorComparer : IComparer
        {

            public int Compare(object left, object right)
            {
                Color cLeft = (Color)left;
                Color cRight = (Color)right;
                return string.Compare(cLeft.Name, cRight.Name, false, CultureInfo.InvariantCulture);
            }
        }
    }
}