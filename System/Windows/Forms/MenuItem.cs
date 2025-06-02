using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Windows.Forms
{
    public class MenuItem : Menu
    {
        internal const int STATE_BARBREAK = 0x00000020;
        internal const int STATE_BREAK = 0x00000040;
        internal const int STATE_CHECKED = 0x00000008;
        internal const int STATE_DEFAULT = 0x00001000;
        internal const int STATE_DISABLED = 0x00000003;
        internal const int STATE_RADIOCHECK = 0x00000200;
        internal const int STATE_HIDDEN = 0x00010000;
        internal const int STATE_MDILIST = 0x00020000;
        internal const int STATE_CLONE_MASK = 0x0003136B;
        internal const int STATE_OWNERDRAW = 0x00000100;
        internal const int STATE_INMDIPOPUP = 0x00000200;
        internal const int STATE_HILITE = 0x00000080;

        private Menu menu;
        private bool hasHandle;
        private MenuItemData data;
        private int dataVersion;
        private MenuItem nextLinkedItem; // Next item linked to the same MenuItemData.

        // We need to store a table of all created menuitems, so that other objects
        // such as ContainerControl can get a reference to a particular menuitem,
        // given a unique ID.
        private static Hashtable allCreatedMenuItems = new Hashtable();
        private const uint firstUniqueID = 0xC0000000;
        private static long nextUniqueID = firstUniqueID;
        private uint uniqueID = 0;
        private IntPtr msaaMenuInfoPtr = IntPtr.Zero;
        private bool menuItemIsCreated = false;

        public MenuItem() : this(MenuMerge.Add, 0, 0, null, null, null, null, null)
        {
        }

        public MenuItem(string text) : this(MenuMerge.Add, 0, 0, text, null, null, null, null)
        {
        }

        public MenuItem(string text, EventHandler onClick) : this(MenuMerge.Add, 0, 0, text, onClick, null, null, null)
        {
        }


        public MenuItem(string text, EventHandler onClick, Shortcut shortcut) : this(MenuMerge.Add, 0, shortcut, text, onClick, null, null, null)
        {
        }

        public MenuItem(string text, MenuItem[] items) : this(MenuMerge.Add, 0, 0, text, null, null, null, items)
        {
        }

        internal MenuItem(MenuItemData data)
        : base(null)
        {
            data.AddItem(this);
        }
        [SuppressMessage("Microsoft.Performance", "CA1806:DoNotIgnoreMethodResults")]
        public MenuItem(MenuMerge mergeType, int mergeOrder, Shortcut shortcut,
                        string text, EventHandler onClick, EventHandler onPopup,
                        EventHandler onSelect, MenuItem[] items)

        : base(items)
        {

            new MenuItemData(this, mergeType, mergeOrder, shortcut, true,
                             text, onClick, onPopup, onSelect, null, null);
        }

#if DEBUG
        private string _debugText;
        private int _creationNumber;
        private Menu _debugParentMenu;
        private static int CreateCount;
#endif


        [
        Browsable(false),
        DefaultValue(false)
        ]
        public bool BarBreak
        {
            get
            {
                return (data.State & STATE_BARBREAK) != 0;
            }

            set
            {
                data.SetState(STATE_BARBREAK, value);
            }
        }

        [Browsable(false), DefaultValue(false)]
        public bool Break
        {
            get
            {
                return (data.State & STATE_BREAK) != 0;
            }

            set
            {
                data.SetState(STATE_BREAK, value);
            }
        }

        [DefaultValue(false)]
        public bool Checked
        {
            get
            {
                return (data.State & STATE_CHECKED) != 0;
            }

            set
            {
                //if trying to set checked=true - if we're a top-level item (from a mainmenu) or have children, don't do this...
                if (value == true && (ItemCount != 0 || (Parent != null && (Parent is MainMenu))))
                {
                    throw new ArgumentException("MenuItemInvalidCheckProperty");
                }

                data.SetState(STATE_CHECKED, value);
            }
        }

        [DefaultValue(false)]
        public bool DefaultItem
        {
            get { return (data.State & STATE_DEFAULT) != 0; }
            set
            {
                if (menu != null)
                {
                    if (value)
                    {
                        //UnsafeNativeMethods.SetMenuDefaultItem(new HandleRef(menu, menu.handle), MenuID, false);
                    }
                    else if (DefaultItem)
                    {
                        //UnsafeNativeMethods.SetMenuDefaultItem(new HandleRef(menu, menu.handle), -1, false);
                    }
                }
                data.SetState(STATE_DEFAULT, value);
            }
        }

        [DefaultValue(false)]
        public bool OwnerDraw
        {
            get
            {
                return ((data.State & STATE_OWNERDRAW) != 0);
            }
            set
            {
                data.SetState(STATE_OWNERDRAW, value);
            }

        }

        [Localizable(true), DefaultValue(true)]
        public bool Enabled
        {
            get
            {
                return (data.State & STATE_DISABLED) == 0;
            }

            set
            {
                data.SetState(STATE_DISABLED, !value);
            }
        }

        [Browsable(false)]
        public int Index
        {
            get
            {
                if (menu != null)
                {
                    for (int i = 0; i < menu.ItemCount; i++)
                    {
                        if (menu.items[i] == this) return i;
                    }
                }
                return -1;
            }

            set
            {
                int oldIndex = Index;
                if (oldIndex >= 0)
                {
                    if (value < 0 || value >= menu.ItemCount)
                    {
                        throw new ArgumentOutOfRangeException("Index", "InvalidArgument");
                    }

                    if (value != oldIndex)
                    {
                        // this.menu reverts to null when we're removed, so hold onto it in a local variable
                        Menu parent = menu;
                        parent.MenuItems.RemoveAt(oldIndex);
                        parent.MenuItems.Add(value, this);
                    }
                }
            }
        }

        [Browsable(false)]
        public override bool IsParent
        {
            get
            {
                bool parent = false;
                if (data != null && MdiList)
                {
                    for (int i = 0; i < ItemCount; i++)
                    {
                        if (!(items[i].data.UserData is MdiListUserData))
                        {
                            parent = true;
                            break;
                        }
                    }
                    if (!parent)
                    {
                        if (FindMdiForms().Length > 0)
                        {
                            parent = true;
                        }
                    }
                    if (!parent)
                    {
                        if (menu != null && !(menu is MenuItem))
                        {
                            parent = true;
                        }
                    }
                }
                else
                {
                    parent = base.IsParent;
                }
                return parent;
            }
        }

        [DefaultValue(false)]
        public bool MdiList
        {
            get
            {
                return (data.State & STATE_MDILIST) != 0;
            }
            set
            {
                data.MdiList = value;
                CleanListItems(this);
            }
        }

        internal Menu Menu
        {
            get
            {
                return menu;
            }
            set
            {
                menu = value;
            }
        }

        protected int MenuID
        {
            get { return data.GetMenuID(); }
        }

        internal bool Selected
        {
            get
            {
                if (menu == null)
                    return false;

                NativeMethods.MENUITEMINFO_T info = new NativeMethods.MENUITEMINFO_T();
                info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MENUITEMINFO_T));
                info.fMask = NativeMethods.MIIM_STATE;
                //UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(menu, menu.handle), MenuID, false, info);

                return (info.fState & STATE_HILITE) != 0;
            }
        }


        internal int MenuIndex
        {
            get
            {
                if (menu == null) return -1;

                //int count = UnsafeNativeMethods.GetMenuItemCount(new HandleRef(menu, menu.Handle));
                int count = 0;
                int id = MenuID;
                NativeMethods.MENUITEMINFO_T info = new NativeMethods.MENUITEMINFO_T();
                info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MENUITEMINFO_T));
                info.fMask = NativeMethods.MIIM_ID | NativeMethods.MIIM_SUBMENU;

                for (int i = 0; i < count; i++)
                {
                    //UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(menu, menu.handle), i, true, info);

                    // For sub menus, the handle is always valid.  For
                    // items, however, it is always zero.
                    //
                    if ((info.hSubMenu == IntPtr.Zero || info.hSubMenu == Handle) && info.wID == id)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        [DefaultValue(MenuMerge.Add)]
        public MenuMerge MergeType
        {
            get
            {
                return data.mergeType;
            }
            set
            {

                //valid values are 0x0 to 0x3
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)MenuMerge.Add, (int)MenuMerge.Remove))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(MenuMerge));
                }

                data.MergeType = value;
            }
        }

        [DefaultValue(0)]
        public int MergeOrder
        {
            get
            {
                return data.mergeOrder;
            }
            set
            {
                data.MergeOrder = value;
            }
        }

        [Browsable(false)]
        public char Mnemonic
        {
            get
            {
                return data.Mnemonic;
            }
        }

        [Browsable(false)]
        public Menu Parent
        {
            get { return menu; }
        }

        [DefaultValue(false)]
        public bool RadioCheck
        {
            get
            {
                return (data.State & STATE_RADIOCHECK) != 0;
            }
            set
            {
                data.SetState(STATE_RADIOCHECK, value);
            }
        }

        internal override bool RenderIsRightToLeft
        {
            get
            {
                if (Parent == null)
                    return false;
                else
                    return Parent.RenderIsRightToLeft;
            }
        }

        [Localizable(true)]
        public string Text
        {
            get
            {
                return data.caption;
            }
            set
            {
                data.SetCaption(value);
            }
        }

        [Localizable(true), DefaultValue(Shortcut.None)]
        public Shortcut Shortcut
        {
            get
            {
                return data.shortcut;
            }
            [SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]
            set
            {
                if (!Enum.IsDefined(typeof(Shortcut), value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(Shortcut));
                }

                data.shortcut = value;
                UpdateMenuItem(true);
            }
        }

        [DefaultValue(true), Localizable(true)]
        public bool ShowShortcut
        {
            get
            {

                return data.showShortcut;
            }
            set
            {
                if (value != data.showShortcut)
                {
                    data.showShortcut = value;
                    UpdateMenuItem(true);
                }
            }
        }

        [Localizable(true), DefaultValue(true)]
        public bool Visible
        {
            get
            {
                return (data.State & STATE_HIDDEN) == 0;
            }
            set
            {
                data.Visible = value;
            }
        }

        public event EventHandler Click
        {
            add
            {
                data.onClick += value;
            }
            remove
            {
                data.onClick -= value;
            }
        }

        public event DrawItemEventHandler DrawItem
        {
            add
            {
                data.onDrawItem += value;
            }
            remove
            {
                data.onDrawItem -= value;
            }
        }

        public event MeasureItemEventHandler MeasureItem
        {
            add
            {
                data.onMeasureItem += value;
            }
            remove
            {
                data.onMeasureItem -= value;
            }
        }

        public event EventHandler Popup
        {
            add
            {
                data.onPopup += value;
            }
            remove
            {
                data.onPopup -= value;
            }
        }

        public event EventHandler Select
        {
            add
            {
                data.onSelect += value;
            }
            remove
            {
                data.onSelect -= value;
            }
        }

        private static void CleanListItems(MenuItem senderMenu)
        {

            // remove dynamic items.

            for (int i = senderMenu.MenuItems.Count - 1; i >= 0; i--)
            {
                MenuItem item = senderMenu.MenuItems[i];
                if (item.data.UserData is MdiListUserData)
                {
                    // this is a dynamic item. clean it up!
                    // 
                    item.Dispose();
                    continue;
                }
            }
        }

        public virtual MenuItem CloneMenu()
        {
            MenuItem newItem = new MenuItem();
            newItem.CloneMenu(this);
            return newItem;
        }

        [SuppressMessage("Microsoft.Performance", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")] // Shipped in Everett
        protected void CloneMenu(MenuItem itemSrc)
        {
            base.CloneMenu(itemSrc);
            int state = itemSrc.data.State;
            new MenuItemData(this,
                             itemSrc.MergeType, itemSrc.MergeOrder, itemSrc.Shortcut, itemSrc.ShowShortcut,
                             itemSrc.Text, itemSrc.data.onClick, itemSrc.data.onPopup, itemSrc.data.onSelect,
                             itemSrc.data.onDrawItem, itemSrc.data.onMeasureItem);
            data.SetState(state & STATE_CLONE_MASK, true);
        }

        internal virtual void CreateMenuItem()
        {
            if ((data.State & STATE_HIDDEN) == 0)
            {
                NativeMethods.MENUITEMINFO_T info = CreateMenuItemInfo();
                //UnsafeNativeMethods.InsertMenuItem(new HandleRef(menu, menu.handle), -1, true, info);

                hasHandle = info.hSubMenu != IntPtr.Zero;
                dataVersion = data.version;

                menuItemIsCreated = true;
                if (RenderIsRightToLeft)
                {
                    Menu.UpdateRtl(true);
                }
            }
        }

        private NativeMethods.MENUITEMINFO_T CreateMenuItemInfo()
        {
            NativeMethods.MENUITEMINFO_T info = new NativeMethods.MENUITEMINFO_T();
            info.fMask = NativeMethods.MIIM_ID | NativeMethods.MIIM_STATE |
                         NativeMethods.MIIM_SUBMENU | NativeMethods.MIIM_TYPE | NativeMethods.MIIM_DATA;
            info.fType = data.State & (STATE_BARBREAK | STATE_BREAK | STATE_RADIOCHECK | STATE_OWNERDRAW);

            // V7#646 - Top level menu items shouldn't have barbreak or break
            //          bits set on them...
            //
            bool isTopLevel = false;
            if (menu == GetMainMenu())
            {
                isTopLevel = true;
            }

            if (data.caption.Equals("-"))
            {
                if (isTopLevel)
                {
                    data.caption = " ";
                    info.fType |= NativeMethods.MFT_MENUBREAK;
                }
                else
                {
                    info.fType |= NativeMethods.MFT_SEPARATOR;
                }
            }


            info.fState = data.State & (STATE_CHECKED | STATE_DEFAULT | STATE_DISABLED);

            info.wID = MenuID;
            if (IsParent)
            {
                info.hSubMenu = Handle;
            }
            info.hbmpChecked = IntPtr.Zero;
            info.hbmpUnchecked = IntPtr.Zero;

            // Assign a unique ID to this menu item object...
            //    The ID is stored in the dwItemData of the corresponding Win32 menu item, so that when we get Win32
            //    messages about the item later, we can delegate to the original object menu item object. A static
            //    hash table is used to map IDs to menu item objects.
            //
            if (uniqueID == 0)
            {
                lock (allCreatedMenuItems)
                {
                    uniqueID = (uint)Interlocked.Increment(ref nextUniqueID);
                    Debug.Assert(uniqueID >= firstUniqueID); // ...check for ID range exhaustion (unlikely!)
                    // We add a weak ref wrapping a MenuItem to the static hash table, as supposed to adding the item 
                    // ref itself, to allow the item to be finalized in case it is not disposed and no longer referenced 
                    // anywhere else, hence preventing leaks. See bug#352644
                    allCreatedMenuItems.Add(uniqueID, new WeakReference(this));
                }
            }

            // To check it's 32-bit OS or 64-bit OS.
            if (IntPtr.Size == 4)
            {
                // Store the unique ID in the dwItemData...
                //     For simple menu items, we can just put the unique ID in the dwItemData. But for owner-draw items,
                //     we need to point the dwItemData at an MSAAMENUINFO structure so that MSAA can get the item text.
                //     To allow us to reliably distinguish between IDs and structure pointers later on, we keep IDs in
                //     the 0xC0000000-0xFFFFFFFF range. This is the top 1Gb of unmananged process memory, where an app's
                //     heap allocations should never come from. So that we can still get the ID from the dwItemData for
                //     an owner-draw item later on, a copy of the ID is tacked onto the end of the MSAAMENUINFO structure.
                //
                if (data.OwnerDraw)
                    info.dwItemData = AllocMsaaMenuInfo();
                else
                    info.dwItemData = (IntPtr)unchecked((int)uniqueID);
            }
            else
            {
                // On Win64, there are no reserved address ranges we can use for menu item IDs. So instead we will
                // have to allocate an MSAMENUINFO heap structure for all menu items, not just owner-drawn ones.
                info.dwItemData = AllocMsaaMenuInfo();
            }

            // We won't render the shortcut if: 1) it's not set, 2) we're a parent, 3) we're toplevel
            //
            if (data.showShortcut && data.shortcut != 0 && !IsParent && !isTopLevel)
            {
                info.dwTypeData = data.caption + "\t" + TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString((Keys)(int)data.shortcut);
            }
            else
            {
                // Windows issue: Items with empty captions sometimes block keyboard
                // access to other items in same menu.
                info.dwTypeData = (data.caption.Length == 0 ? " " : data.caption);
            }
            info.cch = 0;

            return info;
        }

        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                if (menu != null)
                {
                    menu.MenuItems.Remove(this);
                }

                if (data != null)
                {
                    data.RemoveItem(this);
                }
                lock (allCreatedMenuItems)
                {
                    allCreatedMenuItems.Remove(uniqueID);
                }
                this.uniqueID = 0;

            }
            FreeMsaaMenuInfo();
            base.Dispose(disposing);
        }


        // Given a unique menu item ID, find the corresponding MenuItem
        // object, using the master lookup table of all created MenuItems.
        internal static MenuItem GetMenuItemFromUniqueID(uint uniqueID)
        {
            WeakReference weakRef = (WeakReference)allCreatedMenuItems[uniqueID];
            if (weakRef != null && weakRef.IsAlive)
            {
                return (MenuItem)weakRef.Target;
            }
            Debug.Fail("Weakref for menu item has expired or has been removed!  Who is trying to access this ID?");
            return null;
        }

        // Given the "item data" value of a Win32 menu item, find the corresponding MenuItem object (using
        // the master lookup table of all created MenuItems). The item data may be either the unique menu
        // item ID, or a pointer to an MSAAMENUINFO structure with a copy of the unique ID tacked to the end.
        // To reliably tell IDs and structure addresses apart, IDs live in the 0xC0000000-0xFFFFFFFF range.
        // This is the top 1Gb of unmananged process memory, where an app's heap allocations should never be.
        [SuppressMessage("Microsoft.Performance", "CA1808:AvoidCallsThatBoxValueTypes")]
        internal static MenuItem GetMenuItemFromItemData(IntPtr itemData)
        {
            uint uniqueID = (uint)(ulong)itemData;

            if (uniqueID == 0)
                return null;

            // To check it's 32-bit OS or 64-bit OS.
            if (IntPtr.Size == 4)
            {
                if (uniqueID < firstUniqueID)
                {
                    MsaaMenuInfoWithId msaaMenuInfo = (MsaaMenuInfoWithId)Marshal.PtrToStructure(itemData, typeof(MsaaMenuInfoWithId));
                    uniqueID = msaaMenuInfo.uniqueID;
                }
            }
            else
            {
                // ...its always a pointer on Win64 (see CreateMenuItemInfo)
                MsaaMenuInfoWithId msaaMenuInfo = (MsaaMenuInfoWithId)Marshal.PtrToStructure(itemData, typeof(MsaaMenuInfoWithId));
                uniqueID = msaaMenuInfo.uniqueID;
            }

            return GetMenuItemFromUniqueID(uniqueID);
        }

        // MsaaMenuInfoWithId is an MSAAMENUINFO structure with a menu item ID field tacked onto the
        // end. This allows us to pass the data we need to Win32 / MSAA, and still be able to get the ID
        // out again later on, so we can delegate Win32 menu messages back to the correct MenuItem object.
        [StructLayout(LayoutKind.Sequential)]
        private struct MsaaMenuInfoWithId
        {
            public NativeMethods.MSAAMENUINFO msaaMenuInfo;
            public uint uniqueID;

            public MsaaMenuInfoWithId(string text, uint uniqueID)
            {
                msaaMenuInfo = new NativeMethods.MSAAMENUINFO(text);
                this.uniqueID = uniqueID;
            }
        }

        // Creates an MSAAMENUINFO structure (in the unmanaged heap) based on the current state
        // of this MenuItem object. Address of this structure is cached in the object so we can
        // free it later on using FreeMsaaMenuInfo(). If structure has already been allocated,
        // it is destroyed and a new one created.
        private IntPtr AllocMsaaMenuInfo()
        {
            FreeMsaaMenuInfo();
            msaaMenuInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MsaaMenuInfoWithId)));

            // To check it's 32-bit OS or 64-bit OS.
            if (IntPtr.Size == 4)
            {
                // We only check this on Win32, irrelevant on Win64 (see CreateMenuItemInfo)
                Debug.Assert(((uint)(ulong)msaaMenuInfoPtr) < firstUniqueID); // ...check for incursion into menu item ID range (unlikely!)
            }

            MsaaMenuInfoWithId msaaMenuInfoStruct = new MsaaMenuInfoWithId(data.caption, uniqueID);
            Marshal.StructureToPtr(msaaMenuInfoStruct, msaaMenuInfoPtr, false);
            Debug.Assert(msaaMenuInfoPtr != IntPtr.Zero);
            return msaaMenuInfoPtr;
        }

        // Frees the MSAAMENUINFO structure (in the unmanaged heap) for the current MenuObject object,
        // if one has previously been allocated. Takes care to free sub-structures too, to avoid leaks!
        private void FreeMsaaMenuInfo()
        {
            if (msaaMenuInfoPtr != IntPtr.Zero)
            {
                Marshal.DestroyStructure(msaaMenuInfoPtr, typeof(MsaaMenuInfoWithId));
                Marshal.FreeHGlobal(msaaMenuInfoPtr);
                msaaMenuInfoPtr = IntPtr.Zero;
            }
        }

        internal override void ItemsChanged(int change)
        {
            base.ItemsChanged(change);

            if (change == CHANGE_ITEMS)
            {
                // when the menu collection changes deal w/ it locally
                Debug.Assert(!this.created, "base.ItemsChanged should have wiped out our handles");
                if (menu != null && menu.created)
                {
                    UpdateMenuItem(true);
                    CreateMenuItems();
                }
            }
            else
            {
                if (!hasHandle && IsParent)
                    UpdateMenuItem(true);

                MainMenu main = GetMainMenu();
                if (main != null && ((data.State & STATE_INMDIPOPUP) == 0))
                {
                    main.ItemsChanged(change, this);
                }
            }
        }

        internal void ItemsChanged(int change, MenuItem item)
        {
            if (change == CHANGE_ITEMADDED &&
                this.data != null &&
                this.data.baseItem != null &&
                this.data.baseItem.MenuItems.Contains(item))
            {
                if (menu != null && menu.created)
                {
                    UpdateMenuItem(true);
                    CreateMenuItems();
                }
                else if (this.data != null)
                {
                    MenuItem currentMenuItem = this.data.firstItem;
                    while (currentMenuItem != null)
                    {
                        if (currentMenuItem.created)
                        {
                            MenuItem newItem = item.CloneMenu();
                            item.data.AddItem(newItem);
                            currentMenuItem.MenuItems.Add(newItem);
                            // newItem.menu = currentMenuItem;
                            // newItem.CreateMenuItem();
                            break;
                        }
                        currentMenuItem = currentMenuItem.nextLinkedItem;
                    }
                }
            }
        }

        internal Form[] FindMdiForms()
        {
            Form[] forms = null;
            MainMenu main = GetMainMenu();
            Form menuForm = null;
            if (main != null)
            {
                menuForm = main.GetFormUnsafe();
            }
            if (menuForm != null)
            {
                forms = menuForm.MdiChildren;
            }
            if (forms == null)
            {
                forms = new Form[0];
            }
            return forms;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")] // "-" is OK
        private void PopulateMdiList()
        {
            MenuItem senderMenu = this;
            data.SetState(STATE_INMDIPOPUP, true);
            try
            {
                CleanListItems(this);

                // add new items
                //
                Form[] forms = FindMdiForms();
                if (forms != null && forms.Length > 0)
                {

                    Form activeMdiChild = GetMainMenu().GetFormUnsafe().ActiveMdiChild;

                    if (senderMenu.MenuItems.Count > 0)
                    {
                        // SECREVIEW : Late-binding does not represent a security threat, see bug#411899 for more info..
                        //
                        MenuItem sep = (MenuItem)Activator.CreateInstance(this.GetType());
                        sep.data.UserData = new MdiListUserData();
                        sep.Text = "-";
                        senderMenu.MenuItems.Add(sep);
                    }

                    // VSWhidbey 93540: Build a list of child windows to be displayed in
                    // the MDIList menu item...
                    // Show the first maxMenuForms visible elements of forms[] as Window menu items, except:
                    // Always show the active form, even if it's not in the first maxMenuForms visible elements of forms[].
                    // If the active form isn't in the first maxMenuForms forms, then show the first maxMenuForms-1 elements
                    // in forms[], and make the active form the last one on the menu.
                    // VSWhidbey 260405: don't count nonvisible forms against the limit on Window menu items.

                    const int maxMenuForms = 9; // Max number of Window menu items for forms
                    int visibleChildren = 0;    // the number of visible child forms (so we know to show More Windows...)
                    int accel = 1;              // prefix the form name with this digit, underlined, as an accelerator
                    int formsAddedToMenu = 0;
                    bool activeFormAdded = false;
                    for (int i = 0; i < forms.Length; i++)
                    {
                        if (forms[i].Visible)
                        {
                            visibleChildren++;
                            if ((activeFormAdded && (formsAddedToMenu < maxMenuForms)) ||  // don't exceed max
                                (!activeFormAdded && (formsAddedToMenu < (maxMenuForms - 1)) ||  // save room for active if it's not in yet
                                (forms[i].Equals(activeMdiChild))))
                            {                           // there's always room for activeMdiChild
                                // SECREVIEW : Late-binding does not represent a security threat, see bug#411899 for more info..
                                //
                                MenuItem windowItem = (MenuItem)Activator.CreateInstance(this.GetType());
                                windowItem.data.UserData = new MdiListFormData(this, i);

                                if (forms[i].Equals(activeMdiChild))
                                {
                                    windowItem.Checked = true;
                                    activeFormAdded = true;
                                }
                                windowItem.Text = String.Format(CultureInfo.CurrentUICulture, "&{0} {1}", accel, forms[i].Text);
                                accel++;
                                formsAddedToMenu++;
                                senderMenu.MenuItems.Add(windowItem);
                            }
                        }
                    }

                    // VSWhidbey 93540: Display the More Windows menu option when there are more than 9 MDI
                    // Child menu items to be displayed. This is necessary because we're managing our own
                    // MDI lists, rather than letting Windows do this for us.
                    if (visibleChildren > maxMenuForms)
                    {
                        // SECREVIEW : Late-binding does not represent a security threat, see bug#411899 for more info..
                        //
                        MenuItem moreWindows = (MenuItem)Activator.CreateInstance(this.GetType());
                        moreWindows.data.UserData = new MdiListMoreWindowsData(this);
                        moreWindows.Text = "MDIMenuMoreWindows";
                        senderMenu.MenuItems.Add(moreWindows);
                    }
                }
            }
            finally
            {
                data.SetState(STATE_INMDIPOPUP, false);
            }
        }

        public virtual MenuItem MergeMenu()
        {
            // SECREVIEW : Late-binding does not represent a security threat, see bug#411899 for more info..
            //
            MenuItem newItem = (MenuItem)Activator.CreateInstance(this.GetType());
            data.AddItem(newItem);
            newItem.MergeMenu(this);
            return newItem;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // Shipped in Everett
        public void MergeMenu(MenuItem itemSrc)
        {
            base.MergeMenu(itemSrc);
            itemSrc.data.AddItem(this);
        }

        protected virtual void OnClick(EventArgs e)
        {
            if (data.UserData is MdiListUserData)
            {
                ((MdiListUserData)data.UserData).OnClick(e);
            }
            else if (data.baseItem != this)
            {
                data.baseItem.OnClick(e);
            }
            else if (data.onClick != null)
            {
                data.onClick(this, e);
            }
        }

        protected virtual void OnDrawItem(DrawItemEventArgs e)
        {
            if (data.baseItem != this)
            {
                data.baseItem.OnDrawItem(e);
            }
            else if (data.onDrawItem != null)
            {
                data.onDrawItem(this, e);
            }
        }

        protected virtual void OnMeasureItem(MeasureItemEventArgs e)
        {
            if (data.baseItem != this)
            {
                data.baseItem.OnMeasureItem(e);
            }
            else if (data.onMeasureItem != null)
            {
                data.onMeasureItem(this, e);
            }

        }

        protected virtual void OnPopup(EventArgs e)
        {
            bool recreate = false;
            for (int i = 0; i < ItemCount; i++)
            {
                if (items[i].MdiList)
                {
                    recreate = true;
                    items[i].UpdateMenuItem(true);
                }
            }
            if (recreate || (hasHandle && !IsParent))
            {
                UpdateMenuItem(true);
            }

            if (data.baseItem != this)
            {
                data.baseItem.OnPopup(e);
            }
            else if (data.onPopup != null)
            {
                data.onPopup(this, e);
            }

            // Update any subitem states that got changed in the event
            for (int i = 0; i < ItemCount; i++)
            {
                items[i].UpdateMenuItemIfDirty();
            }

            if (MdiList)
            {
                PopulateMdiList();
            }
        }

        protected virtual void OnSelect(EventArgs e)
        {
            if (data.baseItem != this)
            {
                data.baseItem.OnSelect(e);
            }
            else if (data.onSelect != null)
            {
                data.onSelect(this, e);
            }
        }

        protected virtual void OnInitMenuPopup(EventArgs e)
        {
            OnPopup(e);
        }

        // C#r
        internal virtual void _OnInitMenuPopup(EventArgs e)
        {
            OnInitMenuPopup(e);
        }

        public void PerformClick()
        {
            OnClick(EventArgs.Empty);
        }

        public virtual void PerformSelect()
        {
            OnSelect(EventArgs.Empty);
        }

        internal virtual bool ShortcutClick()
        {
            if (menu is MenuItem)
            {
                MenuItem parent = (MenuItem)menu;
                if (!parent.ShortcutClick() || menu != parent) return false;
            }
            if ((data.State & STATE_DISABLED) != 0) return false;
            if (ItemCount > 0)
                OnPopup(EventArgs.Empty);
            else
                OnClick(EventArgs.Empty);
            return true;
        }


        public override string ToString()
        {

            string s = base.ToString();

            String menuItemText = String.Empty;

            if (data != null && data.caption != null)
                menuItemText = data.caption;

            return s + ", Text: " + menuItemText;
        }

        internal void UpdateMenuItemIfDirty()
        {
            if (dataVersion != data.version)
                UpdateMenuItem(true);
        }

        internal void UpdateItemRtl(bool setRightToLeftBit)
        {
            if (!menuItemIsCreated)
            {
                return;
            }

            NativeMethods.MENUITEMINFO_T info = new NativeMethods.MENUITEMINFO_T();
            info.fMask = NativeMethods.MIIM_TYPE | NativeMethods.MIIM_STATE | NativeMethods.MIIM_SUBMENU;
            info.dwTypeData = new string('\0', Text.Length + 2);
            info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MENUITEMINFO_T));
            info.cch = info.dwTypeData.Length - 1;
            //UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(menu, menu.handle), MenuID, false, info);
            if (setRightToLeftBit)
            {
                info.fType |= NativeMethods.MFT_RIGHTJUSTIFY | NativeMethods.MFT_RIGHTORDER;
            }
            else
            {
                info.fType &= ~(NativeMethods.MFT_RIGHTJUSTIFY | NativeMethods.MFT_RIGHTORDER);
            }
            //UnsafeNativeMethods.SetMenuItemInfo(new HandleRef(menu, menu.handle), MenuID, false, info);
        }


        internal void UpdateMenuItem(bool force)
        {
            if (menu == null || !menu.created)
            {
                return;
            }

            if (force || menu is MainMenu || menu is ContextMenu)
            {
                NativeMethods.MENUITEMINFO_T info = CreateMenuItemInfo();
                //UnsafeNativeMethods.SetMenuItemInfo(new HandleRef(menu, menu.handle), MenuID, false, info);

                if (hasHandle && info.hSubMenu == IntPtr.Zero)
                {
                    ClearHandles();
                }
                hasHandle = info.hSubMenu != IntPtr.Zero;
                dataVersion = data.version;
                if (menu is MainMenu)
                {
                    Form f = ((MainMenu)menu).GetFormUnsafe();
                    if (f != null)
                    {
                        //SafeNativeMethods.DrawMenuBar(new HandleRef(f, f.Handle));
                    }
                }
            }
        }

        //internal void WmDrawItem(ref Message m)
        //{

        //    // Handles the OnDrawItem message sent from ContainerControl

        //    NativeMethods.DRAWITEMSTRUCT dis = (NativeMethods.DRAWITEMSTRUCT)m.GetLParam(typeof(NativeMethods.DRAWITEMSTRUCT));
        //    Debug.WriteLineIf(Control.PaletteTracing.TraceVerbose, Handle + ": Force set palette in MenuItem drawitem");
        //    IntPtr oldPal = Control.SetUpPalette(dis.hDC, false /*force*/, false);
        //    try
        //    {
        //        Graphics g = Graphics.FromHdcInternal(dis.hDC);
        //        try
        //        {
        //            OnDrawItem(new DrawItemEventArgs(g, SystemInformation.MenuFont, Rectangle.FromLTRB(dis.rcItem.left, dis.rcItem.top, dis.rcItem.right, dis.rcItem.bottom), Index, (DrawItemState)dis.itemState));
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
        //            SafeNativeMethods.SelectPalette(new HandleRef(null, dis.hDC), new HandleRef(null, oldPal), 0);
        //        }
        //    }

        //    m.Result = (IntPtr)1;
        //}

        //internal void WmMeasureItem(ref Message m)
        //{

        //    // Handles the OnMeasureItem message sent from ContainerControl

        //    // Obtain the measure item struct
        //    NativeMethods.MEASUREITEMSTRUCT mis = (NativeMethods.MEASUREITEMSTRUCT)m.GetLParam(typeof(NativeMethods.MEASUREITEMSTRUCT));
        //    // The OnMeasureItem handler now determines the height and width of the item

        //    IntPtr screendc = UnsafeNativeMethods.GetDC(NativeMethods.NullHandleRef);
        //    Graphics graphics = Graphics.FromHdcInternal(screendc);
        //    MeasureItemEventArgs mie = new MeasureItemEventArgs(graphics, Index);
        //    try
        //    {
        //        OnMeasureItem(mie);
        //    }
        //    finally
        //    {
        //        graphics.Dispose();
        //    }
        //    UnsafeNativeMethods.ReleaseDC(NativeMethods.NullHandleRef, new HandleRef(null, screendc));

        //    // Update the measure item struct with the new width and height
        //    mis.itemHeight = mie.ItemHeight;
        //    mis.itemWidth = mie.ItemWidth;
        //    Marshal.StructureToPtr(mis, m.LParam, false);

        //    m.Result = (IntPtr)1;
        //}


        internal class MenuItemData : ICommandExecutor
        {
            internal MenuItem baseItem;
            internal MenuItem firstItem;

            private int state;
            internal int version;
            internal MenuMerge mergeType;
            internal int mergeOrder;
            internal string caption;
            internal short mnemonic;
            internal Shortcut shortcut;
            internal bool showShortcut;
            internal EventHandler onClick;
            internal EventHandler onPopup;
            internal EventHandler onSelect;
            internal DrawItemEventHandler onDrawItem;
            internal MeasureItemEventHandler onMeasureItem;

            private object userData = null;
            internal Command cmd;

            internal MenuItemData(MenuItem baseItem, MenuMerge mergeType, int mergeOrder, Shortcut shortcut, bool showShortcut,
                                  string caption, EventHandler onClick, EventHandler onPopup, EventHandler onSelect,
                                  DrawItemEventHandler onDrawItem, MeasureItemEventHandler onMeasureItem)
            {
                AddItem(baseItem);
                this.mergeType = mergeType;
                this.mergeOrder = mergeOrder;
                this.shortcut = shortcut;
                this.showShortcut = showShortcut;
                this.caption = caption == null ? "" : caption;
                this.onClick = onClick;
                this.onPopup = onPopup;
                this.onSelect = onSelect;
                this.onDrawItem = onDrawItem;
                this.onMeasureItem = onMeasureItem;
                this.version = 1;
                this.mnemonic = -1;
            }


            internal bool OwnerDraw
            {
                get
                {
                    return ((State & STATE_OWNERDRAW) != 0);
                }
                set
                {
                    SetState(STATE_OWNERDRAW, value);
                }
            }

            internal bool MdiList
            {
                get
                {
                    return HasState(STATE_MDILIST);
                }
                set
                {
                    if (((state & STATE_MDILIST) != 0) != value)
                    {
                        SetState(STATE_MDILIST, value);
                        for (MenuItem item = firstItem; item != null; item = item.nextLinkedItem)
                        {
                            item.ItemsChanged(Menu.CHANGE_MDI);
                        }
                    }
                }
            }

            internal MenuMerge MergeType
            {
                get
                {
                    return mergeType;
                }
                set
                {
                    if (mergeType != value)
                    {
                        mergeType = value;
                        ItemsChanged(Menu.CHANGE_MERGE);
                    }
                }
            }

            internal int MergeOrder
            {
                get
                {
                    return mergeOrder;
                }
                set
                {
                    if (mergeOrder != value)
                    {
                        mergeOrder = value;
                        ItemsChanged(Menu.CHANGE_MERGE);
                    }
                }
            }

            internal char Mnemonic
            {
                get
                {
                    if (mnemonic == -1)
                    {
                        mnemonic = (short)WindowsFormsUtils.GetMnemonic(caption, true);
                    }
                    return (char)mnemonic;
                }
            }

            internal int State
            {
                get
                {
                    return state;
                }
            }

            internal bool Visible
            {
                get
                {
                    return (state & MenuItem.STATE_HIDDEN) == 0;
                }
                set
                {
                    if (((state & MenuItem.STATE_HIDDEN) == 0) != value)
                    {
                        state = value ? state & ~MenuItem.STATE_HIDDEN : state | MenuItem.STATE_HIDDEN;
                        ItemsChanged(Menu.CHANGE_VISIBLE);
                    }
                }
            }


            internal object UserData
            {
                get
                {
                    return userData;
                }
                set
                {
                    userData = value;
                }
            }

            internal void AddItem(MenuItem item)
            {
                if (item.data != this)
                {
                    if (item.data != null)
                    {
                        item.data.RemoveItem(item);
                    }

                    item.nextLinkedItem = firstItem;
                    firstItem = item;
                    if (baseItem == null) baseItem = item;
                    item.data = this;
                    item.dataVersion = 0;
                    item.UpdateMenuItem(false);
                }
            }

            public void Execute()
            {
                if (baseItem != null)
                {
                    baseItem.OnClick(EventArgs.Empty);
                }
            }

            internal int GetMenuID()
            {
                if (null == cmd)
                    cmd = new Command(this);
                return cmd.ID;
            }

            internal void ItemsChanged(int change)
            {
                for (MenuItem item = firstItem; item != null; item = item.nextLinkedItem)
                {
                    if (item.menu != null)
                        item.menu.ItemsChanged(change);
                }
            }

            internal void RemoveItem(MenuItem item)
            {
                Debug.Assert(item.data == this, "bad item passed to MenuItemData.removeItem");

                if (item == firstItem)
                {
                    firstItem = item.nextLinkedItem;
                }
                else
                {
                    MenuItem itemT;
                    for (itemT = firstItem; item != itemT.nextLinkedItem;)
                        itemT = itemT.nextLinkedItem;
                    itemT.nextLinkedItem = item.nextLinkedItem;
                }
                item.nextLinkedItem = null;
                item.data = null;
                item.dataVersion = 0;

                if (item == baseItem)
                {
                    baseItem = firstItem;
                }

                if (firstItem == null)
                {
                    // No longer needed. Toss all references and the Command object.
                    Debug.Assert(baseItem == null, "why isn't baseItem null?");
                    onClick = null;
                    onPopup = null;
                    onSelect = null;
                    onDrawItem = null;
                    onMeasureItem = null;
                    if (cmd != null)
                    {
                        cmd.Dispose();
                        cmd = null;
                    }
                }
            }

            internal void SetCaption(string value)
            {
                if (value == null)
                    value = "";
                if (!caption.Equals(value))
                {
                    caption = value;
                    UpdateMenuItems();
                }

#if DEBUG
                if (value.Length > 0)
                {
                    baseItem._debugText = value;
                }
#endif
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal bool HasState(int flag)
            {
                return ((State & flag) == flag);
            }

            internal void SetState(int flag, bool value)
            {
                if (((state & flag) != 0) != value)
                {
                    state = value ? state | flag : state & ~flag;
                    UpdateMenuItems();
                }
            }

            internal void UpdateMenuItems()
            {
                version++;
                for (MenuItem item = firstItem; item != null; item = item.nextLinkedItem)
                {
                    item.UpdateMenuItem(true);
                }
            }

        }

        private class MdiListUserData
        {
            public virtual void OnClick(EventArgs e)
            {
            }
        }

        private class MdiListFormData : MdiListUserData
        {
            private MenuItem parent;
            private int boundIndex;

            public MdiListFormData(MenuItem parentItem, int boundFormIndex)
            {
                boundIndex = boundFormIndex;
                parent = parentItem;
            }

            public override void OnClick(EventArgs e)
            {
                if (boundIndex != -1)
                {
                    // SECREVIEW : User selected a window, that means it is OK 
                    //           : to move focus
                    //
                    IntSecurity.ModifyFocus.Assert();
                    try
                    {
                        Form[] forms = parent.FindMdiForms();
                        Debug.Assert(forms != null, "Didn't get a list of the MDI Forms.");

                        if (forms != null && forms.Length > boundIndex)
                        {
                            Form boundForm = forms[boundIndex];
                            //boundForm.Activate();
                            //if (boundForm.ActiveControl != null && !boundForm.ActiveControl.Focused)
                            //{
                            //    boundForm.ActiveControl.Focus();
                            //}
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
        }

        private class MdiListMoreWindowsData : MdiListUserData
        {

            private MenuItem parent;

            public MdiListMoreWindowsData(MenuItem parent)
            {
                this.parent = parent;
            }

            public override void OnClick(EventArgs e)
            {
                Form[] forms = parent.FindMdiForms();
                Debug.Assert(forms != null, "Didn't get a list of the MDI Forms.");
                Form active = parent.GetMainMenu().GetFormUnsafe().ActiveMdiChild;
                Debug.Assert(active != null, "Didn't get the active MDI child");
                if (forms != null && forms.Length > 0 && active != null)
                {



                    // SECREVIEW : "System" style dialog, no user code will execute, and
                    //           : we don't want the restricted dialog options...
                    //
                    IntSecurity.AllWindows.Assert();
                    try
                    {
                        using (MdiWindowDialog dialog = new MdiWindowDialog())
                        {
                            dialog.SetItems(active, forms);
                            DialogResult result = dialog.ShowDialog();
                            if (result == DialogResult.OK)
                            {

                                // AllWindows Assert above allows this...
                                //
                                //dialog.ActiveChildForm.Activate();
                                //if (dialog.ActiveChildForm.ActiveControl != null && !dialog.ActiveChildForm.ActiveControl.Focused)
                                //{
                                //    dialog.ActiveChildForm.ActiveControl.Focus();
                                //}
                            }
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
        }
    }
}
