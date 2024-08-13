using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Forms
{
    static internal class ClientUtils
    {
#pragma warning disable 618
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsCriticalException(Exception ex)
        {
            return ex is NullReferenceException
                    || ex is StackOverflowException
                    || ex is OutOfMemoryException
                    || ex is System.Threading.ThreadAbortException
                    || ex is ExecutionEngineException
                    || ex is IndexOutOfRangeException
                    || ex is AccessViolationException;
        }
#pragma warning restore 618

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsSecurityOrCriticalException(Exception ex)
        {
            return (ex is System.Security.SecurityException) || IsCriticalException(ex);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int GetBitCount(uint x)
        {
            int count = 0;
            while (x > 0)
            {
                x &= x - 1;
                count++;
            }
            return count;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)
        {
            bool valid = (value >= minValue) && (value <= maxValue);
            return valid;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
        public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue, int maxNumberOfBitsOn)
        {
            System.Diagnostics.Debug.Assert(maxNumberOfBitsOn >= 0 && maxNumberOfBitsOn < 32, "expect this to be greater than zero and less than 32");

            bool valid = (value >= minValue) && (value <= maxValue);
            //Note: if it's 0, it'll have no bits on.  If it's a power of 2, it'll have 1.
            valid = (valid && GetBitCount((uint)value) <= maxNumberOfBitsOn);
            return valid;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
        public static bool IsEnumValid_Masked(Enum enumValue, int value, UInt32 mask)
        {
            bool valid = ((value & mask) == value);
            return valid;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsEnumValid_NotSequential(System.Enum enumValue, int value, params int[] enumValues)
        {
            System.Diagnostics.Debug.Assert(Enum.GetValues(enumValue.GetType()).Length == enumValues.Length, "Not all the enum members were passed in.");
            for (int i = 0; i < enumValues.Length; i++)
            {
                if (enumValues[i] == value)
                {
                    return true;
                }
            }
            return false;
        }
    }
}