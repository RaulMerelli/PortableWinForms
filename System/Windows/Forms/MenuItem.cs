namespace System.Windows.Forms
{
    class MenuItem : Control
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
