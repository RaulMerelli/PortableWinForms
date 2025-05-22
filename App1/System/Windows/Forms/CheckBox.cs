using App1;
using System.ComponentModel;

namespace System.Windows.Forms
{
    public class CheckBox : ButtonBase
    {
        private static readonly object EVENT_CHECKEDCHANGED = new object();
        private static readonly object EVENT_CHECKSTATECHANGED = new object();
        private static readonly object EVENT_APPEARANCECHANGED = new object();
        private CheckState checkState;
        private Appearance appearance;
        private bool autoCheck;
        private bool threeState;
        private ContentAlignment checkAlign = ContentAlignment.MiddleLeft;

        public bool Checked { 
            get
            {
                return checkState != CheckState.Unchecked;
            }
            set
            {
                if (value != Checked)
                {
                    CheckState = (value ? CheckState.Checked : CheckState.Unchecked);
                }
            }
        }

        public CheckState CheckState
        {
            get
            {
                return checkState;
            }
            set
            {
                if (!ClientUtils.IsEnumValid(value, (int)value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(CheckState));
                }

                if (checkState != value)
                {
                    bool @checked = Checked;
                    checkState = value;
                    if (layoutPerformed)
                    {
                                            }

                    if (@checked != Checked)
                    {
                                                                    }

                                                        }
            }
        }

        public ContentAlignment CheckAlign
        {
            get
            {
                return checkAlign;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(ContentAlignment));
                }

                if (checkAlign != value)
                {
                    checkAlign = value;
                                        /*
                    LayoutTransaction.DoLayoutIf(AutoSize, ParentInternal, this, PropertyNames.CheckAlign);
                    if (base.OwnerDraw)
                    {
                        Invalidate();
                    }
                    else
                    {
                        UpdateStyles();
                    }
                    */
                }
            }
        }

        public Appearance Appearance
        {
            get
            {
                return appearance;
            }
            set
            {
                if (!ClientUtils.IsEnumValid(value, (int)value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(Appearance));
                }

                if (appearance == value)
                {
                    return;
                }

                                /*using (LayoutTransaction.CreateTransactionIf(AutoSize, ParentInternal, this, PropertyNames.Appearance))
                {*/
                    appearance = value;
                    /*
                     * if (base.OwnerDraw)
                    {
                        Refresh();
                    }
                    else
                    {
                        UpdateStyles();
                    }

                    OnAppearanceChanged(EventArgs.Empty);
                    */
                /*}*/
            }
        }

        public bool AutoCheck
        {
            get
            {
                return autoCheck;
            }
            set
            {
                autoCheck = value;
            }
        }

        public override ContentAlignment TextAlign
        {
            get
            {
                return base.TextAlign;
            }
            set
            {
                base.TextAlign = value;
            }
        }

        public bool ThreeState
        {
            get
            {
                return threeState;
            }
            set
            {
                threeState = value;
            }
        }

        public event EventHandler AppearanceChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_APPEARANCECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_APPEARANCECHANGED, value);
            }
        }

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

        public event EventHandler CheckedChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_CHECKEDCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CHECKEDCHANGED, value);
            }
        }

        public event EventHandler CheckStateChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_CHECKSTATECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CHECKSTATECHANGED, value);
            }
        }


        public async override void PerformLayout()
        {
            string style = "";
            string script = "";

            identifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += $"document.getElementById('{identifier}').addEventListener('click', function() {{ eventHandler('{identifier}', 'Click');}});​";
            script += $"document.getElementById('{identifier}').addEventListener('mouseenter', function() {{ eventHandler('{identifier}', 'MouseEnter');}});​";
            script += $"document.getElementById('{identifier}').addEventListener('mouseleave', function() {{ eventHandler('{identifier}', 'MouseLeave');}});​";

            await Page.Add(Parent.identifier, "innerHTML", $"'<div class=\"checkbox\" style=\"{style}\" id=\"{identifier}\"><input type=\"checkbox\" id=\"{identifier}-input\" name=\"{Parent.identifier}-option\" value=\"test\"><label for=\"{identifier}-input\">{Text}</label></div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
