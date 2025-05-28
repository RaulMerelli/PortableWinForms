namespace System.Windows.Forms
{
    using System;
    using Microsoft.Win32;
    using System.ComponentModel;
    using System.Collections;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;

    [DefaultEvent("CollectionChanged")]
    public class BindingContext : ICollection
    {

        private Hashtable listManagers;

        int ICollection.Count
        {
            get
            {
                ScrubWeakRefs();
                return listManagers.Count;
            }
        }

        void ICollection.CopyTo(Array ar, int index)
        {
            IntSecurity.UnmanagedCode.Demand();
            ScrubWeakRefs();
            listManagers.CopyTo(ar, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IntSecurity.UnmanagedCode.Demand();
            ScrubWeakRefs();
            return listManagers.GetEnumerator();
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                // so the user will know that it has to lock this object
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        public BindingContext()
        {
            listManagers = new Hashtable();
        }

        public BindingManagerBase this[object dataSource]
        {
            get
            {
                return this[dataSource, ""];
            }
        }

        public BindingManagerBase this[object dataSource, string dataMember]
        {
            get
            {
                return EnsureListManager(dataSource, dataMember);
            }
        }

        internal protected void Add(object dataSource, BindingManagerBase listManager)
        {
            /* !!THIS METHOD IS OBSOLETE AND UNUSED!! */
            AddCore(dataSource, listManager);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataSource));
        }

        protected virtual void AddCore(object dataSource, BindingManagerBase listManager)
        {
            /* !!THIS METHOD IS OBSOLETE AND UNUSED!! */
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            if (listManager == null)
                throw new ArgumentNullException("listManager");

            // listManagers[dataSource] = listManager;
            listManagers[GetKey(dataSource, "")] = new WeakReference(listManager, false);
        }

        public event CollectionChangeEventHandler CollectionChanged
        {
            /* !!THIS EVENT IS OBSOLETE AND UNUSED!! */
            [SuppressMessage("Microsoft.Performance", "CA1801:AvoidUnusedParameters")]
            add
            {
                throw new NotImplementedException();
            }
            [SuppressMessage("Microsoft.Performance", "CA1801:AvoidUnusedParameters")]
            remove
            {
            }
        }

        internal protected void Clear()
        {
            /* !!THIS METHOD IS OBSOLETE AND UNUSED!! */
            ClearCore();
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        protected virtual void ClearCore()
        {
            /* !!THIS METHOD IS OBSOLETE AND UNUSED!! */
            listManagers.Clear();
        }

        public bool Contains(object dataSource)
        {
            return Contains(dataSource, "");
        }

        public bool Contains(object dataSource, string dataMember)
        {
            return listManagers.ContainsKey(GetKey(dataSource, dataMember));
        }

        internal HashKey GetKey(object dataSource, string dataMember)
        {
            return new HashKey(dataSource, dataMember);
        }

        internal class HashKey
        {
            WeakReference wRef;
            int dataSourceHashCode;
            string dataMember;

            internal HashKey(object dataSource, string dataMember)
            {
                if (dataSource == null)
                    throw new ArgumentNullException("dataSource");
                if (dataMember == null)
                    dataMember = "";
                // The dataMember should be case insensitive.
                // so convert the dataMember to lower case
                //
                this.wRef = new WeakReference(dataSource, false);
                this.dataSourceHashCode = dataSource.GetHashCode();
                this.dataMember = dataMember.ToLower(CultureInfo.InvariantCulture);
            }

            public override int GetHashCode()
            {
                return dataSourceHashCode * dataMember.GetHashCode();
            }

            public override bool Equals(object target)
            {
                if (target is HashKey)
                {
                    HashKey keyTarget = (HashKey)target;
                    return wRef.Target == keyTarget.wRef.Target && dataMember == keyTarget.dataMember;
                }
                return false;
            }
        }

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
        {
        }

        internal protected void Remove(object dataSource)
        {
            /* !!THIS METHOD IS OBSOLETE AND UNUSED!! */
            RemoveCore(dataSource);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, dataSource));
        }

        protected virtual void RemoveCore(object dataSource)
        {
            /* !!THIS METHOD IS OBSOLETE AND UNUSED!! */
            listManagers.Remove(GetKey(dataSource, ""));
        }

        internal BindingManagerBase EnsureListManager(object dataSource, string dataMember)
        {
            BindingManagerBase bindingManagerBase = null;

            if (dataMember == null)
                dataMember = "";

            // Check whether data source wants to provide its own binding managers
            // (but fall through to old logic if it fails to provide us with one)
            //
            if (dataSource is ICurrencyManagerProvider)
            {
                bindingManagerBase = (dataSource as ICurrencyManagerProvider).GetRelatedCurrencyManager(dataMember);

                if (bindingManagerBase != null)
                {
                    return bindingManagerBase;
                }
            }

            // Check for previously created binding manager
            //
            HashKey key = GetKey(dataSource, dataMember);
            WeakReference wRef;
            wRef = listManagers[key] as WeakReference;
            if (wRef != null)
                bindingManagerBase = (BindingManagerBase)wRef.Target;
            if (bindingManagerBase != null)
            {
                return bindingManagerBase;
            }

            if (dataMember.Length == 0)
            {
                // No data member specified, so create binding manager directly on the data source
                //
                if (dataSource is IList || dataSource is IListSource)
                {
                    // IListSource so we can bind the dataGrid to a table and a dataSet
                    bindingManagerBase = new CurrencyManager(dataSource);
                }
                else
                {
                    // Otherwise assume simple property binding
                    bindingManagerBase = new PropertyManager(dataSource);
                }
            }
            else
            {
                // Data member specified, so get data source's binding manager, and hook a 'related' binding manager to it
                //
                int lastDot = dataMember.LastIndexOf(".");
                string dataPath = (lastDot == -1) ? "" : dataMember.Substring(0, lastDot);
                string dataField = dataMember.Substring(lastDot + 1);

                BindingManagerBase formerManager = EnsureListManager(dataSource, dataPath);

                PropertyDescriptor prop = formerManager.GetItemProperties().Find(dataField, true);
                if (prop == null)
                    //throw new ArgumentException(SR.GetString(SR.RelatedListManagerChild, dataField));
                    throw new ArgumentException("RelatedListManagerChild");

                if (typeof(IList).IsAssignableFrom(prop.PropertyType))
                    bindingManagerBase = new RelatedCurrencyManager(formerManager, dataField);
                else
                    bindingManagerBase = new RelatedPropertyManager(formerManager, dataField);
            }

            // if wRef == null, then it is the first time we want this bindingManagerBase: so add it
            // if wRef != null, then the bindingManagerBase was GC'ed at some point: keep the old wRef and change its target
            if (wRef == null)
                listManagers.Add(key, new WeakReference(bindingManagerBase, false));
            else
                wRef.Target = bindingManagerBase;

            IntSecurity.UnmanagedCode.Demand();
            ScrubWeakRefs();
            // Return the final binding manager
            return bindingManagerBase;
        }

        // may throw
        private static void CheckPropertyBindingCycles(BindingContext newBindingContext, Binding propBinding)
        {
            if (newBindingContext == null || propBinding == null)
                return;
            if (newBindingContext.Contains(propBinding.BindableComponent, ""))
            {
                // this way we do not add a bindingManagerBase to the
                // bindingContext if there isn't one already
                BindingManagerBase bindingManagerBase = newBindingContext.EnsureListManager(propBinding.BindableComponent, "");
                for (int i = 0; i < bindingManagerBase.Bindings.Count; i++)
                {
                    Binding binding = bindingManagerBase.Bindings[i];
                    if (binding.DataSource == propBinding.BindableComponent)
                    {
                        if (propBinding.BindToObject.BindingMemberInfo.BindingMember.Equals(binding.PropertyName))
                            //throw new ArgumentException(SR.GetString(SR.DataBindingCycle, binding.PropertyName), "propBinding");
                            throw new ArgumentException("DataBindingCycle");
                    }
                    else if (propBinding.BindToObject.BindingManagerBase is PropertyManager)
                        CheckPropertyBindingCycles(newBindingContext, binding);
                }
            }
        }

        private void ScrubWeakRefs()
        {
            ArrayList cleanupList = null;
            foreach (DictionaryEntry de in listManagers)
            {
                WeakReference wRef = (WeakReference)de.Value;
                if (wRef.Target == null)
                {
                    if (cleanupList == null)
                    {
                        cleanupList = new ArrayList();
                    }
                    cleanupList.Add(de.Key);
                }
            }

            if (cleanupList != null)
            {
                foreach (object o in cleanupList)
                {
                    listManagers.Remove(o);
                }
            }
        }

        public static void UpdateBinding(BindingContext newBindingContext, Binding binding)
        {
            BindingManagerBase oldManager = binding.BindingManagerBase;
            if (oldManager != null)
            {
                oldManager.Bindings.Remove(binding);
            }

            if (newBindingContext != null)
            {
                // we need to first check for cycles before adding this binding to the collection
                // of bindings.
                if (binding.BindToObject.BindingManagerBase is PropertyManager)
                    CheckPropertyBindingCycles(newBindingContext, binding);

                BindToObject bindTo = binding.BindToObject;
                BindingManagerBase newManager = newBindingContext.EnsureListManager(bindTo.DataSource, bindTo.BindingMemberInfo.BindingPath);
                newManager.Bindings.Add(binding);
            }
        }
    }
}