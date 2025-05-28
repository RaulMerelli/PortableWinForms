namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum BoundsSpecified
    {
        X = 0x1,
        Y = 0x2,
        Width = 0x4,
        Height = 0x8,
        Location = X | Y,
        Size = Width | Height,
        All = Location | Size,
        None = 0,
    }
}