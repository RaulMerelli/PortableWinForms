using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms.Layout;
using System.Runtime.Serialization.Formatters;
using System.Diagnostics;
using System;
using Microsoft.Win32;
#if WINDOWS_UWP
using App1;
#else
using AndroidWinForms;
#endif

namespace System.Windows.Forms
{
    [
    ComVisible(true),
    ClassInterface(ClassInterfaceType.AutoDispatch),
    DefaultProperty("BorderStyle"),
    DefaultEvent("Paint"),
    ]
    public class Panel : ScrollableControl
    {

        private BorderStyle borderStyle = System.Windows.Forms.BorderStyle.None;

        public Panel()
        : base()
        {
            // this class overrides GetPreferredSizeCore, let Control automatically cache the result
            SetState2(STATE2_USEPREFERREDSIZECACHE, true);
            TabStop = false;
            SetStyle(ControlStyles.Selectable |
                     ControlStyles.AllPaintingInWmPaint, false);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        new public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        [
        Browsable(true),
        DefaultValue(AutoSizeMode.GrowOnly),
        Localizable(true)
        ]
        public virtual AutoSizeMode AutoSizeMode
        {
            get
            {
                return GetAutoSizeMode();
            }
            set
            {
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)AutoSizeMode.GrowAndShrink, (int)AutoSizeMode.GrowOnly))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(AutoSizeMode));
                }
                if (GetAutoSizeMode() != value)
                {
                    SetAutoSizeMode(value);
                    if (ParentInternal != null)
                    {
                        // DefaultLayout does not keep anchor information until it needs to.  When
                        // AutoSize became a common property, we could no longer blindly call into
                        // DefaultLayout, so now we do a special InitLayout just for DefaultLayout.
                        if (ParentInternal.LayoutEngine == DefaultLayout.Instance)
                        {
                            ParentInternal.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
                        }
                        LayoutTransaction.DoLayout(ParentInternal, this, PropertyNames.AutoSize);
                    }
                }
            }
        }

        [
        DefaultValue(BorderStyle.None),
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
                if (borderStyle != value)
                {
                    //valid values are 0x0 to 0x2
                    if (!ClientUtils.IsEnumValid(value, (int)value, (int)BorderStyle.None, (int)BorderStyle.Fixed3D))
                    {
                        throw new InvalidEnumArgumentException("value", (int)value, typeof(BorderStyle));
                    }

                    borderStyle = value;
                    UpdateStyles();
                }
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= NativeMethods.WS_EX_CONTROLPARENT;

        //        cp.ExStyle &= (~NativeMethods.WS_EX_CLIENTEDGE);
        //        cp.Style &= (~NativeMethods.WS_BORDER);

        //        switch (borderStyle)
        //        {
        //            case BorderStyle.Fixed3D:
        //                cp.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;
        //                break;
        //            case BorderStyle.FixedSingle:
        //                cp.Style |= NativeMethods.WS_BORDER;
        //                break;
        //        }
        //        return cp;
        //    }
        //}

        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, 100);
            }
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            // Translating 0,0 from ClientSize to actual Size tells us how much space
            // is required for the borders.
            Size borderSize = SizeFromClientSize(Size.Empty);
            Size totalPadding = borderSize + Padding.Size;

            return LayoutEngine.GetPreferredSize(this, proposedSize - totalPadding) + totalPadding;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event KeyEventHandler KeyUp
        {
            add
            {
                base.KeyUp += value;
            }
            remove
            {
                base.KeyUp -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event KeyEventHandler KeyDown
        {
            add
            {
                base.KeyDown += value;
            }
            remove
            {
                base.KeyDown -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event KeyPressEventHandler KeyPress
        {
            add
            {
                base.KeyPress += value;
            }
            remove
            {
                base.KeyPress -= value;
            }
        }

        [DefaultValue(false)]
        new public bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        internal override void OnResize(EventArgs eventargs)
        {
            if (DesignMode && borderStyle == BorderStyle.None)
            {
                Invalidate();
            }
            base.OnResize(eventargs);
        }


        //internal override void PrintToMetaFileRecursive(HandleRef hDC, IntPtr lParam, Rectangle bounds)
        //{
        //    base.PrintToMetaFileRecursive(hDC, lParam, bounds);

        //    using (WindowsFormsUtils.DCMapping mapping = new WindowsFormsUtils.DCMapping(hDC, bounds))
        //    {
        //        using (Graphics g = Graphics.FromHdcInternal(hDC.Handle))
        //        {
        //            ControlPaint.PrintBorder(g, new Rectangle(Point.Empty, Size), BorderStyle, Border3DStyle.Sunken);
        //        }
        //    }
        //}

        private static string StringFromBorderStyle(BorderStyle value)
        {
            Type borderStyleType = typeof(BorderStyle);
            return (ClientUtils.IsEnumValid(value, (int)value, (int)BorderStyle.None, (int)BorderStyle.Fixed3D)) ? (borderStyleType.ToString() + "." + value.ToString()) : "[Invalid BorderStyle]";
        }

        public override string ToString()
        {

            string s = base.ToString();
            return s + ", BorderStyle: " + StringFromBorderStyle(borderStyle);
        }

        public async override void PerformLayout()
        {
            string style = "";
            string styleclass = "";
            string script = "";

            switch (borderStyle)
            {
                case BorderStyle.Fixed3D:
                    styleclass = "panelframe-3d";
                    break;
                case BorderStyle.FixedSingle:
                    styleclass = "panelframe-single";
                    break;
                case BorderStyle.None:
                    styleclass = "panelframe-none";
                    break;
            }

            WebviewIdentifier += Name;

            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"margin: 0px;";
            style += $"padding: 0px;";
            style += $"background-color: {System.Drawing.ColorTranslator.ToHtml(BackColor)};";
            style += CssLocationAndSize();

            script += preLayoutScriptString;

            await Page.Add(Parent.WebviewIdentifier, "innerHTML", $"'<div id=\"{WebviewIdentifier}\" style=\"{style}\" class=\"{styleclass}\"></div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
