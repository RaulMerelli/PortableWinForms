namespace System.Windows.Forms
{
    public class ComboBox : ListControl
    {
        public ComboBoxStyle DropDownStyle = ComboBoxStyle.DropDownList;

        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
