using App1;
using System.ComponentModel;

namespace System.Windows.Forms
{
    public class ButtonBase : Control
    {
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
                    Page.RunScript($"document.getElementById(\"{identifier}\").getElementsByTagName('p')[0].innerHTML=\"{value}\"");
                }
            }
        }
        internal ContentAlignment textAlign = ContentAlignment.MiddleCenter;
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
        public bool UseCompatibleTextRendering = true;
        public bool UseVisualStyleBackColor = true;
    }
}
