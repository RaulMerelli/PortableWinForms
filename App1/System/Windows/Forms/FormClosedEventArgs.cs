namespace System.Windows.Forms
{
    using System;

    public class FormClosedEventArgs : EventArgs
    {
        private CloseReason closeReason;

        public FormClosedEventArgs(CloseReason closeReason)
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