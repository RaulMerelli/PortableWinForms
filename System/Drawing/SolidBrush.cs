using System.Drawing.Internal;
using System.Runtime.Versioning;

namespace System.Drawing
{
    public sealed class SolidBrush : Brush, ISystemColorTracker
    {
        // GDI+ doesn't understand system colors, so we need to cache the value here
        private Color color = Color.Empty;
        private bool immutable;

        public Color Color { get; internal set; }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public void OnSystemColorChanged()
        {
            throw new NotImplementedException();
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public SolidBrush(Color color)
        {
            this.color = color;

            IntPtr brush = IntPtr.Zero;
            //int status = SafeNativeMethods.Gdip.GdipCreateSolidFill(this.color.ToArgb(), out brush);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //    throw SafeNativeMethods.Gdip.StatusException(status);

            SetNativeBrushInternal(brush);

            if (color.IsSystemColor)
                SystemColorTracker.Add(this);
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal SolidBrush(Color color, bool immutable) : this(color)
        {
            this.immutable = immutable;
        }
    }
}
