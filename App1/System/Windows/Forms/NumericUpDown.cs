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
