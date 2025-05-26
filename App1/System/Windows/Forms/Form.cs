using App1;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Windows.Forms
{
    public class Form : ContainerControl
    {
        private static readonly object EVENT_ACTIVATED = new object();
        private static readonly object EVENT_CLOSING = new object();
        private static readonly object EVENT_CLOSED = new object();
        private static readonly object EVENT_FORMCLOSING = new object();
        private static readonly object EVENT_FORMCLOSED = new object();
        private static readonly object EVENT_DEACTIVATE = new object();
        private static readonly object EVENT_LOAD = new object();
        private static readonly object EVENT_MDI_CHILD_ACTIVATE = new object();
        private static readonly object EVENT_INPUTLANGCHANGE = new object();
        private static readonly object EVENT_INPUTLANGCHANGEREQUEST = new object();
        private static readonly object EVENT_MENUSTART = new object();
        private static readonly object EVENT_MENUCOMPLETE = new object();
        private static readonly object EVENT_MAXIMUMSIZECHANGED = new object();
        private static readonly object EVENT_MINIMUMSIZECHANGED = new object();
        private static readonly object EVENT_HELPBUTTONCLICKED = new object();
        private static readonly object EVENT_SHOWN = new object();
        private static readonly object EVENT_RESIZEBEGIN = new object();
        private static readonly object EVENT_RESIZEEND = new object();
        private static readonly object EVENT_RIGHTTOLEFTLAYOUTCHANGED = new object();
        private static readonly object EVENT_DPI_CHANGED = new object();

        //
        // The following flags should be used with formState[..] not formStateEx[..]
        // Don't add any more sections to this vector, it is already full.
        //
        private static readonly BitVector32.Section FormStateAllowTransparency = BitVector32.CreateSection(1);
        private static readonly BitVector32.Section FormStateBorderStyle = BitVector32.CreateSection(6, FormStateAllowTransparency);
        private static readonly BitVector32.Section FormStateTaskBar = BitVector32.CreateSection(1, FormStateBorderStyle);
        private static readonly BitVector32.Section FormStateControlBox = BitVector32.CreateSection(1, FormStateTaskBar);
        private static readonly BitVector32.Section FormStateKeyPreview = BitVector32.CreateSection(1, FormStateControlBox);
        private static readonly BitVector32.Section FormStateLayered = BitVector32.CreateSection(1, FormStateKeyPreview);
        private static readonly BitVector32.Section FormStateMaximizeBox = BitVector32.CreateSection(1, FormStateLayered);
        private static readonly BitVector32.Section FormStateMinimizeBox = BitVector32.CreateSection(1, FormStateMaximizeBox);
        private static readonly BitVector32.Section FormStateHelpButton = BitVector32.CreateSection(1, FormStateMinimizeBox);
        private static readonly BitVector32.Section FormStateStartPos = BitVector32.CreateSection(4, FormStateHelpButton);
        private static readonly BitVector32.Section FormStateWindowState = BitVector32.CreateSection(2, FormStateStartPos);
        private static readonly BitVector32.Section FormStateShowWindowOnCreate = BitVector32.CreateSection(1, FormStateWindowState);
        private static readonly BitVector32.Section FormStateAutoScaling = BitVector32.CreateSection(1, FormStateShowWindowOnCreate);
        private static readonly BitVector32.Section FormStateSetClientSize = BitVector32.CreateSection(1, FormStateAutoScaling);
        private static readonly BitVector32.Section FormStateTopMost = BitVector32.CreateSection(1, FormStateSetClientSize);
        private static readonly BitVector32.Section FormStateSWCalled = BitVector32.CreateSection(1, FormStateTopMost);
        private static readonly BitVector32.Section FormStateMdiChildMax = BitVector32.CreateSection(1, FormStateSWCalled);
        private static readonly BitVector32.Section FormStateRenderSizeGrip = BitVector32.CreateSection(1, FormStateMdiChildMax);
        private static readonly BitVector32.Section FormStateSizeGripStyle = BitVector32.CreateSection(2, FormStateRenderSizeGrip);
        private static readonly BitVector32.Section FormStateIsRestrictedWindow = BitVector32.CreateSection(1, FormStateSizeGripStyle);
        private static readonly BitVector32.Section FormStateIsRestrictedWindowChecked = BitVector32.CreateSection(1, FormStateIsRestrictedWindow);
        private static readonly BitVector32.Section FormStateIsWindowActivated = BitVector32.CreateSection(1, FormStateIsRestrictedWindowChecked);
        private static readonly BitVector32.Section FormStateIsTextEmpty = BitVector32.CreateSection(1, FormStateIsWindowActivated);
        private static readonly BitVector32.Section FormStateIsActive = BitVector32.CreateSection(1, FormStateIsTextEmpty);
        private static readonly BitVector32.Section FormStateIconSet = BitVector32.CreateSection(1, FormStateIsActive);

#if SECURITY_DIALOG
        private static readonly BitVector32.Section FormStateAddedSecurityMenuItem       = BitVector32.CreateSection(1, FormStateIconSet);
#endif

        //
        // The following flags should be used with formStateEx[...] not formState[..]
        //
        private static readonly BitVector32.Section FormStateExCalledClosing = BitVector32.CreateSection(1);
        private static readonly BitVector32.Section FormStateExUpdateMenuHandlesSuspendCount = BitVector32.CreateSection(8, FormStateExCalledClosing);
        private static readonly BitVector32.Section FormStateExUpdateMenuHandlesDeferred = BitVector32.CreateSection(1, FormStateExUpdateMenuHandlesSuspendCount);
        private static readonly BitVector32.Section FormStateExUseMdiChildProc = BitVector32.CreateSection(1, FormStateExUpdateMenuHandlesDeferred);
        private static readonly BitVector32.Section FormStateExCalledOnLoad = BitVector32.CreateSection(1, FormStateExUseMdiChildProc);
        private static readonly BitVector32.Section FormStateExCalledMakeVisible = BitVector32.CreateSection(1, FormStateExCalledOnLoad);
        private static readonly BitVector32.Section FormStateExCalledCreateControl = BitVector32.CreateSection(1, FormStateExCalledMakeVisible);
        private static readonly BitVector32.Section FormStateExAutoSize = BitVector32.CreateSection(1, FormStateExCalledCreateControl);
        private static readonly BitVector32.Section FormStateExInUpdateMdiControlStrip = BitVector32.CreateSection(1, FormStateExAutoSize);
        private static readonly BitVector32.Section FormStateExShowIcon = BitVector32.CreateSection(1, FormStateExInUpdateMdiControlStrip);
        private static readonly BitVector32.Section FormStateExMnemonicProcessed = BitVector32.CreateSection(1, FormStateExShowIcon);
        private static readonly BitVector32.Section FormStateExInScale = BitVector32.CreateSection(1, FormStateExMnemonicProcessed);
        private static readonly BitVector32.Section FormStateExInModalSizingLoop = BitVector32.CreateSection(1, FormStateExInScale);
        private static readonly BitVector32.Section FormStateExSettingAutoScale = BitVector32.CreateSection(1, FormStateExInModalSizingLoop);
        private static readonly BitVector32.Section FormStateExWindowBoundsWidthIsClientSize = BitVector32.CreateSection(1, FormStateExSettingAutoScale);
        private static readonly BitVector32.Section FormStateExWindowBoundsHeightIsClientSize = BitVector32.CreateSection(1, FormStateExWindowBoundsWidthIsClientSize);
        private static readonly BitVector32.Section FormStateExWindowClosing = BitVector32.CreateSection(1, FormStateExWindowBoundsHeightIsClientSize);

        private const int SizeGripSize = 16;
        private static object internalSyncObject = new object();

        // Property store keys for properties.  The property store allocates most efficiently
        // in groups of four, so we try to lump properties in groups of four based on how
        // likely they are going to be used in a group.
        //
        private static readonly int PropAcceptButton = PropertyStore.CreateKey();
        private static readonly int PropCancelButton = PropertyStore.CreateKey();
        private static readonly int PropDefaultButton = PropertyStore.CreateKey();
        private static readonly int PropDialogOwner = PropertyStore.CreateKey();

        private static readonly int PropMainMenu = PropertyStore.CreateKey();
        private static readonly int PropDummyMenu = PropertyStore.CreateKey();
        private static readonly int PropCurMenu = PropertyStore.CreateKey();
        private static readonly int PropMergedMenu = PropertyStore.CreateKey();

        private static readonly int PropOwner = PropertyStore.CreateKey();
        private static readonly int PropOwnedForms = PropertyStore.CreateKey();
        private static readonly int PropMaximizedBounds = PropertyStore.CreateKey();
        private static readonly int PropOwnedFormsCount = PropertyStore.CreateKey();

        private static readonly int PropMinTrackSizeWidth = PropertyStore.CreateKey();
        private static readonly int PropMinTrackSizeHeight = PropertyStore.CreateKey();
        private static readonly int PropMaxTrackSizeWidth = PropertyStore.CreateKey();
        private static readonly int PropMaxTrackSizeHeight = PropertyStore.CreateKey();

        private static readonly int PropFormMdiParent = PropertyStore.CreateKey();
        private static readonly int PropActiveMdiChild = PropertyStore.CreateKey();
        private static readonly int PropFormerlyActiveMdiChild = PropertyStore.CreateKey();
        private static readonly int PropMdiChildFocusable = PropertyStore.CreateKey();

        private static readonly int PropMainMenuStrip = PropertyStore.CreateKey();
        private static readonly int PropMdiWindowListStrip = PropertyStore.CreateKey();
        private static readonly int PropMdiControlStrip = PropertyStore.CreateKey();
        private static readonly int PropSecurityTip = PropertyStore.CreateKey();

        private static readonly int PropOpacity = PropertyStore.CreateKey();
        private static readonly int PropTransparencyKey = PropertyStore.CreateKey();
#if SECURITY_DIALOG
        private static readonly int PropSecuritySystemMenuItem = PropertyStore.CreateKey();
#endif

        ///////////////////////////////////////////////////////////////////////
        // Form per instance members
        //
        // Note: Do not add anything to this list unless absolutely neccessary.
        //
        // Begin Members {

        // List of properties that are generally set, so we keep them directly on
        // Form.
        //

        private BitVector32 formState = new BitVector32(0x21338);   // magic value... all the defaults... see the ctor for details...
        private BitVector32 formStateEx = new BitVector32();

        private Size autoScaleBaseSize = System.Drawing.Size.Empty;
        private Size minAutoSize = Size.Empty;
        private Rectangle restoredWindowBounds = new Rectangle(-1, -1, -1, -1);
        private BoundsSpecified restoredWindowBoundsSpecified;
        private DialogResult dialogResult;
        private MdiClient ctlClient;
        //private NativeWindow ownerWindow;
        private string userWindowText; // Used to cache user's text in semi-trust since the window text is added security info.
        private string securityZone;
        private string securitySite;
        private bool rightToLeftLayout = false;


        //Whidbey RestoreBounds ...
        private Rectangle restoreBounds = new Rectangle(-1, -1, -1, -1);
        private CloseReason closeReason = CloseReason.None;

        //private VisualStyleRenderer sizeGripRenderer;

        public Form()
        : base()
        {

            // we must setup the formState *before* calling Control's ctor... so we do that
            // at the member variable... that magic number is generated by switching
            // the line below to "true" and running a form.
            //
            // keep the "init" and "assert" sections always in sync!
            //

            // assert section...
            //
            Debug.Assert(formState[FormStateAllowTransparency] == 0, "Failed to set formState[FormStateAllowTransparency]");
            Debug.Assert(formState[FormStateBorderStyle] == (int)FormBorderStyle.Sizable, "Failed to set formState[FormStateBorderStyle]");
            Debug.Assert(formState[FormStateTaskBar] == 1, "Failed to set formState[FormStateTaskBar]");
            Debug.Assert(formState[FormStateControlBox] == 1, "Failed to set formState[FormStateControlBox]");
            Debug.Assert(formState[FormStateKeyPreview] == 0, "Failed to set formState[FormStateKeyPreview]");
            Debug.Assert(formState[FormStateLayered] == 0, "Failed to set formState[FormStateLayered]");
            Debug.Assert(formState[FormStateMaximizeBox] == 1, "Failed to set formState[FormStateMaximizeBox]");
            Debug.Assert(formState[FormStateMinimizeBox] == 1, "Failed to set formState[FormStateMinimizeBox]");
            Debug.Assert(formState[FormStateHelpButton] == 0, "Failed to set formState[FormStateHelpButton]");
            Debug.Assert(formState[FormStateStartPos] == (int)FormStartPosition.WindowsDefaultLocation, "Failed to set formState[FormStateStartPos]");
            Debug.Assert(formState[FormStateWindowState] == (int)FormWindowState.Normal, "Failed to set formState[FormStateWindowState]");
            Debug.Assert(formState[FormStateShowWindowOnCreate] == 0, "Failed to set formState[FormStateShowWindowOnCreate]");
            Debug.Assert(formState[FormStateAutoScaling] == 1, "Failed to set formState[FormStateAutoScaling]");
            Debug.Assert(formState[FormStateSetClientSize] == 0, "Failed to set formState[FormStateSetClientSize]");
            Debug.Assert(formState[FormStateTopMost] == 0, "Failed to set formState[FormStateTopMost]");
            Debug.Assert(formState[FormStateSWCalled] == 0, "Failed to set formState[FormStateSWCalled]");
            Debug.Assert(formState[FormStateMdiChildMax] == 0, "Failed to set formState[FormStateMdiChildMax]");
            Debug.Assert(formState[FormStateRenderSizeGrip] == 0, "Failed to set formState[FormStateRenderSizeGrip]");
            Debug.Assert(formState[FormStateSizeGripStyle] == 0, "Failed to set formState[FormStateSizeGripStyle]");
            // can't check these... Control::.ctor may force the check
            // of security... you can only assert these are 0 when running
            // under full trust...
            //
            //Debug.Assert(formState[FormStateIsRestrictedWindow]          == 0, "Failed to set formState[FormStateIsRestrictedWindow]");
            //Debug.Assert(formState[FormStateIsRestrictedWindowChecked]   == 0, "Failed to set formState[FormStateIsRestrictedWindowChecked]");
            Debug.Assert(formState[FormStateIsWindowActivated] == 0, "Failed to set formState[FormStateIsWindowActivated]");
            Debug.Assert(formState[FormStateIsTextEmpty] == 0, "Failed to set formState[FormStateIsTextEmpty]");
            Debug.Assert(formState[FormStateIsActive] == 0, "Failed to set formState[FormStateIsActive]");
            Debug.Assert(formState[FormStateIconSet] == 0, "Failed to set formState[FormStateIconSet]");

            // SECURITY NOTE: The IsRestrictedWindow check is done once and cached. We force it to happen here
            // since we want to ensure the check is done on the code that constructs the form.
            bool temp = IsRestrictedWindow;

            formStateEx[FormStateExShowIcon] = 1;

            SetState(STATE_VISIBLE, false);
            SetState(STATE_TOPLEVEL, true);

#if EVERETTROLLBACK
            // VSWhidbey# 393617 (MDI: Roll back feature to Everett + QFE source base).  Code left here for ref.
            // VSWhidbey# 357405 Enabling this code introduces a breaking change that has was approved. 
            // If this needs to be enabled, also CanTabStop and TabStop code needs to be added back in Control.cs
            // and Form.cs.  Look at RADBU CL#963988 for ref.
            
            // VSWhidbey 93518, 93544, 93547, 93563, and 93568: Set this value to false
            // so that the window style will not include the WS_TABSTOP bit, which is
            // identical to WS_MAXIMIZEBOX. Otherwise, our test suite won't be able to
            // determine whether or not the window utilizes the Maximize Box in the
            // window caption.
            SetState(STATE_TABSTOP, false);
#endif
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsRestrictedWindow
        {
            get
            {
                if (formState[FormStateIsRestrictedWindowChecked] == 0)
                {
                    formState[FormStateIsRestrictedWindow] = 0;
                    Debug.Indent();

                    try
                    {
                        IntSecurity.WindowAdornmentModification.Demand();
                    }
                    catch (SecurityException)
                    {
                        formState[FormStateIsRestrictedWindow] = 1;
                    }
                    catch
                    {
                        formState[FormStateIsRestrictedWindow] = 1; // To be on the safe side
                        formState[FormStateIsRestrictedWindowChecked] = 1;
                        throw;
                    }
                    Debug.Unindent();
                    formState[FormStateIsRestrictedWindowChecked] = 1;
                }

                return formState[FormStateIsRestrictedWindow] != 0;
            }
        }

        [DefaultValue(false)]
        public bool KeyPreview
        {
            get
            {
                return formState[FormStateKeyPreview] != 0;
            }
            set
            {
                if (value)
                {
                    formState[FormStateKeyPreview] = 1;
                }
                else
                {
                    formState[FormStateKeyPreview] = 0;
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool Modal
        {
            get
            {
                return GetState(STATE_MODAL);
            }
        }

        [DefaultValue(1.0)]
        public double Opacity
        {
            get
            {
                object opacity = Properties.GetObject(PropOpacity);
                if (opacity != null)
                {
                    return Convert.ToDouble(opacity, CultureInfo.InvariantCulture);
                }
                else
                {
                    return 1.0f;
                }
            }
            set
            {
                // In restricted mode a form cannot be made less visible than 50% opacity.
                if (IsRestrictedWindow)
                {
                    value = Math.Max(value, .50f);
                }

                if (value > 1.0)
                {
                    value = 1.0f;
                }
                else if (value < 0.0)
                {
                    value = 0.0f;
                }

                Properties.SetObject(PropOpacity, value);

                bool oldLayered = (formState[FormStateLayered] != 0);

                if (OpacityAsByte < 255 && OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
                {
                    AllowTransparency = true;
                    if (formState[FormStateLayered] != 1)
                    {
                        formState[FormStateLayered] = 1;
                        if (!oldLayered)
                        {
                            UpdateStyles();
                        }
                    }
                }
                else
                {
                    formState[FormStateLayered] = (this.TransparencyKey != Color.Empty) ? 1 : 0;
                    if (oldLayered != (formState[FormStateLayered] != 0))
                    {
                        //int exStyle = unchecked((int)(long)UnsafeNativeMethods.GetWindowLong(new HandleRef(this, Handle), NativeMethods.GWL_EXSTYLE));
                        //CreateParams cp = CreateParams;
                        //if (exStyle != cp.ExStyle)
                        //{
                        //    UnsafeNativeMethods.SetWindowLong(new HandleRef(this, Handle), NativeMethods.GWL_EXSTYLE, new HandleRef(null, (IntPtr)cp.ExStyle));
                        //}
                    }
                }

                UpdateLayered();
            }
        }

        private byte OpacityAsByte
        {
            get
            {
                return (byte)(Opacity * 255.0f);
            }
        }

        public Color TransparencyKey
        {
            get
            {
                object key = Properties.GetObject(PropTransparencyKey);
                if (key != null)
                {
                    return (Color)key;
                }
                return Color.Empty;
            }
            set
            {
                Properties.SetObject(PropTransparencyKey, value);
                if (!IsMdiContainer)
                {
                    bool oldLayered = (formState[FormStateLayered] == 1);
                    if (value != Color.Empty)
                    {
                        IntSecurity.TransparentWindows.Demand();
                        AllowTransparency = true;
                        formState[FormStateLayered] = 1;
                    }
                    else
                    {
                        formState[FormStateLayered] = (this.OpacityAsByte < 255) ? 1 : 0;
                    }
                    if (oldLayered != (formState[FormStateLayered] != 0))
                    {
                        UpdateStyles();
                    }
                    UpdateLayered();
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool AllowTransparency
        {
            get
            {
                return formState[FormStateAllowTransparency] != 0;
            }
            set
            {
                if (value != (formState[FormStateAllowTransparency] != 0) &&
                    OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
                {
                    formState[FormStateAllowTransparency] = (value ? 1 : 0);

                    formState[FormStateLayered] = formState[FormStateAllowTransparency];

                    UpdateStyles();

                    if (!value)
                    {
                        if (Properties.ContainsObject(PropOpacity))
                        {
                            Properties.SetObject(PropOpacity, (object)1.0f);
                        }
                        if (Properties.ContainsObject(PropTransparencyKey))
                        {
                            Properties.SetObject(PropTransparencyKey, Color.Empty);
                        }
                        UpdateLayered();
                    }
                }
            }
        }

        [DefaultValue(false)]
        public bool IsMdiContainer
        {
            get
            {
                return ctlClient != null;
            }

            set
            {
                if (value == IsMdiContainer)
                    return;

                if (value)
                {
                    Debug.Assert(ctlClient == null, "why isn't ctlClient null");
                    AllowTransparency = false;
                    Controls.Add(new MdiClient());
                }
                else
                {
                    Debug.Assert(ctlClient != null, "why is ctlClient null");
                    ActiveMdiChildInternal = null;
                    ctlClient.Dispose();
                }
                //since we paint the background when mdi is true, we need
                //to invalidate here
                //
                Invalidate();
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool IsMdiChild
        {
            get
            {
                return (Properties.GetObject(PropFormMdiParent) != null);
            }
        }

        // Created for fix VSWhidbey 182131. Deactivates active MDI child and temporarily marks it as unfocusable,
        // so that WM_SETFOCUS sent to MDIClient does not activate that child. (See MdiClient.WndProc).
        internal bool IsMdiChildFocusable
        {
            get
            {
                if (this.Properties.ContainsObject(PropMdiChildFocusable))
                {
                    return (bool)this.Properties.GetObject(PropMdiChildFocusable);
                }
                return false;
            }
            set
            {
                if (value != this.IsMdiChildFocusable)
                {
                    this.Properties.SetObject(PropMdiChildFocusable, value);
                }
            }
        }

        internal Form ActiveMdiChildInternal
        {
            get
            {
                return (Form)Properties.GetObject(PropActiveMdiChild);
            }

            set
            {
                Properties.SetObject(PropActiveMdiChild, value);
            }
        }

        //VSWhidbey 439815: we don't repaint the mdi child that used to be active any more.  We used to do this in Activated, but no 
        //longer do because of added event Deactivate.
        private Form FormerlyActiveMdiChild
        {
            get
            {
                return (Form)Properties.GetObject(PropFormerlyActiveMdiChild);
            }

            set
            {
                Properties.SetObject(PropFormerlyActiveMdiChild, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TopLevel
        {
            get
            {
                return GetTopLevel();
            }
            set
            {
                if (!value && ((Form)this).IsMdiContainer && !DesignMode)
                {
                    throw new ArgumentException("MDIContainerMustBeTopLevel");
                }
                //SetTopLevel(value);
            }
        }

        private void UpdateLayered()
        {
            if ((formState[FormStateLayered] != 0) && IsHandleCreated && TopLevel && OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
            {
                bool result;

                Color transparencyKey = TransparencyKey;

                if (transparencyKey.IsEmpty)
                {
                    //result = UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this, Handle), 0, OpacityAsByte, NativeMethods.LWA_ALPHA);
                }
                else if (OpacityAsByte == 255)
                {
                    // Windows doesn't do so well setting colorkey and alpha, so avoid it if we can
                    //result = UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this, Handle), ColorTranslator.ToWin32(transparencyKey), 0, NativeMethods.LWA_COLORKEY);
                }
                else
                {
                    //result = UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this, Handle), ColorTranslator.ToWin32(transparencyKey),
                    //                                            OpacityAsByte, NativeMethods.LWA_ALPHA | NativeMethods.LWA_COLORKEY);
                }

                //if (!result)
                //{
                //    throw new Win32Exception();
                //}
            }
        }

        internal string text;
        public override string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (value != text)
                {
                    text = value;
                    if (layoutPerformed)
                    {
                        Page.RunScript($"$(document.getElementById('{WebviewIdentifier}')).parent().closest('.jsPanel')[0].querySelector('.jsPanel-title').innerHTML=\"{value}\"");
                    }
                }
            }
        }

        public new Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                if (value != base.Size)
                {
                    base.Size = value;
                    if (layoutPerformed)
                    {
                        Page.RunScript($"$(document.getElementById('{WebviewIdentifier}')).parent().closest('.jsPanel')[0].resize({{width:{value.Width},height:{value.Height}}});");
                    }
                }
            }
        }

        public new Point Location
        {
            get
            {
                return base.location;
            }
            set
            {
                if (value != base.location)
                {
                    base.location = value;
                    if (layoutPerformed)
                    {
                        Page.RunScript($@"$(document.getElementById('{WebviewIdentifier}')).parent().closest('.jsPanel')[0].reposition({{ left: {value.X}, top: {value.Y}}});");
                    }
                }
            }
        }

        protected override void UpdateDefaultButton()
        {
            ContainerControl cc = this;

            while (cc.ActiveControl is ContainerControl)
            {
                cc = cc.ActiveControl as ContainerControl;
                Debug.Assert(cc != null);

                if (cc is Form)
                {
                    // VSWhidbey#291004: Don't allow a parent form to get its default button from a child form,
                    // otherwise the two forms will 'compete' for the Enter key and produce unpredictable results.
                    // This is aimed primarily at fixing the behavior of MDI container forms.
                    cc = this;
                    break;
                }
            }

            if (cc.ActiveControl is IButtonControl)
            {
                SetDefaultButton((IButtonControl)cc.ActiveControl);
            }
            else
            {
                SetDefaultButton(AcceptButton);
            }
        }

        [
        DefaultValue(null),
        ]
        public IButtonControl AcceptButton
        {
            get
            {
                return (IButtonControl)Properties.GetObject(PropAcceptButton);
            }
            set
            {
                if (AcceptButton != value)
                {
                    Properties.SetObject(PropAcceptButton, value);
                    UpdateDefaultButton();

                    // this was removed as it breaks any accept button that isn't
                    // an OK, like in the case of wizards 'next' button.  it was
                    // added as a fix to 47209...which has been reactivated.
                    /*
                    if (acceptButton != null && acceptButton.DialogResult == DialogResult.None) {
                        acceptButton.DialogResult = DialogResult.OK;
                    }
                    */
                }
            }
        }

        private void SetDefaultButton(IButtonControl button)
        {
            IButtonControl defaultButton = (IButtonControl)Properties.GetObject(PropDefaultButton);

            if (defaultButton != button)
            {
                if (defaultButton != null) defaultButton.NotifyDefault(false);
                Properties.SetObject(PropDefaultButton, button);
                if (button != null) button.NotifyDefault(true);
            }
        }

        internal Form OwnerInternal
        {
            get
            {
                return (Form)Properties.GetObject(PropOwner);
            }
        }

        [DefaultValue(true)        ]
        public bool MinimizeBox
        {
            get
            {
                return formState[FormStateMinimizeBox] != 0;
            }
            set
            {
                if (value)
                {
                    formState[FormStateMinimizeBox] = 1;
                }
                else
                {
                    formState[FormStateMinimizeBox] = 0;
                }
                //UpdateFormStyles();
            }
        }

        [DefaultValue(true)]
        public bool MaximizeBox
        {
            get
            {
                return formState[FormStateMaximizeBox] != 0;
            }
            set
            {
                if (value)
                {
                    formState[FormStateMaximizeBox] = 1;
                }
                else
                {
                    formState[FormStateMaximizeBox] = 0;
                }
                //UpdateFormStyles();
            }
        }

        public FormBorderStyle FormBorderStyle = FormBorderStyle.Sizable;

        public int FakeHandle;

        public override void PerformLayout()
        {
            create();
            PerformChildLayout();
            layoutPerformed = true;
        }

        private async void webView2_WebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            string received = args.TryGetWebMessageAsString();
            var eventReturn = JsonHelper.Deserialize<EventReturn>(received);

            Control child = FindControlById(this, eventReturn.identifier);

            if (child != null)
            {
                switch (eventReturn.eventName)
                {
                    case "Click":
                        child.OnClick(EventArgs.Empty);
                        break;
                    case "TextChanged":
                        if (child.GetType().IsSubclassOf(typeof(TextBox)) || child.GetType() == typeof(TextBox))
                        {
                            TextBox textBox = (TextBox)child;
                            textBox.Text = await Page.Get(textBox.WebviewIdentifier, textBox.Multiline ? "innerHTML" : "value");
                        }
                        if (child.GetType().IsSubclassOf(typeof(Label)) || child.GetType() == typeof(Label))
                        {
                            Label label = (Label)child;
                            label.Text = await Page.Get(label.WebviewIdentifier, "innerHTML");
                        }
                        if (child.GetType().IsSubclassOf(typeof(Button)) || child.GetType() == typeof(Button))
                        {
                            Button button = (Button)child;
                            button.Text = await Page.GetFromScript($"document.getElementById(\"{WebviewIdentifier}\").getElementsByTagName('p')[0].innerHTML");
                        }
                        else
                        {
                            // In other cases the event is already handled by .Text change
                            child.OnTextChanged(EventArgs.Empty);
                        }
                        break;
                    case "Resize":
                    case "ResizeBegin":
                    case "ResizeEnd":
                        if (child.GetType().IsSubclassOf(typeof(Form)) || child.GetType() == typeof(Form))
                        {
                            Form form = (Form)child;
                            string wstr = (await Page.RunScript($"$(document.getElementById('{WebviewIdentifier}')).parent().closest('.jsPanel')[0].style.width")).Replace("px", "").Replace("\"", "");
                            string hstr = (await Page.RunScript($"$(document.getElementById('{WebviewIdentifier}')).parent().closest('.jsPanel')[0].style.height")).Replace("px", "").Replace("\"", "");
                            int w = (int)double.Parse(wstr, CultureInfo.InvariantCulture);
                            int h = (int)double.Parse(hstr, CultureInfo.InvariantCulture);
                            Size = new Size(w, h);
                            if (eventReturn.eventName == "ResizeBegin")
                            {
                                form.OnResizeBegin(EventArgs.Empty);
                            }
                            else if (eventReturn.eventName == "ResizeEnd")
                            {
                                form.OnResizeEnd(EventArgs.Empty);
                            }
                            else
                            {
                                form.OnResize(EventArgs.Empty);
                            }
                        }
                        break;
                    case "Move":
                    case "MoveBegin":
                    case "MoveEnd":
                        if (child.GetType().IsSubclassOf(typeof(Form)) || child.GetType() == typeof(Form))
                        {
                            Form form = (Form)child;
                            string xstr = (await Page.RunScript($"$(document.getElementById('{WebviewIdentifier}')).parent().closest('.jsPanel')[0].style.left")).Replace("px", "").Replace("\"", "");
                            string ystr = (await Page.RunScript($"$(document.getElementById('{WebviewIdentifier}')).parent().closest('.jsPanel')[0].style.top")).Replace("px", "").Replace("\"", "");
                            int x = (int)double.Parse(xstr, CultureInfo.InvariantCulture);
                            int y = (int)double.Parse(ystr, CultureInfo.InvariantCulture);
                            location = new Point(x, y);
                            form.OnLocationChanged(EventArgs.Empty);
                        }
                        break;
                    case "MouseEnter":
                        child.OnMouseEnter(EventArgs.Empty);
                        break;
                    case "MouseLeave":
                        child.OnMouseLeave(EventArgs.Empty);
                        break;
                    case "MouseClick":
                        child.OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)); // dummy, data has to be retrived from html
                        break;
                    case "MouseDown":
                        child.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)); // dummy, data has to be retrived from html
                        break;
                    case "MouseUp":
                        child.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)); // dummy, data has to be retrived from html
                        break;
                }
            }
        }

        internal override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (formState[FormStateRenderSizeGrip] != 0)
            {
                Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnResizeBegin(EventArgs e)
        {
            if (CanRaiseEvents)
            {
                ((EventHandler)base.Events[EVENT_RESIZEBEGIN])?.Invoke(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnResizeEnd(EventArgs e)
        {
            if (CanRaiseEvents)
            {
                ((EventHandler)base.Events[EVENT_RESIZEEND])?.Invoke(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnClosing(CancelEventArgs e)
        {
            CancelEventHandler handler = (CancelEventHandler)Events[EVENT_CLOSING];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnClosed(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[EVENT_CLOSED];
            if (handler != null) handler(this, e);
        }


        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnFormClosing(FormClosingEventArgs e)
        {
            FormClosingEventHandler handler = (FormClosingEventHandler)Events[EVENT_FORMCLOSING];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnFormClosed(FormClosedEventArgs e)
        {
            //Remove the form from Application.OpenForms (nothing happens if isn't present)
            Application.OpenFormsInternalRemove(this);

            FormClosedEventHandler handler = (FormClosedEventHandler)Events[EVENT_FORMCLOSED];
            if (handler != null) handler(this, e);
        }

        public event EventHandler ResizeBegin
        {
            add
            {
                base.Events.AddHandler(EVENT_RESIZEBEGIN, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RESIZEBEGIN, value);
            }
        }

        public event EventHandler ResizeEnd
        {
            add
            {
                base.Events.AddHandler(EVENT_RESIZEEND, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RESIZEEND, value);
            }
        }

        public event EventHandler Load
        {
            add
            {
                Events.AddHandler(EVENT_LOAD, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_LOAD, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Form[] OwnedForms
        {
            get
            {
                Form[] ownedForms = (Form[])Properties.GetObject(PropOwnedForms);
                int ownedFormsCount = Properties.GetInteger(PropOwnedFormsCount);

                Form[] result = new Form[ownedFormsCount];
                if (ownedFormsCount > 0)
                {
                    Array.Copy(ownedForms, 0, result, 0, ownedFormsCount);
                }

                return result;
            }
        }

        [Localizable(true), DefaultValue(FormStartPosition.WindowsDefaultLocation)]
        public FormStartPosition StartPosition
        {
            get
            {
                return (FormStartPosition)formState[FormStateStartPos];
            }
            set
            {
                //valid values are 0x0 to 0x4
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)FormStartPosition.Manual, (int)FormStartPosition.CenterParent))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(FormStartPosition));
                }
                formState[FormStateStartPos] = (int)value;
            }
        }

        internal bool RaiseFormClosingOnAppExit()
        {
            FormClosingEventArgs e = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);
            // e.Cancel = !Validate(true);    This would cause a breaking change between v2.0 and v1.0/v1.1 in case validation fails.
            if (!Modal)
            {
                /* This is not required because Application.ExitPrivate() loops through all forms in the Application.OpenForms collection
                // Fire FormClosing event on all MDI children
                if (IsMdiContainer) {
                    FormClosingEventArgs fce = new FormClosingEventArgs(CloseReason.MdiFormClosing, e.Cancel);
                    foreach(Form mdiChild in MdiChildren) {
                        if (mdiChild.IsHandleCreated) {
                            mdiChild.OnFormClosing(fce);
                            if (fce.Cancel) {
                                e.Cancel = true;
                                break;
                            }
                        }
                    }
                }
                */

                // Fire FormClosing event on all the forms that this form owns and are not in the Application.OpenForms collection
                // This is to be consistent with what WmClose does.
                int ownedFormsCount = Properties.GetInteger(PropOwnedFormsCount);
                if (ownedFormsCount > 0)
                {
                    Form[] ownedForms = this.OwnedForms;
                    FormClosingEventArgs fce = new FormClosingEventArgs(CloseReason.FormOwnerClosing, false);
                    for (int i = ownedFormsCount - 1; i >= 0; i--)
                    {
                        if (ownedForms[i] != null && !Application.OpenFormsInternal.Contains(ownedForms[i]))
                        {
                            ownedForms[i].OnFormClosing(fce);
                            if (fce.Cancel)
                            {
                                e.Cancel = true;
                                break;
                            }
                        }
                    }
                }
            }
            OnFormClosing(e);
            return e.Cancel;
        }

        async void create()
        {
            FakeHandle = new Random().Next(1, 65000);
            WebviewIdentifier = FakeHandle + "-" + Name;
            string style = "";

            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"background-color: {System.Drawing.ColorTranslator.ToHtml(BackColor)};";
            style += $"background-repeat: repeat;";
            style += $"min-height: {ClientSize.Height}px;";
            style += $"min-width: {ClientSize.Width}px;";
            style += $"height: 100%;";
            style += $"width: 100%;";
            style += $"overflow: hidden;";

            string htmlContent = $"<div id=\"{WebviewIdentifier}\" class=\"{this.Name}\" style=\"{style}\"></div>";

            JsonProperties.Properties properties = new JsonProperties.Properties
            {
                id = FakeHandle.ToString(),
                identifier = FakeHandle.ToString() + "-" + Name.ToString(),
                icon = "",
                maximize = MaximizeBox,
                minimize = MinimizeBox,
                mdi = false,
                position = new JsonProperties.Position
                {
                    at = "left-top",
                    my = "left-top"
                },
                resizable = FormBorderStyle == FormBorderStyle.Sizable || FormBorderStyle == FormBorderStyle.SizableToolWindow,
                size = new JsonProperties.Size
                {
                    width = $"{ClientSize.Width}px",
                    height = $"{ClientSize.Height}px"
                },
                title = Text
            };
            string propertiesSerialized = JsonHelper.Serialize(properties);
            string script = $"launchPostWindowSuccess(`{htmlContent}`,`{propertiesSerialized}`)";
            await Page.RunScript(script);
            Page.pContainer.WebMessageReceived += webView2_WebMessageReceived;
        }

        // Static recursive method to find the Control with the specified id
        internal Control FindControlById(Control startControl, string id)
        {
            // Check if the startControl has the matching id
            if (startControl.WebviewIdentifier == id)
            {
                return startControl;
            }

            // If not, recursively search through the nested controls using a classic for loop
            for (int i = 0; i < startControl.Controls.Count; i++)
            {
                var result = FindControlById(startControl.Controls[i], id);
                if (result != null)
                {
                    return result;
                }
            }

            // If no control is found with the specified id, return null
            return null;
        }
    }
}
