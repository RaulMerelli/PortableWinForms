namespace System.Windows.Forms.Layout
{
    using System;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Windows.Forms;
    using System.ComponentModel;


    // Some LayoutEngines extend the same properties to their children.  We want
    // these extended properties to retain their value when moved from one container
    // to another.  (For example, set BoxStretchInternal on a control in FlowPanel and then move
    // the control to GridPanel.)  CommonProperties is a place to define keys and
    // accessors for such properties.
    internal class CommonProperties
    {


        private static readonly int _layoutStateProperty = PropertyStore.CreateKey();
        private static readonly int _specifiedBoundsProperty = PropertyStore.CreateKey();
        private static readonly int _preferredSizeCacheProperty = PropertyStore.CreateKey();
        private static readonly int _paddingProperty = PropertyStore.CreateKey();

        private static readonly int _marginProperty = PropertyStore.CreateKey();
        private static readonly int _minimumSizeProperty = PropertyStore.CreateKey();
        private static readonly int _maximumSizeProperty = PropertyStore.CreateKey();
        private static readonly int _layoutBoundsProperty = PropertyStore.CreateKey();

#if DEBUG
        private static readonly int _lastKnownStateProperty = PropertyStore.CreateKey();

#endif

        internal const ContentAlignment DefaultAlignment = ContentAlignment.TopLeft;
        internal const AnchorStyles DefaultAnchor = AnchorStyles.Top | AnchorStyles.Left;
        internal const bool DefaultAutoSize = false;

        internal const DockStyle DefaultDock = DockStyle.None;
        internal static readonly Padding DefaultMargin = new Padding(3);
        internal static readonly Size DefaultMinimumSize = new Size(0, 0);
        internal static readonly Size DefaultMaximumSize = new Size(0, 0);


        // DO NOT MOVE THE FOLLOWING 4 SECTIONS
        // We have done some special arranging here so that if the first 7 bits of state are zero, we know
        // that the control is purely absolutely positioned and DefaultLayout does not need to do
        // anything.
        //
        private static readonly BitVector32.Section _dockAndAnchorNeedsLayoutSection = BitVector32.CreateSection(0x7F);
        private static readonly BitVector32.Section _dockAndAnchorSection = BitVector32.CreateSection(0x0F);
        private static readonly BitVector32.Section _dockModeSection = BitVector32.CreateSection(0x01, _dockAndAnchorSection);
        private static readonly BitVector32.Section _autoSizeSection = BitVector32.CreateSection(0x01, _dockModeSection);
        private static readonly BitVector32.Section _BoxStretchInternalSection = BitVector32.CreateSection(0x03, _autoSizeSection);
        private static readonly BitVector32.Section _anchorNeverShrinksSection = BitVector32.CreateSection(0x01, _BoxStretchInternalSection);
        private static readonly BitVector32.Section _flowBreakSection = BitVector32.CreateSection(0x01, _anchorNeverShrinksSection);
        private static readonly BitVector32.Section _selfAutoSizingSection = BitVector32.CreateSection(0x01, _flowBreakSection);
        private static readonly BitVector32.Section _autoSizeModeSection = BitVector32.CreateSection(0x01, _selfAutoSizingSection);



        private enum DockAnchorMode
        {
            Anchor = 0,
            Dock = 1
        }

        #region AppliesToAllLayouts

        ///
        internal static void ClearMaximumSize(IArrangedElement element)
        {
            if (element.Properties.ContainsObject(_maximumSizeProperty))
            {
                element.Properties.RemoveObject(_maximumSizeProperty);
            }
        }

        ///
        internal static bool GetAutoSize(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            int value = state[_autoSizeSection];
            return value != 0;
        }

        ///
        internal static Padding GetMargin(IArrangedElement element)
        {
            bool found;
            Padding padding = element.Properties.GetPadding(_marginProperty, out found);
            if (found)
            {
                return padding;
            }
            return DefaultMargin;
        }

        internal static Size GetMaximumSize(IArrangedElement element, Size defaultMaximumSize)
        {
            bool found;
            Size size = element.Properties.GetSize(_maximumSizeProperty, out found);
            if (found)
            {
                return size;
            }
            return defaultMaximumSize;
        }


        internal static Size GetMinimumSize(IArrangedElement element, Size defaultMinimumSize)
        {
            bool found;
            Size size = element.Properties.GetSize(_minimumSizeProperty, out found);
            if (found)
            {
                return size;
            }
            return defaultMinimumSize;
        }

        ///
        internal static Padding GetPadding(IArrangedElement element, Padding defaultPadding)
        {
            bool found;
            Padding padding = element.Properties.GetPadding(_paddingProperty, out found);
            if (found)
            {
                return padding;
            }
            return defaultPadding;
        }

        internal static Rectangle GetSpecifiedBounds(IArrangedElement element)
        {
            bool found;
            Rectangle rectangle = element.Properties.GetRectangle(_specifiedBoundsProperty, out found);
            if (found && rectangle != LayoutUtils.MaxRectangle)
            {
                return rectangle;
            }
            return element.Bounds;
        }

        internal static void ResetPadding(IArrangedElement element)
        {
            object value = element.Properties.GetObject(_paddingProperty);
            if (value != null)
            {
                element.Properties.RemoveObject(_paddingProperty);
            }
        }


        internal static void SetAutoSize(IArrangedElement element, bool value)
        {
            Debug.Assert(value != GetAutoSize(element), "PERF: Caller should guard against setting AutoSize to original value.");

            BitVector32 state = GetLayoutState(element);
            state[_autoSizeSection] = value ? 1 : 0;
            SetLayoutState(element, state);
            if (value == false)
            {
                // If autoSize is being turned off, restore the control to its specified bounds.
                element.SetBounds(GetSpecifiedBounds(element), BoundsSpecified.None);
            }

            Debug.Assert(GetAutoSize(element) == value, "Error detected setting AutoSize.");
        }

        internal static void SetMargin(IArrangedElement element, Padding value)
        {
            Debug.Assert(value != GetMargin(element), "PERF: Caller should guard against setting Margin to original value.");

            element.Properties.SetPadding(_marginProperty, value);

            Debug.Assert(GetMargin(element) == value, "Error detected setting Margin.");

            LayoutTransaction.DoLayout(element.Container, element, PropertyNames.Margin);

        }


        internal static void SetMaximumSize(IArrangedElement element, Size value)
        {
            Debug.Assert(value != GetMaximumSize(element, new Size(-7109, -7107)),
                "PERF: Caller should guard against setting MaximumSize to original value.");

            element.Properties.SetSize(_maximumSizeProperty, value);

            // Element bounds may need to truncated to new maximum
            // 
            Rectangle bounds = element.Bounds;
            bounds.Width = Math.Min(bounds.Width, value.Width);
            bounds.Height = Math.Min(bounds.Height, value.Height);
            element.SetBounds(bounds, BoundsSpecified.Size);

            // element.SetBounds does a SetBoundsCore.  We still need to explicitly refresh parent layout.
            LayoutTransaction.DoLayout(element.Container, element, PropertyNames.MaximumSize);

            Debug.Assert(GetMaximumSize(element, new Size(-7109, -7107)) == value, "Error detected setting MaximumSize.");
        }

        internal static void SetMinimumSize(IArrangedElement element, Size value)
        {
            Debug.Assert(value != GetMinimumSize(element, new Size(-7109, -7107)),
                "PERF: Caller should guard against setting MinimumSize to original value.");

            element.Properties.SetSize(_minimumSizeProperty, value);

            using (new LayoutTransaction(element.Container as Control, element, PropertyNames.MinimumSize))
            {
                // Element bounds may need to inflated to new minimum
                // 
                Rectangle bounds = element.Bounds;
                bounds.Width = Math.Max(bounds.Width, value.Width);
                bounds.Height = Math.Max(bounds.Height, value.Height);
                element.SetBounds(bounds, BoundsSpecified.Size);
            }

            Debug.Assert(GetMinimumSize(element, new Size(-7109, -7107)) == value, "Error detected setting MinimumSize.");
        }


        internal static void SetPadding(IArrangedElement element, Padding value)
        {
            Debug.Assert(value != GetPadding(element, new Padding(-7105)),
                "PERF: Caller should guard against setting Padding to original value.");

            value = LayoutUtils.ClampNegativePaddingToZero(value);
            element.Properties.SetPadding(_paddingProperty, value);


            Debug.Assert(GetPadding(element, new Padding(-7105)) == value, "Error detected setting Padding.");
        }


        ///
        ///
        internal static void UpdateSpecifiedBounds(IArrangedElement element, int x, int y, int width, int height, BoundsSpecified specified)
        {
            Rectangle originalBounds = CommonProperties.GetSpecifiedBounds(element);

            // PERF note: Bitwise operator usage intentional to optimize out branching.

            bool xChangedButNotSpecified = ((specified & BoundsSpecified.X) == BoundsSpecified.None) & x != originalBounds.X;
            bool yChangedButNotSpecified = ((specified & BoundsSpecified.Y) == BoundsSpecified.None) & y != originalBounds.Y;
            bool wChangedButNotSpecified = ((specified & BoundsSpecified.Width) == BoundsSpecified.None) & width != originalBounds.Width;
            bool hChangedButNotSpecified = ((specified & BoundsSpecified.Height) == BoundsSpecified.None) & height != originalBounds.Height;

            if (xChangedButNotSpecified | yChangedButNotSpecified | wChangedButNotSpecified | hChangedButNotSpecified)
            {
                // if any of them are changed and specified cache the new value.

                if (!xChangedButNotSpecified) originalBounds.X = x;
                if (!yChangedButNotSpecified) originalBounds.Y = y;
                if (!wChangedButNotSpecified) originalBounds.Width = width;
                if (!hChangedButNotSpecified) originalBounds.Height = height;

                element.Properties.SetRectangle(_specifiedBoundsProperty, originalBounds);

            }
            else
            {
                // SetBoundsCore is going to call this a lot with the same bounds.  Avoid the set object
                // (which indirectly may causes an allocation) if we can.
                if (element.Properties.ContainsObject(_specifiedBoundsProperty))
                {
                    // use MaxRectangle instead of null so we can reuse the SizeWrapper in the property store.
                    element.Properties.SetRectangle(_specifiedBoundsProperty, LayoutUtils.MaxRectangle);
                }
            }
        }

        // Used by ToolStripControlHost.Size.
        internal static void UpdateSpecifiedBounds(IArrangedElement element, int x, int y, int width, int height)
        {
            Rectangle bounds = new Rectangle(x, y, width, height);
            element.Properties.SetRectangle(_specifiedBoundsProperty, bounds);
        }





        internal static void xClearPreferredSizeCache(IArrangedElement element)
        {
            element.Properties.SetSize(_preferredSizeCacheProperty, LayoutUtils.InvalidSize);
#if DEBUG
            Debug_ClearProperties(element);
#endif

            Debug.Assert(xGetPreferredSizeCache(element) == Size.Empty, "Error detected in xClearPreferredSizeCache.");
        }

        internal static void xClearAllPreferredSizeCaches(IArrangedElement start)
        {
            CommonProperties.xClearPreferredSizeCache(start);

            ArrangedElementCollection controlsCollection = start.Children;
            // This may have changed the sizes of our children.
            // PERFNOTE: This is more efficient than using Foreach.  Foreach
            // forces the creation of an array subset enum each time we
            // enumerate
            for (int i = 0; i < controlsCollection.Count; i++)
            {
                xClearAllPreferredSizeCaches(controlsCollection[i]);
            }
        }

        internal static Size xGetPreferredSizeCache(IArrangedElement element)
        {
            bool found;
            Size size = element.Properties.GetSize(_preferredSizeCacheProperty, out found);
            if (found && (size != LayoutUtils.InvalidSize))
            {
                return size;
            }
            return Size.Empty;
        }

        internal static void xSetPreferredSizeCache(IArrangedElement element, Size value)
        {
            Debug.Assert(value == Size.Empty || value != xGetPreferredSizeCache(element), "PERF: Caller should guard against setting PreferredSizeCache to original value.");
#if DEBUG
            Debug_SnapProperties(element);
#endif
            element.Properties.SetSize(_preferredSizeCacheProperty, value);
            Debug.Assert(xGetPreferredSizeCache(element) == value, "Error detected in xGetPreferredSizeCache.");
        }

        #endregion

        #region DockAndAnchorLayoutSpecific    

        internal static AutoSizeMode GetAutoSizeMode(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            return state[_autoSizeModeSection] == 0 ? AutoSizeMode.GrowOnly : AutoSizeMode.GrowAndShrink;
        }

        internal static bool GetNeedsDockAndAnchorLayout(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            bool result = state[_dockAndAnchorNeedsLayoutSection] != 0;

            Debug.Assert(
                (xGetAnchor(element) == DefaultAnchor
                && xGetDock(element) == DefaultDock
                && GetAutoSize(element) == DefaultAutoSize) != result,
                "Individual values of Anchor/Dock/AutoRelocate/Autosize contradict GetNeedsDockAndAnchorLayout().");

            return result;
        }

        internal static bool GetNeedsAnchorLayout(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            bool result = (state[_dockAndAnchorNeedsLayoutSection] != 0) && (state[_dockModeSection] == (int)DockAnchorMode.Anchor);

            Debug.Assert(
                (xGetAnchor(element) != DefaultAnchor
                || (GetAutoSize(element) != DefaultAutoSize && xGetDock(element) == DockStyle.None)) == result,
                "Individual values of Anchor/Dock/AutoRelocate/Autosize contradict GetNeedsAnchorLayout().");

            return result;
        }

        internal static bool GetNeedsDockLayout(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            bool result = state[_dockModeSection] == (int)DockAnchorMode.Dock && element.ParticipatesInLayout;

            Debug.Assert(((xGetDock(element) != DockStyle.None) && element.ParticipatesInLayout) == result,
                "Error detected in GetNeedsDockLayout().");

            return result;
        }

        internal static bool GetSelfAutoSizeInDefaultLayout(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            int value = state[_selfAutoSizingSection];
            return (value == 1);
        }


        internal static void SetAutoSizeMode(IArrangedElement element, AutoSizeMode mode)
        {
            BitVector32 state = GetLayoutState(element);
            state[_autoSizeModeSection] = mode == AutoSizeMode.GrowAndShrink ? 1 : 0;
            SetLayoutState(element, state);
        }

        internal static bool ShouldSelfSize(IArrangedElement element)
        {
            if (GetAutoSize(element))
            {
                // check for legacy layout engine
                if (element.Container is Control)
                {
                    //if (((Control)element.Container).LayoutEngine is DefaultLayout)
                    //{
                    //    return GetSelfAutoSizeInDefaultLayout(element);
                    //}
                }
                // else
                //   - unknown element type
                //   - new LayoutEngine which should set the size to the preferredSize anyways.
                return false;
            }
            // autosize false things should selfsize.
            return true;
        }

        internal static void SetSelfAutoSizeInDefaultLayout(IArrangedElement element, bool value)
        {
            Debug.Assert(value != GetSelfAutoSizeInDefaultLayout(element), "PERF: Caller should guard against setting AutoSize to original value.");

            BitVector32 state = GetLayoutState(element);
            state[_selfAutoSizingSection] = value ? 1 : 0;
            SetLayoutState(element, state);

            Debug.Assert(GetSelfAutoSizeInDefaultLayout(element) == value, "Error detected setting AutoSize.");
        }


        internal static AnchorStyles xGetAnchor(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            AnchorStyles value = (AnchorStyles)state[_dockAndAnchorSection];
            DockAnchorMode mode = (DockAnchorMode)state[_dockModeSection];

            // If we are docked, or if it the value is 0, we return DefaultAnchor
            value = mode == DockAnchorMode.Anchor ? xTranslateAnchorValue(value) : DefaultAnchor;

            Debug.Assert(mode == DockAnchorMode.Anchor || value == DefaultAnchor, "xGetAnchor needs to return DefaultAnchor when docked.");
            return value;
        }

        internal static bool xGetAutoSizedAndAnchored(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);

            if (state[_selfAutoSizingSection] != 0)
            {
                return false;
            }
            bool result = (state[_autoSizeSection] != 0) && (state[_dockModeSection] == (int)DockAnchorMode.Anchor);
            Debug.Assert(result == (GetAutoSize(element) && xGetDock(element) == DockStyle.None),
                "Error detected in xGetAutoSizeAndAnchored.");

            return result;
        }

        internal static DockStyle xGetDock(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            DockStyle value = (DockStyle)state[_dockAndAnchorSection];
            DockAnchorMode mode = (DockAnchorMode)state[_dockModeSection];

            // If we are anchored we return DefaultDock
            value = mode == DockAnchorMode.Dock ? value : DefaultDock;
            Debug.Assert(ClientUtils.IsEnumValid(value, (int)value, (int)DockStyle.None, (int)DockStyle.Fill), "Illegal value returned form xGetDock.");

            Debug.Assert(mode == DockAnchorMode.Dock || value == DefaultDock,
                "xGetDock needs to return the DefaultDock style when not docked.");

            return value;
        }



        internal static void xSetAnchor(IArrangedElement element, AnchorStyles value)
        {
            Debug.Assert(value != xGetAnchor(element), "PERF: Caller should guard against setting Anchor to original value.");

            BitVector32 state = GetLayoutState(element);

            // We translate DefaultAnchor to zero - see the _dockAndAnchorNeedsLayoutSection section above.
            state[_dockAndAnchorSection] = (int)xTranslateAnchorValue(value);
            state[_dockModeSection] = (int)DockAnchorMode.Anchor;

            SetLayoutState(element, state);

            Debug.Assert(xGetAnchor(element) == value, "Error detected setting Anchor.");
            Debug.Assert(GetLayoutState(element)[_dockModeSection] == (int)DockAnchorMode.Anchor,
                "xSetAnchor did not set mode to Anchor.");
        }

        internal static void xSetDock(IArrangedElement element, DockStyle value)
        {
            Debug.Assert(value != xGetDock(element), "PERF: Caller should guard against setting Dock to original value.");
            Debug.Assert(ClientUtils.IsEnumValid(value, (int)value, (int)DockStyle.None, (int)DockStyle.Fill), "Illegal value passed to xSetDock.");

            BitVector32 state = GetLayoutState(element);

            state[_dockAndAnchorSection] = (int)value;     // See xTranslateAnchorValue for why this works with Dock.None.
            state[_dockModeSection] = (int)(value == DockStyle.None ? DockAnchorMode.Anchor : DockAnchorMode.Dock);

            SetLayoutState(element, state);

            Debug.Assert(xGetDock(element) == value, "Error detected setting Dock.");
            Debug.Assert((GetLayoutState(element)[_dockModeSection] == (int)DockAnchorMode.Dock)
                == (value != DockStyle.None), "xSetDock set DockMode incorrectly.");
        }

        private static AnchorStyles xTranslateAnchorValue(AnchorStyles anchor)
        {
            switch (anchor)
            {
                case AnchorStyles.None:
                    return DefaultAnchor;
                case DefaultAnchor:
                    return AnchorStyles.None;
            }
            return anchor;
        }

        #endregion

        #region FlowLayoutSpecific       
        // 




        internal static bool GetFlowBreak(IArrangedElement element)
        {
            BitVector32 state = GetLayoutState(element);
            int value = state[_flowBreakSection];
            return value == 1;
        }


        internal static void SetFlowBreak(IArrangedElement element, bool value)
        {
            Debug.Assert(value != GetFlowBreak(element), "PERF: Caller should guard against setting FlowBreak to original value.");

            BitVector32 state = GetLayoutState(element);
            state[_flowBreakSection] = value ? 1 : 0;
            SetLayoutState(element, state);

            LayoutTransaction.DoLayout(element.Container, element, PropertyNames.FlowBreak);

            Debug.Assert(GetFlowBreak(element) == value, "Error detected setitng SetFlowBreak.");
        }
        #endregion
        #region AutoScrollSpecific

        ///
        internal static Size GetLayoutBounds(IArrangedElement element)
        {
            bool found;
            Size size = element.Properties.GetSize(_layoutBoundsProperty, out found);
            if (found)
            {
                return size;
            }
            return Size.Empty;
        }


        ///
        internal static void SetLayoutBounds(IArrangedElement element, Size value)
        {
            element.Properties.SetSize(_layoutBoundsProperty, value);
        }


        internal static bool HasLayoutBounds(IArrangedElement element)
        {
            bool found;
            element.Properties.GetSize(_layoutBoundsProperty, out found);
            return found;
        }

        #endregion
        #region InternalCommonPropertiesHelpers

        ///
        internal static BitVector32 GetLayoutState(IArrangedElement element)
        {
            return new BitVector32(element.Properties.GetInteger(_layoutStateProperty));
        }

        internal static void SetLayoutState(IArrangedElement element, BitVector32 state)
        {
            element.Properties.SetInteger(_layoutStateProperty, state.Data);
        }
        #endregion

        #region DebugHelpers
#if DEBUG


        internal static readonly TraceSwitch PreferredSize = new TraceSwitch("PreferredSize", "Debug preferred size assertion");

        internal static string Debug_GetChangedProperties(IArrangedElement element)
        {
            string diff = "";
            if (PreferredSize.TraceVerbose)
            {
                Hashtable propertyHash = element.Properties.GetObject(_lastKnownStateProperty) as Hashtable;
                if (propertyHash != null)
                {
                    foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(element))
                    {
                        if (propertyHash.ContainsKey(pd.Name) && (propertyHash[pd.Name].ToString() != pd.Converter.ConvertToString(pd.GetValue(element))))
                        {
                            diff += "Prop [ " + pd.Name + "] OLD [" + propertyHash[pd.Name] + "] NEW [" + pd.Converter.ConvertToString(pd.GetValue(element)) + "]\r\n";
                        }
                    }
                }
            }
            else
            {
                diff = "For more info, try enabling PreferredSize trace switch";
            }
            return diff;

        }

        internal static void Debug_SnapProperties(IArrangedElement element)
        {
            // DEBUG - store off the old state so we can figure out what has changed in a GPS assert
            element.Properties.SetObject(_lastKnownStateProperty, Debug_GetCurrentPropertyState(element));
        }
        internal static void Debug_ClearProperties(IArrangedElement element)
        {
            // DEBUG - clear off the old state so we can figure out what has changed in a GPS assert
            element.Properties.SetObject(_lastKnownStateProperty, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static Hashtable Debug_GetCurrentPropertyState(object obj)
        {

            Hashtable propertyHash = new Hashtable();
            if (PreferredSize.TraceVerbose)
            {

                foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(obj))
                {
                    if (pd.Name == "PreferredSize")
                    {
                        continue;  // avoid accidentally forcing a call to GetPreferredSize
                    }
                    try
                    {
                        if (pd.IsBrowsable && !pd.IsReadOnly && pd.SerializationVisibility != DesignerSerializationVisibility.Hidden)
                        {
                            propertyHash[pd.Name] = pd.Converter.ConvertToString(pd.GetValue(obj));
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return propertyHash;

        }


#endif
        #endregion
    }
}