﻿namespace System.Windows.Forms
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;
    using System.Collections;
    using System.Collections.Generic;

    internal class RelatedCurrencyManager : CurrencyManager
    {

        BindingManagerBase parentManager;
        string dataField;
        PropertyDescriptor fieldInfo;
        static List<BindingManagerBase> IgnoreItemChangedTable = new List<BindingManagerBase>();

        [
            SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")  // If the constructor does not set the dataSource
                                                                                                    // it would be a breaking change.
        ]
        internal RelatedCurrencyManager(BindingManagerBase parentManager, string dataField) : base(null)
        {
            Bind(parentManager, dataField);
        }

        internal void Bind(BindingManagerBase parentManager, string dataField)
        {
            Debug.Assert(parentManager != null, "How could this be a null parentManager.");

            // Unwire previous BindingManagerBase
            UnwireParentManager(this.parentManager);

            this.parentManager = parentManager;
            this.dataField = dataField;
            this.fieldInfo = parentManager.GetItemProperties().Find(dataField, true);
            if (fieldInfo == null || !typeof(IList).IsAssignableFrom(fieldInfo.PropertyType))
            {
                //throw new ArgumentException(SR.GetString(SR.RelatedListManagerChild, dataField));
                throw new ArgumentException("RelatedListManagerChild");
            }
            this.finalType = fieldInfo.PropertyType;

            // Wire new BindingManagerBase
            WireParentManager(this.parentManager);

            ParentManager_CurrentItemChanged(parentManager, EventArgs.Empty);
        }

        private void UnwireParentManager(BindingManagerBase bmb)
        {
            if (bmb != null)
            {
                bmb.CurrentItemChanged -= new EventHandler(ParentManager_CurrentItemChanged);

                if (bmb is CurrencyManager)
                {
                    (bmb as CurrencyManager).MetaDataChanged -= new EventHandler(ParentManager_MetaDataChanged);
                }
            }
        }

        private void WireParentManager(BindingManagerBase bmb)
        {
            if (bmb != null)
            {
                bmb.CurrentItemChanged += new EventHandler(ParentManager_CurrentItemChanged);

                if (bmb is CurrencyManager)
                {
                    (bmb as CurrencyManager).MetaDataChanged += new EventHandler(ParentManager_MetaDataChanged);
                }
            }
        }

        internal override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            PropertyDescriptor[] accessors;

            if (listAccessors != null && listAccessors.Length > 0)
            {
                accessors = new PropertyDescriptor[listAccessors.Length + 1];
                listAccessors.CopyTo(accessors, 1);
            }
            else
            {
                accessors = new PropertyDescriptor[1];
            }

            // Set this accessor (add to the beginning)
            accessors[0] = this.fieldInfo;

            // Get props
            return parentManager.GetItemProperties(accessors);
        }

        public override PropertyDescriptorCollection GetItemProperties()
        {
            return GetItemProperties(null);
        }

        // <devdoc>
        //    <para>Gets the name of the list.</para>
        // </devdoc>
        internal override string GetListName()
        {
            string name = GetListName(new ArrayList());
            if (name.Length > 0)
            {
                return name;
            }
            return base.GetListName();
        }

        protected internal override string GetListName(ArrayList listAccessors)
        {
            listAccessors.Insert(0, fieldInfo);
            return parentManager.GetListName(listAccessors);
        }

        private void ParentManager_MetaDataChanged(object sender, EventArgs e)
        {
            // Propagate MetaDataChanged events from the parent manager
            base.OnMetaDataChanged(e);
        }

        private void ParentManager_CurrentItemChanged(object sender, EventArgs e)
        {
            if (IgnoreItemChangedTable.Contains(parentManager))
            {
                return;
            }

            int oldlistposition = this.listposition;

            // we only pull the data from the controls into the backEnd. we do not care about keeping the lastGoodKnownRow
            // when we are about to change the entire list in this currencymanager.
            try
            {
                PullData();
            }
            catch (Exception ex)
            {
                OnDataError(ex);
            }

            if (parentManager is CurrencyManager)
            {
                CurrencyManager curManager = (CurrencyManager)parentManager;
                if (curManager.Count > 0)
                {
                    // Parent list has a current row, so get the related list from the relevant property on that row.
                    SetDataSource(fieldInfo.GetValue(curManager.Current));
                    listposition = (Count > 0 ? 0 : -1);
                }
                else
                {
                    // APPCOMPAT: bring back the Everett behavior where the currency manager adds an item and 
                    // then it cancels the addition.
                    // vsw 427731.
                    // 
                    // really, really hocky.
                    // will throw if the list in the curManager is not IBindingList
                    // and this will fail if the IBindingList does not have list change notification. read on....
                    // when a new item will get added to an empty parent table, 
                    // the table will fire OnCurrentChanged and this method will get executed again
                    // allowing us to set the data source to an object with the right properties (so we can show
                    // metadata at design time).
                    // we then call CancelCurrentEdit to remove the dummy row, but making sure to ignore any
                    // OnCurrentItemChanged that results from this action (to avoid infinite recursion)
                    curManager.AddNew();
                    try
                    {
                        IgnoreItemChangedTable.Add(curManager);
                        curManager.CancelCurrentEdit();
                    }
                    finally
                    {
                        if (IgnoreItemChangedTable.Contains(curManager))
                        {
                            IgnoreItemChangedTable.Remove(curManager);
                        }
                    }
                }
            }
            else
            {
                // Case where the parent is not a list, but a single object
                SetDataSource(fieldInfo.GetValue(parentManager.Current));
                listposition = (Count > 0 ? 0 : -1);
            }
            if (oldlistposition != listposition)
                OnPositionChanged(EventArgs.Empty);
            OnCurrentChanged(EventArgs.Empty);
            OnCurrentItemChanged(EventArgs.Empty);
        }

    }
}