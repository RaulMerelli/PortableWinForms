namespace System.Windows.Forms
{
    public class DragEventArgs : EventArgs
    {
        private readonly IDataObject data;

        private readonly int keyState;

        private readonly int x;

        private readonly int y;

        private readonly DragDropEffects allowedEffect;

        private DragDropEffects effect;

        public IDataObject Data => data;

        public int KeyState => keyState;

        public int X => x;

        public int Y => y;

        public DragDropEffects AllowedEffect => allowedEffect;

        public DragDropEffects Effect
        {
            get
            {
                return effect;
            }
            set
            {
                effect = value;
            }
        }

        public DragEventArgs(IDataObject data, int keyState, int x, int y, DragDropEffects allowedEffect, DragDropEffects effect)
        {
            this.data = data;
            this.keyState = keyState;
            this.x = x;
            this.y = y;
            this.allowedEffect = allowedEffect;
            this.effect = effect;
        }
    }
}
