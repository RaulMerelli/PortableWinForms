namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;

    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
    public class PaintValueEventArgs : EventArgs
    {
        private readonly ITypeDescriptorContext context;

        private readonly object valueToPaint;

        //private readonly Graphics graphics;

        private readonly Rectangle bounds;

        //public PaintValueEventArgs(ITypeDescriptorContext context, object value, Graphics graphics, Rectangle bounds)
        //{
        //    this.context = context;
        //    this.valueToPaint = value;

        //    this.graphics = graphics;
        //    if (graphics == null)
        //        throw new ArgumentNullException("graphics");

        //    this.bounds = bounds;
        //}

        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
        }

        public ITypeDescriptorContext Context
        {
            get
            {
                return context;
            }
        }

        //public Graphics Graphics
        //{
        //    get
        //    {
        //        return graphics;
        //    }
        //}

        public object Value
        {
            get
            {
                return valueToPaint;
            }
        }
    }
}