namespace System.Windows.Forms
{
    class MenuStrip : ToolStrip
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
