namespace System.Windows.Forms
{
    class DataGridView : Control
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
