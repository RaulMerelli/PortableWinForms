namespace System.Windows.Forms
{
    public class PictureBox : Control
    {
        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
