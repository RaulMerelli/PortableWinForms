namespace System.Windows.Forms
{
    using System;
    public class BindingManagerDataErrorEventArgs : EventArgs
    {
        private Exception exception;

        public BindingManagerDataErrorEventArgs(Exception exception)
        {
            this.exception = exception;
        }

        public Exception Exception
        {
            get
            {
                return this.exception;
            }
        }
    }
}