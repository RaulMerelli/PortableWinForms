using App1;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
    public class Form : Control
    {
        private static readonly object EVENT_RESIZEBEGIN = new object();

        private static readonly object EVENT_RESIZEEND = new object();

        internal string text;
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
                        Page.RunScript($"$(document.getElementById('{identifier}')).parent().closest('.jsPanel')[0].querySelector('.jsPanel-title').innerHTML=\"{value}\"");
                    }
                }
            }
        }

        public Size Size
        {
            get
            {
                return base.size;
            }
            set
            {
                if (value != base.Size)
                {
                    base.size = value;
                    if (layoutPerformed)
                    {
                        Page.RunScript($"$(document.getElementById('{identifier}')).parent().closest('.jsPanel')[0].resize({{width:{value.Width},height:{value.Height}}});");
                    }
                }
            }
        }

        public bool UseCompatibleTextRendering = true;
        public SizeF AutoScaleDimensions;
        public AutoScaleMode AutoScaleMode;
        public bool MinimizeBox = true;
        public bool MaximizeBox = true;
        public FormBorderStyle FormBorderStyle = FormBorderStyle.Sizable;

        public int FakeHandle;

        public override void PerformLayout()
        {
            create();
            PerformChildLayout();
            layoutPerformed = true;
        }

        private async void webView2_WebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            string received = args.TryGetWebMessageAsString();
            var eventReturn = JsonHelper.Deserialize<EventReturn>(received);

            Control child = FindControlById(this, eventReturn.identifier);

            if (child != null)
            {
                switch (eventReturn.eventName)
                {
                    case "Click":
                        child.OnClick(EventArgs.Empty);
                        break;
                    case "Input":
                        if (child.GetType().IsSubclassOf(typeof(TextBox)) || child.GetType() == typeof(TextBox))
                        {
                            TextBox textBox = (TextBox)child;
                            textBox.text = await Page.Get(textBox.identifier, textBox.Multiline ? "innerHTML" : "value");
                        }
                        if (child.GetType().IsSubclassOf(typeof(Label)) || child.GetType() == typeof(Label))
                        {
                            Label label = (Label)child;
                            label.text = await Page.Get(label.identifier, "innerHTML");
                        }
                        if (child.GetType().IsSubclassOf(typeof(Button)) || child.GetType() == typeof(Button))
                        {
                            Button button = (Button)child;
                            button.text = await Page.GetFromScript($"document.getElementById(\"{identifier}\").getElementsByTagName('p')[0].innerHTML");
                        }
                        child.OnTextChanged(EventArgs.Empty);
                        break;
                    case "Resize":
                    case "ResizeBegin":
                    case "ResizeEnd":
                        if (child.GetType().IsSubclassOf(typeof(Form)) || child.GetType() == typeof(Form))
                        {
                            Form form = (Form)child;
                            string wstr = (await Page.RunScript($"$(document.getElementById('{identifier}')).parent().closest('.jsPanel')[0].style.width")).Replace("px", "").Replace("\"", "");
                            string hstr = (await Page.RunScript($"$(document.getElementById('{identifier}')).parent().closest('.jsPanel')[0].style.height")).Replace("px", "").Replace("\"", "");
                            int w = int.Parse(wstr);
                            int h = int.Parse(hstr);
                            size = new Size(w, h);
                            if (eventReturn.eventName == "ResizeBegin")
                            {
                                form.OnResizeBegin(EventArgs.Empty);
                            }
                            else if (eventReturn.eventName == "ResizeEnd")
                            {
                                form.OnResizeEnd(EventArgs.Empty);
                            }
                            else
                            {
                                form.OnResize(EventArgs.Empty);
                            }
                        }
                        break;
                }
            }
        }

        protected internal override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            //if (formState[FormStateRenderSizeGrip] != 0)
            //{
            //    Invalidate();
            //}
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnResizeBegin(EventArgs e)
        {
            if (CanRaiseEvents)
            {
                ((EventHandler)base.Events[EVENT_RESIZEBEGIN])?.Invoke(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnResizeEnd(EventArgs e)
        {
            if (CanRaiseEvents)
            {
                ((EventHandler)base.Events[EVENT_RESIZEEND])?.Invoke(this, e);
            }
        }

        public event EventHandler ResizeBegin
        {
            add
            {
                base.Events.AddHandler(EVENT_RESIZEBEGIN, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RESIZEBEGIN, value);
            }
        }

        public event EventHandler ResizeEnd
        {
            add
            {
                base.Events.AddHandler(EVENT_RESIZEEND, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RESIZEEND, value);
            }
        }

        async void create()
        {
            FakeHandle = new Random().Next(1, 65000);
            identifier = FakeHandle + "-" + Name;
            string style = "";

            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"background-color: {System.Drawing.ColorTranslator.ToHtml(BackColor)};";
            style += $"background-repeat: repeat;";
            style += $"min-height: {ClientSize.Height}px;";
            style += $"min-width: {ClientSize.Width}px;";
            style += $"height: 100%;";
            style += $"width: 100%;";
            style += $"overflow: hidden;";

            string htmlContent = $"<div id=\"{identifier}\" class=\"{this.Name}\" style=\"{style}\"></div>";

            JsonProperties.Properties properties = new JsonProperties.Properties
            {
                id = FakeHandle.ToString(),
                identifier = FakeHandle.ToString() + "-" + Name.ToString(),
                icon = "",
                maximize = MaximizeBox,
                minimize = MinimizeBox,
                mdi = false,
                position = new JsonProperties.Position
                {
                    at = "left-top",
                    my = "left-top"
                },
                resizable = FormBorderStyle == FormBorderStyle.Sizable || FormBorderStyle == FormBorderStyle.SizableToolWindow,
                size = new JsonProperties.Size
                {
                    width = $"{ClientSize.Width}px",
                    height = $"{ClientSize.Height}px"
                },
                title = Text
            };
            string propertiesSerialized = JsonHelper.Serialize(properties);
            string script = $"launchPostWindowSuccess(`{htmlContent}`,`{propertiesSerialized}`)";
            await Page.RunScript(script);
            Page.pContainer.WebMessageReceived += webView2_WebMessageReceived;
        }

        // Static recursive method to find the Control with the specified id
        internal Control FindControlById(Control startControl, string id)
        {
            // Check if the startControl has the matching id
            if (startControl.identifier == id)
            {
                return startControl;
            }

            // If not, recursively search through the nested controls using a classic for loop
            for (int i = 0; i < startControl.Controls.Count; i++)
            {
                var result = FindControlById(startControl.Controls[i], id);
                if (result != null)
                {
                    return result;
                }
            }

            // If no control is found with the specified id, return null
            return null;
        }
    }
}
