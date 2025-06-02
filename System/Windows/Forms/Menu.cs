namespace System.Windows.Forms
{
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.Drawing;
    using Microsoft.Win32;
    using System.Security;
    using System.Globalization;
    using System.Runtime.Versioning;

    [ToolboxItemFilter("System.Windows.Forms"), ListBindable(false)]
    public abstract class Menu : Component
    {
        internal const int CHANGE_ITEMS = 0; // item(s) added or removed
        internal const int CHANGE_VISIBLE = 1; // item(s) hidden or shown
        internal const int CHANGE_MDI = 2; // mdi item changed
        internal const int CHANGE_MERGE = 3; // mergeType or mergeOrder changed
        internal const int CHANGE_ITEMADDED = 4; // mergeType or mergeOrder changed

        public const int FindHandle = 0;
        public const int FindShortcut = 1;

        private MenuItemCollection itemsCollection;
        internal MenuItem[] items;
        private int _itemCount;
        internal IntPtr handle;
        internal bool created;
        private object userData;
        private string name;

        protected Menu(MenuItem[] items)
        {
            if (items != null)
            {
                MenuItems.AddRange(items);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr Handle
        {
            get
            {
                if (handle == IntPtr.Zero) handle = CreateMenuHandle();
                CreateMenuItems();
                return handle;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool IsParent
        {
            [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return null != items && ItemCount > 0;
            }
        }

        internal int ItemCount
        {
            get
            {
                return _itemCount;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MenuItem MdiListItem
        {
            get
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    MenuItem item = items[i];
                    if (item.MdiList)
                        return item;
                    if (item.IsParent)
                    {
                        item = item.MdiListItem;
                        if (item != null) return item;
                    }
                }
                return null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string Name
        {
            get
            {
                return WindowsFormsUtils.GetComponentName(this, name);
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    name = null;
                }
                else
                {
                    name = value;
                }
                if (Site != null)
                {
                    Site.Name = name;
                }
            }
        }


        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), MergableProperty(false)]
        public MenuItemCollection MenuItems
        {
            get
            {
                if (itemsCollection == null)
                {
                    itemsCollection = new MenuItemCollection(this);
                }
                return itemsCollection;
            }
        }

        internal virtual bool RenderIsRightToLeft
        {
            get
            {
                Debug.Assert(true, "Should never get called");
                return false;

            }
        }


        [Localizable(false), Bindable(true), DefaultValue(null), TypeConverter(typeof(StringConverter))]
        public object Tag
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

        internal void ClearHandles()
        {
            if (handle != IntPtr.Zero)
            {
                //UnsafeNativeMethods.DestroyMenu(new HandleRef(this, handle));
            }
            handle = IntPtr.Zero;
            if (created)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    items[i].ClearHandles();
                }
                created = false;
            }
        }

        [
            SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly") // Shipped as is in Everett
        ]
        protected internal void CloneMenu(Menu menuSrc)
        {
            MenuItem[] newItems = null;
            if (menuSrc.items != null)
            {
                int count = menuSrc.MenuItems.Count;
                newItems = new MenuItem[count];
                for (int i = 0; i < count; i++)
                    newItems[i] = menuSrc.MenuItems[i].CloneMenu();
            }
            MenuItems.Clear();
            if (newItems != null)
            {
                MenuItems.AddRange(newItems);
            }
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        protected virtual IntPtr CreateMenuHandle()
        {
            //return UnsafeNativeMethods.CreatePopupMenu();
            return IntPtr.Zero;
        }

        internal void CreateMenuItems()
        {
            if (!created)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    items[i].CreateMenuItem();
                }
                created = true;
            }
        }

        internal void DestroyMenuItems()
        {
            if (created)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    items[i].ClearHandles();
                }
                //while (UnsafeNativeMethods.GetMenuItemCount(new HandleRef(this, handle)) > 0)
                //{
                //    UnsafeNativeMethods.RemoveMenu(new HandleRef(this, handle), 0, NativeMethods.MF_BYPOSITION);
                //}
                created = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (ItemCount > 0)
                {
                    MenuItem item = items[--_itemCount];

                    // remove the item before we dispose it so it still has valid state
                    // for undo/redo
                    //
                    if (item.Site != null && item.Site.Container != null)
                    {
                        item.Site.Container.Remove(item);
                    }

                    item.Menu = null;
                    item.Dispose();
                }
                items = null;
            }
            if (handle != IntPtr.Zero)
            {
                //UnsafeNativeMethods.DestroyMenu(new HandleRef(this, handle));
                this.handle = IntPtr.Zero;
                if (disposing)
                {
                    ClearHandles();
                }
            }
            base.Dispose(disposing);
        }

        public MenuItem FindMenuItem(int type, IntPtr value)
        {
            Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "ControlFromHandleOrLocation Demanded");
            IntSecurity.ControlFromHandleOrLocation.Demand();
            return FindMenuItemInternal(type, value);
        }

        private MenuItem FindMenuItemInternal(int type, IntPtr value)
        {
            for (int i = 0; i < ItemCount; i++)
            {
                MenuItem item = items[i];
                switch (type)
                {
                    case FindHandle:
                        if (item.handle == value) return item;
                        break;
                    case FindShortcut:
                        if (item.Shortcut == (Shortcut)(int)value) return item;
                        break;
                }
                item = item.FindMenuItemInternal(type, value);
                if (item != null) return item;
            }
            return null;
        }

        protected int FindMergePosition(int mergeOrder)
        {
            int iMin, iLim, iT;

            for (iMin = 0, iLim = ItemCount; iMin < iLim;)
            {
                iT = (iMin + iLim) / 2;
                if (items[iT].MergeOrder <= mergeOrder)
                    iMin = iT + 1;
                else
                    iLim = iT;
            }
            return iMin;
        }

        // VSWhidbey 94987: A new method for finding the approximate merge position. The original
        // method assumed (incorrectly) that the MergeOrder of the target menu would be sequential
        // as it's guaranteed to be in the MDI imlementation of merging container and child
        // menus. However, user code can call MergeMenu independently on a source and target
        // menu whose MergeOrder values are not necessarily pre-sorted.
        internal int xFindMergePosition(int mergeOrder)
        {
            int nPosition = 0;

            // Iterate from beginning to end since we can't assume any sequential ordering to MergeOrder
            for (int nLoop = 0; nLoop < ItemCount; nLoop++)
            {

                if (items[nLoop].MergeOrder > mergeOrder)
                {
                    // We didn't find what we're looking for, but we've found a stopping point.
                    break;
                }
                else if (items[nLoop].MergeOrder < mergeOrder)
                {
                    // We might have found what we're looking for, but we'll have to come around again
                    // to know.
                    nPosition = nLoop + 1;
                }
                else if (mergeOrder == items[nLoop].MergeOrder)
                {
                    // We've found what we're looking for, so use this value for the merge order
                    nPosition = nLoop;
                    break;
                }
            }

            return nPosition;
        }

        //There's a win32 problem that doesn't allow menus to cascade right to left
        //unless we explicitely set the bit on the menu the first time it pops up
        internal void UpdateRtl(bool setRightToLeftBit)
        {
            foreach (MenuItem item in MenuItems)
            {
                item.UpdateItemRtl(setRightToLeftBit);
                item.UpdateRtl(setRightToLeftBit);
            }
        }

        public ContextMenu GetContextMenu()
        {
            Menu menuT;
            for (menuT = this; !(menuT is ContextMenu);)
            {
                if (!(menuT is MenuItem)) return null;
                menuT = ((MenuItem)menuT).Menu;
            }
            return (ContextMenu)menuT;

        }

        public MainMenu GetMainMenu()
        {
            Menu menuT;
            for (menuT = this; !(menuT is MainMenu);)
            {
                if (!(menuT is MenuItem)) return null;
                menuT = ((MenuItem)menuT).Menu;
            }
            return (MainMenu)menuT;
        }

        internal virtual void ItemsChanged(int change)
        {
            switch (change)
            {
                case CHANGE_ITEMS:
                case CHANGE_VISIBLE:
                    DestroyMenuItems();
                    break;
            }
        }

        private IntPtr MatchKeyToMenuItem(int startItem, char key, MenuItemKeyComparer comparer)
        {
            int firstMatch = -1;
            bool multipleMatches = false;

            for (int i = 0; i < items.Length && !multipleMatches; ++i)
            {
                int itemIndex = (startItem + i) % items.Length;
                MenuItem mi = items[itemIndex];
                if (mi != null && comparer(mi, key))
                {
                    if (firstMatch < 0)
                    {
                        // VSWhidbey 218021 using Index doesnt respect hidden items.
                        firstMatch = mi.MenuIndex;
                    }
                    else
                    {
                        multipleMatches = true;
                    }
                }
            }

            if (firstMatch < 0)
                return IntPtr.Zero;

            int action = multipleMatches ? NativeMethods.MNC_SELECT : NativeMethods.MNC_EXECUTE;
            return (IntPtr)NativeMethods.Util.MAKELONG(firstMatch, action);
        }

        private delegate bool MenuItemKeyComparer(MenuItem mi, char key);

        [
            SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly") // Shipped as is in Everett
        ]
        public virtual void MergeMenu(Menu menuSrc)
        {
            int i, j;
            MenuItem item;
            MenuItem itemDst;

            if (menuSrc == this)
                throw new ArgumentException("MenuMergeWithSelf");

            if (menuSrc.items != null && items == null)
            {
                MenuItems.Clear();
            }

            for (i = 0; i < menuSrc.ItemCount; i++)
            {
                item = menuSrc.items[i];

                switch (item.MergeType)
                {
                    default:
                        continue;
                    case MenuMerge.Add:
                        MenuItems.Add(FindMergePosition(item.MergeOrder), item.MergeMenu());
                        continue;
                    case MenuMerge.Replace:
                    case MenuMerge.MergeItems:
                        break;
                }

                int mergeOrder = item.MergeOrder;
                // Can we find a menu item with a matching merge order?
                // VSWhidbey 94987: Use new method to find the approximate merge position. The original
                // method assumed (incorrectly) that the MergeOrder of the target menu would be sequential
                // as it's guaranteed to be in the MDI imlementation of merging container and child
                // menus. However, user code can call MergeMenu independently on a source and target
                // menu whose MergeOrder values are not necessarily pre-sorted.
                for (j = xFindMergePosition(mergeOrder); ; j++)
                {

                    if (j >= ItemCount)
                    {
                        // A matching merge position could not be found,
                        // so simply append this menu item to the end.
                        MenuItems.Add(j, item.MergeMenu());
                        break;
                    }
                    itemDst = items[j];
                    if (itemDst.MergeOrder != mergeOrder)
                    {
                        MenuItems.Add(j, item.MergeMenu());
                        break;
                    }
                    if (itemDst.MergeType != MenuMerge.Add)
                    {
                        if (item.MergeType != MenuMerge.MergeItems
                            || itemDst.MergeType != MenuMerge.MergeItems)
                        {
                            itemDst.Dispose();
                            MenuItems.Add(j, item.MergeMenu());
                        }
                        else
                        {
                            itemDst.MergeMenu(item);
                        }
                        break;
                    }
                }
            }
        }

        internal virtual bool ProcessInitMenuPopup(IntPtr handle)
        {
            MenuItem item = FindMenuItemInternal(FindHandle, handle);
            if (item != null)
            {
                item._OnInitMenuPopup(EventArgs.Empty);
                item.CreateMenuItems();
                return true;
            }
            return false;
        }

        [
            System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode),
            System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        protected internal virtual bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            MenuItem item = FindMenuItemInternal(FindShortcut, (IntPtr)(int)keyData);
            return item != null ? item.ShortcutClick() : false;
        }

        internal int SelectedMenuItemIndex
        {
            get
            {
                for (int i = 0; i < items.Length; ++i)
                {
                    MenuItem mi = items[i];
                    if (mi != null && mi.Selected)
                        return i;
                }
                return -1;
            }
        }


        public override string ToString()
        {

            string s = base.ToString();
            return s + ", Items.Count: " + ItemCount.ToString(CultureInfo.CurrentCulture);
        }

        internal void WmMenuChar(ref Message m)
        {
            Menu menu = (m.LParam == handle) ? this : FindMenuItemInternal(FindHandle, m.LParam);

            if (menu == null)
                return;

            char menuKey = Char.ToUpper((char)NativeMethods.Util.LOWORD(m.WParam), CultureInfo.CurrentCulture);

            m.Result = menu.WmMenuCharInternal(menuKey);
        }

        internal IntPtr WmMenuCharInternal(char key)
        {
            // Start looking just beyond the current selected item (otherwise just start at the top)
            int startItem = (SelectedMenuItemIndex + 1) % items.Length;

            // First, search for match among owner-draw items with explicitly defined access keys (eg. "S&ave")
            IntPtr result = MatchKeyToMenuItem(startItem, key, new MenuItemKeyComparer(CheckOwnerDrawItemWithMnemonic));

            // Next, search for match among owner-draw items with no access keys (looking at first char of item text)
            if (result == IntPtr.Zero)
                result = MatchKeyToMenuItem(startItem, key, new MenuItemKeyComparer(CheckOwnerDrawItemNoMnemonic));

            return result;
        }

        private bool CheckOwnerDrawItemWithMnemonic(MenuItem mi, char key)
        {
            return mi.OwnerDraw &&
                   mi.Mnemonic == key;
        }

        private bool CheckOwnerDrawItemNoMnemonic(MenuItem mi, char key)
        {
            return mi.OwnerDraw &&
                   mi.Mnemonic == 0 &&
                   mi.Text.Length > 0 &&
                   Char.ToUpper(mi.Text[0], CultureInfo.CurrentCulture) == key;
        }

        [ListBindable(false)]
        public class MenuItemCollection : IList
        {
            private Menu owner;

            private int lastAccessedIndex = -1;

            public MenuItemCollection(Menu owner)
            {
                this.owner = owner;
            }

            public virtual MenuItem this[int index]
            {
                get
                {
                    if (index < 0 || index >= owner.ItemCount)
                        throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                    return owner.items[index];
                }
                // set not supported
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            public virtual MenuItem this[string key]
            {
                get
                {
                    // We do not support null and empty string as valid keys.
                    if (string.IsNullOrEmpty(key))
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

            public int Count
            {
                get
                {
                    return owner.ItemCount;
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


            public virtual MenuItem Add(string caption)
            {
                MenuItem item = new MenuItem(caption);
                Add(item);
                return item;
            }

            public virtual MenuItem Add(string caption, EventHandler onClick)
            {
                MenuItem item = new MenuItem(caption, onClick);
                Add(item);
                return item;
            }

            public virtual MenuItem Add(string caption, MenuItem[] items)
            {
                MenuItem item = new MenuItem(caption, items);
                Add(item);
                return item;
            }

            public virtual int Add(MenuItem item)
            {
                return Add(owner.ItemCount, item);
            }

            public virtual int Add(int index, MenuItem item)
            {

                // MenuItems can only belong to one menu at a time
                if (item.Menu != null)
                {

                    // First check that we're not adding ourself, i.e. walk
                    // the parent chain for equality
                    if (owner is MenuItem)
                    {
                        MenuItem parent = (MenuItem)owner;
                        while (parent != null)
                        {
                            if (parent.Equals(item))
                            {
                                throw new ArgumentException("MenuItemAlreadyExists");
                            }
                            if (parent.Parent is MenuItem)
                                parent = (MenuItem)parent.Parent;
                            else
                                break;
                        }
                    }

                    //if we're re-adding an item back to the same collection
                    //the target index needs to be decremented since we're
                    //removing an item from the collection
                    if (item.Menu.Equals(owner) && index > 0)
                    {
                        index--;
                    }

                    item.Menu.MenuItems.Remove(item);
                }

                // Validate our index
                if (index < 0 || index > owner.ItemCount)
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                if (owner.items == null || owner.items.Length == owner.ItemCount)
                {
                    MenuItem[] newItems = new MenuItem[owner.ItemCount < 2 ? 4 : owner.ItemCount * 2];
                    if (owner.ItemCount > 0) System.Array.Copy(owner.items, 0, newItems, 0, owner.ItemCount);
                    owner.items = newItems;
                }
                System.Array.Copy(owner.items, index, owner.items, index + 1, owner.ItemCount - index);
                owner.items[index] = item;
                owner._itemCount++;
                item.Menu = owner;
                owner.ItemsChanged(CHANGE_ITEMS);
                if (owner is MenuItem)
                {
                    ((MenuItem)owner).ItemsChanged(CHANGE_ITEMADDED, item);
                }

                return index;
            }

            public virtual void AddRange(MenuItem[] items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                foreach (MenuItem item in items)
                {
                    Add(item);
                }
            }

            int IList.Add(object value)
            {
                if (value is MenuItem)
                {
                    return Add((MenuItem)value);
                }
                else
                {
                    throw new ArgumentException("MenuBadMenuItem");
                }
            }

            public bool Contains(MenuItem value)
            {
                return IndexOf(value) != -1;
            }

            bool IList.Contains(object value)
            {
                if (value is MenuItem)
                {
                    return Contains((MenuItem)value);
                }
                else
                {
                    return false;
                }
            }

            public virtual bool ContainsKey(string key)
            {
                return IsValidIndex(IndexOfKey(key));
            }

            public MenuItem[] Find(string key, bool searchAllChildren)
            {

                if ((key == null) || (key.Length == 0))
                {
                    throw new System.ArgumentNullException("key", "FindKeyMayNotBeEmptyOrNull");
                }


                ArrayList foundMenuItems = FindInternal(key, searchAllChildren, this, new ArrayList());

                // Make this a stongly typed collection.
                MenuItem[] stronglyTypedfoundMenuItems = new MenuItem[foundMenuItems.Count];
                foundMenuItems.CopyTo(stronglyTypedfoundMenuItems, 0);

                return stronglyTypedfoundMenuItems;
            }

            private ArrayList FindInternal(string key, bool searchAllChildren, MenuItemCollection menuItemsToLookIn, ArrayList foundMenuItems)
            {
                if ((menuItemsToLookIn == null) || (foundMenuItems == null))
                {
                    return null;  // 
                }

                // Perform breadth first search - as it's likely people will want controls belonging
                // to the same parent close to each other.

                for (int i = 0; i < menuItemsToLookIn.Count; i++)
                {
                    if (menuItemsToLookIn[i] == null)
                    {
                        continue;
                    }

                    if (WindowsFormsUtils.SafeCompareStrings(menuItemsToLookIn[i].Name, key, /* ignoreCase = */ true))
                    {
                        foundMenuItems.Add(menuItemsToLookIn[i]);
                    }
                }

                // Optional recurive search for controls in child collections.

                if (searchAllChildren)
                {
                    for (int i = 0; i < menuItemsToLookIn.Count; i++)
                    {
                        if (menuItemsToLookIn[i] == null)
                        {
                            continue;
                        }
                        if ((menuItemsToLookIn[i].MenuItems != null) && menuItemsToLookIn[i].MenuItems.Count > 0)
                        {
                            // if it has a valid child collecion, append those results to our collection
                            foundMenuItems = FindInternal(key, searchAllChildren, menuItemsToLookIn[i].MenuItems, foundMenuItems);
                        }
                    }
                }
                return foundMenuItems;
            }

            public int IndexOf(MenuItem value)
            {
                for (int index = 0; index < Count; ++index)
                {
                    if (this[index] == value)
                    {
                        return index;
                    }
                }
                return -1;
            }

            int IList.IndexOf(object value)
            {
                if (value is MenuItem)
                {
                    return IndexOf((MenuItem)value);
                }
                else
                {
                    return -1;
                }
            }

            public virtual int IndexOfKey(String key)
            {
                // Step 0 - Arg validation
                if (string.IsNullOrEmpty(key))
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

            void IList.Insert(int index, object value)
            {
                if (value is MenuItem)
                {
                    Add(index, (MenuItem)value);
                }
                else
                {
                    throw new ArgumentException("MenuBadMenuItem");
                }
            }


            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }


            public virtual void Clear()
            {
                if (owner.ItemCount > 0)
                {

                    for (int i = 0; i < owner.ItemCount; i++)
                    {
                        owner.items[i].Menu = null;
                    }

                    owner._itemCount = 0;
                    owner.items = null;

                    owner.ItemsChanged(CHANGE_ITEMS);

                    if (owner is MenuItem)
                    {
                        ((MenuItem)(owner)).UpdateMenuItem(true);
                    }
                }
            }

            public void CopyTo(Array dest, int index)
            {
                if (owner.ItemCount > 0)
                {
                    System.Array.Copy(owner.items, 0, dest, index, owner.ItemCount);
                }
            }

            public IEnumerator GetEnumerator()
            {
                return new WindowsFormsUtils.ArraySubsetEnumerator(owner.items, owner.ItemCount);
            }

            public virtual void RemoveAt(int index)
            {
                if (index < 0 || index >= owner.ItemCount)
                {
                    throw new ArgumentOutOfRangeException("index", "InvalidArgument");
                }

                MenuItem item = owner.items[index];
                item.Menu = null;
                owner._itemCount--;
                System.Array.Copy(owner.items, index + 1, owner.items, index, owner.ItemCount - index);
                owner.items[owner.ItemCount] = null;
                owner.ItemsChanged(CHANGE_ITEMS);

                //if the last item was removed, clear the collection
                //
                if (owner.ItemCount == 0)
                {
                    Clear();
                }

            }

            public virtual void RemoveByKey(string key)
            {
                int index = IndexOfKey(key);
                if (IsValidIndex(index))
                {
                    RemoveAt(index);
                }
            }

            public virtual void Remove(MenuItem item)
            {
                if (item.Menu == owner)
                {
                    RemoveAt(item.Index);
                }
            }

            void IList.Remove(object value)
            {
                if (value is MenuItem)
                {
                    Remove((MenuItem)value);
                }
            }
        }
    }
}