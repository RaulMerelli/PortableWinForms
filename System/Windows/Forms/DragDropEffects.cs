namespace System.Windows.Forms
{
    [Flags]
    public enum DragDropEffects
    {
        None = 0,
        Copy = 1,
        Move = 2,
        Link = 4,
        Scroll = int.MinValue,
        All = -2147483645
    }
}
