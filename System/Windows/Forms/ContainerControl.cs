﻿namespace System.Windows.Forms
{
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System;
    using System.Drawing;
    using System.ComponentModel.Design;
    using System.Windows.Forms.Layout;
    using System.Windows.Forms.Internal;
    using Microsoft.Win32;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true),
     ClassInterface(ClassInterfaceType.AutoDispatch)
    ]
    public class ContainerControl : ScrollableControl, IContainerControl
    {
        private Control activeControl; // current active control
        private Control focusedControl; // Current focused control. Do not directly edit this value.
        private Control unvalidatedControl; // The last control that requires validation.  Do not directly edit this value.
        private AutoValidate autoValidate = AutoValidate.Inherit; // Indicates whether automatic validation is turned on.
        private EventHandler autoValidateChanged; // Event fired whenever the AutoValidate property changes.

        // Auto scaling property values
        private SizeF autoScaleDimensions = SizeF.Empty;
        private SizeF currentAutoScaleDimensions = SizeF.Empty;
        private AutoScaleMode autoScaleMode = AutoScaleMode.Inherit;
        private BitVector32 state = new BitVector32();

        private static readonly int stateScalingNeededOnLayout = BitVector32.CreateMask(); // True if we need to perform scaling when layout resumes
        private static readonly int stateValidating = BitVector32.CreateMask(stateScalingNeededOnLayout); // Indicates whether we're currently state[stateValidating].
        private static readonly int stateProcessingMnemonic = BitVector32.CreateMask(stateValidating); // Indicates whether we or one of our children is currently processing a mnemonic.
        private static readonly int stateScalingChild = BitVector32.CreateMask(stateProcessingMnemonic); // True while we are scaling a child control
        private static readonly int stateParentChanged = BitVector32.CreateMask(stateScalingChild); // Flagged when a parent changes so we can adpat our scaling logic to match

        private static readonly int PropAxContainer = PropertyStore.CreateKey();
        private const string fontMeasureString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public ContainerControl() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, false);

            // this class overrides GetPreferredSizeCore, let Control automatically cache the result
            SetState2(STATE2_USEPREFERREDSIZECACHE, true);
        }

        [Localizable(true)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SizeF AutoScaleDimensions
        {
            get
            {
                return autoScaleDimensions;
            }
            [
                SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters") // value is the name of the param passed in.
                                                                                                            // So we don't have to localize it.
            ]
            set
            {
                if (value.Width < 0 || value.Height < 0)
                {
                    throw new ArgumentOutOfRangeException("ContainerControlInvalidAutoScaleDimensions");
                }
                autoScaleDimensions = value;
                if (!autoScaleDimensions.IsEmpty)
                {
                    LayoutScalingNeeded();
                }
            }
        }

        protected SizeF AutoScaleFactor
        {
            get
            {
                SizeF current = CurrentAutoScaleDimensions;
                SizeF saved = AutoScaleDimensions;

                // If no one has configured auto scale dimensions yet, the scaling factor
                // is unity.
                if (saved.IsEmpty)
                {
                    return new SizeF(1.0F, 1.0F);
                }

                return new SizeF(current.Width / saved.Width, current.Height / saved.Height);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AutoScaleMode AutoScaleMode
        {
            get
            {
                return autoScaleMode;
            }
            set
            {
                //valid values are 0x0 to 0x3
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)AutoScaleMode.None, (int)AutoScaleMode.Inherit))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(AutoScaleMode));
                }

                bool scalingNeeded = false;

                if (value != autoScaleMode)
                {

                    // Invalidate any current scaling factors.  If we
                    // are changing AutoScaleMode to anything other than
                    // its default, we should clear out autoScaleDimensions as it is
                    // nonsensical.
                    if (autoScaleMode != AutoScaleMode.Inherit)
                    {
                        autoScaleDimensions = SizeF.Empty;
                    }

                    currentAutoScaleDimensions = SizeF.Empty;
                    autoScaleMode = value;
                    scalingNeeded = true;
                }

                OnAutoScaleModeChanged();

                if (scalingNeeded)
                {
                    LayoutScalingNeeded();
                }
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        AmbientValue(AutoValidate.Inherit),
        ]
        public virtual AutoValidate AutoValidate
        {
            get
            {
                if (autoValidate == AutoValidate.Inherit)
                {
                    return GetAutoValidateForControl(this);
                }
                else
                {
                    return autoValidate;
                }
            }
            set
            {
                // PERF/FXCop: dont use Enum.IsDefined.
                switch (value)
                {
                    case AutoValidate.Disable:
                    case AutoValidate.EnablePreventFocusChange:
                    case AutoValidate.EnableAllowFocusChange:
                    case AutoValidate.Inherit:
                        break;
                    default:
                        throw new InvalidEnumArgumentException("AutoValidate", (int)value, typeof(AutoValidate));
                }

                if (autoValidate != value)
                {
                    autoValidate = value;
                    OnAutoValidateChanged(EventArgs.Empty);
                }
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public event EventHandler AutoValidateChanged
        {
            add
            {
                this.autoValidateChanged += value;
            }
            remove
            {
                this.autoValidateChanged -= value;
            }
        }

        [
        Browsable(false),
        ]
        public override BindingContext BindingContext
        {
            get
            {
                BindingContext bm = base.BindingContext;
                if (bm == null)
                {
                    bm = new BindingContext();
                    BindingContext = bm;
                }
                return bm;
            }
            set
            {
                base.BindingContext = value;
            }
        }

        protected override bool CanEnableIme
        {
            get
            {
                // Note: If overriding this property make sure to copy the Debug code and call this method.

                Debug.Indent();
                Debug.WriteLineIf(CompModSwitches.ImeMode.Level >= TraceLevel.Info, "Inside get_CanEnableIme(), value = false" + ", this = " + this);
                Debug.Unindent();

                return false;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Control ActiveControl
        {
            get
            {
                return activeControl;
            }

            set
            {
                SetActiveControl(value);
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= NativeMethods.WS_EX_CONTROLPARENT;
        //        return cp;
        //    }
        //}

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public SizeF CurrentAutoScaleDimensions
        {
            get
            {
                if (currentAutoScaleDimensions.IsEmpty)
                {
                    switch (AutoScaleMode)
                    {
                        case AutoScaleMode.Font:
                            //currentAutoScaleDimensions = GetFontAutoScaleDimensions();
                            break;

                        case AutoScaleMode.Dpi:
                            // Screen Dpi
                            //if (DpiHelper.EnableDpiChangedMessageHandling)
                            //{
                            //    currentAutoScaleDimensions = new SizeF((float)deviceDpi, (float)deviceDpi);
                            //}
                            //else
                            //{
                            //    // this DPI value comes from the primary monitor.
                            //    currentAutoScaleDimensions = WindowsGraphicsCacheManager.MeasurementGraphics.DeviceContext.Dpi;
                            //}
                            break;

                        default:
                            currentAutoScaleDimensions = AutoScaleDimensions;
                            break;
                    }
                }

                return currentAutoScaleDimensions;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Form ParentForm
        {
            get
            {
                Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "GetParent Demanded");
                IntSecurity.GetParent.Demand();
                return ParentFormInternal;
            }
        }

        internal Form ParentFormInternal
        {
            get
            {
                if (ParentInternal != null)
                {
                    return ParentInternal.FindFormInternal();
                }
                else
                {
                    if (this is Form)
                    {
                        return null;
                    }

                    return FindFormInternal();
                }
            }
        }

        // Package scope for Control
        bool IContainerControl.ActivateControl(Control control)
        {
            Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "ModifyFocus Demanded");
            IntSecurity.ModifyFocus.Demand();

            return ActivateControlInternal(control, true);
        }

        internal bool ActivateControlInternal(Control control)
        {
            return ActivateControlInternal(control, true);
        }

        internal bool ActivateControlInternal(Control control, bool originator)
        {
            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "ContainerControl::ActivateControlInternal(" + (control == null ? "null" : control.Name) + "," + originator.ToString() + ") - " + this.Name);
            // Recursive function that makes sure that the chain of active controls
            // is coherent.
            bool ret = true;
            bool updateContainerActiveControl = false;
            ContainerControl cc = null;
            Control parent = this.ParentInternal;
            if (parent != null)
            {
                cc = (parent.GetContainerControlInternal()) as ContainerControl;
                if (cc != null)
                {
                    updateContainerActiveControl = (cc.ActiveControl != this);
                }
            }
            if (control != activeControl || updateContainerActiveControl)
            {
                if (updateContainerActiveControl)
                {
                    if (!cc.ActivateControlInternal(this, false))
                    {
                        return false;
                    }
                }
                ret = AssignActiveControlInternal((control == this) ? null : control);
            }

            if (originator)
            {
                ScrollActiveControlIntoView();
            }
            return ret;
        }

        internal bool HasFocusableChild()
        {
            Control ctl = null;
            do
            {
                ctl = GetNextControl(ctl, true);
                if (ctl != null &&
                    ctl.CanSelect &&
                    ctl.TabStop)
                {
                    break;
                }
            } while (ctl != null);
            return ctl != null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void AdjustFormScrollbars(bool displayScrollbars)
        {
            base.AdjustFormScrollbars(displayScrollbars);

            if (!GetScrollState(ScrollStateUserHasScrolled))
            {
                ScrollActiveControlIntoView();
            }
        }

        internal virtual void AfterControlRemoved(Control control, Control oldParent)
        {
            ContainerControl cc;
            Debug.Assert(control != null);
            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "ContainerControl::AfterControlRemoved(" + control.Name + ") - " + this.Name);
            if (control == activeControl || control.Contains(activeControl))
            {
                bool selected;
                // SECREVIEW : Note that a function overriding "protected virtual void Control::Select(bool directed, bool forward)"
                //             called by SelectNextControl will be able to set the focus to any control.
                //             This is also enabled by the ModifyFocus.Assert inside Control::SelectNextIfFocused.
                IntSecurity.ModifyFocus.Assert();
                try
                {
                    selected = SelectNextControl(control, true, true, true, true);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (selected && this.activeControl != control)
                {
                    // Bug 847648.
                    // Add the check. If it is set to true, do not call into FocusActiveControlInternal().
                    // The TOP MDI window could be gone and CreateHandle method will fail 
                    // because it try to create a parking window Parent for the MDI children 
                    if (!this.activeControl.Parent.IsTopMdiWindowClosing)
                    {
                        FocusActiveControlInternal();
                    }
                }
                else
                {
                    SetActiveControlInternal(null);
                }
            }
            else if (activeControl == null && ParentInternal != null)
            {
                // The last control of an active container was removed. Focus needs to be given to the next
                // control in the Form.
                cc = ParentInternal.GetContainerControlInternal() as ContainerControl;
                if (cc != null && cc.ActiveControl == this)
                {
                    Form f = FindFormInternal();
                    if (f != null)
                    {
                        // SECREVIEW : Same comment as above.
                        IntSecurity.ModifyFocus.Assert();
                        try
                        {
                            f.SelectNextControl(this, true, true, true, true);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                }
            }

            // VSWhidbey#262686: Two controls in UserControls that don't take focus via UI can have bad behavior if ...
            // VSWhidbey#537131: 
            // When a control is removed from a container, not only do we need to clear the unvalidatedControl of that 
            // container potentially, but the unvalidatedControl of all its container parents, up the chain, needs to
            // now point to the old parent of the disappearing control.
            cc = this;
            while (cc != null)
            {
                Control parent = cc.ParentInternal;
                if (parent == null)
                {
                    break;
                }
                else
                {
                    cc = parent.GetContainerControlInternal() as ContainerControl;
                }
                if (cc != null &&
                    cc.unvalidatedControl != null &&
                    (cc.unvalidatedControl == control || control.Contains(cc.unvalidatedControl)))
                {
                    cc.unvalidatedControl = oldParent;
                }
            }

            if (control == unvalidatedControl || control.Contains(unvalidatedControl))
            {
                unvalidatedControl = null;
            }
        }

        private bool AssignActiveControlInternal(Control value)
        {
#if DEBUG            
            if (value == null || (value != null && value.ParentInternal != null && !value.ParentInternal.IsContainerControl))
            {
                Debug.Assert(value == null || (value.ParentInternal != null && this == value.ParentInternal.GetContainerControlInternal()));
            }
#endif

            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "ContainerControl::AssignActiveControlInternal(" + (value == null ? "null" : value.Name) + ") - " + this.Name);
            if (activeControl != value)
            {
                // cpb: #7318
#if FALSE
                if (activeControl != null) {
                    AxHost.Container cont = FindAxContainer();
                    if (cont != null) {
                        cont.OnOldActiveControl(activeControl, value);
                    }
                }
#endif
                try
                {
                    if (value != null)
                    {
                        value.BecomingActiveControl = true;
                    }
                    activeControl = value;
                    UpdateFocusedControl();
                }
                finally
                {
                    if (value != null)
                    {
                        value.BecomingActiveControl = false;
                    }
                }
                if (activeControl == value)
                {
                    // cpb: #7318
#if FALSE
                    AxHost.Container cont = FindAxContainer();
                    if (cont != null) {
                        cont.OnNewActiveControl(value);
                    }
#endif
                    Form form = FindFormInternal();
                    if (form != null)
                    {
                        form.UpdateDefaultButton();
                    }
                }
            }
            else
            {
                focusedControl = activeControl;
            }
            return (activeControl == value);
        }

        //private void AxContainerFormCreated()
        //{
        //    ((AxHost.AxContainer)Properties.GetObject(PropAxContainer)).FormCreated();
        //}

        internal override bool CanProcessMnemonic()
        {
            if (this.state[stateProcessingMnemonic])
            {
                return true;
            }

            return base.CanProcessMnemonic();
        }

        //internal AxHost.AxContainer CreateAxContainer()
        //{
        //    object aXContainer = Properties.GetObject(PropAxContainer);
        //    if (aXContainer == null)
        //    {
        //        aXContainer = new AxHost.AxContainer(this);
        //        Properties.SetObject(PropAxContainer, aXContainer);
        //    }
        //    return (AxHost.AxContainer)aXContainer;
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                activeControl = null;
            }

            base.Dispose(disposing);

            focusedControl = null;
            unvalidatedControl = null;
        }

        private void EnableRequiredScaling(Control start, bool enable)
        {
            start.RequiredScalingEnabled = enable;
            foreach (Control c in start.Controls)
            {
                EnableRequiredScaling(c, enable);
            }
        }

        internal void FocusActiveControlInternal()
        {
            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "ContainerControl::FocusActiveControlInternal() - " + this.Name);
#if DEBUG
            // Things really get ugly if you try to pop up an assert dialog here
            if (activeControl != null && !this.Contains(activeControl))
                Debug.WriteLine("ActiveControl is not a child of this ContainerControl");
#endif

            if (activeControl != null && activeControl.Visible)
            {

                // Avoid focus loops, especially with ComboBoxes, on Win98/ME.
                //
                //IntPtr focusHandle = UnsafeNativeMethods.GetFocus();
                //if (focusHandle == IntPtr.Zero || Control.FromChildHandleInternal(focusHandle) != activeControl)
                //{
                //    UnsafeNativeMethods.SetFocus(new HandleRef(activeControl, activeControl.Handle));
                //}
            }
            else
            {
                // Determine and focus closest visible parent
                ContainerControl cc = this;
                while (cc != null && !cc.Visible)
                {
                    Control parent = cc.ParentInternal;
                    if (parent != null)
                    {
                        cc = parent.GetContainerControlInternal() as ContainerControl;
                    }
                    else
                    {
                        break;
                    }
                }
                if (cc != null && cc.Visible)
                {
                    //UnsafeNativeMethods.SetFocus(new HandleRef(cc, cc.Handle));
                }
            }
        }


        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            // Translating 0,0 from ClientSize to actual Size tells us how much space
            // is required for the borders.
            Size borderSize = SizeFromClientSize(Size.Empty);
            Size totalPadding = borderSize + Padding.Size;
            return LayoutEngine.GetPreferredSize(this, proposedSize - totalPadding) + totalPadding;
        }

        //internal override Rectangle GetToolNativeScreenRectangle()
        //{
        //    if (this.GetTopLevel())
        //    {
        //        // Get window's client rectangle (i.e. without chrome) expressed in screen coordinates
        //        NativeMethods.RECT clientRectangle = new NativeMethods.RECT();
        //        UnsafeNativeMethods.GetClientRect(new HandleRef(this, this.Handle), ref clientRectangle);
        //        NativeMethods.POINT topLeftPoint = new NativeMethods.POINT(0, 0);
        //        UnsafeNativeMethods.ClientToScreen(new HandleRef(this, this.Handle), topLeftPoint);
        //        return new Rectangle(topLeftPoint.x, topLeftPoint.y, clientRectangle.right, clientRectangle.bottom);
        //    }
        //    else
        //    {
        //        return base.GetToolNativeScreenRectangle();
        //    }
        //}

        //[SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")] // Refers to 'fontMeasureString'.
        //[SuppressMessage("Microsoft.Usage", "CA2204:LiteralsShouldBeSpelledCorrectly")]
        //private SizeF GetFontAutoScaleDimensions()
        //{
        //    SizeF retval = SizeF.Empty;

        //    // Windows uses CreateCompatibleDC(NULL) to get a
        //    // memory DC for the monitor the application is currently
        //    // on.
        //    IntPtr dc = UnsafeNativeMethods.CreateCompatibleDC(NativeMethods.NullHandleRef);
        //    if (dc == IntPtr.Zero)
        //    {
        //        throw new Win32Exception();
        //    }

        //    HandleRef hdc = new HandleRef(this, dc);

        //    try
        //    {
        //        // We clone the Windows scaling function here as closely as
        //        // possible.  They use textmetric for height, and textmetric
        //        // for width of fixed width fonts.  For variable width fonts
        //        // they use GetTextExtentPoint32 and pass in a long a-Z string.
        //        // We must do the same here if our dialogs are to scale in a
        //        // similar fashion.

        //        HandleRef hfont = new HandleRef(this, FontHandle);
        //        HandleRef hfontOld = new HandleRef(this, SafeNativeMethods.SelectObject(hdc, hfont));

        //        try
        //        {
        //            NativeMethods.TEXTMETRIC tm = new NativeMethods.TEXTMETRIC();
        //            SafeNativeMethods.GetTextMetrics(hdc, ref tm);

        //            retval.Height = tm.tmHeight;

        //            if ((tm.tmPitchAndFamily & NativeMethods.TMPF_FIXED_PITCH) != 0)
        //            {
        //                IntNativeMethods.SIZE size = new IntNativeMethods.SIZE();
        //                IntUnsafeNativeMethods.GetTextExtentPoint32(hdc, fontMeasureString, size);
        //                // Note: intentional integer round off here for Win32 compat
        //                //retval.Width = (float)(((size.cx / 26) + 1) / 2);
        //                retval.Width = (int)Math.Round(((float)size.cx) / ((float)fontMeasureString.Length));
        //            }
        //            else
        //            {
        //                retval.Width = tm.tmAveCharWidth;
        //            }
        //        }
        //        finally
        //        {
        //            SafeNativeMethods.SelectObject(hdc, hfontOld);
        //        }
        //    }
        //    finally
        //    {
        //        UnsafeNativeMethods.DeleteCompatibleDC(hdc);
        //    }

        //    return retval;
        //}

        private void LayoutScalingNeeded()
        {

            EnableRequiredScaling(this, true);
            state[stateScalingNeededOnLayout] = true;

            // If layout is not currently suspended, then perform a layout now,
            // as otherwise we don't know when one will happen.
            if (!IsLayoutSuspended)
            {
                LayoutTransaction.DoLayout(this, this, PropertyNames.Bounds);
            }
        }

        internal virtual void OnAutoScaleModeChanged()
        {
        }

        protected virtual void OnAutoValidateChanged(EventArgs e)
        {
            if (autoValidateChanged != null)
            {
                autoValidateChanged(this, e);
            }
        }

        // Refer VsWhidbey : 515910 & 269769
        internal override void OnFrameWindowActivate(bool fActivate)
        {
            if (fActivate)
            {
                IntSecurity.ModifyFocus.Assert();
                try
                {
                    if (ActiveControl == null)
                    {
                        SelectNextControl(null, true, true, true, false);
                    }
                    InnerMostActiveContainerControl.FocusActiveControlInternal();
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        internal override void OnChildLayoutResuming(Control child, bool performLayout)
        {
            base.OnChildLayoutResuming(child, performLayout);

            // do not scale children if AutoScaleMode is set to Dpi
            //if (DpiHelper.EnableSinglePassScalingOfDpiForms && (AutoScaleMode == AutoScaleMode.Dpi))
            //{
            //    return;
            //}

            // We need to scale children before their layout engines get to them.
            // We don't have a lot of opportunity for that because the code
            // generator always generates a PerformLayout() right after a
            // ResumeLayout(false).  This seems to be the most oppportune place
            // for thiis, although it is unfortunate.
            if (!state[stateScalingChild] && !performLayout && AutoScaleMode != AutoScaleMode.None && AutoScaleMode != AutoScaleMode.Inherit && state[stateScalingNeededOnLayout])
            {
                state[stateScalingChild] = true;
                try
                {
                    child.Scale(AutoScaleFactor, SizeF.Empty, this);
                }
                finally
                {
                    state[stateScalingChild] = false;
                }
            }
        }

        internal override void OnCreateControl()
        {
            base.OnCreateControl();

            //if (Properties.GetObject(PropAxContainer) != null)
            //{
            //    AxContainerFormCreated();
            //}
            OnBindingContextChanged(EventArgs.Empty);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal override void OnFontChanged(EventArgs e)
        {
            if (AutoScaleMode == AutoScaleMode.Font)
            {
                currentAutoScaleDimensions = SizeF.Empty;

                // If the font changes and we are going to autoscale
                // as a result, do it now, and wrap the entire
                // transaction in a suspend layout to prevent
                // the layout engines from conflicting with our
                // work.
                SuspendAllLayout(this);

                try
                {
                    PerformAutoScale(!RequiredScalingEnabled, true);
                }
                finally
                {
                    ResumeAllLayout(this, false);
                }
            }

            base.OnFontChanged(e);
        }

        internal void FormDpiChanged(float factor)
        {
            Debug.Assert(this is Form);

            currentAutoScaleDimensions = SizeF.Empty;

            SuspendAllLayout(this);
            SizeF factorSize = new SizeF(factor, factor);
            try
            {
                //ScaleChildControls(factorSize, factorSize, this, true);
            }
            finally
            {
                ResumeAllLayout(this, false);
            }
        }

        internal override void OnLayout(LayoutEventArgs e)
        {
            PerformNeededAutoScaleOnLayout();
            base.OnLayout(e);
        }

        internal override void OnLayoutResuming(bool performLayout)
        {
            PerformNeededAutoScaleOnLayout();
            base.OnLayoutResuming(performLayout);
        }

        internal override void OnParentChanged(EventArgs e)
        {
            state[stateParentChanged] = !RequiredScalingEnabled;
            base.OnParentChanged(e);
        }

        public void PerformAutoScale()
        {
            PerformAutoScale(true, true);
        }

        private void PerformAutoScale(bool includedBounds, bool excludedBounds)
        {

            bool suspended = false;

            try
            {
                if (AutoScaleMode != AutoScaleMode.None && AutoScaleMode != AutoScaleMode.Inherit)
                {
                    SuspendAllLayout(this);
                    suspended = true;

                    // Walk each control recursively and scale.  We search the control
                    // for its own set of scaling data; if we don't find it, we use the current
                    // container control's scaling data.  Once we scale a control, we set
                    // its scaling factors to unity.  As we walk out of a container control,
                    // we set its scaling factor to unity too.
                    SizeF included = SizeF.Empty;
                    SizeF excluded = SizeF.Empty;

                    if (includedBounds) included = AutoScaleFactor;
                    if (excludedBounds) excluded = AutoScaleFactor;

                    Scale(included, excluded, this);
                    autoScaleDimensions = CurrentAutoScaleDimensions;
                }
            }
            finally
            {
                if (includedBounds)
                {
                    state[stateScalingNeededOnLayout] = false;
                    EnableRequiredScaling(this, false);
                }
                state[stateParentChanged] = false;

                if (suspended)
                {
                    ResumeAllLayout(this, false);
                }
            }
        }

        private void PerformNeededAutoScaleOnLayout()
        {
            if (state[stateScalingNeededOnLayout])
            {
                PerformAutoScale(state[stateScalingNeededOnLayout], false);
            }
        }

        internal void ResumeAllLayout(Control start, bool performLayout)
        {

            ControlCollection controlsCollection = start.Controls;
            // This may have changed the sizes of our children.
            // PERFNOTE: This is more efficient than using Foreach.  Foreach
            // forces the creation of an array subset enum each time we
            // enumerate
            for (int i = 0; i < controlsCollection.Count; i++)
            {
                ResumeAllLayout(controlsCollection[i], performLayout);
            }

            start.ResumeLayout(performLayout);
        }

        internal void SuspendAllLayout(Control start)
        {
            start.SuspendLayout();
            CommonProperties.xClearPreferredSizeCache(start);

            ControlCollection controlsCollection = start.Controls;
            // This may have changed the sizes of our children.
            // PERFNOTE: This is more efficient than using Foreach.  Foreach
            // forces the creation of an array subset enum each time we
            // enumerate
            for (int i = 0; i < controlsCollection.Count; i++)
            {
                SuspendAllLayout(controlsCollection[i]);
            }
        }

        internal override void Scale(SizeF includedFactor, SizeF excludedFactor, Control requestingControl)
        {

            // If we're inhieriting our scaling from our parent, Scale is really easy:  just do the
            // base class implementation.
            if (AutoScaleMode == AutoScaleMode.Inherit)
            {
                base.Scale(includedFactor, excludedFactor, requestingControl);
            }
            else
            {
                // We scale our controls based on our own auto scaling
                // factor, not the one provided to us.  We only do this for
                // controls that are not required to be scaled (excluded controls).
                SizeF ourExcludedFactor = excludedFactor;
                SizeF childIncludedFactor = includedFactor;

                if (!ourExcludedFactor.IsEmpty)
                {
                    ourExcludedFactor = AutoScaleFactor;
                }

                // If we're not supposed to be scaling, don't scale the internal
                // ones either.
                if (AutoScaleMode == AutoScaleMode.None)
                {
                    childIncludedFactor = AutoScaleFactor;
                }

                // When we scale, we are establishing new baselines for the
                // positions of all controls.  Therefore, we should resume(false).
                using (new LayoutTransaction(this, this, PropertyNames.Bounds, false))
                {

                    // Our own container control poses a problem.  We want
                    // an outer control to be responsible for scaling it,
                    // because the outer control knows the container's dimensions.
                    // We detect this by checking who is requesting that the
                    // scaling occur.
                    SizeF ourExternalContainerFactor = ourExcludedFactor;

                    if (!excludedFactor.IsEmpty && ParentInternal != null)
                    {
                        ourExternalContainerFactor = SizeF.Empty;

                        bool scaleUs = (requestingControl != this || state[stateParentChanged]);

                        // Hack for design time support:  we may be parented within another form
                        // that is not part of the designer.
                        if (!scaleUs)
                        {
                            bool dt = false;
                            bool parentDt = false;
                            ISite site = Site;
                            ISite parentSite = ParentInternal.Site;

                            if (site != null) dt = site.DesignMode;
                            if (parentSite != null) parentDt = parentSite.DesignMode;

                            if (dt && !parentDt)
                            {
                                scaleUs = true;
                            }
                        }

                        if (scaleUs)
                        {
                            ourExternalContainerFactor = excludedFactor;
                        }
                    }

                    //ScaleControl(includedFactor, ourExternalContainerFactor, requestingControl);
                    //ScaleChildControls(childIncludedFactor, ourExcludedFactor, requestingControl);
                }
            }
        }

        private bool ProcessArrowKey(bool forward)
        {
            Control group = this;
            if (activeControl != null)
            {
                group = activeControl.ParentInternal;
            }
            return group.SelectNextControl(activeControl, forward, false, false, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogChar(char charCode)
        {
#if DEBUG
            Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "ContainerControl.ProcessDialogChar [" + charCode.ToString() + "]");
#endif
            // If we're the top-level form or control, we need to do the mnemonic handling
            //
            ContainerControl parent = GetContainerControlInternal() as ContainerControl;
            if (parent != null && charCode != ' ' && ProcessMnemonic(charCode)) return true;
            return base.ProcessDialogChar(charCode);
        }

        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        internal override bool ProcessDialogKey(Keys keyData)
        {
#if DEBUG
            Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "ContainerControl.ProcessDialogKey [" + keyData.ToString() + "]");
#endif
            if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
            {
                Keys keyCode = (Keys)keyData & Keys.KeyCode;
                switch (keyCode)
                {
                    case Keys.Tab:
                        if (ProcessTabKey((keyData & Keys.Shift) == Keys.None)) return true;
                        break;
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Up:
                    case Keys.Down:
                        if (ProcessArrowKey(keyCode == Keys.Right ||
                                            keyCode == Keys.Down)) return true;
                        break;
                }
            }
            return base.ProcessDialogKey(keyData);
        }


        //[
        //    SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)
        //]
        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "ContainerControl.ProcessCmdKey " + msg.ToString());

        //    if (base.ProcessCmdKey(ref msg, keyData))
        //    {
        //        return true;
        //    }
        //    if (ParentInternal == null)
        //    {
        //        // unfortunately, we have to stick this here for the case where we're hosted without
        //        // a form in the chain.  This would be something like a context menu strip with shortcuts
        //        // hosted within Office, VS or IE.
        //        //
        //        // this is an optimized search O(number of ToolStrips in thread)
        //        // that happens only if the key routing makes it to the top.
        //        return ToolStripManager.ProcessCmdKey(ref msg, keyData);
        //    }
        //    return false;
        //}

        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
#if DEBUG
            Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "ContainerControl.ProcessMnemonic [" + charCode.ToString() + "]");
            Debug.Indent();
            Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "this == " + ToString());
#endif
            if (!CanProcessMnemonic())
            {
                return false;
            }

            if (Controls.Count == 0)
            {
                Debug.Unindent();
                return false;
            }

            // Start with the active control.
            //
            Control start = ActiveControl;

#if DEBUG
            int count = 0;
#endif //DEBUG

            // Set the processing mnemonic flag so child controls don't check for it when checking if they
            // can process the mnemonic.
            this.state[stateProcessingMnemonic] = true;

            bool processed = false;

            try
            {
                // safety flag to avoid infinite loop when testing controls in a container.
                bool wrapped = false;

                Control ctl = start;
                Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "Check starting at '" + ((start != null) ? start.ToString() : "<null>") + "'");

                do
                {
                    // Loop through the controls starting at the control next to the current Active control in the Tab order
                    // till we find someone willing to process this mnemonic.
                    // We don't start the search on the Active control to allow controls in the same container with the same 
                    // mnemonic (bad UI design but supported) to be processed sequentially (see VSWhidbey#428029).
#if DEBUG
                    count++;
                    if (count > 9999)
                    {
                        Debug.Fail("Infinite loop trying to find controls which can ProcessMnemonic()!!!");
                    }
#endif //DEBUG
                    ctl = GetNextControl(ctl, true);

                    if (ctl != null)
                    {
                        // Processing the mnemonic can change the value of CanProcessMnemonic. See ASURT 39583.
                        if (ctl.ProcessMnemonic(charCode))
                        {
                            processed = true;
                            break;
                        }
                    }
                    else
                    { // ctl is null
                        if (wrapped)
                        {
                            break;      // This avoids infinite loops
                        }

                        wrapped = true;
                    }
                } while (ctl != start);
            }
            finally
            {
                this.state[stateProcessingMnemonic] = false;
            }

            Debug.Unindent();
            return processed;
        }

        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected virtual bool ProcessTabKey(bool forward)
        {
            if (SelectNextControl(activeControl, forward, true, true, false)) return true;
            return false;
        }

        private ScrollableControl FindScrollableParent(Control ctl)
        {
            Control current = ctl.ParentInternal;
            while (current != null && !(current is ScrollableControl))
            {
                current = current.ParentInternal;
            }
            if (current != null)
            {
                return (ScrollableControl)current;
            }
            return null;
        }

        private void ScrollActiveControlIntoView()
        {
            Control last = activeControl;
            if (last != null)
            {
                ScrollableControl scrollParent = FindScrollableParent(last);

                while (scrollParent != null)
                {
                    scrollParent.ScrollControlIntoView(activeControl);
                    last = scrollParent;
                    scrollParent = FindScrollableParent(scrollParent);
                }
            }
        }

        protected override void Select(bool directed, bool forward)
        {
            bool correctParentActiveControl = true;
            if (ParentInternal != null)
            {
                IContainerControl c = ParentInternal.GetContainerControlInternal();
                if (c != null)
                {
                    c.ActiveControl = this;
                    correctParentActiveControl = (c.ActiveControl == this);
                }
            }
            if (directed && correctParentActiveControl)
            {
                SelectNextControl(null, forward, true, true, false);
            }
        }

        private void SetActiveControl(Control ctl)
        {
            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "ContainerControl::SetActiveControl(" + (ctl == null ? "null" : ctl.Name) + ") - " + this.Name);

            SetActiveControlInternal(ctl);
        }

        internal void SetActiveControlInternal(Control value)
        {
            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "ContainerControl::SetActiveControlInternal(" + (value == null ? "null" : value.Name) + ") - " + this.Name);
            if (activeControl != value || (value != null /*&& !value.Focused*/))
            {
                if (value != null && !Contains(value))
                {
                    throw new ArgumentException("CannotActivateControl");
                }

                bool ret;
                ContainerControl cc = this;

                if (value != null && value.ParentInternal != null)
                {
                    cc = (value.ParentInternal.GetContainerControlInternal()) as ContainerControl;
                }
                if (cc != null)
                {
                    // Call to the recursive function that corrects the chain
                    // of active controls
                    ret = cc.ActivateControlInternal(value, false);
                }
                else
                {
                    ret = AssignActiveControlInternal(value);
                }

                if (cc != null && ret)
                {
                    ContainerControl ccAncestor = this;
                    while (ccAncestor.ParentInternal != null &&
                           ccAncestor.ParentInternal.GetContainerControlInternal() is ContainerControl)
                    {
                        ccAncestor = ccAncestor.ParentInternal.GetContainerControlInternal() as ContainerControl;
                        Debug.Assert(ccAncestor != null);
                    }

                    //if (ccAncestor.ContainsFocus &&
                    //    (value == null ||
                    //     !(value is UserControl) ||
                    //     (value is UserControl && !((UserControl)value).HasFocusableChild())))
                    //{
                    //    cc.FocusActiveControlInternal();
                    //}
                }
            }
        }

        internal ContainerControl InnerMostActiveContainerControl
        {
            get
            {
                ContainerControl ret = this;
                while (ret.ActiveControl is ContainerControl)
                {
                    ret = (ContainerControl)ret.ActiveControl;
                }
                return ret;
            }
        }

        internal ContainerControl InnerMostFocusedContainerControl
        {
            get
            {
                ContainerControl ret = this;
                while (ret.focusedControl is ContainerControl)
                {
                    ret = (ContainerControl)ret.focusedControl;
                }
                return ret;
            }
        }

        protected virtual void UpdateDefaultButton()
        {
            // hook for form
        }

        internal void UpdateFocusedControl()
        {
            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "ContainerControl::UpdateFocusedControl() - " + this.Name);

            // Capture the current focusedControl as the unvalidatedControl if we don't have one/are not validating.
            EnsureUnvalidatedControl(focusedControl);
            Control pathControl = focusedControl;

            while (activeControl != pathControl)
            {
                if (pathControl == null || pathControl.IsDescendant(activeControl))
                {
                    // heading down. find next control on path.
                    Control nextControlDown = activeControl;
                    while (true)
                    {
                        Control parent = nextControlDown.ParentInternal;
                        if (parent == this || parent == pathControl)
                            break;
                        nextControlDown = nextControlDown.ParentInternal;
                    }

                    Control priorFocusedControl = focusedControl = pathControl;
                    EnterValidation(nextControlDown);
                    // If validation changed position, then jump back to the loop.
                    if (focusedControl != priorFocusedControl)
                    {
                        pathControl = focusedControl;
                        continue;
                    }

                    pathControl = nextControlDown;
                    //if (NativeWindow.WndProcShouldBeDebuggable)
                    //{
                    //    pathControl.NotifyEnter();
                    //}
                    //else
                    {
                        try
                        {
                            pathControl.NotifyEnter();
                        }
                        catch (Exception e)
                        {
                            Application.OnThreadException(e);
                        }
                    }
                }
                else
                {
                    // heading up.
                    ContainerControl innerMostFCC = InnerMostFocusedContainerControl;
                    Control stopControl = null;

                    if (innerMostFCC.focusedControl != null)
                    {
                        pathControl = innerMostFCC.focusedControl;
                        stopControl = innerMostFCC;

                        if (innerMostFCC != this)
                        {
                            innerMostFCC.focusedControl = null;
                            if (!(innerMostFCC.ParentInternal != null && innerMostFCC.ParentInternal is MdiClient))
                            {
                                // Don't reset the active control of a MDIChild that loses the focus
                                innerMostFCC.activeControl = null;
                            }
                        }
                    }
                    else
                    {
                        pathControl = innerMostFCC;
                        // innerMostFCC.ParentInternal can be null when the ActiveControl is deleted.
                        if (innerMostFCC.ParentInternal != null)
                        {
                            ContainerControl cc = (innerMostFCC.ParentInternal.GetContainerControlInternal()) as ContainerControl;
                            stopControl = cc;
                            if (cc != null && cc != this)
                            {
                                cc.focusedControl = null;
                                cc.activeControl = null;
                            }
                        }
                    }

                    do
                    {
                        Control leaveControl = pathControl;

                        if (pathControl != null)
                        {
                            pathControl = pathControl.ParentInternal;
                        }

                        if (pathControl == this)
                        {
                            pathControl = null;
                        }

                        if (leaveControl != null)
                        {
                            //if (NativeWindow.WndProcShouldBeDebuggable)
                            //{
                            //    leaveControl.NotifyLeave();
                            //}
                            //else
                            {
                                try
                                {
                                    leaveControl.NotifyLeave();
                                }
                                catch (Exception e)
                                {
                                    Application.OnThreadException(e);
                                }
                            }
                        }
                    }
                    while (pathControl != null &&
                           pathControl != stopControl &&
                           !pathControl.IsDescendant(activeControl));
                }
            }

