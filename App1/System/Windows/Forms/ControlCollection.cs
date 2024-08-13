namespace System.Windows.Forms
{
    public class ControlCollection
    {
        private int count = 0;
        private Control[] controls = new Control[0];

        public int Count { get => count; }

        public void Add(Control value)
        {
            Control[] backupControls = controls;
            controls = new Control[backupControls.Length + 1];
            for (int i = 0; i < backupControls.Length; i++)
            {
                controls[i] = backupControls[i];
            }
            controls[backupControls.Length] = value;
            count = controls.Length;
        }

        public Control this[int index]
        {
            get
            {
                return controls[index];
            }
            set
            {
                controls[index] = value;
                count = controls.Length;
            }
        }
    }
}
