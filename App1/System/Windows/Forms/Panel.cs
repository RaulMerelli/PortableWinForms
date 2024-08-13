namespace System.Windows.Forms
{
    public class Panel : ScrollableControl
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
