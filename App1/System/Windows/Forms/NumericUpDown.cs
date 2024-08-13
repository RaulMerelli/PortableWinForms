namespace System.Windows.Forms
{

    public class NumericUpDown : Control
    {
        public bool UseCompatibleTextRendering = true;
        decimal Increment = 1;
        private decimal maximum = 100;
        private decimal minimum = 0;

        int DecimalPlaces = 0;

        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }

        private void UpdateUpDown()
        {
            //Win32.SendMessage(UpDownHandle, Win32.UDM_SETRANGE, 0, Win32.MAKELPARAM((int)maximum, (int)minimum));    // Sets the controls direction and range.
        }

        public decimal Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                maximum = value;
                UpdateUpDown();
            }
        }

        public decimal Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
                UpdateUpDown();
            }
        }
    }
}
