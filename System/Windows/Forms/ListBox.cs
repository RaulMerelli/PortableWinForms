namespace System.Windows.Forms
{
    using Microsoft.CodeAnalysis;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms.Layout;
    using System.Windows.Forms.VisualStyles;

    [
    ComVisible(true),
    ClassInterface(ClassInterfaceType.AutoDispatch),
    DefaultEvent("SelectedIndexChanged"),
    DefaultProperty("Items"),
    DefaultBindingProperty("SelectedValue"),
    ]
    public class ListBox : ListControl
    {
        public const int NoMatches = NativeMethods.LB_ERR;
        public const int DefaultItemHeight = 13; // 13 == listbox's non-ownerdraw item height.  That's with Win2k and
                                                 // the default font; on other platforms and with other fonts, it may be different.

        private const int maxWin9xHeight = 32767; //Win9x doesn't deal with height > 32K

        private static readonly object EVENT_SELECTEDINDEXCHANGED = new object();
        private static readonly object EVENT_DRAWITEM = new object();
        private static readonly object EVENT_MEASUREITEM = new object();

        static bool checkedOS = false;
        static bool runningOnWin2K = true;

        SelectedObjectCollection selectedItems;
        SelectedIndexCollection selectedIndices;
        ObjectCollection itemsCollection;

        // 

        int itemHeight = DefaultItemHeight;
        int columnWidth;
        int requestedHeight;
        int topIndex;
        int horizontalExtent = 0;
        int maxWidth = -1;
        int updateCount = 0;

        bool sorted = false;
        bool scrollAlwaysVisible = false;
        bool integralHeight = true;
        bool integralHeightAdjust = false;
        bool multiColumn = false;
        bool horizontalScrollbar = false;
        bool useTabStops = true;
        bool useCustomTabOffsets = false;
        bool fontIsChanged = false;
        bool doubleClickFired = false;
        bool selectedValueChangedFired = false;

        DrawMode drawMode = System.Windows.Forms.DrawMode.Normal;
        BorderStyle borderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        SelectionMode selectionMode = System.Windows.Forms.SelectionMode.One;

        // VsWhidbey : 447524
        SelectionMode cachedSelectionMode = System.Windows.Forms.SelectionMode.One;
        //We need to know that we are in middle of handleRecreate through Setter of SelectionMode. 
        //In this case we set a bool denoting that we are changing SelectionMode and 
        //in this case we should always use the cachedValue instead of the currently set value. 
        //We need to change this in the count as well as SelectedIndex code where we access the SelectionMode.
        private bool selectionModeChanging = false;

        private IntegerCollection customTabOffsets;

        private const int defaultListItemStartPos = 1;

        private const int defaultListItemBorderHeight = 1;

        private const int defaultListItemPaddingBuffer = 3;


        internal int scaledListItemStartPosition = defaultListItemStartPos;
        internal int scaledListItemBordersHeight = 2 * defaultListItemBorderHeight;
        internal int scaledListItemPaddingBuffer = defaultListItemPaddingBuffer;


        public ListBox() : base()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.StandardClick |
                     ControlStyles.UseTextForAccessibility, false);

            // this class overrides GetPreferredSizeCore, let Control automatically cache the result
            SetState2(STATE2_USEPREFERREDSIZECACHE, true);

            SetBounds(0, 0, 120, 96);

            requestedHeight = Height;

            PrepareForDrawing();
        }

        //protected override void RescaleConstantsForDpi(int deviceDpiOld, int deviceDpiNew)
        //{
        //    base.RescaleConstantsForDpi(deviceDpiOld, deviceDpiNew);
        //    PrepareForDrawing();
        //}

        private void PrepareForDrawing()
        {
            // Scale paddings
            //if (DpiHelper.EnableCheckedListBoxHighDpiImprovements)
            //{
            //    scaledListItemStartPosition = LogicalToDeviceUnits(defaultListItemStartPos);

            //    // height inlude 2 borders ( top and bottom). we are using multiplication by 2 instead of scaling doubled value to get an even number 
            //    // that might helps us in positioning control in the center for list items.
            //    scaledListItemBordersHeight = 2 * LogicalToDeviceUnits(defaultListItemBorderHeight);
            //    scaledListItemPaddingBuffer = LogicalToDeviceUnits(defaultListItemPaddingBuffer);
            //}
        }

        public override Color BackColor
        {
            get
            {
                if (ShouldSerializeBackColor())
                {
                    return base.BackColor;
                }
                else
                {
                    return SystemColors.Window;
                }
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

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
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
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

        [
        DefaultValue(BorderStyle.Fixed3D),
        DispId(NativeMethods.ActiveX.DISPID_BORDERSTYLE),
        ]
        public BorderStyle BorderStyle
        {
            get
            {
                return borderStyle;
            }

            set
            {
                //valid values are 0x0 to 0x2
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)BorderStyle.None, (int)BorderStyle.Fixed3D))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(BorderStyle));
                }

                if (value != borderStyle)
                {
                    borderStyle = value;
                    RecreateHandle();
                    // Avoid the listbox and textbox behavior in Collection editors
                    //
                    integralHeightAdjust = true;
                    try
                    {
                        Height = requestedHeight;
                    }
                    finally
                    {
                        integralHeightAdjust = false;
                    }
                }
            }
        }

        [
        Localizable(true),
        DefaultValue(0),
        ]
        public int ColumnWidth
        {
            get
            {
                return columnWidth;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("InvalidLowBoundArgumentEx");
                }
                if (columnWidth != value)
                {
                    columnWidth = value;
                    // if it's zero, we need to reset, and only way to do
                    // that is to recreate the handle.
                    if (columnWidth == 0)
                    {
                        RecreateHandle();
                    }
                    else if (IsHandleCreated)
                    {
                        //SendMessage(NativeMethods.LB_SETCOLUMNWIDTH, columnWidth, 0);
                    }
                }
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ClassName = "LISTBOX";

        //        cp.Style |= NativeMethods.WS_VSCROLL | NativeMethods.LBS_NOTIFY | NativeMethods.LBS_HASSTRINGS;
        //        if (scrollAlwaysVisible) cp.Style |= NativeMethods.LBS_DISABLENOSCROLL;
        //        if (!integralHeight) cp.Style |= NativeMethods.LBS_NOINTEGRALHEIGHT;
        //        if (useTabStops) cp.Style |= NativeMethods.LBS_USETABSTOPS;

        //        switch (borderStyle)
        //        {
        //            case BorderStyle.Fixed3D:
        //                cp.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;
        //                break;
        //            case BorderStyle.FixedSingle:
        //                cp.Style |= NativeMethods.WS_BORDER;
        //                break;
        //        }

        //        if (multiColumn)
        //        {
        //            cp.Style |= NativeMethods.LBS_MULTICOLUMN | NativeMethods.WS_HSCROLL;
        //        }
        //        else if (horizontalScrollbar)
        //        {
        //            cp.Style |= NativeMethods.WS_HSCROLL;
        //        }

        //        switch (selectionMode)
        //        {
        //            case SelectionMode.None:
        //                cp.Style |= NativeMethods.LBS_NOSEL;
        //                break;
        //            case SelectionMode.MultiSimple:
        //                cp.Style |= NativeMethods.LBS_MULTIPLESEL;
        //                break;
        //            case SelectionMode.MultiExtended:
        //                cp.Style |= NativeMethods.LBS_EXTENDEDSEL;
        //                break;
        //            case SelectionMode.One:
        //                break;
        //        }

        //        switch (drawMode)
        //        {
        //            case DrawMode.Normal:
        //                break;
        //            case DrawMode.OwnerDrawFixed:
        //                cp.Style |= NativeMethods.LBS_OWNERDRAWFIXED;
        //                break;
        //            case DrawMode.OwnerDrawVariable:
        //                cp.Style |= NativeMethods.LBS_OWNERDRAWVARIABLE;
        //                break;
        //        }

        //        return cp;
        //    }
        //}


        [DefaultValue(false), Browsable(false)]
        public bool UseCustomTabOffsets
        {
            get
            {
                return useCustomTabOffsets;
            }
            set
            {
                if (useCustomTabOffsets != value)
                {
                    useCustomTabOffsets = value;
                    RecreateHandle();
                }
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(120, 96);
            }
        }

        [DefaultValue(DrawMode.Normal), RefreshProperties(RefreshProperties.Repaint)]
        public virtual DrawMode DrawMode
        {
            get
            {
                return drawMode;
            }

            set
            {
                //valid values are 0x0 to 0x2
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)DrawMode.Normal, (int)DrawMode.OwnerDrawVariable))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(DrawMode));
                }
                if (drawMode != value)
                {
                    if (MultiColumn && value == DrawMode.OwnerDrawVariable)
                    {
                        throw new ArgumentException("ListBoxVarHeightMultiCol");
                    }
                    drawMode = value;
                    RecreateHandle();
                    if (drawMode == DrawMode.OwnerDrawVariable)
                    {
                        // VSWhidbey 139179 - force a layout after RecreateHandle() completes because now
                        // the LB is definitely fully populated and can report a preferred size accurately.
                        LayoutTransaction.DoLayoutIf(AutoSize, this.ParentInternal, this, PropertyNames.DrawMode);
                    }
                }
            }
        }

        // Used internally to find the currently focused item
        //
        internal int FocusedIndex
        {
            get
            {
                if (IsHandleCreated)
                {
                    //return unchecked((int)(long)SendMessage(NativeMethods.LB_GETCARETINDEX, 0, 0));
                }

                return -1;
            }
        }

        // VSWhidbey 95179: The scroll bars don't display properly when the IntegralHeight == false
        // and the control is resized before the font size is change and the new font size causes
        // the height of all the items to exceed the new height of the control. This is a bug in
        // the control, but can be easily worked around by removing and re-adding all the items.
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;

                if (false == integralHeight)
                {
                    // VSWhidbey 95179: Refresh the list to force the scroll bars to display
                    // when the integral height is false.
                    RefreshItems();
                }
            }
        }

        public override Color ForeColor
        {
            get
            {
                if (ShouldSerializeForeColor())
                {
                    return base.ForeColor;
                }
                else
                {
                    return SystemColors.WindowText;
                }
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [DefaultValue(0), Localizable(true)]
        public int HorizontalExtent
        {
            get
            {
                return horizontalExtent;
            }

            set
            {
                if (value != horizontalExtent)
                {
                    horizontalExtent = value;
                    UpdateHorizontalExtent();
                }
            }
        }

        [DefaultValue(false), Localizable(true)]
        public bool HorizontalScrollbar
        {
            get
            {
                return horizontalScrollbar;
            }

            set
            {
                if (value != horizontalScrollbar)
                {
                    horizontalScrollbar = value;

                    // There seems to be a bug in the native ListBox in that the addition
                    // of the horizontal scroll bar does not get reflected in the control
                    // rightaway. So, we refresh the items here.
                    RefreshItems();

                    // Only need to recreate the handle if not MultiColumn
                    // (HorizontalScrollbar has no effect on a MultiColumn listbox)
                    //
                    if (!MultiColumn)
                    {
                        RecreateHandle();
                    }
                }
            }
        }

        [DefaultValue(true), Localizable(true), RefreshProperties(RefreshProperties.Repaint)]
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
                    RecreateHandle();
                    // Avoid the listbox and textbox behaviour in Collection editors
                    //

                    integralHeightAdjust = true;
                    try
                    {
                        Height = requestedHeight;
                    }
                    finally
                    {
                        integralHeightAdjust = false;
                    }
                }
            }
        }

        [DefaultValue(DefaultItemHeight), Localizable(true), RefreshProperties(RefreshProperties.Repaint)]
        public virtual int ItemHeight
        {
            get
            {
                if (drawMode == DrawMode.OwnerDrawFixed ||
                    drawMode == DrawMode.OwnerDrawVariable)
                {
                    return itemHeight;
                }

                return GetItemHeight(0);
            }

            set
            {
                if (value < 1 || value > 255)
                {
                    throw new ArgumentOutOfRangeException("ItemHeight", "InvalidExBoundArgument");
                }
                if (itemHeight != value)
                {
                    itemHeight = value;
                    if (drawMode == DrawMode.OwnerDrawFixed && IsHandleCreated)
                    {
                        BeginUpdate();
                        //SendMessage(NativeMethods.LB_SETITEMHEIGHT, 0, value);

                        // Changing the item height might require a resize for IntegralHeight list boxes
                        //
                        if (IntegralHeight)
                        {
                            Size oldSize = Size;
                            Size = new Size(oldSize.Width + 1, oldSize.Height);
                            Size = oldSize;
                        }

                        EndUpdate();
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Localizable(true),  MergableProperty(false)]
        public ObjectCollection Items
        {
            get
            {
                if (itemsCollection == null)
                {
                    itemsCollection = CreateItemCollection();
                }
                return itemsCollection;
            }
        }

        // Computes the maximum width of all items in the ListBox
        //
        internal virtual int MaxItemWidth
        {
            get
            {

                if (horizontalExtent > 0)
                {
                    return horizontalExtent;
                }

                if (DrawMode != DrawMode.Normal)
                {
                    return -1;
                }

                // Return cached maxWidth if available
                //
                if (maxWidth > -1)
                {
                    return maxWidth;
                }

                // Compute maximum width
                //
                maxWidth = ComputeMaxItemWidth(maxWidth);

                return maxWidth;
            }
        }

        [DefaultValue(false)]
        public bool MultiColumn
        {
            get
            {
                return multiColumn;
            }
            set
            {
                if (multiColumn != value)
                {
                    if (value && drawMode == DrawMode.OwnerDrawVariable)
                    {
                        throw new ArgumentException("ListBoxVarHeightMultiCol");
                    }
                    multiColumn = value;
                    RecreateHandle();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PreferredHeight
        {
            get
            {
                int height = 0;

                if (drawMode == DrawMode.OwnerDrawVariable)
                {
                    // VSWhidbey 139179 - don't try to get item heights from the LB when items haven't been
                    // added to the LB yet. Just return current height.
                    if (RecreatingHandle || GetState(STATE_CREATINGHANDLE))
                    {
                        height = this.Height;
                    }
                    else
                    {
                        if (itemsCollection != null)
                        {
                            int cnt = itemsCollection.Count;
                            for (int i = 0; i < cnt; i++)
                            {
                                height += GetItemHeight(i);
                            }
                        }
                    }
                }
                else
                {
                    //VSWhidbey #148270
                    //When the list is empty, we don't want to multiply by 0 here.
                    int cnt = (itemsCollection == null || itemsCollection.Count == 0) ? 1 : itemsCollection.Count;
                    height = GetItemHeight(0) * cnt;
                }

                if (borderStyle != BorderStyle.None)
                {
                    height += SystemInformation.BorderSize.Height * 4 + 3;
                }

                return height;
            }
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            int height = PreferredHeight;
            int width;

            // Convert with a dummy height to add space required for borders
            // VSWhidbey #151141 -PreferredSize should return either the new
            // size of the control, or the default size if the handle has not been
            // created
            if (IsHandleCreated)
            {
                width = SizeFromClientSize(new Size(MaxItemWidth, height)).Width;
                width += SystemInformation.VerticalScrollBarWidth + 4;
            }
            else
            {
                return DefaultSize;
            }
            return new Size(width, height) + Padding.Size;
        }

        public override RightToLeft RightToLeft
        {
            get
            {
                if (!RunningOnWin2K)
                {
                    return RightToLeft.No;
                }
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
            }
        }

        static bool RunningOnWin2K
        {
            get
            {
                if (!checkedOS)
                {
                    if (Environment.OSVersion.Platform != System.PlatformID.Win32NT ||
                        Environment.OSVersion.Version.Major < 5)
                    {
                        runningOnWin2K = false;
                        checkedOS = true;
                    }
                }
                return runningOnWin2K;
            }
        }

        [DefaultValue(false), Localizable(true)]
        public bool ScrollAlwaysVisible
        {
            get
            {
                return scrollAlwaysVisible;
            }
            set
            {
                if (scrollAlwaysVisible != value)
                {
                    scrollAlwaysVisible = value;
                    RecreateHandle();
                }
            }
        }

        protected override bool AllowSelection
        {
            get
            {
                return selectionMode != SelectionMode.None;
            }
        }

        [Browsable(false), Bindable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int SelectedIndex
        {
            get
            {

                SelectionMode current = (selectionModeChanging) ? cachedSelectionMode : selectionMode;

                if (current == SelectionMode.None)
                {
                    return -1;
                }

                if (current == SelectionMode.One && IsHandleCreated)
                {
                    //return unchecked((int)(long)SendMessage(NativeMethods.LB_GETCURSEL, 0, 0));
                    return 0;
                }

                if (itemsCollection != null && SelectedItems.Count > 0)
                {
                    return Items.IndexOfIdentifier(SelectedItems.GetObjectAt(0));
                }

                return -1;
            }
            set
            {

                int itemCount = (itemsCollection == null) ? 0 : itemsCollection.Count;

                if (value < -1 || value >= itemCount)
                {
                    throw new ArgumentOutOfRangeException("SelectedIndex", "InvalidArgument");
                }

                if (selectionMode == SelectionMode.None)
                {
                    throw new ArgumentException("ListBoxInvalidSelectionMode");
                }

                if (selectionMode == SelectionMode.One && value != -1)
                {

                    // Single select an individual value.
                    int currentIndex = SelectedIndex;

                    if (currentIndex != value)
                    {
                        if (currentIndex != -1)
                        {
                            SelectedItems.SetSelected(currentIndex, false);
                        }
                        SelectedItems.SetSelected(value, true);

                        if (IsHandleCreated)
                        {
                            NativeSetSelected(value, true);
                        }

                        OnSelectedIndexChanged(EventArgs.Empty);
                    }
                }
                else if (value == -1)
                {
                    if (SelectedIndex != -1)
                    {
                        ClearSelected();
                        // ClearSelected raises OnSelectedIndexChanged for us
                    }
                }
                else
                {
                    if (!SelectedItems.GetSelected(value))
                    {

                        // Select this item while keeping any previously selected items selected.
                        //
                        SelectedItems.SetSelected(value, true);
                        if (IsHandleCreated)
                        {
                            NativeSetSelected(value, true);
                        }
                        OnSelectedIndexChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SelectedIndexCollection SelectedIndices
        {
            get
            {
                if (selectedIndices == null)
                {
                    selectedIndices = new SelectedIndexCollection(this);
                }
                return selectedIndices;
            }
        }

        [Browsable(false), Bindable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get
            {
                if (SelectedItems.Count > 0)
                {
                    return SelectedItems[0];
                }

                return null;
            }
            set
            {
                if (itemsCollection != null)
                {
                    if (value != null)
                    {
                        int index = itemsCollection.IndexOf(value);
                        if (index != -1)
                        {
                            SelectedIndex = index;
                        }
                    }
                    else
                    {
                        SelectedIndex = -1;
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SelectedObjectCollection SelectedItems
        {
            get
            {
                if (selectedItems == null)
                {
                    selectedItems = new SelectedObjectCollection(this);
                }
                return selectedItems;
            }
        }

        public virtual SelectionMode SelectionMode
        {
            get
            {
                return selectionMode;
            }
            set
            {
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)SelectionMode.None, (int)SelectionMode.MultiExtended))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(SelectionMode));
                }

                if (selectionMode != value)
                {
                    SelectedItems.EnsureUpToDate();
                    selectionMode = value;
                    try
                    {
                        selectionModeChanging = true;
                        RecreateHandle();
                    }
                    finally
                    {
                        selectionModeChanging = false;
                        cachedSelectionMode = selectionMode;
                        // update the selectedItems list and SelectedItems index collection
                        if (IsHandleCreated)
                        {
                            NativeUpdateSelection();
                        }
                    }
                }
            }
        }

        [DefaultValue(false)]
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
                    sorted = value;

                    if (sorted && itemsCollection != null && itemsCollection.Count >= 1)
                    {
                        Sort();
                    }
                }
            }
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Bindable(false)
        ]
        public override string Text
        {
            get
            {
                if (SelectionMode != SelectionMode.None && SelectedItem != null)
                {
                    if (FormattingEnabled)
                    {
                        return GetItemText(SelectedItem);
                    }
                    else
                    {
                        return FilterItemOnProperty(SelectedItem).ToString();
                    }
                }
                else
                {
                    return base.Text;
                }
            }
            set
            {
                base.Text = value;

                // Scan through the list items looking for the supplied text string.  If we find it,
                // select it.
                //
                if (SelectionMode != SelectionMode.None && value != null && (SelectedItem == null || !value.Equals(GetItemText(SelectedItem))))
                {

                    int cnt = Items.Count;
                    for (int index = 0; index < cnt; ++index)
                    {
                        if (String.Compare(value, GetItemText(Items[index]), true, CultureInfo.CurrentCulture) == 0)
                        {
                            SelectedIndex = index;
                            return;
                        }
                    }
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        new public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TopIndex
        {
            get
            {
                if (IsHandleCreated)
                {
                    //return unchecked((int)(long)SendMessage(NativeMethods.LB_GETTOPINDEX, 0, 0));
                    return 0;
                }
                else
                {
                    return topIndex;
                }
            }
            set
            {
                if (IsHandleCreated)
                {
                    //SendMessage(NativeMethods.LB_SETTOPINDEX, value, 0);
                }
                else
                {
                    topIndex = value;
                }
            }
        }

        [DefaultValue(true)]
        public bool UseTabStops
        {
            get
            {
                return useTabStops;
            }
            set
            {
                if (useTabStops != value)
                {
                    useTabStops = value;
                    RecreateHandle();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Browsable(false)]
        public IntegerCollection CustomTabOffsets
        {
            get
            {
                if (customTabOffsets == null)
                {
                    customTabOffsets = new IntegerCollection(this);
                }
                return customTabOffsets;
            }
        }

        [Obsolete("This method has been deprecated.  There is no replacement.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void AddItemsCore(object[] value)
        {
            int count = value == null ? 0 : value.Length;
            if (count == 0)
            {
                return;
            }

            Items.AddRangeInternal(value);
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler Click
        {
            add
            {
                base.Click += value;
            }
            remove
            {
                base.Click -= value;
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public new event MouseEventHandler MouseClick
        {
            add
            {
                base.MouseClick += value;
            }
            remove
            {
                base.MouseClick -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get { return base.Padding; }
            set { base.Padding = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler PaddingChanged
        {
            add { base.PaddingChanged += value; }
            remove { base.PaddingChanged -= value; }
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

        public event MeasureItemEventHandler MeasureItem
        {
            add
            {
                Events.AddHandler(EVENT_MEASUREITEM, value);
            }
            remove
            {
                Events.RemoveHandler(EVENT_MEASUREITEM, value);
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

        public void BeginUpdate()
        {
            //BeginUpdateInternal();
            updateCount++;
        }

        private void CheckIndex(int index)
        {
            if (index < 0 || index >= Items.Count)
                throw new ArgumentOutOfRangeException("index", "IndexOutOfRange");
        }

        private void CheckNoDataSource()
        {
            if (DataSource != null)
                throw new ArgumentException("DataSourceLocksItems");
        }

        protected virtual ObjectCollection CreateItemCollection()
        {
            return new ObjectCollection(this);
        }

        internal virtual int ComputeMaxItemWidth(int oldMax)
        {
            // pass LayoutUtils the collection of strings
            string[] strings = new string[this.Items.Count];

            for (int i = 0; i < Items.Count; i++)
            {
                strings[i] = GetItemText(Items[i]);
            }

            Size textSize = LayoutUtils.OldGetLargestStringSizeInCollection(Font, strings);
            return Math.Max(oldMax, textSize.Width);
        }

        public void ClearSelected()
        {

            bool hadSelection = false;

            int itemCount = (itemsCollection == null) ? 0 : itemsCollection.Count;
            for (int x = 0; x < itemCount; x++)
            {
                if (SelectedItems.GetSelected(x))
                {
                    hadSelection = true;
                    SelectedItems.SetSelected(x, false);
                    if (IsHandleCreated)
                    {
                        NativeSetSelected(x, false);
                    }
                }
            }

            if (hadSelection)
            {
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        public void EndUpdate()
        {
            //EndUpdateInternal();
            --updateCount;
        }

        public int FindString(string s)
        {
            return FindString(s, -1);
        }

        public int FindString(string s, int startIndex)
        {
            if (s == null) return -1;

            int itemCount = (itemsCollection == null) ? 0 : itemsCollection.Count;

            if (itemCount == 0)
            {
                return -1;
            }

            // VSWhidbey 95158: The last item in the list is still a valid starting point for a search.
            if (startIndex < -1 || startIndex >= itemCount)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            // Always use the managed FindStringInternal instead of LB_FINDSTRING.
            // The managed version correctly handles Turkish I.
            return FindStringInternal(s, Items, startIndex, false);
        }

        public int FindStringExact(string s)
        {
            return FindStringExact(s, -1);
        }

        public int FindStringExact(string s, int startIndex)
        {
            if (s == null) return -1;

            int itemCount = (itemsCollection == null) ? 0 : itemsCollection.Count;

            if (itemCount == 0)
            {
                return -1;
            }

            // VSWhidbey 95158: The last item in the list is still a valid starting point for a search.
            if (startIndex < -1 || startIndex >= itemCount)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            // Always use the managed FindStringInternal instead of LB_FINDSTRING.
            // The managed version correctly handles Turkish I.
            //
            return FindStringInternal(s, Items, startIndex, true);
        }

        public int GetItemHeight(int index)
        {
            int itemCount = (itemsCollection == null) ? 0 : itemsCollection.Count;

            // Note: index == 0 is OK even if the ListBox currently has
            // no items.
            //
            if (index < 0 || (index > 0 && index >= itemCount))
                throw new ArgumentOutOfRangeException("index", "InvalidArgument");

            if (drawMode != DrawMode.OwnerDrawVariable) index = 0;

            if (IsHandleCreated)
            {
                int h = 0; // unchecked((int)(long)SendMessage(NativeMethods.LB_GETITEMHEIGHT, index, 0));
                if (h == -1)
                    throw new Win32Exception();
                return h;
            }

            return itemHeight;
        }

        public Rectangle GetItemRectangle(int index)
        {
            CheckIndex(index);
            NativeMethods.RECT rect = new NativeMethods.RECT();
            //SendMessage(NativeMethods.LB_GETITEMRECT, index, ref rect);
            return Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
        }

        //[EditorBrowsable(EditorBrowsableState.Advanced)]
        //protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
        //{
        //    // update bounds' height to use the requested height, not the current height.  These
        //    // can be different if integral height is turned on.
        //    bounds.Height = requestedHeight;
        //    return base.GetScaledBounds(bounds, factor, specified);
        //}

        public bool GetSelected(int index)
        {
            CheckIndex(index);
            return GetSelectedInternal(index);
        }

        private bool GetSelectedInternal(int index)
        {
            if (IsHandleCreated)
            {
                //int sel = unchecked((int)(long)SendMessage(NativeMethods.LB_GETSEL, index, 0));
                int sel = -1;
                if (sel == -1)
                {
                    throw new Win32Exception();
                }
                return sel > 0;
            }
            else
            {
                if (itemsCollection != null && SelectedItems.GetSelected(index))
                {
                    return true;
                }
                return false;
            }
        }

        public int IndexFromPoint(Point p)
        {
            return IndexFromPoint(p.X, p.Y);
        }

        public int IndexFromPoint(int x, int y)
        {
            //NT4 SP6A : SendMessage Fails. So First check whether the point is in Client Co-ordinates and then
            //call Sendmessage.
            //
            NativeMethods.RECT r = new NativeMethods.RECT();
            //UnsafeNativeMethods.GetClientRect(new HandleRef(this, Handle), ref r);
            //if (r.left <= x && x < r.right && r.top <= y && y < r.bottom)
            //{
            //    int index = unchecked((int)(long)SendMessage(NativeMethods.LB_ITEMFROMPOINT, 0, unchecked((int)(long)NativeMethods.Util.MAKELPARAM(x, y))));
            //    if (NativeMethods.Util.HIWORD(index) == 0)
            //    {
            //        // Inside ListBox client area
            //        return NativeMethods.Util.LOWORD(index);
            //    }
            //}

            return NoMatches;
        }

        private int NativeAdd(object item)
        {
            Debug.Assert(IsHandleCreated, "Shouldn't be calling Native methods before the handle is created.");
            int insertIndex = 0; //unchecked((int)(long)SendMessage(NativeMethods.LB_ADDSTRING, 0, GetItemText(item)));

            if (insertIndex == NativeMethods.LB_ERRSPACE)
            {
                throw new OutOfMemoryException();
            }

            if (insertIndex == NativeMethods.LB_ERR)
            {
                // On some platforms (e.g. Win98), the ListBox control
                // appears to return LB_ERR if there are a large number (>32000)
                // of items. It doesn't appear to set error codes appropriately,
                // so we'll have to assume that LB_ERR corresponds to item
                // overflow.
                //
                throw new OutOfMemoryException("ListBoxItemOverflow");
            }

            return insertIndex;
        }

        private void NativeClear()
        {
            Debug.Assert(IsHandleCreated, "Shouldn't be calling Native methods before the handle is created.");
            //SendMessage(NativeMethods.LB_RESETCONTENT, 0, 0);
        }

        internal string NativeGetItemText(int index)
        {
            int len = 0; // unchecked((int)(long)SendMessage(NativeMethods.LB_GETTEXTLEN, index, 0));
            StringBuilder sb = new StringBuilder(len + 1);
            //UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.LB_GETTEXT, index, sb);
            return sb.ToString();
        }

        private int NativeInsert(int index, object item)
        {
            Debug.Assert(IsHandleCreated, "Shouldn't be calling Native methods before the handle is created.");
            int insertIndex = 0; // unchecked((int)(long)SendMessage(NativeMethods.LB_INSERTSTRING, index, GetItemText(item)));

            if (insertIndex == NativeMethods.LB_ERRSPACE)
            {
                throw new OutOfMemoryException();
            }

            if (insertIndex == NativeMethods.LB_ERR)
            {
                // On some platforms (e.g. Win98), the ListBox control
                // appears to return LB_ERR if there are a large number (>32000)
                // of items. It doesn't appear to set error codes appropriately,
                // so we'll have to assume that LB_ERR corresponds to item
                // overflow.
                //
                throw new OutOfMemoryException("ListBoxItemOverflow");
            }

            Debug.Assert(insertIndex == index, "NativeListBox inserted at " + insertIndex + " not the requested index of " + index);
            return insertIndex;
        }

        private void NativeRemoveAt(int index)
        {
            Debug.Assert(IsHandleCreated, "Shouldn't be calling Native methods before the handle is created.");

            //bool selected = (unchecked((int)(long)SendMessage(NativeMethods.LB_GETSEL, (IntPtr)index, IntPtr.Zero)) > 0);
            bool selected = false;
            //SendMessage(NativeMethods.LB_DELETESTRING, index, 0);

            //If the item currently selected is removed then we should fire a Selectionchanged event...
            //as the next time selected index returns -1...

            if (selected)
            {
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        private void NativeSetSelected(int index, bool value)
        {
            Debug.Assert(IsHandleCreated, "Should only call Native methods after the handle has been created");
            Debug.Assert(selectionMode != SelectionMode.None, "Guard against setting selection for None selection mode outside this code.");

            if (selectionMode == SelectionMode.One)
            {
                //SendMessage(NativeMethods.LB_SETCURSEL, (value ? index : -1), 0);
            }
            else
            {
                //SendMessage(NativeMethods.LB_SETSEL, value ? -1 : 0, index);
            }
        }

        private void NativeUpdateSelection()
        {
            Debug.Assert(IsHandleCreated, "Should only call native methods if handle is created");

            // Clear the selection state.
            //
            int cnt = Items.Count;
            for (int i = 0; i < cnt; i++)
            {
                SelectedItems.SetSelected(i, false);
            }

            int[] result = null;

            switch (selectionMode)
            {

                case SelectionMode.One:
                    //int index = unchecked((int)(long)SendMessage(NativeMethods.LB_GETCURSEL, 0, 0));
                    //if (index >= 0) result = new int[] { index };
                    break;

                case SelectionMode.MultiSimple:
                case SelectionMode.MultiExtended:
                    //int count = unchecked((int)(long)SendMessage(NativeMethods.LB_GETSELCOUNT, 0, 0));
                    //if (count > 0)
                    //{
                    //    result = new int[count];
                    //    UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.LB_GETSELITEMS, count, result);
                    //}
                    break;
            }

            // Now set the selected state on the appropriate items.
            //
            if (result != null)
            {
                foreach (int i in result)
                {
                    SelectedItems.SetSelected(i, true);
                }
            }
        }

        protected override void OnChangeUICues(UICuesEventArgs e)
        {

            // ListBox seems to get a bit confused when the UI cues change for the first
            // time - it draws the focus rect when it shouldn't and vice-versa. So when
            // the UI cues change, we just do an extra invalidate to get it into the
            // right state.
            //
            Invalidate();

            base.OnChangeUICues(e);
        }

        protected virtual void OnDrawItem(DrawItemEventArgs e)
        {
            DrawItemEventHandler handler = (DrawItemEventHandler)Events[EVENT_DRAWITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);


            //for getting the current Locale to set the Scrollbars...
            //
            //SendMessage(NativeMethods.LB_SETLOCALE, CultureInfo.CurrentCulture.LCID, 0);

            if (columnWidth != 0)
            {
                //SendMessage(NativeMethods.LB_SETCOLUMNWIDTH, columnWidth, 0);
            }
            if (drawMode == DrawMode.OwnerDrawFixed)
            {
                //SendMessage(NativeMethods.LB_SETITEMHEIGHT, 0, ItemHeight);
            }

            if (topIndex != 0)
            {
                //SendMessage(NativeMethods.LB_SETTOPINDEX, topIndex, 0);
            }

            if (UseCustomTabOffsets && CustomTabOffsets != null)
            {
                int wpar = CustomTabOffsets.Count;
                int[] offsets = new int[wpar];
                CustomTabOffsets.CopyTo(offsets, 0);
                //UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.LB_SETTABSTOPS, wpar, offsets);
            }

            if (itemsCollection != null)
            {

                int count = itemsCollection.Count;

                for (int i = 0; i < count; i++)
                {
                    NativeAdd(itemsCollection[i]);

                    if (selectionMode != SelectionMode.None)
                    {
                        if (selectedItems != null)
                        {
                            selectedItems.PushSelectionIntoNativeListBox(i);
                        }
                    }
                }
            }
            if (selectedItems != null)
            {
                if (selectedItems.Count > 0 && selectionMode == SelectionMode.One)
                {
                    SelectedItems.Dirty();
                    SelectedItems.EnsureUpToDate();
                }
            }
            UpdateHorizontalExtent();
        }

        internal override void OnHandleDestroyed(EventArgs e)
        {
            SelectedItems.EnsureUpToDate();
            if (Disposing)
            {
                itemsCollection = null;
            }
            base.OnHandleDestroyed(e);
        }

        protected virtual void OnMeasureItem(MeasureItemEventArgs e)
        {
            MeasureItemEventHandler handler = (MeasureItemEventHandler)Events[EVENT_MEASUREITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            // Changing the font causes us to resize, always rounding down.
            // Make sure we do this after base.OnPropertyChanged, which sends the WM_SETFONT message

            // Avoid the listbox and textbox behaviour in Collection editors
            //
            UpdateFontCache();
        }


        internal override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            //No need to RecreateHandle if we are removing the Listbox from controls collection...
            //so check the parent before recreating the handle...
            if (this.ParentInternal != null)
            {
                RecreateHandle();
            }
        }

        internal override void OnResize(EventArgs e)
        {

            base.OnResize(e);

            // There are some repainting issues for RightToLeft - so invalidate when we resize.
            //
            if (RightToLeft == RightToLeft.Yes || this.HorizontalScrollbar)
            {
                Invalidate();
            }

        }

        internal override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);

            // set the position in the dataSource, if there is any
            // we will only set the position in the currencyManager if it is different
            // from the SelectedIndex. Setting CurrencyManager::Position (even w/o changing it)
            // calls CurrencyManager::EndCurrentEdit, and that will pull the dataFrom the controls
            // into the backEnd. We do not need to do that.
            //
            if (this.DataManager != null && DataManager.Position != SelectedIndex)
            {
                //read this as "if everett or   (whidbey and selindex is valid)"
                if (!FormattingEnabled || this.SelectedIndex != -1)
                {
                    // VSWhidbey 95176: don't change dataManager position if we simply unselected everything.
                    // (Doing so would cause the first LB item to be selected...)
                    this.DataManager.Position = this.SelectedIndex;
                }
            }

            // VSWhidbey 163411: Call the handler after updating the DataManager's position so that
            // the DataManager's selected index will be correct in an event handler.
            EventHandler handler = (EventHandler)Events[EVENT_SELECTEDINDEXCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
            selectedValueChangedFired = true;
        }

        internal override void OnDataSourceChanged(EventArgs e)
        {
            if (DataSource == null)
            {
                BeginUpdate();
                SelectedIndex = -1;
                Items.ClearInternal();
                EndUpdate();
            }
            base.OnDataSourceChanged(e);
            RefreshItems();
        }

        internal override void OnDisplayMemberChanged(EventArgs e)
        {
            base.OnDisplayMemberChanged(e);

            // we want to use the new DisplayMember even if there is no data source
            RefreshItems();

            if (SelectionMode != SelectionMode.None && this.DataManager != null)
                this.SelectedIndex = this.DataManager.Position;
        }

        public override void Refresh()
        {
            if (drawMode == DrawMode.OwnerDrawVariable)
            {
                //Fire MeasureItem for Each Item in the Listbox...
                int cnt = Items.Count;
                Graphics graphics = null; //CreateGraphicsInternal();

                try
                {
                    for (int i = 0; i < cnt; i++)
                    {
                        MeasureItemEventArgs mie = new MeasureItemEventArgs(graphics, i, ItemHeight);
                        OnMeasureItem(mie);
                    }
                }
                finally
                {
                    graphics.Dispose();
                }

            }
            base.Refresh();
        }

        protected override void RefreshItems()
        {

            // Store the currently selected object collection.
            //
            ObjectCollection savedItems = itemsCollection;

            // Clear the items.
            //
            itemsCollection = null;
            selectedIndices = null;

            if (IsHandleCreated)
            {
                NativeClear();
            }

            object[] newItems = null;

            // if we have a dataSource and a DisplayMember, then use it
            // to populate the Items collection
            //
            if (this.DataManager != null && this.DataManager.Count != -1)
            {
                newItems = new object[this.DataManager.Count];
                for (int i = 0; i < newItems.Length; i++)
                {
                    newItems[i] = this.DataManager[i];
                }
            }
            else if (savedItems != null)
            {
                newItems = new object[savedItems.Count];
                savedItems.CopyTo(newItems, 0);
            }

            // Store the current list of items
            //
            if (newItems != null)
            {
                Items.AddRangeInternal(newItems);
            }

            // Restore the selected indices if SelectionMode allows it.
            //
            if (SelectionMode != SelectionMode.None)
            {
                if (this.DataManager != null)
                {
                    // put the selectedIndex in sync w/ the position in the dataManager
                    this.SelectedIndex = this.DataManager.Position;
                }
                else
                {
                    if (savedItems != null)
                    {
                        int cnt = savedItems.Count;
                        for (int index = 0; index < cnt; index++)
                        {
                            if (savedItems.InnerArray.GetState(index, SelectedObjectCollection.SelectedObjectMask))
                            {
                                SelectedItem = savedItems[index];
                            }
                        }
                    }
                }
            }

        }

        protected override void RefreshItem(int index)
        {
            Items.SetItemInternal(index, Items[index]);
        }

        public override void ResetBackColor()
        {
            base.ResetBackColor();
        }

        public override void ResetForeColor()
        {
            base.ResetForeColor();
        }


        private void ResetItemHeight()
        {
            itemHeight = DefaultItemHeight;
        }

        //[SuppressMessage("Microsoft.Portability", "CA1902:AvoidTestingForFloatingPointEquality")]
        //protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        //{

        //    if (factor.Width != 1F && factor.Height != 1F)
        //    {
        //        UpdateFontCache();
        //    }
        //    base.ScaleControl(factor, specified);
        //}


        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {

            // Avoid the listbox and textbox behaviour in Collection editors
            //


            if (!integralHeightAdjust && height != Height)
                requestedHeight = height;
            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void SetItemsCore(IList value)
        {
            BeginUpdate();
            Items.ClearInternal();
            Items.AddRangeInternal(value);

            this.SelectedItems.Dirty();

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
                    //SendMessage(NativeMethods.LB_SETCURSEL, DataManager.Position, 0);
                }

                // if the list changed and we still did not fire the
                // onselectedChanged event, then fire it now;
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

        public void SetSelected(int index, bool value)
        {
            int itemCount = (itemsCollection == null) ? 0 : itemsCollection.Count;
            if (index < 0 || index >= itemCount)
                throw new ArgumentOutOfRangeException("index", "InvalidArgument");

            if (selectionMode == SelectionMode.None)
                throw new InvalidOperationException("ListBoxInvalidSelectionMode");

            SelectedItems.SetSelected(index, value);
            if (IsHandleCreated)
            {
                NativeSetSelected(index, value);
            }
            SelectedItems.Dirty();
            OnSelectedIndexChanged(EventArgs.Empty);
        }

        protected virtual void Sort()
        {
            // This will force the collection to add each item back to itself
            // if sorted is now true, then the add method will insert the item
            // into the correct position
            //
            CheckNoDataSource();

            SelectedObjectCollection currentSelections = SelectedItems;
            currentSelections.EnsureUpToDate();

            if (sorted && itemsCollection != null)
            {
                itemsCollection.InnerArray.Sort();

                // Now that we've sorted, update our handle
                // if it has been created.
                if (IsHandleCreated)
                {
                    NativeClear();
                    int count = itemsCollection.Count;
                    for (int i = 0; i < count; i++)
                    {
                        NativeAdd(itemsCollection[i]);
                        if (currentSelections.GetSelected(i))
                        {
                            NativeSetSelected(i, true);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {

            string s = base.ToString();
            if (itemsCollection != null)
            {
                s += ", Items.Count: " + Items.Count.ToString(CultureInfo.CurrentCulture);
                if (Items.Count > 0)
                {
                    string z = GetItemText(Items[0]);
                    string txt = (z.Length > 40) ? z.Substring(0, 40) : z;
                    s += ", Items[0]: " + txt;
                }
            }
            return s;
        }
        private void UpdateFontCache()
        {
            fontIsChanged = true;
            integralHeightAdjust = true;
            try
            {
                Height = requestedHeight;
            }
            finally
            {
                integralHeightAdjust = false;
            }
            maxWidth = -1;
            UpdateHorizontalExtent();
            // clear the preferred size cache.
            CommonProperties.xClearPreferredSizeCache(this);

        }

        private void UpdateHorizontalExtent()
        {
            if (!multiColumn && horizontalScrollbar && IsHandleCreated)
            {
                int width = horizontalExtent;
                if (width == 0)
                {
                    width = MaxItemWidth;
                }
                //SendMessage(NativeMethods.LB_SETHORIZONTALEXTENT, width, 0);
            }
        }

        // Updates the cached max item width
        //
        private void UpdateMaxItemWidth(object item, bool removing)
        {

            // We shouldn't be caching maxWidth if we don't have horizontal scrollbars,
            // or horizontal extent has been set
            //
            if (!horizontalScrollbar || horizontalExtent > 0)
            {
                maxWidth = -1;
                return;
            }

            // Only update if we are currently caching maxWidth
            //
            if (maxWidth > -1)
            {

                // Compute item width
                //
                int width;
                width = 0;
                //using (Graphics graphics = CreateGraphicsInternal())
                //{
                //    width = (int)(Math.Ceiling(graphics.MeasureString(GetItemText(item), this.Font).Width));
                //}

                if (removing)
                {
                    // We're removing this item, so if it's the longest
                    // in the list, reset the cache
                    //
                    if (width >= maxWidth)
                    {
                        maxWidth = -1;
                    }
                }
                else
                {
                    // We're adding or inserting this item - update the cache
                    //
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
            }
        }

        // Updates the Custom TabOffsets
        //

        private void UpdateCustomTabOffsets()
        {
            if (IsHandleCreated && UseCustomTabOffsets && CustomTabOffsets != null)
            {
                int wpar = CustomTabOffsets.Count;
                int[] offsets = new int[wpar];
                CustomTabOffsets.CopyTo(offsets, 0);
                //UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.LB_SETTABSTOPS, wpar, offsets);
                Invalidate();
            }
        }

        //private void WmPrint(ref Message m)
        //{
        //    base.WndProc(ref m);
        //    if ((NativeMethods.PRF_NONCLIENT & (int)m.LParam) != 0 && Application.RenderWithVisualStyles && this.BorderStyle == BorderStyle.Fixed3D)
        //    {
        //        IntSecurity.UnmanagedCode.Assert();
        //        try
        //        {
        //            using (Graphics g = Graphics.FromHdc(m.WParam))
        //            {
        //                Rectangle rect = new Rectangle(0, 0, this.Size.Width - 1, this.Size.Height - 1);
        //                using (Pen pen = new Pen(VisualStyleInformation.TextControlBorder))
        //                {
        //                    g.DrawRectangle(pen, rect);
        //                }
        //                rect.Inflate(-1, -1);
        //                g.DrawRectangle(SystemPens.Window, rect);
        //            }
        //        }
        //        finally
        //        {
        //            CodeAccessPermission.RevertAssert();
        //        }
        //    }
        //}

        //[
        //System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode),
        //System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        //]
        //protected virtual void WmReflectCommand(ref Message m)
        //{
        //    switch (NativeMethods.Util.HIWORD(m.WParam))
        //    {
        //        case NativeMethods.LBN_SELCHANGE:
        //            if (selectedItems != null)
        //            {
        //                selectedItems.Dirty();
        //            }
        //            OnSelectedIndexChanged(EventArgs.Empty);
        //            break;
        //        case NativeMethods.LBN_DBLCLK:
        //            // Handle this inside WM_LBUTTONDBLCLK
        //            // OnDoubleClick(EventArgs.Empty);
        //            break;
        //    }
        //}

        //private void WmReflectDrawItem(ref Message m)
        //{
        //    NativeMethods.DRAWITEMSTRUCT dis = (NativeMethods.DRAWITEMSTRUCT)m.GetLParam(typeof(NativeMethods.DRAWITEMSTRUCT));
        //    IntPtr dc = dis.hDC;
        //    IntPtr oldPal = SetUpPalette(dc, false /*force*/, false /*realize*/);
        //    try
        //    {
        //        Graphics g = Graphics.FromHdcInternal(dc);

        //        try
        //        {
        //            Rectangle bounds = Rectangle.FromLTRB(dis.rcItem.left, dis.rcItem.top, dis.rcItem.right, dis.rcItem.bottom);

        //            if (HorizontalScrollbar)
        //            {
        //                if (MultiColumn)
        //                {
        //                    bounds.Width = Math.Max(ColumnWidth, bounds.Width);
        //                }
        //                else
        //                {
        //                    bounds.Width = Math.Max(MaxItemWidth, bounds.Width);
        //                }
        //            }


        //            OnDrawItem(new DrawItemEventArgs(g, Font, bounds, dis.itemID, (DrawItemState)dis.itemState, ForeColor, BackColor));
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
        //            SafeNativeMethods.SelectPalette(new HandleRef(null, dc), new HandleRef(null, oldPal), 0);
        //        }
        //    }
        //    m.Result = (IntPtr)1;
        //}

        //private void WmReflectMeasureItem(ref Message m)
        //{

        //    NativeMethods.MEASUREITEMSTRUCT mis = (NativeMethods.MEASUREITEMSTRUCT)m.GetLParam(typeof(NativeMethods.MEASUREITEMSTRUCT));

        //    if (drawMode == DrawMode.OwnerDrawVariable && mis.itemID >= 0)
        //    {
        //        Graphics graphics = CreateGraphicsInternal();
        //        MeasureItemEventArgs mie = new MeasureItemEventArgs(graphics, mis.itemID, ItemHeight);
        //        try
        //        {
        //            OnMeasureItem(mie);
        //            mis.itemHeight = mie.ItemHeight;
        //        }
        //        finally
        //        {
        //            graphics.Dispose();
        //        }
        //    }
        //    else
        //    {
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
        //        case NativeMethods.WM_REFLECT + NativeMethods.WM_COMMAND:
        //            WmReflectCommand(ref m);
        //            break;
        //        case NativeMethods.WM_REFLECT + NativeMethods.WM_DRAWITEM:
        //            WmReflectDrawItem(ref m);
        //            break;
        //        case NativeMethods.WM_REFLECT + NativeMethods.WM_MEASUREITEM:
        //            WmReflectMeasureItem(ref m);
        //            break;
        //        case NativeMethods.WM_PRINT:
        //            WmPrint(ref m);
        //            break;
        //        case NativeMethods.WM_LBUTTONDOWN:
        //            if (selectedItems != null)
        //            {
        //                selectedItems.Dirty();
        //            }
        //            base.WndProc(ref m);
        //            break;
        //        case NativeMethods.WM_LBUTTONUP:
        //            // Get the mouse location
        //            //
        //            int x = NativeMethods.Util.SignedLOWORD(m.LParam);
        //            int y = NativeMethods.Util.SignedHIWORD(m.LParam);
        //            Point pt = new Point(x, y);
        //            pt = PointToScreen(pt);
        //            bool captured = Capture;
        //            if (captured && UnsafeNativeMethods.WindowFromPoint(pt.X, pt.Y) == Handle)
        //            {


        //                if (!doubleClickFired && !ValidationCancelled)
        //                {
        //                    OnClick(new MouseEventArgs(MouseButtons.Left, 1, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));
        //                    OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));

        //                }
        //                else
        //                {
        //                    doubleClickFired = false;
        //                    // WM_COMMAND is only fired if the user double clicks an item,
        //                    // so we can't use that as a double-click substitute
        //                    if (!ValidationCancelled)
        //                    {
        //                        OnDoubleClick(new MouseEventArgs(MouseButtons.Left, 2, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));
        //                        OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Left, 2, NativeMethods.Util.SignedLOWORD(m.LParam), NativeMethods.Util.SignedHIWORD(m.LParam), 0));

        //                    }
        //                }
        //            }

        //            //
        //            // If this control has been disposed in the user's event handler, then we need to ignore the WM_LBUTTONUP
        //            // message to avoid exceptions thrown as a result of handle re-creation (VSWhidbey#95150).
        //            // We handle this situation here and not at the top of the window procedure since this is the only place
        //            // where we can get disposed as an effect of external code (form.Close() for instance) and then pass the
        //            // message to the base class.
        //            //
        //            if (GetState(STATE_DISPOSED))
        //            {
        //                base.DefWndProc(ref m);
        //            }
        //            else
        //            {
        //                base.WndProc(ref m);
        //            }

        //            doubleClickFired = false;
        //            break;

        //        case NativeMethods.WM_RBUTTONUP:
        //            // Get the mouse location
        //            //
        //            int rx = NativeMethods.Util.SignedLOWORD(m.LParam);
        //            int ry = NativeMethods.Util.SignedHIWORD(m.LParam);
        //            Point rpt = new Point(rx, ry);
        //            rpt = PointToScreen(rpt);
        //            bool rCaptured = Capture;
        //            if (rCaptured && UnsafeNativeMethods.WindowFromPoint(rpt.X, rpt.Y) == Handle)
        //            {
        //                if (selectedItems != null)
        //                {
        //                    selectedItems.Dirty();
        //                }
        //            }
        //            base.WndProc(ref m);
        //            break;

        //        case NativeMethods.WM_LBUTTONDBLCLK:
        //            //the Listbox gets  WM_LBUTTONDOWN - WM_LBUTTONUP -WM_LBUTTONDBLCLK - WM_LBUTTONUP...
        //            //sequence for doubleclick...
        //            //the first WM_LBUTTONUP, resets the flag for Doubleclick
        //            //So its necessary for us to set it again...
        //            doubleClickFired = true;
        //            base.WndProc(ref m);
        //            break;

        //        case NativeMethods.WM_WINDOWPOSCHANGED:
        //            base.WndProc(ref m);
        //            if (integralHeight && fontIsChanged)
        //            {
        //                Height = Math.Max(Height, ItemHeight);
        //                fontIsChanged = false;
        //            }
        //            break;

        //        default:
        //            base.WndProc(ref m);
        //            break;
        //    }
        //}

        //protected override AccessibleObject CreateAccessibilityInstance()
        //{
        //    if (AccessibilityImprovements.Level3)
        //    {
        //        return new ListBoxAccessibleObject(this);
        //    }
        //    else
        //    {
        //        return base.CreateAccessibilityInstance();
        //    }
        //}

        ///
        internal class ItemArray : IComparer
        {

            private static int lastMask = 1;

            private ListControl listControl;
            private Entry[] entries;
            private int count;
            private int version;

            public ItemArray(ListControl listControl)
            {
                this.listControl = listControl;
            }

            public int Version
            {
                get
                {
                    return version;
                }
            }

            public object Add(object item)
            {
                EnsureSpace(1);
                version++;
                entries[count] = new Entry(item);
                return entries[count++];
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void AddRange(ICollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                EnsureSpace(items.Count);
                foreach (object i in items)
                {
                    entries[count++] = new Entry(i);
                }
                version++;
            }

            public void Clear()
            {
                if (count > 0)
                {
                    Array.Clear(entries, 0, count);
                }

                count = 0;
                version++;
            }

            public static int CreateMask()
            {
                int mask = lastMask;
                lastMask = lastMask << 1;
                Debug.Assert(lastMask > mask, "We have overflowed our state mask.");
                return mask;
            }

            private void EnsureSpace(int elements)
            {
                if (entries == null)
                {
                    entries = new Entry[Math.Max(elements, 4)];
                }
                else if (count + elements >= entries.Length)
                {
                    int newLength = Math.Max(entries.Length * 2, entries.Length + elements);
                    Entry[] newEntries = new Entry[newLength];
                    entries.CopyTo(newEntries, 0);
                    entries = newEntries;
                }
            }

            public int GetActualIndex(int virtualIndex, int stateMask)
            {
                if (stateMask == 0)
                {
                    return virtualIndex;
                }

                // More complex; we must compute this index.
                int calcIndex = -1;
                for (int i = 0; i < count; i++)
                {
                    if ((entries[i].state & stateMask) != 0)
                    {
                        calcIndex++;
                        if (calcIndex == virtualIndex)
                        {
                            return i;
                        }
                    }
                }

                return -1;
            }

            public int GetCount(int stateMask)
            {
                // If mask is zero, then just give the main count
                if (stateMask == 0)
                {
                    return count;
                }

                // more complex:  must provide a count of items
                // based on a mask.

                int filteredCount = 0;

                for (int i = 0; i < count; i++)
                {
                    if ((entries[i].state & stateMask) != 0)
                    {
                        filteredCount++;
                    }
                }

                return filteredCount;
            }

            public IEnumerator GetEnumerator(int stateMask)
            {
                return GetEnumerator(stateMask, false);
            }

            public IEnumerator GetEnumerator(int stateMask, bool anyBit)
            {
                return new EntryEnumerator(this, stateMask, anyBit);
            }

            public object GetItem(int virtualIndex, int stateMask)
            {
                int actualIndex = GetActualIndex(virtualIndex, stateMask);

                if (actualIndex == -1)
                {
                    throw new IndexOutOfRangeException();
                }

                return entries[actualIndex].item;
            }
            internal object GetEntryObject(int virtualIndex, int stateMask)
            {
                int actualIndex = GetActualIndex(virtualIndex, stateMask);

                if (actualIndex == -1)
                {
                    throw new IndexOutOfRangeException();
                }

                return entries[actualIndex];
            }
            public bool GetState(int index, int stateMask)
            {
                return ((entries[index].state & stateMask) == stateMask);
            }

            public int IndexOf(object item, int stateMask)
            {

                int virtualIndex = -1;

                for (int i = 0; i < count; i++)
                {
                    if (stateMask == 0 || (entries[i].state & stateMask) != 0)
                    {
                        virtualIndex++;
                        if (entries[i].item.Equals(item))
                        {
                            return virtualIndex;
                        }
                    }
                }

                return -1;
            }

            public int IndexOfIdentifier(object identifier, int stateMask)
            {
                int virtualIndex = -1;

                for (int i = 0; i < count; i++)
                {
                    if (stateMask == 0 || (entries[i].state & stateMask) != 0)
                    {
                        virtualIndex++;
                        if (entries[i] == identifier)
                        {
                            return virtualIndex;
                        }
                    }
                }

                return -1;
            }

            public void Insert(int index, object item)
            {
                EnsureSpace(1);

                if (index < count)
                {
                    System.Array.Copy(entries, index, entries, index + 1, count - index);
                }

                entries[index] = new Entry(item);
                count++;
                version++;
            }

            public void Remove(object item)
            {
                int index = IndexOf(item, 0);

                if (index != -1)
                {
                    RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                count--;
                for (int i = index; i < count; i++)
                {
                    entries[i] = entries[i + 1];
                }
                entries[count] = null;
                version++;
            }

            public void SetItem(int index, object item)
            {
                entries[index].item = item;
            }

            public void SetState(int index, int stateMask, bool value)
            {
                if (value)
                {
                    entries[index].state |= stateMask;
                }
                else
                {
                    entries[index].state &= ~stateMask;
                }
                version++;
            }

            public int BinarySearch(object element)
            {
                return Array.BinarySearch(entries, 0, count, element, this);
            }


            public void Sort()
            {
                Array.Sort(entries, 0, count, this);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void Sort(Array externalArray)
            {
                Array.Sort(externalArray, this);
            }

            int IComparer.Compare(object item1, object item2)
            {
                if (item1 == null)
                {
                    if (item2 == null)
                        return 0; //both null, then they are equal

                    return -1; //item1 is null, but item2 is valid (greater)
                }
                if (item2 == null)
                    return 1; //item2 is null, so item 1 is greater

                if (item1 is Entry)
                {
                    item1 = ((Entry)item1).item;
                }

                if (item2 is Entry)
                {
                    item2 = ((Entry)item2).item;
                }

                String itemName1 = listControl.GetItemText(item1);
                String itemName2 = listControl.GetItemText(item2);

                CompareInfo compInfo = (Application.CurrentCulture).CompareInfo;
                return compInfo.Compare(itemName1, itemName2, CompareOptions.StringSort);
            }

            private class Entry
            {
                public object item;
                public int state;

                public Entry(object item)
                {
                    this.item = item;
                    this.state = 0;
                }
            }

            private class EntryEnumerator : IEnumerator
            {
                private ItemArray items;
                private bool anyBit;
                private int state;
                private int current;
                private int version;

                public EntryEnumerator(ItemArray items, int state, bool anyBit)
                {
                    this.items = items;
                    this.state = state;
                    this.anyBit = anyBit;
                    this.version = items.version;
                    this.current = -1;
                }

                bool IEnumerator.MoveNext()
                {
                    if (version != items.version) throw new InvalidOperationException("ListEnumVersionMismatch");

                    while (true)
                    {
                        if (current < items.count - 1)
                        {
                            current++;
                            if (anyBit)
                            {
                                if ((items.entries[current].state & state) != 0)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if ((items.entries[current].state & state) == state)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            current = items.count;
                            return false;
                        }
                    }
                }

                void IEnumerator.Reset()
                {
                    if (version != items.version) throw new InvalidOperationException("ListEnumVersionMismatch");
                    current = -1;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (current == -1 || current == items.count)
                        {
                            throw new InvalidOperationException("ListEnumCurrentOutOfRange");
                        }

                        return items.entries[current].item;
                    }
                }
            }
        }

        // Items
        [ListBindable(false)]
        public class ObjectCollection : IList
        {

            private ListBox owner;
            private ItemArray items;

            public ObjectCollection(ListBox owner)
            {
                this.owner = owner;
            }

            public ObjectCollection(ListBox owner, ObjectCollection value)
            {
                this.owner = owner;
                this.AddRange(value);
            }

            public ObjectCollection(ListBox owner, object[] value)
            {
                this.owner = owner;
                this.AddRange(value);
            }

            public int Count
            {
                get
                {
                    return InnerArray.GetCount(0);
                }
            }

            internal ItemArray InnerArray
            {
                get
                {
                    if (items == null)
                    {
                        items = new ItemArray(owner);
                    }
                    return items;
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
                owner.UpdateHorizontalExtent();
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
                    InnerArray.Add(item);
                }
                else
                {
                    if (Count > 0)
                    {
                        index = InnerArray.BinarySearch(item);
                        if (index < 0)
                        {
                            index = ~index; // getting the index of the first element that is larger than the search value
                                            //this index will be used for insert
                        }
                    }
                    else
                        index = 0;

                    Debug.Assert(index >= 0 && index <= Count, "Wrong index for insert");
                    InnerArray.Insert(index, item);
                }
                bool successful = false;

                try
                {
                    if (owner.sorted)
                    {
                        if (owner.IsHandleCreated)
                        {
                            owner.NativeInsert(index, item);
                            owner.UpdateMaxItemWidth(item, false);
                            if (owner.selectedItems != null)
                            {
                                // VSWhidbey 95187: sorting may throw the LB contents and the selectedItem array out of synch.
                                owner.selectedItems.Dirty();
                            }
                        }
                    }
                    else
                    {
                        index = Count - 1;
                        if (owner.IsHandleCreated)
                        {
                            owner.NativeAdd(item);
                            owner.UpdateMaxItemWidth(item, false);
                        }
                    }
                    successful = true;
                }
                finally
                {
                    if (!successful)
                    {
                        InnerArray.Remove(item);
                    }
                }

                return index;
            }


            int IList.Add(object item)
            {
                return Add(item);
            }

            public void AddRange(ObjectCollection value)
            {
                owner.CheckNoDataSource();
                AddRangeInternal((ICollection)value);
            }

            public void AddRange(object[] items)
            {
                owner.CheckNoDataSource();
                AddRangeInternal((ICollection)items);
            }

            internal void AddRangeInternal(ICollection items)
            {

                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                owner.BeginUpdate();
                try
                {
                    foreach (object item in items)
                    {
                        // adding items one-by-one for performance 
                        // not using sort because after the array is sorted index of each newly added item will need to be found
                        // AddInternal is based on BinarySearch and finds index without any additional cost
                        AddInternal(item);
                    }
                }
                finally
                {
                    owner.UpdateHorizontalExtent();
                    owner.EndUpdate();
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public virtual object this[int index]
            {
                get
                {
                    if (index < 0 || index >= InnerArray.GetCount(0))
                    {
                        throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                    }

                    return InnerArray.GetItem(index, 0);
                }
                set
                {
                    owner.CheckNoDataSource();
                    SetItemInternal(index, value);
                }
            }

            public virtual void Clear()
            {
                owner.CheckNoDataSource();
                ClearInternal();
            }

            internal void ClearInternal()
            {

                //update the width.. to reset Scrollbars..
                // Clear the selection state.
                //
                int cnt = owner.Items.Count;
                for (int i = 0; i < cnt; i++)
                {
                    owner.UpdateMaxItemWidth(InnerArray.GetItem(i, 0), true);
                }


                if (owner.IsHandleCreated)
                {
                    owner.NativeClear();
                }
                InnerArray.Clear();
                owner.maxWidth = -1;
                owner.UpdateHorizontalExtent();
            }

            public bool Contains(object value)
            {
                return IndexOf(value) != -1;
            }

            public void CopyTo(object[] destination, int arrayIndex)
            {
                int count = InnerArray.GetCount(0);
                for (int i = 0; i < count; i++)
                {
                    destination[i + arrayIndex] = InnerArray.GetItem(i, 0);
                }
            }

            void ICollection.CopyTo(Array destination, int index)
            {
                int count = InnerArray.GetCount(0);
                for (int i = 0; i < count; i++)
                {
                    destination.SetValue(InnerArray.GetItem(i, 0), i + index);
                }
            }

            public IEnumerator GetEnumerator()
            {
                return InnerArray.GetEnumerator(0);
            }

            public int IndexOf(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                return InnerArray.IndexOf(value, 0);
            }

            internal int IndexOfIdentifier(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                return InnerArray.IndexOfIdentifier(value, 0);
            }

            public void Insert(int index, object item)
            {
                owner.CheckNoDataSource();

                if (index < 0 || index > InnerArray.GetCount(0))
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                if (item == null)
                {
                    throw new ArgumentNullException("item");
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
                    InnerArray.Insert(index, item);
                    if (owner.IsHandleCreated)
                    {

                        bool successful = false;

                        try
                        {
                            owner.NativeInsert(index, item);
                            owner.UpdateMaxItemWidth(item, false);
                            successful = true;
                        }
                        finally
                        {
                            if (!successful)
                            {
                                InnerArray.RemoveAt(index);
                            }
                        }
                    }
                }
                owner.UpdateHorizontalExtent();
            }

            public void Remove(object value)
            {

                int index = InnerArray.IndexOf(value, 0);

                if (index != -1)
                {
                    RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                owner.CheckNoDataSource();

                if (index < 0 || index >= InnerArray.GetCount(0))
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                owner.UpdateMaxItemWidth(InnerArray.GetItem(index, 0), true);

                // VSWhidbey 95181: Update InnerArray before calling NativeRemoveAt to ensure that when
                // SelectedIndexChanged is raised (by NativeRemoveAt), InnerArray's state matches wrapped LB state.
                InnerArray.RemoveAt(index);

                if (owner.IsHandleCreated)
                {
                    owner.NativeRemoveAt(index);
                }

                owner.UpdateHorizontalExtent();
            }

            internal void SetItemInternal(int index, object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (index < 0 || index >= InnerArray.GetCount(0))
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                owner.UpdateMaxItemWidth(InnerArray.GetItem(index, 0), true);
                InnerArray.SetItem(index, value);

                // If the native control has been created, and the display text of the new list item object
                // is different to the current text in the native list item, recreate the native list item...
                if (owner.IsHandleCreated)
                {
                    bool selected = (owner.SelectedIndex == index);
                    if (String.Compare(this.owner.GetItemText(value), this.owner.NativeGetItemText(index), true, CultureInfo.CurrentCulture) != 0)
                    {
                        owner.NativeRemoveAt(index);
                        owner.SelectedItems.SetSelected(index, false);
                        owner.NativeInsert(index, value);
                        owner.UpdateMaxItemWidth(value, false);
                        if (selected)
                        {
                            owner.SelectedIndex = index;
                        }
                    }
                    else
                    {
                        // NEW - FOR COMPATIBILITY REASONS
                        // Minimum compatibility fix for VSWhidbey 377287
                        if (selected)
                        {
                            owner.OnSelectedIndexChanged(EventArgs.Empty); //will fire selectedvaluechanged
                        }
                    }
                }
                owner.UpdateHorizontalExtent();
            }
        } // end ObjectCollection

        //******************************************************************************************
        // IntegerCollection
        public class IntegerCollection : IList
        {
            private ListBox owner;
            private int[] innerArray;
            private int count = 0;

            public IntegerCollection(ListBox owner)
            {
                this.owner = owner;
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return count;
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
                    return true;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            bool IList.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool Contains(int item)
            {
                return IndexOf(item) != -1;
            }

            bool IList.Contains(object item)
            {
                if (item is Int32)
                {
                    return Contains((int)item);
                }
                else
                {
                    return false;
                }
            }

            public void Clear()
            {
                count = 0;
                innerArray = null;
            }

            public int IndexOf(int item)
            {
                int index = -1;

                if (innerArray != null)
                {
                    index = Array.IndexOf(innerArray, item);

                    // We initialize innerArray with more elements than needed in the method EnsureSpace, 
                    // and we don't actually remove element from innerArray in the method RemoveAt,
                    // so there maybe some elements which are not actually in innerArray will be found
                    // and we need to filter them out
                    if (index >= count)
                    {
                        index = -1;
                    }
                }

                return index;
            }

            int IList.IndexOf(object item)
            {
                if (item is Int32)
                {
                    return IndexOf((int)item);
                }
                else
                {
                    return -1;
                }
            }


            private int AddInternal(int item)
            {

                EnsureSpace(1);

                int index = IndexOf(item);
                if (index == -1)
                {
                    innerArray[count++] = item;
                    Array.Sort(innerArray, 0, count);
                    index = IndexOf(item);
                }
                return index;
            }

            public int Add(int item)
            {
                int index = AddInternal(item);
                owner.UpdateCustomTabOffsets();

                return index;
            }

            [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
            [
                SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters") // "item" is the name of the param passed in.
                                                                                                            // So we don't have to localize it.
            ]
            int IList.Add(object item)
            {
                if (!(item is int))
                {
                    throw new ArgumentException("item");
                }
                return Add((int)item);
            }

            public void AddRange(int[] items)
            {
                AddRangeInternal((ICollection)items);
            }

            public void AddRange(IntegerCollection value)
            {
                AddRangeInternal((ICollection)value);
            }

            [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
            [
                SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters") // "item" is the name of the param passed in.
                                                                                                            // So we don't have to localize it.
            ]
            private void AddRangeInternal(ICollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                owner.BeginUpdate();
                try
                {
                    EnsureSpace(items.Count);
                    foreach (object item in items)
                    {
                        if (!(item is int))
                        {
                            throw new ArgumentException("item");
                        }
                        else
                        {
                            AddInternal((int)item);
                        }
                    }
                    owner.UpdateCustomTabOffsets();
                }
                finally
                {
                    owner.EndUpdate();
                }
            }


            private void EnsureSpace(int elements)
            {
                if (innerArray == null)
                {
                    innerArray = new int[Math.Max(elements, 4)];
                }
                else if (count + elements >= innerArray.Length)
                {
                    int newLength = Math.Max(innerArray.Length * 2, innerArray.Length + elements);
                    int[] newEntries = new int[newLength];
                    innerArray.CopyTo(newEntries, 0);
                    innerArray = newEntries;
                }
            }

            void IList.Clear()
            {
                Clear();
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException("ListBoxCantInsertIntoIntegerCollection");
            }

            [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
            [
                SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters") // "value" is the name of the param passed in.
                                                                                                            // So we don't have to localize it.
            ]
            void IList.Remove(object value)
            {
                if (!(value is int))
                {
                    throw new ArgumentException("value");
                }
                Remove((int)value);
            }

            void IList.RemoveAt(int index)
            {
                RemoveAt(index);
            }

            public void Remove(int item)
            {

                int index = IndexOf(item);

                if (index != -1)
                {
                    RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                if (index < 0 || index >= count)
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                count--;
                for (int i = index; i < count; i++)
                {
                    innerArray[i] = innerArray[i + 1];
                }
            }

            [
                SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters") // "index" is the name of the param passed in.
                                                                                                            // So we don't have to localize it.
            ]
            public int this[int index]
            {
                get
                {
                    return innerArray[index];
                }
                [
                    SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")     // This exception already shipped.
                                                                                                            // We can't change its text.
                ]
                set
                {

                    if (index < 0 || index >= count)
                    {
                        throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                    }
                    innerArray[index] = (int)value;
                    owner.UpdateCustomTabOffsets();


                }
            }

            [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                [
                    SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters"),    // "value" is the name of the param.
                                                                                                                    // So we don't have to localize it.
                    SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")             // This exception already shipped.
                                                                                                                    // We can't change its text.
                ]
                set
                {
                    if (!(value is int))
                    {
                        throw new ArgumentException("value");
                    }
                    else
                    {
                        this[index] = (int)value;
                    }

                }
            }

            public void CopyTo(Array destination, int index)
            {
                int cnt = Count;
                for (int i = 0; i < cnt; i++)
                {
                    destination.SetValue(this[i], i + index);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new CustomTabOffsetsEnumerator(this);
            }

            private class CustomTabOffsetsEnumerator : IEnumerator
            {
                private IntegerCollection items;
                private int current;

                public CustomTabOffsetsEnumerator(IntegerCollection items)
                {
                    this.items = items;
                    this.current = -1;
                }

                bool IEnumerator.MoveNext()
                {

                    if (current < items.Count - 1)
                    {
                        current++;
                        return true;
                    }
                    else
                    {
                        current = items.Count;
                        return false;
                    }
                }

                void IEnumerator.Reset()
                {
                    current = -1;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (current == -1 || current == items.Count)
                        {
                            throw new InvalidOperationException("ListEnumCurrentOutOfRange");
                        }

                        return items[current];
                    }
                }
            }
        }

        //******************************************************************************************

        // SelectedIndices
        public class SelectedIndexCollection : IList
        {
            private ListBox owner;

            /* C#r: protected */
            public SelectedIndexCollection(ListBox owner)
            {
                this.owner = owner;
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return owner.SelectedItems.Count;
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
                    return true;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public bool Contains(int selectedIndex)
            {
                return IndexOf(selectedIndex) != -1;
            }

            bool IList.Contains(object selectedIndex)
            {
                if (selectedIndex is Int32)
                {
                    return Contains((int)selectedIndex);
                }
                else
                {
                    return false;
                }
            }

            public int IndexOf(int selectedIndex)
            {

                // Just what does this do?  The selectedIndex parameter above is the index into the
                // main object collection.  We look at the state of that item, and if the state indicates
                // that it is selected, we get back the virtualized index into this collection.  Indexes on
                // this collection match those on the SelectedObjectCollection.
                if (selectedIndex >= 0 &&
                    selectedIndex < InnerArray.GetCount(0) &&
                    InnerArray.GetState(selectedIndex, SelectedObjectCollection.SelectedObjectMask))
                {

                    return InnerArray.IndexOf(InnerArray.GetItem(selectedIndex, 0), SelectedObjectCollection.SelectedObjectMask);
                }

                return -1;
            }

            int IList.IndexOf(object selectedIndex)
            {
                if (selectedIndex is Int32)
                {
                    return IndexOf((int)selectedIndex);
                }
                else
                {
                    return -1;
                }
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException("ListBoxSelectedIndexCollectionIsReadOnly");
            }

            void IList.Clear()
            {
                throw new NotSupportedException("ListBoxSelectedIndexCollectionIsReadOnly");
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException("ListBoxSelectedIndexCollectionIsReadOnly");
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException("ListBoxSelectedIndexCollectionIsReadOnly");
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException("ListBoxSelectedIndexCollectionIsReadOnly");
            }

            public int this[int index]
            {
                get
                {
                    object identifier = InnerArray.GetEntryObject(index, SelectedObjectCollection.SelectedObjectMask);
                    return InnerArray.IndexOfIdentifier(identifier, 0);
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException("ListBoxSelectedIndexCollectionIsReadOnly");
                }
            }

            private ItemArray InnerArray
            {
                get
                {
                    owner.SelectedItems.EnsureUpToDate();
                    return ((ObjectCollection)owner.Items).InnerArray;
                }
            }

            public void CopyTo(Array destination, int index)
            {
                int cnt = Count;
                for (int i = 0; i < cnt; i++)
                {
                    destination.SetValue(this[i], i + index);
                }
            }

            public void Clear()
            {
                if (owner != null)
                {
                    owner.ClearSelected();
                }
            }

            public void Add(int index)
            {
                if (owner != null)
                {
                    ObjectCollection items = owner.Items;
                    if (items != null)
                    {
                        if (index != -1 && !Contains(index))
                        {
                            owner.SetSelected(index, true);
                        }
                    }
                }
            }

            public void Remove(int index)
            {
                if (owner != null)
                {
                    ObjectCollection items = owner.Items;
                    if (items != null)
                    {
                        if (index != -1 && Contains(index))
                        {
                            owner.SetSelected(index, false);
                        }
                    }
                }
            }

            public IEnumerator GetEnumerator()
            {
                return new SelectedIndexEnumerator(this);
            }

            private class SelectedIndexEnumerator : IEnumerator
            {
                private SelectedIndexCollection items;
                private int current;

                public SelectedIndexEnumerator(SelectedIndexCollection items)
                {
                    this.items = items;
                    this.current = -1;
                }

                bool IEnumerator.MoveNext()
                {

                    if (current < items.Count - 1)
                    {
                        current++;
                        return true;
                    }
                    else
                    {
                        current = items.Count;
                        return false;
                    }
                }

                void IEnumerator.Reset()
                {
                    current = -1;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (current == -1 || current == items.Count)
                        {
                            throw new InvalidOperationException("ListEnumCurrentOutOfRange");
                        }

                        return items[current];
                    }
                }
            }
        }

        // Should be "ObjectCollection", except we already have one of those.
        public class SelectedObjectCollection : IList
        {

            // This is the bitmask used within ItemArray to identify selected objects.
            internal static int SelectedObjectMask = ItemArray.CreateMask();

            private ListBox owner;
            private bool stateDirty;
            private int lastVersion;
            private int count;

            /* C#r: protected */
            public SelectedObjectCollection(ListBox owner)
            {
                this.owner = owner;
                this.stateDirty = true;
                this.lastVersion = -1;
            }

            public int Count
            {
                get
                {
                    if (owner.IsHandleCreated)
                    {
                        SelectionMode current = (owner.selectionModeChanging) ? owner.cachedSelectionMode : owner.selectionMode;
                        switch (current)
                        {

                            case SelectionMode.None:
                                return 0;

                            case SelectionMode.One:
                                int index = owner.SelectedIndex;
                                if (index >= 0)
                                {
                                    return 1;
                                }
                                return 0;

                            case SelectionMode.MultiSimple:
                            case SelectionMode.MultiExtended:
                                return 0; // unchecked((int)(long)owner.SendMessage(NativeMethods.LB_GETSELCOUNT, 0, 0));
                        }

                        return 0;
                    }

                    // If the handle hasn't been created, we must do this the hard way.
                    // Getting the count when using a mask is expensive, so cache it.
                    //
                    if (lastVersion != InnerArray.Version)
                    {
                        lastVersion = InnerArray.Version;
                        count = InnerArray.GetCount(SelectedObjectMask);
                    }

                    return count;
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
                    return true;
                }
            }

            internal void Dirty()
            {
                stateDirty = true;
            }

            private ItemArray InnerArray
            {
                get
                {
                    EnsureUpToDate();
                    return ((ObjectCollection)owner.Items).InnerArray;
                }
            }


            internal void EnsureUpToDate()
            {
                if (stateDirty)
                {
                    stateDirty = false;
                    if (owner.IsHandleCreated)
                    {
                        owner.NativeUpdateSelection();
                    }
                }
            }


            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public bool Contains(object selectedObject)
            {
                return IndexOf(selectedObject) != -1;
            }

            public int IndexOf(object selectedObject)
            {
                return InnerArray.IndexOf(selectedObject, SelectedObjectMask);
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException("ListBoxSelectedObjectCollectionIsReadOnly");
            }

            void IList.Clear()
            {
                throw new NotSupportedException("ListBoxSelectedObjectCollectionIsReadOnly");
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException("ListBoxSelectedObjectCollectionIsReadOnly");
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException("ListBoxSelectedObjectCollectionIsReadOnly");
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException("ListBoxSelectedObjectCollectionIsReadOnly");
            }

            // A new internal method used in SelectedIndex getter...
            // For a Multi select ListBox there can be two items with the same name ...
            // and hence a object comparison is required...
            // This method returns the "object" at the passed index rather than the "item" ...
            // this "object" is then compared in the IndexOf( ) method of the itemsCollection.
            //
            internal object GetObjectAt(int index)
            {
                return InnerArray.GetEntryObject(index, SelectedObjectCollection.SelectedObjectMask);
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public object this[int index]
            {
                get
                {
                    return InnerArray.GetItem(index, SelectedObjectMask);
                }
                set
                {
                    throw new NotSupportedException("ListBoxSelectedObjectCollectionIsReadOnly");
                }
            }

            public void CopyTo(Array destination, int index)
            {
                int cnt = InnerArray.GetCount(SelectedObjectMask);
                for (int i = 0; i < cnt; i++)
                {
                    destination.SetValue(InnerArray.GetItem(i, SelectedObjectMask), i + index);
                }
            }

            public IEnumerator GetEnumerator()
            {
                return InnerArray.GetEnumerator(SelectedObjectMask);
            }

            internal bool GetSelected(int index)
            {
                return InnerArray.GetState(index, SelectedObjectMask);
            }

            // when SelectedObjectsCollection::ItemArray is accessed we push the selection from Native ListBox into our .Net ListBox - see EnsureUpToDate()
            // when we create the handle we need to be able to do the opposite : push the selection from .Net ListBox into Native ListBox
            internal void PushSelectionIntoNativeListBox(int index)
            {
                // we can't use ItemArray accessor because this will wipe out our Selection collection
                bool selected = ((ObjectCollection)owner.Items).InnerArray.GetState(index, SelectedObjectMask);
                // push selection only if the item is actually selected
                // this also takes care of the case where owner.SelectionMode == SelectionMode.One
                if (selected)
                {
                    this.owner.NativeSetSelected(index, true /*we signal selection to the native listBox only if the item is actually selected*/);
                }
            }

            internal void SetSelected(int index, bool value)
            {
                InnerArray.SetState(index, SelectedObjectMask, value);
            }

            public void Clear()
            {
                if (owner != null)
                {
                    owner.ClearSelected();
                }
            }

            public void Add(object value)
            {
                if (owner != null)
                {
                    ObjectCollection items = owner.Items;
                    if (items != null && value != null)
                    {
                        int index = items.IndexOf(value);
                        if (index != -1 && !GetSelected(index))
                        {
                            owner.SelectedIndex = index;
                        }
                    }
                }
            }

            public void Remove(object value)
            {
                if (owner != null)
                {
                    ObjectCollection items = owner.Items;
                    if (items != null & value != null)
                    {
                        int index = items.IndexOf(value);
                        if (index != -1 && GetSelected(index))
                        {
                            owner.SetSelected(index, false);
                        }
                    }
                }
            }
        }

        //private sealed class ListBoxAccessibleObject : ControlAccessibleObject
        //{
        //    private readonly ListBox owner;

        //    public ListBoxAccessibleObject(ListBox control) : base(control)
        //    {
        //        this.owner = control;
        //    }

        //    #region IAccessibleEx related overrides

        //    internal override bool IsIAccessibleExSupported()
        //    {
        //        return true;
        //    }

        //    internal override object GetObjectForChild(int childId)
        //    {
        //        Accessibility.IAccessible systemIAccessible = this.GetSystemIAccessibleInternal();
        //        if (IsChildIdValid(childId, systemIAccessible))
        //        {
        //            if ((AccessibleRole)systemIAccessible.accRole[childId] == AccessibleRole.ListItem)
        //            {
        //                return new ListBoxItemAccessibleObject(this.owner, childId);
        //            }
        //        }

        //        return base.GetObjectForChild(childId);
        //    }

        //    #endregion

        //    private static bool IsChildIdValid(int childId, Accessibility.IAccessible systemIAccessible)
        //    {
        //        // 0 (i.e. CHILDID_SELF) references the control itself, so it is considered "invalid" in this scope
        //        return childId > 0 && childId <= systemIAccessible.accChildCount;
        //    }

        //    private sealed class ListBoxItemAccessibleObject : AccessibleObject
        //    {
        //        private readonly int childId;
        //        private readonly ListBox owner;

        //        public ListBoxItemAccessibleObject(ListBox owner, int childId)
        //        {
        //            Debug.Assert(owner != null, $"{nameof(owner)} should not be null");
        //            Debug.Assert(childId > 0, $"{nameof(childId)} has unexpected value");

        //            this.owner = owner;
        //            this.childId = childId;
        //        }

        //        #region IAccessibleEx related overrides

        //        internal override bool IsIAccessibleExSupported()
        //        {
        //            return true;
        //        }

        //        internal override bool IsPatternSupported(int patternId)
        //        {
        //            if (patternId == NativeMethods.UIA_ScrollItemPatternId)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return base.IsPatternSupported(patternId);
        //            }
        //        }

        //        #endregion

        //        #region IScrollItemProvider related overrides

        //        internal override void ScrollIntoView()
        //        {
        //            if (this.owner.IsHandleCreated && IsChildIdValid(this.childId, this.owner.AccessibilityObject.GetSystemIAccessibleInternal()))
        //            {
        //                this.owner.TopIndex = this.childId - 1;
        //            }
        //        }

        //        #endregion
        //    }
        //}
    }
}