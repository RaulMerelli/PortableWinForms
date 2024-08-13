using System.Drawing;

namespace System.Windows.Forms
{
    public class Control
    {
        public bool Enabled;
        public AnchorStyles Anchor;
        public Point Location;
        public Size Size;
        public string Name;
        public int TabIndex;
        public ControlCollection Controls = new ControlCollection();
        public EventHandler Click;
        public EventHandler TextChanged;
        public bool Visible = true;
        public Control Parent;
        public IntPtr Handle;
        public int internalIndex;
        public string identifier = "";
        //public Font Font;
        public Color ForeColor;
        internal bool layoutPerformed = false;

        public virtual void SuspendLayout()
        {

        }

        public virtual void ResumeLayout(bool performLayout)
        {

        }

        public virtual void PerformLayout()
        {

        }

        protected void PerformChildLayout()
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].identifier = identifier + "-";
                Controls[i].Parent = this;
                Controls[i].PerformLayout();
            }
        }
    }
}