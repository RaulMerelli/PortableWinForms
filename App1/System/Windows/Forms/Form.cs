using App1;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Drawing;

namespace System.Windows.Forms
{
    public class Form : Control
    {
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
                }
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
