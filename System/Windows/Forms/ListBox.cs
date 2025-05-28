using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

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

        internal class ItemArray : IComparer
        {

            private static int lastMask = 1;

            private ListControl listControl;
            private Entry[] entries;
            private int count;
            private int version;

            public ItemArray(ListControl listControl)
            {
                this.listControl = listControl;
            }

            public int Version
            {
                get
                {
                    return version;
                }
            }

            public object Add(object item)
            {
                EnsureSpace(1);
                version++;
                entries[count] = new Entry(item);
                return entries[count++];
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void AddRange(ICollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                EnsureSpace(items.Count);
                foreach (object i in items)
                {
                    entries[count++] = new Entry(i);
                }
                version++;
            }

            public void Clear()
            {
                if (count > 0)
                {
                    Array.Clear(entries, 0, count);
                }

                count = 0;
                version++;
            }

            public static int CreateMask()
            {
                int mask = lastMask;
                lastMask = lastMask << 1;
                Debug.Assert(lastMask > mask, "We have overflowed our state mask.");
                return mask;
            }

            private void EnsureSpace(int elements)
            {
                if (entries == null)
                {
                    entries = new Entry[Math.Max(elements, 4)];
                }
                else if (count + elements >= entries.Length)
                {
                    int newLength = Math.Max(entries.Length * 2, entries.Length + elements);
                    Entry[] newEntries = new Entry[newLength];
                    entries.CopyTo(newEntries, 0);
                    entries = newEntries;
                }
            }

            public int GetActualIndex(int virtualIndex, int stateMask)
            {
                if (stateMask == 0)
                {
                    return virtualIndex;
                }

                // More complex; we must compute this index.
                int calcIndex = -1;
                for (int i = 0; i < count; i++)
                {
                    if ((entries[i].state & stateMask) != 0)
                    {
                        calcIndex++;
                        if (calcIndex == virtualIndex)
                        {
                            return i;
                        }
                    }
                }

                return -1;
            }

            public int GetCount(int stateMask)
            {
                // If mask is zero, then just give the main count
                if (stateMask == 0)
                {
                    return count;
                }

                // more complex:  must provide a count of items
                // based on a mask.

                int filteredCount = 0;

                for (int i = 0; i < count; i++)
                {
                    if ((entries[i].state & stateMask) != 0)
                    {
                        filteredCount++;
                    }
                }

                return filteredCount;
            }

            public IEnumerator GetEnumerator(int stateMask)
            {
                return GetEnumerator(stateMask, false);
            }

            public IEnumerator GetEnumerator(int stateMask, bool anyBit)
            {
                return new EntryEnumerator(this, stateMask, anyBit);
            }

            public object GetItem(int virtualIndex, int stateMask)
            {
                int actualIndex = GetActualIndex(virtualIndex, stateMask);

                if (actualIndex == -1)
                {
                    throw new IndexOutOfRangeException();
                }

                return entries[actualIndex].item;
            }
            internal object GetEntryObject(int virtualIndex, int stateMask)
            {
                int actualIndex = GetActualIndex(virtualIndex, stateMask);

                if (actualIndex == -1)
                {
                    throw new IndexOutOfRangeException();
                }

                return entries[actualIndex];
            }
            public bool GetState(int index, int stateMask)
            {
                return ((entries[index].state & stateMask) == stateMask);
            }

            public int IndexOf(object item, int stateMask)
            {

                int virtualIndex = -1;

                for (int i = 0; i < count; i++)
                {
                    if (stateMask == 0 || (entries[i].state & stateMask) != 0)
                    {
                        virtualIndex++;
                        if (entries[i].item.Equals(item))
                        {
                            return virtualIndex;
                        }
                    }
                }

                return -1;
            }

            public int IndexOfIdentifier(object identifier, int stateMask)
            {
                int virtualIndex = -1;

                for (int i = 0; i < count; i++)
                {
                    if (stateMask == 0 || (entries[i].state & stateMask) != 0)
                    {
                        virtualIndex++;
                        if (entries[i] == identifier)
                        {
                            return virtualIndex;
                        }
                    }
                }

                return -1;
            }

            public void Insert(int index, object item)
            {
                EnsureSpace(1);

                if (index < count)
                {
                    System.Array.Copy(entries, index, entries, index + 1, count - index);
                }

                entries[index] = new Entry(item);
                count++;
                version++;
            }

            public void Remove(object item)
            {
                int index = IndexOf(item, 0);

                if (index != -1)
                {
                    RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                count--;
                for (int i = index; i < count; i++)
                {
                    entries[i] = entries[i + 1];
                }
                entries[count] = null;
                version++;
            }

            public void SetItem(int index, object item)
            {
                entries[index].item = item;
            }

            public void SetState(int index, int stateMask, bool value)
            {
                if (value)
                {
                    entries[index].state |= stateMask;
                }
                else
                {
                    entries[index].state &= ~stateMask;
                }
                version++;
            }

            public int BinarySearch(object element)
            {
                return Array.BinarySearch(entries, 0, count, element, this);
            }


            public void Sort()
            {
                Array.Sort(entries, 0, count, this);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void Sort(Array externalArray)
            {
                Array.Sort(externalArray, this);
            }

            int IComparer.Compare(object item1, object item2)
            {
                if (item1 == null)
                {
                    if (item2 == null)
                        return 0; //both null, then they are equal

                    return -1; //item1 is null, but item2 is valid (greater)
                }
                if (item2 == null)
                    return 1; //item2 is null, so item 1 is greater

                if (item1 is Entry)
                {
                    item1 = ((Entry)item1).item;
                }

                if (item2 is Entry)
                {
                    item2 = ((Entry)item2).item;
                }

                String itemName1 = listControl.GetItemText(item1);
                String itemName2 = listControl.GetItemText(item2);

                //CompareInfo compInfo = (Application.CurrentCulture).CompareInfo;
                //return compInfo.Compare(itemName1, itemName2, CompareOptions.StringSort);
                return 0;
            }

            private class Entry
            {
                public object item;
                public int state;

                public Entry(object item)
                {
                    this.item = item;
                    this.state = 0;
                }
            }

            private class EntryEnumerator : IEnumerator
            {
                private ItemArray items;
                private bool anyBit;
                private int state;
                private int current;
                private int version;

                public EntryEnumerator(ItemArray items, int state, bool anyBit)
                {
                    this.items = items;
                    this.state = state;
                    this.anyBit = anyBit;
                    this.version = items.version;
                    this.current = -1;
                }

                bool IEnumerator.MoveNext()
                {
                    if (version != items.version) throw new InvalidOperationException("ListEnumVersionMismatch");

                    while (true)
                    {
                        if (current < items.count - 1)
                        {
                            current++;
                            if (anyBit)
                            {
                                if ((items.entries[current].state & state) != 0)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if ((items.entries[current].state & state) == state)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            current = items.count;
                            return false;
                        }
                    }
                }

                void IEnumerator.Reset()
                {
                    if (version != items.version) throw new InvalidOperationException("ListEnumVersionMismatch");
                    current = -1;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (current == -1 || current == items.count)
                        {
                            throw new InvalidOperationException("ListEnumCurrentOutOfRange");
                        }

                        return items.entries[current].item;
                    }
                }
            }
        }

        public class ObjectCollection : IList
        {

            private ListBox owner;
            private ItemArray items;

            public ObjectCollection(ListBox owner)
            {
                this.owner = owner;
            }

            public ObjectCollection(ListBox owner, ObjectCollection value)
            {
                this.owner = owner;
                this.AddRange(value);
            }

            public ObjectCollection(ListBox owner, object[] value)
            {
                this.owner = owner;
                this.AddRange(value);
            }

            public int Count
            {
                get
                {
                    return InnerArray.GetCount(0);
                }
            }

            internal ItemArray InnerArray
            {
                get
                {
                    if (items == null)
                    {
                        items = new ItemArray(owner);
                    }
                    return items;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }


            public int Add(object item)
            {
                //owner.CheckNoDataSource();
                int index = AddInternal(item);
                //owner.UpdateHorizontalExtent();
                return index;
            }


            private int AddInternal(object item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                int index = -1;
                //if (!owner.sorted)
                //{
                //    InnerArray.Add(item);
                //}
                //else
                {
                    if (Count > 0)
                    {
                        index = InnerArray.BinarySearch(item);
                        if (index < 0)
                        {
                            index = ~index; // getting the index of the first element that is larger than the search value
                                            //this index will be used for insert
                        }
                    }
                    else
                        index = 0;

                    Debug.Assert(index >= 0 && index <= Count, "Wrong index for insert");
                    InnerArray.Insert(index, item);
                }
                bool successful = false;

                try
                {
                    //if (owner.sorted)
                    //{
                    //    if (owner.IsHandleCreated)
                    //    {
                    //        owner.NativeInsert(index, item);
                    //        owner.UpdateMaxItemWidth(item, false);
                    //        if (owner.selectedItems != null)
                    //        {
                    //            // VSWhidbey 95187: sorting may throw the LB contents and the selectedItem array out of synch.
                    //            owner.selectedItems.Dirty();
                    //        }
                    //    }
                    //}
                    //else
                    {
                        index = Count - 1;
                        if (owner.IsHandleCreated)
                        {
                            //owner.NativeAdd(item);
                            //owner.UpdateMaxItemWidth(item, false);
                        }
                    }
                    successful = true;
                }
                finally
                {
                    if (!successful)
                    {
                        InnerArray.Remove(item);
                    }
                }

                return index;
            }


            int IList.Add(object item)
            {
                return Add(item);
            }

            public void AddRange(ObjectCollection value)
            {
                //owner.CheckNoDataSource();
                AddRangeInternal((ICollection)value);
            }

            public void AddRange(object[] items)
            {
                //owner.CheckNoDataSource();
                AddRangeInternal((ICollection)items);
            }

            internal void AddRangeInternal(ICollection items)
            {

                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                //owner.BeginUpdate();
                try
                {
                    foreach (object item in items)
                    {
                        // adding items one-by-one for performance 
                        // not using sort because after the array is sorted index of each newly added item will need to be found
                        // AddInternal is based on BinarySearch and finds index without any additional cost
                        AddInternal(item);
                    }
                }
                finally
                {
                    //owner.UpdateHorizontalExtent();
                    //owner.EndUpdate();
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public virtual object this[int index]
            {
                get
                {
                    if (index < 0 || index >= InnerArray.GetCount(0))
                    {
                        throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                    }

                    return InnerArray.GetItem(index, 0);
                }
                set
                {
                    //owner.CheckNoDataSource();
                    SetItemInternal(index, value);
                }
            }

            public virtual void Clear()
            {
                //owner.CheckNoDataSource();
                ClearInternal();
            }

            internal void ClearInternal()
            {

                //update the width.. to reset Scrollbars..
                // Clear the selection state.
                //
                int cnt = owner.Items.Count;
                for (int i = 0; i < cnt; i++)
                {
                    //owner.UpdateMaxItemWidth(InnerArray.GetItem(i, 0), true);
                }


                if (owner.IsHandleCreated)
                {
                    //owner.NativeClear();
                }
                InnerArray.Clear();
                //owner.maxWidth = -1;
                //owner.UpdateHorizontalExtent();
            }

            public bool Contains(object value)
            {
                return IndexOf(value) != -1;
            }

            public void CopyTo(object[] destination, int arrayIndex)
            {
                int count = InnerArray.GetCount(0);
                for (int i = 0; i < count; i++)
                {
                    destination[i + arrayIndex] = InnerArray.GetItem(i, 0);
                }
            }

            void ICollection.CopyTo(Array destination, int index)
            {
                int count = InnerArray.GetCount(0);
                for (int i = 0; i < count; i++)
                {
                    destination.SetValue(InnerArray.GetItem(i, 0), i + index);
                }
            }

            public IEnumerator GetEnumerator()
            {
                return InnerArray.GetEnumerator(0);
            }

            public int IndexOf(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                return InnerArray.IndexOf(value, 0);
            }

            internal int IndexOfIdentifier(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                return InnerArray.IndexOfIdentifier(value, 0);
            }

            public void Insert(int index, object item)
            {
                //owner.CheckNoDataSource();

                if (index < 0 || index > InnerArray.GetCount(0))
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                // If the combo box is sorted, then nust treat this like an add
                // because we are going to twiddle the index anyway.
                //
                //if (owner.sorted)
                //{
                //    Add(item);
                //}
                //else
                {
                    InnerArray.Insert(index, item);
                    if (owner.IsHandleCreated)
                    {

                        bool successful = false;

                        try
                        {
                            //owner.NativeInsert(index, item);
                            //owner.UpdateMaxItemWidth(item, false);
                            successful = true;
                        }
                        finally
                        {
                            if (!successful)
                            {
                                InnerArray.RemoveAt(index);
                            }
                        }
                    }
                }
                //owner.UpdateHorizontalExtent();
            }

            public void Remove(object value)
            {

                int index = InnerArray.IndexOf(value, 0);

                if (index != -1)
                {
                    RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                //owner.CheckNoDataSource();

                //if (index < 0 || index >= InnerArray.GetCount(0))
                //{
                //    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                //}

                //owner.UpdateMaxItemWidth(InnerArray.GetItem(index, 0), true);

                //InnerArray.RemoveAt(index);

                //if (owner.IsHandleCreated)
                //{
                //    owner.NativeRemoveAt(index);
                //}

                //owner.UpdateHorizontalExtent();
            }

            internal void SetItemInternal(int index, object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (index < 0 || index >= InnerArray.GetCount(0))
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                //owner.UpdateMaxItemWidth(InnerArray.GetItem(index, 0), true);
                InnerArray.SetItem(index, value);

                // If the native control has been created, and the display text of the new list item object
                // is different to the current text in the native list item, recreate the native list item...
                if (owner.IsHandleCreated)
                {
                    bool selected = (owner.SelectedIndex == index);
                    //if (String.Compare(this.owner.GetItemText(value), this.owner.NativeGetItemText(index), true, CultureInfo.CurrentCulture) != 0)
                    //{
                    //    owner.NativeRemoveAt(index);
                    //    owner.SelectedItems.SetSelected(index, false);
                    //    owner.NativeInsert(index, value);
                    //    owner.UpdateMaxItemWidth(value, false);
                    //    if (selected)
                    //    {
                    //        owner.SelectedIndex = index;
                    //    }
                    //}
                    //else
                    //{
                    //    // NEW - FOR COMPATIBILITY REASONS
                    //    // Minimum compatibility fix for VSWhidbey 377287
                    //    if (selected)
                    //    {
                    //        owner.OnSelectedIndexChanged(EventArgs.Empty); //will fire selectedvaluechanged
                    //    }
                    //}
                }
                //owner.UpdateHorizontalExtent();
            }
        } // end ObjectCollection
    }
}
