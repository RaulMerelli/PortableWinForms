namespace System.Drawing.Imaging
{
    using System;

    /**
     * frame dimension constants (used with Bitmap.FrameDimensionsList)
     */
    // [TypeConverterAttribute(typeof(FrameDimensionConverter))]
    public sealed class FrameDimension
    {
        // Frame dimension GUIDs, from sdkinc\imgguids.h
        private static FrameDimension time = new FrameDimension(new Guid("{6aedbd6d-3fb5-418a-83a6-7f45229dc872}"));
        private static FrameDimension resolution = new FrameDimension(new Guid("{84236f7b-3bd3-428f-8dab-4ea1439ca315}"));
        private static FrameDimension page = new FrameDimension(new Guid("{7462dc86-6180-4c7e-8e3f-ee7333a7a483}"));

        private Guid guid;

        public FrameDimension(Guid guid)
        {
            this.guid = guid;
        }

        public Guid Guid
        {
            get { return guid; }
        }

        public static FrameDimension Time
        {
            get { return time; }
        }

        public static FrameDimension Resolution
        {
            get { return resolution; }
        }

        public static FrameDimension Page
        {
            get { return page; }
        }
        public override bool Equals(object o)
        {
            FrameDimension format = o as FrameDimension;
            if (format == null)
                return false;
            return this.guid == format.guid;
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public override string ToString()
        {
            if (this == time) return "Time";
            if (this == resolution) return "Resolution";
            if (this == page) return "Page";
            return "[FrameDimension: " + guid + "]";
        }
    }
}