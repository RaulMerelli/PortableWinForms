namespace System.Windows.Forms
{
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System;
    using System.Runtime.Versioning;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Collections;
    using System.Drawing;
    using System.Windows.Forms.Internal;
    using System.Drawing.Design;
    using Microsoft.Win32;
    using System.Reflection;
    using System.Collections.Specialized;
    using System.Text;
    using System.Diagnostics.CodeAnalysis;
    using static System.Windows.Forms.ListBox;

    [
    ComVisible(true),
    ClassInterface(ClassInterfaceType.AutoDispatch),
    DefaultEvent("SelectedIndexChanged"),
    DefaultProperty("Items"),
    DefaultBindingProperty("Text"),
    ]
    public class ComboBox : ListControl
    {

        private static readonly object EVENT_DROPDOWN = new object();
        private static readonly object EVENT_DRAWITEM = new object();
        private static readonly object EVENT_MEASUREITEM = new object();
        private static readonly object EVENT_SELECTEDINDEXCHANGED = new object();
        private static readonly object EVENT_SELECTIONCHANGECOMMITTED = new object();
        private static readonly object EVENT_SELECTEDITEMCHANGED = new object();
        private static readonly object EVENT_DROPDOWNSTYLE = new object();
        private static readonly object EVENT_TEXTUPDATE = new object();
        private static readonly object EVENT_DROPDOWNCLOSED = new object();

        private static readonly int PropMaxLength = PropertyStore.CreateKey();
        private static readonly int PropItemHeight = PropertyStore.CreateKey();
        private static readonly int PropDropDownWidth = PropertyStore.CreateKey();
        private static readonly int PropDropDownHeight = PropertyStore.CreateKey();
        private static readonly int PropStyle = PropertyStore.CreateKey();
        private static readonly int PropDrawMode = PropertyStore.CreateKey();
        private static readonly int PropMatchingText = PropertyStore.CreateKey();
        private static readonly int PropFlatComboAdapter = PropertyStore.CreateKey();

        private const int DefaultSimpleStyleHeight = 150;
        private const int DefaultDropDownHeight = 106;
        private const int AutoCompleteTimeout = 10000000; // 1 second timeout for resetting the MatchingText
        private bool autoCompleteDroppedDown = false;

        private FlatStyle flatStyle = FlatStyle.Standard;
        private int updateCount;

        //Timestamp of the last keystroke. Used for auto-completion
        // in DropDownList style.
        private long autoCompleteTimeStamp;

        private int selectedIndex = -1;  // used when we don't have a handle.
        private bool allowCommit = true;

        // When the style is "simple", the requested height is used
        // for the actual height of the control.
        // When the style is non-simple, the height of the control
        // is determined by the OS.
        // This fixes bug #20966
        private int requestedHeight;

        //private ComboBoxChildNativeWindow childDropDown;
        //private ComboBoxChildNativeWindow childEdit;
        //private ComboBoxChildNativeWindow childListBox;

        private IntPtr dropDownHandle;
        private ObjectCollection itemsCollection;
        private short prefHeightCache = -1;
        private short maxDropDownItems = 8;
        private bool integralHeight = true;
        private bool mousePressed;
        private bool mouseEvents;
        private bool mouseInEdit;

        private bool sorted;
        private bool fireSetFocus = true;
        private bool fireLostFocus = true;
        private bool mouseOver;
        private bool suppressNextWindosPos;
        private bool canFireLostFocus;

        // When the user types a letter and drops the dropdown...
        // the combobox itself auto-searches the matching item...
        // and selects the item in the edit...
        // thus changing the windowText...
        // hence we should Fire the TextChanged event in such a scenario..
        // The string below is used for checking the window Text before and after the dropdown.
        private string currentText = "";
        private string lastTextChangedValue;
        private bool dropDown;
        //private AutoCompleteDropDownFinder finder = new AutoCompleteDropDownFinder();

        private bool selectedValueChangedFired;

        private AutoCompleteMode autoCompleteMode = AutoCompleteMode.None;
        private AutoCompleteSource autoCompleteSource = AutoCompleteSource.None;
        private AutoCompleteStringCollection autoCompleteCustomSource;
        private StringSource stringSource;
        private bool fromHandleCreate = false;

        //private ComboBoxChildListUiaProvider childListAccessibleObject;
        //private ComboBoxChildEditUiaProvider childEditAccessibleObject;
        //private ComboBoxChildTextUiaProvider childTextAccessibleObject;

        // Indicates whether the dropdown list will be closed  after
        // selection (on getting CBN_SELENDOK notification) to prevent
        // focusing on the list item after hiding the list.
        private bool dropDownWillBeClosed = false;

        public ComboBox()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.UseTextForAccessibility |
                     ControlStyles.StandardClick, false);

            requestedHeight = DefaultSimpleStyleHeight;

            // this class overrides GetPreferredSizeCore, let Control automatically cache the result
            SetState2(STATE2_USEPREFERREDSIZECACHE, true);
        }

        [
        DefaultValue(AutoCompleteMode.None),
        Browsable(true), EditorBrowsable(EditorBrowsableState.Always)
        ]
        public AutoCompleteMode AutoCompleteMode
        {
            get
            {
                return autoCompleteMode;
            }
            set
            {
                //valid values are 0x0 to 0x3
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)AutoCompleteMode.None, (int)AutoCompleteMode.SuggestAppend))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(AutoCompleteMode));
                }
                if (this.DropDownStyle == ComboBoxStyle.DropDownList &&
                    this.AutoCompleteSource != AutoCompleteSource.ListItems &&
                    value != AutoCompleteMode.None)
                {
                    //throw new NotSupportedException(SR.GetString(SR.ComboBoxAutoCompleteModeOnlyNoneAllowed));
                    throw new NotSupportedException("ComboBoxAutoCompleteModeOnlyNoneAllowed");
                }
                //if (Application.OleRequired() != System.Threading.ApartmentState.STA)
                //{
                //    //throw new ThreadStateException(SR.GetString(SR.ThreadMustBeSTA));
                //    throw new ThreadStateException("ThreadMustBeSTA");
                //}
                bool resetAutoComplete = false;
                if (autoCompleteMode != AutoCompleteMode.None && value == AutoCompleteMode.None)
                {
                    resetAutoComplete = true;
                }
                autoCompleteMode = value;
                SetAutoComplete(resetAutoComplete, true);
            }
        }

        [
        DefaultValue(AutoCompleteSource.None),
        Browsable(true), EditorBrowsable(EditorBrowsableState.Always)
        ]
        public AutoCompleteSource AutoCompleteSource
        {
            get
            {
                return autoCompleteSource;
            }
            set
            {
                // FxCop: Avoid usage of Enum.IsDefined - this looks like an enum that could grow
                if (!ClientUtils.IsEnumValid_NotSequential(value, (int)value,
                                                    (int)AutoCompleteSource.None,
                                                    (int)AutoCompleteSource.AllSystemSources,
                                                    (int)AutoCompleteSource.AllUrl,
                                                    (int)AutoCompleteSource.CustomSource,
                                                    (int)AutoCompleteSource.FileSystem,
                                                    (int)AutoCompleteSource.FileSystemDirectories,
                                                    (int)AutoCompleteSource.HistoryList,
                                                    (int)AutoCompleteSource.ListItems,
                                                    (int)AutoCompleteSource.RecentlyUsedList))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(AutoCompleteSource));
                }

                if (this.DropDownStyle == ComboBoxStyle.DropDownList &&
                    this.AutoCompleteMode != AutoCompleteMode.None &&
                    value != AutoCompleteSource.ListItems)
                {
                    //throw new NotSupportedException(SR.GetString(SR.ComboBoxAutoCompleteSourceOnlyListItemsAllowed));
                    throw new NotSupportedException("ComboBoxAutoCompleteSourceOnlyListItemsAllowed");
                }
                //if (Application.OleRequired() != System.Threading.ApartmentState.STA)
                //{
                //    //throw new ThreadStateException(SR.GetString(SR.ThreadMustBeSTA));
                //    throw new ThreadStateException("ThreadMustBeSTA");
                //}

                // VSWhidbey 466300
                if (value != AutoCompleteSource.None &&
                    value != AutoCompleteSource.CustomSource &&
                    value != AutoCompleteSource.ListItems)
                {
                    FileIOPermission fiop = new FileIOPermission(PermissionState.Unrestricted);
                    fiop.AllFiles = FileIOPermissionAccess.PathDiscovery;
                    fiop.Demand();
                }

                autoCompleteSource = value;
                SetAutoComplete(false, true);
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Localizable(true),
        Browsable(true), EditorBrowsable(EditorBrowsableState.Always)
        ]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get
            {
                if (autoCompleteCustomSource == null)
                {
                    autoCompleteCustomSource = new AutoCompleteStringCollection();
                    autoCompleteCustomSource.CollectionChanged += new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                }
                return autoCompleteCustomSource;
            }
            set
            {
                if (autoCompleteCustomSource != value)
                {

                    if (autoCompleteCustomSource != null)
                    {
                        autoCompleteCustomSource.CollectionChanged -= new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                    }

                    autoCompleteCustomSource = value;

                    if (autoCompleteCustomSource != null)
                    {
                        autoCompleteCustomSource.CollectionChanged += new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                    }
                    SetAutoComplete(false, true);
                }
            }
        }

        //public override Color BackColor
        //{
        //    get
        //    {
        //        if (ShouldSerializeBackColor())
        //        {
        //            return base.BackColor;
        //        }
        //        else
        //        {
        //            return SystemColors.Window;
        //        }
        //    }
        //    set
        //    {
        //        base.BackColor = value;
        //    }
        //}

        //[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        //public override Image BackgroundImage
        //{
        //    get
        //    {
        //        return base.BackgroundImage;
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
        //        return base.BackgroundImageLayout;
        //    }
        //    set
        //    {
        //        base.BackgroundImageLayout = value;
        //    }
        //}

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        new public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.BackgroundImageChanged += value;
            }
            remove
            {
                base.BackgroundImageChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        new public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                base.BackgroundImageLayoutChanged -= value;
            }
        }

        //internal ChildAccessibleObject ChildEditAccessibleObject
        //{
        //    get
        //    {
        //        if (childEditAccessibleObject == null)
        //        {
        //            childEditAccessibleObject = new ComboBoxChildEditUiaProvider(this, childEdit.Handle);
        //        }

        //        return childEditAccessibleObject;
        //    }
        //}

        //internal ChildAccessibleObject ChildListAccessibleObject
        //{
        //    get
        //    {
        //        if (childListAccessibleObject == null)
        //        {
        //            childListAccessibleObject =
        //                new ComboBoxChildListUiaProvider(this, DropDownStyle == ComboBoxStyle.Simple ? childListBox.Handle : dropDownHandle);
        //        }

        //        return childListAccessibleObject;
        //    }
        //}

        //internal AccessibleObject ChildTextAccessibleObject
        //{
        //    get
        //    {
        //        if (childTextAccessibleObject == null)
        //        {
        //            childTextAccessibleObject = new ComboBoxChildTextUiaProvider(this);
        //        }

        //        return childTextAccessibleObject;
        //    }
        //}

        //protected override CreateParams CreateParams
        //{
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ClassName = "COMBOBOX";
        //        cp.Style |= NativeMethods.WS_VSCROLL | NativeMethods.CBS_HASSTRINGS | NativeMethods.CBS_AUTOHSCROLL;
        //        cp.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;
        //        if (!integralHeight) cp.Style |= NativeMethods.CBS_NOINTEGRALHEIGHT;

        //        switch (DropDownStyle)
        //        {
        //            case ComboBoxStyle.Simple:
        //                cp.Style |= NativeMethods.CBS_SIMPLE;
        //                break;
        //            case ComboBoxStyle.DropDown:
        //                cp.Style |= NativeMethods.CBS_DROPDOWN;
        //                // Make sure we put the height back or we won't be able to size the dropdown!
        //                cp.Height = PreferredHeight;
        //                break;
        //            case ComboBoxStyle.DropDownList:
        //                cp.Style |= NativeMethods.CBS_DROPDOWNLIST;
        //                // Comment above...
        //                cp.Height = PreferredHeight;
        //                break;
        //        }
        //        switch (DrawMode)
        //        {

        //            case DrawMode.OwnerDrawFixed:
        //                cp.Style |= NativeMethods.CBS_OWNERDRAWFIXED;
        //                break;
        //            case DrawMode.OwnerDrawVariable:
        //                cp.Style |= NativeMethods.CBS_OWNERDRAWVARIABLE;
        //                break;
        //        }

        //        return cp;
        //    }
        //}

        protected override Size DefaultSize
        {
            get
            {
                return new Size(121, PreferredHeight);
            }
        }

        [
        DefaultValue(null),
        RefreshProperties(RefreshProperties.Repaint),
        AttributeProvider(typeof(IListSource)),
        ]
        public new object DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                base.DataSource = value;
            }
        }

        [
        DefaultValue(DrawMode.Normal),
        RefreshProperties(RefreshProperties.Repaint)
        ]
        public DrawMode DrawMode
        {
            get
            {
                bool found;
                int drawMode = Properties.GetInteger(PropDrawMode, out found);
                if (found)
                {
                    return (DrawMode)drawMode;
                }

                return DrawMode.Normal;
            }
            set
            {
                if (DrawMode != value)
                {
                    //valid values are 0x0 to 0x2.
                    if (!ClientUtils.IsEnumValid(value, (int)value, (int)DrawMode.Normal, (int)DrawMode.OwnerDrawVariable))
                    {
                        throw new InvalidEnumArgumentException("value", (int)value, typeof(DrawMode));
                    }
                    ResetHeightCache();
                    Properties.SetInteger(PropDrawMode, (int)value);
                    //RecreateHandle();
                }
            }
        }

        public int DropDownWidth
        {
            get
            {
                bool found;
                int dropDownWidth = Properties.GetInteger(PropDropDownWidth, out found);

                if (found)
                {
                    return dropDownWidth;
                }
                else
                {
                    return Width;
                }
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("DropDownWidth", "InvalidArgument");
                }
                if (Properties.GetInteger(PropDropDownWidth) != value)
                {
                    Properties.SetInteger(PropDropDownWidth, value);
                    if (IsHandleCreated)
                    {
                        //SendMessage(NativeMethods.CB_SETDROPPEDWIDTH, value, 0);
                    }

                }
            }
        }

        [
        Browsable(true), EditorBrowsable(EditorBrowsableState.Always),
        DefaultValue(106)
        ]
        public int DropDownHeight
        {
            get
            {
                bool found;
                int dropDownHeight = Properties.GetInteger(PropDropDownHeight, out found);
                if (found)
                {
                    return dropDownHeight;
                }
                else
                {
                    return DefaultDropDownHeight;
                }
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("DropDownHeight", "InvalidArgument");
                }
                if (Properties.GetInteger(PropDropDownHeight) != value)
                {
                    Properties.SetInteger(PropDropDownHeight, value);

                    // The dropDownHeight is not reflected unless the
                    // combobox integralHeight == false..
                    IntegralHeight = false;
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool DroppedDown
        {
            get
            {
                if (IsHandleCreated)
                {
                    //return unchecked((int)(long)SendMessage(NativeMethods.CB_GETDROPPEDSTATE, 0, 0)) != 0;
                    return false;
                }
                else
                {
                    return false;
                }
            }

            set
            {

                if (!IsHandleCreated)
                {
                    //CreateHandle();
                }

                //SendMessage(NativeMethods.CB_SHOWDROPDOWN, value ? -1 : 0, 0);
            }
        }

        [
        DefaultValue(FlatStyle.Standard),
        Localizable(true),
        ]
        public FlatStyle FlatStyle
        {
            get
            {
                return flatStyle;
            }
            set
            {
                //valid values are 0x0 to 0x3
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)FlatStyle.Flat, (int)FlatStyle.System))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(FlatStyle));
                }
                flatStyle = value;
                Invalidate();
            }
        }

        //public override bool Focused
        //{
        //    get
        //    {
        //        if (base.Focused) return true;
        //        IntPtr focus = UnsafeNativeMethods.GetFocus();
        //        return focus != IntPtr.Zero && ((childEdit != null && focus == childEdit.Handle) || (childListBox != null && focus == childListBox.Handle));
        //    }
        //}

        //        //public override Color ForeColor
        //        //{
        //        //    get
        //        //    {
        //        //        if (ShouldSerializeForeColor())
        //        //        {
        //        //            return base.ForeColor;
        //        //        }
        //        //        else
        //        //        {
        //        //            return SystemColors.WindowText;
        //        //        }
        //        //    }
        //        //    set
        //        //    {
        //        //        base.ForeColor = value;
        //        //    }
        //        //}

        [
        DefaultValue(true),
        Localizable(true),
        ]
        public bool IntegralHeight
        {
            get
            {
                return integralHeight;
            }

            set
            {
                if (integralHeight != value)
                {
                    integralHeight = value;
                    //RecreateHandle();
                }
            }
        }

        //        [
        //        Localizable(true),
        //        ]
        //        public int ItemHeight
        //        {
        //            get
        //            {
        //                DrawMode drawMode = DrawMode;
        //                if (drawMode == DrawMode.OwnerDrawFixed ||
        //                    drawMode == DrawMode.OwnerDrawVariable ||
        //                    !IsHandleCreated)
        //                {

        //                    bool found;
        //                    int itemHeight = Properties.GetInteger(PropItemHeight, out found);
        //                    if (found)
        //                    {
        //                        return itemHeight;
        //                    }
        //                    else
        //                    {
        //                        return FontHeight + 2;   // bug (90774)+2 for the 1 pixel gap on each side (up and Bottom) of the Text.
        //                    }
        //                }

        //                // Note that the above if clause deals with the case when the handle has not yet been created
        //                Debug.Assert(IsHandleCreated, "Handle should be created at this point");

        //                int h = unchecked((int)(long)SendMessage(NativeMethods.CB_GETITEMHEIGHT, 0, 0));
        //                if (h == -1)
        //                {
        //                    throw new Win32Exception();
        //                }
        //                return h;
        //            }

        //            set
        //            {
        //                if (value < 1)
        //                {
        //                    throw new ArgumentOutOfRangeException("ItemHeight", SR.GetString(SR.InvalidArgument, "ItemHeight", (value).ToString(CultureInfo.CurrentCulture)));
        //                }

        //                ResetHeightCache();

        //                if (Properties.GetInteger(PropItemHeight) != value)
        //                {
        //                    Properties.SetInteger(PropItemHeight, value);
        //                    if (DrawMode != DrawMode.Normal)
        //                    {
        //                        UpdateItemHeight();
        //                    }
        //                }
        //            }
        //        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Localizable(true),
        MergableProperty(false)
        ]
        public ObjectCollection Items
        {
            get
            {
                if (itemsCollection == null)
                {
                    itemsCollection = new ObjectCollection(this);
                }
                return itemsCollection;
            }
        }

        // Text used to match an item in the list when auto-completion
        // is used in DropDownList style.
        private string MatchingText
        {
            get
            {
                string matchingText = (string)Properties.GetObject(PropMatchingText);
                return (matchingText == null) ? string.Empty : matchingText;
            }
            set
            {
                if (value != null || this.Properties.ContainsObject(PropMatchingText))
                {
                    Properties.SetObject(PropMatchingText, value);
                }
            }
        }

        [
        DefaultValue(8),
        Localizable(true),
        ]
        public int MaxDropDownItems
        {
            get
            {
                return maxDropDownItems;
            }
            set
            {
                if (value < 1 || value > 100)
                {
                    throw new ArgumentOutOfRangeException("MaxDropDownItems", "InvalidBoundArgument");
                }
                maxDropDownItems = (short)value;
            }
        }

        public override Size MaximumSize
        {
            get { return base.MaximumSize; }
            set
            {
                base.MaximumSize = new Size(value.Width, 0);
            }
        }

        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set
            {
                base.MinimumSize = new Size(value.Width, 0);
            }
        }

        [
        DefaultValue(0),
        Localizable(true),
        ]
        public int MaxLength
        {
            get
            {
                return Properties.GetInteger(PropMaxLength);
            }
            set
            {
                if (value < 0) value = 0;
                if (MaxLength != value)
                {
                    Properties.SetInteger(PropMaxLength, value);
                    //if (IsHandleCreated) SendMessage(NativeMethods.CB_LIMITTEXT, value, 0);
                }
            }
        }

        internal bool MouseIsOver
        {
            get { return mouseOver; }
            set
            {
                if (mouseOver != value)
                {
                    mouseOver = value;
                    // Nothing to see here... Just keep on walking... VSWhidbey 504477.
                    // Turns out that with Theming off, we don't get quite the same messages as with theming on, so
                    // our drawing gets a little messed up. So in case theming is off, force a draw here.
                    //if ((!ContainsFocus || !Application.RenderWithVisualStyles) && this.FlatStyle == FlatStyle.Popup)
                    //{
                    //    Invalidate();
                    //    Update();
                    //}
                }
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public new Padding Padding
        {
            get { return base.Padding; }
            set { base.Padding = value; }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public new event EventHandler PaddingChanged
        {
            add { base.PaddingChanged += value; }
            remove { base.PaddingChanged -= value; }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int PreferredHeight
        {
            get
            {
                if (!FormattingEnabled)
                {
                    //see #465057 for why I did this

                    //do preferred height the old broken way for everett apps
                    //we need this for compat reasons because (get this)
                    //  (a) everett preferredheight was always wrong.
                    //  (b) so, when combobox1.Size = actualdefaultsize was called, it would enter setboundscore
                    //  (c) this updated requestedheight
                    //  (d) if the user then changed the combo to simple style, the height did not change.
                    // We simply cannot match this behavior if preferredheight is corrected so that (b) never
                    // occurs.  We simply do not know when Size was set.

                    // So in whidbey, the behavior will be:
                    //  (1) user uses default size = setting dropdownstyle=simple will revert to simple height
                    //  (2) user uses nondefault size = setting dropdownstyle=simple will not change height from this value

                    //In everett
                    //  if the user manually sets Size = (121, 20) in code (usually height gets forced to 21), then he will see Whidey.(1) above
                    //  user usually uses nondefault size and will experience whidbey.(2) above

                    //Size textSize = TextRenderer.MeasureText(LayoutUtils.TestString, this.Font, new Size(Int16.MaxValue, (int)(FontHeight * 1.25)), TextFormatFlags.SingleLine);
                    //prefHeightCache = (short)(textSize.Height + SystemInformation.BorderSize.Height * 8 + Padding.Size.Height);

                    return prefHeightCache;
                }
                else
                {
                    // Normally we do this sort of calculation in GetPreferredSizeCore which has builtin
                    // caching, but in this case we can not because PreferredHeight is used in ApplySizeConstraints
                    // which is used by GetPreferredSize (infinite loop).
                    if (prefHeightCache < 0)
                    {
                        Size textSize = Size.Empty;
                        //Size textSize = TextRenderer.MeasureText(LayoutUtils.TestString, this.Font, new Size(Int16.MaxValue, (int)(FontHeight * 1.25)), TextFormatFlags.SingleLine);

                        if (DropDownStyle == ComboBoxStyle.Simple)
                        {
                            int itemCount = Items.Count + 1;
                            prefHeightCache = (short)(textSize.Height * itemCount + SystemInformation.BorderSize.Height * 16 + Padding.Size.Height);
                        }
                        else
                        {
                            // We do this old school rather than use SizeFromClientSize because CreateParams calls this
                            // method and SizeFromClientSize calls CreateParams (another infinite loop.)
                            prefHeightCache = (short)GetComboHeight();
                        }
                    }
                    return prefHeightCache;
                }
            }
        }


        //VSWhidbey 194386 - ComboBox.PreferredHeight returns incorrect values
        // This is translated from windows implementation.Since we cannot control the size
        // of the combo box, we need to use the same calculation they do.
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")] // "0" is what Windows uses in all languages.
        private int GetComboHeight()
        {

            int cyCombo = 0;
            // Add on CYEDGE just for some extra space in the edit field/static item.       
            // It's really only for static text items, but we want static & editable
            // controls to be the same height.
            Size textExtent = Size.Empty;

            //using (WindowsFont font = WindowsFont.FromFont(this.Font))
            //{
            //    // this is the character that Windows uses to determine the extent
            //    textExtent = WindowsGraphicsCacheManager.MeasurementGraphics.GetTextExtent("0", font);
            //}

            int dyEdit = textExtent.Height + SystemInformation.Border3DSize.Height;

            if (DrawMode != DrawMode.Normal)
            {
                // This is an ownerdraw combo.  Have the owner tell us how tall this
                // item is.
                //dyEdit = ItemHeight;
            }

            // Set the initial width to be the combo box rect.  Later we will shorten it
            // if there is a dropdown button.           
            Size fixedFrameBoderSize = SystemInformation.FixedFrameBorderSize;
            cyCombo = 2 * fixedFrameBoderSize.Height + dyEdit;
            return cyCombo;
        }

        private string[] GetStringsForAutoComplete(IList collection)
        {
            if (collection is AutoCompleteStringCollection)
            {
                string[] strings = new string[AutoCompleteCustomSource.Count];
                for (int i = 0; i < AutoCompleteCustomSource.Count; i++)
                {
                    strings[i] = AutoCompleteCustomSource[i];
                }
                return strings;

            }
            else if (collection is ObjectCollection)
            {
                string[] strings = new string[itemsCollection.Count];
                for (int i = 0; i < itemsCollection.Count; i++)
                {
                    strings[i] = GetItemText(itemsCollection[i]);
                }
                return strings;

            }
            return new string[0];
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override int SelectedIndex
        {
            get
            {
                if (IsHandleCreated)
                {
                    //return unchecked((int)(long)SendMessage(NativeMethods.CB_GETCURSEL, 0, 0));
                    return selectedIndex;
                }
                else
                {
                    return selectedIndex;
                }
            }
            set
            {
                if (SelectedIndex != value)
                {
                    int itemCount = 0;
                    if (itemsCollection != null)
                    {
                        itemCount = itemsCollection.Count;
                    }

                    if (value < -1 || value >= itemCount)
                    {
                        throw new ArgumentOutOfRangeException("SelectedIndex", "InvalidArgument");
                    }

                    if (IsHandleCreated)
                    {
                        //SendMessage(NativeMethods.CB_SETCURSEL, value, 0);

                    }
                    else
                    {
                        selectedIndex = value;
                    }

                    UpdateText();

                    if (IsHandleCreated)
                    {
                        OnTextChanged(EventArgs.Empty);
                    }

                    OnSelectedItemChanged(EventArgs.Empty);
                    OnSelectedIndexChanged(EventArgs.Empty);
                }
            }
        }

        [
        Browsable(false),
        Bindable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public object SelectedItem
        {
            get
            {
                int index = SelectedIndex;
                return (index == -1) ? null : Items[index];
            }
            set
            {
                int x = -1;

                if (itemsCollection != null)
                {
                    //bug (82115)
                    if (value != null)
                        x = itemsCollection.IndexOf(value);
                    else
                        SelectedIndex = -1;
                }

                if (x != -1)
                {
                    SelectedIndex = x;
                }
            }
        }

        //        [
        //        Browsable(false),
        //        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        //        ]
        //        public string SelectedText
        //        {
        //            get
        //            {
        //                if (DropDownStyle == ComboBoxStyle.DropDownList) return "";
        //                return Text.Substring(SelectionStart, SelectionLength);
        //            }
        //            set
        //            {
        //                if (DropDownStyle != ComboBoxStyle.DropDownList)
        //                {
        //                    //guard against null string, since otherwise we will throw an
        //                    //AccessViolation exception, which is bad
        //                    string str = (value == null ? "" : value);
        //                    //CreateControl();
        //                    //if (IsHandleCreated)
        //                    //{
        //                    //    Debug.Assert(childEdit != null);
        //                    //    if (childEdit != null)
        //                    //    {
        //                    //        UnsafeNativeMethods.SendMessage(new HandleRef(this, childEdit.Handle), NativeMethods.EM_REPLACESEL, NativeMethods.InvalidIntPtr, str);
        //                    //    }
        //                    //}
        //                }
        //            }
        //        }

        //        [
        //        Browsable(false),
        //        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        //        ]
        //        public int SelectionLength
        //        {
        //            get
        //            {
        //                int[] end = new int[] { 0 };
        //                int[] start = new int[] { 0 };
        //                //UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.CB_GETEDITSEL, start, end);
        //                return end[0] - start[0];
        //            }
        //            set
        //            {
        //                // SelectionLength can be negtive...
        //                Select(SelectionStart, value);
        //            }
        //        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int SelectionStart
        {
            get
            {
                int[] value = new int[] { 0 };
                //UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.CB_GETEDITSEL, value, (int[])null);
                return value[0];
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("SelectionStart", "InvalidArgument");
                }
                //Select(value, SelectionLength);
            }
        }

        [
        DefaultValue(false),
        ]
        public bool Sorted
        {
            get
            {
                return sorted;
            }
            set
            {
                if (sorted != value)
                {
                    if (this.DataSource != null && value)
                    {
                        //throw new ArgumentException(SR.GetString(SR.ComboBoxSortWithDataSource));
                        throw new ArgumentException("ComboBoxSortWithDataSource");
                    }

                    sorted = value;
                    RefreshItems();
                    SelectedIndex = -1;
                }
            }
        }

        [
        DefaultValue(ComboBoxStyle.DropDown),
        RefreshPropertiesAttribute(RefreshProperties.Repaint)
        ]
        public ComboBoxStyle DropDownStyle
        {
            get
            {
                bool found;
                int style = Properties.GetInteger(PropStyle, out found);
                if (found)
                {
                    return (ComboBoxStyle)style;
                }

                return ComboBoxStyle.DropDown;
            }
            set
            {
                if (DropDownStyle != value)
                {

                    // verify that 'value' is a valid enum type...
                    //valid values are 0x0 to 0x2
                    if (!ClientUtils.IsEnumValid(value, (int)value, (int)ComboBoxStyle.Simple, (int)ComboBoxStyle.DropDownList))
                    {
                        throw new InvalidEnumArgumentException("value", (int)value, typeof(ComboBoxStyle));
                    }

                    if (value == ComboBoxStyle.DropDownList &&
                        this.AutoCompleteSource != AutoCompleteSource.ListItems &&
                        this.AutoCompleteMode != AutoCompleteMode.None)
                    {
                        this.AutoCompleteMode = AutoCompleteMode.None;
                    }

                    // reset preferred height.
                    ResetHeightCache();

                    Properties.SetInteger(PropStyle, (int)value);

                    if (IsHandleCreated)
                    {
                        //RecreateHandle();
                    }

                    OnDropDownStyleChanged(EventArgs.Empty);
                }
            }
        }

        //        [
        //        Localizable(true),
        //        Bindable(true)
        //        ]
        //        public override string Text
        //        {
        //            get
        //            {
        //                if (SelectedItem != null && !BindingFieldEmpty)
        //                {

        //                    //preserve everett behavior if "formatting enabled == false" -- just return selecteditem text.
        //                    if (FormattingEnabled)
        //                    {
        //                        string candidate = GetItemText(SelectedItem);
        //                        if (!String.IsNullOrEmpty(candidate))
        //                        {
        //                            if (String.Compare(candidate, base.Text, true, CultureInfo.CurrentCulture) == 0)
        //                            {
        //                                return candidate;   //for whidbey, if we only differ by case -- return the candidate;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        return FilterItemOnProperty(SelectedItem).ToString();       //heinous.
        //                    }
        //                }
        //                return base.Text;
        //            }
        //            set
        //            {
        //                if (DropDownStyle == ComboBoxStyle.DropDownList && !IsHandleCreated && !String.IsNullOrEmpty(value) && FindStringExact(value) == -1)
        //                    return;

        //                base.Text = value;
        //                object selectedItem = null;

        //                selectedItem = SelectedItem;

        //                if (!DesignMode)
        //                {
        //                    //bug <70650> Subhag removed 'value.Length == 0' check to handle String.Empty.
        //                    //
        //                    if (value == null)
        //                    {
        //                        SelectedIndex = -1;
        //                    }
        //                    else if (value != null &&
        //                        (selectedItem == null || (String.Compare(value, GetItemText(selectedItem), false, CultureInfo.CurrentCulture) != 0)))
        //                    {

        //                        int index = FindStringIgnoreCase(value);

        //                        //we cannot set the index to -1 unless we want to do a hack and save/restore text
        //                        //because the native control will erase the text when we change the index to -1
        //                        if (index != -1)
        //                        {
        //                            SelectedIndex = index;
        //                        }
        //                    }
        //                }
        //            }
        //        }


        //        private int FindStringIgnoreCase(string value)
        //        {
        //            //look for an exact match and then a case insensitive match if that fails.
        //            int index = FindStringExact(value, -1, false);

        //            if (index == -1)
        //            {
        //                index = FindStringExact(value, -1, true);
        //            }

        //            return index;
        //        }

        //        // Special AutoComplete notification handling
        //        // If the text changed, this will fire TextChanged
        //        // If it matches an item in the list, this will fire SIC and SVC
        //        private void NotifyAutoComplete()
        //        {
        //            NotifyAutoComplete(true);
        //        }

        //        // Special AutoComplete notification handling
        //        // If the text changed, this will fire TextChanged
        //        // If it matches an item in the list, this will fire SIC and SVC
        //        private void NotifyAutoComplete(bool setSelectedIndex)
        //        {
        //            string text = this.Text;
        //            bool textChanged = (text != this.lastTextChangedValue);
        //            bool selectedIndexSet = false;

        //            if (setSelectedIndex)
        //            {
        //                // Process return key.  This is sent by the AutoComplete DropDown when a
        //                // selection is made from the DropDown
        //                // Check to see if the Text Changed.  If so, at least fire a TextChanged
        //                int index = FindStringIgnoreCase(text);

        //                if ((index != -1) && (index != SelectedIndex))
        //                {
        //                    // We found a match, do the full monty
        //                    SelectedIndex = index;

        //                    // Put the cursor at the end
        //                    SelectionStart = 0;
        //                    SelectionLength = text.Length;

        //                    selectedIndexSet = true;
        //                }
        //            }

        //            //don't fire textch if we had set the selectedindex -- because it was already fired if so.
        //            if (textChanged && !selectedIndexSet)
        //            {
        //                // No match, just fire a TextChagned
        //                OnTextChanged(EventArgs.Empty);
        //            }

        //            // Save the new value
        //            this.lastTextChangedValue = text;
        //        }

        //internal override bool SupportsUiaProviders
        //{
        //    get
        //    {
        //        return AccessibilityImprovements.Level3 && !DesignMode;
        //    }
        //}

        // Returns true if using System AutoComplete
        private bool SystemAutoCompleteEnabled
        {
            get
            {
                return ((this.autoCompleteMode != AutoCompleteMode.None) && (this.DropDownStyle != ComboBoxStyle.DropDownList));
            }
        }

        // VSWhidbey 95691: Prevent this event from being displayed in the Property Grid.
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler DoubleClick
        {
            add
            {
                base.DoubleClick += value;
            }
            remove
            {
                base.DoubleClick -= value;
            }
        }

        public event DrawItemEventHandler DrawItem
        {
            add
            {
                Events.AddHandler(EVENT_DRAWITEM, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_DRAWITEM, value);
            }
        }

        public event EventHandler DropDown
        {
            add
            {
                Events.AddHandler(EVENT_DROPDOWN, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_DROPDOWN, value);
            }
        }

        public event MeasureItemEventHandler MeasureItem
        {
            add
            {
                Events.AddHandler(EVENT_MEASUREITEM, value);
                //UpdateItemHeight();
            }
            remove
            {
                Events.RemoveHandler(EVENT_MEASUREITEM, value);
                //UpdateItemHeight();
            }
        }

        public event EventHandler SelectedIndexChanged
        {
            add
            {
                Events.AddHandler(EVENT_SELECTEDINDEXCHANGED, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_SELECTEDINDEXCHANGED, value);
            }
        }

        public event EventHandler SelectionChangeCommitted
        {
            add
            {
                Events.AddHandler(EVENT_SELECTIONCHANGECOMMITTED, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_SELECTIONCHANGECOMMITTED, value);
            }
        }

        public event EventHandler DropDownStyleChanged
        {
            add
            {
                Events.AddHandler(EVENT_DROPDOWNSTYLE, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_DROPDOWNSTYLE, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event PaintEventHandler Paint
        {
            add
            {
                base.Paint += value;
            }
            remove
            {
                base.Paint -= value;
            }
        }

        public event EventHandler TextUpdate
        {
            add
            {
                Events.AddHandler(EVENT_TEXTUPDATE, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_TEXTUPDATE, value);
            }
        }

        public event EventHandler DropDownClosed
        {
            add
            {
                Events.AddHandler(EVENT_DROPDOWNCLOSED, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_DROPDOWNCLOSED, value);
            }
        }

        //        [Obsolete("This method has been deprecated.  There is no replacement.  http://go.microsoft.com/fwlink/?linkid=14202")]
        //        protected virtual void AddItemsCore(object[] value)
        //        {
        //            int count = value == null ? 0 : value.Length;
        //            if (count == 0)
        //            {
        //                return;
        //            }

        //            BeginUpdate();
        //            try
        //            {
        //                Items.AddRangeInternal(value);
        //            }
        //            finally
        //            {
        //                EndUpdate();
        //            }
        //        }

        public void BeginUpdate()
        {
            updateCount++;
            //BeginUpdateInternal();
        }

        private void CheckNoDataSource()
        {
            if (DataSource != null)
            {
                //throw new ArgumentException(SR.GetString(SR.DataSourceLocksItems));
                throw new ArgumentException("DataSourceLocksItems");
            }
        }

        //        //protected override AccessibleObject CreateAccessibilityInstance()
        //        //{
        //        //    if (AccessibilityImprovements.Level3)
        //        //    {
        //        //        return new ComboBoxUiaProvider(this);
        //        //    }
        //        //    else if (AccessibilityImprovements.Level1)
        //        //    {
        //        //        return new ComboBoxExAccessibleObject(this);
        //        //    }
        //        //    else
        //        //    {
        //        //        return new ComboBoxAccessibleObject(this);
        //        //    }
        //        //}

        internal bool UpdateNeeded()
        {
            return (updateCount == 0);
        }

        //        //internal Point EditToComboboxMapping(Message m)
        //        //{
        //        //    if (childEdit == null)
        //        //    {
        //        //        return new Point(0, 0);
        //        //    }
        //        //    // Get the Combox Rect ...
        //        //    //
        //        //    NativeMethods.RECT comboRectMid = new NativeMethods.RECT();
        //        //    UnsafeNativeMethods.GetWindowRect(new HandleRef(this, Handle), ref comboRectMid);
        //        //    //
        //        //    //Get the Edit Rectangle...
        //        //    //
        //        //    NativeMethods.RECT editRectMid = new NativeMethods.RECT();
        //        //    UnsafeNativeMethods.GetWindowRect(new HandleRef(this, childEdit.Handle), ref editRectMid);

        //        //    //get the delta
        //        //    int comboXMid = NativeMethods.Util.SignedLOWORD(m.LParam) + (editRectMid.left - comboRectMid.left);
        //        //    int comboYMid = NativeMethods.Util.SignedHIWORD(m.LParam) + (editRectMid.top - comboRectMid.top);

        //        //    return (new Point(comboXMid, comboYMid));

        //        //}

        //        //private void ChildWndProc(ref Message m)
        //        //{

        //        //    switch (m.Msg)
        //        //    {
        //        //        case NativeMethods.WM_CHAR:
        //        //            if (DropDownStyle == ComboBoxStyle.Simple && m.HWnd == childListBox.Handle)
        //        //            {
        //        //                DefChildWndProc(ref m);
        //        //            }
        //        //            else
        //        //            {
        //        //                if (PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed)
        //        //                {
        //        //                    if (ProcessKeyMessage(ref m))
        //        //                    {
        //        //                        return;
        //        //                    }
        //        //                    DefChildWndProc(ref m);
        //        //                }
        //        //            }
        //        //            break;
        //        //        case NativeMethods.WM_SYSCHAR:
        //        //            if (DropDownStyle == ComboBoxStyle.Simple && m.HWnd == childListBox.Handle)
        //        //            {
        //        //                DefChildWndProc(ref m);
        //        //            }
        //        //            else
        //        //            {
        //        //                if (PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed)
        //        //                {
        //        //                    if (ProcessKeyEventArgs(ref m))
        //        //                    {
        //        //                        return;
        //        //                    }
        //        //                    DefChildWndProc(ref m);
        //        //                }
        //        //            }
        //        //            break;
        //        //        case NativeMethods.WM_KEYDOWN:
        //        //        case NativeMethods.WM_SYSKEYDOWN:
        //        //            if (SystemAutoCompleteEnabled && !ACNativeWindow.AutoCompleteActive)
        //        //            {
        //        //                finder.FindDropDowns(false);
        //        //            }

        //        //            if (AutoCompleteMode != AutoCompleteMode.None)
        //        //            {
        //        //                char keyChar = unchecked((char)(long)m.WParam);
        //        //                if (keyChar == (char)(int)Keys.Escape)
        //        //                {
        //        //                    this.DroppedDown = false;
        //        //                }
        //        //                else if (keyChar == (char)(int)Keys.Return && this.DroppedDown)
        //        //                {
        //        //                    UpdateText();
        //        //                    OnSelectionChangeCommittedInternal(EventArgs.Empty);
        //        //                    this.DroppedDown = false;
        //        //                }
        //        //            }

        //        //            if (DropDownStyle == ComboBoxStyle.Simple && m.HWnd == childListBox.Handle)
        //        //            {
        //        //                DefChildWndProc(ref m);
        //        //            }
        //        //            else
        //        //            {
        //        //                if (PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed)
        //        //                {
        //        //                    if (ProcessKeyMessage(ref m))
        //        //                    {
        //        //                        return;
        //        //                    }
        //        //                    DefChildWndProc(ref m);
        //        //                }
        //        //            }
        //        //            break;

        //        //        case NativeMethods.WM_INPUTLANGCHANGE:
        //        //            DefChildWndProc(ref m);
        //        //            break;

        //        //        case NativeMethods.WM_KEYUP:
        //        //        case NativeMethods.WM_SYSKEYUP:
        //        //            if (DropDownStyle == ComboBoxStyle.Simple && m.HWnd == childListBox.Handle)
        //        //            {
        //        //                DefChildWndProc(ref m);
        //        //            }
        //        //            else
        //        //            {
        //        //                if (PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed)
        //        //                {
        //        //                    if (ProcessKeyMessage(ref m))
        //        //                    {
        //        //                        return;
        //        //                    }
        //        //                    DefChildWndProc(ref m);
        //        //                }
        //        //            }
        //        //            if (SystemAutoCompleteEnabled && !ACNativeWindow.AutoCompleteActive)
        //        //            {
        //        //                finder.FindDropDowns();
        //        //            }

        //        //            break;
        //        //        case NativeMethods.WM_KILLFOCUS:
        //        //            // Consider - If we dont' have a childwndproc, then we don't get here, so we don't 
        //        //            // update the cache. Do we need to? This happens when we have a DropDownList.
        //        //            if (!DesignMode)
        //        //            {
        //        //                OnImeContextStatusChanged(m.HWnd);
        //        //            }

        //        //            DefChildWndProc(ref m);
        //        //            // We don't want to fire the focus events twice -
        //        //            // once in the combobox and once here.
        //        //            if (fireLostFocus)
        //        //            {
        //        //                this.InvokeLostFocus(this, EventArgs.Empty);
        //        //            }

        //        //            if (FlatStyle == FlatStyle.Popup)
        //        //            {
        //        //                this.Invalidate();
        //        //            }

        //        //            break;
        //        //        case NativeMethods.WM_SETFOCUS:

        //        //            // Consider - If we dont' have a childwndproc, then we don't get here, so we don't 
        //        //            // set the status. Do we need to? This happens when we have a DropDownList.
        //        //            if (!DesignMode)
        //        //            {
        //        //                ImeContext.SetImeStatus(CachedImeMode, m.HWnd);
        //        //            }

        //        //            if (!HostedInWin32DialogManager)
        //        //            {
        //        //                IContainerControl c = GetContainerControlInternal();
        //        //                if (c != null)
        //        //                {
        //        //                    ContainerControl container = c as ContainerControl;
        //        //                    if (container != null)
        //        //                    {
        //        //                        if (!container.ActivateControlInternal(this, false))
        //        //                        {
        //        //                            return;
        //        //                        }
        //        //                    }
        //        //                }
        //        //            }

        //        //            DefChildWndProc(ref m);

        //        //            // We don't want to fire the focus events twice -
        //        //            // once in the combobox and once here.
        //        //            if (fireSetFocus)
        //        //            {
        //        //                if (!DesignMode && (childEdit != null && m.HWnd == childEdit.Handle) && !LocalAppContextSwitches.EnableLegacyIMEFocusInComboBox)
        //        //                {
        //        //                    WmImeSetFocus();
        //        //                }
        //        //                this.InvokeGotFocus(this, EventArgs.Empty);
        //        //            }

        //        //            if (FlatStyle == FlatStyle.Popup)
        //        //            {
        //        //                this.Invalidate();
        //        //            }
        //        //            break;

        //        //        case NativeMethods.WM_SETFONT:
        //        //            DefChildWndProc(ref m);
        //        //            if (childEdit != null && m.HWnd == childEdit.Handle)
        //        //            {
        //        //                UnsafeNativeMethods.SendMessage(new HandleRef(this, childEdit.Handle), NativeMethods.EM_SETMARGINS,
        //        //                                          NativeMethods.EC_LEFTMARGIN | NativeMethods.EC_RIGHTMARGIN, 0);
        //        //            }
        //        //            break;
        //        //        case NativeMethods.WM_LBUTTONDBLCLK:
        //        //            //the Listbox gets  WM_LBUTTONDOWN - WM_LBUTTONUP -WM_LBUTTONDBLCLK - WM_LBUTTONUP...
        //        //            //sequence for doubleclick...
        //        //            //Set MouseEvents...
        //        //            mousePressed = true;
        //        //            mouseEvents = true;
        //        //            CaptureInternal = true;
        //        //            //Call the DefWndProc() so that mousemove messages get to the windows edit(112079)
        //        //            //
        //        //            DefChildWndProc(ref m);
        //        //            //the up gets fired from "Combo-box's WndPrc --- So Convert these Coordinates to Combobox coordianate...
        //        //            //
        //        //            Point Ptlc = EditToComboboxMapping(m);
        //        //            OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, Ptlc.X, Ptlc.Y, 0));
        //        //            break;

        //        //        case NativeMethods.WM_MBUTTONDBLCLK:
        //        //            //the Listbox gets  WM_LBUTTONDOWN - WM_LBUTTONUP -WM_LBUTTONDBLCLK - WM_LBUTTONUP...
        //        //            //sequence for doubleclick...
        //        //            //Set MouseEvents...
        //        //            mousePressed = true;
        //        //            mouseEvents = true;
        //        //            CaptureInternal = true;
        //        //            //Call the DefWndProc() so that mousemove messages get to the windows edit(112079)
        //        //            //
        //        //            DefChildWndProc(ref m);
        //        //            //the up gets fired from "Combo-box's WndPrc --- So Convert these Coordinates to Combobox coordianate...
        //        //            //
        //        //            Point Ptmc = EditToComboboxMapping(m);
        //        //            OnMouseDown(new MouseEventArgs(MouseButtons.Middle, 1, Ptmc.X, Ptmc.Y, 0));
        //        //            break;

        //        //        case NativeMethods.WM_RBUTTONDBLCLK:
        //        //            //the Listbox gets  WM_LBUTTONDOWN - WM_LBUTTONUP -WM_LBUTTONDBLCLK - WM_LBUTTONUP...
        //        //            //sequence for doubleclick...
        //        //            //Set MouseEvents...
        //        //            mousePressed = true;
        //        //            mouseEvents = true;
        //        //            CaptureInternal = true;
        //        //            //Call the DefWndProc() so that mousemove messages get to the windows edit(112079)
        //        //            //
        //        //            DefChildWndProc(ref m);
        //        //            //the up gets fired from "Combo-box's WndPrc --- So Convert these Coordinates to Combobox coordianate...
        //        //            //
        //        //            Point Ptrc = EditToComboboxMapping(m);
        //        //            OnMouseDown(new MouseEventArgs(MouseButtons.Right, 1, Ptrc.X, Ptrc.Y, 0));
        //        //            break;

        //        //        case NativeMethods.WM_LBUTTONDOWN:
        //        //            mousePressed = true;
        //        //            mouseEvents = true;
        //        //            //set the mouse capture .. this is the Child Wndproc..
        //        //            //
        //        //            CaptureInternal = true;
        //        //            DefChildWndProc(ref m);
        //        //            //the up gets fired from "Combo-box's WndPrc --- So Convert these Coordinates to Combobox coordianate...
        //        //            //
        //        //            Point Ptl = EditToComboboxMapping(m);

        //        //            OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, Ptl.X, Ptl.Y, 0));
        //        //            break;
        //        //        case NativeMethods.WM_LBUTTONUP:
        //        //            // Get the mouse location
        //        //            //
        //        //            NativeMethods.RECT r = new NativeMethods.RECT();
        //        //            UnsafeNativeMethods.GetWindowRect(new HandleRef(this, Handle), ref r);
        //        //            Rectangle ClientRect = new Rectangle(r.left, r.top, r.right - r.left, r.bottom - r.top);
        //        //            // Get the mouse location
        //        //            //
        //        //            int x = NativeMethods.Util.SignedLOWORD(m.LParam);
        //        //            int y = NativeMethods.Util.SignedHIWORD(m.LParam);
        //        //            Point pt = new Point(x, y);
        //        //            pt = PointToScreen(pt);
        //        //            // combo box gets a WM_LBUTTONUP for focus change ...
        //        //            // So check MouseEvents....
        //        //            if (mouseEvents && !ValidationCancelled)
        //        //            {
        //        //                mouseEvents = false;
        //        //                if (mousePressed)
        //        //                {
        //        //                    if (ClientRect.Contains(pt))
        //        //                    {
        //        //                        mousePressed = false;
        //        //                        OnClick(new MouseEventArgs(MouseButtons.Left, 1, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));
        //        //                        OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                        mousePressed = false;
        //        //                        mouseInEdit = false;
        //        //                        OnMouseLeave(EventArgs.Empty);
        //        //                    }
        //        //                }
        //        //            }
        //        //            DefChildWndProc(ref m);
        //        //            CaptureInternal = false;

        //        //            //the up gets fired from "Combo-box's WndPrc --- So Convert these Coordinates to Combobox coordianate...
        //        //            //
        //        //            pt = EditToComboboxMapping(m);

        //        //            OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, pt.X, pt.Y, 0));
        //        //            break;
        //        //        case NativeMethods.WM_MBUTTONDOWN:
        //        //            mousePressed = true;
        //        //            mouseEvents = true;
        //        //            //set the mouse capture .. this is the Child Wndproc..
        //        //            //
        //        //            CaptureInternal = true;
        //        //            DefChildWndProc(ref m);
        //        //            //the up gets fired from "Combo-box's WndPrc --- So Convert these Coordinates to Combobox coordianate...
        //        //            //
        //        //            Point P = EditToComboboxMapping(m);

        //        //            OnMouseDown(new MouseEventArgs(MouseButtons.Middle, 1, P.X, P.Y, 0));
        //        //            break;
        //        //        case NativeMethods.WM_RBUTTONDOWN:
        //        //            mousePressed = true;
        //        //            mouseEvents = true;

        //        //            //set the mouse capture .. this is the Child Wndproc..
        //        //            // Bug# 112108: If I set the capture=true here, the
        //        //            // DefWndProc() never fires the WM_CONTEXTMENU that would show
        //        //            // the default context menu.
        //        //            //
        //        //            if (this.ContextMenu != null || this.ContextMenuStrip != null)
        //        //                CaptureInternal = true;
        //        //            DefChildWndProc(ref m);
        //        //            //the up gets fired from "Combo-box's WndPrc --- So Convert these Coordinates to Combobox coordianate...
        //        //            //
        //        //            Point Pt = EditToComboboxMapping(m);

        //        //            OnMouseDown(new MouseEventArgs(MouseButtons.Right, 1, Pt.X, Pt.Y, 0));
        //        //            break;
        //        //        case NativeMethods.WM_MBUTTONUP:
        //        //            mousePressed = false;
        //        //            mouseEvents = false;
        //        //            //set the mouse capture .. this is the Child Wndproc..
        //        //            //
        //        //            CaptureInternal = false;
        //        //            DefChildWndProc(ref m);
        //        //            OnMouseUp(new MouseEventArgs(MouseButtons.Middle, 1, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));
        //        //            break;
        //        //        case NativeMethods.WM_RBUTTONUP:
        //        //            mousePressed = false;
        //        //            mouseEvents = false;
        //        //            //set the mouse capture .. this is the Child Wndproc..
        //        //            //
        //        //            if (this.ContextMenu != null)
        //        //                CaptureInternal = false;
        //        //            DefChildWndProc(ref m);
        //        //            //the up gets fired from "Combo-box's WndPrc --- So Convert these Coordinates to Combobox coordianate...
        //        //            //
        //        //            Point ptRBtnUp = EditToComboboxMapping(m);

        //        //            OnMouseUp(new MouseEventArgs(MouseButtons.Right, 1, ptRBtnUp.X, ptRBtnUp.Y, 0));
        //        //            break;

        //        //        case NativeMethods.WM_CONTEXTMENU:
        //        //            // Forward context menu messages to the parent control
        //        //            if (this.ContextMenu != null || this.ContextMenuStrip != null)
        //        //            {
        //        //                UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.WM_CONTEXTMENU, m.WParam, m.LParam);
        //        //            }
        //        //            else
        //        //            {
        //        //                DefChildWndProc(ref m);
        //        //            }
        //        //            break;

        //        //        case NativeMethods.WM_MOUSEMOVE:
        //        //            Point point = EditToComboboxMapping(m);
        //        //            //Call the DefWndProc() so that mousemove messages get to the windows edit(112079)
        //        //            //
        //        //            DefChildWndProc(ref m);
        //        //            OnMouseEnterInternal(EventArgs.Empty);
        //        //            OnMouseMove(new MouseEventArgs(MouseButtons, 0, point.X, point.Y, 0));
        //        //            break;

        //        //        case NativeMethods.WM_SETCURSOR:
        //        //            if (Cursor != DefaultCursor && childEdit != null && m.HWnd == childEdit.Handle && NativeMethods.Util.LOWORD(m.LParam) == NativeMethods.HTCLIENT)
        //        //            {
        //        //                Cursor.CurrentInternal = Cursor;
        //        //            }
        //        //            else
        //        //            {
        //        //                DefChildWndProc(ref m);
        //        //            }
        //        //            break;

        //        //        case NativeMethods.WM_MOUSELEAVE:
        //        //            DefChildWndProc(ref m);
        //        //            OnMouseLeaveInternal(EventArgs.Empty);
        //        //            break;

        //        //        default:
        //        //            DefChildWndProc(ref m);
        //        //            break;
        //        //    }
        //        //}

        //        private void OnMouseEnterInternal(EventArgs args)
        //        {
        //            if (!mouseInEdit)
        //            {
        //                OnMouseEnter(args);
        //                mouseInEdit = true;
        //            }
        //        }

        //        private void OnMouseLeaveInternal(EventArgs args)
        //        {
        //            NativeMethods.RECT rect = new NativeMethods.RECT();
        //            UnsafeNativeMethods.GetWindowRect(new HandleRef(this, Handle), ref rect);
        //            Rectangle Rect = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        //            Point p = MousePosition;
        //            if (!Rect.Contains(p))
        //            {
        //                OnMouseLeave(args);
        //                mouseInEdit = false;
        //            }
        //        }

        //        private void DefChildWndProc(ref Message m)
        //        {
        //            if (childEdit != null)
        //            {
        //                NativeWindow childWindow;
        //                if (m.HWnd == childEdit.Handle)
        //                {
        //                    childWindow = childEdit;
        //                }
        //                else if (AccessibilityImprovements.Level3 && m.HWnd == dropDownHandle)
        //                {
        //                    childWindow = childDropDown;
        //                }
        //                else
        //                {
        //                    childWindow = childListBox;
        //                }

        //                //childwindow could be null if the handle was recreated while within a message handler
        //                // and then whoever recreated the handle allowed the message to continue to be processed
        //                //we cannot really be sure the new child will properly handle this window message, so we eat it.
        //                if (childWindow != null)
        //                {
        //                    childWindow.DefWndProc(ref m);
        //                }
        //            }
        //        }

        //        protected override void Dispose(bool disposing)
        //        {
        //            if (disposing)
        //            {
        //                if (autoCompleteCustomSource != null)
        //                {
        //                    autoCompleteCustomSource.CollectionChanged -= new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
        //                }
        //                if (stringSource != null)
        //                {
        //                    stringSource.ReleaseAutoComplete();
        //                    stringSource = null;
        //                }
        //            }
        //            base.Dispose(disposing);
        //        }

        public void EndUpdate()
        {
            updateCount--;
            if (updateCount == 0 && AutoCompleteSource == AutoCompleteSource.ListItems)
            {
                SetAutoComplete(false, false);
            }
            //if (EndUpdateInternal())
            //{
            //    if (childEdit != null && childEdit.Handle != IntPtr.Zero)
            //    {
            //        //SafeNativeMethods.InvalidateRect(new HandleRef(this, childEdit.Handle), null, false);
            //    }
            //    if (childListBox != null && childListBox.Handle != IntPtr.Zero)
            //    {
            //        //SafeNativeMethods.InvalidateRect(new HandleRef(this, childListBox.Handle), null, false);
            //    }
            //}
        }

        //        public int FindString(string s)
        //        {
        //            return FindString(s, -1);
        //        }

        //        public int FindString(string s, int startIndex)
        //        {
        //            if (s == null)
        //            {
        //                return -1;
        //            }

        //            if (itemsCollection == null || itemsCollection.Count == 0)
        //            {
        //                return -1;
        //            }

        //            // VSWhidbey 95158: The last item in the list is still a valid starting point for a search.
        //            if (startIndex < -1 || startIndex >= itemsCollection.Count)
        //            {
        //                throw new ArgumentOutOfRangeException("startIndex");
        //            }

        //            // Always use the managed FindStringInternal instead of CB_FINDSTRING.
        //            // The managed version correctly handles Turkish I.
        //            //
        //            return FindStringInternal(s, Items, startIndex, false);
        //        }

        //        public int FindStringExact(string s)
        //        {
        //            return FindStringExact(s, -1, true);
        //        }

        //        public int FindStringExact(string s, int startIndex)
        //        {
        //            return FindStringExact(s, startIndex, true);
        //        }

        //        internal int FindStringExact(string s, int startIndex, bool ignorecase)
        //        {
        //            if (s == null) return -1;

        //            if (itemsCollection == null || itemsCollection.Count == 0)
        //            {
        //                return -1;
        //            }

        //            // VSWhidbey 95158: The last item in the list is still a valid starting point for a search.
        //            if (startIndex < -1 || startIndex >= itemsCollection.Count)
        //            {
        //                throw new ArgumentOutOfRangeException("startIndex");
        //            }

        //            // Always use the managed FindStringInternal instead of CB_FINDSTRINGEXACT.
        //            // The managed version correctly handles Turkish I.
        //            //
        //            return FindStringInternal(s, Items, startIndex, true, ignorecase);
        //        }

        //        // GetPreferredSize and SetBoundsCore call this method to allow controls to self impose
        //        // constraints on their size.
        //        internal override Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
        //        {
        //            if (DropDownStyle == ComboBoxStyle.DropDown
        //                || DropDownStyle == ComboBoxStyle.DropDownList)
        //            {

        //                proposedHeight = PreferredHeight;
        //            }

        //            return base.ApplyBoundsConstraints(suggestedX, suggestedY, proposedWidth, proposedHeight);
        //        }

        //        [SuppressMessage("Microsoft.Portability", "CA1902:AvoidTestingForFloatingPointEquality")]
        //        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        //        {

        //            if (factor.Width != 1F && factor.Height != 1F)
        //            {
        //                // VSWhidbey 440275
        //                // we get called on scale before we get a notification that our font has changed.
        //                // in this case, we need to reset our height cache.
        //                ResetHeightCache();
        //            }
        //            base.ScaleControl(factor, specified);
        //        }

        //        public int GetItemHeight(int index)
        //        {

        //            // This function is only relevant for OwnerDrawVariable
        //            if (DrawMode != DrawMode.OwnerDrawVariable)
        //            {
        //                return ItemHeight;
        //            }

        //            if (index < 0 || itemsCollection == null || index >= itemsCollection.Count)
        //            {
        //                throw new ArgumentOutOfRangeException("index", "InvalidArgument");
        //            }

        //            if (IsHandleCreated)
        //            {

        //                int h = unchecked((int)(long)SendMessage(NativeMethods.CB_GETITEMHEIGHT, index, 0));
        //                if (h == -1)
        //                {
        //                    throw new Win32Exception();
        //                }
        //                return h;
        //            }

        //            return ItemHeight;
        //        }

        //        //internal IntPtr GetListHandle()
        //        //{
        //        //    return DropDownStyle == ComboBoxStyle.Simple ? childListBox.Handle : dropDownHandle;
        //        //}

        //        //internal NativeWindow GetListNativeWindow()
        //        //{
        //        //    return DropDownStyle == ComboBoxStyle.Simple ? childListBox : childDropDown;
        //        //}

        //        internal int GetListNativeWindowRuntimeIdPart()
        //        {
        //            var listNativeWindow = GetListNativeWindow();
        //            return listNativeWindow != null ? listNativeWindow.GetHashCode() : 0;
        //        }

        //        internal override IntPtr InitializeDCForWmCtlColor(IntPtr dc, int msg)
        //        {
        //            if ((msg == NativeMethods.WM_CTLCOLORSTATIC) && !ShouldSerializeBackColor())
        //            {
        //                // Let the Win32 Edit control handle background colors itself.
        //                // This is necessary because a disabled edit control will display a different
        //                // BackColor than when enabled.
        //                return IntPtr.Zero;
        //            }
        //            else if ((msg == NativeMethods.WM_CTLCOLORLISTBOX) && GetStyle(ControlStyles.UserPaint))
        //            {
        //                // VSWhidbey#93929. Base class returns hollow brush when UserPaint style is set, to avoid flicker in
        //                // main control. But when returning colors for child dropdown list, return normal ForeColor/BackColor,
        //                // since hollow brush leaves the list background unpainted.
        //                SafeNativeMethods.SetTextColor(new HandleRef(null, dc), ColorTranslator.ToWin32(ForeColor));
        //                SafeNativeMethods.SetBkColor(new HandleRef(null, dc), ColorTranslator.ToWin32(BackColor));
        //                return BackColorBrush;
        //            }
        //            else
        //            {
        //                return base.InitializeDCForWmCtlColor(dc, msg);
        //            }
        //        }

        //        // Returns true when the key processing needs to be intercepted to allow
        //        // auto-completion in DropDownList style.
        //        //private bool InterceptAutoCompleteKeystroke(Message m)
        //        //{
        //        //    if (m.Msg == NativeMethods.WM_KEYDOWN)
        //        //    {
        //        //        Debug.Assert((ModifierKeys & Keys.Alt) == 0);
        //        //        // Keys.Delete only triggers a WM_KEYDOWN and WM_KEYUP, and no WM_CHAR. That's why it's treated separately.
        //        //        if ((Keys)unchecked((int)(long)m.WParam) == Keys.Delete)
        //        //        {
        //        //            // Reset matching text and remove any selection
        //        //            this.MatchingText = "";
        //        //            this.autoCompleteTimeStamp = DateTime.Now.Ticks;
        //        //            if (this.Items.Count > 0)
        //        //            {
        //        //                SelectedIndex = 0;
        //        //            }
        //        //            return false;
        //        //        }
        //        //    }
        //        //    else if (m.Msg == NativeMethods.WM_CHAR)
        //        //    {
        //        //        Debug.Assert((ModifierKeys & Keys.Alt) == 0);
        //        //        char keyChar = unchecked((char)(long)m.WParam);
        //        //        if (keyChar == (char)Keys.Back)
        //        //        {
        //        //            if (DateTime.Now.Ticks - this.autoCompleteTimeStamp > AutoCompleteTimeout ||
        //        //                this.MatchingText.Length <= 1)
        //        //            {
        //        //                // Reset matching text and remove any selection
        //        //                this.MatchingText = "";
        //        //                if (this.Items.Count > 0)
        //        //                {
        //        //                    SelectedIndex = 0;
        //        //                }
        //        //            }
        //        //            else
        //        //            {
        //        //                // Remove one character from matching text and rematch
        //        //                this.MatchingText = this.MatchingText.Remove(this.MatchingText.Length - 1);
        //        //                this.SelectedIndex = FindString(this.MatchingText);
        //        //            }
        //        //            this.autoCompleteTimeStamp = DateTime.Now.Ticks;
        //        //            return false;
        //        //        }
        //        //        else if (keyChar == (char)Keys.Escape)
        //        //        {
        //        //            this.MatchingText = "";
        //        //        }

        //        //        string newMatchingText;
        //        //        if (keyChar != (char)Keys.Escape && keyChar != (char)Keys.Return && !DroppedDown
        //        //            && AutoCompleteMode != AutoCompleteMode.Append)
        //        //        {
        //        //            DroppedDown = true;
        //        //        }
        //        //        if (DateTime.Now.Ticks - this.autoCompleteTimeStamp > AutoCompleteTimeout)
        //        //        {
        //        //            newMatchingText = new string(keyChar, 1);
        //        //            if (FindString(newMatchingText) != -1)
        //        //            {
        //        //                this.MatchingText = newMatchingText;
        //        //                // Select the found item
        //        //            }
        //        //            this.autoCompleteTimeStamp = DateTime.Now.Ticks;
        //        //            return false;
        //        //        }
        //        //        else
        //        //        {
        //        //            newMatchingText = this.MatchingText + keyChar;
        //        //            int itemFound = FindString(newMatchingText);
        //        //            if (itemFound != -1)
        //        //            {
        //        //                this.MatchingText = newMatchingText;
        //        //                if (itemFound != this.SelectedIndex)
        //        //                {
        //        //                    this.SelectedIndex = itemFound;
        //        //                }
        //        //            }
        //        //            // Do not change the selection
        //        //            this.autoCompleteTimeStamp = DateTime.Now.Ticks;
        //        //            return true;
        //        //        }
        //        //    }
        //        //    return false;
        //        //}

        //        // Invalidate the entire control, including child HWNDs and non-client areas
        //        private void InvalidateEverything()
        //        {
        //            SafeNativeMethods.RedrawWindow(new HandleRef(this, Handle),
        //                                           null, NativeMethods.NullHandleRef,
        //                                           NativeMethods.RDW_INVALIDATE |
        //                                           NativeMethods.RDW_FRAME |  // Control.Invalidate(true) doesn't invalidate the non-client region
        //                                           NativeMethods.RDW_ERASE |
        //                                           NativeMethods.RDW_ALLCHILDREN);
        //        }

        //        protected override bool IsInputKey(Keys keyData)
        //        {

        //            Keys keyCode = keyData & (Keys.KeyCode | Keys.Alt);
        //            if (keyCode == Keys.Return || keyCode == Keys.Escape)
        //            {
        //                if (this.DroppedDown || autoCompleteDroppedDown)
        //                {
        //                    //old behavior
        //                    return true;
        //                }
        //                else if (SystemAutoCompleteEnabled && ACNativeWindow.AutoCompleteActive)
        //                {
        //                    autoCompleteDroppedDown = true;
        //                    return true;
        //                }
        //            }

        //            return base.IsInputKey(keyData);
        //        }

        //        private int NativeAdd(object item)
        //        {
        //            Debug.Assert(IsHandleCreated, "Shouldn't be calling Native methods before the handle is created.");
        //            int insertIndex = unchecked((int)(long)SendMessage(NativeMethods.CB_ADDSTRING, 0, GetItemText(item)));
        //            if (insertIndex < 0)
        //            {
        //                throw new OutOfMemoryException("ComboBoxItemOverflow");
        //            }
        //            return insertIndex;
        //        }

        //        private void NativeClear()
        //        {
        //            Debug.Assert(IsHandleCreated, "Shouldn't be calling Native methods before the handle is created.");
        //            string saved = null;
        //            if (DropDownStyle != ComboBoxStyle.DropDownList)
        //            {
        //                saved = WindowText;
        //            }
        //            SendMessage(NativeMethods.CB_RESETCONTENT, 0, 0);
        //            if (saved != null)
        //            {
        //                WindowText = saved;
        //            }
        //        }

        //        private string NativeGetItemText(int index)
        //        {
        //            int len = unchecked((int)(long)SendMessage(NativeMethods.CB_GETLBTEXTLEN, index, 0));
        //            StringBuilder sb = new StringBuilder(len + 1);
        //            UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.CB_GETLBTEXT, index, sb);
        //            return sb.ToString();
        //        }

        //        private int NativeInsert(int index, object item)
        //        {
        //            Debug.Assert(IsHandleCreated, "Shouldn't be calling Native methods before the handle is created.");
        //            int insertIndex = unchecked((int)(long)SendMessage(NativeMethods.CB_INSERTSTRING, index, GetItemText(item)));
        //            if (insertIndex < 0)
        //            {
        //                throw new OutOfMemoryException(SR.GetString(SR.ComboBoxItemOverflow));
        //            }
        //            Debug.Assert(insertIndex == index, "NativeComboBox inserted at " + insertIndex + " not the requested index of " + index);
        //            return insertIndex;
        //        }

        //        private void NativeRemoveAt(int index)
        //        {
        //            Debug.Assert(IsHandleCreated, "Shouldn't be calling Native methods before the handle is created.");

        //            // Windows combo does not invalidate the selected region if you remove the
        //            // currently selected item.  Test for this and invalidate.  Note that because
        //            // invalidate will lazy-paint we can actually invalidate before we send the
        //            // delete message.
        //            //
        //            if (DropDownStyle == ComboBoxStyle.DropDownList && SelectedIndex == index)
        //            {
        //                Invalidate();
        //            }

        //            SendMessage(NativeMethods.CB_DELETESTRING, index, 0);
        //        }

        //        internal override void RecreateHandleCore()
        //        {

        //            string oldText = WindowText;
        //            base.RecreateHandleCore();
        //            if (!String.IsNullOrEmpty(oldText) && String.IsNullOrEmpty(WindowText))
        //            {
        //                WindowText = oldText;   //restore the window text
        //            }
        //        }

        //        protected override void CreateHandle()
        //        {
        //            using (new LayoutTransaction(ParentInternal, this, PropertyNames.Bounds))
        //            {
        //                base.CreateHandle();
        //            }
        //        }

        //        protected override void OnHandleCreated(EventArgs e)
        //        {

        //            base.OnHandleCreated(e);

        //            if (MaxLength > 0)
        //            {
        //                SendMessage(NativeMethods.CB_LIMITTEXT, MaxLength, 0);
        //            }

        //            // Get the handles and wndprocs of the ComboBox's child windows
        //            //
        //            Debug.Assert(childEdit == null, "Child edit window already attached");
        //            Debug.Assert(childListBox == null, "Child listbox window already attached");

        //            bool ok = childEdit == null && childListBox == null;

        //            if (ok && DropDownStyle != ComboBoxStyle.DropDownList)
        //            {
        //                IntPtr hwnd = UnsafeNativeMethods.GetWindow(new HandleRef(this, Handle), NativeMethods.GW_CHILD);
        //                if (hwnd != IntPtr.Zero)
        //                {

        //                    // if it's a simple dropdown list, the first HWND is the list box.
        //                    //                    
        //                    if (DropDownStyle == ComboBoxStyle.Simple)
        //                    {
        //                        childListBox = new ComboBoxChildNativeWindow(this, ChildWindowType.ListBox);
        //                        childListBox.AssignHandle(hwnd);

        //                        // get the edits hwnd...
        //                        //
        //                        hwnd = UnsafeNativeMethods.GetWindow(new HandleRef(this, hwnd), NativeMethods.GW_HWNDNEXT);
        //                    }

        //                    childEdit = new ComboBoxChildNativeWindow(this, ChildWindowType.Edit);
        //                    childEdit.AssignHandle(hwnd);

        //                    // set the initial margin for combobox to be zero (this is also done whenever the font is changed).
        //                    //
        //                    UnsafeNativeMethods.SendMessage(new HandleRef(this, childEdit.Handle), NativeMethods.EM_SETMARGINS,
        //                                              NativeMethods.EC_LEFTMARGIN | NativeMethods.EC_RIGHTMARGIN, 0);
        //                }
        //            }

        //            bool found;
        //            int dropDownWidth = Properties.GetInteger(PropDropDownWidth, out found);
        //            if (found)
        //            {
        //                SendMessage(NativeMethods.CB_SETDROPPEDWIDTH, dropDownWidth, 0);
        //            }

        //            found = false;
        //            int itemHeight = Properties.GetInteger(PropItemHeight, out found);
        //            if (found)
        //            {
        //                // someone has set the item height - update it
        //                UpdateItemHeight();
        //            }
        //            // Resize a simple style combobox on handle creation
        //            // to respect the requested height.
        //            //
        //            if (DropDownStyle == ComboBoxStyle.Simple)
        //            {
        //                Height = requestedHeight;
        //            }

        //            //If HandleCreated set the AutoComplete...
        //            //this function checks if the correct properties are set to enable AutoComplete feature on combobox.
        //            try
        //            {
        //                fromHandleCreate = true;
        //                SetAutoComplete(false, false);
        //            }
        //            finally
        //            {
        //                fromHandleCreate = false;
        //            }


        //            if (itemsCollection != null)
        //            {
        //                foreach (object item in itemsCollection)
        //                {
        //                    NativeAdd(item);
        //                }

        //                // Now udpate the current selection.
        //                //
        //                if (selectedIndex >= 0)
        //                {
        //                    SendMessage(NativeMethods.CB_SETCURSEL, selectedIndex, 0);
        //                    UpdateText();
        //                    selectedIndex = -1;
        //                }
        //            }
        //            // NOTE: Setting SelectedIndex must be the last thing we do. See ASURT 73949.

        //        }

        //        protected override void OnHandleDestroyed(EventArgs e)
        //        {
        //            dropDownHandle = IntPtr.Zero;
        //            if (Disposing)
        //            {
        //                itemsCollection = null;
        //                selectedIndex = -1;
        //            }
        //            else
        //            {
        //                selectedIndex = SelectedIndex;
        //            }
        //            if (stringSource != null)
        //            {
        //                stringSource.ReleaseAutoComplete();
        //                stringSource = null;
        //            }
        //            base.OnHandleDestroyed(e);
        //        }

        //        protected virtual void OnDrawItem(DrawItemEventArgs e)
        //        {
        //            DrawItemEventHandler handler = (DrawItemEventHandler)Events[EVENT_DRAWITEM];
        //            if (handler != null) handler(this, e);
        //        }

        //        protected virtual void OnDropDown(EventArgs e)
        //        {
        //            EventHandler handler = (EventHandler)Events[EVENT_DROPDOWN];
        //            if (handler != null) handler(this, e);

        //            //if (AccessibilityImprovements.Level3)
        //            //{
        //            //    // Notify collapsed/expanded property change.
        //            //    AccessibilityObject.RaiseAutomationPropertyChangedEvent(
        //            //        NativeMethods.UIA_ExpandCollapseExpandCollapseStatePropertyId,
        //            //        UnsafeNativeMethods.ExpandCollapseState.Collapsed,
        //            //        UnsafeNativeMethods.ExpandCollapseState.Expanded);

        //            //    var accessibleObject = AccessibilityObject as ComboBoxUiaProvider;
        //            //    if (accessibleObject != null)
        //            //    {
        //            //        accessibleObject.SetComboBoxItemFocus();
        //            //    }
        //            //}
        //        }

        //        [EditorBrowsable(EditorBrowsableState.Advanced)]
        //        protected override void OnKeyDown(KeyEventArgs e)
        //        {
        //            // Do Return/ESC handling
        //            if (SystemAutoCompleteEnabled)
        //            {
        //                if (e.KeyCode == Keys.Return)
        //                {
        //                    // Set SelectedIndex
        //                    NotifyAutoComplete(true);
        //                }
        //                else if ((e.KeyCode == Keys.Escape) && autoCompleteDroppedDown)
        //                {
        //                    // Fire TextChanged Only
        //                    NotifyAutoComplete(false);
        //                }
        //                autoCompleteDroppedDown = false;
        //            }

        //            // Base Handling
        //            base.OnKeyDown(e);
        //        }

        //        protected override void OnKeyPress(KeyPressEventArgs e)
        //        {

        //            base.OnKeyPress(e);

        //            //return when dropped down already fires commit.
        //            if (!e.Handled && (e.KeyChar == (char)(int)Keys.Return || e.KeyChar == (char)(int)Keys.Escape)
        //                && DroppedDown)
        //            {
        //                dropDown = false;
        //                if (FormattingEnabled)
        //                {
        //                    //Set the Text which would Compare the WindowText with the TEXT and change SelectedIndex.
        //                    Text = WindowText;
        //                    SelectAll();
        //                    e.Handled = false;
        //                }
        //                else
        //                {
        //                    DroppedDown = false;
        //                    e.Handled = true;
        //                }
        //            }
        //        }

        //        protected virtual void OnMeasureItem(MeasureItemEventArgs e)
        //        {
        //            MeasureItemEventHandler handler = (MeasureItemEventHandler)Events[EVENT_MEASUREITEM];
        //            if (handler != null) handler(this, e);
        //        }

        //        protected override void OnMouseEnter(EventArgs e)
        //        {
        //            base.OnMouseEnter(e);
        //            MouseIsOver = true;
        //        }

        //        protected override void OnMouseLeave(EventArgs e)
        //        {
        //            base.OnMouseLeave(e);
        //            MouseIsOver = false;
        //        }

        //        private void OnSelectionChangeCommittedInternal(EventArgs e)
        //        {
        //            //There are cases where if we disable the combo while in this event handler, it sends the message again.
        //            //This is a recursion gaurd to ensure we only send one commit per user action.
        //            if (allowCommit)
        //            {
        //                try
        //                {
        //                    allowCommit = false;
        //                    OnSelectionChangeCommitted(e);
        //                }
        //                finally
        //                {
        //                    allowCommit = true;
        //                }
        //            }
        //        }

        //        protected virtual void OnSelectionChangeCommitted(EventArgs e)
        //        {
        //            EventHandler handler = (EventHandler)Events[EVENT_SELECTIONCHANGECOMMITTED];
        //            if (handler != null) handler(this, e);

        //            // The user selects a list item or selects an item and then closes the list.
        //            // It indicates that the user's selection is to be processed but should not
        //            // be focused after closing the list.
        //            if (dropDown)
        //            {
        //                dropDownWillBeClosed = true;
        //            }
        //        }

        internal override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            EventHandler handler = (EventHandler)Events[EVENT_SELECTEDINDEXCHANGED];
            if (handler != null) handler(this, e);

            if (dropDownWillBeClosed)
            {
                // This is after-closing selection - do not focus on the list item
                // and reset the state to announce the selections later.
                dropDownWillBeClosed = false;
            }
            //else if (AccessibilityImprovements.Level3)
            //{
            //    var accessibleObject = AccessibilityObject as ComboBoxUiaProvider;
            //    if (accessibleObject != null)
            //    {

            //        // Announce DropDown- and DropDownList-styled ComboBox item selection using keyboard
            //        // in case when Level 3 is enabled and DropDown is not in expanded state. Simple-styled
            //        // ComboBox selection is announced by TextProvider.
            //        if (DropDownStyle == ComboBoxStyle.DropDownList || DropDownStyle == ComboBoxStyle.DropDown)
            //        {
            //            if (dropDown)
            //            {
            //                accessibleObject.SetComboBoxItemFocus();
            //            }

            //            accessibleObject.SetComboBoxItemSelection();
            //        }
            //    }
            //}

            // set the position in the dataSource, if there is any
            // we will only set the position in the currencyManager if it is different
            // from the SelectedIndex. Setting CurrencyManager::Position (even w/o changing it)
            // calls CurrencyManager::EndCurrentEdit, and that will pull the dataFrom the controls
            // into the backEnd. We do not need to do that.
            //
            // don't change the position if SelectedIndex is -1 because this indicates a selection not from the list.
            if (this.DataManager != null && DataManager.Position != SelectedIndex)
            {
                //read this as "if everett or   (whidbey and selindex is valid)"
                if (!FormattingEnabled || SelectedIndex != -1)
                {
                    this.DataManager.Position = this.SelectedIndex;
                }
            }
        }

        internal override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
            selectedValueChangedFired = true;
        }

        internal virtual void OnSelectedItemChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[EVENT_SELECTEDITEMCHANGED];
            if (handler != null) handler(this, e);
        }

        internal virtual void OnDropDownStyleChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[EVENT_DROPDOWNSTYLE];
            if (handler != null) handler(this, e);
        }

        internal override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            if (DropDownStyle == ComboBoxStyle.Simple) Invalidate();
        }

        internal override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ResetHeightCache();

            if (this.AutoCompleteMode == AutoCompleteMode.None)
            {
                UpdateControl(true);
            }
            else
            {
                //we always will recreate the handle when autocomplete mode is on
                //RecreateHandle();
            }
            CommonProperties.xClearPreferredSizeCache(this);
        }


        private void OnAutoCompleteCustomSourceChanged(object sender, CollectionChangeEventArgs e)
        {
            if (AutoCompleteSource == AutoCompleteSource.CustomSource)
            {
                if (AutoCompleteCustomSource.Count == 0)
                {
                    SetAutoComplete(true, true /*recreate handle*/);
                }
                else
                {
                    SetAutoComplete(true, false);
                }

            }
        }

        internal override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            UpdateControl(false);
        }

        internal override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            UpdateControl(false);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal override void OnGotFocus(EventArgs e)
        {
            if (!canFireLostFocus)
            {
                base.OnGotFocus(e);
                canFireLostFocus = true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal override void OnLostFocus(EventArgs e)
        {
            if (canFireLostFocus)
            {
                if (this.AutoCompleteMode != AutoCompleteMode.None &&
                    this.AutoCompleteSource == AutoCompleteSource.ListItems &&
                    this.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    this.MatchingText = "";
                }
                base.OnLostFocus(e);
                canFireLostFocus = false;
            }
        }

        //        [EditorBrowsable(EditorBrowsableState.Advanced)]
        //        protected override void OnTextChanged(EventArgs e)
        //        {
        //            if (SystemAutoCompleteEnabled)
        //            {
        //                string text = this.Text;

        //                // Prevent multiple TextChanges...
        //                if (text != this.lastTextChangedValue)
        //                {
        //                    // Need to still fire a TextChanged
        //                    base.OnTextChanged(e);

        //                    // Save the new value
        //                    this.lastTextChangedValue = text;
        //                }
        //            }
        //            else
        //            {
        //                // Call the base
        //                base.OnTextChanged(e);
        //            }
        //        }

        //        [EditorBrowsable(EditorBrowsableState.Advanced)]
        //        protected override void OnValidating(CancelEventArgs e)
        //        {
        //            if (SystemAutoCompleteEnabled)
        //            {
        //                // Handle AutoComplete notification
        //                NotifyAutoComplete();
        //            }

        //            // Call base
        //            base.OnValidating(e);
        //        }


        private void UpdateControl(bool recreate)
        {
            //clear the pref height cache
            ResetHeightCache();

            if (IsHandleCreated)
            {
                if (DropDownStyle == ComboBoxStyle.Simple && recreate)
                {
                    // Control forgets to add a scrollbar. See ASURT 19113.
                    //RecreateHandle();
                }
                else
                {
                    //UpdateItemHeight();
                    // Force everything to repaint. See ASURT 19113.
                    //InvalidateEverything();
                }
            }
        }

        internal override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (DropDownStyle == ComboBoxStyle.Simple)
            {
                // simple style combo boxes have more painting problems than you can shake a stick at (#19113)
                //InvalidateEverything();
            }
        }

        internal override void OnDataSourceChanged(EventArgs e)
        {
            if (Sorted)
            {
                if (DataSource != null && Created)
                {
                    // we will only throw the exception when the control is already on the form.
                    Debug.Assert(DisplayMember.Equals(String.Empty), "if this list is sorted it means that dataSource was null when Sorted first became true. at that point DisplayMember had to be String.Empty");
                    DataSource = null;
                    throw new InvalidOperationException(/*ComboBoxDataSourceWithSort*/);
                }
            }
            if (DataSource == null)
            {
                BeginUpdate();
                SelectedIndex = -1;
                Items.ClearInternal();
                EndUpdate();
            }
            if (!Sorted && Created)
                base.OnDataSourceChanged(e);
            RefreshItems();
        }

        internal override void OnDisplayMemberChanged(EventArgs e)
        {
            base.OnDisplayMemberChanged(e);

            // bug 63005: when we change the displayMember, we need to refresh the
            // items collection
            //
            RefreshItems();
        }

        internal virtual void OnDropDownClosed(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[EVENT_DROPDOWNCLOSED];
            if (handler != null) handler(this, e);

            //if (AccessibilityImprovements.Level3)
            //{
            //    // Need to announce the focus on combo-box with new selected value on drop-down close.
            //    // If do not do this focus in Level 3 stays on list item of unvisible list.
            //    // This is necessary for DropDown style as edit should not take focus.
            //    if (DropDownStyle == ComboBoxStyle.DropDown)
            //    {
            //        AccessibilityObject.RaiseAutomationEvent(NativeMethods.UIA_AutomationFocusChangedEventId);
            //    }

            //    // Notify Collapsed/expanded property change.
            //    AccessibilityObject.RaiseAutomationPropertyChangedEvent(
            //        NativeMethods.UIA_ExpandCollapseExpandCollapseStatePropertyId,
            //        UnsafeNativeMethods.ExpandCollapseState.Expanded,
            //        UnsafeNativeMethods.ExpandCollapseState.Collapsed);

            //    // Collapsing the DropDown, so reset the flag.
            //    dropDownWillBeClosed = false;
            //}
        }

        internal virtual void OnTextUpdate(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[EVENT_TEXTUPDATE];
            if (handler != null) handler(this, e);
        }

        //        [
        //            SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode),
        //        ]

        //        protected override bool ProcessKeyEventArgs(ref Message m)
        //        {
        //            if (this.AutoCompleteMode != AutoCompleteMode.None &&
        //                this.AutoCompleteSource == AutoCompleteSource.ListItems &&
        //                this.DropDownStyle == ComboBoxStyle.DropDownList &&
        //                InterceptAutoCompleteKeystroke(m))
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return base.ProcessKeyEventArgs(ref m);
        //            }
        //        }

        private void ResetHeightCache()
        {
            prefHeightCache = -1;
        }

        //        protected override void RefreshItems()
        //        {

        //            // Save off the selection and the current collection.
        //            //
        //            int selectedIndex = SelectedIndex;
        //            ObjectCollection savedItems = itemsCollection;

        //            itemsCollection = null;


        //            object[] newItems = null;

        //            // if we have a dataSource and a DisplayMember, then use it
        //            // to populate the Items collection
        //            //
        //            if (this.DataManager != null && this.DataManager.Count != -1)
        //            {
        //                newItems = new object[this.DataManager.Count];
        //                for (int i = 0; i < newItems.Length; i++)
        //                {
        //                    newItems[i] = this.DataManager[i];
        //                }
        //            }
        //            else if (savedItems != null)
        //            {
        //                newItems = new object[savedItems.Count];
        //                savedItems.CopyTo(newItems, 0);
        //            }
        //            BeginUpdate();
        //            try
        //            {
        //                // Clear the items.
        //                //
        //                if (IsHandleCreated)
        //                {
        //                    NativeClear();
        //                }
        //                // Store the current list of items
        //                //
        //                if (newItems != null)
        //                {
        //                    Items.AddRangeInternal(newItems);
        //                }
        //                if (this.DataManager != null)
        //                {
        //                    // put the selectedIndex in sync w/ the position in the dataManager
        //                    this.SelectedIndex = this.DataManager.Position;
        //                }
        //                else
        //                {
        //                    SelectedIndex = selectedIndex;
        //                }
        //            }
        //            finally
        //            {
        //                EndUpdate();
        //            }
        //        }

        protected override void RefreshItem(int index)
        {
            Items.SetItemInternal(index, Items[index]);
        }

        //private void ReleaseChildWindow()
        //{

        //    if (childEdit != null)
        //    {

        //        // We do not use UI Automation provider for child edit, so do not need to release providers.
        //        childEdit.ReleaseHandle();
        //        childEdit = null;
        //    }

        //    if (childListBox != null)
        //    {

        //        // Need to notify UI Automation that it can safely remove all map entries that refer to the specified window.
        //        if (AccessibilityImprovements.Level3)
        //        {
        //            ReleaseUiaProvider(childListBox.Handle);
        //        }

        //        childListBox.ReleaseHandle();
        //        childListBox = null;
        //    }

        //    if (childDropDown != null)
        //    {

        //        // Need to notify UI Automation that it can safely remove all map entries that refer to the specified window.
        //        if (AccessibilityImprovements.Level3)
        //        {
        //            ReleaseUiaProvider(childDropDown.Handle);
        //        }

        //        childDropDown.ReleaseHandle();
        //        childDropDown = null;
        //    }
        //}

        //internal override void ReleaseUiaProvider(IntPtr handle)
        //{
        //    base.ReleaseUiaProvider(handle);

        //    var uiaProvider = AccessibilityObject as ComboBoxUiaProvider;
        //    uiaProvider?.ResetListItemAccessibleObjects();
        //}

        private void ResetAutoCompleteCustomSource()
        {
            AutoCompleteCustomSource = null;
        }

        private void ResetDropDownWidth()
        {
            Properties.RemoveInteger(PropDropDownWidth);
        }

        private void ResetItemHeight()
        {
            Properties.RemoveInteger(PropItemHeight);
        }

        //        public override void ResetText()
        //        {
        //            base.ResetText();
        //        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void SetAutoComplete(bool reset, bool recreate)
        {
            //if (!IsHandleCreated || childEdit == null)
            //{
            //    return;
            //}

            if (AutoCompleteMode != AutoCompleteMode.None)
            {
                if (!fromHandleCreate && recreate && IsHandleCreated)
                {
                    //RecreateHandle to avoid Leak.
                    // notice the use of member variable to avoid re-entrancy
                    AutoCompleteMode backUpMode = this.AutoCompleteMode;
                    autoCompleteMode = AutoCompleteMode.None;
                    //RecreateHandle();
                    autoCompleteMode = backUpMode;
                }

                if (AutoCompleteSource == AutoCompleteSource.CustomSource)
                {
                    if (AutoCompleteCustomSource != null)
                    {
                        //if (AutoCompleteCustomSource.Count == 0)
                        //{
                        //    int mode = NativeMethods.AUTOSUGGEST_OFF | NativeMethods.AUTOAPPEND_OFF;
                        //    SafeNativeMethods.SHAutoComplete(new HandleRef(this, childEdit.Handle), mode);
                        //}
                        //else
                        //{

                        //    if (stringSource == null)
                        //    {
                        //        stringSource = new StringSource(GetStringsForAutoComplete(AutoCompleteCustomSource));
                        //        if (!stringSource.Bind(new HandleRef(this, childEdit.Handle), (int)AutoCompleteMode))
                        //        {
                        //            throw new ArgumentException("AutoCompleteFailure");
                        //        }
                        //    }
                        //    else
                        //    {
                        //        stringSource.RefreshList(GetStringsForAutoComplete(AutoCompleteCustomSource));
                        //    }

                        //}
                    }
                }
                else if (AutoCompleteSource == AutoCompleteSource.ListItems)
                {
                    if (DropDownStyle != ComboBoxStyle.DropDownList)
                    {
                        if (itemsCollection != null)
                        {
                            //if (itemsCollection.Count == 0)
                            //{
                            //    int mode = NativeMethods.AUTOSUGGEST_OFF | NativeMethods.AUTOAPPEND_OFF;
                            //    SafeNativeMethods.SHAutoComplete(new HandleRef(this, childEdit.Handle), mode);
                            //}
                            //else
                            //{

                            //    if (stringSource == null)
                            //    {
                            //        stringSource = new StringSource(GetStringsForAutoComplete(Items));
                            //        if (!stringSource.Bind(new HandleRef(this, childEdit.Handle), (int)AutoCompleteMode))
                            //        {
                            //            throw new ArgumentException(SR.GetString(SR.AutoCompleteFailureListItems));
                            //        }
                            //    }
                            //    else
                            //    {
                            //        stringSource.RefreshList(GetStringsForAutoComplete(Items));
                            //    }

                            //}
                        }
                    }
                    else
                    {
                        // Drop Down List special handling
                        Debug.Assert(DropDownStyle == ComboBoxStyle.DropDownList);
                        int mode = NativeMethods.AUTOSUGGEST_OFF | NativeMethods.AUTOAPPEND_OFF;
                        //SafeNativeMethods.SHAutoComplete(new HandleRef(this, childEdit.Handle), mode);
                    }
                }
                else
                {
                    try
                    {
                        int mode = 0;

                        if (AutoCompleteMode == AutoCompleteMode.Suggest)
                        {
                            mode |= NativeMethods.AUTOSUGGEST | NativeMethods.AUTOAPPEND_OFF;
                        }
                        if (AutoCompleteMode == AutoCompleteMode.Append)
                        {
                            mode |= NativeMethods.AUTOAPPEND | NativeMethods.AUTOSUGGEST_OFF;
                        }
                        if (AutoCompleteMode == AutoCompleteMode.SuggestAppend)
                        {
                            mode |= NativeMethods.AUTOSUGGEST;
                            mode |= NativeMethods.AUTOAPPEND;
                        }
                        //int ret = SafeNativeMethods.SHAutoComplete(new HandleRef(this, childEdit.Handle), (int)AutoCompleteSource | mode);
                    }
                    catch (SecurityException)
                    {
                        // If we don't have full trust, degrade gracefully. Allow the control to
                        // function without auto-complete. Allow the app to continue running.
                    }
                }
            }
            else if (reset)
            {
                int mode = NativeMethods.AUTOSUGGEST_OFF | NativeMethods.AUTOAPPEND_OFF;
                //SafeNativeMethods.SHAutoComplete(new HandleRef(this, childEdit.Handle), mode);
            }
        }

        public void Select(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start", "InvalidArgument");
            }
            // the Length can be negative to support Selecting in the "reverse" direction..
            int end = start + length;

            // but end cannot be negative... this means Length is far negative...
            if (end < 0)
            {
                throw new ArgumentOutOfRangeException("length", "InvalidArgument");
            }

            //SendMessage(NativeMethods.CB_SETEDITSEL, 0, NativeMethods.Util.MAKELPARAM(start, end));
        }

        public void SelectAll()
        {
            Select(0, Int32.MaxValue);
        }

        //        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        //        {
        //            // If we are changing height, store the requested height.
        //            // Requested height is used if the style is changed to simple.
        //            // (Bug fix #20966)
        //            if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
        //            {
        //                requestedHeight = height;
        //            }

        //            base.SetBoundsCore(x, y, width, height, specified);
        //        }

        protected override void SetItemsCore(IList value)
        {
            //BeginUpdate();
            Items.ClearInternal();
            Items.AddRangeInternal(value);

            // if the list changed, we want to keep the same selected index
            // CurrencyManager will provide the PositionChanged event
            // it will be provided before changing the list though...
            if (this.DataManager != null)
            {
                if (this.DataSource is ICurrencyManagerProvider)
                {
                    // Everett ListControl's had a bug where they would not fire
                    // OnSelectedValueChanged if their list of items were refreshed.
                    // We fix this post-Everett.
                    // However, for APPCOMPAT reasons, we only want to fix it when binding to 
                    // Whidbey components.
                    // vsw 547279.
                    this.selectedValueChangedFired = false;
                }

                if (IsHandleCreated)
                {
                    //SendMessage(NativeMethods.CB_SETCURSEL, DataManager.Position, 0);
                }
                else
                {
                    selectedIndex = DataManager.Position;
                }

                // if set_SelectedIndexChanged did not fire OnSelectedValueChanged
                // then we have to fire it ourselves, cos the list changed anyway
                if (!selectedValueChangedFired)
                {
                    OnSelectedValueChanged(EventArgs.Empty);
                    selectedValueChangedFired = false;
                }
            }

            EndUpdate();
        }

        protected override void SetItemCore(int index, object value)
        {
            Items.SetItemInternal(index, value);
        }

        //        private bool ShouldSerializeAutoCompleteCustomSource()
        //        {
        //            return autoCompleteCustomSource != null && autoCompleteCustomSource.Count > 0;
        //        }

        //        internal bool ShouldSerializeDropDownWidth()
        //        {
        //            return (Properties.ContainsInteger(PropDropDownWidth));
        //        }

        //        internal bool ShouldSerializeItemHeight()
        //        {
        //            return (Properties.ContainsInteger(PropItemHeight));
        //        }

        //        internal override bool ShouldSerializeText()
        //        {
        //            return SelectedIndex == -1 && base.ShouldSerializeText();
        //        }

        public override string ToString()
        {

            string s = base.ToString();
            return s + ", Items.Count: " + ((itemsCollection == null) ? (0).ToString(CultureInfo.CurrentCulture) : itemsCollection.Count.ToString(CultureInfo.CurrentCulture));
        }

        //        private void UpdateDropDownHeight()
        //        {
        //            if (dropDownHandle != IntPtr.Zero)
        //            {
        //                //Now use the DropDownHeight property instead of calculating the Height...
        //                int height = DropDownHeight;
        //                if (height == DefaultDropDownHeight)
        //                {
        //                    int itemCount = (itemsCollection == null) ? 0 : itemsCollection.Count;
        //                    int count = Math.Min(Math.Max(itemCount, 1), maxDropDownItems);
        //                    height = (ItemHeight * count + 2);
        //                }

        //                SafeNativeMethods.SetWindowPos(new HandleRef(this, dropDownHandle), NativeMethods.NullHandleRef, 0, 0, DropDownWidth, height,
        //                                     NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOZORDER);
        //            }
        //        }

        //        private void UpdateItemHeight()
        //        {
        //            //if (!IsHandleCreated)
        //            //{
        //            //    // VSWhidbey 156992: if we don't create control here we report item heights incorrectly later on.
        //            //    CreateControl();
        //            //}
        //            if (DrawMode == DrawMode.OwnerDrawFixed)
        //            {
        //                //SendMessage(NativeMethods.CB_SETITEMHEIGHT, -1, ItemHeight);
        //                //SendMessage(NativeMethods.CB_SETITEMHEIGHT, 0, ItemHeight);
        //            }
        //            else if (DrawMode == DrawMode.OwnerDrawVariable)
        //            {
        //                //SendMessage(NativeMethods.CB_SETITEMHEIGHT, -1, ItemHeight);
        //                //Graphics graphics = CreateGraphicsInternal();
        //                //for (int i = 0; i < Items.Count; i++)
        //                //{
        //                //    int original = unchecked((int)(long)SendMessage(NativeMethods.CB_GETITEMHEIGHT, i, 0));
        //                //    MeasureItemEventArgs mievent = new MeasureItemEventArgs(graphics, i, original);
        //                //    OnMeasureItem(mievent);
        //                //    if (mievent.ItemHeight != original)
        //                //    {
        //                //        SendMessage(NativeMethods.CB_SETITEMHEIGHT, i, mievent.ItemHeight);
        //                //    }
        //                //}
        //                //graphics.Dispose();
        //            }
        //        }

        private void UpdateText()
        {
            // V#45724 - Fire text changed for dropdown combos when the selection
            //           changes, since the text really does change.  We've got
            //           to do this asynchronously because the actual edit text
            //           isn't updated until a bit later (V#51240).
            //

            // QFE 2471:
            // v1.0 - ComboBox::set_Text compared items w/ "value" and set the SelectedIndex accordingly
            // v1.0 - null values can't correspond to String.Empty
            // v1.0 - SelectedIndex == -1 corresponds to Text == String.Emtpy
            //
            // v1.1 - ComboBox::set_Text compares FilterItemOnProperty(item) w/ "value" and set the SelectedIndex accordingly
            // v1.1 - null values correspond to String.Empty
            // v1.1 - SelectedIndex == -1 corresponds to Text == null
            string s = null;

            if (SelectedIndex != -1)
            {
                object item = Items[SelectedIndex];
                if (item != null)
                {
                    s = GetItemText(item);
                }
            }

            //Text = s;

            //if (DropDownStyle == ComboBoxStyle.DropDown)
            //{
            //    if (childEdit != null && childEdit.Handle != IntPtr.Zero)
            //    {
            //        UnsafeNativeMethods.SendMessage(new HandleRef(this, childEdit.Handle), NativeMethods.WM_SETTEXT, IntPtr.Zero, s);
            //    }
            //}
        }

        //private void WmEraseBkgnd(ref Message m)
        //{
        //    if ((DropDownStyle == ComboBoxStyle.Simple) && ParentInternal != null)
        //    {
        //        NativeMethods.RECT rect = new NativeMethods.RECT();
        //        SafeNativeMethods.GetClientRect(new HandleRef(this, Handle), ref rect);
        //        Control p = ParentInternal;
        //        Graphics graphics = Graphics.FromHdcInternal(m.WParam);
        //        if (p != null)
        //        {
        //            Brush brush = new SolidBrush(p.BackColor);
        //            graphics.FillRectangle(brush, rect.left, rect.top,
        //                                   rect.right - rect.left, rect.bottom - rect.top);
        //            brush.Dispose();
        //        }
        //        else
        //        {
        //            graphics.FillRectangle(SystemBrushes.Control, rect.left, rect.top,
        //                                   rect.right - rect.left, rect.bottom - rect.top);
        //        }
        //        graphics.Dispose();
        //        m.Result = (IntPtr)1;
        //        return;
        //    }
        //    base.WndProc(ref m);
        //}

        //private void WmParentNotify(ref Message m)
        //{
        //    base.WndProc(ref m);
        //    if (unchecked((int)(long)m.WParam) == (NativeMethods.WM_CREATE | 1000 << 16))
        //    {
        //        dropDownHandle = m.LParam;

        //        if (AccessibilityImprovements.Level3)
        //        {
        //            // By some reason WmParentNotify with WM_DESTROY is not called before recreation.
        //            // So release the old references here.
        //            if (childDropDown != null)
        //            {
        //                // Need to notify UI Automation that it can safely remove all map entries that refer to the specified window.
        //                ReleaseUiaProvider(childListBox.Handle);

        //                childDropDown.ReleaseHandle();
        //            }

        //            childDropDown = new ComboBoxChildNativeWindow(this, ChildWindowType.DropDownList);
        //            childDropDown.AssignHandle(dropDownHandle);

        //            // Reset the child list accessible object in case the the DDL is recreated.
        //            // For instance when dialog window containging the ComboBox is reopened.
        //            childListAccessibleObject = null;
        //        }
        //    }
        //}

        ///
        ///
        ///
        ///
        ///
        ///
        ///

        //private void WmReflectCommand(ref Message m)
        //{
        //    switch (NativeMethods.Util.HIWORD(m.WParam))
        //    {
        //        case NativeMethods.CBN_DBLCLK:
        //            //OnDoubleClick(EventArgs.Empty);
        //            break;
        //        case NativeMethods.CBN_EDITUPDATE:
        //            OnTextUpdate(EventArgs.Empty);
        //            break;
        //        case NativeMethods.CBN_CLOSEUP:

        //            OnDropDownClosed(EventArgs.Empty);
        //            if (FormattingEnabled && Text != currentText && dropDown)
        //            {
        //                OnTextChanged(EventArgs.Empty);
        //            }
        //            dropDown = false;
        //            break;
        //        case NativeMethods.CBN_DROPDOWN:
        //            currentText = Text;
        //            dropDown = true;
        //            OnDropDown(EventArgs.Empty);
        //            UpdateDropDownHeight();

        //            break;
        //        case NativeMethods.CBN_EDITCHANGE:
        //            OnTextChanged(EventArgs.Empty);
        //            break;
        //        case NativeMethods.CBN_SELCHANGE:
        //            UpdateText();
        //            OnSelectedIndexChanged(EventArgs.Empty);
        //            break;
        //        case NativeMethods.CBN_SELENDOK:
        //            OnSelectionChangeCommittedInternal(EventArgs.Empty);
        //            break;
        //    }
        //}

        //private void WmReflectDrawItem(ref Message m)
        //{
        //    NativeMethods.DRAWITEMSTRUCT dis = (NativeMethods.DRAWITEMSTRUCT)m.GetLParam(typeof(NativeMethods.DRAWITEMSTRUCT));
        //    IntPtr oldPal = SetUpPalette(dis.hDC, false /*force*/, false /*realize*/);
        //    try
        //    {
        //        Graphics g = Graphics.FromHdcInternal(dis.hDC);

        //        try
        //        {
        //            OnDrawItem(new DrawItemEventArgs(g, Font, Rectangle.FromLTRB(dis.rcItem.left, dis.rcItem.top, dis.rcItem.right, dis.rcItem.bottom),
        //                                             dis.itemID, (DrawItemState)dis.itemState, ForeColor, BackColor));
        //        }
        //        finally
        //        {
        //            g.Dispose();
        //        }
        //    }
        //    finally
        //    {
        //        if (oldPal != IntPtr.Zero)
        //        {
        //            SafeNativeMethods.SelectPalette(new HandleRef(this, dis.hDC), new HandleRef(null, oldPal), 0);
        //        }
        //    }
        //    m.Result = (IntPtr)1;
        //}

        //private void WmReflectMeasureItem(ref Message m)
        //{
        //    NativeMethods.MEASUREITEMSTRUCT mis = (NativeMethods.MEASUREITEMSTRUCT)m.GetLParam(typeof(NativeMethods.MEASUREITEMSTRUCT));

        //    // Determine if message was sent by a combo item or the combo edit field
        //    if (DrawMode == DrawMode.OwnerDrawVariable && mis.itemID >= 0)
        //    {
        //        Graphics graphics = CreateGraphicsInternal();
        //        MeasureItemEventArgs mie = new MeasureItemEventArgs(graphics, mis.itemID, ItemHeight);
        //        OnMeasureItem(mie);
        //        mis.itemHeight = mie.ItemHeight;
        //        graphics.Dispose();
        //    }
        //    else
        //    {
        //        // Message was sent by the combo edit field
        //        mis.itemHeight = ItemHeight;
        //    }
        //    Marshal.StructureToPtr(mis, m.LParam, false);
        //    m.Result = (IntPtr)1;
        //}

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //protected override void WndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {
        //        // We don't want to fire the focus events twice -
        //        // once in the combobox and once in the ChildWndProc.
        //        case NativeMethods.WM_SETFOCUS:
        //            try
        //            {
        //                fireSetFocus = false;
        //                base.WndProc(ref m);
        //            }

        //            finally
        //            {
        //                fireSetFocus = true;
        //            }
        //            break;
        //        case NativeMethods.WM_KILLFOCUS:
        //            try
        //            {
        //                fireLostFocus = false;
        //                base.WndProc(ref m);
        //                // Nothing to see here... Just keep on walking... VSWhidbey 504477.
        //                // Turns out that with Theming off, we don't get quite the same messages as with theming on.

        //                // With theming on we get a WM_MOUSELEAVE after a WM_KILLFOCUS even if you use the Tab key
        //                // to move focus. Our response to WM_MOUSELEAVE causes us to repaint everything correctly.

        //                // With theming off, we do not get a WM_MOUSELEAVE after a WM_KILLFOCUS, and since we don't have a childwndproc
        //                // when we are a Flat DropDownList, we need to force a repaint. The easiest way to do this is to send a 
        //                // WM_MOUSELEAVE to ourselves, since that also sets up the right state. Or... at least the state is the same
        //                // as with Theming on.

        //                // This is such a @#$(*&#@$ hack.

        //                if (!Application.RenderWithVisualStyles && GetStyle(ControlStyles.UserPaint) == false && this.DropDownStyle == ComboBoxStyle.DropDownList && (FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup))
        //                {
        //                    UnsafeNativeMethods.PostMessage(new HandleRef(this, Handle), NativeMethods.WM_MOUSELEAVE, 0, 0);
        //                }
        //            }

        //            finally
        //            {
        //                fireLostFocus = true;
        //            }
        //            break;
        //        case NativeMethods.WM_CTLCOLOREDIT:
        //        case NativeMethods.WM_CTLCOLORLISTBOX:
        //            m.Result = InitializeDCForWmCtlColor(m.WParam, m.Msg);
        //            break;
        //        case NativeMethods.WM_ERASEBKGND:
        //            WmEraseBkgnd(ref m);
        //            break;
        //        case NativeMethods.WM_PARENTNOTIFY:
        //            WmParentNotify(ref m);
        //            break;
        //        case NativeMethods.WM_REFLECT + NativeMethods.WM_COMMAND:
        //            WmReflectCommand(ref m);
        //            break;
        //        case NativeMethods.WM_REFLECT + NativeMethods.WM_DRAWITEM:
        //            WmReflectDrawItem(ref m);
        //            break;
        //        case NativeMethods.WM_REFLECT + NativeMethods.WM_MEASUREITEM:
        //            WmReflectMeasureItem(ref m);
        //            break;
        //        case NativeMethods.WM_LBUTTONDOWN:
        //            mouseEvents = true;
        //            base.WndProc(ref m);
        //            break;
        //        case NativeMethods.WM_LBUTTONUP:
        //            // Get the mouse location
        //            //
        //            NativeMethods.RECT r = new NativeMethods.RECT();
        //            UnsafeNativeMethods.GetWindowRect(new HandleRef(this, Handle), ref r);
        //            Rectangle ClientRect = new Rectangle(r.left, r.top, r.right - r.left, r.bottom - r.top);

        //            int x = NativeMethods.Util.SignedLOWORD(m.LParam);
        //            int y = NativeMethods.Util.SignedHIWORD(m.LParam);
        //            Point pt = new Point(x, y);
        //            pt = PointToScreen(pt);
        //            //mouseEvents is used to keep the check that we get the WM_LBUTTONUP after
        //            //WM_LBUTTONDOWN or WM_LBUTTONDBLBCLK
        //            // combo box gets a WM_LBUTTONUP for focus change ...
        //            //
        //            if (mouseEvents && !ValidationCancelled)
        //            {
        //                mouseEvents = false;
        //                bool captured = Capture;
        //                if (captured && ClientRect.Contains(pt))
        //                {
        //                    OnClick(new MouseEventArgs(MouseButtons.Left, 1, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));
        //                    OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));

        //                }
        //                base.WndProc(ref m);
        //            }
        //            else
        //            {
        //                CaptureInternal = false;
        //                DefWndProc(ref m);
        //            }
        //            break;

        //        case NativeMethods.WM_MOUSELEAVE:
        //            DefWndProc(ref m);
        //            OnMouseLeaveInternal(EventArgs.Empty);
        //            break;

        //        case NativeMethods.WM_PAINT:
        //            if (GetStyle(ControlStyles.UserPaint) == false && (FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup))
        //            {

        //                using (WindowsRegion dr = new WindowsRegion(FlatComboBoxAdapter.dropDownRect))
        //                {
        //                    using (WindowsRegion wr = new WindowsRegion(this.Bounds))
        //                    {

        //                        // Stash off the region we have to update (the base is going to clear this off in BeginPaint)
        //                        NativeMethods.RegionFlags updateRegionFlags = (NativeMethods.RegionFlags)SafeNativeMethods.GetUpdateRgn(new HandleRef(this, this.Handle), new HandleRef(this, wr.HRegion), true);

        //                        dr.CombineRegion(wr, dr, RegionCombineMode.DIFF);

        //                        Rectangle updateRegionBoundingRect = wr.ToRectangle();
        //                        FlatComboBoxAdapter.ValidateOwnerDrawRegions(this, updateRegionBoundingRect);
        //                        // Call the base class to do its painting (with a clipped DC).

        //                        NativeMethods.PAINTSTRUCT ps = new NativeMethods.PAINTSTRUCT();
        //                        IntPtr dc;
        //                        bool disposeDc = false;
        //                        if (m.WParam == IntPtr.Zero)
        //                        {
        //                            dc = UnsafeNativeMethods.BeginPaint(new HandleRef(this, Handle), ref ps);
        //                            disposeDc = true;
        //                        }
        //                        else
        //                        {
        //                            dc = m.WParam;
        //                        }

        //                        using (DeviceContext mDC = DeviceContext.FromHdc(dc))
        //                        {
        //                            using (WindowsGraphics wg = new WindowsGraphics(mDC))
        //                            {
        //                                if (updateRegionFlags != NativeMethods.RegionFlags.ERROR)
        //                                {
        //                                    wg.DeviceContext.SetClip(dr);
        //                                }
        //                                m.WParam = dc;
        //                                DefWndProc(ref m);
        //                                if (updateRegionFlags != NativeMethods.RegionFlags.ERROR)
        //                                {
        //                                    wg.DeviceContext.SetClip(wr);
        //                                }
        //                                using (Graphics g = Graphics.FromHdcInternal(dc))
        //                                {
        //                                    FlatComboBoxAdapter.DrawFlatCombo(this, g);
        //                                }
        //                            }
        //                        }

        //                        if (disposeDc)
        //                        {
        //                            UnsafeNativeMethods.EndPaint(new HandleRef(this, Handle), ref ps);
        //                        }

        //                    }
        //                    return;
        //                }
        //            }

        //            base.WndProc(ref m);
        //            break;
        //        case NativeMethods.WM_PRINTCLIENT:
        //            // all the fancy stuff we do in OnPaint has to happen again in OnPrint.
        //            if (GetStyle(ControlStyles.UserPaint) == false && FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup)
        //            {
        //                DefWndProc(ref m);

        //                if ((unchecked((int)(long)m.LParam) & NativeMethods.PRF_CLIENT) == NativeMethods.PRF_CLIENT)
        //                {
        //                    if (GetStyle(ControlStyles.UserPaint) == false && FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup)
        //                    {
        //                        using (Graphics g = Graphics.FromHdcInternal(m.WParam))
        //                        {
        //                            FlatComboBoxAdapter.DrawFlatCombo(this, g);
        //                        }
        //                    }
        //                    return;
        //                }
        //            }
        //            base.WndProc(ref m);
        //            return;
        //        case NativeMethods.WM_SETCURSOR:
        //            base.WndProc(ref m);
        //            break;

        //        case NativeMethods.WM_SETFONT:
        //            //(bug 119265)
        //            if (Width == 0)
        //            {
        //                suppressNextWindosPos = true;
        //            }
        //            base.WndProc(ref m);
        //            break;


        //        case NativeMethods.WM_WINDOWPOSCHANGED:
        //            if (!suppressNextWindosPos)
        //            {
        //                base.WndProc(ref m);
        //            }
        //            suppressNextWindosPos = false;
        //            break;

        //        case NativeMethods.WM_NCDESTROY:
        //            base.WndProc(ref m);
        //            ReleaseChildWindow();
        //            break;

        //        default:
        //            if (m.Msg == NativeMethods.WM_MOUSEENTER)
        //            {
        //                DefWndProc(ref m);
        //                OnMouseEnterInternal(EventArgs.Empty);
        //                break;
        //            }
        //            base.WndProc(ref m);
        //            break;
        //    }
        //}

        ////[ComVisible(true)]
        ////internal class ComboBoxChildNativeWindow : NativeWindow
        ////{














        ////}

        private sealed class ItemComparer : System.Collections.IComparer
        {
            private ComboBox comboBox;

            public ItemComparer(ComboBox comboBox)
            {
                this.comboBox = comboBox;
            }

            public int Compare(object item1, object item2)
            {
                if (item1 == null)
                {
                    if (item2 == null)
                        return 0; //both null, then they are equal

                    return -1; //item1 is null, but item2 is valid (greater)
                }
                if (item2 == null)
                    return 1; //item2 is null, so item 1 is greater

                String itemName1 = comboBox.GetItemText(item1);
                String itemName2 = comboBox.GetItemText(item2);

                //CompareInfo compInfo = (Application.CurrentCulture).CompareInfo;
                //return compInfo.Compare(itemName1, itemName2, CompareOptions.StringSort);
                return 0;
            }
        }

        [ListBindable(false)]
        public class ObjectCollection : IList
        {

            private ComboBox owner;
            private ArrayList innerList;
            private IComparer comparer;

            public ObjectCollection(ComboBox owner)
            {
                this.owner = owner;
            }

            private IComparer Comparer
            {
                get
                {
                    if (comparer == null)
                    {
                        comparer = new ItemComparer(owner);
                    }
                    return comparer;
                }
            }

            private ArrayList InnerList
            {
                get
                {
                    if (innerList == null)
                    {
                        innerList = new ArrayList();
                    }
                    return innerList;
                }
            }

            public int Count
            {
                get
                {
                    return InnerList.Count;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public int Add(object item)
            {
                owner.CheckNoDataSource();
                int index = AddInternal(item);
                if (owner.UpdateNeeded() && owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                {
                    owner.SetAutoComplete(false, false);
                }
                return index;
            }

            private int AddInternal(object item)
            {

                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                int index = -1;
                if (!owner.sorted)
                {
                    InnerList.Add(item);
                }
                else
                {
                    index = InnerList.BinarySearch(item, Comparer);
                    if (index < 0)
                    {
                        index = ~index; // getting the index of the first element that is larger than the search value
                    }

                    Debug.Assert(index >= 0 && index <= InnerList.Count, "Wrong index for insert");
                    InnerList.Insert(index, item);
                }
                bool successful = false;

                try
                {
                    if (owner.sorted)
                    {
                        if (owner.IsHandleCreated)
                        {
                            //owner.NativeInsert(index, item);
                        }
                    }
                    else
                    {
                        index = InnerList.Count - 1;
                        if (owner.IsHandleCreated)
                        {
                            //owner.NativeAdd(item);
                        }
                    }
                    successful = true;
                }
                finally
                {
                    if (!successful)
                    {
                        InnerList.Remove(item);
                    }
                }

                return index;
            }

            int IList.Add(object item)
            {
                return Add(item);
            }

            public void AddRange(object[] items)
            {
                owner.CheckNoDataSource();
                owner.BeginUpdate();
                try
                {
                    AddRangeInternal(items);
                }
                finally
                {
                    owner.EndUpdate();
                }
            }

            internal void AddRangeInternal(IList items)
            {

                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                foreach (object item in items)
                {
                    // adding items one-by-one for performance (especially for sorted combobox)
                    // we can not rely on ArrayList.Sort since its worst case complexity is n*n
                    // AddInternal is based on BinarySearch and ensures n*log(n) complexity
                    AddInternal(item);
                }
                if (owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                {
                    owner.SetAutoComplete(false, false);
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public virtual object this[int index]
            {
                get
                {
                    if (index < 0 || index >= InnerList.Count)
                    {
                        //throw new ArgumentOutOfRangeException("index", SR.GetString(SR.InvalidArgument, "index", (index).ToString(CultureInfo.CurrentCulture)));
                        throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                    }

                    return InnerList[index];
                }
                set
                {
                    owner.CheckNoDataSource();
                    SetItemInternal(index, value);
                }
            }

            public void Clear()
            {
                owner.CheckNoDataSource();
                ClearInternal();
            }

            internal void ClearInternal()
            {

                if (owner.IsHandleCreated)
                {
                    //owner.NativeClear();
                }

                InnerList.Clear();
                owner.selectedIndex = -1;
                if (owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                {
                    owner.SetAutoComplete(false, true /*recreateHandle*/);
                }
            }

            public bool Contains(object value)
            {
                return IndexOf(value) != -1;
            }

            public void CopyTo(object[] destination, int arrayIndex)
            {
                InnerList.CopyTo(destination, arrayIndex);
            }

            void ICollection.CopyTo(Array destination, int index)
            {
                InnerList.CopyTo(destination, index);
            }

            public IEnumerator GetEnumerator()
            {
                return InnerList.GetEnumerator();
            }

            public int IndexOf(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                return InnerList.IndexOf(value);
            }

            public void Insert(int index, object item)
            {
                owner.CheckNoDataSource();

                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                if (index < 0 || index > InnerList.Count)
                {
                    //throw new ArgumentOutOfRangeException("index", SR.GetString(SR.InvalidArgument, "index", (index).ToString(CultureInfo.CurrentCulture)));
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                // If the combo box is sorted, then nust treat this like an add
                // because we are going to twiddle the index anyway.
                //
                if (owner.sorted)
                {
                    Add(item);
                }
                else
                {
                    InnerList.Insert(index, item);
                    if (owner.IsHandleCreated)
                    {

                        bool successful = false;

                        try
                        {
                            //owner.NativeInsert(index, item);
                            successful = true;
                        }
                        finally
                        {
                            if (successful)
                            {
                                if (owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                                {
                                    owner.SetAutoComplete(false, false);
                                }
                            }
                            else
                            {
                                InnerList.RemoveAt(index);
                            }
                        }
                    }
                }
            }

            public void RemoveAt(int index)
            {
                owner.CheckNoDataSource();

                if (index < 0 || index >= InnerList.Count)
                {
                    //throw new ArgumentOutOfRangeException("index", SR.GetString(SR.InvalidArgument, "index", (index).ToString(CultureInfo.CurrentCulture)));
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                if (owner.IsHandleCreated)
                {
                    //owner.NativeRemoveAt(index);
                }

                InnerList.RemoveAt(index);
                if (!owner.IsHandleCreated && index < owner.selectedIndex)
                {
                    owner.selectedIndex--;
                }
                if (owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                {
                    owner.SetAutoComplete(false, false);
                }
            }

            public void Remove(object value)
            {

                int index = InnerList.IndexOf(value);

                if (index != -1)
                {
                    RemoveAt(index);
                }
            }

            internal void SetItemInternal(int index, object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (index < 0 || index >= InnerList.Count)
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                InnerList[index] = value;

                // If the native control has been created, and the display text of the new list item object
                // is different to the current text in the native list item, recreate the native list item...
                if (owner.IsHandleCreated)
                {
                    bool selected = (index == owner.SelectedIndex);

                    //if (String.Compare(this.owner.GetItemText(value), this.owner.NativeGetItemText(index), true, CultureInfo.CurrentCulture) != 0)
                    //{
                    //    owner.NativeRemoveAt(index);
                    //    owner.NativeInsert(index, value);
                    //    if (selected)
                    //    {
                    //        owner.SelectedIndex = index;
                    //        owner.UpdateText();
                    //    }
                    //    if (owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                    //    {
                    //        owner.SetAutoComplete(false, false);
                    //    }
                    //}
                    //else
                    {
                        // NEW - FOR COMPATIBILITY REASONS
                        // Minimum compatibility fix for VSWhidbey 377287/444903
                        if (selected)
                        {
                            owner.OnSelectedItemChanged(EventArgs.Empty);   //we do this because set_SelectedIndex does this. (for consistency)
                            owner.OnSelectedIndexChanged(EventArgs.Empty);
                        }
                    }
                }
            }

        } // end ObjectCollection

        //[ComVisible(true)]
        //public class ChildAccessibleObject : AccessibleObject
        //{

        //    ComboBox owner;

        //    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    public ChildAccessibleObject(ComboBox owner, IntPtr handle)
        //    {
        //        Debug.Assert(owner != null && owner.Handle != IntPtr.Zero, "ComboBox's handle hasn't been created");

        //        this.owner = owner;
        //        UseStdAccessibleObjects(handle);
        //    }

        //    public override string Name
        //    {
        //        get
        //        {
        //            return owner.AccessibilityObject.Name;
        //        }
        //    }
        //}

        //[System.Runtime.InteropServices.ComVisible(true)]
        //internal class ComboBoxAccessibleObject : ControlAccessibleObject
        //{

        //    private const int COMBOBOX_ACC_ITEM_INDEX = 1;

        //    public ComboBoxAccessibleObject(Control ownerControl)
        //        : base(ownerControl)
        //    {
        //    }

        //    internal override string get_accNameInternal(object childID)
        //    {
        //        this.ValidateChildID(ref childID);

        //        if (childID != null && ((int)childID) == COMBOBOX_ACC_ITEM_INDEX)
        //        {
        //            return this.Name;
        //        }
        //        else
        //        {
        //            return base.get_accNameInternal(childID);
        //        }
        //    }

        //    internal override string get_accKeyboardShortcutInternal(object childID)
        //    {
        //        this.ValidateChildID(ref childID);
        //        if (childID != null && ((int)childID) == COMBOBOX_ACC_ITEM_INDEX)
        //        {
        //            return this.KeyboardShortcut;
        //        }
        //        else
        //        {
        //            return base.get_accKeyboardShortcutInternal(childID);
        //        }
        //    }
        //}

        //[ComVisible(true)]
        //internal class ComboBoxExAccessibleObject : ComboBoxAccessibleObject
        //{

        //    private ComboBox ownerItem = null;

        //    private void ComboBoxDefaultAction(bool expand)
        //    {
        //        if (ownerItem.DroppedDown != expand)
        //        {
        //            ownerItem.DroppedDown = expand;
        //        }
        //    }

        //    public ComboBoxExAccessibleObject(ComboBox ownerControl)
        //        : base(ownerControl)
        //    {
        //        ownerItem = ownerControl;
        //    }

        //    internal override bool IsIAccessibleExSupported()
        //    {
        //        if (ownerItem != null)
        //        {
        //            return true;
        //        }
        //        return base.IsIAccessibleExSupported();
        //    }

        //    internal override bool IsPatternSupported(int patternId)
        //    {
        //        if (patternId == NativeMethods.UIA_ExpandCollapsePatternId)
        //        {
        //            if (ownerItem.DropDownStyle == ComboBoxStyle.Simple)
        //            {
        //                return false;
        //            }
        //            return true;
        //        }
        //        else
        //        {
        //            if (patternId == NativeMethods.UIA_ValuePatternId)
        //            {
        //                if (ownerItem.DropDownStyle == ComboBoxStyle.DropDownList)
        //                {
        //                    // NOTE: Initially the Value pattern is disabled for DropDownList in managed code,
        //                    // but despite of this MSAA provides true (when Level < 3). When UIA mode is enabled,
        //                    // Value pattern becomes disabled for the DropDownList and brings inconsistency.
        //                    // At this time keeping 'return false;' commented out and preserving Value pattern
        //                    // enabled in all cases: Level < 3 (by MSAA) and Level3 (by UIA).
        //                    // return false;
        //                    return AccessibilityImprovements.Level3;
        //                }
        //                return true;
        //            }
        //        }
        //        return base.IsPatternSupported(patternId);
        //    }

        //    internal override int[] RuntimeId
        //    {
        //        get
        //        {
        //            if (ownerItem != null)
        //            {
        //                // we need to provide a unique ID
        //                // others are implementing this in the same manner
        //                // first item is static - 0x2a (RuntimeIDFirstItem)
        //                // second item can be anything, but here it is a hash

        //                var runtimeId = new int[3];
        //                runtimeId[0] = RuntimeIDFirstItem;
        //                runtimeId[1] = (int)(long)ownerItem.Handle;
        //                runtimeId[2] = ownerItem.GetHashCode();
        //                return runtimeId;
        //            }

        //            return base.RuntimeId;
        //        }
        //    }

        //    internal override object GetPropertyValue(int propertyID)
        //    {

        //        switch (propertyID)
        //        {
        //            case NativeMethods.UIA_NamePropertyId:
        //                return Name;
        //            case NativeMethods.UIA_IsExpandCollapsePatternAvailablePropertyId:
        //                return (object)this.IsPatternSupported(NativeMethods.UIA_ExpandCollapsePatternId);
        //            case NativeMethods.UIA_IsValuePatternAvailablePropertyId:
        //                return (object)this.IsPatternSupported(NativeMethods.UIA_ValuePatternId);

        //            default:
        //                return base.GetPropertyValue(propertyID);
        //        }
        //    }

        //    internal override void Expand()
        //    {
        //        ComboBoxDefaultAction(true);
        //    }

        //    internal override void Collapse()
        //    {
        //        ComboBoxDefaultAction(false);
        //    }

        //    internal override UnsafeNativeMethods.ExpandCollapseState ExpandCollapseState
        //    {
        //        get
        //        {
        //            return ownerItem.DroppedDown == true ? UnsafeNativeMethods.ExpandCollapseState.Expanded : UnsafeNativeMethods.ExpandCollapseState.Collapsed;
        //        }
        //    }
        //}

        //[ComVisible(true)]
        //internal class ComboBoxItemAccessibleObject : AccessibleObject
        //{

        //    private ComboBox _owningComboBox;
        //    private object _owningItem;
        //    private IAccessible _systemIAccessible;

        //    public ComboBoxItemAccessibleObject(ComboBox owningComboBox, object owningItem)
        //    {
        //        _owningComboBox = owningComboBox;
        //        _owningItem = owningItem;

        //        _systemIAccessible = _owningComboBox.ChildListAccessibleObject.GetSystemIAccessibleInternal();
        //    }

        //    public override Rectangle Bounds
        //    {
        //        get
        //        {
        //            var listAccessibleObject = _owningComboBox.ChildListAccessibleObject;
        //            int currentIndex = GetCurrentIndex();

        //            var parentRect = listAccessibleObject.BoundingRectangle;
        //            int left = parentRect.Left;
        //            int top = parentRect.Top + _owningComboBox.ItemHeight * currentIndex;
        //            int width = parentRect.Width;
        //            int height = _owningComboBox.ItemHeight;

        //            return new Rectangle(left, top, width, height);
        //        }
        //    }

        //    public override string DefaultAction
        //    {
        //        get
        //        {
        //            return _systemIAccessible.accDefaultAction[GetChildId()];
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragment FragmentNavigate(UnsafeNativeMethods.NavigateDirection direction)
        //    {
        //        switch (direction)
        //        {
        //            case UnsafeNativeMethods.NavigateDirection.Parent:
        //                return _owningComboBox.ChildListAccessibleObject;
        //            case UnsafeNativeMethods.NavigateDirection.NextSibling:
        //                int currentIndex = GetCurrentIndex();
        //                var comboBoxChildListUiaProvider = _owningComboBox.ChildListAccessibleObject as ComboBoxChildListUiaProvider;
        //                if (comboBoxChildListUiaProvider != null)
        //                {
        //                    int itemsCount = comboBoxChildListUiaProvider.GetChildFragmentCount();
        //                    int nextItemIndex = currentIndex + 1;
        //                    if (itemsCount > nextItemIndex)
        //                    {
        //                        return comboBoxChildListUiaProvider.GetChildFragment(nextItemIndex);
        //                    }
        //                }
        //                break;
        //            case UnsafeNativeMethods.NavigateDirection.PreviousSibling:
        //                currentIndex = GetCurrentIndex();
        //                comboBoxChildListUiaProvider = _owningComboBox.ChildListAccessibleObject as ComboBoxChildListUiaProvider;
        //                if (comboBoxChildListUiaProvider != null)
        //                {
        //                    var itemsCount = comboBoxChildListUiaProvider.GetChildFragmentCount();
        //                    int previousItemIndex = currentIndex - 1;
        //                    if (previousItemIndex >= 0)
        //                    {
        //                        return comboBoxChildListUiaProvider.GetChildFragment(previousItemIndex);
        //                    }
        //                }

        //                break;
        //        }

        //        return base.FragmentNavigate(direction);
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragmentRoot FragmentRoot
        //    {
        //        get
        //        {
        //            return _owningComboBox.AccessibilityObject;
        //        }
        //    }

        //    private int GetCurrentIndex()
        //    {
        //        return _owningComboBox.Items.IndexOf(_owningItem);
        //    }

        //    internal override int GetChildId()
        //    {
        //        return GetCurrentIndex() + 1; // Index is zero-based, Child ID is 1-based.
        //    }

        //    internal override object GetPropertyValue(int propertyID)
        //    {
        //        switch (propertyID)
        //        {
        //            case NativeMethods.UIA_RuntimeIdPropertyId:
        //                return RuntimeId;
        //            case NativeMethods.UIA_BoundingRectanglePropertyId:
        //                return BoundingRectangle;
        //            case NativeMethods.UIA_ControlTypePropertyId:
        //                return NativeMethods.UIA_ListItemControlTypeId;
        //            case NativeMethods.UIA_NamePropertyId:
        //                return Name;
        //            case NativeMethods.UIA_AccessKeyPropertyId:
        //                return KeyboardShortcut ?? string.Empty;
        //            case NativeMethods.UIA_HasKeyboardFocusPropertyId:
        //                return _owningComboBox.Focused && _owningComboBox.SelectedIndex == GetCurrentIndex();
        //            case NativeMethods.UIA_IsKeyboardFocusablePropertyId:
        //                return (State & AccessibleStates.Focusable) == AccessibleStates.Focusable;
        //            case NativeMethods.UIA_IsEnabledPropertyId:
        //                return _owningComboBox.Enabled;
        //            case NativeMethods.UIA_HelpTextPropertyId:
        //                return Help ?? string.Empty;
        //            case NativeMethods.UIA_IsControlElementPropertyId:
        //                return true;
        //            case NativeMethods.UIA_IsContentElementPropertyId:
        //                return true;
        //            case NativeMethods.UIA_IsPasswordPropertyId:
        //                return false;
        //            case NativeMethods.UIA_IsOffscreenPropertyId:
        //                return (State & AccessibleStates.Offscreen) == AccessibleStates.Offscreen;
        //            case NativeMethods.UIA_IsSelectionItemPatternAvailablePropertyId:
        //                return true;
        //            case NativeMethods.UIA_SelectionItemIsSelectedPropertyId:
        //                return (State & AccessibleStates.Selected) != 0;
        //            case NativeMethods.UIA_SelectionItemSelectionContainerPropertyId:
        //                return _owningComboBox.ChildListAccessibleObject;

        //            default:
        //                return base.GetPropertyValue(propertyID);
        //        }
        //    }

        //    public override string Help
        //    {
        //        get
        //        {
        //            return _systemIAccessible.accHelp[GetChildId()];
        //        }
        //    }

        //    internal override bool IsPatternSupported(int patternId)
        //    {
        //        if (patternId == NativeMethods.UIA_LegacyIAccessiblePatternId ||
        //            patternId == NativeMethods.UIA_InvokePatternId ||
        //            patternId == NativeMethods.UIA_SelectionItemPatternId)
        //        {
        //            return true;
        //        }

        //        return base.IsPatternSupported(patternId);
        //    }

        //    public override string Name
        //    {
        //        get
        //        {
        //            if (_owningComboBox != null)
        //            {
        //                return _owningItem.ToString();
        //            }

        //            return base.Name;
        //        }

        //        set
        //        {
        //            base.Name = value;
        //        }
        //    }

        //    public override AccessibleRole Role
        //    {
        //        get
        //        {
        //            return (AccessibleRole)_systemIAccessible.get_accRole(GetChildId());
        //        }
        //    }

        //    internal override int[] RuntimeId
        //    {
        //        get
        //        {
        //            var runtimeId = new int[4];
        //            runtimeId[0] = RuntimeIDFirstItem;
        //            runtimeId[1] = (int)(long)_owningComboBox.Handle;
        //            runtimeId[2] = _owningComboBox.GetListNativeWindowRuntimeIdPart();
        //            runtimeId[3] = _owningItem.GetHashCode();

        //            return runtimeId;
        //        }
        //    }

        //    public override AccessibleStates State
        //    {
        //        get
        //        {
        //            return (AccessibleStates)_systemIAccessible.get_accState(GetChildId());
        //        }
        //    }

        //    internal override void SetFocus()
        //    {
        //        RaiseAutomationEvent(NativeMethods.UIA_AutomationFocusChangedEventId);

        //        base.SetFocus();
        //    }

        //    internal override void SelectItem()
        //    {
        //        _owningComboBox.SelectedIndex = GetCurrentIndex();

        //        SafeNativeMethods.InvalidateRect(new HandleRef(this, _owningComboBox.GetListHandle()), null, false);
        //    }

        //    internal override void AddToSelection()
        //    {
        //        SelectItem();
        //    }

        //    internal override void RemoveFromSelection()
        //    {
        //        // Do nothing, C++ implementation returns UIA_E_INVALIDOPERATION 0x80131509
        //    }

        //    internal override bool IsItemSelected
        //    {
        //        get
        //        {
        //            return (State & AccessibleStates.Selected) != 0;
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderSimple ItemSelectionContainer
        //    {
        //        get
        //        {
        //            return _owningComboBox.ChildListAccessibleObject;
        //        }
        //    }
        //}

        //internal class ComboBoxItemAccessibleObjectCollection : Hashtable
        //{

        //    private ComboBox _owningComboBoxBox;

        //    public ComboBoxItemAccessibleObjectCollection(ComboBox owningComboBoxBox)
        //    {
        //        _owningComboBoxBox = owningComboBoxBox;
        //    }

        //    public override object this[object key]
        //    {
        //        get
        //        {
        //            if (!ContainsKey(key))
        //            {
        //                var itemAccessibleObject = new ComboBoxItemAccessibleObject(_owningComboBoxBox, key);
        //                base[key] = itemAccessibleObject;
        //            }

        //            return base[key];
        //        }

        //        set
        //        {
        //            base[key] = value;
        //        }
        //    }
        //}

        //[ComVisible(true)]
        //internal class ComboBoxUiaProvider : ComboBoxExAccessibleObject
        //{
        //    private ComboBoxChildDropDownButtonUiaProvider _dropDownButtonUiaProvider;
        //    private ComboBoxItemAccessibleObjectCollection _itemAccessibleObjects;
        //    private ComboBox _owningComboBox;

        //    public ComboBoxUiaProvider(ComboBox owningComboBox) : base(owningComboBox)
        //    {
        //        _owningComboBox = owningComboBox;
        //        _itemAccessibleObjects = new ComboBoxItemAccessibleObjectCollection(owningComboBox);
        //    }

        //    public ComboBoxItemAccessibleObjectCollection ItemAccessibleObjects
        //    {
        //        get
        //        {
        //            return _itemAccessibleObjects;
        //        }
        //    }

        //    public ComboBoxChildDropDownButtonUiaProvider DropDownButtonUiaProvider
        //    {
        //        get
        //        {
        //            return (_dropDownButtonUiaProvider ?? new ComboBoxChildDropDownButtonUiaProvider(_owningComboBox, _owningComboBox.Handle));
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragment FragmentNavigate(UnsafeNativeMethods.NavigateDirection direction)
        //    {
        //        if (direction == UnsafeNativeMethods.NavigateDirection.FirstChild)
        //        {
        //            return GetChildFragment(0);
        //        }
        //        else if (direction == UnsafeNativeMethods.NavigateDirection.LastChild)
        //        {
        //            var childFragmentCount = GetChildFragmentCount();
        //            if (childFragmentCount > 0)
        //            {
        //                return GetChildFragment(childFragmentCount - 1);
        //            }
        //        }

        //        return base.FragmentNavigate(direction);
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragmentRoot FragmentRoot
        //    {
        //        get
        //        {
        //            return this;
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderSimple GetOverrideProviderForHwnd(IntPtr hwnd)
        //    {
        //        if (hwnd == _owningComboBox.childEdit.Handle)
        //        {
        //            return _owningComboBox.ChildEditAccessibleObject;
        //        }
        //        else if (
        //            hwnd == _owningComboBox.childListBox.Handle ||
        //            hwnd == _owningComboBox.dropDownHandle)
        //        {
        //            return _owningComboBox.ChildListAccessibleObject;
        //        }

        //        return null;
        //    }

        //    internal AccessibleObject GetChildFragment(int index)
        //    {
        //        if (_owningComboBox.DropDownStyle == ComboBoxStyle.DropDownList)
        //        {
        //            if (index == 0)
        //            {
        //                return _owningComboBox.ChildTextAccessibleObject;
        //            }

        //            index--;
        //        }

        //        if (index == 0 && _owningComboBox.DropDownStyle != ComboBoxStyle.Simple)
        //        {
        //            return DropDownButtonUiaProvider;
        //        }

        //        return null;
        //    }

        //    internal int GetChildFragmentCount()
        //    {
        //        int childFragmentCount = 0;

        //        if (_owningComboBox.DropDownStyle == ComboBoxStyle.DropDownList)
        //        {
        //            childFragmentCount++; // Text instead of edit for style is DropDownList but not DropDown.
        //        }

        //        if (_owningComboBox.DropDownStyle != ComboBoxStyle.Simple)
        //        {
        //            childFragmentCount++; // DropDown button.
        //        }

        //        return childFragmentCount;
        //    }

        //    internal override object GetPropertyValue(int propertyID)
        //    {
        //        switch (propertyID)
        //        {
        //            case NativeMethods.UIA_ControlTypePropertyId:
        //                return NativeMethods.UIA_ComboBoxControlTypeId;
        //            case NativeMethods.UIA_HasKeyboardFocusPropertyId:
        //                return _owningComboBox.Focused;
        //            case NativeMethods.UIA_NativeWindowHandlePropertyId:
        //                return _owningComboBox.Handle;

        //            default:
        //                return base.GetPropertyValue(propertyID);
        //        }
        //    }

        //    internal void ResetListItemAccessibleObjects()
        //    {
        //        _itemAccessibleObjects.Clear();
        //    }

        //    internal void SetComboBoxItemFocus()
        //    {
        //        var selectedItem = _owningComboBox.SelectedItem;
        //        if (selectedItem == null)
        //        {
        //            return;
        //        }

        //        var itemAccessibleObject = ItemAccessibleObjects[selectedItem] as ComboBoxItemAccessibleObject;
        //        if (itemAccessibleObject != null)
        //        {
        //            itemAccessibleObject.SetFocus();
        //        }
        //    }

        //    internal void SetComboBoxItemSelection()
        //    {
        //        var selectedItem = _owningComboBox.SelectedItem;
        //        if (selectedItem == null)
        //        {
        //            return;
        //        }

        //        var itemAccessibleObject = ItemAccessibleObjects[selectedItem] as ComboBoxItemAccessibleObject;
        //        if (itemAccessibleObject != null)
        //        {
        //            itemAccessibleObject.RaiseAutomationEvent(NativeMethods.UIA_SelectionItem_ElementSelectedEventId);
        //        }
        //    }

        //    internal override void SetFocus()
        //    {
        //        base.SetFocus();

        //        RaiseAutomationEvent(NativeMethods.UIA_AutomationFocusChangedEventId);
        //    }
        //}

        //[ComVisible(true)]
        //internal class ComboBoxChildEditUiaProvider : ChildAccessibleObject
        //{
        //    private const string COMBO_BOX_EDIT_AUTOMATION_ID = "1001";

        //    private ComboBox _owner;
        //    private IntPtr _handle;

        //    public ComboBoxChildEditUiaProvider(ComboBox owner, IntPtr childEditControlhandle) : base(owner, childEditControlhandle)
        //    {
        //        _owner = owner;
        //        _handle = childEditControlhandle;
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragment FragmentNavigate(UnsafeNativeMethods.NavigateDirection direction)
        //    {
        //        switch (direction)
        //        {
        //            case UnsafeNativeMethods.NavigateDirection.Parent:
        //                Debug.WriteLine("Edit parent " + _owner.AccessibilityObject.GetPropertyValue(NativeMethods.UIA_ControlTypePropertyId));
        //                return _owner.AccessibilityObject;
        //            case UnsafeNativeMethods.NavigateDirection.NextSibling:
        //                if (_owner.DropDownStyle == ComboBoxStyle.Simple)
        //                {
        //                    return null;
        //                }

        //                var comboBoxUiaProvider = _owner.AccessibilityObject as ComboBoxUiaProvider;
        //                if (comboBoxUiaProvider != null)
        //                {
        //                    int comboBoxChildFragmentCount = comboBoxUiaProvider.GetChildFragmentCount();
        //                    if (comboBoxChildFragmentCount > 1)
        //                    { // DropDown button is next;
        //                        return comboBoxUiaProvider.GetChildFragment(comboBoxChildFragmentCount - 1);
        //                    }
        //                }

        //                return null;
        //            case UnsafeNativeMethods.NavigateDirection.PreviousSibling:
        //                comboBoxUiaProvider = _owner.AccessibilityObject as ComboBoxUiaProvider;
        //                if (comboBoxUiaProvider != null)
        //                {
        //                    var firstComboBoxChildFragment = comboBoxUiaProvider.GetChildFragment(0);
        //                    if (RuntimeId != firstComboBoxChildFragment.RuntimeId)
        //                    {
        //                        return firstComboBoxChildFragment;
        //                    }
        //                }

        //                return null;
        //            default:
        //                return base.FragmentNavigate(direction);
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragmentRoot FragmentRoot
        //    {
        //        get
        //        {
        //            return _owner.AccessibilityObject;
        //        }
        //    }

        //    internal override object GetPropertyValue(int propertyID)
        //    {
        //        switch (propertyID)
        //        {
        //            case NativeMethods.UIA_RuntimeIdPropertyId:
        //                return RuntimeId;
        //            case NativeMethods.UIA_BoundingRectanglePropertyId:
        //                return Bounds;
        //            case NativeMethods.UIA_ControlTypePropertyId:
        //                return NativeMethods.UIA_EditControlTypeId;
        //            case NativeMethods.UIA_NamePropertyId:
        //                return Name;
        //            case NativeMethods.UIA_AccessKeyPropertyId:
        //                return string.Empty;
        //            case NativeMethods.UIA_HasKeyboardFocusPropertyId:
        //                return _owner.Focused;
        //            case NativeMethods.UIA_IsKeyboardFocusablePropertyId:
        //                return (State & AccessibleStates.Focusable) == AccessibleStates.Focusable;
        //            case NativeMethods.UIA_IsEnabledPropertyId:
        //                return _owner.Enabled;
        //            case NativeMethods.UIA_AutomationIdPropertyId:
        //                return COMBO_BOX_EDIT_AUTOMATION_ID;
        //            case NativeMethods.UIA_HelpTextPropertyId:
        //                return Help ?? string.Empty;
        //            case NativeMethods.UIA_IsPasswordPropertyId:
        //                return false;
        //            case NativeMethods.UIA_NativeWindowHandlePropertyId:
        //                return _handle;
        //            case NativeMethods.UIA_IsOffscreenPropertyId:
        //                return false;

        //            default:
        //                return base.GetPropertyValue(propertyID);
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderSimple HostRawElementProvider
        //    {
        //        get
        //        {
        //            if (AccessibilityImprovements.Level3)
        //            {
        //                UnsafeNativeMethods.IRawElementProviderSimple provider;
        //                UnsafeNativeMethods.UiaHostProviderFromHwnd(new HandleRef(this, _handle), out provider);
        //                return provider;
        //            }

        //            return base.HostRawElementProvider;
        //        }
        //    }

        //    internal override bool IsIAccessibleExSupported()
        //    {
        //        return true;
        //    }

        //    internal override int ProviderOptions
        //    {
        //        get
        //        {
        //            return (int)UnsafeNativeMethods.ProviderOptions.ClientSideProvider;
        //        }
        //    }

        //    internal override int[] RuntimeId
        //    {
        //        get
        //        {
        //            var runtimeId = new int[2];
        //            runtimeId[0] = RuntimeIDFirstItem;
        //            runtimeId[1] = GetHashCode();

        //            return runtimeId;
        //        }
        //    }
        //}

        //[ComVisible(true)]
        //internal class ComboBoxChildListUiaProvider : ChildAccessibleObject
        //{
        //    private const string COMBO_BOX_LIST_AUTOMATION_ID = "1000";

        //    private ComboBox _owningComboBox;
        //    private IntPtr _childListControlhandle;

        //    public ComboBoxChildListUiaProvider(ComboBox owningComboBox, IntPtr childListControlhandle) : base(owningComboBox, childListControlhandle)
        //    {
        //        _owningComboBox = owningComboBox;
        //        _childListControlhandle = childListControlhandle;
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragment ElementProviderFromPoint(double x, double y)
        //    {
        //        if (AccessibilityImprovements.Level3)
        //        {
        //            var systemIAccessible = GetSystemIAccessibleInternal();
        //            if (systemIAccessible != null)
        //            {
        //                object result = systemIAccessible.accHitTest((int)x, (int)y);
        //                if (result is int)
        //                {
        //                    int childId = (int)result;
        //                    return GetChildFragment(childId - 1);
        //                }
        //                else
        //                {
        //                    return null;
        //                }
        //            }
        //        }

        //        return base.ElementProviderFromPoint(x, y);
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragment FragmentNavigate(UnsafeNativeMethods.NavigateDirection direction)
        //    {
        //        switch (direction)
        //        {
        //            case UnsafeNativeMethods.NavigateDirection.FirstChild:
        //                return GetChildFragment(0);
        //            case UnsafeNativeMethods.NavigateDirection.LastChild:
        //                var childFragmentCount = GetChildFragmentCount();
        //                if (childFragmentCount > 0)
        //                {
        //                    return GetChildFragment(childFragmentCount - 1);
        //                }

        //                return null;
        //            default:
        //                return base.FragmentNavigate(direction);
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragmentRoot FragmentRoot
        //    {
        //        get
        //        {
        //            return _owningComboBox.AccessibilityObject;
        //        }
        //    }

        //    public AccessibleObject GetChildFragment(int index)
        //    {
        //        if (index < 0 || index >= _owningComboBox.Items.Count)
        //        {
        //            return null;
        //        }

        //        var item = _owningComboBox.Items[index];
        //        var comboBoxUiaProvider = _owningComboBox.AccessibilityObject as ComboBoxUiaProvider;
        //        return comboBoxUiaProvider.ItemAccessibleObjects[item] as AccessibleObject;
        //    }

        //    public int GetChildFragmentCount()
        //    {
        //        return _owningComboBox.Items.Count;
        //    }

        //    internal override object GetPropertyValue(int propertyID)
        //    {
        //        switch (propertyID)
        //        {
        //            case NativeMethods.UIA_RuntimeIdPropertyId:
        //                return RuntimeId;
        //            case NativeMethods.UIA_BoundingRectanglePropertyId:
        //                return Bounds;
        //            case NativeMethods.UIA_ControlTypePropertyId:
        //                return NativeMethods.UIA_ListControlTypeId;
        //            case NativeMethods.UIA_NamePropertyId:
        //                return Name;
        //            case NativeMethods.UIA_AccessKeyPropertyId:
        //                return string.Empty;
        //            case NativeMethods.UIA_HasKeyboardFocusPropertyId:
        //                return false; // Narrator should keep the keyboard focus on th ComboBox itself but not on the DropDown.
        //            case NativeMethods.UIA_IsKeyboardFocusablePropertyId:
        //                return (State & AccessibleStates.Focusable) == AccessibleStates.Focusable;
        //            case NativeMethods.UIA_IsEnabledPropertyId:
        //                return _owningComboBox.Enabled;
        //            case NativeMethods.UIA_AutomationIdPropertyId:
        //                return COMBO_BOX_LIST_AUTOMATION_ID;
        //            case NativeMethods.UIA_HelpTextPropertyId:
        //                return Help ?? string.Empty;
        //            case NativeMethods.UIA_IsPasswordPropertyId:
        //                return false;
        //            case NativeMethods.UIA_NativeWindowHandlePropertyId:
        //                return _childListControlhandle;
        //            case NativeMethods.UIA_IsOffscreenPropertyId:
        //                return false;
        //            case NativeMethods.UIA_IsSelectionPatternAvailablePropertyId:
        //                return true;
        //            case NativeMethods.UIA_SelectionCanSelectMultiplePropertyId:
        //                return CanSelectMultiple;
        //            case NativeMethods.UIA_SelectionIsSelectionRequiredPropertyId:
        //                return IsSelectionRequired;

        //            default:
        //                return base.GetPropertyValue(propertyID);
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragment GetFocus()
        //    {
        //        return GetFocused();
        //    }

        //    public override AccessibleObject GetFocused()
        //    {
        //        int selectedIndex = _owningComboBox.SelectedIndex;
        //        return GetChildFragment(selectedIndex);
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderSimple[] GetSelection()
        //    {
        //        int selectedIndex = _owningComboBox.SelectedIndex;

        //        AccessibleObject itemAccessibleObject = GetChildFragment(selectedIndex);
        //        if (itemAccessibleObject != null)
        //        {
        //            return new UnsafeNativeMethods.IRawElementProviderSimple[] {
        //                itemAccessibleObject
        //            };
        //        }

        //        return new UnsafeNativeMethods.IRawElementProviderSimple[0];
        //    }

        //    internal override bool CanSelectMultiple
        //    {
        //        get
        //        {
        //            return false;
        //        }
        //    }

        //    internal override bool IsSelectionRequired
        //    {
        //        get
        //        {
        //            return true;
        //        }
        //    }

        //    internal override bool IsPatternSupported(int patternId)
        //    {
        //        if (patternId == NativeMethods.UIA_LegacyIAccessiblePatternId ||
        //            patternId == NativeMethods.UIA_SelectionPatternId)
        //        {
        //            return true;
        //        }

        //        return base.IsPatternSupported(patternId);
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderSimple HostRawElementProvider
        //    {
        //        get
        //        {
        //            if (AccessibilityImprovements.Level3)
        //            {
        //                UnsafeNativeMethods.IRawElementProviderSimple provider;
        //                UnsafeNativeMethods.UiaHostProviderFromHwnd(new HandleRef(this, _childListControlhandle), out provider);
        //                return provider;
        //            }

        //            return base.HostRawElementProvider;
        //        }
        //    }

        //    internal override int[] RuntimeId
        //    {
        //        get
        //        {
        //            var runtimeId = new int[3];
        //            runtimeId[0] = RuntimeIDFirstItem;
        //            runtimeId[1] = (int)(long)_owningComboBox.Handle;
        //            runtimeId[2] = _owningComboBox.GetListNativeWindowRuntimeIdPart();

        //            return runtimeId;
        //        }
        //    }

        //    public override AccessibleStates State
        //    {
        //        get
        //        {
        //            AccessibleStates state = AccessibleStates.Focusable;
        //            if (_owningComboBox.Focused)
        //            {
        //                state |= AccessibleStates.Focused;
        //            }

        //            return state;
        //        }
        //    }
        //}

        //[ComVisible(true)]
        //internal class ComboBoxChildTextUiaProvider : AccessibleObject
        //{

        //    private const int COMBOBOX_TEXT_ACC_ITEM_INDEX = 1;

        //    private ComboBox _owner;

        //    public ComboBoxChildTextUiaProvider(ComboBox owner)
        //    {
        //        _owner = owner;
        //    }

        //    public override Rectangle Bounds
        //    {
        //        get
        //        {
        //            return _owner.AccessibilityObject.Bounds;
        //        }
        //    }

        //    internal override int GetChildId()
        //    {
        //        return COMBOBOX_TEXT_ACC_ITEM_INDEX;
        //    }

        //    public override string Name
        //    {
        //        get
        //        {
        //            return _owner.AccessibilityObject.Name ?? string.Empty;
        //        }
        //        set
        //        {
        //            // Do nothing.
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragment FragmentNavigate(UnsafeNativeMethods.NavigateDirection direction)
        //    {
        //        switch (direction)
        //        {
        //            case UnsafeNativeMethods.NavigateDirection.Parent:
        //                return _owner.AccessibilityObject;
        //            case UnsafeNativeMethods.NavigateDirection.NextSibling:
        //                var comboBoxUiaProvider = _owner.AccessibilityObject as ComboBoxUiaProvider;
        //                if (comboBoxUiaProvider != null)
        //                {
        //                    int comboBoxChildFragmentCount = comboBoxUiaProvider.GetChildFragmentCount();
        //                    if (comboBoxChildFragmentCount > 1)
        //                    { // DropDown button is next;
        //                        return comboBoxUiaProvider.GetChildFragment(comboBoxChildFragmentCount - 1);
        //                    }
        //                }

        //                return null;
        //            case UnsafeNativeMethods.NavigateDirection.PreviousSibling:
        //                comboBoxUiaProvider = _owner.AccessibilityObject as ComboBoxUiaProvider;
        //                if (comboBoxUiaProvider != null)
        //                {
        //                    var firstComboBoxChildFragment = comboBoxUiaProvider.GetChildFragment(0);
        //                    if (RuntimeId != firstComboBoxChildFragment.RuntimeId)
        //                    {
        //                        return firstComboBoxChildFragment;
        //                    }
        //                }

        //                return null;
        //            default:
        //                return base.FragmentNavigate(direction);
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragmentRoot FragmentRoot
        //    {
        //        get
        //        {
        //            return _owner.AccessibilityObject;
        //        }
        //    }

        //    internal override object GetPropertyValue(int propertyID)
        //    {
        //        switch (propertyID)
        //        {
        //            case NativeMethods.UIA_RuntimeIdPropertyId:
        //                return RuntimeId;
        //            case NativeMethods.UIA_BoundingRectanglePropertyId:
        //                return Bounds;
        //            case NativeMethods.UIA_ControlTypePropertyId:
        //                return NativeMethods.UIA_TextControlTypeId;
        //            case NativeMethods.UIA_NamePropertyId:
        //                return Name;
        //            case NativeMethods.UIA_AccessKeyPropertyId:
        //                return string.Empty;
        //            case NativeMethods.UIA_HasKeyboardFocusPropertyId:
        //                return _owner.Focused;
        //            case NativeMethods.UIA_IsKeyboardFocusablePropertyId:
        //                return (State & AccessibleStates.Focusable) == AccessibleStates.Focusable;
        //            case NativeMethods.UIA_IsEnabledPropertyId:
        //                return _owner.Enabled;
        //            case NativeMethods.UIA_HelpTextPropertyId:
        //                return Help ?? string.Empty;
        //            case NativeMethods.UIA_IsPasswordPropertyId:
        //            case NativeMethods.UIA_IsOffscreenPropertyId:
        //                return false;
        //            default:
        //                return base.GetPropertyValue(propertyID);
        //        }
        //    }

        //    internal override int[] RuntimeId
        //    {
        //        get
        //        {
        //            var runtimeId = new int[5];
        //            runtimeId[0] = RuntimeIDFirstItem;
        //            runtimeId[1] = (int)(long)_owner.Handle;
        //            runtimeId[2] = _owner.GetHashCode();
        //            runtimeId[3] = GetHashCode();
        //            runtimeId[4] = GetChildId();

        //            return runtimeId;
        //        }
        //    }

        //    public override AccessibleStates State
        //    {
        //        get
        //        {
        //            AccessibleStates state = AccessibleStates.Focusable;
        //            if (_owner.Focused)
        //            {
        //                state |= AccessibleStates.Focused;
        //            }

        //            return state;
        //        }
        //    }
        //}

        //[ComVisible(true)]
        //internal class ComboBoxChildDropDownButtonUiaProvider : AccessibleObject
        //{

        //    private const int COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX = 2;
        //    private ComboBox _owner;

        //    public ComboBoxChildDropDownButtonUiaProvider(ComboBox owner, IntPtr comboBoxControlhandle)
        //    {
        //        _owner = owner;
        //        UseStdAccessibleObjects(comboBoxControlhandle);
        //    }

        //    public override string Name
        //    {
        //        get
        //        {
        //            return get_accNameInternal(COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX);
        //        }
        //        set
        //        {
        //            var systemIAccessible = GetSystemIAccessibleInternal();
        //            systemIAccessible.set_accName(COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX, value);
        //        }
        //    }

        //    public override Rectangle Bounds
        //    {
        //        get
        //        {
        //            int left;
        //            int top;
        //            int width;
        //            int height;
        //            var systemIAccessible = GetSystemIAccessibleInternal();
        //            systemIAccessible.accLocation(out left, out top, out width, out height, COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX);
        //            return new Rectangle(left, top, width, height);
        //        }
        //    }

        //    public override string DefaultAction
        //    {
        //        get
        //        {
        //            var systemIAccessible = GetSystemIAccessibleInternal();
        //            return systemIAccessible.accDefaultAction[COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX];
        //        }
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragment FragmentNavigate(UnsafeNativeMethods.NavigateDirection direction)
        //    {
        //        if (direction == UnsafeNativeMethods.NavigateDirection.Parent)
        //        {
        //            return _owner.AccessibilityObject;
        //        }
        //        else if (direction == UnsafeNativeMethods.NavigateDirection.PreviousSibling)
        //        {
        //            var comboBoxUiaProvider = _owner.AccessibilityObject as ComboBoxUiaProvider;
        //            if (comboBoxUiaProvider != null)
        //            {
        //                int comboBoxChildFragmentCount = comboBoxUiaProvider.GetChildFragmentCount();
        //                if (comboBoxChildFragmentCount > 1)
        //                { // Text or edit is previous;
        //                    return comboBoxUiaProvider.GetChildFragment(comboBoxChildFragmentCount - 1);
        //                }
        //            }

        //            return null;
        //        }

        //        return base.FragmentNavigate(direction);
        //    }

        //    internal override UnsafeNativeMethods.IRawElementProviderFragmentRoot FragmentRoot
        //    {
        //        get
        //        {
        //            return _owner.AccessibilityObject;
        //        }
        //    }

        //    internal override int GetChildId()
        //    {
        //        return COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX;
        //    }

        //    internal override object GetPropertyValue(int propertyID)
        //    {
        //        switch (propertyID)
        //        {
        //            case NativeMethods.UIA_RuntimeIdPropertyId:
        //                return RuntimeId;
        //            case NativeMethods.UIA_BoundingRectanglePropertyId:
        //                return BoundingRectangle;
        //            case NativeMethods.UIA_ControlTypePropertyId:
        //                return NativeMethods.UIA_ButtonControlTypeId;
        //            case NativeMethods.UIA_NamePropertyId:
        //                return Name;
        //            case NativeMethods.UIA_AccessKeyPropertyId:
        //                return KeyboardShortcut;
        //            case NativeMethods.UIA_HasKeyboardFocusPropertyId:
        //                return _owner.Focused;
        //            case NativeMethods.UIA_IsKeyboardFocusablePropertyId:
        //                return (State & AccessibleStates.Focusable) == AccessibleStates.Focusable;
        //            case NativeMethods.UIA_IsEnabledPropertyId:
        //                return _owner.Enabled;
        //            case NativeMethods.UIA_HelpTextPropertyId:
        //                return Help ?? string.Empty;
        //            case NativeMethods.UIA_IsPasswordPropertyId:
        //                return false;
        //            case NativeMethods.UIA_IsOffscreenPropertyId:
        //                return (State & AccessibleStates.Offscreen) == AccessibleStates.Offscreen;
        //            default:
        //                return base.GetPropertyValue(propertyID);
        //        }
        //    }

        //    public override string Help
        //    {
        //        get
        //        {
        //            var systemIAccessible = GetSystemIAccessibleInternal();
        //            return systemIAccessible.accHelp[COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX];
        //        }
        //    }

        //    public override string KeyboardShortcut
        //    {
        //        get
        //        {
        //            var systemIAccessible = GetSystemIAccessibleInternal();
        //            return systemIAccessible.get_accKeyboardShortcut(COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX);
        //        }
        //    }

        //    internal override bool IsPatternSupported(int patternId)
        //    {
        //        if (patternId == NativeMethods.UIA_LegacyIAccessiblePatternId ||
        //            patternId == NativeMethods.UIA_InvokePatternId)
        //        {
        //            return true;
        //        }

        //        return base.IsPatternSupported(patternId);
        //    }

        //    public override AccessibleRole Role
        //    {
        //        get
        //        {
        //            var systemIAccessible = GetSystemIAccessibleInternal();
        //            return (AccessibleRole)systemIAccessible.get_accRole(COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX);
        //        }
        //    }

        //    internal override int[] RuntimeId
        //    {
        //        get
        //        {
        //            var runtimeId = new int[5];
        //            runtimeId[0] = RuntimeIDFirstItem;
        //            runtimeId[1] = (int)(long)_owner.Handle;
        //            runtimeId[2] = _owner.GetHashCode();

        //            // Made up constant from MSAA proxy. When MSAA proxy is used as an accessibility provider,
        //            // the similar Runtime ID is returned (for consistency purpose)
        //            const int generatedRuntimeId = 61453;
        //            runtimeId[3] = generatedRuntimeId;
        //            runtimeId[4] = COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX;
        //            return runtimeId;
        //        }
        //    }

        //    public override AccessibleStates State
        //    {
        //        get
        //        {
        //            var systemIAccessible = GetSystemIAccessibleInternal();
        //            return (AccessibleStates)systemIAccessible.get_accState(COMBOBOX_DROPDOWN_BUTTON_ACC_ITEM_INDEX);
        //        }
        //    }
        //}

        //private sealed class ACNativeWindow : NativeWindow
        //{
        //    static internal int inWndProcCnt;
        //    //this hashtable can contain null for those ACWindows we find, but are sure are not ours.
        //    static private Hashtable ACWindows = new Hashtable();

        //    [
        //        SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")
        //    ]
        //    internal ACNativeWindow(IntPtr acHandle)
        //    {
        //        Debug.Assert(!ACWindows.ContainsKey(acHandle));
        //        this.AssignHandle(acHandle);
        //        ACWindows.Add(acHandle, this);
        //        UnsafeNativeMethods.EnumChildWindows(new HandleRef(this, acHandle),
        //            new NativeMethods.EnumChildrenCallback(ACNativeWindow.RegisterACWindowRecursive),
        //            NativeMethods.NullHandleRef);
        //    }

        //    [SuppressMessage("Microsoft.Performance", "CA1806:DoNotIgnoreMethodResults")]
        //    private static bool RegisterACWindowRecursive(IntPtr handle, IntPtr lparam)
        //    {
        //        if (!ACWindows.ContainsKey(handle))
        //        {
        //            ACNativeWindow newAC = new ACNativeWindow(handle);
        //        }
        //        return true;
        //    }

        //    internal bool Visible
        //    {
        //        get
        //        {
        //            return SafeNativeMethods.IsWindowVisible(new HandleRef(this, Handle));
        //        }
        //    }

        //    static internal bool AutoCompleteActive
        //    {
        //        get
        //        {
        //            if (inWndProcCnt > 0)
        //            {
        //                return true;
        //            }
        //            foreach (object o in ACWindows.Values)
        //            {
        //                ACNativeWindow window = o as ACNativeWindow;
        //                if (window != null && window.Visible)
        //                {
        //                    return true;
        //                }
        //            }
        //            return false;
        //        }
        //    }

        //    protected override void WndProc(ref Message m)
        //    {
        //        inWndProcCnt++;
        //        try
        //        {
        //            base.WndProc(ref m);
        //        }
        //        finally
        //        {
        //            inWndProcCnt--;
        //        }

        //        if (m.Msg == NativeMethods.WM_NCDESTROY)
        //        {
        //            Debug.Assert(ACWindows.ContainsKey(this.Handle));
        //            ACWindows.Remove(this.Handle);   //so we do not leak ac windows.
        //        }
        //    }

        //    [SuppressMessage("Microsoft.Performance", "CA1806:DoNotIgnoreMethodResults")]
        //    internal static void RegisterACWindow(IntPtr acHandle, bool subclass)
        //    {
        //        if (subclass && ACWindows.ContainsKey(acHandle))
        //        {
        //            if (ACWindows[acHandle] == null)
        //            {
        //                ACWindows.Remove(acHandle); //if an external handle got destroyed, dont let it stop us.
        //            }
        //        }

        //        if (!ACWindows.ContainsKey(acHandle))
        //        {
        //            if (subclass)
        //            {
        //                ACNativeWindow newAC = new ACNativeWindow(acHandle);
        //            }
        //            else
        //            {
        //                ACWindows.Add(acHandle, null);
        //            }
        //        }
        //    }

        //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1808:AvoidCallsThatBoxValueTypes")] // Perfectly acceptible when dealing with PropertyDescriptors
        //    internal static void ClearNullACWindows()
        //    {
        //        ArrayList nulllist = new ArrayList();
        //        foreach (DictionaryEntry e in ACWindows)
        //        {
        //            if (e.Value == null)
        //            {
        //                nulllist.Add(e.Key);
        //            }
        //        }
        //        foreach (IntPtr handle in nulllist)
        //        {
        //            ACWindows.Remove(handle);
        //        }
        //    }
        //}

        //        private class AutoCompleteDropDownFinder
        //        {
        //            private const int MaxClassName = 256;
        //            private const string AutoCompleteClassName = "Auto-Suggest Dropdown";
        //            bool shouldSubClass = false; //nonstatic

        //            //internal void FindDropDowns()
        //            //{
        //            //    FindDropDowns(true);
        //            //}

        //            //internal void FindDropDowns(bool subclass)
        //            //{
        //            //    if (!subclass)
        //            //    {
        //            //        //generating a before snapshot -- lets lose the null handles
        //            //        ACNativeWindow.ClearNullACWindows();
        //            //    }
        //            //    // Look for a popped up dropdown
        //            //    shouldSubClass = subclass;
        //            //    UnsafeNativeMethods.EnumThreadWindows(SafeNativeMethods.GetCurrentThreadId(), new NativeMethods.EnumThreadWindowsCallback(this.Callback), new HandleRef(null, IntPtr.Zero));
        //            //}

        //            //private bool Callback(IntPtr hWnd, IntPtr lParam)
        //            //{
        //            //    HandleRef hRef = new HandleRef(null, hWnd);

        //            //    // Check class name and see if it's visible
        //            //    if (GetClassName(hRef) == AutoCompleteClassName)
        //            //    {
        //            //        ACNativeWindow.RegisterACWindow(hRef.Handle, shouldSubClass);
        //            //    }

        //            //    return true;
        //            //}

        //            //static string GetClassName(HandleRef hRef)
        //            //{
        //            //    StringBuilder sb = new StringBuilder(MaxClassName);
        //            //    UnsafeNativeMethods.GetClassName(hRef, sb, MaxClassName);
        //            //    return sb.ToString();
        //            //}
        //        }


        //        //private FlatComboAdapter FlatComboBoxAdapter
        //        //{
        //        //    get
        //        //    {
        //        //        FlatComboAdapter comboAdapter = Properties.GetObject(PropFlatComboAdapter) as FlatComboAdapter;
        //        //        if (comboAdapter == null || !comboAdapter.IsValid(this))
        //        //        {
        //        //            comboAdapter = CreateFlatComboAdapterInstance();
        //        //            Properties.SetObject(PropFlatComboAdapter, comboAdapter);
        //        //        }
        //        //        return comboAdapter;
        //        //    }
        //        //}

        //        //internal virtual FlatComboAdapter CreateFlatComboAdapterInstance()
        //        //{
        //        //    return new FlatComboAdapter(this,/*smallButton=*/false);
        //        //}


        //        internal class FlatComboAdapter
        //        {
        //            Rectangle outerBorder;
        //            Rectangle innerBorder;
        //            Rectangle innerInnerBorder;
        //            internal Rectangle dropDownRect;
        //            Rectangle whiteFillRect;
        //            Rectangle clientRect;

        //            RightToLeft origRightToLeft; // The combo box's RTL value when we were created

        //            private const int WhiteFillRectWidth = 5; // used for making the button look smaller than it is

        //            private static bool isScalingInitialized = false;
        //            private static int OFFSET_2PIXELS = 2;
        //            protected static int Offset2Pixels = OFFSET_2PIXELS;

        //            //public FlatComboAdapter(ComboBox comboBox, bool smallButton)
        //            //{
        //            //    // adapter is re-created when combobox is resized, see IsValid method, thus we don't need to handle DPI changed explicitly 
        //            //    if ((!isScalingInitialized && DpiHelper.IsScalingRequired) || DpiHelper.EnableDpiChangedMessageHandling)
        //            //    {
        //            //        Offset2Pixels = comboBox.LogicalToDeviceUnits(OFFSET_2PIXELS);
        //            //        isScalingInitialized = true;
        //            //    }

        //            //    clientRect = comboBox.ClientRectangle;
        //            //    int dropDownButtonWidth = SystemInformation.GetHorizontalScrollBarArrowWidthForDpi(comboBox.deviceDpi);
        //            //    outerBorder = new Rectangle(clientRect.Location, new Size(clientRect.Width - 1, clientRect.Height - 1));
        //            //    innerBorder = new Rectangle(outerBorder.X + 1, outerBorder.Y + 1, outerBorder.Width - dropDownButtonWidth - 2, outerBorder.Height - 2);
        //            //    innerInnerBorder = new Rectangle(innerBorder.X + 1, innerBorder.Y + 1, innerBorder.Width - 2, innerBorder.Height - 2);
        //            //    dropDownRect = new Rectangle(innerBorder.Right + 1, innerBorder.Y, dropDownButtonWidth, innerBorder.Height + 1);


        //            //    // fill in several pixels of the dropdown rect with white so that it looks like the combo button is thinner.
        //            //    if (smallButton)
        //            //    {
        //            //        whiteFillRect = dropDownRect;
        //            //        whiteFillRect.Width = WhiteFillRectWidth;
        //            //        dropDownRect.X += WhiteFillRectWidth;
        //            //        dropDownRect.Width -= WhiteFillRectWidth;
        //            //    }

        //            //    origRightToLeft = comboBox.RightToLeft;


        //            //    if (origRightToLeft == RightToLeft.Yes)
        //            //    {
        //            //        innerBorder.X = clientRect.Width - innerBorder.Right;
        //            //        innerInnerBorder.X = clientRect.Width - innerInnerBorder.Right;
        //            //        dropDownRect.X = clientRect.Width - dropDownRect.Right;
        //            //        whiteFillRect.X = clientRect.Width - whiteFillRect.Right + 1;  // since we're filling, we need to move over to the next px.
        //            //    }

        //            //}

        //            //public bool IsValid(ComboBox combo)
        //            //{
        //            //    return (combo.ClientRectangle == clientRect && combo.RightToLeft == origRightToLeft);
        //            //}

        //            //public virtual void DrawFlatCombo(ComboBox comboBox, Graphics g)
        //            //{
        //            //    if (comboBox.DropDownStyle == ComboBoxStyle.Simple)
        //            //    {
        //            //        return;
        //            //    }

        //            //    Color outerBorderColor = GetOuterBorderColor(comboBox);
        //            //    Color innerBorderColor = GetInnerBorderColor(comboBox);
        //            //    bool rightToLeft = comboBox.RightToLeft == RightToLeft.Yes;

        //            //    // draw the drop down
        //            //    DrawFlatComboDropDown(comboBox, g, dropDownRect);

        //            //    // when we are disabled there is one line of color that seems to eek through if backcolor is set
        //            //    // so lets erase it.
        //            //    if (!LayoutUtils.IsZeroWidthOrHeight(whiteFillRect))
        //            //    {
        //            //        // fill in two more pixels with white so it looks smaller.
        //            //        using (Brush b = new SolidBrush(innerBorderColor))
        //            //        {
        //            //            g.FillRectangle(b, whiteFillRect);
        //            //        }
        //            //    }



        //            //    // Draw the outer border
        //            //    if (outerBorderColor.IsSystemColor)
        //            //    {
        //            //        Pen outerBorderPen = SystemPens.FromSystemColor(outerBorderColor);
        //            //        g.DrawRectangle(outerBorderPen, outerBorder);
        //            //        if (rightToLeft)
        //            //        {
        //            //            g.DrawRectangle(outerBorderPen, new Rectangle(outerBorder.X, outerBorder.Y, dropDownRect.Width + 1, outerBorder.Height));
        //            //        }
        //            //        else
        //            //        {
        //            //            g.DrawRectangle(outerBorderPen, new Rectangle(dropDownRect.X, outerBorder.Y, outerBorder.Right - dropDownRect.X, outerBorder.Height));
        //            //        }
        //            //    }
        //            //    else
        //            //    {
        //            //        using (Pen outerBorderPen = new Pen(outerBorderColor))
        //            //        {
        //            //            g.DrawRectangle(outerBorderPen, outerBorder);
        //            //            if (rightToLeft)
        //            //            {
        //            //                g.DrawRectangle(outerBorderPen, new Rectangle(outerBorder.X, outerBorder.Y, dropDownRect.Width + 1, outerBorder.Height));
        //            //            }
        //            //            else
        //            //            {
        //            //                g.DrawRectangle(outerBorderPen, new Rectangle(dropDownRect.X, outerBorder.Y, outerBorder.Right - dropDownRect.X, outerBorder.Height));
        //            //            }
        //            //        }
        //            //    }

        //            //    // Draw the inner border
        //            //    if (innerBorderColor.IsSystemColor)
        //            //    {
        //            //        Pen innerBorderPen = SystemPens.FromSystemColor(innerBorderColor);
        //            //        g.DrawRectangle(innerBorderPen, innerBorder);
        //            //        g.DrawRectangle(innerBorderPen, innerInnerBorder);
        //            //    }
        //            //    else
        //            //    {
        //            //        using (Pen innerBorderPen = new Pen(innerBorderColor))
        //            //        {
        //            //            g.DrawRectangle(innerBorderPen, innerBorder);
        //            //            g.DrawRectangle(innerBorderPen, innerInnerBorder);
        //            //        }
        //            //    }



        //            //    // Draw a dark border around everything if we're in popup mode
        //            //    if ((!comboBox.Enabled) || (comboBox.FlatStyle == FlatStyle.Popup))
        //            //    {
        //            //        bool focused = comboBox.ContainsFocus || comboBox.MouseIsOver;
        //            //        Color borderPenColor = GetPopupOuterBorderColor(comboBox, focused);

        //            //        using (Pen borderPen = new Pen(borderPenColor))
        //            //        {

        //            //            Pen innerPen = (comboBox.Enabled) ? borderPen : SystemPens.Control;

        //            //            // around the dropdown
        //            //            if (rightToLeft)
        //            //            {
        //            //                g.DrawRectangle(innerPen, new Rectangle(outerBorder.X, outerBorder.Y, dropDownRect.Width + 1, outerBorder.Height));
        //            //            }
        //            //            else
        //            //            {
        //            //                g.DrawRectangle(innerPen, new Rectangle(dropDownRect.X, outerBorder.Y, outerBorder.Right - dropDownRect.X, outerBorder.Height));
        //            //            }

        //            //            // around the whole combobox.
        //            //            g.DrawRectangle(borderPen, outerBorder);

        //            //        }
        //            //    }

        //            //}


        //            //protected virtual void DrawFlatComboDropDown(ComboBox comboBox, Graphics g, Rectangle dropDownRect)
        //            //{

        //            //    g.FillRectangle(SystemBrushes.Control, dropDownRect);

        //            //    Brush brush = (comboBox.Enabled) ? SystemBrushes.ControlText : SystemBrushes.ControlDark;

        //            //    Point middle = new Point(dropDownRect.Left + dropDownRect.Width / 2, dropDownRect.Top + dropDownRect.Height / 2);
        //            //    if (origRightToLeft == RightToLeft.Yes)
        //            //    {
        //            //        // if the width is odd - favor pushing it over one pixel left.
        //            //        middle.X -= (dropDownRect.Width % 2);
        //            //    }
        //            //    else
        //            //    {
        //            //        // if the width is odd - favor pushing it over one pixel right.
        //            //        middle.X += (dropDownRect.Width % 2);
        //            //    }

        //            //    g.FillPolygon(brush, new Point[] {
        //            //         new Point(middle.X - Offset2Pixels, middle.Y - 1),
        //            //         new Point(middle.X + Offset2Pixels + 1, middle.Y - 1),
        //            //         new Point(middle.X, middle.Y + Offset2Pixels)
        //            //     });
        //            //}

        //            protected virtual Color GetOuterBorderColor(ComboBox comboBox)
        //            {
        //                return (comboBox.Enabled) ? SystemColors.Window : SystemColors.ControlDark;
        //            }

        //            protected virtual Color GetPopupOuterBorderColor(ComboBox comboBox, bool focused)
        //            {
        //                if (!comboBox.Enabled)
        //                {
        //                    return SystemColors.ControlDark;
        //                }
        //                return (focused) ? SystemColors.ControlDark : SystemColors.Window;
        //            }

        //            protected virtual Color GetInnerBorderColor(ComboBox comboBox)
        //            {
        //                return (comboBox.Enabled) ? comboBox.BackColor : SystemColors.Control;
        //            }

        //            // this eliminates flicker by removing the pieces we're going to paint ourselves from 
        //            // the update region.  Note the UpdateRegionBox is the bounding box of the actual update region.
        //            // this is just here so we can quickly eliminate rectangles that arent in the update region.
        //            //public void ValidateOwnerDrawRegions(ComboBox comboBox, Rectangle updateRegionBox)
        //            //{
        //            //    NativeMethods.RECT validRect;
        //            //    if (comboBox != null) { return; }
        //            //    Rectangle topOwnerDrawArea = new Rectangle(0, 0, comboBox.Width, innerBorder.Top);
        //            //    Rectangle bottomOwnerDrawArea = new Rectangle(0, innerBorder.Bottom, comboBox.Width, comboBox.Height - innerBorder.Bottom);
        //            //    Rectangle leftOwnerDrawArea = new Rectangle(0, 0, innerBorder.Left, comboBox.Height);
        //            //    Rectangle rightOwnerDrawArea = new Rectangle(innerBorder.Right, 0, comboBox.Width - innerBorder.Right, comboBox.Height);

        //            //    if (topOwnerDrawArea.IntersectsWith(updateRegionBox))
        //            //    {
        //            //        validRect = new NativeMethods.RECT(topOwnerDrawArea);
        //            //        SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref validRect);
        //            //    }

        //            //    if (bottomOwnerDrawArea.IntersectsWith(updateRegionBox))
        //            //    {
        //            //        validRect = new NativeMethods.RECT(bottomOwnerDrawArea);
        //            //        SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref validRect);
        //            //    }

        //            //    if (leftOwnerDrawArea.IntersectsWith(updateRegionBox))
        //            //    {
        //            //        validRect = new NativeMethods.RECT(leftOwnerDrawArea);
        //            //        SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref validRect);
        //            //    }

        //            //    if (rightOwnerDrawArea.IntersectsWith(updateRegionBox))
        //            //    {
        //            //        validRect = new NativeMethods.RECT(rightOwnerDrawArea);
        //            //        SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref validRect);
        //            //    }

        //            //}
        //        }

        internal enum ChildWindowType
        {
            ListBox,
            Edit,
            DropDownList
        }
    }
}