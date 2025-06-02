namespace System.Windows.Forms
{
    using System.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using Microsoft.Win32;
    using System.Security.Permissions;
    using System.Runtime.Versioning;

    [ToolboxItemFilter("System.Windows.Forms.MainMenu")]
    public class MainMenu : Menu
    {
        internal Form form;
        internal Form ownerForm;  // this is the form that created this menu, and is the only form allowed to dispose it.
        private RightToLeft rightToLeft = System.Windows.Forms.RightToLeft.Inherit;
        private EventHandler onCollapse;

        public MainMenu()
            : base(null)
        {

        }

        public MainMenu(IContainer container) : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            container.Add(this);
        }

        public MainMenu(MenuItem[] items)
            : base(items)
        {

        }

        public event EventHandler Collapse
        {
            add
            {
                onCollapse += value;
            }
            remove
            {
                onCollapse -= value;
            }
        }


        // VSWhidbey 94189: Add an AmbientValue attribute so that the Reset context menu becomes available in the Property Grid.
        [Localizable(true), AmbientValue(RightToLeft.Inherit)]
        public virtual RightToLeft RightToLeft
        {
            get
            {
                if (System.Windows.Forms.RightToLeft.Inherit == rightToLeft)
                {
                    if (form != null)
                    {
                        return form.RightToLeft;
                    }
                    else
                    {
                        return RightToLeft.Inherit;
                    }
                }
                else
                {
                    return rightToLeft;
                }
            }
            set
            {

                //valid values are 0x0 to 0x2
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)RightToLeft.No, (int)RightToLeft.Inherit))
                {
                    throw new InvalidEnumArgumentException("RightToLeft", (int)value, typeof(RightToLeft));
                }
                if (rightToLeft != value)
                {
                    rightToLeft = value;
                    UpdateRtl((value == System.Windows.Forms.RightToLeft.Yes));
                }

            }
        }

        internal override bool RenderIsRightToLeft
        {
            get
            {
                return (RightToLeft == System.Windows.Forms.RightToLeft.Yes && (form == null || !form.IsMirrored));
            }
        }

        public virtual MainMenu CloneMenu()
        {
            MainMenu newMenu = new MainMenu();
            newMenu.CloneMenu(this);
            return newMenu;
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        protected override IntPtr CreateMenuHandle()
        {
            //return UnsafeNativeMethods.CreateMenu();
            return IntPtr.Zero;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (form != null && (ownerForm == null || form == ownerForm))
                {
                    form.Menu = null;
                }
            }
            base.Dispose(disposing);
        }

        [UIPermission(SecurityAction.Demand, Window = UIPermissionWindow.AllWindows)]
        public Form GetForm()
        {
            return form;
        }

        internal Form GetFormUnsafe()
        {
            return form;
        }

        internal override void ItemsChanged(int change)
        {
            base.ItemsChanged(change);
            if (form != null)
                form.MenuChanged(change, this);
        }

        internal virtual void ItemsChanged(int change, Menu menu)
        {
            if (form != null)
                form.MenuChanged(change, menu);
        }

        protected internal virtual void OnCollapse(EventArgs e)
        {
            if (onCollapse != null)
            {
                onCollapse(this, e);
            }
        }

        internal virtual bool ShouldSerializeRightToLeft()
        {
            if (System.Windows.Forms.RightToLeft.Inherit == RightToLeft)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            // VSWhidbey 495300: removing GetForm information 
            return base.ToString();
        }
    }
}