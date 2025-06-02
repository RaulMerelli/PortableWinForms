namespace System.Windows.Forms
{
    using System.Collections;
    using System.Windows.Forms.Layout;

    public class TableLayoutRowStyleCollection : TableLayoutStyleCollection
    {

        internal TableLayoutRowStyleCollection(IArrangedElement Owner) : base(Owner) { }
        internal TableLayoutRowStyleCollection() : base(null) { }

        internal override string PropertyName
        {
            get { return PropertyNames.RowStyles; }
        }

        public int Add(RowStyle rowStyle) { return ((IList)this).Add(rowStyle); }

        public void Insert(int index, RowStyle rowStyle) { ((IList)this).Insert(index, rowStyle); }

        public new RowStyle this[int index]
        {
            get { return (RowStyle)((IList)this)[index]; }
            set { ((IList)this)[index] = value; }
        }

        public void Remove(RowStyle rowStyle) { ((IList)this).Remove(rowStyle); }

        public bool Contains(RowStyle rowStyle) { return ((IList)this).Contains(rowStyle); }

        public int IndexOf(RowStyle rowStyle) { return ((IList)this).IndexOf(rowStyle); }
    }
}