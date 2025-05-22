namespace System.Windows.Forms
{

    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;

    internal class PropertyStore
    {

        private static int currentKey;

        private IntegerEntry[] intEntries;
        private ObjectEntry[] objEntries;

        public bool ContainsInteger(int key)
        {
            bool found;
            GetInteger(key, out found);
            return found;
        }

        public bool ContainsObject(int key)
        {
            bool found;
            GetObject(key, out found);
            return found;
        }

        public static int CreateKey()
        {
            return currentKey++;
        }

        public Color GetColor(int key)
        {
            bool found;
            return GetColor(key, out found);
        }

        // this is a wrapper around GetObject designed to 
        // reduce the boxing hit
        public Color GetColor(int key, out bool found)
        {
            object storedObject = GetObject(key, out found);

            if (found)
            {
                ColorWrapper wrapper = storedObject as ColorWrapper;

                if (wrapper != null)
                {
                    return wrapper.Color;
                }
#if DEBUG
                else if (storedObject != null)
                {
                    Debug.Fail("Have non-null object that isnt a color wrapper stored in a color entry!\r\nDid someone SetObject instead of SetColor?");
                }
#endif 
            }
            // we didnt actually find a non-null color wrapper.
            found = false;
            return Color.Empty;
        }


        public Padding GetPadding(int key)
        {
            bool found;
            return GetPadding(key, out found);
        }

        // this is a wrapper around GetObject designed to 
        // reduce the boxing hit
        public Padding GetPadding(int key, out bool found)
        {
            object storedObject = GetObject(key, out found);

            if (found)
            {
                PaddingWrapper wrapper = storedObject as PaddingWrapper;


                if (wrapper != null)
                {
                    return wrapper.Padding;
                }
#if DEBUG
                else if (storedObject != null)
                {
                    Debug.Fail("Have non-null object that isnt a padding wrapper stored in a padding entry!\r\nDid someone SetObject instead of SetPadding?");
                }
#endif                
            }

            // we didnt actually find a non-null padding wrapper.
            found = false;
            return Padding.Empty;
        }
#if false // FXCOP currently not used 
        public Size GetSize(int key) {
            bool found;
            return GetSize(key, out found);                
        }
#endif

        // this is a wrapper around GetObject designed to 
        // reduce the boxing hit
        public Size GetSize(int key, out bool found)
        {
            object storedObject = GetObject(key, out found);

            if (found)
            {
                SizeWrapper wrapper = storedObject as SizeWrapper;
                if (wrapper != null)
                {
                    return wrapper.Size;
                }
#if DEBUG
                else if (storedObject != null)
                {
                    Debug.Fail("Have non-null object that isnt a padding wrapper stored in a padding entry!\r\nDid someone SetObject instead of SetPadding?");
                }
#endif
            }
            // we didnt actually find a non-null size wrapper.
            found = false;
            return Size.Empty;
        }

        public Rectangle GetRectangle(int key)
        {
            bool found;
            return GetRectangle(key, out found);
        }

        // this is a wrapper around GetObject designed to 
        // reduce the boxing hit
        public Rectangle GetRectangle(int key, out bool found)
        {
            object storedObject = GetObject(key, out found);

            if (found)
            {
                RectangleWrapper wrapper = storedObject as RectangleWrapper;
                if (wrapper != null)
                {
                    return wrapper.Rectangle;
                }
#if DEBUG                
                else if (storedObject != null)
                {
                    Debug.Fail("Have non-null object that isnt a Rectangle wrapper stored in a Rectangle entry!\r\nDid someone SetObject instead of SetRectangle?");
                }
#endif                
            }
            // we didnt actually find a non-null rectangle wrapper.
            found = false;
            return Rectangle.Empty;
        }

        public int GetInteger(int key)
        {
            bool found;
            return GetInteger(key, out found);
        }

        public int GetInteger(int key, out bool found)
        {

            int value = 0;
            int index;
            short element;
            short keyIndex = SplitKey(key, out element);

            found = false;

            if (LocateIntegerEntry(keyIndex, out index))
            {
                // We have found the relevant entry.  See if
                // the bitmask indicates the value is used.
                //
                if (((1 << element) & intEntries[index].Mask) != 0)
                {

                    found = true;

                    switch (element)
                    {
                        case 0:
                            value = intEntries[index].Value1;
                            break;

                        case 1:
                            value = intEntries[index].Value2;
                            break;

                        case 2:
                            value = intEntries[index].Value3;
                            break;

                        case 3:
                            value = intEntries[index].Value4;
                            break;

                        default:
                            Debug.Fail("Invalid element obtained from LocateIntegerEntry");
                            break;
                    }
                }
            }

            return value;
        }

        public object GetObject(int key)
        {
            bool found;
            return GetObject(key, out found);
        }

        public object GetObject(int key, out bool found)
        {

            object value = null;
            int index;
            short element;
            short keyIndex = SplitKey(key, out element);

            found = false;

            if (LocateObjectEntry(keyIndex, out index))
            {
                // We have found the relevant entry.  See if
                // the bitmask indicates the value is used.
                //
                if (((1 << element) & objEntries[index].Mask) != 0)
                {

                    found = true;

                    switch (element)
                    {
                        case 0:
                            value = objEntries[index].Value1;
                            break;

                        case 1:
                            value = objEntries[index].Value2;
                            break;

                        case 2:
                            value = objEntries[index].Value3;
                            break;

                        case 3:
                            value = objEntries[index].Value4;
                            break;

                        default:
                            Debug.Fail("Invalid element obtained from LocateObjectEntry");
                            break;
                    }
                }
            }

            return value;
        }


        private bool LocateIntegerEntry(short entryKey, out int index)
        {
            if (intEntries != null)
            {
                int length = intEntries.Length;
                if (length <= 16)
                {
                    //if the array is small enough, we unroll the binary search to be more efficient.
                    //usually the performance gain is around 10% to 20%
                    //DON'T change this code unless you are very confident!
                    index = 0;
                    int midPoint = length / 2;
                    if (intEntries[midPoint].Key <= entryKey)
                    {
                        index = midPoint;
                    }
                    //we don't move this inside the previous if branch since this catches both the case
                    //index == 0 and index = midPoint
                    if (intEntries[index].Key == entryKey)
                    {
                        return true;
                    }

                    midPoint = (length + 1) / 4;
                    if (intEntries[index + midPoint].Key <= entryKey)
                    {
                        index += midPoint;
                        if (intEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }

                    midPoint = (length + 3) / 8;
                    if (intEntries[index + midPoint].Key <= entryKey)
                    {
                        index += midPoint;
                        if (intEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }

                    midPoint = (length + 7) / 16;
                    if (intEntries[index + midPoint].Key <= entryKey)
                    {
                        index += midPoint;
                        if (intEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }

                    Debug.Assert(index < length);
                    if (entryKey > intEntries[index].Key)
                    {
                        index++;
                    }
                    Debug_VerifyLocateIntegerEntry(index, entryKey, length);
                    return false;
                }
                else
                {
                    // Entries are stored in numerical order by key index so we can
                    // do a binary search on them.
                    //
                    int max = length - 1;
                    int min = 0;
                    int idx = 0;

                    do
                    {
                        idx = (max + min) / 2;
                        short currentKeyIndex = intEntries[idx].Key;

                        if (currentKeyIndex == entryKey)
                        {
                            index = idx;
                            return true;
                        }
                        else if (entryKey < currentKeyIndex)
                        {
                            max = idx - 1;
                        }
                        else
                        {
                            min = idx + 1;
                        }
                    }
                    while (max >= min);

                    // Didn't find the index.  Setup our output
                    // appropriately
                    //
                    index = idx;
                    if (entryKey > intEntries[idx].Key)
                    {
                        index++;
                    }
                    return false;
                }
            }
            else
            {
                index = 0;
                return false;
            }
        }

        private bool LocateObjectEntry(short entryKey, out int index)
        {
            if (objEntries != null)
            {
                int length = objEntries.Length;
                Debug.Assert(length > 0);
                if (length <= 16)
                {
                    //if the array is small enough, we unroll the binary search to be more efficient.
                    //usually the performance gain is around 10% to 20%
                    //DON'T change this code unless you are very confident!
                    index = 0;
                    int midPoint = length / 2;
                    if (objEntries[midPoint].Key <= entryKey)
                    {
                        index = midPoint;
                    }
                    //we don't move this inside the previous if branch since this catches both the case
                    //index == 0 and index = midPoint
                    if (objEntries[index].Key == entryKey)
                    {
                        return true;
                    }

                    midPoint = (length + 1) / 4;
                    if (objEntries[index + midPoint].Key <= entryKey)
                    {
                        index += midPoint;
                        if (objEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }

                    midPoint = (length + 3) / 8;
                    if (objEntries[index + midPoint].Key <= entryKey)
                    {
                        index += midPoint;
                        if (objEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }

                    midPoint = (length + 7) / 16;
                    if (objEntries[index + midPoint].Key <= entryKey)
                    {
                        index += midPoint;
                        if (objEntries[index].Key == entryKey)
                        {
                            return true;
                        }
                    }

                    Debug.Assert(index < length);
                    if (entryKey > objEntries[index].Key)
                    {
                        index++;
                    }
                    Debug_VerifyLocateObjectEntry(index, entryKey, length);
                    return false;
                }
                else
                {
                    // Entries are stored in numerical order by key index so we can
                    // do a binary search on them.
                    //
                    int max = length - 1;
                    int min = 0;
                    int idx = 0;

                    do
                    {
                        idx = (max + min) / 2;
                        short currentKeyIndex = objEntries[idx].Key;

                        if (currentKeyIndex == entryKey)
                        {
                            index = idx;
                            return true;
                        }
                        else if (entryKey < currentKeyIndex)
                        {
                            max = idx - 1;
                        }
                        else
                        {
                            min = idx + 1;
                        }
                    }
                    while (max >= min);

                    // Didn't find the index.  Setup our output
                    // appropriately
                    //
                    index = idx;
                    if (entryKey > objEntries[idx].Key)
                    {
                        index++;
                    }
                    return false;
                }
            }
            else
            {
                index = 0;
                return false;
            }
        }
        /*
                public Color RemoveColor(int key) {
                    RemoveObject(key);
                }
        */
        public void RemoveInteger(int key)
        {
            int index;
            short element;
            short entryKey = SplitKey(key, out element);

            if (LocateIntegerEntry(entryKey, out index))
            {
                if (((1 << element) & intEntries[index].Mask) == 0)
                {
                    // this element is not being used - return right away
                    return;
                }

                // declare that the element is no longer used
                intEntries[index].Mask &= (short)(~((short)(1 << element)));

                if (intEntries[index].Mask == 0)
                {
                    // this object entry is no longer in use - let's remove it all together
                    // not great for perf but very simple and we don't expect to remove much
                    IntegerEntry[] newEntries = new IntegerEntry[intEntries.Length - 1];
                    if (index > 0)
                    {
                        Array.Copy(intEntries, 0, newEntries, 0, index);
                    }
                    if (index < newEntries.Length)
                    {
                        Debug.Assert(intEntries.Length - index - 1 > 0);
                        Array.Copy(intEntries, index + 1, newEntries, index, intEntries.Length - index - 1);
                    }
                    intEntries = newEntries;
                }
                else
                {
                    // this object entry is still in use - let's just clean up the deleted element
                    switch (element)
                    {
                        case 0:
                            intEntries[index].Value1 = 0;
                            break;

                        case 1:
                            intEntries[index].Value2 = 0;
                            break;

                        case 2:
                            intEntries[index].Value3 = 0;
                            break;

                        case 3:
                            intEntries[index].Value4 = 0;
                            break;

                        default:
                            Debug.Fail("Invalid element obtained from LocateIntegerEntry");
                            break;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void RemoveObject(int key)
        {
            int index;
            short element;
            short entryKey = SplitKey(key, out element);

            if (LocateObjectEntry(entryKey, out index))
            {
                if (((1 << element) & objEntries[index].Mask) == 0)
                {
                    // this element is not being used - return right away
                    return;
                }

                // declare that the element is no longer used
                objEntries[index].Mask &= (short)(~((short)(1 << element)));

                if (objEntries[index].Mask == 0)
                {
                    // this object entry is no longer in use - let's remove it all together
                    // not great for perf but very simple and we don't expect to remove much
                    if (objEntries.Length == 1)
                    {
                        // instead of allocating an array of length 0, we simply reset the array to null.
                        objEntries = null;
                    }
                    else
                    {
                        ObjectEntry[] newEntries = new ObjectEntry[objEntries.Length - 1];
                        if (index > 0)
                        {
                            Array.Copy(objEntries, 0, newEntries, 0, index);
                        }
                        if (index < newEntries.Length)
                        {
                            Debug.Assert(objEntries.Length - index - 1 > 0);
                            Array.Copy(objEntries, index + 1, newEntries, index, objEntries.Length - index - 1);
                        }
                        objEntries = newEntries;
                    }
                }
                else
                {
                    // this object entry is still in use - let's just clean up the deleted element
                    switch (element)
                    {
                        case 0:
                            objEntries[index].Value1 = null;
                            break;

                        case 1:
                            objEntries[index].Value2 = null;
                            break;

                        case 2:
                            objEntries[index].Value3 = null;
                            break;

                        case 3:
                            objEntries[index].Value4 = null;
                            break;

                        default:
                            Debug.Fail("Invalid element obtained from LocateObjectEntry");
                            break;
                    }
                }
            }
        }

        public void SetColor(int key, Color value)
        {
            bool found;
            object storedObject = GetObject(key, out found);

            if (!found)
            {
                SetObject(key, new ColorWrapper(value));
            }
            else
            {
                ColorWrapper wrapper = storedObject as ColorWrapper;
                if (wrapper != null)
                {
                    // re-using the wrapper reduces the boxing hit.
                    wrapper.Color = value;
                }
                else
                {
                    Debug.Assert(storedObject == null, "object should either be null or ColorWrapper"); // could someone have SetObject to this key behind our backs?
                    SetObject(key, new ColorWrapper(value));
                }

            }
        }
        public void SetPadding(int key, Padding value)
        {
            bool found;
            object storedObject = GetObject(key, out found);

            if (!found)
            {
                SetObject(key, new PaddingWrapper(value));
            }
            else
            {
                PaddingWrapper wrapper = storedObject as PaddingWrapper;
                if (wrapper != null)
                {
                    // re-using the wrapper reduces the boxing hit.
                    wrapper.Padding = value;
                }
                else
                {
                    Debug.Assert(storedObject == null, "object should either be null or PaddingWrapper"); // could someone have SetObject to this key behind our backs?
                    SetObject(key, new PaddingWrapper(value));
                }

            }
        }
        public void SetRectangle(int key, Rectangle value)
        {

            bool found;
            object storedObject = GetObject(key, out found);

            if (!found)
            {
                SetObject(key, new RectangleWrapper(value));
            }
            else
            {
                RectangleWrapper wrapper = storedObject as RectangleWrapper;
                if (wrapper != null)
                {
                    // re-using the wrapper reduces the boxing hit.
                    wrapper.Rectangle = value;
                }
                else
                {
                    Debug.Assert(storedObject == null, "object should either be null or RectangleWrapper"); // could someone have SetObject to this key behind our backs?
                    SetObject(key, new RectangleWrapper(value));
                }

            }

        }
        public void SetSize(int key, Size value)
        {

            bool found;
            object storedObject = GetObject(key, out found);

            if (!found)
            {
                SetObject(key, new SizeWrapper(value));
            }
            else
            {
                SizeWrapper wrapper = storedObject as SizeWrapper;
                if (wrapper != null)
                {
                    // re-using the wrapper reduces the boxing hit.
                    wrapper.Size = value;
                }
                else
                {
                    Debug.Assert(storedObject == null, "object should either be null or SizeWrapper"); // could someone have SetObject to this key behind our backs?
                    SetObject(key, new SizeWrapper(value));
                }

            }
        }
        public void SetInteger(int key, int value)
        {
            int index;
            short element;
            short entryKey = SplitKey(key, out element);

            if (!LocateIntegerEntry(entryKey, out index))
            {

                // We must allocate a new entry.
                //
                if (intEntries != null)
                {
                    IntegerEntry[] newEntries = new IntegerEntry[intEntries.Length + 1];

                    if (index > 0)
                    {
                        Array.Copy(intEntries, 0, newEntries, 0, index);
                    }

                    if (intEntries.Length - index > 0)
                    {
                        Array.Copy(intEntries, index, newEntries, index + 1, intEntries.Length - index);
                    }

                    intEntries = newEntries;
                }
                else
                {
                    intEntries = new IntegerEntry[1];
                    Debug.Assert(index == 0, "LocateIntegerEntry should have given us a zero index.");
                }

                intEntries[index].Key = entryKey;
            }

            // Now determine which value to set.
            //
            switch (element)
            {
                case 0:
                    intEntries[index].Value1 = value;
                    break;

                case 1:
                    intEntries[index].Value2 = value;
                    break;

                case 2:
                    intEntries[index].Value3 = value;
                    break;

                case 3:
                    intEntries[index].Value4 = value;
                    break;

                default:
                    Debug.Fail("Invalid element obtained from LocateIntegerEntry");
                    break;
            }

            intEntries[index].Mask = (short)((1 << element) | (ushort)(intEntries[index].Mask));
        }

        public void SetObject(int key, object value)
        {
            int index;
            short element;
            short entryKey = SplitKey(key, out element);

            if (!LocateObjectEntry(entryKey, out index))
            {

                // We must allocate a new entry.
                //
                if (objEntries != null)
                {
                    ObjectEntry[] newEntries = new ObjectEntry[objEntries.Length + 1];

                    if (index > 0)
                    {
                        Array.Copy(objEntries, 0, newEntries, 0, index);
                    }

                    if (objEntries.Length - index > 0)
                    {
                        Array.Copy(objEntries, index, newEntries, index + 1, objEntries.Length - index);
                    }

                    objEntries = newEntries;
                }
                else
                {
                    objEntries = new ObjectEntry[1];
                    Debug.Assert(index == 0, "LocateObjectEntry should have given us a zero index.");
                }

                objEntries[index].Key = entryKey;
            }

            // Now determine which value to set.
            //
            switch (element)
            {
                case 0:
                    objEntries[index].Value1 = value;
                    break;

                case 1:
                    objEntries[index].Value2 = value;
                    break;

                case 2:
                    objEntries[index].Value3 = value;
                    break;

                case 3:
                    objEntries[index].Value4 = value;
                    break;

                default:
                    Debug.Fail("Invalid element obtained from LocateObjectEntry");
                    break;
            }

            objEntries[index].Mask = (short)((ushort)(objEntries[index].Mask) | (1 << element));
        }

        private short SplitKey(int key, out short element)
        {
            element = (short)(key & 0x00000003);
            return (short)(key & 0xFFFFFFFC);
        }

        [Conditional("DEBUG_PROPERTYSTORE")]
        private void Debug_VerifyLocateIntegerEntry(int index, short entryKey, int length)
        {
            int max = length - 1;
            int min = 0;
            int idx = 0;

            do
            {
                idx = (max + min) / 2;
                short currentKeyIndex = intEntries[idx].Key;

                if (currentKeyIndex == entryKey)
                {
                    Debug.Assert(index == idx, "GetIntegerEntry in property store broken. index is " + index + " while it should be " + idx + "length of the array is " + length);
                }
                else if (entryKey < currentKeyIndex)
                {
                    max = idx - 1;
                }
                else
                {
                    min = idx + 1;
                }
            }
            while (max >= min);

            // shouldn't find the index if we run this debug code
            if (entryKey > intEntries[idx].Key)
            {
                idx++;
            }
            Debug.Assert(index == idx, "GetIntegerEntry in property store broken. index is " + index + " while it should be " + idx + "length of the array is " + length);
        }

        [Conditional("DEBUG_PROPERTYSTORE")]
        private void Debug_VerifyLocateObjectEntry(int index, short entryKey, int length)
        {
            int max = length - 1;
            int min = 0;
            int idx = 0;

            do
            {
                idx = (max + min) / 2;
                short currentKeyIndex = objEntries[idx].Key;

                if (currentKeyIndex == entryKey)
                {
                    Debug.Assert(index == idx, "GetObjEntry in property store broken. index is " + index + " while is should be " + idx + "length of the array is " + length);
                }
                else if (entryKey < currentKeyIndex)
                {
                    max = idx - 1;
                }
                else
                {
                    min = idx + 1;
                }
            }
            while (max >= min);

            if (entryKey > objEntries[idx].Key)
            {
                idx++;
            }
            Debug.Assert(index == idx, "GetObjEntry in property store broken. index is " + index + " while is should be " + idx + "length of the array is " + length);
        }

        private struct IntegerEntry
        {
            public short Key;
            public short Mask;  // only lower four bits are used; mask of used values.
            public int Value1;
            public int Value2;
            public int Value3;
            public int Value4;
        }

        private struct ObjectEntry
        {
            public short Key;
            public short Mask;  // only lower four bits are used; mask of used values.
            public object Value1;
            public object Value2;
            public object Value3;
            public object Value4;
        }


        private sealed class ColorWrapper
        {
            public Color Color;
            public ColorWrapper(Color color)
            {
                this.Color = color;
            }
        }


        private sealed class PaddingWrapper
        {
            public Padding Padding;
            public PaddingWrapper(Padding padding)
            {
                this.Padding = padding;
            }
        }
        private sealed class RectangleWrapper
        {
            public Rectangle Rectangle;
            public RectangleWrapper(Rectangle rectangle)
            {
                this.Rectangle = rectangle;
            }
        }
        private sealed class SizeWrapper
        {
            public Size Size;
            public SizeWrapper(Size size)
            {
                this.Size = size;
            }
        }

    }
}