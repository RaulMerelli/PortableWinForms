namespace System.Windows.Forms
{
    class ToolStrip : Control
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
