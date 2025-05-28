namespace System.Windows.Forms
{
    public class ControlEventArgs : EventArgs
    {
        private Control control;

        public Control Control => control;

        public ControlEventArgs(Control control)
        {
            this.control = control;
        }
    }
}
