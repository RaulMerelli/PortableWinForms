namespace System.Drawing.Imaging
{
    // sdkinc\imaging.h
    public sealed class PropertyItem
    {
        int id;
        int len;
        short type;
        byte[] value;

        internal PropertyItem()
        {
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public int Len
        {
            get { return len; }
            set { len = value; }
        }
        public short Type
        {
            get { return type; }
            set { type = value; }
        }
        public byte[] Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }
}