namespace System.Drawing
{
    [Serializable]
    public sealed class Metafile : Image
    {
        internal void SetNativeImage(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("NativeHandle0");

            nativeImage = handle;
        }
        internal static Metafile FromGDIplus(IntPtr nativeImage)
        {
            Metafile metafile = new Metafile();
            metafile.SetNativeImage(nativeImage);
            return metafile;
        }

        private Metafile()
        {
        }
    }
}
