﻿namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Windows.Forms.Layout;
    using System.Reflection;

    [TypeConverterAttribute(typeof(TableLayoutSettings.StyleConverter))]
    public abstract class TableLayoutStyle
    {
        private IArrangedElement _owner;
        private SizeType _sizeType = SizeType.AutoSize;
        private float _size;

        [DefaultValue(SizeType.AutoSize)]
        public SizeType SizeType
        {
            get { return _sizeType; }
            set
            {
                if (_sizeType != value)
                {
                    _sizeType = value;
                    if (Owner != null)
                    {
                        LayoutTransaction.DoLayout(Owner, Owner, PropertyNames.Style);
                        Control owner = Owner as Control;
                        if (owner != null)
                        {
                            owner.Invalidate();
                        }
                    }
                }
            }
        }

        internal float Size
        {
            get { return _size; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Size", "InvalidLowBoundArgumentEx");
                }
                if (_size != value)
                {
                    _size = value;
                    if (Owner != null)
                    {
                        LayoutTransaction.DoLayout(Owner, Owner, PropertyNames.Style);
                        Control owner = Owner as Control;
                        if (owner != null)
                        {
                            owner.Invalidate();
                        }
                    }
                }
            }
        }

        private bool ShouldSerializeSize()
        {
            return SizeType != SizeType.AutoSize;
        }

        internal IArrangedElement Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        //set the size without doing a layout
        internal void SetSize(float size)
        {
            Debug.Assert(size >= 0);
            _size = size;
        }
    }
}