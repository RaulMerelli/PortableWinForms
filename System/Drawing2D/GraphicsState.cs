namespace System.Drawing.Drawing2D
{
    using System;

    public sealed class GraphicsState : MarshalByRefObject
    {
        internal int nativeState;

        internal GraphicsState(int nativeState)
        {
            this.nativeState = nativeState;
        }
    }
}