namespace System.Windows.Forms
{
    public class QueryContinueDragEventArgs : EventArgs
    {
        private readonly int keyState;

        private readonly bool escapePressed;

        private DragAction action;

        public int KeyState => keyState;

        public bool EscapePressed => escapePressed;

        public DragAction Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }

        public QueryContinueDragEventArgs(int keyState, bool escapePressed, DragAction action)
        {
            this.keyState = keyState;
            this.escapePressed = escapePressed;
            this.action = action;
        }
    }
}
