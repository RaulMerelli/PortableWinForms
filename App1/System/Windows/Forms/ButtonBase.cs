using App1;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
    [
    ComVisible(true),
    ClassInterface(ClassInterfaceType.AutoDispatch),
    ]
    public class ButtonBase : Control
    {
        private FlatStyle flatStyle = System.Windows.Forms.FlatStyle.Standard;
        private ContentAlignment imageAlign = ContentAlignment.MiddleCenter;
        private ContentAlignment textAlign = ContentAlignment.MiddleCenter;
        private TextImageRelation textImageRelation = TextImageRelation.Overlay;
        //private ImageList.Indexer imageIndex = new ImageList.Indexer();
        private FlatButtonAppearance flatAppearance;
        //private ImageList imageList;
        //private Image image;

        private const int FlagMouseOver = 0x0001;
        private const int FlagMouseDown = 0x0002;
        private const int FlagMousePressed = 0x0004;
        private const int FlagInButtonUp = 0x0008;
        private const int FlagCurrentlyAnimating = 0x0010;
        private const int FlagAutoEllipsis = 0x0020;
        private const int FlagIsDefault = 0x0040;
        private const int FlagUseMnemonic = 0x0080;
        private const int FlagShowToolTip = 0x0100;
        private int state = 0;

        //private ToolTip textToolTip;

        //this allows the user to disable visual styles for the button so that it inherits its background color
        private bool enableVisualStyleBackground = true;

        private bool isEnableVisualStyleBackgroundSet = false;

        public bool AutoSize;
        internal string text;
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                if (layoutPerformed)
                {
                    Page.RunScript($"document.getElementById(\"{WebviewIdentifier}\").getElementsByTagName('p')[0].innerHTML=\"{value}\"");
                }
            }
        }

        public virtual ContentAlignment TextAlign
        {
            get
            {
                return textAlign;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(ContentAlignment));
                }

                if (value != textAlign)
                {
                    textAlign = value;
                }
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
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)FlatStyle.Flat, (int)FlatStyle.System))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(FlatStyle));
                }
                flatStyle = value;
                LayoutTransaction.DoLayoutIf(AutoSize, ParentInternal, this, PropertyNames.FlatStyle);
                Invalidate();
                UpdateOwnerDraw();
            }
        }

        [
        Browsable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ]
        public FlatButtonAppearance FlatAppearance
        {
            get
            {
                if (flatAppearance == null)
                {
                    flatAppearance = new FlatButtonAppearance(this);
                }

                return flatAppearance;
            }
        }

        internal bool OwnerDraw
        {
            get
            {
                return FlatStyle != FlatStyle.System;
            }
        }

        internal virtual Rectangle DownChangeRectangle
        {
            get
            {
                return ClientRectangle;
            }
        }

        internal bool MouseIsPressed
        {
            get
            {
                return GetFlag(FlagMousePressed);
            }
        }

        // a "smart" version of mouseDown for Appearance.Button CheckBoxes & RadioButtons
        // for these, instead of being based on the actual mouse state, it's based on the appropriate button state
        internal bool MouseIsDown
        {
            get
            {
                return GetFlag(FlagMouseDown);
            }
        }

        // a "smart" version of mouseOver for Appearance.Button CheckBoxes & RadioButtons
        // for these, instead of being based on the actual mouse state, it's based on the appropriate button state
        internal bool MouseIsOver
        {
            get
            {
                return GetFlag(FlagMouseOver);
            }
        }

        protected override ImeMode DefaultImeMode
        {
            get
            {
                return ImeMode.Disable;
            }
        }

        protected internal bool IsDefault
        {
            get
            {
                return GetFlag(FlagIsDefault);
            }
            set
            {
                if (GetFlag(FlagIsDefault) != value)
                {
                    SetFlag(FlagIsDefault, value);
                    if (IsHandleCreated)
                    {
                        if (OwnerDraw)
                        {
                            Invalidate();
                        }
                        else
                        {
                            UpdateStyles();
                        }
                    }
                }
            }
        }

        private void UpdateOwnerDraw()
        {
            if (OwnerDraw != GetStyle(ControlStyles.UserPaint))
            {
                SetStyle(ControlStyles.UserMouse | ControlStyles.UserPaint, OwnerDraw);
                //RecreateHandle();
            }
        }

        private bool GetFlag(int flag)
        {
            return ((state & flag) == flag);
        }

        private void SetFlag(int flag, bool value)
        {
            bool oldValue = ((state & flag) != 0);

            if (value)
            {
                state |= flag;
            }
            else
            {
                state &= ~flag;
            }

            if (OwnerDraw && (flag & FlagMouseDown) != 0 && value != oldValue)
            {
                //AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
            }
        }

        protected void ResetFlagsandPaint()
        {
            SetFlag(FlagMousePressed, false);
            SetFlag(FlagMouseDown, false);
            Invalidate(DownChangeRectangle);
            Update();
        }

        [DefaultValue(true)]
        public bool UseMnemonic
        {
            get
            {
                return GetFlag(FlagUseMnemonic);
            }

            set
            {
                SetFlag(FlagUseMnemonic, value);
                LayoutTransaction.DoLayoutIf(AutoSize, ParentInternal, this, PropertyNames.Text);
                Invalidate();
            }
        }


        public bool UseCompatibleTextRendering = true;
        public bool UseVisualStyleBackColor = true;
    }
}
