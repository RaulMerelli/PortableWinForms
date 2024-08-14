using System.ComponentModel;

namespace System.Windows.Forms
{
    public sealed class LayoutEventArgs : EventArgs
    {
        private readonly IComponent affectedComponent;

        private readonly string affectedProperty;

        public IComponent AffectedComponent => affectedComponent;

        public Control AffectedControl => affectedComponent as Control;

        public string AffectedProperty => affectedProperty;

        public LayoutEventArgs(IComponent affectedComponent, string affectedProperty)
        {
            this.affectedComponent = affectedComponent;
            this.affectedProperty = affectedProperty;
        }

        public LayoutEventArgs(Control affectedControl, string affectedProperty)
            : this((IComponent)affectedControl, affectedProperty)
        {
        }
    }
}
