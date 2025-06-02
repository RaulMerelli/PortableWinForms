namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;

    [DefaultEvent("Popup")]
    public class ContextMenu : Menu
    {

        private EventHandler onPopup;
        private EventHandler onCollapse;
        internal Control sourceControl;

        private RightToLeft rightToLeft = System.Windows.Forms.RightToLeft.Inherit;

        public ContextMenu()
            : base(null)
        {
        }

        public ContextMenu(MenuItem[] menuItems)
            : base(menuItems)
        {
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control SourceControl
        {
            [UIPermission(SecurityAction.Demand, Window = UIPermissionWindow.AllWindows)]
            get
            {
                return sourceControl;
            }
        }

        public event EventHandler Popup
        {
            add
            {
                onPopup += value;
            }
            remove
            {
                onPopup -= value;
            }
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

        // VSWhidbey 164244: Add a DefaultValue attribute so that the Reset context menu becomes
        // available in the Property Grid but the default value remains No.
        [Localizable(true), DefaultValue(RightToLeft.No)]
        public virtual RightToLeft RightToLeft
        {
            get
            {
                if (System.Windows.Forms.RightToLeft.Inherit == rightToLeft)
                {
                    if (sourceControl != null)
                    {
                        return ((Control)sourceControl).RightToLeft;
                    }
                    else
                    {
                        return RightToLeft.No;
                    }
                }
                else
                {
                    return rightToLeft;
                }
            }
            set
            {

                //valid values are 0x0 to 0x2.
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)RightToLeft.No, (int)RightToLeft.Inherit))
                {
                    throw new InvalidEnumArgumentException("RightToLeft", (int)value, typeof(RightToLeft));
                }
                if (RightToLeft != value)
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
                return (rightToLeft == System.Windows.Forms.RightToLeft.Yes);
            }
        }
        protected internal virtual void OnPopup(EventArgs e)
        {
            if (onPopup != null)
            {
                onPopup(this, e);
            }
        }

        protected internal virtual void OnCollapse(EventArgs e)
        {
            if (onCollapse != null)
            {
                onCollapse(this, e);
            }
        }

        [
            System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode),
            System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        protected internal virtual bool ProcessCmdKey(ref Message msg, Keys keyData, Control control)
        {
            sourceControl = control;
            return ProcessCmdKey(ref msg, keyData);
        }

        private void ResetRightToLeft()
        {
            RightToLeft = RightToLeft.No;
        }

        internal virtual bool ShouldSerializeRightToLeft()
        {
            if (System.Windows.Forms.RightToLeft.Inherit == rightToLeft)
            {
                return false;
            }
            return true;
        }

        public void Show(Control control, Point pos)
        {
            Show(control, pos, NativeMethods.TPM_VERTICAL | NativeMethods.TPM_RIGHTBUTTON);
        }

        public void Show(Control control, Point pos, LeftRightAlignment alignment)
        {

            // This code below looks wrong but it's correct. 
            // Microsoft Left alignment means we want the menu to show up left of the point it is invoked from.
            // We specify TPM_RIGHTALIGN which tells win32 to align the right side of this 
            // menu with the point (which aligns it Left visually)
            if (alignment == LeftRightAlignment.Left)
            {
                Show(control, pos, NativeMethods.TPM_VERTICAL | NativeMethods.TPM_RIGHTBUTTON | NativeMethods.TPM_RIGHTALIGN);
            }
            else
            {
                Show(control, pos, NativeMethods.TPM_VERTICAL | NativeMethods.TPM_RIGHTBUTTON | NativeMethods.TPM_LEFTALIGN);
            }
        }

        private void Show(Control control, Point pos, int flags)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            if (!control.IsHandleCreated || !control.Visible)
                throw new ArgumentException("ContextMenuInvalidParent");

            sourceControl = control;

            OnPopup(EventArgs.Empty);
            //pos = control.PointToScreen(pos);
            //SafeNativeMethods.TrackPopupMenuEx(new HandleRef(this, Handle),
            //    flags,
            //    pos.X,
            //    pos.Y,
            //    new HandleRef(control, control.Handle),
            //    null);
        }

    }
}