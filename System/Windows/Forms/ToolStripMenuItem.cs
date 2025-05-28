namespace System.Windows.Forms
{
    class ToolStripMenuItem : Control
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
