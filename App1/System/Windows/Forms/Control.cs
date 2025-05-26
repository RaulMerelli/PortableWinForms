using App1;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.Layout;
using System.Xml.Linq;

namespace System.Windows.Forms
{
    public class Control : Component, IArrangedElement
    {
        internal const int STATE_CREATED = 0x00000001;
        internal const int STATE_VISIBLE = 0x00000002;
        internal const int STATE_ENABLED = 0x00000004;
        internal const int STATE_TABSTOP = 0x00000008;
        internal const int STATE_RECREATE = 0x00000010;
        internal const int STATE_MODAL = 0x00000020;
        internal const int STATE_ALLOWDROP = 0x00000040;
        internal const int STATE_DROPTARGET = 0x00000080;
        internal const int STATE_NOZORDER = 0x00000100;
        internal const int STATE_LAYOUTDEFERRED = 0x00000200;
        internal const int STATE_USEWAITCURSOR = 0x00000400;
        internal const int STATE_DISPOSED = 0x00000800;
        internal const int STATE_DISPOSING = 0x00001000;
        internal const int STATE_MOUSEENTERPENDING = 0x00002000;
        internal const int STATE_TRACKINGMOUSEEVENT = 0x00004000;
        internal const int STATE_THREADMARSHALLPENDING = 0x00008000;
        internal const int STATE_SIZELOCKEDBYOS = 0x00010000;
        internal const int STATE_CAUSESVALIDATION = 0x00020000;
        internal const int STATE_CREATINGHANDLE = 0x00040000;
        internal const int STATE_TOPLEVEL = 0x00080000;
        internal const int STATE_ISACCESSIBLE = 0x00100000;
        internal const int STATE_OWNCTLBRUSH = 0x00200000;
        internal const int STATE_EXCEPTIONWHILEPAINTING = 0x00400000;
        internal const int STATE_LAYOUTISDIRTY = 0x00800000;
        internal const int STATE_CHECKEDHOST = 0x01000000;
        internal const int STATE_HOSTEDINDIALOG = 0x02000000;
        internal const int STATE_DOUBLECLICKFIRED = 0x04000000;
        internal const int STATE_MOUSEPRESSED = 0x08000000;
        internal const int STATE_VALIDATIONCANCELLED = 0x10000000;
        internal const int STATE_PARENTRECREATING = 0x20000000;
        internal const int STATE_MIRRORED = 0x40000000;

        // HACK HACK HACK - when we change RightToLeft, we need to change the scrollbar thumb.
        // We can't do that until after the control has been created, and all the items added
        // back. This is because the system control won't know the nMin and nMax of the scroll
        // bar until the items are added. So in RightToLeftChanged, we set a flag that indicates
        // that we want to set the scroll position. In OnHandleCreated we check this flag,
        // and if set, we BeginInvoke. We have to BeginInvoke since we have to wait until the items
        // are added. We only want to do this when RightToLeft changes thus the flags 
        // STATE2_HAVEINVOKED and STATE2_SETSCROLLPOS. Otherwise we would do this on each HandleCreated.
        private const int STATE2_HAVEINVOKED = 0x00000001;
        private const int STATE2_SETSCROLLPOS = 0x00000002;
        private const int STATE2_LISTENINGTOUSERPREFERENCECHANGED = 0x00000004;   // set when the control is listening to SystemEvents.UserPreferenceChanged.
        internal const int STATE2_INTERESTEDINUSERPREFERENCECHANGED = 0x00000008;   // if set, the control will listen to SystemEvents.UserPreferenceChanged when TopLevel is true and handle is created.
        internal const int STATE2_MAINTAINSOWNCAPTUREMODE = 0x00000010;   // if set, the control DOES NOT necessarily take capture on MouseDown
        private const int STATE2_BECOMINGACTIVECONTROL = 0x00000020;   // set to true by ContainerControl when this control is becoming its active control

        private const int STATE2_CLEARLAYOUTARGS = 0x00000040;   // if set, the next time PerformLayout is called, cachedLayoutEventArg will be cleared.
        private const int STATE2_INPUTKEY = 0x00000080;
        private const int STATE2_INPUTCHAR = 0x00000100;
        private const int STATE2_UICUES = 0x00000200;
        private const int STATE2_ISACTIVEX = 0x00000400;
        internal const int STATE2_USEPREFERREDSIZECACHE = 0x00000800;
        internal const int STATE2_TOPMDIWINDOWCLOSING = 0x00001000;
        internal const int STATE2_CURRENTLYBEINGSCALED = 0x00002000;   // if set, the control is being scaled, currently

        private static readonly object EventAutoSizeChanged = new object();
        private static readonly object EventKeyDown = new object();
        private static readonly object EventKeyPress = new object();
        private static readonly object EventKeyUp = new object();
        private static readonly object EventMouseDown = new object();
        private static readonly object EventMouseEnter = new object();
        private static readonly object EventMouseLeave = new object();
        private static readonly object EventDpiChangedBeforeParent = new object();
        private static readonly object EventDpiChangedAfterParent = new object();
        private static readonly object EventMouseHover = new object();
        private static readonly object EventMouseMove = new object();
        private static readonly object EventMouseUp = new object();
        private static readonly object EventMouseWheel = new object();
        private static readonly object EventClick = new object();
        private static readonly object EventClientSize = new object();
        private static readonly object EventDoubleClick = new object();
        private static readonly object EventMouseClick = new object();
        private static readonly object EventMouseDoubleClick = new object();
        private static readonly object EventMouseCaptureChanged = new object();
        private static readonly object EventMove = new object();
        private static readonly object EventResize = new object();
        private static readonly object EventLayout = new object();
        private static readonly object EventGotFocus = new object();
        private static readonly object EventLostFocus = new object();
        private static readonly object EventEnabledChanged = new object();
        private static readonly object EventEnter = new object();
        private static readonly object EventLeave = new object();
        private static readonly object EventHandleCreated = new object();
        private static readonly object EventHandleDestroyed = new object();
        private static readonly object EventVisibleChanged = new object();
        private static readonly object EventControlAdded = new object();
        private static readonly object EventControlRemoved = new object();
        private static readonly object EventChangeUICues = new object();
        private static readonly object EventSystemColorsChanged = new object();
        private static readonly object EventValidating = new object();
        private static readonly object EventValidated = new object();
        private static readonly object EventStyleChanged = new object();
        private static readonly object EventImeModeChanged = new object();
        private static readonly object EventHelpRequested = new object();
        private static readonly object EventPaint = new object();
        private static readonly object EventInvalidated = new object();
        private static readonly object EventQueryContinueDrag = new object();
        private static readonly object EventGiveFeedback = new object();
        private static readonly object EventDragEnter = new object();
        private static readonly object EventDragLeave = new object();
        private static readonly object EventDragOver = new object();
        private static readonly object EventDragDrop = new object();
        private static readonly object EventQueryAccessibilityHelp = new object();
        private static readonly object EventBackgroundImage = new object();
        private static readonly object EventBackgroundImageLayout = new object();
        private static readonly object EventBindingContext = new object();
        private static readonly object EventBackColor = new object();
        private static readonly object EventParent = new object();
        private static readonly object EventVisible = new object();
        private static readonly object EventText = new object();
        private static readonly object EventTabStop = new object();
        private static readonly object EventTabIndex = new object();
        private static readonly object EventSize = new object();
        private static readonly object EventRightToLeft = new object();
        private static readonly object EventLocation = new object();
        private static readonly object EventForeColor = new object();
        private static readonly object EventFont = new object();
        private static readonly object EventEnabled = new object();
        private static readonly object EventDock = new object();
        private static readonly object EventCursor = new object();
        private static readonly object EventContextMenu = new object();
        private static readonly object EventContextMenuStrip = new object();
        private static readonly object EventCausesValidation = new object();
        private static readonly object EventRegionChanged = new object();
        private static readonly object EventMarginChanged = new object();
        internal static readonly object EventPaddingChanged = new object();
        private static readonly object EventPreviewKeyDown = new object();


#if WIN95_SUPPORT
        private static int mouseWheelMessage = NativeMethods.WM_MOUSEWHEEL;
        private static bool mouseWheelRoutingNeeded;
        private static bool mouseWheelInit;
#endif

        private static int threadCallbackMessage;

        // Initially check for illegal multithreading based on whether the
        // debugger is attached.
        [ResourceExposure(ResourceScope.Process)]
        private static bool checkForIllegalCrossThreadCalls = Debugger.IsAttached;
        private static ContextCallback invokeMarshaledCallbackHelperDelegate;

        [ThreadStatic]
        private static bool inCrossThreadSafeCall = false;

        // disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        [ThreadStatic]
        internal static HelpInfo currentHelpInfo = null;
#pragma warning restore 0414

        //private static FontHandleWrapper defaultFontHandleWrapper;

        private const short PaintLayerBackground = 1;
        private const short PaintLayerForeground = 2;

        private const byte RequiredScalingEnabledMask = 0x10;
        private const byte RequiredScalingMask = 0x0F;

        private const byte HighOrderBitMask = 0x80;

        private static Font defaultFont;

        // Property store keys for properties.  The property store allocates most efficiently
        // in groups of four, so we try to lump properties in groups of four based on how
        // likely they are going to be used in a group.
        //
        private static readonly int PropName = PropertyStore.CreateKey();
        private static readonly int PropBackBrush = PropertyStore.CreateKey();
        private static readonly int PropFontHeight = PropertyStore.CreateKey();
        private static readonly int PropCurrentAmbientFont = PropertyStore.CreateKey();

        private static readonly int PropControlsCollection = PropertyStore.CreateKey();
        private static readonly int PropBackColor = PropertyStore.CreateKey();
        private static readonly int PropForeColor = PropertyStore.CreateKey();
        private static readonly int PropFont = PropertyStore.CreateKey();

        private static readonly int PropBackgroundImage = PropertyStore.CreateKey();
        private static readonly int PropFontHandleWrapper = PropertyStore.CreateKey();
        private static readonly int PropUserData = PropertyStore.CreateKey();
        private static readonly int PropContextMenu = PropertyStore.CreateKey();

        private static readonly int PropCursor = PropertyStore.CreateKey();
        private static readonly int PropRegion = PropertyStore.CreateKey();
        private static readonly int PropRightToLeft = PropertyStore.CreateKey();

        private static readonly int PropBindings = PropertyStore.CreateKey();
        private static readonly int PropBindingManager = PropertyStore.CreateKey();
        private static readonly int PropAccessibleDefaultActionDescription = PropertyStore.CreateKey();
        private static readonly int PropAccessibleDescription = PropertyStore.CreateKey();

        private static readonly int PropAccessibility = PropertyStore.CreateKey();
        private static readonly int PropNcAccessibility = PropertyStore.CreateKey();
        private static readonly int PropAccessibleName = PropertyStore.CreateKey();
        private static readonly int PropAccessibleRole = PropertyStore.CreateKey();

        private static readonly int PropPaintingException = PropertyStore.CreateKey();
        private static readonly int PropActiveXImpl = PropertyStore.CreateKey();
        private static readonly int PropControlVersionInfo = PropertyStore.CreateKey();
        private static readonly int PropBackgroundImageLayout = PropertyStore.CreateKey();

        private static readonly int PropAccessibleHelpProvider = PropertyStore.CreateKey();
        private static readonly int PropContextMenuStrip = PropertyStore.CreateKey();
        private static readonly int PropAutoScrollOffset = PropertyStore.CreateKey();
        private static readonly int PropUseCompatibleTextRendering = PropertyStore.CreateKey();

        private static readonly int PropImeWmCharsToIgnore = PropertyStore.CreateKey();
        private static readonly int PropImeMode = PropertyStore.CreateKey();
        private static readonly int PropDisableImeModeChangedCount = PropertyStore.CreateKey();
        private static readonly int PropLastCanEnableIme = PropertyStore.CreateKey();

        private static readonly int PropCacheTextCount = PropertyStore.CreateKey();
        private static readonly int PropCacheTextField = PropertyStore.CreateKey();
        private static readonly int PropAmbientPropertiesService = PropertyStore.CreateKey();

        private static bool needToLoadComCtl = true;

        // This switch determines the default text rendering engine to use by some controls that support switching rendering engine.
        // CheckedListBox, PropertyGrid, GroupBox, Label and LinkLabel, and ButtonBase controls.
        // True means use GDI+, false means use GDI (TextRenderer).
        internal static bool UseCompatibleTextRenderingDefault = true;

        ///////////////////////////////////////////////////////////////////////
        // Control per instance members
        //
        // Note: Do not add anything to this list unless absolutely neccessary.
        //       Every control on a form has the overhead of all of these
        //       variables!
        //
        // Begin Members {

        // List of properties that are generally set, so we keep them directly on
        // control.
        //

        // Resist the temptation to make this variable 'internal' rather than
        // private. Handle access should be tightly controlled, and is in this
        // file.  Making it 'internal' makes controlling it quite difficult.
        //private ControlNativeWindow window;

        private Control parent;
        private Control reflectParent;
        //private CreateParams createParams;
        private int x;                      // CONSIDER: changing this to short
        private int y;
        private int width;
        private int height;
        private int clientWidth;
        private int clientHeight;
        private int state;                  // See STATE_ constants above
        private int state2;                 // See STATE2_ constants above
        private ControlStyles controlStyle;           // User supplied control style
        private int tabIndex;
        private string text;                   // See ControlStyles.CacheText for usage notes
        private byte layoutSuspendCount;
        private byte requiredScaling;        // bits 0-4: BoundsSpecified stored in RequiredScaling property.  Bit 5: RequiredScalingEnabled property.
        private PropertyStore propertyStore;          // Contains all properties that are not always set.
        private NativeMethods.TRACKMOUSEEVENT trackMouseEvent;
        private short updateCount;
        private LayoutEventArgs cachedLayoutEventArgs;
        //private Queue threadCallbackList;
        internal int deviceDpi;


        // for keeping track of our ui state for focus and keyboard cues.  using a member variable
        // here because we hit this a lot
        //
        private int uiCuesState;

        private const int UISTATE_FOCUS_CUES_MASK = 0x000F;
        private const int UISTATE_FOCUS_CUES_HIDDEN = 0x0001;
        private const int UISTATE_FOCUS_CUES_SHOW = 0x0002;
        private const int UISTATE_KEYBOARD_CUES_MASK = 0x00F0;
        private const int UISTATE_KEYBOARD_CUES_HIDDEN = 0x0010;
        private const int UISTATE_KEYBOARD_CUES_SHOW = 0x0020;

        [ThreadStatic]
        private static byte[] tempKeyboardStateArray;

        // } End Members
        ///////////////////////////////////////////////////////////////////////
        ///
        public bool Enabled;
        public string Name;
        public int TabIndex;
        public Control() : this(true)
        {
        }

        internal Control(bool autoInstallSyncContext) : base()
        {
            propertyStore = new PropertyStore();

            //DpiHelper.InitializeDpiHelperForWinforms();
            // Initialize DPI to the value on the primary screen, we will have the correct value when the Handle is created.
            //deviceDpi = DpiHelper.DeviceDpi;

            //window = new ControlNativeWindow(this);
            RequiredScalingEnabled = true;
            //RequiredScaling = BoundsSpecified.All;
            tabIndex = -1;

            state = STATE_VISIBLE | STATE_ENABLED | STATE_TABSTOP | STATE_CAUSESVALIDATION;
            state2 = STATE2_INTERESTEDINUSERPREFERENCECHANGED;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.StandardClick |
                     ControlStyles.StandardDoubleClick |
                     ControlStyles.UseTextForAccessibility |
                     ControlStyles.Selectable, true);

            // We baked the "default default" margin and min size into CommonProperties
            // so that in the common case the PropertyStore would be empty.  If, however,
            // someone overrides these Default* methads, we need to write the default
            // value into the PropertyStore in the ctor.
            //
            if (DefaultMargin != CommonProperties.DefaultMargin)
            {
                Margin = DefaultMargin;
            }
            if (DefaultMinimumSize != CommonProperties.DefaultMinimumSize)
            {
                MinimumSize = DefaultMinimumSize;
            }
            if (DefaultMaximumSize != CommonProperties.DefaultMaximumSize)
            {
                MaximumSize = DefaultMaximumSize;
            }

            // Compute our default size.
            //
            Size defaultSize = DefaultSize;
            width = defaultSize.Width;
            height = defaultSize.Height;

            // DefaultSize may have hit GetPreferredSize causing a PreferredSize to be cached.  The
            // PreferredSize may change as a result of the current size.  Since a  SetBoundsCore did
            // not happen, so we need to clear the preferredSize cache manually.
            CommonProperties.xClearPreferredSizeCache(this);

            if (width != 0 && height != 0)
            {
                NativeMethods.RECT rect = new NativeMethods.RECT();
                rect.left = rect.right = rect.top = rect.bottom = 0;

                //CreateParams cp = CreateParams;

                //AdjustWindowRectEx(ref rect, cp.Style, false, cp.ExStyle);
                clientWidth = width - (rect.right - rect.left);
                clientHeight = height - (rect.bottom - rect.top);
            }


            // Set up for async operations on this thread.
            if (autoInstallSyncContext)
            {
                WindowsFormsSynchronizationContext.InstallIfNeeded();
            }
        }

        public Control(string text) : this((Control)null, text)
        {
        }

        public Control(string text, int left, int top, int width, int height) :
                    this((Control)null, text, left, top, width, height)
        {
        }

        public Control(Control parent, string text) : this()
        {
            this.Parent = parent;
            this.Text = text;
        }

        public Control(Control parent, string text, int left, int top, int width, int height) : this(parent, text)
        {
            this.Location = new Point(left, top);
            this.Size = new Size(width, height);
        }

        [Localizable(true)]
        public Padding Margin
        {
            get { return CommonProperties.GetMargin(this); }
            set
            {
                // This should be done here rather than in the property store as
                // some IArrangedElements actually support negative padding.
                value = LayoutUtils.ClampNegativePaddingToZero(value);

                // SetMargin causes a layout as a side effect.
                if (value != Margin)
                {
                    CommonProperties.SetMargin(this, value);
                    OnMarginChanged(EventArgs.Empty);
                }
                Debug.Assert(Margin == value, "Error detected while setting Margin.");
            }
        }


        //public Object Invoke(Delegate method, params Object[] args)
        //{
        //    using (new MultithreadSafeCallScope())
        //    {
        //        Control marshaler = FindMarshalingControl();
        //        return marshaler.MarshaledInvoke(this, method, args, true);
        //    }
        //}

        private Control FindMarshalingControl()
        {
            lock (this)
            {
                Control c = this;

                while (c != null && !c.IsHandleCreated)
                {
                    Control p = c.ParentInternal;
                    c = p;
                }

                if (c == null)
                {
                    // No control with a created handle.  We
                    // just use our own control.  MarshaledInvoke
                    // will throw an exception because there
                    // is no handle.
                    //
                    c = this;
                }
                else
                {
                    Debug.Assert(c.IsHandleCreated, "FindMarshalingControl chose a bad control.");
                }

                return (Control)c;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ]
        public ControlCollection Controls
        {
            get
            {
                ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);

                if (controlsCollection == null)
                {
                    controlsCollection = CreateControlsInstance();
                    Properties.SetObject(PropControlsCollection, controlsCollection);
                }
                return controlsCollection;
            }
        }

        internal List<string[]> preLayoutScript = new List<string[]>();

        internal string preLayoutScriptString
        {
            get
            {
                string result = "";
                for (int i = 0; i < preLayoutScript.Count; i++)
                {
                    if (preLayoutScript[i].Length == 3)
                    {
                        preLayoutScript[i][1] = WebviewIdentifier;
                        result += string.Join("", preLayoutScript[i]);
                    }
                }
                return result;
            }
        }

