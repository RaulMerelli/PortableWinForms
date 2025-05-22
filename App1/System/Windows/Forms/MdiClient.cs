namespace System.Windows.Forms
{

    using Microsoft.Win32;
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.ServiceModel.Channels;

    [
    ComVisible(true),
    ClassInterface(ClassInterfaceType.AutoDispatch),
    ToolboxItem(false),
    DesignTimeVisible(false)
    ]
    public sealed class MdiClient : Control
    {

        // kept in add order, not ZOrder. Need to return the correct
        // array of items...
        //
        private ArrayList children = new ArrayList();

        public MdiClient() : base()
        {
            SetStyle(ControlStyles.Selectable, false);
            BackColor = SystemColors.AppWorkspace;
            Dock = DockStyle.Fill;
        }

        //[
        //Localizable(true)
        //]
        //public override Image BackgroundImage
        //{
        //    get
        //    {
        //        Image result = base.BackgroundImage;
        //        if (result == null && ParentInternal != null)
        //            result = ParentInternal.BackgroundImage;
        //        return result;
        //    }

        //    set
        //    {
        //        base.BackgroundImage = value;
        //    }
        //}

        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        //public override ImageLayout BackgroundImageLayout
        //{
        //    get
        //    {
        //        Image backgroundImage = BackgroundImage;
        //        if (backgroundImage != null && ParentInternal != null)
        //        {
        //            ImageLayout imageLayout = base.BackgroundImageLayout;
        //            if (imageLayout != ParentInternal.BackgroundImageLayout)
        //            {
        //                // if the Layout is set on the parent use that.
        //                return ParentInternal.BackgroundImageLayout;
        //            }
        //        }
        //        return base.BackgroundImageLayout;
        //    }
        //    set
        //    {
        //        base.BackgroundImageLayout = value;
        //    }
        //}

        //protected override CreateParams CreateParams
        //{
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;

        //        cp.ClassName = "MDICLIENT";

        //        // Note: Don't set the MDIS_ALLCHILDSTYLES CreatParams.Style bit, it prevents an MDI child form from getting activated 
        //        // when made visible (no WM_MDIACTIVATE sent to it), and forcing activation on it changes the activation event sequence
        //        // (MdiChildActivate/Enter/Focus/Activate/etc.).
        //        // Comment for removed code: 
        //        // VSWhidbey 93518, 93544, 93547, 93563, and 93568: Add the style MDIS_ALLCHILDSTYLES
        //        // so that MDI Client windows can have the WS_VISIBLE style removed from the window style
        //        // to make them not visible but still present.
        //        cp.Style |= NativeMethods.WS_VSCROLL | NativeMethods.WS_HSCROLL;
        //        cp.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;
        //        cp.Param = new NativeMethods.CLIENTCREATESTRUCT(IntPtr.Zero, 1);
        //        ISite site = (ParentInternal == null) ? null : ParentInternal.Site;
        //        if (site != null && site.DesignMode)
        //        {
        //            cp.Style |= NativeMethods.WS_DISABLED;
        //            SetState(STATE_ENABLED, false);
        //        }

        //        if (this.RightToLeft == RightToLeft.Yes && this.ParentInternal != null && this.ParentInternal.IsMirrored)
        //        {
        //            //We want to turn on mirroring for MdiClient explicitly.
        //            cp.ExStyle |= NativeMethods.WS_EX_LAYOUTRTL | NativeMethods.WS_EX_NOINHERITLAYOUT;
        //            //Don't need these styles when mirroring is turned on.
        //            cp.ExStyle &= ~(NativeMethods.WS_EX_RTLREADING | NativeMethods.WS_EX_RIGHT | NativeMethods.WS_EX_LEFTSCROLLBAR);
        //        }

        //        return cp;
        //    }
        //}

        public Form[] MdiChildren
        {
            get
            {
                Form[] temp = new Form[children.Count];
                children.CopyTo(temp, 0);
                return temp;
            }
        }

        //protected override Control.ControlCollection CreateControlsInstance()
        //{
        //    return new ControlCollection(this);
        //}

        public void LayoutMdi(MdiLayout value)
        {
            if (Handle == IntPtr.Zero)
                return;

            switch (value)
            {
                case MdiLayout.Cascade:
                    //SendMessage(NativeMethods.WM_MDICASCADE, 0, 0);
                    break;
                case MdiLayout.TileVertical:
                    //SendMessage(NativeMethods.WM_MDITILE, NativeMethods.MDITILE_VERTICAL, 0);
                    break;
                case MdiLayout.TileHorizontal:
                    //SendMessage(NativeMethods.WM_MDITILE, NativeMethods.MDITILE_HORIZONTAL, 0);
                    break;
                case MdiLayout.ArrangeIcons:
                    //SendMessage(NativeMethods.WM_MDIICONARRANGE, 0, 0);
                    break;
            }
        }

        internal override void OnResize(EventArgs e)
        {
            ISite site = (ParentInternal == null) ? null : ParentInternal.Site;
            if (site != null && site.DesignMode && Handle != IntPtr.Zero)
            {
                //SetWindowRgn();
            }
            base.OnResize(e);
        }


        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void ScaleCore(float dx, float dy)
        //{

        //    // Don't scale child forms...
        //    //

        //    SuspendLayout();
        //    try
        //    {
        //        Rectangle bounds = Bounds;
        //        int sx = (int)Math.Round(bounds.X * dx);
        //        int sy = (int)Math.Round(bounds.Y * dy);
        //        int sw = (int)Math.Round((bounds.X + bounds.Width) * dx - sx);
        //        int sh = (int)Math.Round((bounds.Y + bounds.Height) * dy - sy);
        //        SetBounds(sx, sy, sw, sh, BoundsSpecified.All);
        //    }
        //    finally
        //    {
        //        ResumeLayout();
        //    }
        //}

        //protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        //{
        //    // never scale X and Y of an MDI client form
        //    specified &= ~BoundsSpecified.Location;
        //    base.ScaleControl(factor, specified);
        //}

        //protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        //{
        //    ISite site = (ParentInternal == null) ? null : ParentInternal.Site;
        //    if (IsHandleCreated && (site == null || !site.DesignMode))
        //    {
        //        Rectangle oldBounds = Bounds;
        //        base.SetBoundsCore(x, y, width, height, specified);
        //        Rectangle newBounds = Bounds;

        //        int yDelta = oldBounds.Height - newBounds.Height;
        //        if (yDelta != 0)
        //        {
        //            // NOTE: This logic is to keep minimized MDI children anchored to
        //            // the bottom left of the client area, normally they are anchored
        //            // to the top right which just looks wierd!
        //            //
        //            NativeMethods.WINDOWPLACEMENT wp = new NativeMethods.WINDOWPLACEMENT();
        //            wp.length = Marshal.SizeOf(typeof(NativeMethods.WINDOWPLACEMENT));

        //            for (int i = 0; i < Controls.Count; i++)
        //            {
        //                Control ctl = Controls[i];
        //                if (ctl != null && ctl is Form)
        //                {
        //                    Form child = (Form)ctl;
        //                    // VSWhidbey 93551: Only adjust the window position for visible MDI Child windows to prevent
        //                    // them from being re-displayed.
        //                    if (child.CanRecreateHandle() && child.WindowState == FormWindowState.Minimized)
        //                    {
        //                        UnsafeNativeMethods.GetWindowPlacement(new HandleRef(child, child.Handle), ref wp);
        //                        wp.ptMinPosition_y -= yDelta;
        //                        if (wp.ptMinPosition_y == -1)
        //                        {
        //                            if (yDelta < 0)
        //                            {
        //                                wp.ptMinPosition_y = 0;
        //                            }
        //                            else
        //                            {
        //                                wp.ptMinPosition_y = -2;
        //                            }
        //                        }
        //                        wp.flags = NativeMethods.WPF_SETMINPOSITION;
        //                        UnsafeNativeMethods.SetWindowPlacement(new HandleRef(child, child.Handle), ref wp);
        //                        wp.flags = 0;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        base.SetBoundsCore(x, y, width, height, specified);
        //    }
        //}

        //private void SetWindowRgn()
        //{
        //    IntPtr rgn1 = IntPtr.Zero;
        //    IntPtr rgn2 = IntPtr.Zero;
        //    NativeMethods.RECT rect = new NativeMethods.RECT();
        //    CreateParams cp = CreateParams;

        //    AdjustWindowRectEx(ref rect, cp.Style, false, cp.ExStyle);

        //    Rectangle bounds = Bounds;
        //    rgn1 = SafeNativeMethods.CreateRectRgn(0, 0, bounds.Width, bounds.Height);
        //    try
        //    {
        //        rgn2 = SafeNativeMethods.CreateRectRgn(-rect.left, -rect.top,
        //                                     bounds.Width - rect.right, bounds.Height - rect.bottom);
        //        try
        //        {
        //            if (rgn1 == IntPtr.Zero || rgn2 == IntPtr.Zero)
        //                throw new InvalidOperationException(SR.GetString(SR.ErrorSettingWindowRegion));

        //            if (SafeNativeMethods.CombineRgn(new HandleRef(null, rgn1), new HandleRef(null, rgn1), new HandleRef(null, rgn2), NativeMethods.RGN_DIFF) == 0)
        //                throw new InvalidOperationException(SR.GetString(SR.ErrorSettingWindowRegion));

        //            if (UnsafeNativeMethods.SetWindowRgn(new HandleRef(this, Handle), new HandleRef(null, rgn1), true) == 0)
        //            {
        //                throw new InvalidOperationException(SR.GetString(SR.ErrorSettingWindowRegion));
        //            }
        //            else
        //            {
        //                // The hwnd now owns the region.
        //                rgn1 = IntPtr.Zero;
        //            }
        //        }
        //        finally
        //        {
        //            if (rgn2 != IntPtr.Zero)
        //            {
        //                SafeNativeMethods.DeleteObject(new HandleRef(null, rgn2));
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        if (rgn1 != IntPtr.Zero)
        //        {
        //            SafeNativeMethods.DeleteObject(new HandleRef(null, rgn1));
        //        }
        //    }
        //}


        //internal override bool ShouldSerializeBackColor()
        //{
        //    return BackColor != SystemColors.AppWorkspace;
        //}

        private bool ShouldSerializeLocation()
        {
            return false;
        }

        internal override bool ShouldSerializeSize()
        {
            return false;
        }

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //protected override void WndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {

        //        case NativeMethods.WM_CREATE:
        //            if (ParentInternal != null && ParentInternal.Site != null && ParentInternal.Site.DesignMode && Handle != IntPtr.Zero)
        //            {
        //                SetWindowRgn();
        //            }
        //            break;

        //        case NativeMethods.WM_SETFOCUS:
        //            InvokeGotFocus(ParentInternal, EventArgs.Empty);
        //            Form childForm = null;
        //            if (ParentInternal is Form)
        //            {
        //                childForm = ((Form)ParentInternal).ActiveMdiChildInternal;
        //            }
        //            if (childForm == null && MdiChildren.Length > 0 && MdiChildren[0].IsMdiChildFocusable)
        //            {
        //                childForm = MdiChildren[0];
        //            }
        //            if (childForm != null && childForm.Visible)
        //            {
        //                childForm.Active = true;
        //            }

        //            // Do not use control's implementation of WmSetFocus
        //            // as it will improperly activate this control. 
        //            WmImeSetFocus();
        //            DefWndProc(ref m);
        //            InvokeGotFocus(this, EventArgs.Empty);
        //            return;
        //        case NativeMethods.WM_KILLFOCUS:
        //            InvokeLostFocus(ParentInternal, EventArgs.Empty);
        //            break;
        //    }
        //    base.WndProc(ref m);
        //}

        internal override void OnInvokedSetScrollPosition(object sender, EventArgs e)
        {
            //Application.Idle += new EventHandler(this.OnIdle); //do this on idle (it must be mega-delayed).
        }

        private void OnIdle(object sender, EventArgs e)
        {
            //Application.Idle -= new EventHandler(this.OnIdle);
            base.OnInvokedSetScrollPosition(sender, e);
        }

        [ComVisible(false)]
        new public class ControlCollection : Control.ControlCollection
        {
            private MdiClient owner;

            public ControlCollection(MdiClient owner)
            : base(owner)
            {
                this.owner = owner;
            }

            public override void Add(Control value)
            {
                if (value == null)
                {
                    return;
                }
                if (!(value is Form) || !((Form)value).IsMdiChild)
                {
                    throw new ArgumentException("MDIChildAddToNonMDIParent");
                }
                //if (owner.CreateThreadId != value.CreateThreadId)
                //{
                //    throw new ArgumentException("AddDifferentThreads");
                //}
                owner.children.Add((Form)value);
                base.Add(value);
            }

            public override void Remove(Control value)
            {
                owner.children.Remove(value);
                base.Remove(value);
            }
        }
    }
}