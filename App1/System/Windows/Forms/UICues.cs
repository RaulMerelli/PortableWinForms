namespace System.Windows.Forms
{
    [Flags]
    public enum UICues
    {
        ShowFocus = 1,
        ShowKeyboard = 2,
        Shown = 3,
        ChangeFocus = 4,
        ChangeKeyboard = 8,
        Changed = 0xC,
        None = 0
    }
}
