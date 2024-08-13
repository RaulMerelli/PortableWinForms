namespace System.Windows.Forms
{
    public class ScrollableControl : Control
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
