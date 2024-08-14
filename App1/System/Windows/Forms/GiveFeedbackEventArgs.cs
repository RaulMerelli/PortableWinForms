namespace System.Windows.Forms
{
    public class GiveFeedbackEventArgs : EventArgs
    {
        private readonly DragDropEffects effect;

        private bool useDefaultCursors;

        public DragDropEffects Effect => effect;

        public bool UseDefaultCursors
        {
            get
            {
                return useDefaultCursors;
            }
            set
            {
                useDefaultCursors = value;
            }
        }

        public GiveFeedbackEventArgs(DragDropEffects effect, bool useDefaultCursors)
        {
            this.effect = effect;
            this.useDefaultCursors = useDefaultCursors;
        }
    }
}
