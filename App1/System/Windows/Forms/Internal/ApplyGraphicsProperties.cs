using System;

namespace System.Windows.Forms.Internal
{

    [Flags]
    public enum ApplyGraphicsProperties
    {
        // No properties to be applied to the DC obtained from the Graphics object.
        None = 0x00000000,
        // Apply clipping region.
        Clipping = 0x00000001,
        // Apply coordinate transformation.
        TranslateTransform = 0x00000002,
        // Apply all supported Graphics properties.
        All = Clipping | TranslateTransform
    }
}
