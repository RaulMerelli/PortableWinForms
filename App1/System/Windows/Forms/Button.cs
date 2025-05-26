using App1;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
    public class Button : ButtonBase
    {
        private DialogResult dialogResult;
        private const int InvalidDimensionValue = Int32.MinValue;
        private Size systemSize = new Size(InvalidDimensionValue, InvalidDimensionValue);
        public Button() : base()
        {
            // Buttons shouldn't respond to right clicks, so we need to do all our own click logic
            SetStyle(ControlStyles.StandardClick |
                     ControlStyles.StandardDoubleClick,
                     false);
        }

        [
        Browsable(true),
        DefaultValue(AutoSizeMode.GrowOnly),
        Localizable(true),
        ]
        public AutoSizeMode AutoSizeMode
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

        //internal override ButtonBaseAdapter CreateFlatAdapter()
        //{
        //    return new ButtonFlatAdapter(this);
        //}

        //internal override ButtonBaseAdapter CreatePopupAdapter()
        //{
        //    return new ButtonPopupAdapter(this);
        //}

        //internal override ButtonBaseAdapter CreateStandardAdapter()
        //{
        //    return new ButtonStandardAdapter(this);
        //}

        [DefaultValue(DialogResult.None)]
        public virtual DialogResult DialogResult
        {
            get
            {
                return dialogResult;
            }
            set
            {
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)DialogResult.None, (int)DialogResult.No))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(DialogResult));
                }
                dialogResult = value;
            }
        }
        internal override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        internal override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public new event MouseEventHandler MouseDoubleClick
        {
            add
            {
                base.MouseDoubleClick += value;
            }
            remove
            {
                base.MouseDoubleClick -= value;
            }
        }

        public virtual void NotifyDefault(bool value)
        {
            if (IsDefault != value)
            {
                IsDefault = value;
            }
        }

        public async override void PerformLayout()
        {
            string style = "";
            string script = "";

            WebviewIdentifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += $"background-color: {System.Drawing.ColorTranslator.ToHtml(BackColor)};";
            style += CssLocationAndSize();

            script += preLayoutScriptString;

            await Page.Add(Parent.WebviewIdentifier, "innerHTML", $"'<button id=\"{WebviewIdentifier}\" class=\"button\" style=\"{style}\" ><p>{Text}</p></button>';");

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }

        internal override void OnClick(EventArgs e)
        {
            Form form = FindFormInternal();
            if (form != null)
            {
                //form.DialogResult = dialogResult;
            }

            //AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
            //AccessibilityNotifyClients(AccessibleEvents.NameChange, -1);
            
            base.OnClick(e);
        }

        public void PerformClick()
        {
            if (CanSelect)
            {
                bool validatedControlAllowsFocusChange;
                bool validate = ValidateActiveControl(out validatedControlAllowsFocusChange);
                if (!ValidationCancelled && (validate || validatedControlAllowsFocusChange))
                {
                    //Paint in raised state...
                    //
                    ResetFlagsandPaint();
                    OnClick(EventArgs.Empty);
                }
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (UseMnemonic && CanProcessMnemonic() && IsMnemonic(charCode, Text))
            {
                PerformClick();
                return true;
            }
            return base.ProcessMnemonic(charCode);
        }

        public override string ToString()
        {

            string s = base.ToString();
            return s + ", Text: " + Text;
        }
    }
}
