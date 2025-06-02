namespace System.Windows.Forms
{
    public class MenuStrip : ToolStrip
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
