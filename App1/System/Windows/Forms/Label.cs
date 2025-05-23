using App1;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
    public class Label : Control
    {
        private static readonly object EVENT_TEXTALIGNCHANGED = new object();

        private static readonly BitVector32.Section StateUseMnemonic = BitVector32.CreateSection(1);
        private static readonly BitVector32.Section StateAutoSize = BitVector32.CreateSection(1, StateUseMnemonic);
        private static readonly BitVector32.Section StateAnimating = BitVector32.CreateSection(1, StateAutoSize);
        private static readonly BitVector32.Section StateFlatStyle = BitVector32.CreateSection((int)FlatStyle.System, StateAnimating);
        private static readonly BitVector32.Section StateBorderStyle = BitVector32.CreateSection((int)BorderStyle.Fixed3D, StateFlatStyle);
        private static readonly BitVector32.Section StateAutoEllipsis = BitVector32.CreateSection(1, StateBorderStyle);

        private static readonly int PropImageList = PropertyStore.CreateKey();
        private static readonly int PropImage = PropertyStore.CreateKey();

        private static readonly int PropTextAlign = PropertyStore.CreateKey();
        private static readonly int PropImageAlign = PropertyStore.CreateKey();
        private static readonly int PropImageIndex = PropertyStore.CreateKey();

        ///////////////////////////////////////////////////////////////////////
        // Label per instance members
        //
        // Note: Do not add anything to this list unless absolutely neccessary.
        //
        // Begin Members {

        // List of properties that are generally set, so we keep them directly on
        // Label.
        //

        BitVector32 labelState = new BitVector32();
        int requestedHeight;
        int requestedWidth;
        LayoutUtils.MeasureTextCache textMeasurementCache;

        //Tooltip is shown only if the Text in the Label is cut.
        internal bool showToolTip = false;
        //ToolTip textToolTip;
        // This bool suggests that the User has added a toolTip to this label
        // In such a case we should not show the AutoEllipsis tooltip.
        bool controlToolTip = false;
        //AutomationLiveSetting liveSetting;

        public Label()
        : base()
        {
            // this class overrides GetPreferredSizeCore, let Control automatically cache the result
            SetState2(STATE2_USEPREFERREDSIZECACHE, true);

            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer, /*IsOwnerDraw()*/true);

            SetStyle(ControlStyles.FixedHeight |
                     ControlStyles.Selectable, false);

            SetStyle(ControlStyles.ResizeRedraw, true);

            //CommonProperties.SetSelfAutoSizeInDefaultLayout(this, true); // this make the program crash if there is any label at the moment

            labelState[StateFlatStyle] = (int)FlatStyle.Standard;
            labelState[StateUseMnemonic] = 1;
            labelState[StateBorderStyle] = (int)BorderStyle.None;

            TabStop = false;
            requestedHeight = Height;
            requestedWidth = Width;
        }

        public bool AutoSize;
        public bool UseCompatibleTextRendering = true;
        internal string text = "";

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual int PreferredHeight
        {
            get { return PreferredSize.Height; }
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual int PreferredWidth
        {
            get { return PreferredSize.Width; }
        }

        internal void AdjustSize()
        {
            if (!SelfSizing)
            {
                return;
            }

            // the rest is here for RTM compat.

            // If width and/or height are constrained by anchoring, don't adjust control size
            // to fit around text, since this will cause us to lose the original anchored size.
            if (!AutoSize &&
                ((this.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right) ||
                 (this.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom)))
            {
                return;
            }

            // Resize control to fit around current text
            //
            int saveHeight = requestedHeight;
            int saveWidth = requestedWidth;
            try
            {
                Size preferredSize = (AutoSize) ? PreferredSize : new Size(saveWidth, saveHeight);
                Size = preferredSize;
            }
            finally
            {
                requestedHeight = saveHeight;
                requestedWidth = saveWidth;
            }
        }
        internal virtual bool CanUseTextRenderer
        {
            get
            {
                return true;
            }
        }

        private bool SelfSizing
        {
            get
            {
                return CommonProperties.ShouldSelfSize(this);
            }
        }

        internal void Animate()
        {
            Animate(!DesignMode && Visible && Enabled && ParentInternal != null);
        }

        internal void StopAnimate()
        {
            Animate(false);
        }

        private void Animate(bool animate)
        {
            //bool currentlyAnimating = labelState[StateAnimating] != 0;
            //if (animate != currentlyAnimating)
            //{
            //    Image image = (Image)Properties.GetObject(PropImage);
            //    if (animate)
            //    {
            //        if (image != null)
            //        {
            //            ImageAnimator.Animate(image, new EventHandler(this.OnFrameChanged));
            //            labelState[StateAnimating] = animate ? 1 : 0;
            //        }
            //    }
            //    else
            //    {
            //        if (image != null)
            //        {
            //            ImageAnimator.StopAnimate(image, new EventHandler(this.OnFrameChanged));
            //            labelState[StateAnimating] = animate ? 1 : 0;
            //        }
            //    }
            //}
        }

        internal virtual void OnAutoEllipsisChanged(/*EventArgs e*/)
        {
        }

        internal override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Animate();
        }

        internal virtual void OnTextAlignChanged(EventArgs e)
        {
            EventHandler eh = Events[EVENT_TEXTALIGNCHANGED] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        internal override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            AdjustSize();
        }

        public virtual string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (value != text)
                {
                    text = value;
                    if (layoutPerformed)
                    {
                        Page.Set(WebviewIdentifier, "innerHTML", $"\"{value}\"");
                    }
                }
            }
        }

        protected override ImeMode DefaultImeMode
        {
            get
            {
                return ImeMode.Disable;
            }
        }

        protected override Padding DefaultMargin
        {
            get
            {
                return new Padding(3, 0, 3, 0);
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, AutoSize ? PreferredHeight : 23);
            }
        }

        [
            DefaultValue(FlatStyle.Standard),
        ]

        public FlatStyle FlatStyle
        {
            get
            {
                return (FlatStyle)labelState[StateFlatStyle];
            }
            set
            {
                //valid values are 0x0 to 0x3
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)FlatStyle.Flat, (int)FlatStyle.System))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(FlatStyle));
                }

                if (labelState[StateFlatStyle] != (int)value)
                {
                    bool needRecreate = (labelState[StateFlatStyle] == (int)FlatStyle.System) || (value == FlatStyle.System);

                    labelState[StateFlatStyle] = (int)value;

                    //SetStyle(ControlStyles.UserPaint
                    //         | ControlStyles.SupportsTransparentBackColor
                    //         | ControlStyles.OptimizedDoubleBuffer, OwnerDraw);

                    //if (needRecreate)
                    //{
                    //    // this will clear the preferred size cache - it's OK if the parent is null - it would be a NOP.
                    //    LayoutTransaction.DoLayoutIf(AutoSize, ParentInternal, this, PropertyNames.BorderStyle);
                    //    if (AutoSize)
                    //    {
                    //        AdjustSize();
                    //    }
                    //    RecreateHandle();
                    //}
                    //else
                    //{
                    //    Refresh();
                    //}
                }
            }
        }
        public async override void PerformLayout()
        {
            string style = "";
            string script = "";

            WebviewIdentifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += preLayoutScriptString;

            await Page.Add(Parent.WebviewIdentifier, "innerHTML", $"'<div id=\"{WebviewIdentifier}\" style=\"{style}\" class=\"label\">{Text}</div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
