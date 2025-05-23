using App1;
using JsonProperties;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.Layout;
using Windows.UI.Xaml;

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
        public ControlCollection Controls = new ControlCollection();
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

        public bool Visible = true;
        public Control Parent;
        public IntPtr Handle;
        public int internalIndex;
        internal string WebviewIdentifier = "";
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

        [Browsable(false)]
        public Drawing.Size PreferredSize
        {
            get { return GetPreferredSize(Drawing.Size.Empty); }
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
                //IntSecurity.UnmanagedCode.Demand();
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

        internal Drawing.Size size;
        public Drawing.Size Size
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

        Rectangle IArrangedElement.Bounds => throw new NotImplementedException();

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
        IArrangedElement IArrangedElement.Container => throw new NotImplementedException();

        public ArrangedElementCollection Children => throw new NotImplementedException();

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
                Controls[i].Parent = this;
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
            //if ((specified & BoundsSpecified.X) == BoundsSpecified.None) x = this.x;
            //if ((specified & BoundsSpecified.Y) == BoundsSpecified.None) y = this.y;
            //if ((specified & BoundsSpecified.Width) == BoundsSpecified.None) width = this.width;
            //if ((specified & BoundsSpecified.Height) == BoundsSpecified.None) height = this.height;
            //if (this.x != x || this.y != y || this.width != width ||
            //    this.height != height)
            //{
            //    SetBoundsCore(x, y, width, height, specified);

            //    // WM_WINDOWPOSCHANGED will trickle down to an OnResize() which will
            //    // have refreshed the interior layout or the resized control.  We only need to layout
            //    // the parent.  This happens after InitLayout has been invoked.
            //    LayoutTransaction.DoLayout(ParentInternal, this, PropertyNames.Bounds);
            //}
            //else
            //{
            //    // Still need to init scaling.
            //    InitScaling(specified);
            //}
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

            //if (GetState(STATE_DISPOSING | STATE_DISPOSED))
            //{
            //    // if someone's asking when we're disposing just return what we last had.
            //    prefSize = CommonProperties.xGetPreferredSizeCache(this);
            //}
            //else
            //{
            //    // Switch Size.Empty to maximum possible values
            //    proposedSize = LayoutUtils.ConvertZeroToUnbounded(proposedSize);

            //    // Force proposedSize to be within the elements constraints.  (This applies
            //    // minimumSize, maximumSize, etc.)
            //    proposedSize = ApplySizeConstraints(proposedSize);
            //    if (GetState2(STATE2_USEPREFERREDSIZECACHE))
            //    {
            //        Drawing.Size cachedSize = CommonProperties.xGetPreferredSizeCache(this);

            //        // If the "default" preferred size is being requested, and we have a cached value for it, return it.
            //        // 
            //        if (!cachedSize.IsEmpty && (proposedSize == LayoutUtils.MaxSize))
            //        {
            //            return cachedSize;
            //        }
            //    }

            //    CacheTextInternal = true;
            //    try
            //    {
            //        prefSize = GetPreferredSizeCore(proposedSize);
            //    }
            //    finally
            //    {
            //        CacheTextInternal = false;
            //    }

            //    // There is no guarantee that GetPreferredSizeCore() return something within
            //    // proposedSize, so we apply the element's constraints again.
            //    prefSize = ApplySizeConstraints(prefSize);

            //    // If the "default" preferred size was requested, cache the computed value.
            //    // 
            //    if (GetState2(STATE2_USEPREFERREDSIZECACHE) && proposedSize == LayoutUtils.MaxSize)
            //    {
            //        CommonProperties.xSetPreferredSizeCache(this, prefSize);
            //    }
            //}
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
                    //controlsCollection[i].OnParentBindingContextChanged(e);
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
            //if (GetAnyDisposingInHierarchy())
            //{
            //    return;
            //}

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
            //if (GetAnyDisposingInHierarchy())
            //{
            //    return;
            //}

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
            //if (GetAnyDisposingInHierarchy())
            //{
            //    return;
            //}

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
            //if (GetAnyDisposingInHierarchy())
            //{
            //    return;
            //}

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

            //EventHandler eh = Events[EventEnabled] as EventHandler;
            //if (eh != null)
            //{
            //    eh(this, e);
            //}

            //ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            //if (controlsCollection != null)
            //{
            //    // PERFNOTE: This is more efficient than using Foreach.  Foreach
            //    // forces the creation of an array subset enum each time we
            //    // enumerate
            //    for (int i = 0; i < controlsCollection.Count; i++)
            //    {
            //        controlsCollection[i].OnParentEnabledChanged(e);
            //    }
            //}
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
            //if (GetAnyDisposingInHierarchy())
            //{
            //    return;
            //}

            //Invalidate();

            //if (Properties.ContainsInteger(PropFontHeight))
            //{
            //    Properties.SetInteger(PropFontHeight, -1);
            //}


            ////
            //DisposeFontHandle();

            if (IsHandleCreated && !GetStyle(ControlStyles.UserPaint))
            {
                //SetWindowFont();
            }

            //EventHandler eh = Events[EventFont] as EventHandler;
            //if (eh != null)
            //{
            //    eh(this, e);
            //}

            //ControlCollection controlsCollection = (ControlCollection)Properties.GetObject(PropControlsCollection);
            //using (new LayoutTransaction(this, this, PropertyNames.Font, false))
            //{
            //    if (controlsCollection != null)
            //    {
            //        // This may have changed the sizes of our children.
            //        // PERFNOTE: This is more efficient than using Foreach.  Foreach
            //        // forces the creation of an array subset enum each time we
            //        // enumerate
            //        for (int i = 0; i < controlsCollection.Count; i++)
            //        {
            //            controlsCollection[i].OnParentFontChanged(e);
            //        }
            //    }
            //}

            //LayoutTransaction.DoLayout(this, this, PropertyNames.Font);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal virtual void OnForeColorChanged(EventArgs e)
        {
            Contract.Requires(e != null);
            //if (GetAnyDisposingInHierarchy())
            //{
            //    return;
            //}

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
            //if (GetAnyDisposingInHierarchy())
            //{
            //    return;
            //}

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

            //if (GetStyle(ControlStyles.UserPaint))
            //{
            //    // Theme support on Windows XP requires that we paint the background
            //    // and foreground to support semi-transparent children
            //    //
            //    PaintWithErrorHandling(e, PaintLayerBackground);
            //    e.ResetGraphics();
            //    PaintWithErrorHandling(e, PaintLayerForeground);
            //}
            //else
            //{
            //    PrintPaintEventArgs ppev = e as PrintPaintEventArgs;
            //    Message m;
            //    bool releaseDC = false;
            //    IntPtr hdc = IntPtr.Zero;

            //    if (ppev == null)
            //    {
            //        IntPtr flags = (IntPtr)(NativeMethods.PRF_CHILDREN | NativeMethods.PRF_CLIENT | NativeMethods.PRF_ERASEBKGND | NativeMethods.PRF_NONCLIENT);
            //        hdc = e.HDC;

            //        if (hdc == IntPtr.Zero)
            //        {
            //            // a manually created paintevent args
            //            hdc = e.Graphics.GetHdc();
            //            releaseDC = true;
            //        }
            //        m = Message.Create(this.Handle, NativeMethods.WM_PRINTCLIENT, hdc, flags);
            //    }
            //    else
            //    {
            //        m = ppev.Message;
            //    }

            //    try
            //    {
            //        DefWndProc(ref m);
            //    }
            //    finally
            //    {
            //        if (releaseDC)
            //        {
            //            e.Graphics.ReleaseHdcInternal(hdc);
            //        }
            //    }
            //}
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
            //if (visible)
            //{
            //    UnhookMouseEvent();
            //    trackMouseEvent = null;
            //}
            //if (parent != null && visible && !Created)
            //{
            //    bool isDisposing = GetAnyDisposingInHierarchy();
            //    if (!isDisposing)
            //    {
            //        // Usually the control is created by now, but in a few corner cases
            //        // exercised by the PropertyGrid dropdowns, it isn't
            //        CreateControl();
            //    }
            //}

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
                //IntSecurity.UnmanagedCode.Assert();

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

                //if (GetState2(STATE2_INTERESTEDINUSERPREFERENCECHANGED))
                //{
                //    ListenToUserPreferenceChanged(GetTopLevel());
                //}
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

            //bool parentRequiresLayout = LayoutEngine.Layout(this, levent);



            //if (parentRequiresLayout && ParentInternal != null)
            //{
            //    // LayoutEngine.Layout can return true to request that our parent resize us because
            //    // we did not have enough room for our contents.  We can not just call PerformLayout
            //    // because this container is currently suspended.  PerformLayout will check this state
            //    // flag and PerformLayout on our parent.
            //    ParentInternal.SetState(STATE_LAYOUTISDIRTY, true);
            //}
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

        [ListBindable(false), ComVisible(false)]
        public class ControlCollection
        {
            private Control owner;

            private int lastAccessedIndex = -1;

            public ControlCollection()
            {
            }

            public ControlCollection(Control owner)
            {
                this.owner = owner;
            }

            private int count = 0;
            private Control[] controls = new Control[0];

            public int Count { get => count; }

            public virtual void Add(Control value)
            {
                if (value == null)
                    return;
                Control[] backupControls = controls;
                controls = new Control[backupControls.Length + 1];
                for (int i = 0; i < backupControls.Length; i++)
                {
                    controls[i] = backupControls[i];
                }
                controls[backupControls.Length] = value;
                count = controls.Length;
            }

            public virtual void Remove(Control value)
            {
                if (value == null || count == 0)
                    return;

                int indexToRemove = -1;

                for (int i = 0; i < controls.Length; i++)
                {
                    if (controls[i] == value)
                    {
                        indexToRemove = i;
                        break;
                    }
                }

                if (indexToRemove == -1)
                    return;

                Control[] newControls = new Control[controls.Length - 1];
                for (int i = 0, j = 0; i < controls.Length; i++)
                {
                    if (i == indexToRemove)
                        continue;

                    newControls[j++] = controls[i];
                }

                controls = newControls;
                count = controls.Length;
            }

            public Control this[int index]
            {
                get
                {
                    return controls[index];
                }
                set
                {
                    controls[index] = value;
                    count = controls.Length;
                }
            }

            public virtual void Clear()
            {
                controls = new Control[0];
                count = 0;
                lastAccessedIndex = -1;
            }

            public virtual bool IsReadOnly
            {
                get
                {
                    return false; // Assume this collection is not read-only by default
                }
            }
        }

    }
}