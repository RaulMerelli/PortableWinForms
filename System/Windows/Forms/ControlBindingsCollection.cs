namespace System.Windows.Forms
{

    using System;
    using Microsoft.Win32;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Collections;
    using System.Globalization;

    [DefaultEvent("CollectionChanged"),
     ]
    public class ControlBindingsCollection : BindingsCollection
    {

        internal IBindableComponent control;

        private DataSourceUpdateMode defaultDataSourceUpdateMode = DataSourceUpdateMode.OnValidation;

        public ControlBindingsCollection(IBindableComponent control) : base()
        {
            Debug.Assert(control != null, "How could a controlbindingscollection not have a control associated with it!");
            this.control = control;
        }

        public IBindableComponent BindableComponent
        {
            get
            {
                return this.control;
            }
        }

        public Control Control
        {
            get
            {
                return this.control as Control;
            }
        }

        public Binding this[string propertyName]
        {
            get
            {
                foreach (Binding binding in this)
                {
                    if (String.Equals(binding.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return binding;
                    }
                }
                return null;
            }
        }

        public new void Add(Binding binding)
        {
            base.Add(binding);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember)
        {
            return Add(propertyName, dataSource, dataMember, false, this.DefaultDataSourceUpdateMode, null, String.Empty, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled)
        {
            return Add(propertyName, dataSource, dataMember, formattingEnabled, this.DefaultDataSourceUpdateMode, null, String.Empty, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode)
        {
            return Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode, null, String.Empty, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode, object nullValue)
        {
            return Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode, nullValue, String.Empty, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode, object nullValue, string formatString)
        {
            return Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode, nullValue, formatString, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode, object nullValue, string formatString, IFormatProvider formatInfo)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            Binding binding = new Binding(propertyName, dataSource, dataMember, formattingEnabled, updateMode, nullValue, formatString, formatInfo);
            Add(binding);
            return binding;
        }

        protected override void AddCore(Binding dataBinding)
        {
            if (dataBinding == null)
                throw new ArgumentNullException("dataBinding");
            if (dataBinding.BindableComponent == control)
                //throw new ArgumentException(SR.GetString(SR.BindingsCollectionAdd1));
                throw new ArgumentException("BindingsCollectionAdd1");
            if (dataBinding.BindableComponent != null)
                //throw new ArgumentException(SR.GetString(SR.BindingsCollectionAdd2));
                throw new ArgumentException("BindingsCollectionAdd2");

            // important to set prop first for error checking.
            dataBinding.SetBindableComponent(control);

            base.AddCore(dataBinding);
        }

        // internalonly
        internal void CheckDuplicates(Binding binding)
        {
            if (binding.PropertyName.Length == 0)
            {
                return;
            }
            for (int i = 0; i < Count; i++)
            {
                if (binding != this[i] && this[i].PropertyName.Length > 0 &&
                    (String.Compare(binding.PropertyName, this[i].PropertyName, false, CultureInfo.InvariantCulture) == 0))
                {
                    throw new ArgumentException("BindingsCollectionDup");
                }
            }
        }

        public new void Clear()
        {
            base.Clear();
        }

        // internalonly
        protected override void ClearCore()
        {
            int numLinks = Count;
            for (int i = 0; i < numLinks; i++)
            {
                Binding dataBinding = this[i];
                dataBinding.SetBindableComponent(null);
            }
            base.ClearCore();
        }

        public DataSourceUpdateMode DefaultDataSourceUpdateMode
        {
            get
            {
                return defaultDataSourceUpdateMode;
            }

            set
            {
                defaultDataSourceUpdateMode = value;
            }
        }

        public new void Remove(Binding binding)
        {
            base.Remove(binding);
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        protected override void RemoveCore(Binding dataBinding)
        {
            if (dataBinding.BindableComponent != control)
                //throw new ArgumentException(SR.GetString(SR.BindingsCollectionForeign));
                throw new ArgumentException("BindingsCollectionForeign");
            dataBinding.SetBindableComponent(null);
            base.RemoveCore(dataBinding);
        }
    }
}