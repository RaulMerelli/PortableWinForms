namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    public class FormCollection : ReadOnlyCollectionBase
    {

        internal static object CollectionSyncRoot = new object();

        public virtual Form this[string name]
        {
            get
            {
                if (name != null)
                {
                    lock (CollectionSyncRoot)
                    {
                        foreach (Form form in InnerList)
                        {
                            if (string.Equals(form.Name, name, StringComparison.OrdinalIgnoreCase))
                            {
                                return form;
                            }
                        }
                    }
                }
                return null;
            }
        }

        public virtual Form this[int index]
        {
            get
            {
                Form f = null;

                lock (CollectionSyncRoot)
                {
                    f = (Form)InnerList[index];
                }
                return f;
            }
        }

        internal void Add(Form form)
        {
            lock (CollectionSyncRoot)
            {
                InnerList.Add(form);
            }
        }

        internal bool Contains(Form form)
        {
            bool inCollection = false;
            lock (CollectionSyncRoot)
            {
                inCollection = InnerList.Contains(form);
            }
            return inCollection;
        }

        internal void Remove(Form form)
        {
            lock (CollectionSyncRoot)
            {
                InnerList.Remove(form);
            }
        }
    }
}