namespace System.Windows.Forms
{

    using System;
    using Microsoft.Win32;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Collections;

    [DefaultEvent("CollectionChanged")]
    internal class ListManagerBindingsCollection : BindingsCollection
    {

        private BindingManagerBase bindingManagerBase;

        internal ListManagerBindingsCollection(BindingManagerBase bindingManagerBase) : base()
        {
            Debug.Assert(bindingManagerBase != null, "How could a listmanagerbindingscollection not have a bindingManagerBase associated with it!");
            this.bindingManagerBase = bindingManagerBase;
        }

        protected override void AddCore(Binding dataBinding)
        {
            if (dataBinding == null)
                throw new ArgumentNullException("dataBinding");
            if (dataBinding.BindingManagerBase == bindingManagerBase)
                //throw new ArgumentException(SR.GetString(SR.BindingsCollectionAdd1), "dataBinding");
                throw new ArgumentException("BindingsCollectionAdd1");
            if (dataBinding.BindingManagerBase != null)
                //throw new ArgumentException(SR.GetString(SR.BindingsCollectionAdd2), "dataBinding");
                throw new ArgumentException("BindingsCollectionAdd2");

            // important to set prop first for error checking.
            dataBinding.SetListManager(bindingManagerBase);

            base.AddCore(dataBinding);
        }

        protected override void ClearCore()
        {
            int numLinks = Count;
            for (int i = 0; i < numLinks; i++)
            {
                Binding dataBinding = this[i];
                dataBinding.SetListManager(null);
            }
            base.ClearCore();
        }

        protected override void RemoveCore(Binding dataBinding)
        {
            if (dataBinding.BindingManagerBase != bindingManagerBase)
                //throw new ArgumentException(SR.GetString(SR.BindingsCollectionForeign));
                throw new ArgumentException("BindingsCollectionForeign");
            dataBinding.SetListManager(null);
            base.RemoveCore(dataBinding);
        }
    }
}