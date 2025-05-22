namespace System.Windows.Forms
{
    using System;
    using Microsoft.Win32;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Collections;

    [DefaultEvent("CollectionChanged")]
    public class BindingsCollection : System.Windows.Forms.BaseCollection
    {

        private ArrayList list;
        private CollectionChangeEventHandler onCollectionChanging;
        private CollectionChangeEventHandler onCollectionChanged;

        // internalonly
        internal BindingsCollection()
        {
        }

        public override int Count
        {
            get
            {
                if (list == null)
                {
                    return 0;
                }
                return base.Count;
            }
        }

        protected override ArrayList List
        {
            get
            {
                if (list == null)
                    list = new ArrayList();
                return list;
            }
        }

        public Binding this[int index]
        {
            get
            {
                return (Binding)List[index];
            }
        }

        // internalonly
        internal protected void Add(Binding binding)
        {
            CollectionChangeEventArgs ccevent = new CollectionChangeEventArgs(CollectionChangeAction.Add, binding);
            OnCollectionChanging(ccevent);
            AddCore(binding);
            OnCollectionChanged(ccevent);
        }

        // internalonly
        protected virtual void AddCore(Binding dataBinding)
        {
            if (dataBinding == null)
                throw new ArgumentNullException("dataBinding");

            List.Add(dataBinding);
        }

        public event CollectionChangeEventHandler CollectionChanging
        {
            add
            {
                onCollectionChanging += value;
            }
            remove
            {
                onCollectionChanging -= value;
            }
        }

        public event CollectionChangeEventHandler CollectionChanged
        {
            add
            {
                onCollectionChanged += value;
            }
            remove
            {
                onCollectionChanged -= value;
            }
        }

        // internalonly
        internal protected void Clear()
        {
            CollectionChangeEventArgs ccevent = new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null);
            OnCollectionChanging(ccevent);
            ClearCore();
            OnCollectionChanged(ccevent);
        }

        // internalonly
        protected virtual void ClearCore()
        {
            List.Clear();
        }

        protected virtual void OnCollectionChanging(CollectionChangeEventArgs e)
        {
            if (onCollectionChanging != null)
            {
                onCollectionChanging(this, e);
            }
        }

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
        {
            if (onCollectionChanged != null)
            {
                onCollectionChanged(this, ccevent);
            }
        }

        // internalonly
        internal protected void Remove(Binding binding)
        {
            CollectionChangeEventArgs ccevent = new CollectionChangeEventArgs(CollectionChangeAction.Remove, binding);
            OnCollectionChanging(ccevent);
            RemoveCore(binding);
            OnCollectionChanged(ccevent);
        }


        // internalonly
        internal protected void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        // internalonly
        protected virtual void RemoveCore(Binding dataBinding)
        {
            List.Remove(dataBinding);
        }

        // internalonly
        internal protected bool ShouldSerializeMyAll()
        {
            return Count > 0;
        }
    }
}