namespace System.Windows.Forms.Layout
{
    using System;
    using System.Drawing;
    using System.Diagnostics.CodeAnalysis;

    public abstract class LayoutEngine
    {
        internal IArrangedElement CastToArrangedElement(object obj)
        {
            IArrangedElement element = obj as IArrangedElement;
            if (obj == null)
            {
                throw new NotSupportedException("LayoutEngineUnsupportedType");
            }
            return element;
        }

        internal virtual Size GetPreferredSize(IArrangedElement container, Size proposedConstraints) { return Size.Empty; }

        public virtual void InitLayout(object child, BoundsSpecified specified)
        {
            InitLayoutCore(CastToArrangedElement(child), specified);
        }

        internal virtual void InitLayoutCore(IArrangedElement element, BoundsSpecified bounds) { }

        internal virtual void ProcessSuspendedLayoutEventArgs(IArrangedElement container, LayoutEventArgs args) { }
  
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public virtual bool Layout(object container, LayoutEventArgs layoutEventArgs)
        {
            bool parentNeedsLayout = LayoutCore(CastToArrangedElement(container), layoutEventArgs);
            return parentNeedsLayout;
        }

        internal virtual bool LayoutCore(IArrangedElement container, LayoutEventArgs layoutEventArgs) { return false; }
    }
}