        internal void AddJsEvent(object key)
        {
            string[] script = { };
            if (key == EventClick)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').addEventListener('click', clickEvent)" };
            }
            if (key == EventDoubleClick)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').addEventListener('dblclick', doubleClickEvent)" };
            }
            if (key == EventMouseClick)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').addEventListener('click', mouseClickEvent)" };
            }
            if (key == EventMouseDoubleClick)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').addEventListener('dblclick', mouseDoubleClickEvent)" };
            }
            if (key == EventMouseEnter)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').addEventListener('mouseenter', mouseEnterEvent)" };
            }
            if (key == EventMouseLeave)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').addEventListener('mouseleave', mouseLeaveEvent)" };
            }
            if (key == EventMouseMove)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').addEventListener('mousemove', mouseMoveEvent)" };
            }
            //if (key == EventText)
            //{
            //    script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').addEventListener('input', textChangedEvent)" };
            //}
            if (!string.IsNullOrEmpty(WebviewIdentifier))
            {
                if (script.Length > 0)
                {
                    Page.RunScript(string.Join("", script));
                }
            }
            else
            {
                preLayoutScript.Add(script);
            }
        }

        internal void RemoveJsEvent(object key)
        {
            string[] script = { };
            if (key == EventClick)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').removeEventListener('click', clickEvent)" };
            }
            if (key == EventDoubleClick)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').removeEventListener('dblclick', doubleClickEvent)" };
            }
            if (key == EventMouseClick)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').removeEventListener('click', mouseClickEvent)" };
            }
            if (key == EventMouseDoubleClick)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').removeEventListener('dblclick', mouseDoubleClickEvent)" };
            }
            if (key == EventMouseEnter)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').removeEventListener('mouseenter', mouseEnterEvent)" };
            }
            if (key == EventMouseLeave)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').removeEventListener('mouseleave', mouseLeaveEvent)" };
            }
            if (key == EventMouseMove)
            {
                script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').removeEventListener('mousemove', mouseMoveEvent)" };
            }
            //if (key == EventText)
            //{
            //    script = new string[] { $@"document.getElementById('", WebviewIdentifier, "').removeEventListener('input', textChangedEvent)" };
            //}
            if (!string.IsNullOrEmpty(WebviewIdentifier))
            {
                if (script.Length > 0)
                {
                    Page.RunScript(string.Join("", script));
                }
            }
            else
            {
                preLayoutScript.Add(script);
            }
        }


        [
        Localizable(true),
        ]
        public Size Size
        {
            get
            {
                return new Size(width, height);
            }
            set
            {
                SetBounds(x, y, value.Width, value.Height, BoundsSpecified.Size);
            }
        }

        // Set/reset by ContainerControl.AssignActiveControlInternal
        internal bool BecomingActiveControl
        {
            get
            {
                return GetState2(STATE2_BECOMINGACTIVECONTROL);
            }
            set
            {
                if (value != this.BecomingActiveControl)
                {
                    Application.ThreadContext.FromCurrent().ActivatingControl = (value) ? this : null;
                    SetState2(STATE2_BECOMINGACTIVECONTROL, value);
                }
            }
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected internal virtual bool ProcessMnemonic(char charCode)
        {
#if DEBUG
            Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "Control.ProcessMnemonic [0x" + ((int)charCode).ToString("X", CultureInfo.InvariantCulture) + "]");
#endif
            return false;
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected virtual bool ProcessDialogChar(char charCode)
        {
            Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "Control.ProcessDialogChar [" + charCode.ToString() + "]");
            return parent == null ? false : parent.ProcessDialogChar(charCode);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Scale(SizeF factor)
        {

            // VSWhidbey 501184:
            // manually call ScaleControl recursively instead of the internal scale method
            // when someone calls this method, they really do want to do some sort of 
            // zooming feature, as opposed to AutoScale.
            using (new LayoutTransaction(this, this, PropertyNames.Bounds, false))
            {
                //ScaleControl(factor, factor, this);
                if (ScaleChildren)
                {
                    ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
                    if (controlsCollection != null)
                    {
                        // PERFNOTE: This is more efficient than using Foreach.  Foreach
                        // forces the creation of an array subset enum each time we
                        // enumerate
                        for (int i = 0; i < controlsCollection.Count; i++)
                        {
                            Control c = controlsCollection[i];
                            c.Scale(factor);
                        }
                    }
                }
            }

            LayoutTransaction.DoLayout(this, this, PropertyNames.Bounds);

        }

        internal virtual bool CanProcessMnemonic()
        {
            if (!this.Enabled || !this.Visible)
            {
                return false;
            }

            if (this.parent != null)
            {
                return this.parent.CanProcessMnemonic();
            }

            return true;
        }

        internal virtual void Scale(SizeF includedFactor, SizeF excludedFactor, Control requestingControl)
        {
            // When we scale, we are establishing new baselines for the
            // positions of all controls.  Therefore, we should resume(false).
            using (new LayoutTransaction(this, this, PropertyNames.Bounds, false))
            {
                //ScaleControl(includedFactor, excludedFactor, requestingControl);
                //ScaleChildControls(includedFactor, excludedFactor, requestingControl);
            }
            LayoutTransaction.DoLayout(this, this, PropertyNames.Bounds);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual bool ScaleChildren
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                AddJsEvent(EventAutoSizeChanged);
                base.Events.AddHandler(EventAutoSizeChanged, value);
            }
            remove
            {
                RemoveJsEvent(EventAutoSizeChanged);
                base.Events.RemoveHandler(EventAutoSizeChanged, value);
            }
        }

        public event EventHandler BackColorChanged
        {
            add
            {
                AddJsEvent(EventBackColor);
                base.Events.AddHandler(EventBackColor, value);
            }
            remove
            {
                RemoveJsEvent(EventBackColor);
                base.Events.RemoveHandler(EventBackColor, value);
            }
        }

        public event EventHandler BackgroundImageChanged
        {
            add
            {
                AddJsEvent(EventBackgroundImage);
                base.Events.AddHandler(EventBackgroundImage, value);
            }
            remove
            {
                RemoveJsEvent(EventBackgroundImage);
                base.Events.RemoveHandler(EventBackgroundImage, value);
            }
        }

        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                AddJsEvent(EventBackgroundImageLayout);
                base.Events.AddHandler(EventBackgroundImageLayout, value);
            }
            remove
            {
                RemoveJsEvent(EventBackgroundImageLayout);
                base.Events.RemoveHandler(EventBackgroundImageLayout, value);
            }
        }

        public event EventHandler BindingContextChanged
        {
            add
            {
                AddJsEvent(EventBindingContext);
                base.Events.AddHandler(EventBindingContext, value);
            }
            remove
            {
                RemoveJsEvent(EventBindingContext);
                base.Events.RemoveHandler(EventBindingContext, value);
            }
        }

        public event EventHandler CausesValidationChanged
        {
            add
            {
                AddJsEvent(EventCausesValidation);
                base.Events.AddHandler(EventCausesValidation, value);
            }
            remove
            {
                RemoveJsEvent(EventCausesValidation);
                base.Events.RemoveHandler(EventCausesValidation, value);
            }
        }

        public event EventHandler ClientSizeChanged
        {
            add
            {
                AddJsEvent(EventClientSize);
                base.Events.AddHandler(EventClientSize, value);
            }
            remove
            {
                RemoveJsEvent(EventClientSize);
                base.Events.RemoveHandler(EventClientSize, value);
            }
        }

        [Browsable(false)]
        public event EventHandler ContextMenuChanged
        {
            add
            {
                AddJsEvent(EventContextMenu);
                base.Events.AddHandler(EventContextMenu, value);
            }
            remove
            {
                RemoveJsEvent(EventContextMenu);
                base.Events.RemoveHandler(EventContextMenu, value);
            }
        }

        public event EventHandler ContextMenuStripChanged
        {
            add
            {
                AddJsEvent(EventContextMenuStrip);
                base.Events.AddHandler(EventContextMenuStrip, value);
            }
            remove
            {
                RemoveJsEvent(EventContextMenuStrip);
                base.Events.RemoveHandler(EventContextMenuStrip, value);
            }
        }

        public event EventHandler CursorChanged
        {
            add
            {
                AddJsEvent(EventCursor);
                base.Events.AddHandler(EventCursor, value);
            }
            remove
            {
                RemoveJsEvent(EventCursor);
                base.Events.RemoveHandler(EventCursor, value);
            }
        }

        public event EventHandler DockChanged
        {
            add
            {
                AddJsEvent(EventDock);
                base.Events.AddHandler(EventDock, value);
            }
            remove
            {
                RemoveJsEvent(EventDock);
                base.Events.RemoveHandler(EventDock, value);
            }
        }

        public event EventHandler EnabledChanged
        {
            add
            {
                AddJsEvent(EventEnabled);
                base.Events.AddHandler(EventEnabled, value);
            }
            remove
            {
                RemoveJsEvent(EventEnabled);
                base.Events.RemoveHandler(EventEnabled, value);
            }
        }

        public event EventHandler FontChanged
        {
            add
            {
                AddJsEvent(EventFont);
                base.Events.AddHandler(EventFont, value);
            }
            remove
            {
                RemoveJsEvent(EventFont);
                base.Events.RemoveHandler(EventFont, value);
            }
        }

        public event EventHandler ForeColorChanged
        {
            add
            {
                AddJsEvent(EventForeColor);
                base.Events.AddHandler(EventForeColor, value);
            }
            remove
            {
                RemoveJsEvent(EventForeColor);
                base.Events.RemoveHandler(EventForeColor, value);
            }
        }

        public event EventHandler LocationChanged
        {
            add
            {
                AddJsEvent(EventLocation);
                base.Events.AddHandler(EventLocation, value);
            }
            remove
            {
                RemoveJsEvent(EventLocation);
                base.Events.RemoveHandler(EventLocation, value);
            }
        }

        public event EventHandler MarginChanged
        {
            add
            {
                AddJsEvent(EventMarginChanged);
                base.Events.AddHandler(EventMarginChanged, value);
            }
            remove
            {
                RemoveJsEvent(EventMarginChanged);
                base.Events.RemoveHandler(EventMarginChanged, value);
            }
        }

        public event EventHandler RegionChanged
        {
            add
            {
                AddJsEvent(EventRegionChanged);
                base.Events.AddHandler(EventRegionChanged, value);
            }
            remove
            {
                RemoveJsEvent(EventRegionChanged);
                base.Events.RemoveHandler(EventRegionChanged, value);
            }
        }

        public event EventHandler RightToLeftChanged
        {
            add
            {
                AddJsEvent(EventRightToLeft);
                base.Events.AddHandler(EventRightToLeft, value);
            }
            remove
            {
                RemoveJsEvent(EventRightToLeft);
                base.Events.RemoveHandler(EventRightToLeft, value);
            }
        }

        public event EventHandler SizeChanged
        {
            add
            {
                AddJsEvent(EventSize);
                base.Events.AddHandler(EventSize, value);
            }
            remove
            {
                RemoveJsEvent(EventSize);
                base.Events.RemoveHandler(EventSize, value);
            }
        }

        public event EventHandler TabIndexChanged
        {
            add
            {
                AddJsEvent(EventCursor);
                base.Events.AddHandler(EventTabIndex, value);
            }
            remove
            {
                RemoveJsEvent(EventAutoSizeChanged);
                base.Events.RemoveHandler(EventTabIndex, value);
            }
        }

        public event EventHandler TabStopChanged
        {
            add
            {
                AddJsEvent(EventTabStop);
                base.Events.AddHandler(EventTabStop, value);
            }
            remove
            {
                RemoveJsEvent(EventTabStop);
                base.Events.RemoveHandler(EventTabStop, value);
            }
        }

        public event EventHandler TextChanged
        {
            add
            {
                AddJsEvent(EventText);
                base.Events.AddHandler(EventText, value);
            }
            remove
            {
                RemoveJsEvent(EventText);
                base.Events.RemoveHandler(EventText, value);
            }
        }

        public event EventHandler VisibleChanged
        {
            add
            {
                AddJsEvent(EventVisible);
                base.Events.AddHandler(EventVisible, value);
            }
            remove
            {
                RemoveJsEvent(EventVisible);
                base.Events.RemoveHandler(EventVisible, value);
            }
        }

        public event EventHandler Click
        {
            add
            {
                AddJsEvent(EventClick);
                base.Events.AddHandler(EventClick, value);
            }
            remove
            {
                RemoveJsEvent(EventClick);
                base.Events.RemoveHandler(EventClick, value);
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event ControlEventHandler ControlAdded
        {
            add
            {
                AddJsEvent(EventControlAdded);
                base.Events.AddHandler(EventControlAdded, value);
            }
            remove
            {
                RemoveJsEvent(EventControlAdded);
                base.Events.RemoveHandler(EventControlAdded, value);
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event ControlEventHandler ControlRemoved
        {
            add
            {
                AddJsEvent(EventControlRemoved);
                base.Events.AddHandler(EventControlRemoved, value);
            }
            remove
            {
                RemoveJsEvent(EventControlRemoved);
                base.Events.RemoveHandler(EventControlRemoved, value);
            }
        }

        public event DragEventHandler DragDrop
        {
            add
            {
                AddJsEvent(EventDragDrop);
                base.Events.AddHandler(EventDragDrop, value);
            }
            remove
            {
                RemoveJsEvent(EventDragDrop);
                base.Events.RemoveHandler(EventDragDrop, value);
            }
        }

        public event DragEventHandler DragEnter
        {
            add
            {
                AddJsEvent(EventDragEnter);
                base.Events.AddHandler(EventDragEnter, value);
            }
            remove
            {
                RemoveJsEvent(EventDragEnter);
                base.Events.RemoveHandler(EventDragEnter, value);
            }
        }

        public event DragEventHandler DragOver
        {
            add
            {
                AddJsEvent(EventDragOver);
                base.Events.AddHandler(EventDragOver, value);
            }
            remove
            {
                RemoveJsEvent(EventDragOver);
                base.Events.RemoveHandler(EventDragOver, value);
            }
        }

        public event EventHandler DragLeave
        {
            add
            {
                AddJsEvent(EventDragLeave);
                base.Events.AddHandler(EventDragLeave, value);
            }
            remove
            {
                RemoveJsEvent(EventDragLeave);
                base.Events.RemoveHandler(EventDragLeave, value);
            }
        }

        public event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                AddJsEvent(EventGiveFeedback);
                base.Events.AddHandler(EventGiveFeedback, value);
            }
            remove
            {
                RemoveJsEvent(EventGiveFeedback);
                base.Events.RemoveHandler(EventGiveFeedback, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler HandleCreated
        {
            add
            {
                AddJsEvent(EventHandleCreated);
                base.Events.AddHandler(EventHandleCreated, value);
            }
            remove
            {
                RemoveJsEvent(EventHandleCreated);
                base.Events.RemoveHandler(EventHandleCreated, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler HandleDestroyed
        {
            add
            {
                AddJsEvent(EventHandleDestroyed);
                base.Events.AddHandler(EventHandleDestroyed, value);
            }
            remove
            {
                RemoveJsEvent(EventHandleDestroyed);
                base.Events.RemoveHandler(EventHandleDestroyed, value);
            }
        }

        public event HelpEventHandler HelpRequested
        {
            add
            {
                AddJsEvent(EventHelpRequested);
                base.Events.AddHandler(EventHelpRequested, value);
            }
            remove
            {
                RemoveJsEvent(EventHelpRequested);
                base.Events.RemoveHandler(EventHelpRequested, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event InvalidateEventHandler Invalidated
        {
            add
            {
                AddJsEvent(EventInvalidated);
                base.Events.AddHandler(EventInvalidated, value);
            }
            remove
            {
                RemoveJsEvent(EventInvalidated);
                base.Events.RemoveHandler(EventInvalidated, value);
            }
        }

        public event EventHandler PaddingChanged
        {
            add
            {
                AddJsEvent(EventPaddingChanged);
                base.Events.AddHandler(EventPaddingChanged, value);
            }
            remove
            {
                RemoveJsEvent(EventPaddingChanged);
                base.Events.RemoveHandler(EventPaddingChanged, value);
            }
        }

        public event PaintEventHandler Paint
        {
            add
            {
                AddJsEvent(EventPaint);
                base.Events.AddHandler(EventPaint, value);
            }
            remove
            {
                RemoveJsEvent(EventPaint);
                base.Events.RemoveHandler(EventPaint, value);
            }
        }

        public event QueryContinueDragEventHandler QueryContinueDrag
        {
            add
            {
                AddJsEvent(EventQueryContinueDrag);
                base.Events.AddHandler(EventQueryContinueDrag, value);
            }
            remove
            {
                RemoveJsEvent(EventQueryContinueDrag);
                base.Events.RemoveHandler(EventQueryContinueDrag, value);
            }
        }

        public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp
        {
            add
            {
                AddJsEvent(EventQueryAccessibilityHelp);
                base.Events.AddHandler(EventQueryAccessibilityHelp, value);
            }
            remove
            {
                RemoveJsEvent(EventQueryAccessibilityHelp);
                base.Events.RemoveHandler(EventQueryAccessibilityHelp, value);
            }
        }

        public event EventHandler DoubleClick
        {
            add
            {
                AddJsEvent(EventDoubleClick);
                base.Events.AddHandler(EventDoubleClick, value);
            }
            remove
            {
                RemoveJsEvent(EventDoubleClick);
                base.Events.RemoveHandler(EventDoubleClick, value);
            }
        }

        public event EventHandler Enter
        {
            add
            {
                AddJsEvent(EventEnter);
                base.Events.AddHandler(EventEnter, value);
            }
            remove
            {
                RemoveJsEvent(EventEnter);
                base.Events.RemoveHandler(EventEnter, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler GotFocus
        {
            add
            {
                AddJsEvent(EventGotFocus);
                base.Events.AddHandler(EventGotFocus, value);
            }
            remove
            {
                RemoveJsEvent(EventGotFocus);
                base.Events.RemoveHandler(EventGotFocus, value);
            }
        }

        public event KeyEventHandler KeyDown
        {
            add
            {
                AddJsEvent(EventKeyDown);
                base.Events.AddHandler(EventKeyDown, value);
            }
            remove
            {
                RemoveJsEvent(EventKeyDown);
                base.Events.RemoveHandler(EventKeyDown, value);
            }
        }

        public event KeyPressEventHandler KeyPress
        {
            add
            {
                AddJsEvent(EventKeyPress);
                base.Events.AddHandler(EventKeyPress, value);
            }
            remove
            {
                RemoveJsEvent(EventKeyPress);
                base.Events.RemoveHandler(EventKeyPress, value);
            }
        }

        public event KeyEventHandler KeyUp
        {
            add
            {
                AddJsEvent(EventKeyUp);
                base.Events.AddHandler(EventKeyUp, value);
            }
            remove
            {
                RemoveJsEvent(EventKeyUp);
                base.Events.RemoveHandler(EventKeyUp, value);
            }
        }

        public event LayoutEventHandler Layout
        {
            add
            {
                AddJsEvent(EventLayout);
                base.Events.AddHandler(EventLayout, value);
            }
            remove
            {
                RemoveJsEvent(EventLayout);
                base.Events.RemoveHandler(EventLayout, value);
            }
        }

        public event EventHandler Leave
        {
            add
            {
                AddJsEvent(EventLeave);
                base.Events.AddHandler(EventLeave, value);
            }
            remove
            {
                RemoveJsEvent(EventLeave);
                base.Events.RemoveHandler(EventLeave, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler LostFocus
        {
            add
            {
                AddJsEvent(EventLostFocus);
                base.Events.AddHandler(EventLostFocus, value);
            }
            remove
            {
                RemoveJsEvent(EventLostFocus);
                base.Events.RemoveHandler(EventLostFocus, value);
            }
        }

        public event MouseEventHandler MouseClick
        {
            add
            {
                AddJsEvent(EventMouseClick);
                base.Events.AddHandler(EventMouseClick, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseClick);
                base.Events.RemoveHandler(EventMouseClick, value);
            }
        }

        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                AddJsEvent(EventMouseDoubleClick);
                base.Events.AddHandler(EventMouseDoubleClick, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseDoubleClick);
                base.Events.RemoveHandler(EventMouseDoubleClick, value);
            }
        }

        public event EventHandler MouseCaptureChanged
        {
            add
            {
                AddJsEvent(EventMouseCaptureChanged);
                base.Events.AddHandler(EventMouseCaptureChanged, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseCaptureChanged);
                base.Events.RemoveHandler(EventMouseCaptureChanged, value);
            }
        }

        public event MouseEventHandler MouseDown
        {
            add
            {
                AddJsEvent(EventMouseDown);
                base.Events.AddHandler(EventMouseDown, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseDown);
                base.Events.RemoveHandler(EventMouseDown, value);
            }
        }

        public event EventHandler MouseEnter
        {
            add
            {
                AddJsEvent(EventMouseEnter);
                base.Events.AddHandler(EventMouseEnter, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseEnter);
                base.Events.RemoveHandler(EventMouseEnter, value);
            }
        }

        public event EventHandler MouseLeave
        {
            add
            {
                AddJsEvent(EventMouseLeave);
                base.Events.AddHandler(EventMouseLeave, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseLeave);
                base.Events.RemoveHandler(EventMouseLeave, value);
            }
        }

        public event EventHandler DpiChangedBeforeParent
        {
            add
            {
                AddJsEvent(EventDpiChangedBeforeParent);
                base.Events.AddHandler(EventDpiChangedBeforeParent, value);
            }
            remove
            {
                RemoveJsEvent(EventDpiChangedBeforeParent);
                base.Events.RemoveHandler(EventDpiChangedBeforeParent, value);
            }
        }

        public event EventHandler DpiChangedAfterParent
        {
            add
            {
                AddJsEvent(EventDpiChangedAfterParent);
                base.Events.AddHandler(EventDpiChangedAfterParent, value);
            }
            remove
            {
                RemoveJsEvent(EventDpiChangedAfterParent);
                base.Events.RemoveHandler(EventDpiChangedAfterParent, value);
            }
        }

        public event EventHandler MouseHover
        {
            add
            {
                AddJsEvent(EventMouseHover);
                base.Events.AddHandler(EventMouseHover, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseHover);
                base.Events.RemoveHandler(EventMouseHover, value);
            }
        }

        public event MouseEventHandler MouseMove
        {
            add
            {
                AddJsEvent(EventMouseMove);
                base.Events.AddHandler(EventMouseMove, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseMove);
                base.Events.RemoveHandler(EventMouseMove, value);
            }
        }

        public event MouseEventHandler MouseUp
        {
            add
            {
                AddJsEvent(EventMouseUp);
                base.Events.AddHandler(EventMouseUp, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseUp);
                base.Events.RemoveHandler(EventMouseUp, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event MouseEventHandler MouseWheel
        {
            add
            {
                AddJsEvent(EventMouseWheel);
                base.Events.AddHandler(EventMouseWheel, value);
            }
            remove
            {
                RemoveJsEvent(EventMouseWheel);
                base.Events.RemoveHandler(EventMouseWheel, value);
            }
        }

        public event EventHandler Move
        {
            add
            {
                AddJsEvent(EventMove);
                base.Events.AddHandler(EventMove, value);
            }
            remove
            {
                RemoveJsEvent(EventMove);
                base.Events.RemoveHandler(EventMove, value);
            }
        }

        public event PreviewKeyDownEventHandler PreviewKeyDown
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            add
            {
                AddJsEvent(EventPreviewKeyDown);
                base.Events.AddHandler(EventPreviewKeyDown, value);
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            remove
            {
                RemoveJsEvent(EventPreviewKeyDown);
                base.Events.RemoveHandler(EventPreviewKeyDown, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler Resize
        {
            add
            {
                AddJsEvent(EventResize);
                base.Events.AddHandler(EventResize, value);
            }
            remove
            {
                RemoveJsEvent(EventResize);
                base.Events.RemoveHandler(EventResize, value);
            }
        }

        public event UICuesEventHandler ChangeUICues
        {
            add
            {
                AddJsEvent(EventChangeUICues);
                base.Events.AddHandler(EventChangeUICues, value);
            }
            remove
            {
                RemoveJsEvent(EventChangeUICues);
                base.Events.RemoveHandler(EventChangeUICues, value);
            }
        }

        public event EventHandler StyleChanged
        {
            add
            {
                AddJsEvent(EventStyleChanged);
                base.Events.AddHandler(EventStyleChanged, value);
            }
            remove
            {
                RemoveJsEvent(EventStyleChanged);
                base.Events.RemoveHandler(EventStyleChanged, value);
            }
        }

        public event EventHandler SystemColorsChanged
        {
            add
            {
                AddJsEvent(EventSystemColorsChanged);
                base.Events.AddHandler(EventSystemColorsChanged, value);
            }
            remove
            {
                RemoveJsEvent(EventSystemColorsChanged);
                base.Events.RemoveHandler(EventSystemColorsChanged, value);
            }
        }

        public event CancelEventHandler Validating
        {
            add
            {
                AddJsEvent(EventValidating);
                base.Events.AddHandler(EventValidating, value);
            }
            remove
            {
                RemoveJsEvent(EventValidating);
                base.Events.RemoveHandler(EventValidating, value);
            }
        }

        public event EventHandler Validated
        {
            add
            {
                AddJsEvent(EventValidated);
                base.Events.AddHandler(EventValidated, value);
            }
            remove
            {
                RemoveJsEvent(EventValidated);
                base.Events.RemoveHandler(EventValidated, value);
            }
        }

        public event EventHandler ParentChanged
        {
            add
            {
                AddJsEvent(EventParent);
                base.Events.AddHandler(EventParent, value);
            }
            remove
            {
                RemoveJsEvent(EventParent);
                base.Events.RemoveHandler(EventParent, value);
            }
        }

        public event EventHandler ImeModeChanged
        {
            add
            {
                AddJsEvent(EventImeModeChanged);
                base.Events.AddHandler(EventImeModeChanged, value);
            }
            remove
            {
                RemoveJsEvent(EventImeModeChanged);
                base.Events.RemoveHandler(EventImeModeChanged, value);
            }
        }

        internal PropertyStore Properties
        {
            get
            {
                return propertyStore;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual Drawing.Size SizeFromClientSize(Drawing.Size clientSize)
        {
            return SizeFromClientSize(clientSize.Width, clientSize.Height);
        }

        internal Drawing.Size SizeFromClientSize(int width, int height)
        {
            NativeMethods.RECT rect = new NativeMethods.RECT(0, 0, width, height);

            //CreateParams cp = CreateParams;
            //AdjustWindowRectEx(ref rect, cp.Style, HasMenu, cp.ExStyle);
            return rect.Size;
        }

        static Control()
        {
            EventAutoSizeChanged = new object();
            EventKeyDown = new object();
            EventKeyPress = new object();
            EventKeyUp = new object();
            EventMouseDown = new object();
            EventMouseEnter = new object();
            EventMouseLeave = new object();
            EventDpiChangedBeforeParent = new object();
            EventDpiChangedAfterParent = new object();
            EventMouseHover = new object();
            EventMouseMove = new object();
            EventMouseUp = new object();
            EventMouseWheel = new object();
            EventClick = new object();
            EventClientSize = new object();
            EventDoubleClick = new object();
            EventMouseClick = new object();
            EventMouseDoubleClick = new object();
            EventMouseCaptureChanged = new object();
            EventMove = new object();
            EventResize = new object();
            EventLayout = new object();
            EventGotFocus = new object();
            EventLostFocus = new object();
            EventEnabledChanged = new object();
            EventEnter = new object();
            EventLeave = new object();
            EventHandleCreated = new object();
            EventHandleDestroyed = new object();
            EventVisibleChanged = new object();
            EventControlAdded = new object();
            EventControlRemoved = new object();
            EventChangeUICues = new object();
            EventSystemColorsChanged = new object();
            EventValidating = new object();
            EventValidated = new object();
            EventStyleChanged = new object();
            EventImeModeChanged = new object();
            EventHelpRequested = new object();
            EventPaint = new object();
            EventInvalidated = new object();
            EventQueryContinueDrag = new object();
            EventGiveFeedback = new object();
            EventDragEnter = new object();
            EventDragLeave = new object();
            EventDragOver = new object();
            EventDragDrop = new object();
            EventQueryAccessibilityHelp = new object();
            EventBackgroundImage = new object();
            EventBackgroundImageLayout = new object();
            EventBindingContext = new object();
            EventBackColor = new object();
            EventParent = new object();
            EventVisible = new object();
            EventText = new object();
            EventTabStop = new object();
            EventTabIndex = new object();
            EventSize = new object();
            EventRightToLeft = new object();
            EventLocation = new object();
            EventForeColor = new object();
            EventFont = new object();
            EventEnabled = new object();
            EventDock = new object();
            EventCursor = new object();
            EventContextMenu = new object();
            EventContextMenuStrip = new object();
            EventCausesValidation = new object();
            EventRegionChanged = new object();
            EventMarginChanged = new object();
            EventPaddingChanged = new object();
            EventPreviewKeyDown = new object();
        }

        internal void InvokeOnClick(Control toInvoke, EventArgs e)
        {
            toInvoke?.OnClick(e);
        }

        internal virtual void OnClick(EventArgs e)
        {
            ((EventHandler)base.Events[EventClick])?.Invoke(this, e);
        }

        internal virtual void OnResize(EventArgs e)
        {
            ((EventHandler)base.Events[EventResize])?.Invoke(this, e);
        }

        internal virtual void OnPaddingChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetStyle(ControlStyles.ResizeRedraw))
            {
                Invalidate();
            }
            EventHandler handler = (EventHandler)Events[EventPaddingChanged];
            if (handler != null) handler(this, e);
        }

        internal virtual void OnTextChanged(EventArgs e)
        {
            if (base.Events[EventText] is EventHandler eventHandler)
            {
                eventHandler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnLocationChanged(EventArgs e)
        {
            OnMove(EventArgs.Empty);
            if (base.Events[EventLocation] is EventHandler eventHandler)
            {
                eventHandler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMove(EventArgs e)
        {
            ((EventHandler)base.Events[EventMove])?.Invoke(this, e);
            //if (RenderTransparent)
            //{
            //    Invalidate();
            //}
        }

        public virtual bool AllowDrop
        {
            // TODO:
            get
            {
                return false;
            }
            set
            {

            }
        }

        [
        Localizable(true),
        ]
        public bool Visible
        {
            get
            {
                return GetVisibleCore();
            }
            set
            {
                SetVisibleCore(value);
            }
        }


        internal virtual bool GetVisibleCore()
        {
            // We are only visible if our parent is visible
            if (!GetState(STATE_VISIBLE))
                return false;
            else if (ParentInternal == null)
                return true;
            else
                return ParentInternal.GetVisibleCore();
        }

        public bool Contains(Control ctl)
        {
            while (ctl != null)
            {
                ctl = ctl.ParentInternal;
                if (ctl == null)
                {
                    return false;
                }
                if (ctl == this)
                {
                    return true;
                }
            }
            return false;
        }


        internal virtual Control GetFirstChildControlInTabOrder(bool forward)
        {

            ControlCollection ctlControls = (ControlCollection)this.Properties.GetObject(PropControlsCollection);

            Control found = null;
            if (ctlControls != null)
            {
                if (forward)
                {
                    for (int c = 0; c < ctlControls.Count; c++)
                    {
                        if (found == null || found.tabIndex > ctlControls[c].tabIndex)
                        {
                            found = ctlControls[c];
                        }
                    }
                }
                else
                {

                    // Cycle through the controls in reverse z-order looking for the one with the highest
                    // tab index.
                    //
                    for (int c = ctlControls.Count - 1; c >= 0; c--)
                    {
                        if (found == null || found.tabIndex < ctlControls[c].tabIndex)
                        {
                            found = ctlControls[c];
                        }
                    }
                }
            }
            return found;

        }

        public Control GetNextControl(Control ctl, bool forward)
        {
            if (!Contains(ctl))
            {
                ctl = this;
            }

            if (forward)
            {
                ControlCollection ctlControls = (ControlCollection)ctl.Properties.GetObject(PropControlsCollection);

                if (ctlControls != null && ctlControls.Count > 0 && (ctl == this || !IsFocusManagingContainerControl(ctl)))
                {
                    Control found = ctl.GetFirstChildControlInTabOrder(/*forward=*/true);
                    if (found != null)
                    {
                        return found;
                    }
                }

                while (ctl != this)
                {
                    int targetIndex = ctl.tabIndex;
                    bool hitCtl = false;
                    Control found = null;
                    Control p = ctl.parent;

                    // Cycle through the controls in z-order looking for the one with the next highest
                    // tab index.  Because there can be dups, we have to start with the existing tab index and
                    // remember to exclude the current control.
                    //
                    int parentControlCount = 0;

                    ControlCollection parentControls = (ControlCollection)p.Properties.GetObject(PropControlsCollection);

                    if (parentControls != null)
                    {
                        parentControlCount = parentControls.Count;
                    }

                    for (int c = 0; c < parentControlCount; c++)
                    {

                        // The logic for this is a bit lengthy, so I have broken it into separate
                        // caluses:

                        // We are not interested in ourself.
                        //
                        if (parentControls[c] != ctl)
                        {

                            // We are interested in controls with >= tab indexes to ctl.  We must include those
                            // controls with equal indexes to account for duplicate indexes.
                            //
                            if (parentControls[c].tabIndex >= targetIndex)
                            {

                                // Check to see if this control replaces the "best match" we've already
                                // found.
                                //
                                if (found == null || found.tabIndex > parentControls[c].tabIndex)
                                {

                                    // Finally, check to make sure that if this tab index is the same as ctl,
                                    // that we've already encountered ctl in the z-order.  If it isn't the same,
                                    // than we're more than happy with it.
                                    //
                                    if (parentControls[c].tabIndex != targetIndex || hitCtl)
                                    {
                                        found = parentControls[c];
                                    }
                                }
                            }
                        }
                        else
                        {
                            // We track when we have encountered "ctl".  We never want to select ctl again, but
                            // we want to know when we've seen it in case we find another control with the same tab index.
                            //
                            hitCtl = true;
                        }
                    }

                    if (found != null)
                    {
                        return found;
                    }

                    ctl = ctl.parent;
                }
            }
            else
            {
                if (ctl != this)
                {

                    int targetIndex = ctl.tabIndex;
                    bool hitCtl = false;
                    Control found = null;
                    Control p = ctl.parent;

                    // Cycle through the controls in reverse z-order looking for the next lowest tab index.  We must
                    // start with the same tab index as ctl, because there can be dups.
                    //
                    int parentControlCount = 0;

                    ControlCollection parentControls = (ControlCollection)p.Properties.GetObject(PropControlsCollection);

                    if (parentControls != null)
                    {
                        parentControlCount = parentControls.Count;
                    }

                    for (int c = parentControlCount - 1; c >= 0; c--)
                    {

                        // The logic for this is a bit lengthy, so I have broken it into separate
                        // caluses:

                        // We are not interested in ourself.
                        //
                        if (parentControls[c] != ctl)
                        {

                            // We are interested in controls with <= tab indexes to ctl.  We must include those
                            // controls with equal indexes to account for duplicate indexes.
                            //
                            if (parentControls[c].tabIndex <= targetIndex)
                            {

                                // Check to see if this control replaces the "best match" we've already
                                // found.
                                //
                                if (found == null || found.tabIndex < parentControls[c].tabIndex)
                                {

                                    // Finally, check to make sure that if this tab index is the same as ctl,
                                    // that we've already encountered ctl in the z-order.  If it isn't the same,
                                    // than we're more than happy with it.
                                    //
                                    if (parentControls[c].tabIndex != targetIndex || hitCtl)
                                    {
                                        found = parentControls[c];
                                    }
                                }
                            }
                        }
                        else
                        {
                            // We track when we have encountered "ctl".  We never want to select ctl again, but
                            // we want to know when we've seen it in case we find another control with the same tab index.
                            //
                            hitCtl = true;
                        }
                    }

                    // If we were unable to find a control we should return the control's parent.  However, if that parent is us, return
                    // NULL.
                    //
                    if (found != null)
                    {
                        ctl = found;
                    }
                    else
                    {
                        if (p == this)
                        {
                            return null;
                        }
                        else
                        {
                            return p;
                        }
                    }
                }

                // We found a control.  Walk into this control to find the proper sub control within it to select.
                //
                ControlCollection ctlControls = (ControlCollection)ctl.Properties.GetObject(PropControlsCollection);

                while (ctlControls != null && ctlControls.Count > 0 && (ctl == this || !IsFocusManagingContainerControl(ctl)))
                {
                    Control found = ctl.GetFirstChildControlInTabOrder(/*forward=*/false);
                    if (found != null)
                    {
                        ctl = found;
                        ctlControls = (ControlCollection)ctl.Properties.GetObject(PropControlsCollection);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return ctl == this ? null : ctl;
        }

        private Control GetNextSelectableControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
        {
            if (!Contains(ctl) || !nested && ctl.parent != this) ctl = null;

            bool alreadyWrapped = false;
            Control start = ctl;
            do
            {
                ctl = GetNextControl(ctl, forward);
                if (ctl == null)
                {
                    if (!wrap) break;
                    if (alreadyWrapped)
                    {
                        return null; //VSWhidbey 423098 prevent infinite wrapping.
                    }
                    alreadyWrapped = true;
                }
                else
                {
                    if (ctl.CanSelect
                        && (!tabStopOnly || ctl.TabStop)
                        && (nested || ctl.parent == this))
                    {

                        //if (AccessibilityImprovements.Level3 && ctl.parent is ToolStrip)
                        //{
                        //    continue;
                        //}
                        return ctl;
                    }
                }
            } while (ctl != start);
            return null;
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool CanSelect
        {
            // We implement this to allow only AxHost to override canSelectCore, but still
            // expose the method publicly
            //
            get
            {
                return CanSelectCore();
            }
        }

        internal virtual bool CanSelectCore()
        {
            if ((controlStyle & ControlStyles.Selectable) != ControlStyles.Selectable)
            {
                return false;
            }

            for (Control ctl = this; ctl != null; ctl = ctl.parent)
            {
                if (!ctl.Enabled || !ctl.Visible)
                {
                    return false;
                }
            }

            return true;
        }

        public void Select()
        {
            Select(false, false);
        }

        protected virtual void Select(bool directed, bool forward)
        {
            IContainerControl c = GetContainerControlInternal();

            if (c != null)
            {
                c.ActiveControl = this;
            }
        }

        public bool SelectNextControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
        {
            Control nextSelectableControl = this.GetNextSelectableControl(ctl, forward, tabStopOnly, nested, wrap);
            if (nextSelectableControl != null)
            {
                nextSelectableControl.Select(true, forward);
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool SelectNextControlInternal(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
        {
            return SelectNextControl(ctl, forward, tabStopOnly, nested, wrap);
        }

        private void SelectNextIfFocused()
        {
            // V#32437 - We want to move focus away from hidden controls, so this
            //           function was added.
            //
            if (ContainsFocus && ParentInternal != null)
            {
                IContainerControl c = ParentInternal.GetContainerControlInternal();

                if (c != null)
                {
                    // SECREVIEW : Control.SelectNextControl generates a call to ContainerControl.ActiveControl which demands
                    //             ModifyFocus permission, the demand is to prevent DOS attacks but it doesn't expose a sec
                    //             vulnerability indirectly.  So it is safe to call the internal version of SelectNextControl here.
                    //
                    ((Control)c).SelectNextControlInternal(this, true, true, true, true);
                }
            }
        }

        internal virtual bool IsContainerControl
        {
            get
            {
                return false;
            }
        }

        public IContainerControl GetContainerControl()
        {
            Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "GetParent Demanded");
            IntSecurity.GetParent.Demand();
            return GetContainerControlInternal();
        }

        private static bool IsFocusManagingContainerControl(Control ctl)
        {
            return ((ctl.controlStyle & ControlStyles.ContainerControl) == ControlStyles.ContainerControl && ctl is IContainerControl);
        }

        // SECURITY WARNING: This method bypasses a security demand. Use with caution!
        internal IContainerControl GetContainerControlInternal()
        {
            Control c = this;

            // VsWhidbey 434959 : Refer to IsContainerControl property for more details.            
            if (c != null && IsContainerControl)
            {
                c = c.ParentInternal;
            }
            while (c != null && (!IsFocusManagingContainerControl(c)))
            {
                c = c.ParentInternal;
            }
            return (IContainerControl)c;
        }

        protected virtual void SetVisibleCore(bool value)
        {
            try
            {
                System.Internal.HandleCollector.SuspendCollect();

                if (GetVisibleCore() != value)
                {
                    if (!value)
                    {
                        SelectNextIfFocused();
                    }

                    bool fireChange = false;

                    if (GetTopLevel())
                    {

                        // The processing of WmShowWindow will set the visibility
                        // bit and call CreateControl()
                        //
                        if (IsHandleCreated || value)
                        {
                            //SafeNativeMethods.ShowWindow(new HandleRef(this, Handle), value ? ShowParams : NativeMethods.SW_HIDE);
                            PerformLayout();
                        }
                    }
                    else if (IsHandleCreated || value && parent != null && parent.Created)
                    {

                        // We want to mark the control as visible so that CreateControl
                        // knows that we are going to be displayed... however in case
                        // an exception is thrown, we need to back the change out.
                        //
                        SetState(STATE_VISIBLE, value);
                        fireChange = true;
                        try
                        {
                            //if (value) CreateControl();
                            //SafeNativeMethods.SetWindowPos(new HandleRef(window, Handle),
                            //                               NativeMethods.NullHandleRef,
                            //                               0, 0, 0, 0,
                            //                               NativeMethods.SWP_NOSIZE
                            //                               | NativeMethods.SWP_NOMOVE
                            //                               | NativeMethods.SWP_NOZORDER
                            //                               | NativeMethods.SWP_NOACTIVATE
                            //                               | (value ? NativeMethods.SWP_SHOWWINDOW : NativeMethods.SWP_HIDEWINDOW));
                        }
                        catch
                        {
                            SetState(STATE_VISIBLE, !value);
                            throw;
                        }
                    }
                    if (GetVisibleCore() != value)
                    {
                        SetState(STATE_VISIBLE, value);
                        fireChange = true;
                    }

                    if (fireChange)
                    {
                        // We do not do this in the OnPropertyChanged event for visible
                        // Lots of things could cause us to become visible, including a
                        // parent window.  We do not want to indescriminiately layout
                        // due to this, but we do want to layout if the user changed
                        // our visibility.
                        //

                        using (new LayoutTransaction(parent, this, PropertyNames.Visible))
                        {
                            OnVisibleChanged(EventArgs.Empty);
                        }
                    }
                    UpdateRoot();
                }
                else
                { // value of Visible property not changed, but raw bit may have

                    if (!GetState(STATE_VISIBLE) && !value && IsHandleCreated)
                    {
                        // PERF - setting Visible=false twice can get us into this else block
                        // which makes us process WM_WINDOWPOS* messages - make sure we've already 
                        // visible=false - if not, make it so.
                        //if (!SafeNativeMethods.IsWindowVisible(new HandleRef(this, this.Handle)))
                        //{
                        //    // we're already invisible - bail.
                        //    return;
                        //}
                    }

                    SetState(STATE_VISIBLE, value);

                    // If the handle is already created, we need to update the window style.
                    // This situation occurs when the parent control is not currently visible,
                    // but the child control has already been created.
                    //
                    if (IsHandleCreated)
                    {

                        //SafeNativeMethods.SetWindowPos(
                        //                                  new HandleRef(window, Handle), NativeMethods.NullHandleRef, 0, 0, 0, 0, NativeMethods.SWP_NOSIZE |
                        //                                  NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE |
                        //                                  (value ? NativeMethods.SWP_SHOWWINDOW : NativeMethods.SWP_HIDEWINDOW));
                        PerformLayout();
                    }
                }
            }
            finally
            {
                System.Internal.HandleCollector.ResumeCollect();
            }
        }

        private void UpdateRoot()
        {
            //window.LockReference(GetTopLevel() && Visible);
        }

        public IntPtr Handle;
        public int internalIndex;
        internal string WebviewIdentifier = "";
        public Color ForeColor;
        public Color BackColor = Color.FromKnownColor(KnownColor.Control);
        
        public Point AutoScrollOffset;
        public ImageLayout BackgroundImageLayout;


        protected virtual bool CanEnableIme
        {
            get
            {
                // Note: If overriding this property make sure to add the Debug tracing code and call this method (base.CanEnableIme).

                Debug.Indent();
                Debug.WriteLineIf(CompModSwitches.ImeMode.Level >= TraceLevel.Info, string.Format(CultureInfo.CurrentCulture, "Inside get_CanEnableIme(), value = {0}, this = {1}", ImeSupported, this));
                Debug.Unindent();

                return ImeSupported;
            }
        }

        private bool ImeSupported
        {
            get
            {
                return DefaultImeMode != ImeMode.Disable;
            }
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual BindingContext BindingContext
        {
            get
            {
                return BindingContextInternal;
            }
            set
            {
                BindingContextInternal = value;
            }
        }


        internal BindingContext BindingContextInternal
        {
            get
            {
                // See if we have locally overridden the binding manager.
                //
                BindingContext context = (BindingContext)Properties.GetObject(PropBindingManager);
                if (context != null)
                {
                    return context;
                }

                // Otherwise, see if the parent has one for us.
                //
                Control p = ParentInternal;
                if (p != null && p.CanAccessProperties)
                {
                    return p.BindingContext;
                }

                // Otherwise, we have no binding manager available.
                //
                return null;
            }
            set
            {
                BindingContext oldContext = (BindingContext)Properties.GetObject(PropBindingManager);
                BindingContext newContext = value;

                if (oldContext != newContext)
                {
                    Properties.SetObject(PropBindingManager, newContext);

                    // the property change will wire up the bindings.
                    //
                    OnBindingContextChanged(EventArgs.Empty);
                }
            }
        }

        //public Rectangle Bounds;
        public bool CanFocus;
        public bool Capture;
        public bool CausesValidation;
        public static bool CheckForIllegalCrossThreadCalls; public static Color DefaultForeColor
        {
            get { return SystemColors.ControlText; }
        }

        protected virtual Padding DefaultMargin
        {
            get { return CommonProperties.DefaultMargin; }
        }

        protected virtual Drawing.Size DefaultMaximumSize
        {
            get { return CommonProperties.DefaultMaximumSize; }
        }

        protected virtual Drawing.Size DefaultMinimumSize
        {
            get { return CommonProperties.DefaultMinimumSize; }
        }

        protected virtual Padding DefaultPadding
        {
            get { return Padding.Empty; }
        }

        private RightToLeft DefaultRightToLeft
        {
            get { return RightToLeft.No; }
        }

        public static Font DefaultFont
        {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                if (defaultFont == null)
                {
                    defaultFont = SystemFonts.DefaultFont;
                    Debug.Assert(defaultFont != null, "defaultFont wasn't set!");
                }

                return defaultFont;
            }
        }

        [Browsable(false)]
        public Drawing.Size PreferredSize
        {
            get { return GetPreferredSize(Drawing.Size.Empty); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [Localizable(true)]
        [DefaultValue(CommonProperties.DefaultAutoSize)]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool AutoSize
        {
            get { return CommonProperties.GetAutoSize(this); }
            set
            {
                if (value != AutoSize)
                {
                    CommonProperties.SetAutoSize(this, value);
                    if (ParentInternal != null)
                    {
                        // DefaultLayout does not keep anchor information until it needs to.  When
                        // AutoSize became a common property, we could no longer blindly call into
                        // DefaultLayout, so now we do a special InitLayout just for DefaultLayout.
                        if (value && ParentInternal.LayoutEngine == DefaultLayout.Instance)
                        {
                            ParentInternal.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
                        }
                        LayoutTransaction.DoLayout(ParentInternal, this, PropertyNames.AutoSize);
                    }

                    OnAutoSizeChanged(EventArgs.Empty);
                }
            }
        }

        [
        Localizable(true)
        ]
        public Padding Padding
        {
            get { return CommonProperties.GetPadding(this, DefaultPadding); }
            set
            {
                if (value != Padding)
                {
                    CommonProperties.SetPadding(this, value);
                    // Ideally we are being laid out by a LayoutEngine that cares about our preferred size.
                    // We set our LAYOUTISDIRTY bit and ask our parent to refresh us.
                    SetState(STATE_LAYOUTISDIRTY, true);
                    using (new LayoutTransaction(ParentInternal, this, PropertyNames.Padding))
                    {
                        OnPaddingChanged(EventArgs.Empty);
                    }

                    if (GetState(STATE_LAYOUTISDIRTY))
                    {
                        // The above did not cause our layout to be refreshed.  We explicitly refresh our
                        // layout to ensure that any children are repositioned to account for the change
                        // in padding.
                        LayoutTransaction.DoLayout(this, this, PropertyNames.Padding);
                    }
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Control Parent
        {
            get
            {
                Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "GetParent Demanded");
                IntSecurity.GetParent.Demand();

                return ParentInternal;
            }
            set
            {
                ParentInternal = value;
            }
        }

        internal virtual Control ParentInternal
        {
            get
            {
                return parent;
            }
            set
            {
                if (parent != value)
                {
                    if (value != null)
                    {
                        value.Controls.Add(this);
                    }
                    else
                    {
                        parent.Controls.Remove(this);
                    }
                }
            }
        }

        // Form, UserControl, AxHost usage
        internal void SetState(int flag, bool value)
        {
            state = value ? state | flag : state & ~flag;
        }

        // Application, SKWindow usage
        internal void SetState2(int flag, bool value)
        {
            state2 = value ? state2 | flag : state2 & ~flag;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void SetStyle(ControlStyles flag, bool value)
        {
            // WARNING: if we ever add argument checking to "flag", we will need
            // to move private styles like Layered to State.
            // ASURT: 151576 Calling SetStyle(ControlStyles.EnableNotifyMessage,...) should require UnmanagedCode
            if ((flag & ControlStyles.EnableNotifyMessage) != 0 && value)
            {
                // demand security permission for this condition.
                // this will throw security exception in semi-trust.
                IntSecurity.UnmanagedCode.Demand();
            }
            controlStyle = value ? controlStyle | flag : controlStyle & ~flag;
        }

        internal bool GetState(int flag)
        {
            return (state & flag) != 0;
        }

        private bool GetState2(int flag)
        {
            return (state2 & flag) != 0;
        }

        protected bool GetStyle(ControlStyles flag)
        {
            return (controlStyle & flag) == flag;
        }

        public void Hide()
        {
            Visible = false;
        }
        public void Invalidate()
        {
            Invalidate(false);
        }

        public void Invalidate(bool invalidateChildren)
        {
            if (IsHandleCreated)
            {
                //if (invalidateChildren)
                //{
                //    SafeNativeMethods.RedrawWindow(new HandleRef(window, Handle),
                //                                   null, NativeMethods.NullHandleRef,
                //                                   NativeMethods.RDW_INVALIDATE |
                //                                   NativeMethods.RDW_ERASE |
                //                                   NativeMethods.RDW_ALLCHILDREN);
                //}
                //else
                //{
                //    // It's safe to invoke InvalidateRect from a separate thread.
                //    using (new MultithreadSafeCallScope())
                //    {
                //        SafeNativeMethods.InvalidateRect(new HandleRef(window, Handle),
                //                                         null,
                //                                         (controlStyle & ControlStyles.Opaque) != ControlStyles.Opaque);
                //    }
                //}

                //NotifyInvalidate(this.ClientRectangle);
            }
        }

        internal Drawing.Size clientSize;
        public Drawing.Size ClientSize
        {
            get
            {
                return new Drawing.Size(clientWidth, clientHeight);
            }
            set
            {
                clientWidth = value.Width;
                clientHeight = value.Height;
            }
        }
        public bool ContainsFocus;

        internal AnchorStyles anchor;
        public AnchorStyles Anchor
        {
            get
            {
                return anchor;
            }
            set
            {
                if (value != anchor)
                {
                    anchor = value;
                    if (layoutPerformed)
                    {
                        ApplyLocationAndSize();
                    }
                }
            }
        }

        internal Point location;
        public Point Location
        {
            get
            {
                return location;
            }
            set
            {
                if (value != location)
                {
                    location = value;
                    if (layoutPerformed)
                    {
                        ApplyLocationAndSize();
                    }
                }
            }
        }

        internal virtual Drawing.Size GetPreferredSizeCore(Drawing.Size proposedSize)
        {
            return CommonProperties.GetSpecifiedBounds(this).Size;
        }

        public void Show()
        {
            Visible = true;
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(x, y, width, height);
            }

            set
            {
                SetBounds(value.X, value.Y, value.Width, value.Height, BoundsSpecified.All);
            }
        }


        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual Rectangle DisplayRectangle
        {
            [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces")]
            get
            {
                return new Rectangle(0, 0, clientWidth, clientHeight);
            }
        }

        public bool ParticipatesInLayout => throw new NotImplementedException();

        PropertyStore IArrangedElement.Properties
        {
            get { return Properties; }
        }

        [Localizable(true)]
        [AmbientValue(typeof(Drawing.Size), "0, 0")]
        public virtual Drawing.Size MaximumSize
        {
            get { return CommonProperties.GetMaximumSize(this, DefaultMaximumSize); }
            set
            {
                if (value == Drawing.Size.Empty)
                {
                    CommonProperties.ClearMaximumSize(this);
                    Debug.Assert(MaximumSize == DefaultMaximumSize, "Error detected while resetting MaximumSize.");
                }
                else if (value != MaximumSize)
                {
                    // SetMaximumSize causes a layout as a side effect.
                    CommonProperties.SetMaximumSize(this, value);
                    Debug.Assert(MaximumSize == value, "Error detected while setting MaximumSize.");
                }
            }
        }

        [Localizable(true)]
        public virtual Drawing.Size MinimumSize
        {
            get { return CommonProperties.GetMinimumSize(this, DefaultMinimumSize); }
            set
            {
                if (value != MinimumSize)
                {
                    // SetMinimumSize causes a layout as a side effect.
                    CommonProperties.SetMinimumSize(this, value);
                }
                Debug.Assert(MinimumSize == value, "Error detected while setting MinimumSize.");
            }
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Rectangle ClientRectangle
        {
            get
            {
                return new Rectangle(0, 0, clientWidth, clientHeight);
            }
        }
        IArrangedElement IArrangedElement.Container
        {
            get
            {
                // This is safe because the IArrangedElement interface is internal
                return ParentInternal;
            }
        }

        ArrangedElementCollection IArrangedElement.Children
        {
            get
            {
                ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
                if (controlsCollection == null)
                {
                    return ArrangedElementCollection.Empty;
                }
                return controlsCollection;
            }
        }

        internal bool layoutPerformed = false;

        public virtual void SuspendLayout()
        {

        }

        public virtual void ResumeLayout(bool performLayout)
        {

        }

        private void ResetVisible()
        {
            Visible = true;
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool IsMirrored
        {
            get
            {
                if (!IsHandleCreated)
                {
                    //CreateParams cp = CreateParams;
                    //SetState(STATE_MIRRORED, (cp.ExStyle & NativeMethods.WS_EX_LAYOUTRTL) != 0);
                    SetState(STATE_MIRRORED, false);
                }
                return GetState(STATE_MIRRORED);
            }
        }

        public void ResumeLayout()
        {
            ResumeLayout(true);
        }

        public virtual void PerformLayout()
        {

        }

        async Task ApplyLocationAndSize()
        {
            string script = "";

            string left = "";
            string right = "";
            string top = "";
            string bottom = "";
            string width = "";
            string height = "";

            script += ScriptClearCssLocationAndSize();

            CalcLocationAndSize(ref left, ref right, ref top, ref bottom, ref width, ref height);

            if (left != "")
            {
                script += $"document.getElementById(\"{WebviewIdentifier}\").style.left=\"{left}\";";
            }
            if (right != "")
            {
                script += $"document.getElementById(\"{WebviewIdentifier}\").style.right=\"{right}\";";
            }
            if (top != "")
            {
                script += $"document.getElementById(\"{WebviewIdentifier}\").style.top=\"{top}\";";
            }
            if (bottom != "")
            {
                script += $"document.getElementById(\"{WebviewIdentifier}\").style.bottom=\"{bottom}\";";
            }
            if (height != "")
            {
                script += $"document.getElementById(\"{WebviewIdentifier}\").style.height=\"{height}\";";
            }
            if (width != "")
            {
                script += $"document.getElementById(\"{WebviewIdentifier}\").style.width=\"{width}\";";
            }
            await Page.RunScript(script);
        }

        internal string ScriptClearCssLocationAndSize()
        {
            string script = "";
            script += $"document.getElementById(\"{WebviewIdentifier}\").style.removeProperty('width');";
            script += $"document.getElementById(\"{WebviewIdentifier}\").style.removeProperty('height');";
            script += $"document.getElementById(\"{WebviewIdentifier}\").style.removeProperty('top');";
            script += $"document.getElementById(\"{WebviewIdentifier}\").style.removeProperty('bottom');";
            script += $"document.getElementById(\"{WebviewIdentifier}\").style.removeProperty('left');";
            script += $"document.getElementById(\"{WebviewIdentifier}\").style.removeProperty('right');";
            return script;
        }

        internal void CalcLocationAndSize(ref string left, ref string right, ref string top, ref string bottom, ref string width, ref string height)
        {
            if (((Anchor & AnchorStyles.Top) == AnchorStyles.Top) && ((Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
            {
                top = $"{Location.Y}px";
                if (Parent.GetType().IsSubclassOf(typeof(Form)) || Parent.GetType() == typeof(Form))
                {
                    bottom = $"{(Parent as Form).ClientSize.Height - Location.Y - Size.Height}px";
                }
                else
                {
                    bottom = $"{Parent.Size.Height - Location.Y - Size.Height}px";
                }
                height = $"auto";
            }
            else if ((Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
            {
                if (Parent.GetType().IsSubclassOf(typeof(Form)) || Parent.GetType() == typeof(Form))
                {
                    bottom = $"{(Parent as Form).ClientSize.Height - Location.Y - Size.Height}px";
                }
                else
                {
                    bottom = $"{Parent.Size.Height - Location.Y - Size.Height}px";
                }
                height = $"{Size.Height}px";
            }
            else
            {
                top = $"{Location.Y}px";
                height = $"{Size.Height}px";
            }

            if (((Anchor & AnchorStyles.Left) == AnchorStyles.Left) && ((Anchor & AnchorStyles.Right) == AnchorStyles.Right))
            {
                left = $"{Location.X}px;";
                if (Parent.GetType().IsSubclassOf(typeof(Form)) || Parent.GetType() == typeof(Form))
                {
                    right = $"{(Parent as Form).ClientSize.Width - Location.X - Size.Width}px";
                }
                else
                {
                    right = $"{Parent.Size.Width - Location.X - Size.Width}px";
                }
                width += $"auto";
            }
            else if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
            {
                if (Parent.GetType().IsSubclassOf(typeof(Form)) || Parent.GetType() == typeof(Form))
                {
                    right = $"{(Parent as Form).ClientSize.Width - Location.X - Size.Width}px";
                }
                else
                {
                    right = $"{Parent.Size.Width - Location.X - Size.Width}px";
                }
                width = $"{Size.Width}px";
            }
            else
            {
                left = $"{Location.X}px";
                width = $"{Size.Width}px";
            }
        }

        internal string CssLocationAndSize()
        {
            string left = "";
            string right = "";
            string top = "";
            string bottom = "";
            string width = "";
            string height = "";

            string style = "";

            CalcLocationAndSize(ref left, ref right, ref top, ref bottom, ref width, ref height);

            if (left != "")
            {
                style += $"left: {left};";
            }
            if (right != "")
            {
                style += $"right: {right};";
            }
            if (top != "")
            {
                style += $"top: {top};";
            }
            if (bottom != "")
            {
                style += $"bottom: {bottom};";
            }
            if (height != "")
            {
                style += $"height: {height};";
            }
            if (width != "")
            {
                style += $"width: {width};";
            }
            return style;
        }

        protected void PerformChildLayout()
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].WebviewIdentifier = WebviewIdentifier + "-"; 
                //Controls[i].Parent = this; // Custom removed, original code by Microsoft assign the Parent fine when adding in ControlCollection Controls
                Controls[i].PerformLayout();
            }
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            //if (this.x != x || this.y != y || this.width != width ||
            //    this.height != height)
            //{
            //    SetBoundsCore(x, y, width, height, BoundsSpecified.All);

            //    // WM_WINDOWPOSCHANGED will trickle down to an OnResize() which will
            //    // have refreshed the interior layout.  We only need to layout the parent.
            //    LayoutTransaction.DoLayout(ParentInternal, this, PropertyNames.Bounds);
            //}
            //else
            //{
            //    // Still need to init scaling.
            //    InitScaling(BoundsSpecified.All);
            //}
        }

        public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if ((specified & BoundsSpecified.X) == BoundsSpecified.None) x = this.x;
            if ((specified & BoundsSpecified.Y) == BoundsSpecified.None) y = this.y;
            if ((specified & BoundsSpecified.Width) == BoundsSpecified.None) width = this.width;
            if ((specified & BoundsSpecified.Height) == BoundsSpecified.None) height = this.height;
            if (this.x != x || this.y != y || this.width != width ||
                this.height != height)
            {
                SetBoundsCore(x, y, width, height, specified);

                // WM_WINDOWPOSCHANGED will trickle down to an OnResize() which will
                // have refreshed the interior layout or the resized control.  We only need to layout
                // the parent.  This happens after InitLayout has been invoked.
                LayoutTransaction.DoLayout(ParentInternal, this, PropertyNames.Bounds);
            }
            else
            {
                // Still need to init scaling.
                //InitScaling(specified);
            }
        }

        // GetPreferredSize and SetBoundsCore call this method to allow controls to self impose
        // constraints on their size.
        internal Size ApplySizeConstraints(Size proposedSize)
        {
            return ApplyBoundsConstraints(0, 0, proposedSize.Width, proposedSize.Height).Size;
        }

        internal virtual Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
        {

            // COMPAT: 529991 - in Everett we would allow you to set negative values in pre-handle mode
            // in Whidbey, if you've set Min/Max size we will constrain you to 0,0.  Everett apps didnt 
            // have min/max size on control, which is why this works.
            if (MaximumSize != Size.Empty || MinimumSize != Size.Empty)
            {
                Size maximumSize = LayoutUtils.ConvertZeroToUnbounded(MaximumSize);
                Rectangle newBounds = new Rectangle(suggestedX, suggestedY, 0, 0);

                // Clip the size to maximum and inflate it to minimum as necessary.
                newBounds.Size = LayoutUtils.IntersectSizes(new Size(proposedWidth, proposedHeight), maximumSize);
                newBounds.Size = LayoutUtils.UnionSizes(newBounds.Size, MinimumSize);

                return newBounds;
            }

            return new Rectangle(suggestedX, suggestedY, proposedWidth, proposedHeight);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // SetWindowPos below sends a WmWindowPositionChanged (not posts) so we immediately
            // end up in WmWindowPositionChanged which may cause the parent to layout.  We need to
            // suspend/resume to defer the parent from laying out until after InitLayout has been called
            // to update the layout engine's state with the new control bounds.
            if (ParentInternal != null)
            {
                ParentInternal.SuspendLayout();
            }
            try
            {
                if (this.x != x || this.y != y || this.width != width || this.height != height)
                {
                    CommonProperties.UpdateSpecifiedBounds(this, x, y, width, height, specified);

                    // Provide control with an opportunity to apply self imposed constraints on its size.
                    Rectangle adjustedBounds = ApplyBoundsConstraints(x, y, width, height);
                    width = adjustedBounds.Width;
                    height = adjustedBounds.Height;
                    x = adjustedBounds.X;
                    y = adjustedBounds.Y;

                    if (!IsHandleCreated)
                    {
                        // Handle is not created, just record our new position and we're done.
                        UpdateBounds(x, y, width, height);
                    }
                    else
                    {
                        if (!GetState(STATE_SIZELOCKEDBYOS))
                        {
                            int flags = NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE;

                            if (this.x == x && this.y == y)
                            {
                                flags |= NativeMethods.SWP_NOMOVE;
                            }
                            if (this.width == width && this.height == height)
                            {
                                flags |= NativeMethods.SWP_NOSIZE;
                            }

                            //
                            // Give a chance for derived controls to do what they want, just before we resize.
                            OnBoundsUpdate(x, y, width, height);

                            //SafeNativeMethods.SetWindowPos(new HandleRef(window, Handle), NativeMethods.NullHandleRef, x, y, width, height, flags);
                            this.x = x;
                            this.y = y;
                            this.width = width;
                            this.height = height;

                            if (layoutPerformed)
                            {
                                ApplyLocationAndSize();
                            }

                            // NOTE: SetWindowPos causes a WM_WINDOWPOSCHANGED which is processed
                            // synchonously so we effectively end up in UpdateBounds immediately following
                            // SetWindowPos.
                            //
                            //UpdateBounds(x, y, width, height);
                        }
                    }
                }
            }
            finally
            {

                // Initialize the scaling engine.
                //InitScaling(specified);

                if (ParentInternal != null)
                {
                    // Some layout engines (DefaultLayout) base their PreferredSize on
                    // the bounds of their children.  If we change change the child bounds, we
                    // need to clear their PreferredSize cache.  The semantics of SetBoundsCore
                    // is that it does not cause a layout, so we just clear.
                    CommonProperties.xClearPreferredSizeCache(ParentInternal);

                    // Cause the current control to initialize its layout (e.g., Anchored controls
                    // memorize their distance from their parent's edges).  It is your parent's
                    // LayoutEngine which manages your layout, so we call into the parent's
                    // LayoutEngine.
                    ParentInternal.LayoutEngine.InitLayout(this, specified);
                    ParentInternal.ResumeLayout( /* performLayout = */ true);
                }
            }
        }

        // Give a chance for derived controls to do what they want, just before we resize.
        internal virtual void OnBoundsUpdate(int x, int y, int width, int height)
        {
        }


        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal void UpdateBounds()
        {
            //NativeMethods.RECT rect = new NativeMethods.RECT();
            //UnsafeNativeMethods.GetClientRect(new HandleRef(window, InternalHandle), ref rect);
            //int clientWidth = rect.right;
            //int clientHeight = rect.bottom;
            //UnsafeNativeMethods.GetWindowRect(new HandleRef(window, InternalHandle), ref rect);
            //if (!GetTopLevel())
            //{
            //    UnsafeNativeMethods.MapWindowPoints(NativeMethods.NullHandleRef, new HandleRef(null, UnsafeNativeMethods.GetParent(new HandleRef(window, InternalHandle))), ref rect, 2);
            //}

            //UpdateBounds(rect.left, rect.top, rect.right - rect.left,
            //             rect.bottom - rect.top, clientWidth, clientHeight);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void UpdateBounds(int x, int y, int width, int height)
        {
            Debug.Assert(!IsHandleCreated, "Don't call this method when handle is created!!");

            // reverse-engineer the AdjustWindowRectEx call to figure out
            // the appropriate clientWidth and clientHeight
            NativeMethods.RECT rect = new NativeMethods.RECT();
            rect.left = rect.right = rect.top = rect.bottom = 0;

            //CreateParams cp = CreateParams;

            //AdjustWindowRectEx(ref rect, cp.Style, false, cp.ExStyle);
            int clientWidth = width - (rect.right - rect.left);
            int clientHeight = height - (rect.bottom - rect.top);
            UpdateBounds(x, y, width, height, clientWidth, clientHeight);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight)
        {
            bool newLocation = this.x != x || this.y != y;
            bool newSize = this.Width != width || this.Height != height ||
                           this.clientWidth != clientWidth || this.clientHeight != clientHeight;

            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.clientWidth = clientWidth;
            this.clientHeight = clientHeight;


            if (layoutPerformed)
            {
                ApplyLocationAndSize();
            }

            if (newLocation)
            {
                OnLocationChanged(EventArgs.Empty);
            }
            if (newSize)
            {
                OnSizeChanged(EventArgs.Empty);
                OnClientSizeChanged(EventArgs.Empty);

                // Clear PreferredSize cache for this control
                CommonProperties.xClearPreferredSizeCache(this);
                LayoutTransaction.DoLayout(ParentInternal, this, PropertyNames.Bounds);

            }
        }

        void IArrangedElement.SetBounds(Rectangle bounds, BoundsSpecified specified)
        {
            ISite site = Site;
            IComponentChangeService ccs = null;
            PropertyDescriptor sizeProperty = null;
            PropertyDescriptor locationProperty = null;
            bool sizeChanged = false;
            bool locationChanged = false;

            if (site != null && site.DesignMode)
            {
                ccs = (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
                if (ccs != null)
                {
                    sizeProperty = TypeDescriptor.GetProperties(this)[PropertyNames.Size];
                    locationProperty = TypeDescriptor.GetProperties(this)[PropertyNames.Location];
                    Debug.Assert(sizeProperty != null && locationProperty != null, "Error retrieving Size/Location properties on Control.");

                    try
                    {
                        if (sizeProperty != null && !sizeProperty.IsReadOnly && (bounds.Width != this.Width || bounds.Height != this.Height))
                        {
                            if (!(site is INestedSite))
                            {
                                ccs.OnComponentChanging(this, sizeProperty);
                            }
                            sizeChanged = true;
                        }
                        if (locationProperty != null && !locationProperty.IsReadOnly && (bounds.X != this.x || bounds.Y != this.y))
                        {
                            if (!(site is INestedSite))
                            {
                                ccs.OnComponentChanging(this, locationProperty);
                            }
                            locationChanged = true;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // The component change events can throw InvalidOperationException if a change is
                        // currently not allowed (typically because the doc data in VS is locked). 
                        // When this happens, we just eat the exception and proceed with the change.
                    }
                }
            }

            //SetBoundsCore(bounds.X, bounds.Y, bounds.Width, bounds.Height, specified);

            if (site != null && ccs != null)
            {
                try
                {
                    if (sizeChanged) ccs.OnComponentChanged(this, sizeProperty, null, null);
                    if (locationChanged) ccs.OnComponentChanged(this, locationProperty, null, null);
                }
                catch (InvalidOperationException)
                {
                    // The component change events can throw InvalidOperationException if a change is
                    // currently not allowed (typically because the doc data in VS is locked). 
                    // When this happens, we just eat the exception and proceed with the change.
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces")]
        public virtual Drawing.Size GetPreferredSize(Drawing.Size proposedSize)
        {
            Drawing.Size prefSize;

            if (GetState(STATE_DISPOSING | STATE_DISPOSED))
            {
                // if someone's asking when we're disposing just return what we last had.
                prefSize = CommonProperties.xGetPreferredSizeCache(this);
            }
            else
            {
                // Switch Size.Empty to maximum possible values
                proposedSize = LayoutUtils.ConvertZeroToUnbounded(proposedSize);

                // Force proposedSize to be within the elements constraints.  (This applies
                // minimumSize, maximumSize, etc.)
                proposedSize = ApplySizeConstraints(proposedSize);
                if (GetState2(STATE2_USEPREFERREDSIZECACHE))
                {
                    Drawing.Size cachedSize = CommonProperties.xGetPreferredSizeCache(this);

                    // If the "default" preferred size is being requested, and we have a cached value for it, return it.
                    // 
                    if (!cachedSize.IsEmpty && (proposedSize == LayoutUtils.MaxSize))
                    {
                        return cachedSize;
                    }
                }

                CacheTextInternal = true;
                try
                {
                    prefSize = GetPreferredSizeCore(proposedSize);
                }
                finally
                {
                    CacheTextInternal = false;
                }

                // There is no guarantee that GetPreferredSizeCore() return something within
                // proposedSize, so we apply the element's constraints again.
                prefSize = ApplySizeConstraints(prefSize);

                // If the "default" preferred size was requested, cache the computed value.
                // 
                if (GetState2(STATE2_USEPREFERREDSIZECACHE) && proposedSize == LayoutUtils.MaxSize)
                {
                    CommonProperties.xSetPreferredSizeCache(this, prefSize);
                }
            }
            return prefSize;
        }

        void IArrangedElement.PerformLayout(IArrangedElement affectedElement, string propertyName)
        {
            //throw new NotImplementedException();
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Always),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                SetBounds(x, y, value, height, BoundsSpecified.Width);
            }
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Always),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int Height
        {
            get
            {
                return width;
            }
            set
            {
                SetBounds(x, y, width, value, BoundsSpecified.Height);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnBindingContextChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (Properties.GetObject(PropBindings) != null)
            {
                //UpdateBindings();
            }

            EventHandler eh = Events[EventBindingContext] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnParentBindingContextChanged(e);
                }
            }
        }

        internal virtual void OnAutoSizeChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventAutoSizeChanged] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnBackColorChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetAnyDisposingInHierarchy())
            {
                return;
            }

            object backBrush = Properties.GetObject(PropBackBrush);
            if (backBrush != null)
            {
                if (GetState(STATE_OWNCTLBRUSH))
                {
                    IntPtr p = (IntPtr)backBrush;
                    if (p != IntPtr.Zero)
                    {
                        //SafeNativeMethods.DeleteObject(new HandleRef(this, p));
                    }
                }
                Properties.SetObject(PropBackBrush, null);
            }

            Invalidate();

            EventHandler eh = Events[EventBackColor] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnParentBackColorChanged(e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnBackgroundImageChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetAnyDisposingInHierarchy())
            {
                return;
            }

            Invalidate();

            EventHandler eh = Events[EventBackgroundImage] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnParentBackgroundImageChanged(e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnBackgroundImageLayoutChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetAnyDisposingInHierarchy())
            {
                return;
            }

            Invalidate();

            EventHandler eh = Events[EventBackgroundImageLayout] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnCausesValidationChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventCausesValidation] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        internal virtual void OnChildLayoutResuming(Control child, bool performLayout)
        {
            if (ParentInternal != null)
            {
                ParentInternal.OnChildLayoutResuming(child, performLayout);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnContextMenuChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventContextMenu] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnContextMenuStripChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventContextMenuStrip] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnCursorChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventCursor] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnParentCursorChanged(e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnDockChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventDock] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnEnabledChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetAnyDisposingInHierarchy())
            {
                return;
            }

            if (IsHandleCreated)
            {
                //SafeNativeMethods.EnableWindow(new HandleRef(this, Handle), Enabled);

                // User-paint controls should repaint when their enabled state changes
                //
                if (GetStyle(ControlStyles.UserPaint))
                {
                    Invalidate();
                    //Update();
                }
            }

            EventHandler eh = Events[EventEnabled] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnParentEnabledChanged(e);
                }
            }
        }

        // Refer VsWhidbey : 515910 & 269769
        internal virtual void OnFrameWindowActivate(bool fActivate)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnFontChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            // bail if disposing
            //
            if (GetAnyDisposingInHierarchy())
            {
                return;
            }

            Invalidate();

            if (Properties.ContainsInteger(PropFontHeight))
            {
                Properties.SetInteger(PropFontHeight, -1);
            }

            DisposeFontHandle();

            if (IsHandleCreated && !GetStyle(ControlStyles.UserPaint))
            {
                //SetWindowFont();
            }

            EventHandler eh = Events[EventFont] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            using (new LayoutTransaction(this, this, PropertyNames.Font, false))
            {
                if (controlsCollection != null)
                {
                    // This may have changed the sizes of our children.
                    // PERFNOTE: This is more efficient than using Foreach.  Foreach
                    // forces the creation of an array subset enum each time we
                    // enumerate
                    for (int i = 0; i < controlsCollection.Count; i++)
                    {
                        controlsCollection[i].OnParentFontChanged(e);
                    }
                }
            }

            LayoutTransaction.DoLayout(this, this, PropertyNames.Font);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnForeColorChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetAnyDisposingInHierarchy())
            {
                return;
            }

            Invalidate();

            EventHandler eh = Events[EventForeColor] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnParentForeColorChanged(e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnRightToLeftChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetAnyDisposingInHierarchy())
            {
                return;
            }

            // update the scroll position when the handle has been created
            // MUST SET THIS BEFORE CALLING RecreateHandle!!!
            SetState2(STATE2_SETSCROLLPOS, true);

            //RecreateHandle();

            EventHandler eh = Events[EventRightToLeft] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnParentRightToLeftChanged(e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnNotifyMessage(Message m)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnParentBackColorChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            Color backColor = Properties.GetColor(PropBackColor);
            if (backColor.IsEmpty)
            {
                OnBackColorChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnParentBackgroundImageChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            OnBackgroundImageChanged(e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnParentBindingContextChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (Properties.GetObject(PropBindingManager) == null)
            {
                OnBindingContextChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnParentCursorChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (Properties.GetObject(PropCursor) == null)
            {
                OnCursorChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnParentEnabledChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetState(STATE_ENABLED))
            {
                OnEnabledChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnParentFontChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (Properties.GetObject(PropFont) == null)
            {
                OnFontChanged(e);
            }
        }


        // occurs when the parent of this control has recreated
        // its handle.
        // presently internal as we feel this will be rare to
        // require override
        internal virtual void OnParentHandleRecreated()
        {
            // restore ourselves over to the original control.

            // use SetParent directly so as to not raise ParentChanged events
            Control parent = ParentInternal;
            if (parent != null)
            {
                if (IsHandleCreated)
                {
                    //UnsafeNativeMethods.SetParent(new HandleRef(this, this.Handle), new HandleRef(parent, parent.Handle));
                    //UpdateZOrder();
                }
            }
            SetState(STATE_PARENTRECREATING, false);

            //if (ReflectParent == ParentInternal)
            //{
            //    RecreateHandle();
            //}

        }


        // occurs when the parent of this control is recreating
        // its handle.
        // presently internal as we feel this will be rare to
        // require override
        internal virtual void OnParentHandleRecreating()
        {
            SetState(STATE_PARENTRECREATING, true);
            // swoop this control over to the parking window.

            // if we left it parented to the parent control, the DestroyWindow
            // would force us to destroy our handle as well... hopefully
            // parenting us over to the parking window and restoring ourselves
            // should help improve recreate perf.

            // use SetParent directly so as to not raise ParentChanged events
            if (IsHandleCreated)
            {
                //Application.ParkHandle(new HandleRef(this, this.Handle), this.DpiAwarenessContext);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentForeColorChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            Color foreColor = Properties.GetColor(PropForeColor);
            if (foreColor.IsEmpty)
            {
                OnForeColorChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentRightToLeftChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (!Properties.ContainsInteger(PropRightToLeft) || ((RightToLeft)Properties.GetInteger(PropRightToLeft)) == RightToLeft.Inherit)
            {
                OnRightToLeftChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentVisibleChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            if (GetState(STATE_VISIBLE))
            {
                OnVisibleChanged(e);
            }
        }

        // Used to work around VSWhidbey 527459. OnVisibleChanged/OnParentVisibleChanged is not called when a parent becomes invisible
        internal virtual void OnParentBecameInvisible()
        {
            if (GetState(STATE_VISIBLE))
            {
                // This control became invisible too - notify its children
                ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
                if (controlsCollection != null)
                {
                    for (int i = 0; i < controlsCollection.Count; i++)
                    {
                        Control ctl = controlsCollection[i];
                        ctl.OnParentBecameInvisible();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPrint(PaintEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            Contract.EndContractBlock();

            if (GetStyle(ControlStyles.UserPaint))
            {
                // Theme support on Windows XP requires that we paint the background
                // and foreground to support semi-transparent children
                //
                //PaintWithErrorHandling(e, PaintLayerBackground);
                //e.ResetGraphics();
                //PaintWithErrorHandling(e, PaintLayerForeground);
            }
            else
            {
                //PrintPaintEventArgs ppev = e as PrintPaintEventArgs;
                //Message m;
                //bool releaseDC = false;
                //IntPtr hdc = IntPtr.Zero;

                //if (ppev == null)
                //{
                //    IntPtr flags = (IntPtr)(NativeMethods.PRF_CHILDREN | NativeMethods.PRF_CLIENT | NativeMethods.PRF_ERASEBKGND | NativeMethods.PRF_NONCLIENT);
                //    hdc = e.HDC;

                //    if (hdc == IntPtr.Zero)
                //    {
                //        // a manually created paintevent args
                //        hdc = e.Graphics.GetHdc();
                //        releaseDC = true;
                //    }
                //    m = Message.Create(this.Handle, NativeMethods.WM_PRINTCLIENT, hdc, flags);
                //}
                //else
                //{
                //    m = ppev.Message;
                //}

                //try
                //{
                //    DefWndProc(ref m);
                //}
                //finally
                //{
                //    if (releaseDC)
                //    {
                //        e.Graphics.ReleaseHdcInternal(hdc);
                //    }
                //}
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnTabIndexChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventTabIndex] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnTabStopChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventTabStop] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnVisibleChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            bool visible = this.Visible;
            if (visible)
            {
                //UnhookMouseEvent();
                trackMouseEvent = null;
            }
            if (parent != null && visible && !Created)
            {
                bool isDisposing = GetAnyDisposingInHierarchy();
                if (!isDisposing)
                {
                    // Usually the control is created by now, but in a few corner cases
                    // exercised by the PropertyGrid dropdowns, it isn't
                    //CreateControl();
                }
            }

            EventHandler eh = Events[EventVisible] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    Control ctl = controlsCollection[i];
                    if (ctl.Visible)
                    {
                        ctl.OnParentVisibleChanged(e);
                    }
                    if (!visible)
                    {
                        ctl.OnParentBecameInvisible();
                    }
                }
            }
        }

        internal virtual void OnTopMostActiveXParentChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnTopMostActiveXParentChanged(e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnParentChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventParent] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }

            //
            // Inform the control that the topmost control is now an ActiveX control
            //if (TopMostParent.IsActiveX)
            //{
            //    OnTopMostActiveXParentChanged(EventArgs.Empty);
            //}
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnClientSizeChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventClientSize] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnControlAdded(ControlEventArgs e)
        {
            Contract.Requires(e != null);
            ControlEventHandler handler = (ControlEventHandler)Events[EventControlAdded];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnControlRemoved(ControlEventArgs e)
        {
            Contract.Requires(e != null);
            ControlEventHandler handler = (ControlEventHandler)Events[EventControlRemoved];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnCreateControl()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnHandleCreated(EventArgs e)
        {
            Contract.Requires(e != null);
            if (this.IsHandleCreated)
            {

                // Setting fonts is for some reason incredibly expensive.
                // (Even if you exclude font handle creation)
                if (!GetStyle(ControlStyles.UserPaint))
                {
                    //SetWindowFont();
                }

                //if (DpiHelper.EnableDpiChangedMessageHandling && !(typeof(Form).IsAssignableFrom(this.GetType())))
                //{
                //    int old = deviceDpi;
                //    deviceDpi = (int)UnsafeNativeMethods.GetDpiForWindow(new HandleRef(this, HandleInternal));
                //    if (old != deviceDpi)
                //    {
                //        RescaleConstantsForDpi(old, deviceDpi);
                //    }
                //}

                ////
                //SetAcceptDrops(AllowDrop);

                //Region region = (Region)Properties.GetObject(PropRegion);
                //if (region != null)
                //{
                //    IntPtr regionHandle = GetHRgn(region);

                //    try
                //    {
                //        if (IsActiveX)
                //        {
                //            regionHandle = ActiveXMergeRegion(regionHandle);
                //        }

                //        if (UnsafeNativeMethods.SetWindowRgn(new HandleRef(this, Handle), new HandleRef(this, regionHandle), SafeNativeMethods.IsWindowVisible(new HandleRef(this, Handle))) != 0)
                //        {
                //            //The HWnd owns the region.
                //            regionHandle = IntPtr.Zero;
                //        }
                //    }
                //    finally
                //    {
                //        if (regionHandle != IntPtr.Zero)
                //        {
                //            SafeNativeMethods.DeleteObject(new HandleRef(null, regionHandle));
                //        }
                //    }
                //}

                ////
                //ControlAccessibleObject accObj = Properties.GetObject(PropAccessibility) as ControlAccessibleObject;
                //ControlAccessibleObject ncAccObj = Properties.GetObject(PropNcAccessibility) as ControlAccessibleObject;

                // Cache Handle in a local before asserting so we minimize code running under the Assert.
                IntPtr handle = Handle;

                // Reviewed  : ControlAccessibleObject.set_Handle demands UnmanagedCode permission for public use, it doesn't
                //             expose any security vulnerability indirectly. The sec Assert is safe.
                //
                IntSecurity.UnmanagedCode.Assert();

                try
                {
                    //if (accObj != null)
                    //{
                    //    accObj.Handle = handle;
                    //}
                    //if (ncAccObj != null)
                    //{
                    //    ncAccObj.Handle = handle;
                    //}
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }

                // Set the window text from the Text property.
                //
                if (text != null && text.Length != 0)
                {
                    //UnsafeNativeMethods.SetWindowText(new HandleRef(this, Handle), text);
                }

                //if (!(this is ScrollableControl) && !IsMirrored && GetState2(STATE2_SETSCROLLPOS) && !GetState2(STATE2_HAVEINVOKED))
                //{
                //    BeginInvoke(new EventHandler(this.OnSetScrollPosition));
                //    SetState2(STATE2_HAVEINVOKED, true);
                //    SetState2(STATE2_SETSCROLLPOS, false);
                //}

                if (GetState2(STATE2_INTERESTEDINUSERPREFERENCECHANGED))
                {
                    //ListenToUserPreferenceChanged(GetTopLevel());
                }
            }

            EventHandler handler = (EventHandler)Events[EventHandleCreated];
            if (handler != null) handler(this, e);

            if (this.IsHandleCreated)
            {
                // Now, repost the thread callback message if we found it.  We should do
                // this last, so we're as close to the same state as when the message
                // was placed.
                //
                if (GetState(STATE_THREADMARSHALLPENDING))
                {
                    //UnsafeNativeMethods.PostMessage(new HandleRef(this, Handle), threadCallbackMessage, IntPtr.Zero, IntPtr.Zero);
                    SetState(STATE_THREADMARSHALLPENDING, false);
                }
            }
        }

        internal void OnSetScrollPosition(object sender, EventArgs e)
        {
            SetState2(STATE2_HAVEINVOKED, false);
            OnInvokedSetScrollPosition(sender, e);
        }

        internal virtual void OnInvokedSetScrollPosition(object sender, EventArgs e)
        {
            //if (!(this is ScrollableControl) && !IsMirrored)
            //{
            //    NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO();
            //    si.cbSize = Marshal.SizeOf(typeof(NativeMethods.SCROLLINFO));
            //    si.fMask = NativeMethods.SIF_RANGE;
            //    if (UnsafeNativeMethods.GetScrollInfo(new HandleRef(this, Handle), NativeMethods.SB_HORZ, si) != false)
            //    {
            //        si.nPos = (RightToLeft == RightToLeft.Yes) ? si.nMax : si.nMin;
            //        SendMessage(NativeMethods.WM_HSCROLL, NativeMethods.Util.MAKELPARAM(NativeMethods.SB_THUMBPOSITION, si.nPos), 0);
            //    }
            //}
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnHandleDestroyed(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventHandleDestroyed];
            if (handler != null) handler(this, e);

            //UpdateReflectParent(false);

            //if (!RecreatingHandle)
            //{
            //    if (GetState(STATE_OWNCTLBRUSH))
            //    {
            //        object backBrush = Properties.GetObject(PropBackBrush);
            //        if (backBrush != null)
            //        {
            //            IntPtr p = (IntPtr)backBrush;
            //            if (p != IntPtr.Zero)
            //            {
            //                SafeNativeMethods.DeleteObject(new HandleRef(this, p));
            //            }
            //            Properties.SetObject(PropBackBrush, null);
            //        }
            //    }
            //    ListenToUserPreferenceChanged(false /*listen*/);
            //}

            ////

            //try
            //{
            //    if (!GetAnyDisposingInHierarchy())
            //    {
            //        text = Text;
            //        if (text != null && text.Length == 0) text = null;
            //    }
            //    SetAcceptDrops(false);
            //}
            //catch (Exception ex)
            //{
            //    if (ClientUtils.IsSecurityOrCriticalException(ex))
            //    {
            //        throw;
            //    }

            //    // Some ActiveX controls throw exceptions when
            //    // you ask for the text property after you have destroyed their handle. We
            //    // don't want those exceptions to bubble all the way to the top, since
            //    // we leave our state in a mess. See ASURT 49990.
            //    //
            //}
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnDoubleClick(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventDoubleClick];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnDragEnter(DragEventArgs drgevent)
        {
            Contract.Requires(drgevent != null);
            DragEventHandler handler = (DragEventHandler)Events[EventDragEnter];
            if (handler != null) handler(this, drgevent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnDragOver(DragEventArgs drgevent)
        {
            Contract.Requires(drgevent != null);
            DragEventHandler handler = (DragEventHandler)Events[EventDragOver];
            if (handler != null) handler(this, drgevent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnDragLeave(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventDragLeave];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnDragDrop(DragEventArgs drgevent)
        {
            Contract.Requires(drgevent != null);
            DragEventHandler handler = (DragEventHandler)Events[EventDragDrop];
            if (handler != null) handler(this, drgevent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces")]
        internal virtual void OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            Contract.Requires(gfbevent != null);
            GiveFeedbackEventHandler handler = (GiveFeedbackEventHandler)Events[EventGiveFeedback];
            if (handler != null) handler(this, gfbevent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnEnter(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventEnter];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal void InvokeGotFocus(Control toInvoke, EventArgs e)
        {
            if (toInvoke != null)
            {
                toInvoke.OnGotFocus(e);
                //if (!AccessibilityImprovements.UseLegacyToolTipDisplay)
                //{
                //    KeyboardToolTipStateMachine.Instance.NotifyAboutGotFocus(toInvoke);
                //}
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnGotFocus(EventArgs e)
        {
            Contract.Requires(e != null);
            //if (IsActiveX)
            //{
            //    ActiveXOnFocus(true);
            //}

            if (parent != null)
            {
                //parent.ChildGotFocus(this);
            }

            EventHandler handler = (EventHandler)Events[EventGotFocus];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnHelpRequested(HelpEventArgs hevent)
        {
            Contract.Requires(hevent != null);
            HelpEventHandler handler = (HelpEventHandler)Events[EventHelpRequested];
            if (handler != null)
            {
                handler(this, hevent);
                // VSWhidbey 95281: Set this to true so that multiple events aren't raised to the Form.
                hevent.Handled = true;
            }

            if (!hevent.Handled)
            {
                if (ParentInternal != null)
                {
                    ParentInternal.OnHelpRequested(hevent);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnInvalidated(InvalidateEventArgs e)
        {
            Contract.Requires(e != null);
            // Ask the site to change the view...
            //if (IsActiveX)
            //{
            //    ActiveXViewChanged();
            //}

            // Transparent control support
            ControlCollection controls = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controls != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controls.Count; i++)
                {
                    controls[i].OnParentInvalidated(e);
                }
            }

            InvalidateEventHandler handler = (InvalidateEventHandler)Events[EventInvalidated];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnKeyDown(KeyEventArgs e)
        {
            Contract.Requires(e != null);
            KeyEventHandler handler = (KeyEventHandler)Events[EventKeyDown];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnKeyPress(KeyPressEventArgs e)
        {
            Contract.Requires(e != null);
            KeyPressEventHandler handler = (KeyPressEventHandler)Events[EventKeyPress];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnKeyUp(KeyEventArgs e)
        {
            Contract.Requires(e != null);
            KeyEventHandler handler = (KeyEventHandler)Events[EventKeyUp];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnLayout(LayoutEventArgs levent)
        {
            Contract.Requires(levent != null);
            // Ask the site to change the view...
            //if (IsActiveX)
            //{
            //    ActiveXViewChanged();
            //}
            LayoutEventHandler handler = (LayoutEventHandler)Events[EventLayout];
            if (handler != null) handler(this, levent);

            bool parentRequiresLayout = LayoutEngine.Layout(this, levent);

            if (parentRequiresLayout && ParentInternal != null)
            {
                // LayoutEngine.Layout can return true to request that our parent resize us because
                // we did not have enough room for our contents.  We can not just call PerformLayout
                // because this container is currently suspended.  PerformLayout will check this state
                // flag and PerformLayout on our parent.
                ParentInternal.SetState(STATE_LAYOUTISDIRTY, true);
            }
        }

        internal virtual void OnLayoutResuming(bool performLayout)
        {
            if (ParentInternal != null)
            {
                ParentInternal.OnChildLayoutResuming(this, performLayout);
            }
        }

        internal virtual void OnLayoutSuspended()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnLeave(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventLeave];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal void InvokeLostFocus(Control toInvoke, EventArgs e)
        {
            if (toInvoke != null)
            {
                toInvoke.OnLostFocus(e);
                //if (!AccessibilityImprovements.UseLegacyToolTipDisplay)
                //{
                //    KeyboardToolTipStateMachine.Instance.NotifyAboutLostFocus(toInvoke);
                //}
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnLostFocus(EventArgs e)
        {
            Contract.Requires(e != null);
            //if (IsActiveX)
            //{
            //    ActiveXOnFocus(false);
            //}

            EventHandler handler = (EventHandler)Events[EventLostFocus];
            if (handler != null) handler(this, e);
        }

        internal virtual void OnMarginChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventMarginChanged];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            Contract.Requires(e != null);
            MouseEventHandler handler = (MouseEventHandler)Events[EventMouseDoubleClick];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseClick(MouseEventArgs e)
        {
            Contract.Requires(e != null);
            MouseEventHandler handler = (MouseEventHandler)Events[EventMouseClick];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseCaptureChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventMouseCaptureChanged];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseDown(MouseEventArgs e)
        {
            Contract.Requires(e != null);
            MouseEventHandler handler = (MouseEventHandler)Events[EventMouseDown];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseEnter(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventMouseEnter];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseLeave(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventMouseLeave];
            if (handler != null) handler(this, e);
        }

        [
            Browsable(true),
            EditorBrowsable(EditorBrowsableState.Always)
        ]
        internal virtual void OnDpiChangedBeforeParent(EventArgs e)
        {
            Contract.Requires(e != null);
            ((EventHandler)Events[EventDpiChangedBeforeParent])?.Invoke(this, e);
        }

        [
            Browsable(true),
            EditorBrowsable(EditorBrowsableState.Always)
        ]
        internal virtual void OnDpiChangedAfterParent(EventArgs e)
        {
            Contract.Requires(e != null);
            ((EventHandler)Events[EventDpiChangedAfterParent])?.Invoke(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseHover(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventMouseHover];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseMove(MouseEventArgs e)
        {
            Contract.Requires(e != null);
            MouseEventHandler handler = (MouseEventHandler)Events[EventMouseMove];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseUp(MouseEventArgs e)
        {
            Contract.Requires(e != null);
            MouseEventHandler handler = (MouseEventHandler)Events[EventMouseUp];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnMouseWheel(MouseEventArgs e)
        {
            Contract.Requires(e != null);
            MouseEventHandler handler = (MouseEventHandler)Events[EventMouseWheel];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnPaint(PaintEventArgs e)
        {
            Contract.Requires(e != null);
            PaintEventHandler handler = (PaintEventHandler)Events[EventPaint];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnPaintBackground(PaintEventArgs pevent)
        {
            Contract.Requires(pevent != null);
            // We need the true client rectangle as clip rectangle causes
            // problems on "Windows Classic" theme.  
            NativeMethods.RECT rect = new NativeMethods.RECT();
            //UnsafeNativeMethods.GetClientRect(new HandleRef(window, InternalHandle), ref rect);

            //PaintBackground(pevent, new Rectangle(rect.left, rect.top, rect.right, rect.bottom));
        }

        // Transparent control support
        private void OnParentInvalidated(InvalidateEventArgs e)
        {
            Contract.Requires(e != null);
            //if (!RenderTransparent) return;

            if (this.IsHandleCreated)
            {
                // move invalid rect into child space
                Rectangle cliprect = e.InvalidRect;
                Point offs = this.Location;
                cliprect.Offset(-offs.X, -offs.Y);
                cliprect = Rectangle.Intersect(this.ClientRectangle, cliprect);

                // if we don't intersect at all, do nothing
                if (cliprect.IsEmpty) return;
                //this.Invalidate(cliprect);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces")]
        protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            Contract.Requires(qcdevent != null);
            QueryContinueDragEventHandler handler = (QueryContinueDragEventHandler)Events[EventQueryContinueDrag];
            if (handler != null) handler(this, qcdevent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRegionChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler eh = Events[EventRegionChanged] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [
        EditorBrowsable(EditorBrowsableState.Advanced),
        SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode),
        SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode),
        SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")
        ]
        protected virtual void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            Contract.Requires(e != null);
            PreviewKeyDownEventHandler handler = (PreviewKeyDownEventHandler)Events[EventPreviewKeyDown];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnSizeChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            OnResize(EventArgs.Empty);

            EventHandler eh = Events[EventSize] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnChangeUICues(UICuesEventArgs e)
        {
            Contract.Requires(e != null);
            UICuesEventHandler handler = (UICuesEventHandler)Events[EventChangeUICues];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnStyleChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventStyleChanged];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnSystemColorsChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            if (controlsCollection != null)
            {
                // PERFNOTE: This is more efficient than using Foreach.  Foreach
                // forces the creation of an array subset enum each time we
                // enumerate
                for (int i = 0; i < controlsCollection.Count; i++)
                {
                    controlsCollection[i].OnSystemColorsChanged(EventArgs.Empty);
                }
            }
            Invalidate();

            EventHandler handler = (EventHandler)Events[EventSystemColorsChanged];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnValidating(CancelEventArgs e)
        {
            Contract.Requires(e != null);
            CancelEventHandler handler = (CancelEventHandler)Events[EventValidating];
            if (handler != null) handler(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnValidated(EventArgs e)
        {
            Contract.Requires(e != null);
            EventHandler handler = (EventHandler)Events[EventValidated];
            if (handler != null) handler(this, e);
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
        protected virtual bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Alt) == Keys.Alt) return false;
            int mask = NativeMethods.DLGC_WANTALLKEYS;
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Tab:
                    mask = NativeMethods.DLGC_WANTALLKEYS | NativeMethods.DLGC_WANTTAB;
                    break;
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    mask = NativeMethods.DLGC_WANTALLKEYS | NativeMethods.DLGC_WANTARROWS;
                    break;
            }

            if (IsHandleCreated)
            {
                //return (unchecked((int)(long)SendMessage(NativeMethods.WM_GETDLGCODE, 0, 0)) & mask) != 0;
                return false;
            }
            else
            {
                return false;
            }
        }

        internal virtual string WindowText
        {
            get
            {

                if (!IsHandleCreated)
                {
                    if (text == null)
                    {
                        return "";
                    }
                    else
                    {
                        return text;
                    }
                }

                using (new MultithreadSafeCallScope())
                {

                    // it's okay to call GetWindowText cross-thread.
                    //
                    int textLen = 0;
                    //int textLen = SafeNativeMethods.GetWindowTextLength(new HandleRef(window, Handle));

                    // Check to see if the system supports DBCS character
                    // if so, double the length of the buffer.
                    if (SystemInformation.DbcsEnabled)
                    {
                        textLen = (textLen * 2) + 1;
                    }
                    StringBuilder sb = new StringBuilder(textLen + 1);
                    //UnsafeNativeMethods.GetWindowText(new HandleRef(window, Handle), sb, sb.Capacity);
                    return sb.ToString();
                }
            }
            set
            {
                if (value == null) value = "";
                if (!WindowText.Equals(value))
                {
                    if (IsHandleCreated)
                    {
                        //UnsafeNativeMethods.SetWindowText(new HandleRef(window, Handle), value);
                    }
                    else
                    {
                        if (value.Length == 0)
                        {
                            text = null;
                        }
                        else
                        {
                            text = value;
                        }
                    }
                }
            }
        }

        private sealed class MultithreadSafeCallScope : IDisposable
        {
            // Use local stack variable rather than a refcount since we're
            // guaranteed that these 'scopes' are properly nested.
            private bool resultedInSet;

            internal MultithreadSafeCallScope()
            {
                // Only access the thread-local stuff if we're going to be
                // checking for illegal thread calling (no need to incur the
                // expense otherwise).
                if (checkForIllegalCrossThreadCalls && !inCrossThreadSafeCall)
                {
                    inCrossThreadSafeCall = true;
                    resultedInSet = true;
                }
                else
                {
                    resultedInSet = false;
                }
            }

            void IDisposable.Dispose()
            {
                if (resultedInSet)
                {
                    inCrossThreadSafeCall = false;
                }
            }
        }


        [
        Localizable(true),
        Bindable(true),
        DispId(NativeMethods.ActiveX.DISPID_TEXT),
        ]
        public virtual string Text
        {
            get
            {
                if (CacheTextInternal)
                {
                    return (text == null) ? "" : text;
                }
                else
                {
                    return WindowText;
                }
            }

            set
            {
                if (value == null)
                {
                    value = "";
                }

                if (value == Text)
                {
                    return;
                }

                if (CacheTextInternal)
                {
                    text = value;
                }
                WindowText = value;
                OnTextChanged(EventArgs.Empty);

                //if (this.IsMnemonicsListenerAxSourced)
                //{
                //    for (Control ctl = this; ctl != null; ctl = ctl.ParentInternal)
                //    {
                //        ActiveXImpl activeXImpl = (ActiveXImpl)ctl.Properties.GetObject(PropActiveXImpl);
                //        if (activeXImpl != null)
                //        {
                //            activeXImpl.UpdateAccelTable();
                //            break;
                //        }
                //    }
                //}

            }
        }

        internal bool CacheTextInternal
        {
            get
            {

                // check if we're caching text.
                //
                bool found;
                int cacheTextCounter = Properties.GetInteger(PropCacheTextCount, out found);

                return cacheTextCounter > 0 || GetStyle(ControlStyles.CacheText);
            }
            set
            {

                // if this control always cachest text or the handle hasn't been created,
                // just bail.
                //
                if (GetStyle(ControlStyles.CacheText) || !IsHandleCreated)
                {
                    return;
                }

                // otherwise, get the state and update the cache if necessary.
                //
                bool found;
                int cacheTextCounter = Properties.GetInteger(PropCacheTextCount, out found);

                if (value)
                {
                    if (cacheTextCounter == 0)
                    {
                        Properties.SetObject(PropCacheTextField, text);
                        if (text == null)
                        {
                            text = WindowText;
                        }
                    }
                    cacheTextCounter++;
                }
                else
                {
                    cacheTextCounter--;
                    if (cacheTextCounter == 0)
                    {
                        text = (string)Properties.GetObject(PropCacheTextField, out found);
                    }
                }
                Properties.SetInteger(PropCacheTextCount, cacheTextCounter);
            }
        }


        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool RecreatingHandle
        {
            get
            {
                return (state & STATE_RECREATE) != 0;
            }
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool Created
        {
            get
            {
                return (state & STATE_CREATED) != 0;
            }
        }

        protected virtual Drawing.Size DefaultSize
        {
            get { return Drawing.Size.Empty; }
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool IsHandleCreated
        {
            //get { return window.Handle != IntPtr.Zero; }
            get { return !string.IsNullOrEmpty(WebviewIdentifier); }
        }

        protected virtual ImeMode DefaultImeMode
        {
            get { return ImeMode.Inherit; }
        }

        internal int DisableImeModeChangedCount
        {
            get
            {
                Debug.WriteLineIf(CompModSwitches.ImeMode.Level >= TraceLevel.Info, "Inside get_DisableImeModeChangedCount()");
                Debug.Indent();

                bool dummy;
                int val = (int)Properties.GetInteger(PropDisableImeModeChangedCount, out dummy);

                Debug.Assert(val >= 0, "Counter underflow.");
                Debug.WriteLineIf(CompModSwitches.ImeMode.Level >= TraceLevel.Info, "Value: " + val);
                Debug.Unindent();

                return val;
            }
            set
            {
                Debug.WriteLineIf(CompModSwitches.ImeMode.Level >= TraceLevel.Info, "Inside set_DisableImeModeChangedCount(): " + value);
                Properties.SetInteger(PropDisableImeModeChangedCount, value);
            }
        }

        internal static readonly TraceSwitch ControlKeyboardRouting;
        internal static readonly TraceSwitch PaletteTracing;
        internal static readonly TraceSwitch FocusTracing;

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeSize()
        {
            // VSWhidbey 379287: In Whidbey the ControlDesigner class will always serialize size as it replaces the Size
            // property descriptor with its own.  This is here for compat.                    
            Drawing.Size s = DefaultSize;
            return width != s.Width || height != s.Height;
        }

        [
        Localizable(true),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(CommonProperties.DefaultDock),
        ]
        public virtual DockStyle Dock
        {
            get
            {
                return DefaultLayout.GetDock(this);
            }
            set
            {
                if (value != Dock)
                {         
                    SuspendLayout();
                    try
                    {
                        DefaultLayout.SetDock(this, value);
                        OnDockChanged(EventArgs.Empty);
                    }
                    finally
                    {
                        ResumeLayout();
                    }
                }
            }
        }

        internal virtual bool SupportsUiaProviders
        {
            get
            {
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void UpdateStyles()
        {
            //UpdateStylesCore();

            OnStyleChanged(EventArgs.Empty);
        }

        [
        DefaultValue(true),
        DispId(NativeMethods.ActiveX.DISPID_TABSTOP),
        ]
        public bool TabStop
        {
            get
            {
                return TabStopInternal;
            }
            set
            {
                if (TabStop != value)
                {
                    TabStopInternal = value;
                    //if (IsHandleCreated) SetWindowStyle(NativeMethods.WS_TABSTOP, value);
                    OnTabStopChanged(EventArgs.Empty);
                }
            }
        }

        // Grab out the logical of setting TABSTOP state, so that derived class could use this.
        internal bool TabStopInternal
        {
            get
            {
                return (state & STATE_TABSTOP) != 0;
            }
            set
            {
                if (TabStopInternal != value)
                {
                    SetState(STATE_TABSTOP, value);
                }
            }
        }

        protected void SetAutoSizeMode(AutoSizeMode mode)
        {
            CommonProperties.SetAutoSizeMode(this, mode);
        }

        protected AutoSizeMode GetAutoSizeMode()
        {
            return CommonProperties.GetAutoSizeMode(this);
        }

        public virtual LayoutEngine LayoutEngine
        {
            get { return DefaultLayout.Instance; }
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        internal virtual bool ProcessDialogKey(Keys keyData)
        {
            Debug.WriteLineIf(ControlKeyboardRouting.TraceVerbose, "Control.ProcessDialogKey " + keyData.ToString());
            return parent == null ? false : parent.ProcessDialogKey(keyData);
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool IsDisposed
        {
            get
            {
                return GetState(STATE_DISPOSED);
            }
        }

        protected bool GetTopLevel()
        {
            return (state & STATE_TOPLEVEL) != 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void UpdateZOrder()
        {
            if (parent != null)
            {
                parent.UpdateChildZOrder(this);
            }
        }

        private void UpdateChildZOrder(Control ctl)
        {
            if (!IsHandleCreated || !ctl.IsHandleCreated || ctl.parent != this) return;
            IntPtr prevHandle = (IntPtr)NativeMethods.HWND_TOP;
            for (int i = this.Controls.GetChildIndex(ctl); --i >= 0;)
            {
                Control c = Controls[i];
                if (c.IsHandleCreated && c.parent == this)
                {
                    prevHandle = c.Handle;
                    break;
                }
            }
            //if (UnsafeNativeMethods.GetWindow(new HandleRef(ctl.window, ctl.Handle), NativeMethods.GW_HWNDPREV) != prevHandle)
            //{
            //    state |= STATE_NOZORDER;
            //    try
            //    {
            //        SafeNativeMethods.SetWindowPos(new HandleRef(ctl.window, ctl.Handle), new HandleRef(null, prevHandle), 0, 0, 0, 0,
            //                                       NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
            //    }
            //    finally
            //    {
            //        state &= ~STATE_NOZORDER;
            //    }
            //}
        }

        internal bool PerformContainerValidation(ValidationConstraints validationConstraints)
        {
            bool failed = false;

            // For every child control of this container control...
            foreach (Control c in Controls)
            {

                // First, if the control is a container, recurse into its descendants.
                if ((validationConstraints & ValidationConstraints.ImmediateChildren) != ValidationConstraints.ImmediateChildren &&
                    c.ShouldPerformContainerValidation() &&
                    c.PerformContainerValidation(validationConstraints))
                {
                    failed = true;
                }

                // Next, use input flags to decide whether to validate the control itself
                if ((validationConstraints & ValidationConstraints.Selectable) == ValidationConstraints.Selectable && !c.GetStyle(ControlStyles.Selectable) ||
                    (validationConstraints & ValidationConstraints.Enabled) == ValidationConstraints.Enabled && !c.Enabled ||
                    (validationConstraints & ValidationConstraints.Visible) == ValidationConstraints.Visible && !c.Visible ||
                    (validationConstraints & ValidationConstraints.TabStop) == ValidationConstraints.TabStop && !c.TabStop)
                {
                    continue;
                }

                // Finally, perform validation on the control itself
                if (c.PerformControlValidation(true))
                {
                    failed = true;
                }
            }

            return failed;
        }

        internal static AutoValidate GetAutoValidateForControl(Control control)
        {
            ContainerControl parent = control.ParentContainerControl;
            return (parent != null) ? parent.AutoValidate : AutoValidate.EnablePreventFocusChange;
        }

        internal ContainerControl ParentContainerControl
        {
            get
            {
                for (Control c = this.ParentInternal; c != null; c = c.ParentInternal)
                {
                    if (c is ContainerControl)
                    {
                        return c as ContainerControl;
                    }
                }

                return null;
            }
        }

        // Used by form to notify the control that it has been "entered"
        //
        internal void NotifyEnter()
        {
            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "Control::NotifyEnter() - " + this.Name);
            OnEnter(EventArgs.Empty);
        }

        // Used by form to notify the control that it has been "left"
        //
        internal void NotifyLeave()
        {
            Debug.WriteLineIf(Control.FocusTracing.TraceVerbose, "Control::NotifyLeave() - " + this.Name);
            OnLeave(EventArgs.Empty);
        }

        [UIPermission(SecurityAction.Demand, Window = UIPermissionWindow.AllWindows)]
        public Form FindForm()
        {
            return FindFormInternal();
        }

        // SECURITY WARNING: This method bypasses a security demand. Use with caution!
        internal Form FindFormInternal()
        {
            Control cur = this;
            while (cur != null && !(cur is Form))
            {
                cur = cur.ParentInternal;
            }
            return (Form)cur;
        }

        internal bool IsTopMdiWindowClosing
        {
            set
            {
                SetState2(STATE2_TOPMDIWINDOWCLOSING, value);
            }
            get
            {
                return GetState2(STATE2_TOPMDIWINDOWCLOSING);
            }
        }

        internal bool IsLayoutSuspended
        {
            get
            {
                return layoutSuspendCount > 0;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool ShouldAutoValidate
        {
            get
            {
                return GetAutoValidateForControl(this) != AutoValidate.Disable;
            }
        }

        internal virtual bool ShouldPerformContainerValidation()
        {
            return GetStyle(ControlStyles.ContainerControl);
        }

        [
        DefaultValue(false),
        EditorBrowsable(EditorBrowsableState.Always),
        Browsable(true),
        ]
        public bool UseWaitCursor
        {
            get { return GetState(STATE_USEWAITCURSOR); }
            set
            {
                if (GetState(STATE_USEWAITCURSOR) != value)
                {
                    SetState(STATE_USEWAITCURSOR, value);
                    ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);

                    if (controlsCollection != null)
                    {
                        // PERFNOTE: This is more efficient than using Foreach.  Foreach
                        // forces the creation of an array subset enum each time we
                        // enumerate
                        for (int i = 0; i < controlsCollection.Count; i++)
                        {
                            controlsCollection[i].UseWaitCursor = value;
                        }
                    }
                }
            }
        }

        internal bool RequiredScalingEnabled
        {
            get
            {
                return (requiredScaling & RequiredScalingEnabledMask) != 0;
            }
            set
            {
                byte scaling = (byte)(requiredScaling & RequiredScalingMask);
                requiredScaling = scaling;
                if (value) requiredScaling |= RequiredScalingEnabledMask;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual ControlCollection CreateControlsInstance()
        {
            return new System.Windows.Forms.Control.ControlCollection(this);
        }

        public void SendToBack()
        {
            if (parent != null)
            {
                parent.Controls.SetChildIndex(this, -1);
            }
            else if (IsHandleCreated && GetTopLevel())
            {
                //SafeNativeMethods.SetWindowPos(new HandleRef(window, Handle), NativeMethods.HWND_BOTTOM, 0, 0, 0, 0,
                //                               NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
            }

        }
        internal static void CheckParentingCycle(Control bottom, Control toFind)
        {
            Form lastOwner = null;
            Control lastParent = null;

            for (Control ctl = bottom; ctl != null; ctl = ctl.ParentInternal)
            {
                lastParent = ctl;
                if (ctl == toFind)
                {
                    throw new ArgumentException("CircularOwner");
                }
            }

            if (lastParent != null)
            {
                if (lastParent is Form)
                {
                    Form f = (Form)lastParent;
                    for (Form form = f; form != null; form = form.OwnerInternal)
                    {
                        lastOwner = form;
                        if (form == toFind)
                        {
                            throw new ArgumentException("CircularOwner");
                        }
                    }
                }
            }

            if (lastOwner != null)
            {
                if (lastOwner.ParentInternal != null)
                {
                    CheckParentingCycle(lastOwner.ParentInternal, toFind);
                }
            }
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool Disposing
        {
            get
            {
                return GetState(STATE_DISPOSING);
            }
        }

        internal bool GetAnyDisposingInHierarchy()
        {
            Control up = this;
            bool isDisposing = false;
            while (up != null)
            {
                if (up.Disposing)
                {
                    isDisposing = true;
                    break;
                }
                up = up.parent;
            }
            return isDisposing;
        }

        [
        Localizable(true),
        AmbientValue(RightToLeft.Inherit),
        ]
        public virtual RightToLeft RightToLeft
        {
            get
            {
                bool found;
                int rightToLeft = Properties.GetInteger(PropRightToLeft, out found);
                if (!found)
                {
                    rightToLeft = (int)RightToLeft.Inherit;
                }

                if (((RightToLeft)rightToLeft) == RightToLeft.Inherit)
                {
                    Control parent = ParentInternal;
                    if (parent != null)
                    {
                        rightToLeft = (int)parent.RightToLeft;
                    }
                    else
                    {
                        rightToLeft = (int)DefaultRightToLeft;
                    }
                }
                return (RightToLeft)rightToLeft;
            }

            set
            {
                //valid values are 0x0 to 0x2.
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)RightToLeft.No, (int)RightToLeft.Inherit))
                {
                    throw new InvalidEnumArgumentException("RightToLeft", (int)value, typeof(RightToLeft));
                }

                RightToLeft oldValue = RightToLeft;

                if (Properties.ContainsInteger(PropRightToLeft) || value != RightToLeft.Inherit)
                {
                    Properties.SetInteger(PropRightToLeft, (int)value);
                }

                if (oldValue != RightToLeft)
                {
                    // Setting RTL on a container does not cause the container to change size.
                    // Only the children need to have thier layout updated.
                    using (new LayoutTransaction(this, this, PropertyNames.RightToLeft))
                    {
                        OnRightToLeftChanged(EventArgs.Empty);
                    }
                }
            }
        }

        private AmbientProperties AmbientPropertiesService
        {
            get
            {
                bool contains;
                AmbientProperties props = (AmbientProperties)Properties.GetObject(PropAmbientPropertiesService, out contains);
                if (!contains)
                {
                    if (Site != null)
                    {
                        props = (AmbientProperties)Site.GetService(typeof(AmbientProperties));
                    }
                    else
                    {
                        props = (AmbientProperties)GetService(typeof(AmbientProperties));
                    }

                    if (props != null)
                    {
                        Properties.SetObject(PropAmbientPropertiesService, props);
                    }
                }
                return props;
            }
        }

        private Font GetParentFont()
        {
            if (ParentInternal != null && ParentInternal.CanAccessProperties)
                return ParentInternal.Font;
            else
                return null;
        }

        private void DisposeFontHandle()
        {
            if (Properties.ContainsObject(PropFontHandleWrapper))
            {
                FontHandleWrapper fontHandle = Properties.GetObject(PropFontHandleWrapper) as FontHandleWrapper;
                if (fontHandle != null)
                {
                    fontHandle.Dispose();
                }
                Properties.SetObject(PropFontHandleWrapper, null);
            }
        }

        internal virtual bool CanAccessProperties
        {
            get
            {
                return true;
            }
        }

        [
        Localizable(true),
        DispId(NativeMethods.ActiveX.DISPID_FONT),
        AmbientValue(null),
        ]
        public virtual Font Font
        {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Font font = (Font)Properties.GetObject(PropFont);
                if (font != null)
                {
                    return font;
                }

                Font f = GetParentFont();
                if (f != null)
                {
                    return f;
                }

                //if (IsActiveX)
                //{
                //    f = ActiveXAmbientFont;
                //    if (f != null)
                //    {
                //        return f;
                //    }
                //}

                AmbientProperties ambient = AmbientPropertiesService;
                if (ambient != null && ambient.Font != null)
                {
                    return ambient.Font;
                }

                return DefaultFont;
            }
            set
            {
                Font local = (Font)Properties.GetObject(PropFont);
                Font resolved = Font;

                bool localChanged = false;
                if (value == null)
                {
                    if (local != null)
                    {
                        localChanged = true;
                    }
                }
                else
                {
                    if (local == null)
                    {
                        localChanged = true;
                    }
                    else
                    {
                        localChanged = !value.Equals(local);
                    }
                }

                if (localChanged)
                {
                    // Store new local value
                    //
                    Properties.SetObject(PropFont, value);

                    // We only fire the Changed event if the "resolved" value
                    // changed, however we must update the font if the local
                    // value changed...
                    //
                    if (!resolved.Equals(value))
                    {
                        // Cleanup any font handle wrapper...
                        //
                        DisposeFontHandle();

                        if (Properties.ContainsInteger(PropFontHeight))
                        {
                            Properties.SetInteger(PropFontHeight, (value == null) ? -1 : value.Height);
                        }

                        // Font is an ambient property.  We need to layout our parent because Font may
                        // change our size.  We need to layout ourselves because our children may change
                        // size by inheriting the new value.
                        //
                        using (new LayoutTransaction(ParentInternal, this, PropertyNames.Font))
                        {
                            OnFontChanged(EventArgs.Empty);
                        }
                    }
                    else
                    {
                        if (IsHandleCreated && !GetStyle(ControlStyles.UserPaint))
                        {
                            DisposeFontHandle();
                            //SetWindowFont();
                        }
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void InitLayout()
        {
            LayoutEngine.InitLayout(this, BoundsSpecified.All);
        }

        internal virtual void AssignParent(Control value)
        {

            // Adopt the parent's required scaling bits
            if (value != null)
            {
                RequiredScalingEnabled = value.RequiredScalingEnabled;
            }

            if (CanAccessProperties)
            {
                // Store the old values for these properties
                //
                Font oldFont = Font;
                Color oldForeColor = ForeColor;
                Color oldBackColor = BackColor;
                RightToLeft oldRtl = RightToLeft;
                bool oldEnabled = Enabled;
                bool oldVisible = Visible;

                // Update the parent
                //
                parent = value;
                OnParentChanged(EventArgs.Empty);

                if (GetAnyDisposingInHierarchy())
                {
                    return;
                }

                // Compare property values with new parent to old values
                //
                if (oldEnabled != Enabled)
                {
                    OnEnabledChanged(EventArgs.Empty);
                }

                // VSWhidbey 320131
                // When a control seems to be going from invisible -> visible,
                // yet its parent is being set to null and it's not top level, do not raise OnVisibleChanged.
                bool newVisible = Visible;

                if (oldVisible != newVisible && !(!oldVisible && newVisible && parent == null && !GetTopLevel()))
                {
                    OnVisibleChanged(EventArgs.Empty);
                }
                if (!oldFont.Equals(Font))
                {
                    OnFontChanged(EventArgs.Empty);
                }
                if (!oldForeColor.Equals(ForeColor))
                {
                    OnForeColorChanged(EventArgs.Empty);
                }
                if (!oldBackColor.Equals(BackColor))
                {
                    OnBackColorChanged(EventArgs.Empty);
                }
                if (oldRtl != RightToLeft)
                {
                    OnRightToLeftChanged(EventArgs.Empty);
                }
                if (Properties.GetObject(PropBindingManager) == null && this.Created)
                {
                    // We do not want to call our parent's BindingContext property here.
                    // We have no idea if us or any of our children are using data binding,
                    // and invoking the property would just create the binding manager, which
                    // we don't need.  We just blindly notify that the binding manager has
                    // changed, and if anyone cares, they will do the comparison at that time.
                    //
                    OnBindingContextChanged(EventArgs.Empty);
                }
            }
            else
            {
                parent = value;
                OnParentChanged(EventArgs.Empty);
            }

            SetState(STATE_CHECKEDHOST, false);
            if (ParentInternal != null) ParentInternal.LayoutEngine.InitLayout(this, BoundsSpecified.All);
        }


        internal bool ValidationCancelled
        {
            set
            {
                SetState(STATE_VALIDATIONCANCELLED, value);
            }
            get
            {
                if (GetState(STATE_VALIDATIONCANCELLED))
                {
                    return true;
                }
                else
                {
                    Control parent = this.ParentInternal;
                    if (parent != null)
                    {
                        return parent.ValidationCancelled;
                    }

                    return false;
                }
            }
        }

        internal virtual void NotifyValidationResult(object sender, CancelEventArgs ev)
        {
            this.ValidationCancelled = ev.Cancel;
        }

        internal bool IsDescendant(Control descendant)
        {
            Control control = descendant;
            while (control != null)
            {
                if (control == this)
                    return true;
                control = control.ParentInternal;
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void NotifyInvalidate(Rectangle invalidatedArea)
        {
            OnInvalidated(new InvalidateEventArgs(invalidatedArea));
        }

        // Used by form to notify the control that it is validating.
        //
        private bool NotifyValidating()
        {
            CancelEventArgs ev = new CancelEventArgs();
            OnValidating(ev);
            return ev.Cancel;
        }

        // Used by form to notify the control that it has been validated.
        //
        private void NotifyValidated()
        {
            OnValidated(EventArgs.Empty);
        }

        internal bool PerformControlValidation(bool bulkValidation)
        {

            // Skip validation for controls that don't support it
            if (!this.CausesValidation)
            {
                return false;
            }

            // Raise the 'Validating' event. Stop now if handler cancels (ie. control is invalid).
            // NOTE: Handler may throw an exception here, but we must not attempt to catch it.
            if (this.NotifyValidating())
            {
                return true;
            }

            // Raise the 'Validated' event. Handlers may throw exceptions here too - but
            // convert these to ThreadException events, unless the app is being debugged,
            // or the control is being validated as part of a bulk validation operation.
            if (bulkValidation /*|| NativeWindow.WndProcShouldBeDebuggable*/)
            {
                this.NotifyValidated();
            }
            else
            {
                try
                {
                    this.NotifyValidated();
                }
                catch (Exception e)
                {
                    Application.OnThreadException(e);
                }
            }

            return false;
        }

        internal sealed class FontHandleWrapper : MarshalByRefObject, IDisposable
        {
            private IntPtr handle;

            internal FontHandleWrapper(Font font)
            {
                //handle = font.ToHfont();
                System.Internal.HandleCollector.Add(handle, NativeMethods.CommonHandles.GDI);
            }

            internal IntPtr Handle
            {
                get
                {
                    Debug.Assert(handle != IntPtr.Zero, "FontHandleWrapper disposed, but still being accessed");
                    return handle;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {

                if (handle != IntPtr.Zero)
                {
                    //SafeNativeMethods.DeleteObject(new HandleRef(this, handle));
                    handle = IntPtr.Zero;
                }

            }

            ~FontHandleWrapper()
            {
                Dispose(false);
            }
        }

        [
           ListBindable(false), ComVisible(false)
        ]
        public class ControlCollection : ArrangedElementCollection, IList, ICloneable
        {

            private Control owner;

            private int lastAccessedIndex = -1;

            public ControlCollection(Control owner)
            {
                this.owner = owner;
            }

            public virtual bool ContainsKey(string key)
            {
                return IsValidIndex(IndexOfKey(key));
            }

            public virtual void Add(Control value)
            {
                if (value == null)
                    return;
                if (value.GetTopLevel())
                {
                    throw new ArgumentException("TopLevelControlAdd");
                }

                // Verify that the control being added is on the same thread as
                // us...or our parent chain.
                //
                //if (owner.CreateThreadId != value.CreateThreadId)
                //{
                //    throw new ArgumentException("AddDifferentThreads");
                //}

                CheckParentingCycle(owner, value);

                if (value.parent == owner)
                {
                    value.SendToBack();
                    return;
                }

                // Remove the new control from its old parent (if any)
                //
                if (value.parent != null)
                {
                    value.parent.Controls.Remove(value);
                }

                // Add the control
                //
                InnerList.Add(value);

                if (value.tabIndex == -1)
                {

                    // Find the next highest tab index
                    //
                    int nextTabIndex = 0;
                    for (int c = 0; c < (Count - 1); c++)
                    {
                        int t = this[c].TabIndex;
                        if (nextTabIndex <= t)
                        {
                            nextTabIndex = t + 1;
                        }
                    }
                    value.tabIndex = nextTabIndex;
                }

                // if we don't suspend layout, AssignParent will indirectly trigger a layout event
                // before we're ready (AssignParent will fire a PropertyChangedEvent("Visible"), which calls PerformLayout)
                //
                owner.SuspendLayout();

                try
                {
                    Control oldParent = value.parent;
                    try
                    {
                        // AssignParent calls into user code - this could throw, which
                        // would make us short-circuit the rest of the reparenting logic.
                        // you could end up with a control half reparented.
                        value.AssignParent(owner);
                    }
                    finally
                    {
                        if (oldParent != value.parent && (owner.state & STATE_CREATED) != 0)
                        {
                            //value.SetParentHandle(owner.InternalHandle);
                            //if (value.Visible)
                            //{
                            //    value.CreateControl();
                            //}
                        }
                    }

                    value.InitLayout();
                }
                finally
                {
                    owner.ResumeLayout(false);
                }

                // Not putting in the finally block, as it would eat the original
                // exception thrown from AssignParent if the following throws an exception.
                LayoutTransaction.DoLayout(owner, value, PropertyNames.Parent);
                owner.OnControlAdded(new ControlEventArgs(value));


            }

            int IList.Add(object control)
            {
                if (control is Control)
                {
                    Add((Control)control);
                    return IndexOf((Control)control);
                }
                else
                {
                    throw new ArgumentException("ControlBadControl");
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public virtual void AddRange(Control[] controls)
            {
                if (controls == null)
                {
                    throw new ArgumentNullException("controls");
                }
                if (controls.Length > 0)
                {
                    owner.SuspendLayout();
                    try
                    {
                        for (int i = 0; i < controls.Length; ++i)
                        {
                            Add(controls[i]);
                        }
                    }
                    finally
                    {
                        owner.ResumeLayout(true);
                    }
                }
            }

            object ICloneable.Clone()
            {
                // Use CreateControlInstance so we get the same type of ControlCollection, but whack the
                // owner so adding controls to this new collection does not affect the control we cloned from.
                ControlCollection ccOther = owner.CreateControlsInstance();

                // We add using InnerList to prevent unnecessary parent cycle checks, etc.
                ccOther.InnerList.AddRange(this);
                return ccOther;
            }

            public bool Contains(Control control)
            {
                return InnerList.Contains(control);
            }

            public Control[] Find(string key, bool searchAllChildren)
            {
                if (String.IsNullOrEmpty(key))
                {
                    throw new System.ArgumentNullException("key", "FindKeyMayNotBeEmptyOrNull");
                }

                ArrayList foundControls = FindInternal(key, searchAllChildren, this, new ArrayList());

                // Make this a stongly typed collection.
                Control[] stronglyTypedFoundControls = new Control[foundControls.Count];
                foundControls.CopyTo(stronglyTypedFoundControls, 0);

                return stronglyTypedFoundControls;
            }

            private ArrayList FindInternal(string key, bool searchAllChildren, ControlCollection controlsToLookIn, ArrayList foundControls)
            {
                if ((controlsToLookIn == null) || (foundControls == null))
                {
                    return null;
                }

                try
                {
                    // Perform breadth first search - as it's likely people will want controls belonging
                    // to the same parent close to each other.

                    for (int i = 0; i < controlsToLookIn.Count; i++)
                    {
                        if (controlsToLookIn[i] == null)
                        {
                            continue;
                        }

                        if (WindowsFormsUtils.SafeCompareStrings(controlsToLookIn[i].Name, key, /* ignoreCase = */ true))
                        {
                            foundControls.Add(controlsToLookIn[i]);
                        }
                    }

                    // Optional recurive search for controls in child collections.

                    if (searchAllChildren)
                    {
                        for (int i = 0; i < controlsToLookIn.Count; i++)
                        {
                            if (controlsToLookIn[i] == null)
                            {
                                continue;
                            }
                            if ((controlsToLookIn[i].Controls != null) && controlsToLookIn[i].Controls.Count > 0)
                            {
                                // if it has a valid child collecion, append those results to our collection
                                foundControls = FindInternal(key, searchAllChildren, controlsToLookIn[i].Controls, foundControls);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (ClientUtils.IsSecurityOrCriticalException(e))
                    {
                        throw;
                    }
                }
                return foundControls;
            }

            public override IEnumerator GetEnumerator()
            {
                return new ControlCollectionEnumerator(this);
            }

            public int IndexOf(Control control)
            {
                return InnerList.IndexOf(control);
            }

            public virtual int IndexOfKey(String key)
            {
                // Step 0 - Arg validation
                if (String.IsNullOrEmpty(key))
                {
                    return -1; // we dont support empty or null keys.
                }

                // step 1 - check the last cached item
                if (IsValidIndex(lastAccessedIndex))
                {
                    if (WindowsFormsUtils.SafeCompareStrings(this[lastAccessedIndex].Name, key, /* ignoreCase = */ true))
                    {
                        return lastAccessedIndex;
                    }
                }

                // step 2 - search for the item
                for (int i = 0; i < this.Count; i++)
                {
                    if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, /* ignoreCase = */ true))
                    {
                        lastAccessedIndex = i;
                        return i;
                    }
                }

                // step 3 - we didn't find it.  Invalidate the last accessed index and return -1.
                lastAccessedIndex = -1;
                return -1;
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public Control Owner
            {
                get
                {
                    return owner;
                }
            }

            public virtual void Remove(Control value)
            {
                // Sanity check parameter
                if (value == null)
                {
                    return;     // Don't do anything
                }

                if (value.ParentInternal == owner)
                {
                    Debug.Assert(owner != null);

                    //value.SetParentHandle(IntPtr.Zero);

                    // Remove the control from the internal control array
                    //
                    InnerList.Remove(value);
                    value.AssignParent(null);
                    LayoutTransaction.DoLayout(owner, value, PropertyNames.Parent);
                    owner.OnControlRemoved(new ControlEventArgs(value));

                    // ContainerControl needs to see it needs to find a new ActiveControl.
                    //ContainerControl cc = owner.GetContainerControlInternal() as ContainerControl;
                    //if (cc != null)
                    //{
                    //    cc.AfterControlRemoved(value, owner);
                    //}
                }
            }

            void IList.Remove(object control)
            {
                if (control is Control)
                {
                    Remove((Control)control);
                }
            }

            public void RemoveAt(int index)
            {
                Remove(this[index]);
            }

            public virtual void RemoveByKey(string key)
            {
                int index = IndexOfKey(key);
                if (IsValidIndex(index))
                {
                    RemoveAt(index);
                }
            }

            public new virtual Control this[int index]
            {
                get
                {
                    //do some bounds checking here...
                    if (index < 0 || index >= Count)
                    {
                        throw new ArgumentOutOfRangeException("index", "IndexOutOfRange");
                    }

                    Control control = (Control)InnerList[index];
                    Debug.Assert(control != null, "Why are we returning null controls from a valid index?");
                    return control;
                }
            }

            public virtual Control this[string key]
            {
                get
                {
                    // We do not support null and empty string as valid keys.
                    if (String.IsNullOrEmpty(key))
                    {
                        return null;
                    }

                    // Search for the key in our collection
                    int index = IndexOfKey(key);
                    if (IsValidIndex(index))
                    {
                        return this[index];
                    }
                    else
                    {
                        return null;
                    }

                }
            }

            public virtual void Clear()
            {
                owner.SuspendLayout();
                // clear all preferred size caches in the tree - 
                // inherited fonts could go away, etc.
                CommonProperties.xClearAllPreferredSizeCaches(owner);

                try
                {
                    while (Count != 0)
                        RemoveAt(Count - 1);
                }
                finally
                {
                    owner.ResumeLayout();
                }
            }

            public int GetChildIndex(Control child)
            {
                return GetChildIndex(child, true);
            }

            public virtual int GetChildIndex(Control child, bool throwException)
            {
                int index = IndexOf(child);
                if (index == -1 && throwException)
                {
                    throw new ArgumentException("ControlNotChild");
                }
                return index;
            }

            internal virtual void SetChildIndexInternal(Control child, int newIndex)
            {
                // Sanity check parameters
                //
                if (child == null)
                {
                    throw new ArgumentNullException("child");
                }

                int currentIndex = GetChildIndex(child);

                if (currentIndex == newIndex)
                {
                    return;
                }

                if (newIndex >= Count || newIndex == -1)
                {
                    newIndex = Count - 1;
                }

                MoveElement(child, currentIndex, newIndex);
                child.UpdateZOrder();

                LayoutTransaction.DoLayout(owner, child, PropertyNames.ChildIndex);

            }

            public virtual void SetChildIndex(Control child, int newIndex)
            {
                SetChildIndexInternal(child, newIndex);
            }

            // COMPAT: VSWhidbey 448276
            // This is the same as WinformsUtils.ArraySubsetEnumerator 
            // however since we're no longer an array, we've gotta employ a 
            // special version of this.
            private class ControlCollectionEnumerator : IEnumerator
            {
                private ControlCollection controls;
                private int current;
                private int originalCount;

                public ControlCollectionEnumerator(ControlCollection controls)
                {
                    this.controls = controls;
                    this.originalCount = controls.Count;
                    current = -1;
                }

                public bool MoveNext()
                {
                    // VSWhidbey 448276
                    // We have to use Controls.Count here because someone could have deleted 
                    // an item from the array. 
                    //
                    // this can happen if someone does:
                    //     foreach (Control c in Controls) { c.Dispose(); }
                    // 
                    // We also dont want to iterate past the original size of the collection
                    //
                    // this can happen if someone does
                    //     foreach (Control c in Controls) { c.Controls.Add(new Label()); }

                    if (current < controls.Count - 1 && current < originalCount - 1)
                    {
                        current++;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                public void Reset()
                {
                    current = -1;
                }

                public object Current
                {
                    get
                    {
                        if (current == -1)
                        {
                            return null;
                        }
                        else
                        {
                            return controls[current];
                        }
                    }
                }
            }

        }
    }
}