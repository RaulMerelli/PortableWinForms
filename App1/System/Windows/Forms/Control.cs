using App1;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Security.Permissions;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class Control : Component
    {
        int clientWidth;
        int clientHeight;

        private static readonly object EventAutoSizeChanged;

        private static readonly object EventKeyDown;

        private static readonly object EventKeyPress;

        private static readonly object EventKeyUp;

        private static readonly object EventMouseDown;

        private static readonly object EventMouseEnter;

        private static readonly object EventMouseLeave;

        private static readonly object EventDpiChangedBeforeParent;

        private static readonly object EventDpiChangedAfterParent;

        private static readonly object EventMouseHover;

        private static readonly object EventMouseMove;

        private static readonly object EventMouseUp;

        private static readonly object EventMouseWheel;

        private static readonly object EventClick;

        private static readonly object EventClientSize;

        private static readonly object EventDoubleClick;

        private static readonly object EventMouseClick;

        private static readonly object EventMouseDoubleClick;

        private static readonly object EventMouseCaptureChanged;

        private static readonly object EventMove;

        private static readonly object EventResize;

        private static readonly object EventLayout;

        private static readonly object EventGotFocus;

        private static readonly object EventLostFocus;

        private static readonly object EventEnabledChanged;

        private static readonly object EventEnter;

        private static readonly object EventLeave;

        private static readonly object EventHandleCreated;

        private static readonly object EventHandleDestroyed;

        private static readonly object EventVisibleChanged;

        private static readonly object EventControlAdded;

        private static readonly object EventControlRemoved;

        private static readonly object EventChangeUICues;

        private static readonly object EventSystemColorsChanged;

        private static readonly object EventValidating;

        private static readonly object EventValidated;

        private static readonly object EventStyleChanged;

        private static readonly object EventImeModeChanged;

        private static readonly object EventHelpRequested;

        private static readonly object EventPaint;

        private static readonly object EventInvalidated;

        private static readonly object EventQueryContinueDrag;

        private static readonly object EventGiveFeedback;

        private static readonly object EventDragEnter;

        private static readonly object EventDragLeave;

        private static readonly object EventDragOver;

        private static readonly object EventDragDrop;

        private static readonly object EventQueryAccessibilityHelp;

        private static readonly object EventBackgroundImage;

        private static readonly object EventBackgroundImageLayout;

        private static readonly object EventBindingContext;

        private static readonly object EventBackColor;

        private static readonly object EventParent;

        private static readonly object EventVisible;

        private static readonly object EventText;

        private static readonly object EventTabStop;

        private static readonly object EventTabIndex;

        private static readonly object EventSize;

        private static readonly object EventRightToLeft;

        private static readonly object EventLocation;

        private static readonly object EventForeColor;

        private static readonly object EventFont;

        private static readonly object EventEnabled;

        private static readonly object EventDock;

        private static readonly object EventCursor;

        private static readonly object EventContextMenu;

        private static readonly object EventContextMenuStrip;

        private static readonly object EventCausesValidation;

        private static readonly object EventRegionChanged;

        private static readonly object EventMarginChanged;

        internal static readonly object EventPaddingChanged;

        private static readonly object EventPreviewKeyDown;

        public bool Enabled;
        public string Name;
        public int TabIndex;
        public ControlCollection Controls = new ControlCollection();

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.Events.AddHandler(EventAutoSizeChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAutoSizeChanged, value);
            }
        }

        public event EventHandler BackColorChanged
        {
            add
            {
                base.Events.AddHandler(EventBackColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBackColor, value);
            }
        }

        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.Events.AddHandler(EventBackgroundImage, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBackgroundImage, value);
            }
        }

        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.Events.AddHandler(EventBackgroundImageLayout, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBackgroundImageLayout, value);
            }
        }

        public event EventHandler BindingContextChanged
        {
            add
            {
                base.Events.AddHandler(EventBindingContext, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBindingContext, value);
            }
        }

        public event EventHandler CausesValidationChanged
        {
            add
            {
                base.Events.AddHandler(EventCausesValidation, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCausesValidation, value);
            }
        }

        public event EventHandler ClientSizeChanged
        {
            add
            {
                base.Events.AddHandler(EventClientSize, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventClientSize, value);
            }
        }

        [Browsable(false)]
        public event EventHandler ContextMenuChanged
        {
            add
            {
                base.Events.AddHandler(EventContextMenu, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventContextMenu, value);
            }
        }

        public event EventHandler ContextMenuStripChanged
        {
            add
            {
                base.Events.AddHandler(EventContextMenuStrip, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventContextMenuStrip, value);
            }
        }

        public event EventHandler CursorChanged
        {
            add
            {
                base.Events.AddHandler(EventCursor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCursor, value);
            }
        }

        public event EventHandler DockChanged
        {
            add
            {
                base.Events.AddHandler(EventDock, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDock, value);
            }
        }

        public event EventHandler EnabledChanged
        {
            add
            {
                base.Events.AddHandler(EventEnabled, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventEnabled, value);
            }
        }

        public event EventHandler FontChanged
        {
            add
            {
                base.Events.AddHandler(EventFont, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFont, value);
            }
        }

        public event EventHandler ForeColorChanged
        {
            add
            {
                base.Events.AddHandler(EventForeColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventForeColor, value);
            }
        }

        public event EventHandler LocationChanged
        {
            add
            {
                base.Events.AddHandler(EventLocation, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLocation, value);
            }
        }

        public event EventHandler MarginChanged
        {
            add
            {
                base.Events.AddHandler(EventMarginChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMarginChanged, value);
            }
        }

        public event EventHandler RegionChanged
        {
            add
            {
                base.Events.AddHandler(EventRegionChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRegionChanged, value);
            }
        }

        public event EventHandler RightToLeftChanged
        {
            add
            {
                base.Events.AddHandler(EventRightToLeft, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRightToLeft, value);
            }
        }

        public event EventHandler SizeChanged
        {
            add
            {
                base.Events.AddHandler(EventSize, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSize, value);
            }
        }

        public event EventHandler TabIndexChanged
        {
            add
            {
                base.Events.AddHandler(EventTabIndex, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTabIndex, value);
            }
        }

        public event EventHandler TabStopChanged
        {
            add
            {
                base.Events.AddHandler(EventTabStop, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTabStop, value);
            }
        }

        public event EventHandler TextChanged
        {
            add
            {
                base.Events.AddHandler(EventText, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventText, value);
            }
        }

        public event EventHandler VisibleChanged
        {
            add
            {
                base.Events.AddHandler(EventVisible, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventVisible, value);
            }
        }

        public event EventHandler Click
        {
            add
            {
                base.Events.AddHandler(EventClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventClick, value);
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event ControlEventHandler ControlAdded
        {
            add
            {
                base.Events.AddHandler(EventControlAdded, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventControlAdded, value);
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event ControlEventHandler ControlRemoved
        {
            add
            {
                base.Events.AddHandler(EventControlRemoved, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventControlRemoved, value);
            }
        }

        public event DragEventHandler DragDrop
        {
            add
            {
                base.Events.AddHandler(EventDragDrop, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragDrop, value);
            }
        }

        public event DragEventHandler DragEnter
        {
            add
            {
                base.Events.AddHandler(EventDragEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragEnter, value);
            }
        }

        public event DragEventHandler DragOver
        {
            add
            {
                base.Events.AddHandler(EventDragOver, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragOver, value);
            }
        }

        public event EventHandler DragLeave
        {
            add
            {
                base.Events.AddHandler(EventDragLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragLeave, value);
            }
        }

        public event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                base.Events.AddHandler(EventGiveFeedback, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventGiveFeedback, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler HandleCreated
        {
            add
            {
                base.Events.AddHandler(EventHandleCreated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHandleCreated, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler HandleDestroyed
        {
            add
            {
                base.Events.AddHandler(EventHandleDestroyed, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHandleDestroyed, value);
            }
        }

        public event HelpEventHandler HelpRequested
        {
            add
            {
                base.Events.AddHandler(EventHelpRequested, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHelpRequested, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event InvalidateEventHandler Invalidated
        {
            add
            {
                base.Events.AddHandler(EventInvalidated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInvalidated, value);
            }
        }

        public event EventHandler PaddingChanged
        {
            add
            {
                base.Events.AddHandler(EventPaddingChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPaddingChanged, value);
            }
        }

        public event PaintEventHandler Paint
        {
            add
            {
                base.Events.AddHandler(EventPaint, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPaint, value);
            }
        }

        public event QueryContinueDragEventHandler QueryContinueDrag
        {
            add
            {
                base.Events.AddHandler(EventQueryContinueDrag, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventQueryContinueDrag, value);
            }
        }

        public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp
        {
            add
            {
                base.Events.AddHandler(EventQueryAccessibilityHelp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventQueryAccessibilityHelp, value);
            }
        }

        public event EventHandler DoubleClick
        {
            add
            {
                base.Events.AddHandler(EventDoubleClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDoubleClick, value);
            }
        }

        public event EventHandler Enter
        {
            add
            {
                base.Events.AddHandler(EventEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventEnter, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler GotFocus
        {
            add
            {
                base.Events.AddHandler(EventGotFocus, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventGotFocus, value);
            }
        }

        public event KeyEventHandler KeyDown
        {
            add
            {
                base.Events.AddHandler(EventKeyDown, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyDown, value);
            }
        }

        public event KeyPressEventHandler KeyPress
        {
            add
            {
                base.Events.AddHandler(EventKeyPress, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyPress, value);
            }
        }

        public event KeyEventHandler KeyUp
        {
            add
            {
                base.Events.AddHandler(EventKeyUp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyUp, value);
            }
        }

        public event LayoutEventHandler Layout
        {
            add
            {
                base.Events.AddHandler(EventLayout, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLayout, value);
            }
        }

        public event EventHandler Leave
        {
            add
            {
                base.Events.AddHandler(EventLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLeave, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler LostFocus
        {
            add
            {
                base.Events.AddHandler(EventLostFocus, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLostFocus, value);
            }
        }

        public event MouseEventHandler MouseClick
        {
            add
            {
                base.Events.AddHandler(EventMouseClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseClick, value);
            }
        }

        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                base.Events.AddHandler(EventMouseDoubleClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseDoubleClick, value);
            }
        }

        public event EventHandler MouseCaptureChanged
        {
            add
            {
                base.Events.AddHandler(EventMouseCaptureChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseCaptureChanged, value);
            }
        }

        public event MouseEventHandler MouseDown
        {
            add
            {
                base.Events.AddHandler(EventMouseDown, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseDown, value);
            }
        }

        public event EventHandler MouseEnter
        {
            add
            {
                base.Events.AddHandler(EventMouseEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseEnter, value);
            }
        }

        public event EventHandler MouseLeave
        {
            add
            {
                base.Events.AddHandler(EventMouseLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseLeave, value);
            }
        }

        public event EventHandler DpiChangedBeforeParent
        {
            add
            {
                base.Events.AddHandler(EventDpiChangedBeforeParent, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDpiChangedBeforeParent, value);
            }
        }

        public event EventHandler DpiChangedAfterParent
        {
            add
            {
                base.Events.AddHandler(EventDpiChangedAfterParent, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDpiChangedAfterParent, value);
            }
        }

        public event EventHandler MouseHover
        {
            add
            {
                base.Events.AddHandler(EventMouseHover, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseHover, value);
            }
        }

        public event MouseEventHandler MouseMove
        {
            add
            {
                base.Events.AddHandler(EventMouseMove, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseMove, value);
            }
        }

        public event MouseEventHandler MouseUp
        {
            add
            {
                base.Events.AddHandler(EventMouseUp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseUp, value);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event MouseEventHandler MouseWheel
        {
            add
            {
                base.Events.AddHandler(EventMouseWheel, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseWheel, value);
            }
        }

        public event EventHandler Move
        {
            add
            {
                base.Events.AddHandler(EventMove, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMove, value);
            }
        }

        public event PreviewKeyDownEventHandler PreviewKeyDown
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            add
            {
                base.Events.AddHandler(EventPreviewKeyDown, value);
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            remove
            {
                base.Events.RemoveHandler(EventPreviewKeyDown, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler Resize
        {
            add
            {
                base.Events.AddHandler(EventResize, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventResize, value);
            }
        }

        public event UICuesEventHandler ChangeUICues
        {
            add
            {
                base.Events.AddHandler(EventChangeUICues, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventChangeUICues, value);
            }
        }

        public event EventHandler StyleChanged
        {
            add
            {
                base.Events.AddHandler(EventStyleChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventStyleChanged, value);
            }
        }

        public event EventHandler SystemColorsChanged
        {
            add
            {
                base.Events.AddHandler(EventSystemColorsChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSystemColorsChanged, value);
            }
        }

        public event CancelEventHandler Validating
        {
            add
            {
                base.Events.AddHandler(EventValidating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventValidating, value);
            }
        }

        public event EventHandler Validated
        {
            add
            {
                base.Events.AddHandler(EventValidated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventValidated, value);
            }
        }

        public event EventHandler ParentChanged
        {
            add
            {
                base.Events.AddHandler(EventParent, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventParent, value);
            }
        }

        public event EventHandler ImeModeChanged
        {
            add
            {
                base.Events.AddHandler(EventImeModeChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventImeModeChanged, value);
            }
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

        protected void InvokeOnClick(Control toInvoke, EventArgs e)
        {
            toInvoke?.OnClick(e);
        }

        protected virtual internal void OnClick(EventArgs e)
        {
            ((EventHandler)base.Events[EventClick])?.Invoke(this, e);
        }

        protected virtual internal void OnResize(EventArgs e)
        {
            ((EventHandler)base.Events[EventResize])?.Invoke(this, e);
        }

        protected virtual internal void OnTextChanged(EventArgs e)
        {
            if (base.Events[EventText] is EventHandler eventHandler)
            {
                eventHandler(this, e);
            }
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

        public bool Visible = true;
        public Control Parent;
        public IntPtr Handle;
        public int internalIndex;
        public string identifier = "";
        public Color ForeColor;
        public Color BackColor = Color.FromKnownColor(KnownColor.Control);
        public bool AutoSize;
        public Point AutoScrollOffset;
        public ImageLayout BackgroundImageLayout;
        public BindingContext BindingContext;
        public Rectangle Bounds;
        public bool CanFocus;
        public bool Capture;
        public bool CausesValidation;
        public static bool CheckForIllegalCrossThreadCalls;
        protected virtual Padding DefaultPadding => Padding.Empty;
        protected virtual ImeMode DefaultImeMode => ImeMode.Inherit;
        protected virtual Size DefaultSize => Size.Empty;

        internal Size clientSize;
        public Size ClientSize
        {
            get
            {
                return new Size(clientWidth, clientHeight);
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
        internal Size size;
        public Size Size
        {
            get
            {
                return size;
            }
            set
            {
                if (value != size)
                {
                    size = value;
                    if (layoutPerformed)
                    {
                        ApplyLocationAndSize();
                    }
                }
            }
        }
        internal bool layoutPerformed = false;

        public virtual void SuspendLayout()
        {

        }

        public virtual void ResumeLayout(bool performLayout)
        {

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
                script += $"document.getElementById(\"{identifier}\").style.left=\"{left}\";";
            }
            if (right != "")
            {
                script += $"document.getElementById(\"{identifier}\").style.right=\"{right}\";";
            }
            if (top != "")
            {
                script += $"document.getElementById(\"{identifier}\").style.top=\"{top}\";";
            }
            if (bottom != "")
            {
                script += $"document.getElementById(\"{identifier}\").style.bottom=\"{bottom}\";";
            }
            if (height != "")
            {
                script += $"document.getElementById(\"{identifier}\").style.height=\"{height}\";";
            }
            if (width != "")
            {
                script += $"document.getElementById(\"{identifier}\").style.width=\"{width}\";";
            }
            await Page.RunScript(script);
        }

        internal string ScriptClearCssLocationAndSize()
        {
            string script = "";
            script += $"document.getElementById(\"{identifier}\").style.removeProperty('width');";
            script += $"document.getElementById(\"{identifier}\").style.removeProperty('height');";
            script += $"document.getElementById(\"{identifier}\").style.removeProperty('top');";
            script += $"document.getElementById(\"{identifier}\").style.removeProperty('bottom');";
            script += $"document.getElementById(\"{identifier}\").style.removeProperty('left');";
            script += $"document.getElementById(\"{identifier}\").style.removeProperty('right');";
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
                Controls[i].identifier = identifier + "-";
                Controls[i].Parent = this;
                Controls[i].PerformLayout();
            }
        }
    }
}