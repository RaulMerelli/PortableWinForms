namespace System.Windows.Forms.Internal
{
    public enum WindowsFontQuality
    {
        Default = IntNativeMethods.DEFAULT_QUALITY,

        Draft = IntNativeMethods.DRAFT_QUALITY,

        Proof = IntNativeMethods.PROOF_QUALITY,

        NonAntiAliased = IntNativeMethods.NONANTIALIASED_QUALITY,

        AntiAliased = IntNativeMethods.ANTIALIASED_QUALITY,

        ClearType = IntNativeMethods.CLEARTYPE_QUALITY,
        ClearTypeNatural = IntNativeMethods.CLEARTYPE_NATURAL_QUALITY
    }
}