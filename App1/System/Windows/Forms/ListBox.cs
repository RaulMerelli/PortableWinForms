namespace System.Windows.Forms
{
    public class ListBox : ListControl
    {
        public ObjectCollection Items;

        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }

        public class ObjectCollection
        {
            void Addrange()
            {

            }
        }
    }
}
