namespace System.Windows.Forms
{
    class MainMenu : Control
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
