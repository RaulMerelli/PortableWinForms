﻿[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Windows.Forms.WindowsFormsUtils..ctor()")]

namespace System.Windows.Forms
{
    using System.Runtime.Serialization.Formatters;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Drawing.Design;
    using System.ComponentModel;
    using System.Collections;
    using System.Drawing;
    using System.Windows.Forms.Internal;
    using Microsoft.Win32;
    using System.Text;
    using Util = NativeMethods.Util;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Reflection;

    // Miscellaneous Windows Forms utilities
    internal sealed class WindowsFormsUtils
    {

        // A better initializer than Size.Empty to force code using uninitialized to noticably fail.
        public static readonly Size UninitializedSize = new Size(-7199369, -5999471);
        private static bool _targetsAtLeast_v4_5 = RunningOnCheck("TargetsAtLeast_Desktop_V4_5");


        public static readonly ContentAlignment AnyRightAlign = ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight;
        public static readonly ContentAlignment AnyLeftAlign = ContentAlignment.TopLeft | ContentAlignment.MiddleLeft | ContentAlignment.BottomLeft;
        public static readonly ContentAlignment AnyTopAlign = ContentAlignment.TopLeft | ContentAlignment.TopCenter | ContentAlignment.TopRight;
        public static readonly ContentAlignment AnyBottomAlign = ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight;
        public static readonly ContentAlignment AnyMiddleAlign = ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight;
        public static readonly ContentAlignment AnyCenterAlign = ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter;

        //public static Point LastCursorPoint
        //{
        //    get
        //    {
        //        int lastXY = SafeNativeMethods.GetMessagePos();
        //        return new Point(NativeMethods.Util.SignedLOWORD(lastXY), NativeMethods.Util.SignedHIWORD(lastXY));
        //    }
        //}

        //[ResourceExposure(ResourceScope.Process)]
        //[ResourceConsumption(ResourceScope.Process)]
        //public static Graphics CreateMeasurementGraphics()
        //{
        //    return Graphics.FromHdcInternal(WindowsGraphicsCacheManager.MeasurementGraphics.DeviceContext.Hdc);
        //}

        // If you want to know if a piece of text contains one and only one &
        // this is your function.  If you have a character "t" and want match it to &Text
        // Control.IsMnemonic is a better bet.
        public static bool ContainsMnemonic(string text)
        {
            if (text != null)
            {
                int textLength = text.Length;
                int firstAmpersand = text.IndexOf('&', 0);
                if (firstAmpersand >= 0 && firstAmpersand <= /*second to last char=*/textLength - 2)
                {
                    // we found one ampersand and it's either the first character
                    // or the second to last character
                    // or a character in between

                    // We're so close!  make sure we don't have a double ampersand now.
                    int secondAmpersand = text.IndexOf('&', firstAmpersand + 1);
                    if (secondAmpersand == -1)
                    {
                        // didn't find a second one in the string.
                        return true;
                    }
                }
            }
            return false;
        }

        internal static Rectangle ConstrainToScreenWorkingAreaBounds(Rectangle bounds)
        {
            return ConstrainToBounds(Screen.GetWorkingArea(bounds), bounds);
        }

        ///<devdoc> given a rectangle, constrain it to fit onto the current screen.
        ///</devdoc>
        internal static Rectangle ConstrainToScreenBounds(Rectangle bounds)
        {
            return ConstrainToBounds(Screen.FromRectangle(bounds).Bounds, bounds);
        }

        internal static Rectangle ConstrainToBounds(Rectangle constrainingBounds, Rectangle bounds)
        {
            // use screen instead of SystemInformation.WorkingArea for better multimon support.
            if (!constrainingBounds.Contains(bounds))
            {
                // make sure size does not exceed working area.
                bounds.Size = new Size(Math.Min(constrainingBounds.Width - 2, bounds.Width),
                                       Math.Min(constrainingBounds.Height - 2, bounds.Height));

                // X calculations
                //
                // scooch so it will fit on the screen.
                if (bounds.Right > constrainingBounds.Right)
                {
                    // its too far to the right.
                    bounds.X = constrainingBounds.Right - bounds.Width;
                }
                else if (bounds.Left < constrainingBounds.Left)
                {
                    // its too far to the left.
                    bounds.X = constrainingBounds.Left;
                }

                // Y calculations
                //
                // scooch so it will fit on the screen.
                if (bounds.Bottom > constrainingBounds.Bottom)
                {
                    // its too far to the bottom.
                    bounds.Y = constrainingBounds.Bottom - 1 - bounds.Height;
                }
                else if (bounds.Top < constrainingBounds.Top)
                {
                    // its too far to the top.
                    bounds.Y = constrainingBounds.Top;
                }
            }
            return bounds;
        }

        //
        // adds an extra & to to the text so that Fish & Chips can be displayed on a menu item without underlining 
        // anything. This is used in MDIWindowList as we use the MDIChildForm.Text as menu item text. 
        //  Fish & Chips --> Fish && Chips
        internal static string EscapeTextWithAmpersands(string text)
        {
            if (text == null)
            {
                return null;
            }

            int index = text.IndexOf('&');

            if (index == -1)
            {
                return text;
            }

            StringBuilder str = new StringBuilder(text.Substring(0, index));
            for (; index < text.Length; ++index)
            {
                if (text[index] == '&')
                {
                    str.Append("&");
                }
                if (index < text.Length)
                {
                    str.Append(text[index]);
                }
            }
            return str.ToString();
        }

        // helper function for generating information about a particular control
        // use AssertControlInformation if sticking in an assert - then the work
        // to figure out the control info will only be done when the assertion is false.
        internal static string GetControlInformation(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return "Handle is IntPtr.Zero";
            }
            string ret = ""; // in RETAIL just return empty string 
            return ret;
        }

        // only fetch the information on a false assertion.
        internal static string AssertControlInformation(bool condition, Control control)
        {
            if (condition)
            {
                return string.Empty;
            }
            else
            {
                return GetControlInformation(control.Handle);
            }
        }

        // Algorithm suggested by Damien Morton
        internal static int GetCombinedHashCodes(params int[] args)
        {
            const int k = -1640531535;
            int h = -757577119;
            for (int i = 0; i < args.Length; i++)
            {
                h = (args[i] ^ h) * k;
            }
            return h;
        }

        // Retrieves the mnemonic from a given string, or zero if no mnemonic.
        // As used by the Control.Mnemonic to get mnemonic from Control.Text.
        //
        // Boolean argument determines whether returned char is converted to upper
        // case or lower case (always one or the other - case is never preserved).
        public static char GetMnemonic(string text, bool bConvertToUpperCase)
        {
            char mnemonic = (char)0;

            if (text != null)
            {
                int len = text.Length;
                for (int i = 0; i < len - 1; i++)
                {
                    if (text[i] == '&')
                    {
                        if (text[i + 1] == '&')
                        {
                            // we have an escaped &, so we need to skip it.
                            //
                            i++;
                            continue;
                        }
                        if (bConvertToUpperCase)
                        {
                            mnemonic = Char.ToUpper(text[i + 1], CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            mnemonic = Char.ToLower(text[i + 1], CultureInfo.CurrentCulture);
                        }
                        break;
                    }
                }
            }
            return mnemonic;
        }

        // for a given handle, finds the toplevel handle
        //public static HandleRef GetRootHWnd(HandleRef hwnd)
        //{
        //    IntPtr rootHwnd = UnsafeNativeMethods.GetAncestor(new HandleRef(hwnd, hwnd.Handle), NativeMethods.GA_ROOT);
        //    return new HandleRef(hwnd.Wrapper, rootHwnd);
        //}

        //public static HandleRef GetRootHWnd(Control control)
        //{
        //    return GetRootHWnd(new HandleRef(control, control.Handle));
        //}

        // Strips all keyboard mnemonic prefixes from a given string, eg. turning "He&lp" into "Help".
        // Note: Be careful not to call this multiple times on the same string, otherwise you'll turn
        // something like "Fi&sh && Chips" into "Fish & Chips" on the first call, and then "Fish Chips"
        // on the second call.
        public static string TextWithoutMnemonics(string text)
        {
            if (text == null)
            {
                return null;
            }

            int index = text.IndexOf('&');

            if (index == -1)
            {
                return text;
            }

            StringBuilder str = new StringBuilder(text.Substring(0, index));
            for (; index < text.Length; ++index)
            {
                if (text[index] == '&')
                {
                    index++;    // Skip this & and copy the next character instead
                }
                if (index < text.Length)
                {
                    str.Append(text[index]);
                }
            }
            return str.ToString();
        }


        //public static Point TranslatePoint(Point point, Control fromControl, Control toControl)
        //{
        //    NativeMethods.POINT pt = new NativeMethods.POINT(point.X, point.Y);
        //    UnsafeNativeMethods.MapWindowPoints(new HandleRef(fromControl, fromControl.Handle), new HandleRef(toControl, toControl.Handle), pt, 1);
        //    return new Point(pt.x, pt.y);
        //}


        // Compares the strings using invariant culture for Turkish-I support.  Returns true if they match.
        // 
        // If your strings are symbolic (returned from APIs, not from user) the following calls
        // are faster than this method:
        // 
        //  String.Equals(s1, s2, StringComparison.Ordinal)
        //  String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase)
        //
        public static bool SafeCompareStrings(string string1, string string2, bool ignoreCase)
        {
            if ((string1 == null) || (string2 == null))
            {
                // if either key is null, we should return false
                return false;
            }

            // Because String.Compare returns an ordering, it can not terminate early if lengths are not the same.
            // Also, equivalent characters can be encoded in different byte sequences, so it can not necessarily
            // terminate on the first byte which doesn't match.  Hence this optimization.
            if (string1.Length != string2.Length)
            {
                return false;
            }

            return String.Compare(string1, string2, ignoreCase, CultureInfo.InvariantCulture) == 0;
        }

        // RotateLeft(0xFF000000, 4) -> 0xF000000F
        public static int RotateLeft(int value, int nBits)
        {
            Debug.Assert(Marshal.SizeOf(typeof(int)) == 4, "The impossible has happened.");

            nBits = nBits % 32;
            return value << nBits | (value >> (32 - nBits));
        }

        public static string GetComponentName(IComponent component, string defaultNameValue)
        {
            Debug.Assert(component != null, "component passed here cannot be null");
            string result = string.Empty;
            if (string.IsNullOrEmpty(defaultNameValue))
            {
                if (component.Site != null)
                {
                    result = component.Site.Name;
                }
                if (result == null)
                {
                    result = string.Empty;
                }
            }
            else
            {
                result = defaultNameValue;
            }
            return result;
        }

        public static class EnumValidator
        {

            // IsValidContentAlignment
            // Valid values are 0x001,0x002,0x004, 0x010,0x020,0x040, 0x100, 0x200,0x400
            // Method for verifying
            //   Verify that the number passed in has only one bit on
            //   Verify that the bit that is on is a valid bit by bitwise anding it to a mask.
            //
            public static bool IsValidContentAlignment(ContentAlignment contentAlign)
            {
                if (ClientUtils.GetBitCount((uint)contentAlign) != 1)
                {
                    return false;
                }
                // to calculate:
                // foreach (int val in Enum.GetValues(typeof(ContentAlignment))) { mask |= val; }
                int contentAlignmentMask = 0x777;
                return ((contentAlignmentMask & (int)contentAlign) != 0);
            }

            // IsEnumWithinShiftedRange
            // shifts off the number of bits specified by numBitsToShift 
            //   -  makes sure the bits we've shifted off are just zeros
            //   -  then compares if the resulting value is between minValAfterShift and maxValAfterShift
            //  
            // EXAMPLE:
            //    MessageBoxIcon.  Valid values are 0x0, 0x10, 0x20, 0x30, 0x40
            //    Method for verifying: chop off the last 0 by shifting right 4 bits, verify resulting number is between 0 & 4.
            //
            //  WindowsFormsUtils.EnumValidator.IsEnumWithinShiftedRange(icon, /*numBitsToShift*/4, /*min*/0x0,/*max*/0x4)
            //
            public static bool IsEnumWithinShiftedRange(Enum enumValue, int numBitsToShift, int minValAfterShift, int maxValAfterShift)
            {
                int iValue = Convert.ToInt32(enumValue, CultureInfo.InvariantCulture);
                int remainder = iValue >> numBitsToShift;
                if (remainder << numBitsToShift != iValue)
                {
                    // there were bits that we shifted out.
                    return false;
                }
                return (remainder >= minValAfterShift && remainder <= maxValAfterShift);
            }

            // IsValidTextImageRelation
            // valid values are 0,1,2,4,8
            // Method for verifying
            //   Verify that the number is between 0 and 8
            //   Verify that the bit that is on - thus forcing it to be a power of two.
            //          
            public static bool IsValidTextImageRelation(TextImageRelation relation)
            {
                return ClientUtils.IsEnumValid(relation, (int)relation, (int)TextImageRelation.Overlay, (int)TextImageRelation.TextBeforeImage, 1);
            }

            public static bool IsValidArrowDirection(ArrowDirection direction)
            {
                switch (direction)
                {
                    case ArrowDirection.Up:
                    case ArrowDirection.Down:
                    case ArrowDirection.Left:
                    case ArrowDirection.Right:
                        return true;
                    default:
                        return false;
                }
            }
        }

        // To enumerate over only part of an array.
        public class ArraySubsetEnumerator : IEnumerator
        {
            private object[] array; // Perhaps this should really be typed Array, but then we suffer a performance penalty.
            private int total;
            private int current;

            public ArraySubsetEnumerator(object[] array, int count)
            {
                Debug.Assert(count == 0 || array != null, "if array is null, count should be 0");
                Debug.Assert(array == null || count <= array.Length, "Trying to enumerate more than the array contains");
                this.array = array;
                this.total = count;
                current = -1;
            }

            public bool MoveNext()
            {
                if (current < total - 1)
                {
                    current++;
                    return true;
                }
                else
                    return false;
            }

            public void Reset()
            {
                current = -1;
            }

            public object Current
            {
                get
                {
                    if (current == -1)
                        return null;
                    else
                        return array[current];
                }
            }
        }

        internal class ReadOnlyControlCollection : Control.ControlCollection
        {

            private readonly bool _isReadOnly;


            public ReadOnlyControlCollection(Control owner, bool isReadOnly) : base(owner)
            {
                _isReadOnly = isReadOnly;
            }

            public override void Add(Control value)
            {
                if (IsReadOnly)
                {
                    throw new NotSupportedException("ReadonlyControlsCollection");
                }
                AddInternal(value);
            }

            internal virtual void AddInternal(Control value)
            {
                base.Add(value);
            }

            public override void Clear()
            {
                if (IsReadOnly)
                {
                    throw new NotSupportedException("ReadonlyControlsCollection");
                }
                base.Clear();
            }

            internal virtual void RemoveInternal(Control value)
            {
                base.Remove(value);
            }

            //public override void RemoveByKey(string key)
            //{
            //    if (IsReadOnly)
            //    {
            //        throw new NotSupportedException("ReadonlyControlsCollection");
            //    }
            //    base.RemoveByKey(key);
            //}

            public override bool IsReadOnly
            {
                get { return _isReadOnly; }
            }
        }

        internal class TypedControlCollection : ReadOnlyControlCollection
        {

            private Type typeOfControl;
            private Control ownerControl;

            public TypedControlCollection(Control owner, Type typeOfControl, bool isReadOnly) : base(owner, isReadOnly)
            {
                this.typeOfControl = typeOfControl;
                this.ownerControl = owner;
            }

            public TypedControlCollection(Control owner, Type typeOfControl) : base(owner, /*isReadOnly*/false)
            {
                this.typeOfControl = typeOfControl;
                this.ownerControl = owner;
            }

            public override void Add(Control value)
            {
                //Check parenting first for consistency
                //Control.CheckParentingCycle(ownerControl, value);

                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (IsReadOnly)
                {
                    throw new NotSupportedException("ReadonlyControlsCollection");
                }
                if (!typeOfControl.IsAssignableFrom(value.GetType()))
                {
                    throw new ArgumentException("TypedControlCollectionShouldBeOfType");
                }
                base.Add(value);
            }
        }

        //internal struct DCMapping : IDisposable
        //{

        //    private DeviceContext dc;
        //    private Graphics graphics;
        //    Rectangle translatedBounds;

        //    [ResourceExposure(ResourceScope.Process)]
        //    [ResourceConsumption(ResourceScope.Process)]
        //    public DCMapping(HandleRef hDC, Rectangle bounds)
        //    {
        //        if (hDC.Handle == IntPtr.Zero)
        //        {
        //            throw new ArgumentNullException("hDC");
        //        }

        //        bool success;
        //        NativeMethods.POINT viewportOrg = new NativeMethods.POINT();
        //        HandleRef hOriginalClippingRegion = NativeMethods.NullHandleRef;
        //        NativeMethods.RegionFlags originalRegionType = NativeMethods.RegionFlags.NULLREGION;

        //        this.translatedBounds = bounds;
        //        this.graphics = null;
        //        this.dc = DeviceContext.FromHdc(hDC.Handle);

        //        this.dc.SaveHdc();

        //        // Retrieve the x-coordinates and y-coordinates of the viewport origin for the specified device context. 
        //        success = SafeNativeMethods.GetViewportOrgEx(hDC, viewportOrg);
        //        Debug.Assert(success, "GetViewportOrgEx() failed.");

        //        // Create a new rectangular clipping region based off of the bounds specified, shifted over by the x & y specified in the viewport origin.
        //        HandleRef hClippingRegion = new HandleRef(null, SafeNativeMethods.CreateRectRgn(viewportOrg.x + bounds.Left, viewportOrg.y + bounds.Top, viewportOrg.x + bounds.Right, viewportOrg.y + bounds.Bottom));
        //        Debug.Assert(hClippingRegion.Handle != IntPtr.Zero, "CreateRectRgn() failed.");

        //        try
        //        {
        //            // Create an empty region oriented at 0,0 so we can populate it with the original clipping region of the hDC passed in.
        //            hOriginalClippingRegion = new HandleRef(this, SafeNativeMethods.CreateRectRgn(0, 0, 0, 0));
        //            Debug.Assert(hOriginalClippingRegion.Handle != IntPtr.Zero, "CreateRectRgn() failed.");

        //            // Get the clipping region from the hDC: result = {-1 = error, 0 = no region, 1 = success} per MSDN
        //            int result = SafeNativeMethods.GetClipRgn(hDC, hOriginalClippingRegion);
        //            Debug.Assert(result != -1, "GetClipRgn() failed.");

        //            // Shift the viewpoint origint by coordinates specified in "bounds". 
        //            NativeMethods.POINT lastViewPort = new NativeMethods.POINT();
        //            success = SafeNativeMethods.SetViewportOrgEx(hDC, viewportOrg.x + bounds.Left, viewportOrg.y + bounds.Top, lastViewPort);
        //            Debug.Assert(success, "SetViewportOrgEx() failed.");

        //            if (result != 0)
        //            {
        //                // Get the origninal clipping region so we can determine its type (we'll check later if we've restored the region back properly.)
        //                NativeMethods.RECT originalClipRect = new NativeMethods.RECT();
        //                originalRegionType = (NativeMethods.RegionFlags)SafeNativeMethods.GetRgnBox(hOriginalClippingRegion, ref originalClipRect);
        //                Debug.Assert(originalRegionType != NativeMethods.RegionFlags.ERROR, "ERROR returned from SelectClipRgn while selecting the original clipping region..");

        //                if (originalRegionType == NativeMethods.RegionFlags.SIMPLEREGION)
        //                {
        //                    // Find the intersection of our clipping region and the current clipping region (our parent's)
        //                    //      Returns a NULLREGION, the two didn't intersect.
        //                    //      Returns a SIMPLEREGION, the two intersected
        //                    //      Resulting region (stuff that was in hOriginalClippingRegion AND hClippingRegion is placed in hClippingRegion
        //                    NativeMethods.RegionFlags combineResult = (NativeMethods.RegionFlags)SafeNativeMethods.CombineRgn(hClippingRegion, hClippingRegion, hOriginalClippingRegion, NativeMethods.RGN_AND);
        //                    Debug.Assert((combineResult == NativeMethods.RegionFlags.SIMPLEREGION) ||
        //                                    (combineResult == NativeMethods.RegionFlags.NULLREGION),
        //                                    "SIMPLEREGION or NULLREGION expected.");
        //                }
        //            }
        //            else
        //            {
        //                // If there was no clipping region, then the result is a simple region.
        //                // We don't need to keep track of the original now, since it is empty.
        //                SafeNativeMethods.DeleteObject(hOriginalClippingRegion);
        //                hOriginalClippingRegion = new HandleRef(null, IntPtr.Zero);
        //                originalRegionType = NativeMethods.RegionFlags.SIMPLEREGION;
        //            }

        //            // Select the new clipping region; make sure it's a SIMPLEREGION or NULLREGION
        //            NativeMethods.RegionFlags selectResult = (NativeMethods.RegionFlags)SafeNativeMethods.SelectClipRgn(hDC, hClippingRegion);
        //            Debug.Assert((selectResult == NativeMethods.RegionFlags.SIMPLEREGION ||
        //                          selectResult == NativeMethods.RegionFlags.NULLREGION),
        //                          "SIMPLEREGION or NULLLREGION expected.");

        //        }
        //        catch (Exception ex)
        //        {
        //            if (ClientUtils.IsSecurityOrCriticalException(ex))
        //            {
        //                throw;
        //            }

        //            this.dc.RestoreHdc();
        //            this.dc.Dispose();
        //        }
        //        finally
        //        {
        //            // Delete the new clipping region, as the clipping region for the HDC is now set 
        //            // to this rectangle.  Hold on to hOriginalClippingRegion, as we'll need to restore
        //            // it when this object is disposed.
        //            success = SafeNativeMethods.DeleteObject(hClippingRegion);
        //            Debug.Assert(success, "DeleteObject(hClippingRegion) failed.");

        //            if (hOriginalClippingRegion.Handle != IntPtr.Zero)
        //            {
        //                success = SafeNativeMethods.DeleteObject(hOriginalClippingRegion);
        //                Debug.Assert(success, "DeleteObject(hOriginalClippingRegion) failed.");
        //            }
        //        }
        //    }

        //    public void Dispose()
        //    {
        //        if (graphics != null)
        //        {
        //            // Reset GDI+ if used.
        //            // we need to dispose the graphics object first, as it will do 
        //            // some restoration to the ViewPort and ClipRectangle to restore the hDC to 
        //            // the same state it was created in
        //            graphics.Dispose();
        //            graphics = null;
        //        }

        //        if (this.dc != null)
        //        {
        //            // Now properly reset GDI.
        //            this.dc.RestoreHdc();
        //            this.dc.Dispose();
        //            this.dc = null;
        //        }
        //    }

        //    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        //    public Graphics Graphics
        //    {
        //        [ResourceExposure(ResourceScope.Process)]
        //        [ResourceConsumption(ResourceScope.Process)]
        //        get
        //        {
        //            Debug.Assert(this.dc != null, "unexpected null dc!");

        //            if (this.graphics == null)
        //            {
        //                this.graphics = Graphics.FromHdcInternal(dc.Hdc);
        //                this.graphics.SetClip(new Rectangle(Point.Empty, translatedBounds.Size));
        //            }
        //            return this.graphics;
        //        }
        //    }
        //}
        // All 4.x version of the .NET framework are installed in place. For compatibility reasons, we
        // want to know when an application is targeting netfx 4.5 or later vs 4.0
        internal static bool TargetsAtLeast_v4_5
        {
            get
            {
                return _targetsAtLeast_v4_5;
            }
        }

        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        private static bool RunningOnCheck(string propertyName)
        {
            Type binaryCompatibitlityType;
            try
            {
                binaryCompatibitlityType = typeof(Object).GetTypeInfo().Assembly.GetType("System.Runtime.Versioning.BinaryCompatibility", false);
            }
            catch (TypeLoadException)
            {
                return false;
            }

            if (binaryCompatibitlityType == null)
            {
                return false;
            }

            PropertyInfo runningOnV4_5_Property = binaryCompatibitlityType.GetProperty(propertyName, Reflection.BindingFlags.Public | Reflection.BindingFlags.NonPublic | Reflection.BindingFlags.Static);
            if (runningOnV4_5_Property == null)
            {
                return false;
            }

            return (bool)runningOnV4_5_Property.GetValue(null);
        }
    }

}