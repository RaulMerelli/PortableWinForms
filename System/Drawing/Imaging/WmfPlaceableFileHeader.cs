namespace System.Drawing.Imaging
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class WmfPlaceableFileHeader
    {
        int key = unchecked((int)0x9aC6CDD7);
        short hmf;
        short bboxLeft;
        short bboxTop;
        short bboxRight;
        short bboxBottom;
        short inch;
        int reserved;
        short checksum;

        public int Key
        {
            get { return key; }
            set { key = value; }
        }

        public short Hmf
        {
            get { return hmf; }
            set { hmf = value; }
        }

        public short BboxLeft
        {
            get { return bboxLeft; }
            set { bboxLeft = value; }
        }

        public short BboxTop
        {
            get { return bboxTop; }
            set { bboxTop = value; }
        }

        public short BboxRight
        {
            get { return bboxRight; }
            set { bboxRight = value; }
        }

        public short BboxBottom
        {
            get { return bboxBottom; }
            set { bboxBottom = value; }
        }

        public short Inch
        {
            get { return inch; }
            set { inch = value; }
        }

        public int Reserved
        {
            get { return reserved; }
            set { reserved = value; }
        }

        public short Checksum
        {
            get { return checksum; }
            set { checksum = value; }
        }
    }
}