using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Windows.Forms
{
    public class AutoCompleteStringCollection : IList, ICollection, IEnumerable
    {
        private CollectionChangeEventHandler onCollectionChanged;

        private ArrayList data = new ArrayList();

        public string this[int index]
        {
            get
            {
                return (string)data[index];
            }
            set
            {
                OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, data[index]));
                data[index] = value;
                OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, value));
            }
        }

        public int Count => data.Count;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public object SyncRoot
        {
            [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
            get
            {
                return this;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (string)value;
            }
        }

        public event CollectionChangeEventHandler CollectionChanged
        {
            add
            {
                onCollectionChanged = (CollectionChangeEventHandler)Delegate.Combine(onCollectionChanged, value);
            }
            remove
            {
                onCollectionChanged = (CollectionChangeEventHandler)Delegate.Remove(onCollectionChanged, value);
            }
        }

        public AutoCompleteStringCollection()
        {
        }

        protected void OnCollectionChanged(CollectionChangeEventArgs e)
        {
            if (onCollectionChanged != null)
            {
                onCollectionChanged(this, e);
            }
        }

        public int Add(string value)
        {
            int result = data.Add(value);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, value));
            return result;
        }

        public void AddRange(string[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            data.AddRange(value);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        public void Clear()
        {
            data.Clear();
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        public bool Contains(string value)
        {
            return data.Contains(value);
        }

        public void CopyTo(string[] array, int index)
        {
            data.CopyTo(array, index);
        }

        public int IndexOf(string value)
        {
            return data.IndexOf(value);
        }

        public void Insert(int index, string value)
        {
            data.Insert(index, value);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, value));
        }

        public void Remove(string value)
        {
            data.Remove(value);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, value));
        }

        public void RemoveAt(int index)
        {
            string element = (string)data[index];
            data.RemoveAt(index);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, element));
        }

        int IList.Add(object value)
        {
            return Add((string)value);
        }

        bool IList.Contains(object value)
        {
            return Contains((string)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((string)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (string)value);
        }

        void IList.Remove(object value)
        {
            Remove((string)value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            data.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }
}