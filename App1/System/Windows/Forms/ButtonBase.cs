using App1;

namespace System.Windows.Forms
{
    public class ButtonBase : Control
    {
        public bool AutoSize;
        internal string _Text;
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
                if (layoutPerformed)
                {
                    Page.RunScript($"document.getElementById(\"{identifier}\").getElementsByTagName('p')[0].innerHTML=\"{value}\"");
                }
            }
        }
        public bool UseCompatibleTextRendering = true;
        public bool UseVisualStyleBackColor = true;
    }
}
