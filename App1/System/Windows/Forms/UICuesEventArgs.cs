namespace System.Windows.Forms
{
    public class UICuesEventArgs : EventArgs
    {
        private readonly UICues uicues;

        public bool ShowFocus => (uicues & UICues.ShowFocus) != 0;

        public bool ShowKeyboard => (uicues & UICues.ShowKeyboard) != 0;

        public bool ChangeFocus => (uicues & UICues.ChangeFocus) != 0;

        public bool ChangeKeyboard => (uicues & UICues.ChangeKeyboard) != 0;

        public UICues Changed => uicues & UICues.Changed;

        public UICuesEventArgs(UICues uicues)
        {
            this.uicues = uicues;
        }
    }
}