#if DEBUG
            if (activeControl == null || (activeControl != null && activeControl.ParentInternal != null && !activeControl.ParentInternal.IsContainerControl))
            {
                Debug.Assert(activeControl == null || activeControl.ParentInternal.GetContainerControlInternal() == this);
            }
#endif
            focusedControl = activeControl;
            if (activeControl != null)
            {
                EnterValidation(activeControl);
            }
        }

        private void EnsureUnvalidatedControl(Control candidate)
        {
            // Don't change the unvalidated control while in the middle of validation (re-entrancy)
            if (state[stateValidating])
            {
                return;
            }

            // Don't change the existing unvalidated control
            if (unvalidatedControl != null)
            {
                return;
            }

            // No new choice of unvalidated control was specified - leave unvalidated control blank
            if (candidate == null)
            {
                return;
            }

            // Specified control has auto-validation disabled - leave unvalidated control blank
            if (!candidate.ShouldAutoValidate)
            {
                return;
            }

            // Go ahead and make specified control the current unvalidated control for this container
            unvalidatedControl = candidate;

            // In the case of nested container controls, try to pick the deepest possible unvalidated
            // control. For a container with no unvalidated control, use the active control instead.
            // Stop as soon as we encounter any control that has auto-validation turned off.
            while (unvalidatedControl is ContainerControl)
            {
                ContainerControl container = unvalidatedControl as ContainerControl;

                if (container.unvalidatedControl != null && container.unvalidatedControl.ShouldAutoValidate)
                {
                    unvalidatedControl = container.unvalidatedControl;
                }
                else if (container.activeControl != null && container.activeControl.ShouldAutoValidate)
                {
                    unvalidatedControl = container.activeControl;
                }
                else
                {
                    break;
                }
            }
        }

        private void EnterValidation(Control enterControl)
        {
            // No unvalidated control to validate - stop now
            if (unvalidatedControl == null)
            {
                return;
            }

            // Entered control does not trigger validation - stop now
            if (!enterControl.CausesValidation)
            {
                return;
            }

            // Get the effective AutoValidate mode for this control (based on its container control)
            AutoValidate autoValidateMode = Control.GetAutoValidateForControl(unvalidatedControl);

            // Auto-validate has been turned off in container of unvalidated control - stop now
            if (autoValidateMode == AutoValidate.Disable)
            {
                return;
            }

            // Find common ancestor of entered control and unvalidated control
            Control commonAncestor = enterControl;
            while (commonAncestor != null && !commonAncestor.IsDescendant(unvalidatedControl))
            {
                commonAncestor = commonAncestor.ParentInternal;
            }

            // Should we force focus to stay on same control if there is a validation error?
            bool preventFocusChangeOnError = (autoValidateMode == AutoValidate.EnablePreventFocusChange);

            // Validate control and its ancestors, up to (but not including) the common ancestor
            ValidateThroughAncestor(commonAncestor, preventFocusChangeOnError);
        }

        ///
        //
        // -------------------------------
        // INTERNAL NOTE FOR Microsoft DEVS: This version is intended for user code that wants to force validation, even
        // while auto-validation is turned off. When adding any explicit Validate() calls to our code, consider using
        // Validate(true) rather than Validate(), so that you will be sensitive to the current auto-validation setting.
        // -------------------------------
        //
        public bool Validate()
        {
            return Validate(false);
        }

        ///
        public bool Validate(bool checkAutoValidate)
        {
            bool validatedControlAllowsFocusChange;
            return ValidateInternal(checkAutoValidate, out validatedControlAllowsFocusChange);
        }

        internal bool ValidateInternal(bool checkAutoValidate, out bool validatedControlAllowsFocusChange)
        {
            validatedControlAllowsFocusChange = false;

            if (this.AutoValidate == AutoValidate.EnablePreventFocusChange ||
                (activeControl != null && activeControl.CausesValidation))
            {
                if (unvalidatedControl == null)
                {
                    if (focusedControl is ContainerControl && focusedControl.CausesValidation)
                    {
                        ContainerControl c = (ContainerControl)focusedControl;
                        if (!c.ValidateInternal(checkAutoValidate, out validatedControlAllowsFocusChange))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        unvalidatedControl = focusedControl;
                    }
                }

                // Should we force focus to stay on same control if there is a validation error?
                bool preventFocusChangeOnError = true;

                Control controlToValidate = unvalidatedControl != null ? unvalidatedControl : focusedControl;

                if (controlToValidate != null)
                {
                    // Get the effective AutoValidate mode for unvalidated control (based on its container control)
                    AutoValidate autoValidateMode = Control.GetAutoValidateForControl(controlToValidate);

                    // Auto-validate has been turned off in container of unvalidated control - stop now
                    if (checkAutoValidate && autoValidateMode == AutoValidate.Disable)
                    {
                        return true;
                    }
                    preventFocusChangeOnError = (autoValidateMode == AutoValidate.EnablePreventFocusChange);
                    validatedControlAllowsFocusChange = (autoValidateMode == AutoValidate.EnableAllowFocusChange);
                }

                return ValidateThroughAncestor(null, preventFocusChangeOnError);
            }
            return true;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ValidateChildren()
        {
            return ValidateChildren(ValidationConstraints.Selectable);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ValidateChildren(ValidationConstraints validationConstraints)
        {
            // validationConstraints must be a combination of 
            // None = 0x00, Selectable = 0x01, Enabled = 0x02, Visible = 0x04, TabStop = 0x08, ImmediateChildren = 0x10
            // Not using ClientUtils.IsValidEnum here because this is a flags enum and everything is valid between 0x00 and 0x1F.
            if ((int)validationConstraints < 0x00 || (int)validationConstraints > 0x1F)
            {
                throw new InvalidEnumArgumentException("validationConstraints", (int)validationConstraints, typeof(ValidationConstraints));
            }
            return !PerformContainerValidation(validationConstraints);
        }

        private bool ValidateThroughAncestor(Control ancestorControl, bool preventFocusChangeOnError)
        {
            if (ancestorControl == null)
                ancestorControl = this;
            if (state[stateValidating])
                return false;
            if (unvalidatedControl == null)
                unvalidatedControl = focusedControl;
            //return true for a Container Control with no controls to validate....
            //
            if (unvalidatedControl == null)
                return true;
            if (!ancestorControl.IsDescendant(unvalidatedControl))
                return false;

            this.state[stateValidating] = true;
            bool cancel = false;

            Control currentActiveControl = activeControl;
            Control currentValidatingControl = unvalidatedControl;
            if (currentActiveControl != null)
            {
                currentActiveControl.ValidationCancelled = false;
                if (currentActiveControl is ContainerControl)
                {
                    ContainerControl currentActiveContainerControl = currentActiveControl as ContainerControl;

                    currentActiveContainerControl.ResetValidationFlag();
                }
            }
            try
            {
                while (currentValidatingControl != null && currentValidatingControl != ancestorControl)
                {
                    try
                    {
                        cancel = currentValidatingControl.PerformControlValidation(false);
                    }
                    catch
                    {
                        cancel = true;
                        throw;
                    }

                    if (cancel)
                    {
                        break;
                    }

                    currentValidatingControl = currentValidatingControl.ParentInternal;
                }

                if (cancel && preventFocusChangeOnError)
                {
                    if (unvalidatedControl == null && currentValidatingControl != null &&
                        ancestorControl.IsDescendant(currentValidatingControl))
                    {
                        unvalidatedControl = currentValidatingControl;
                    }
                    // This bit 'marks' the control that was going to get the focus, so that it will ignore any pending
                    // mouse or key events. Otherwise it would still perform its default 'click' action or whatever.
                    if (currentActiveControl == activeControl)
                    {
                        if (currentActiveControl != null)
                        {
                            CancelEventArgs ev = new CancelEventArgs();
                            ev.Cancel = true;
                            currentActiveControl.NotifyValidationResult(currentValidatingControl, ev);
                            if (currentActiveControl is ContainerControl)
                            {
                                ContainerControl currentActiveContainerControl = currentActiveControl as ContainerControl;
                                if (currentActiveContainerControl.focusedControl != null)
                                {
                                    currentActiveContainerControl.focusedControl.ValidationCancelled = true;
                                }
                                currentActiveContainerControl.ResetActiveAndFocusedControlsRecursive();
                            }
                        }
                    }
                    // This bit forces the focus to move back to the invalid control
                    SetActiveControlInternal(unvalidatedControl);
                }
            }
            finally
            {
                unvalidatedControl = null;
                state[stateValidating] = false;
            }

            return !cancel;
        }

        private void ResetValidationFlag()
        {
            // PERFNOTE: This is more efficient than using Foreach.  Foreach
            // forces the creation of an array subset enum each time we
            // enumerate
            Control.ControlCollection children = this.Controls;
            int count = children.Count;
            for (int i = 0; i < count; i++)
            {
                children[i].ValidationCancelled = false;
            }
        }

        internal void ResetActiveAndFocusedControlsRecursive()
        {
            if (activeControl is ContainerControl)
            {
                ((ContainerControl)activeControl).ResetActiveAndFocusedControlsRecursive();
            }
            activeControl = null;
            focusedControl = null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeAutoValidate()
        {
            return autoValidate != AutoValidate.Inherit;
        }

        //private void WmSetFocus(ref Message m)
        //{
        //    Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "ContainerControl::WmSetFocus() - " + this.Name);
        //    if (!HostedInWin32DialogManager)
        //    {
        //        if (ActiveControl != null)
        //        {
        //            WmImeSetFocus();
        //            // Microsoft: Do not raise GotFocus event since the focus
        //            //         is given to the visible ActiveControl
        //            if (!ActiveControl.Visible)
        //            {
        //                this.InvokeGotFocus(this, EventArgs.Empty);
        //            }
        //            FocusActiveControlInternal();
        //        }
        //        else
        //        {
        //            if (ParentInternal != null)
        //            {
        //                IContainerControl c = ParentInternal.GetContainerControlInternal();
        //                if (c != null)
        //                {
        //                    bool succeeded = false;

        //                    ContainerControl knowncontainer = c as ContainerControl;
        //                    if (knowncontainer != null)
        //                    {
        //                        succeeded = knowncontainer.ActivateControlInternal(this);
        //                    }
        //                    else
        //                    {

        //                        // SECREVIEW : Taking focus and activating a control in response
        //                        //           : to a user gesture (WM_SETFOCUS) is OK.
        //                        //
        //                        IntSecurity.ModifyFocus.Assert();
        //                        try
        //                        {
        //                            succeeded = c.ActivateControl(this);
        //                        }
        //                        finally
        //                        {
        //                            CodeAccessPermission.RevertAssert();
        //                        }
        //                    }
        //                    if (!succeeded)
        //                    {
        //                        return;
        //                    }
        //                }
        //            }
        //            base.WndProc(ref m);
        //        }
        //    }
        //    else
        //    {
        //        base.WndProc(ref m);
        //    }
        //}

        //[EditorBrowsable(EditorBrowsableState.Advanced)]
        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //protected override void WndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {
        //        case NativeMethods.WM_SETFOCUS:
        //            WmSetFocus(ref m);
        //            break;
        //        default:
        //            base.WndProc(ref m);
        //            break;
        //    }
        //}
    }
}