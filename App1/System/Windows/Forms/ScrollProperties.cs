namespace System.Windows.Forms
{
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System;
    using System.Security.Permissions;
    using System.Runtime.Serialization.Formatters;
    using System.ComponentModel;
    using System.Drawing;
    using Microsoft.Win32;
    using System.Windows.Forms;
    using System.Globalization;

    public abstract class ScrollProperties
    {

        internal int minimum = 0;
        internal int maximum = 100;
        internal int smallChange = 1;
        internal int largeChange = 10;
        internal int value = 0;
        internal bool maximumSetExternally;
        internal bool smallChangeSetExternally;
        internal bool largeChangeSetExternally;

        private ScrollableControl parent;

        protected ScrollableControl ParentControl
        {
            get
            {
                return parent;
            }
        }

        private const int SCROLL_LINE = 5;

        internal bool visible = false;

        //Always Enabled
        private bool enabled = true;


        protected ScrollProperties(ScrollableControl container)
        {
            this.parent = container;
        }

        [        DefaultValue(true),        ]
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (parent.AutoScroll)
                {
                    return;
                }
                if (value != enabled)
                {
                    enabled = value;
                    EnableScroll(value);
                }
            }
        }


        [        DefaultValue(10),        RefreshProperties(RefreshProperties.Repaint)
        ]
        public int LargeChange
        {
            get
            {
                // We preserve the actual large change value that has been set, but when we come to
                // get the value of this property, make sure it's within the maximum allowable value.
                // This way we ensure that we don't depend on the order of property sets when
                // code is generated at design-time.
                //
                return Math.Min(largeChange, maximum - minimum + 1);
            }
            set
            {
                if (largeChange != value)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("LargeChange", "InvalidLowBoundArgumentEx");
                    }
                    largeChange = value;
                    largeChangeSetExternally = true;
                    UpdateScrollInfo();
                }
            }
        }

        [        DefaultValue(100),        RefreshProperties(RefreshProperties.Repaint)
        ]
        public int Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                if (parent.AutoScroll)
                {
                    return;
                }
                if (maximum != value)
                {
                    if (minimum > value)
                        minimum = value;
                    // bring this.value in line.
                    if (value < this.value)
                        Value = value;
                    maximum = value;
                    maximumSetExternally = true;
                    UpdateScrollInfo();
                }
            }
        }

        [        DefaultValue(0),        RefreshProperties(RefreshProperties.Repaint)
        ]
        public int Minimum
        {
            get
            {
                return minimum;
            }

            set
            {
                if (parent.AutoScroll)
                {
                    return;
                }
                if (minimum != value)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("Minimum", "InvalidLowBoundArgumentEx");
                    }
                    if (maximum < value)
                        maximum = value;
                    // bring this.value in line.
                    if (value > this.value)
                        this.value = value;
                    minimum = value;
                    UpdateScrollInfo();
                }
            }
        }

        internal abstract int PageSize
        {
            get;
        }

        internal abstract int Orientation
        {
            get;
        }

        internal abstract int HorizontalDisplayPosition
        {
            get;
        }


        internal abstract int VerticalDisplayPosition
        {
            get;
        }

        [        DefaultValue(1),        ]
        public int SmallChange
        {
            get
            {
                // We can't have SmallChange > LargeChange, but we shouldn't manipulate
                // the set values for these properties, so we just return the smaller 
                // value here. 
                //
                return Math.Min(smallChange, LargeChange);
            }
            set
            {
                if (smallChange != value)
                {

                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("SmallChange", "InvalidLowBoundArgumentEx");
                    }

                    smallChange = value;
                    smallChangeSetExternally = true;
                    UpdateScrollInfo();
                }
            }
        }

        [        DefaultValue(0),
        Bindable(true),        ]
        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                if (this.value != value)
                {
                    if (value < minimum || value > maximum)
                    {
                        throw new ArgumentOutOfRangeException("Value", "InvalidBoundArgument");
                    }
                    this.value = value;
                    UpdateScrollInfo();
                    parent.SetDisplayFromScrollProps(HorizontalDisplayPosition, VerticalDisplayPosition);
                }
            }
        }


        [        DefaultValue(false),        ]
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (parent.AutoScroll)
                {
                    return;
                }
                if (value != visible)
                {
                    visible = value;
                    //parent.UpdateStylesCore();
                    UpdateScrollInfo();
                    parent.SetDisplayFromScrollProps(HorizontalDisplayPosition, VerticalDisplayPosition);
                }
            }
        }

        internal void UpdateScrollInfo()
        {
            if (parent.IsHandleCreated && visible)
            {
                NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO();
                si.cbSize = Marshal.SizeOf(typeof(NativeMethods.SCROLLINFO));
                si.fMask = NativeMethods.SIF_ALL;
                si.nMin = minimum;
                si.nMax = maximum;
                si.nPage = (parent.AutoScroll) ? PageSize : LargeChange;
                si.nPos = value;
                si.nTrackPos = 0;
                //UnsafeNativeMethods.SetScrollInfo(new HandleRef(parent, parent.Handle), Orientation, si, true);
            }
        }

        private void EnableScroll(bool enable)
        {
            if (enable)
            {
                //UnsafeNativeMethods.EnableScrollBar(new HandleRef(parent, parent.Handle), Orientation, NativeMethods.ESB_ENABLE_BOTH);
            }
            else
            {
                //UnsafeNativeMethods.EnableScrollBar(new HandleRef(parent, parent.Handle), Orientation, NativeMethods.ESB_DISABLE_BOTH);
            }

        }

    }
}
