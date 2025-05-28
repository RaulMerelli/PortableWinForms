namespace System.Windows.Forms
{
    public class PreviewKeyDownEventArgs : EventArgs
    {
        private readonly Keys _keyData;

        private bool _isInputKey;

        public bool Alt => (_keyData & Keys.Alt) == Keys.Alt;

        public bool Control => (_keyData & Keys.Control) == Keys.Control;

        public Keys KeyCode
        {
            get
            {
                Keys keys = _keyData & Keys.KeyCode;
                if (!Enum.IsDefined(typeof(Keys), (int)keys))
                {
                    return Keys.None;
                }

                return keys;
            }
        }

        public int KeyValue => (int)(_keyData & Keys.KeyCode);

        public Keys KeyData => _keyData;

        public Keys Modifiers => _keyData & Keys.Modifiers;

        public bool Shift => (_keyData & Keys.Shift) == Keys.Shift;

        public bool IsInputKey
        {
            get
            {
                return _isInputKey;
            }
            set
            {
                _isInputKey = value;
            }
        }

        public PreviewKeyDownEventArgs(Keys keyData)
        {
            _keyData = keyData;
        }
    }
}
