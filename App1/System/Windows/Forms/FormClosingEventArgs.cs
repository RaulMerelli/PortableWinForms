namespace System.Windows.Forms
{
    using System.ComponentModel;

    public class FormClosingEventArgs : CancelEventArgs
    {
        private CloseReason closeReason;

        public FormClosingEventArgs(CloseReason closeReason, bool cancel)
        : base(cancel)
        {
            this.closeReason = closeReason;
        }

        public CloseReason CloseReason
        {
            get
            {
                return closeReason;
            }
        }
    }
}