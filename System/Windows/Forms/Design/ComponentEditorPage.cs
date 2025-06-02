namespace System.Windows.Forms.Design
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System;
    using System.Security.Permissions;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    [ComVisible(true),
     ClassInterface(ClassInterfaceType.AutoDispatch),
     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors") // Shipped in Everett
    ]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public abstract class ComponentEditorPage : Panel
    {

        IComponentEditorPageSite pageSite;
        IComponent component;
        bool firstActivate;
        bool loadRequired;
        int loading;
        Icon icon;
        bool commitOnDeactivate;

        public ComponentEditorPage() : base()
        {
            commitOnDeactivate = false;
            firstActivate = true;
            loadRequired = false;
            loading = 0;

            Visible = false;
        }


        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        new public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        protected IComponentEditorPageSite PageSite
        {
            get { return pageSite; }
            set { pageSite = value; }
        }
        protected IComponent Component
        {
            get { return component; }
            set { component = value; }
        }
        protected bool FirstActivate
        {
            get { return firstActivate; }
            set { firstActivate = value; }
        }
        protected bool LoadRequired
        {
            get { return loadRequired; }
            set { loadRequired = value; }
        }
        protected int Loading
        {
            get { return loading; }
            set { loading = value; }
        }

        public bool CommitOnDeactivate
        {
            get
            {
                return commitOnDeactivate;
            }
            set
            {
                commitOnDeactivate = value;
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.Style &= ~(NativeMethods.WS_BORDER | NativeMethods.WS_OVERLAPPED | NativeMethods.WS_DLGFRAME);
        //        return cp;
        //    }
        //}

        public Icon Icon
        {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get
            {
                if (icon == null)
                {
                    icon = new Icon(typeof(ComponentEditorPage), "ComponentEditorPage.ico");
                }
                return icon;
            }
            set
            {
                icon = value;
            }
        }

        public virtual string Title
        {
            get
            {
                return base.Text;
            }
        }

        public virtual void Activate()
        {
            if (loadRequired)
            {
                EnterLoadingMode();
                LoadComponent();
                ExitLoadingMode();

                loadRequired = false;
            }
            Visible = true;
            firstActivate = false;
        }

        public virtual void ApplyChanges()
        {
            SaveComponent();
        }

        public virtual void Deactivate()
        {
            Visible = false;
        }

        protected void EnterLoadingMode()
        {
            loading++;
        }

        protected void ExitLoadingMode()
        {
            Debug.Assert(loading > 0, "Unbalanced Enter/ExitLoadingMode calls");
            loading--;
        }

        public virtual Control GetControl()
        {
            return this;
        }

        protected IComponent GetSelectedComponent()
        {
            return component;
        }

        public virtual bool IsPageMessage(ref Message msg)
        {
            return PreProcessMessage(ref msg);
        }

        protected bool IsFirstActivate()
        {
            return firstActivate;
        }

        protected bool IsLoading()
        {
            return loading != 0;
        }

        protected abstract void LoadComponent();

        public virtual void OnApplyComplete()
        {
            ReloadComponent();
        }

        protected virtual void ReloadComponent()
        {
            if (Visible == false)
            {
                loadRequired = true;
            }
        }

        protected abstract void SaveComponent();

        protected virtual void SetDirty()
        {
            if (IsLoading() == false)
            {
                pageSite.SetDirty();
            }
        }

        public virtual void SetComponent(IComponent component)
        {
            this.component = component;
            loadRequired = true;
        }

        public virtual void SetSite(IComponentEditorPageSite site)
        {
            this.pageSite = site;

            pageSite.GetControl().Controls.Add(this);
        }

        public virtual void ShowHelp()
        {
        }

        public virtual bool SupportsHelp()
        {
            return false;
        }
    }
}