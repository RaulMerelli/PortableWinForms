namespace System.Drawing.Internal
{
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class GPPOINTF
    {
        internal float X;
        internal float Y;

        internal GPPOINTF()
        {
        }

        internal GPPOINTF(PointF pt)
        {
            X = pt.X;
            Y = pt.Y;
        }

        internal GPPOINTF(Point pt)
        {
            X = (float)pt.X;
            Y = (float)pt.Y;
        }

        internal PointF ToPoint()
        {
            return new PointF(X, Y);
        }
    }
}