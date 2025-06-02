namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Windows.Forms.Layout;

    public abstract class TableLayoutStyleCollection : IList
    {
        private IArrangedElement _owner;
        private ArrayList _innerList = new ArrayList();

        internal TableLayoutStyleCollection(IArrangedElement owner)
        {
            _owner = owner;
        }

        internal IArrangedElement Owner
        {
            get { return _owner; }
        }
        internal virtual string PropertyName
        {
            get { return null; }
        }

        int IList.Add(object style)
        {
            EnsureNotOwned((TableLayoutStyle)style);
            ((TableLayoutStyle)style).Owner = this.Owner;
            int index = _innerList.Add(style);
            PerformLayoutIfOwned();
            return index;
        }

        public int Add(TableLayoutStyle style)
        {
            return ((IList)this).Add(style);
        }


        void IList.Insert(int index, object style)
        {
            EnsureNotOwned((TableLayoutStyle)style);
            ((TableLayoutStyle)style).Owner = this.Owner;
            _innerList.Insert(index, style);
            PerformLayoutIfOwned();

        }

        object IList.this[int index]
        {
            get { return _innerList[index]; }
            set
            {
                TableLayoutStyle style = (TableLayoutStyle)value;
                EnsureNotOwned(style);
                style.Owner = this.Owner;
                _innerList[index] = style;
                PerformLayoutIfOwned();
            }
        }

        public TableLayoutStyle this[int index]
        {
            get { return (TableLayoutStyle)((IList)this)[index]; }
            set { ((IList)this)[index] = value; }
        }

        void IList.Remove(object style)
        {
            ((TableLayoutStyle)style).Owner = null;
            _innerList.Remove(style);
            PerformLayoutIfOwned();
        }

        public void Clear()
        {
            foreach (TableLayoutStyle style in _innerList)
            {
                style.Owner = null;
            }
            _innerList.Clear();
            PerformLayoutIfOwned();
        }

        public void RemoveAt(int index)
        {
            TableLayoutStyle style = (TableLayoutStyle)_innerList[index];
            style.Owner = null;
            _innerList.RemoveAt(index);
            PerformLayoutIfOwned();
        }

        // These methods just forward to _innerList.
        bool IList.Contains(object style) { return _innerList.Contains(style); }
        int IList.IndexOf(object style) { return _innerList.IndexOf(style); }

        // These properties / methods just forward to _innerList and are item-type agnostic.
        bool IList.IsFixedSize { get { return _innerList.IsFixedSize; } }
        bool IList.IsReadOnly { get { return _innerList.IsReadOnly; } }
        void ICollection.CopyTo(System.Array array, int startIndex) { _innerList.CopyTo(array, startIndex); }

        public int Count { get { return _innerList.Count; } }
        bool ICollection.IsSynchronized { get { return _innerList.IsSynchronized; } }
        object ICollection.SyncRoot { get { return _innerList.SyncRoot; } }
        IEnumerator IEnumerable.GetEnumerator() { return _innerList.GetEnumerator(); }

        private void EnsureNotOwned(TableLayoutStyle style)
        {
            if (style.Owner != null)
            {
                throw new ArgumentException("OnlyOneControl");
            }
        }
        internal void EnsureOwnership(IArrangedElement owner)
        {
            _owner = owner;
            for (int i = 0; i < Count; i++)
            {
                this[i].Owner = owner;
            }
        }
        private void PerformLayoutIfOwned()
        {
            if (this.Owner != null)
            {
                LayoutTransaction.DoLayout(this.Owner, this.Owner, PropertyName);
            }
        }
    }
}