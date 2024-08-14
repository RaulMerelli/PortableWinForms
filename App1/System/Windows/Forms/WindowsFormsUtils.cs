using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
    public class WindowsFormsUtils
    {
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

        // RotateLeft(0xFF000000, 4) -> 0xF000000F
        public static int RotateLeft(int value, int nBits)
        {
            Debug.Assert(Marshal.SizeOf(typeof(int)) == 4, "The impossible has happened.");

            nBits = nBits % 32;
            return value << nBits | (value >> (32 - nBits));
        }
    }
}
