using App1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class LinkLabel : Control
    {
        public bool AutoSize;
        public bool UseCompatibleTextRendering = true;
        public Color ActiveLinkColor;
        public Color DisabledLinkColor;

        private Color IELinkColor => LinkUtilities.IELinkColor;

        private Color IEActiveLinkColor => LinkUtilities.IEActiveLinkColor;

        private Color IEVisitedLinkColor => LinkUtilities.IEVisitedLinkColor;

        private static readonly object EventLinkClicked = new object();

        internal string text = "";

        public string Text
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
                        Page.Set(identifier, "innerHTML", $"\"{value}\"");
                    }
                }
            }
        }

        public event LinkLabelLinkClickedEventHandler LinkClicked
        {
            add
            {
                base.Events.AddHandler(EventLinkClicked, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLinkClicked, value);
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

            await Page.Add(Parent.identifier, "innerHTML", $"'<div id=\"{identifier}\" style=\"{style}\" class=\"label\">{Text}</div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }

        public class Link
        {
            private int start;

            private object linkData;

            private LinkState state;

            private bool enabled = true;

            //private Region visualRegion;

            internal int length;

            private LinkLabel owner;

            private string name;

            private string description;

            //internal LinkAccessibleObject accessibleObject;

            private object userData;

            /*internal AccessibleObject AccessibilityObject
            {
                get
                {
                    if (accessibleObject == null)
                    {
                        accessibleObject = new LinkAccessibleObject(this);
                    }

                    return accessibleObject;
                }
            }*/

            public string Description
            {
                get
                {
                    return description;
                }
                set
                {
                    description = value;
                }
            }

            [DefaultValue(true)]
            public bool Enabled
            {
                get
                {
                    return enabled;
                }
                set
                {
                    if (enabled == value)
                    {
                        return;
                    }

                    enabled = value;
                    if ((state & (LinkState)3) != 0)
                    {
                        state &= (LinkState)(-4);
                        if (owner != null)
                        {
                            //owner.OverrideCursor = null;
                        }
                    }

                    if (owner != null)
                    {
                        //owner.InvalidateLink(this);
                    }
                }
            }

            public int Length
            {
                get
                {
                    if (length == -1)
                    {
                        if (owner != null && !string.IsNullOrEmpty(owner.Text))
                        {
                            StringInfo stringInfo = new StringInfo(owner.Text);
                            return stringInfo.LengthInTextElements - Start;
                        }

                        return 0;
                    }

                    return length;
                }
                set
                {
                    if (length != value)
                    {
                        length = value;
                        if (owner != null)
                        {
                            //owner.InvalidateTextLayout();
                            //owner.Invalidate();
                        }
                    }
                }
            }

            [DefaultValue(null)]
            public object LinkData
            {
                get
                {
                    return linkData;
                }
                set
                {
                    linkData = value;
                }
            }

            internal LinkLabel Owner
            {
                get
                {
                    return owner;
                }
                set
                {
                    owner = value;
                }
            }

            internal LinkState State
            {
                get
                {
                    return state;
                }
                set
                {
                    state = value;
                }
            }

            [DefaultValue("")]
            public string Name
            {
                get
                {
                    if (name != null)
                    {
                        return name;
                    }

                    return "";
                }
                set
                {
                    name = value;
                }
            }

            public int Start
            {
                get
                {
                    return start;
                }
                set
                {
                    if (start != value)
                    {
                        start = value;
                        if (owner != null)
                        {
                            /*
                            owner.links.Sort(linkComparer);
                            owner.InvalidateTextLayout();
                            owner.Invalidate();
                            */
                        }
                    }
                }
            }

            [Localizable(false)]
            [Bindable(true)]
            [DefaultValue(null)]
            [TypeConverter(typeof(StringConverter))]
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

            [DefaultValue(false)]
            public bool Visited
            {
                get
                {
                    return (State & LinkState.Visited) == LinkState.Visited;
                }
                set
                {
                    bool visited = Visited;
                    if (value)
                    {
                        State |= LinkState.Visited;
                    }
                    else
                    {
                        State &= (LinkState)(-5);
                    }

                    if (visited != Visited && owner != null)
                    {
                        //owner.InvalidateLink(this);
                    }
                }
            }
            /*
            internal Region VisualRegion
            {
                get
                {
                    return visualRegion;
                }
                set
                {
                    visualRegion = value;
                }
            }
            */
            public Link()
            {
            }

            public Link(int start, int length)
            {
                this.start = start;
                this.length = length;
            }

            public Link(int start, int length, object linkData)
            {
                this.start = start;
                this.length = length;
                this.linkData = linkData;
            }

            internal Link(LinkLabel owner)
            {
                this.owner = owner;
            }
        }
    }
}
