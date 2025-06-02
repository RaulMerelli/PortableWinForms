namespace System.Windows.Forms
{
    public class ToolStrip : Control
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
