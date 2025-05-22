using System.Collections;

namespace System.Windows.Forms
{
    public class ListBox : ListControl
    {
        public ObjectCollection Items;

        public override int SelectedIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }

        protected override void RefreshItem(int index)
        {
            throw new NotImplementedException();
        }

        protected override void SetItemsCore(IList items)
        {
            throw new NotImplementedException();
        }

        public class ObjectCollection
        {
            void Addrange()
            {

            }
        }
    }
}
