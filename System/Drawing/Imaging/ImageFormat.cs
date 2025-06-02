namespace System.Drawing.Imaging
{
    using System;
    using System.Drawing;
    using System.ComponentModel;

    /**
     * Image format constants
     */
    [TypeConverterAttribute(typeof(ImageFormatConverter))]
    public sealed class ImageFormat
    {
        // Format IDs
        // private static ImageFormat undefined = new ImageFormat(new Guid("{b96b3ca9-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat memoryBMP = new ImageFormat(new Guid("{b96b3caa-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat bmp = new ImageFormat(new Guid("{b96b3cab-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat emf = new ImageFormat(new Guid("{b96b3cac-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat wmf = new ImageFormat(new Guid("{b96b3cad-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat jpeg = new ImageFormat(new Guid("{b96b3cae-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat png = new ImageFormat(new Guid("{b96b3caf-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat gif = new ImageFormat(new Guid("{b96b3cb0-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat tiff = new ImageFormat(new Guid("{b96b3cb1-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat exif = new ImageFormat(new Guid("{b96b3cb2-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat photoCD = new ImageFormat(new Guid("{b96b3cb3-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat flashPIX = new ImageFormat(new Guid("{b96b3cb4-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat icon = new ImageFormat(new Guid("{b96b3cb5-0728-11d3-9d7b-0000f81ef32e}"));


        private Guid guid;

        public ImageFormat(Guid guid)
        {
            this.guid = guid;
        }

        public Guid Guid
        {
            get { return guid; }
        }

        public static ImageFormat MemoryBmp
        {
            get { return memoryBMP; }
        }

        public static ImageFormat Bmp
        {
            get { return bmp; }
        }

        public static ImageFormat Emf
        {
            get { return emf; }
        }

        public static ImageFormat Wmf
        {
            get { return wmf; }
        }

        public static ImageFormat Gif
        {
            get { return gif; }
        }

        public static ImageFormat Jpeg
        {
            get { return jpeg; }
        }

        public static ImageFormat Png
        {
            get { return png; }
        }

        public static ImageFormat Tiff
        {
            get { return tiff; }
        }

        public static ImageFormat Exif
        {
            get { return exif; }
        }

        public static ImageFormat Icon
        {
            get { return icon; }
        }

        public override bool Equals(object o)
        {
            ImageFormat format = o as ImageFormat;
            if (format == null)
                return false;
            return this.guid == format.guid;
        }

        public override int GetHashCode()
        {
            return this.guid.GetHashCode();
        }

#if !FEATURE_PAL
        // Find any random encoder which supports this format
        internal ImageCodecInfo FindEncoder()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID.Equals(this.guid))
                    return codec;
            }
            return null;
        }
#endif

        public override string ToString()
        {
            if (this == memoryBMP) return "MemoryBMP";
            if (this == bmp) return "Bmp";
            if (this == emf) return "Emf";
            if (this == wmf) return "Wmf";
            if (this == gif) return "Gif";
            if (this == jpeg) return "Jpeg";
            if (this == png) return "Png";
            if (this == tiff) return "Tiff";
            if (this == exif) return "Exif";
            if (this == icon) return "Icon";
            return "[ImageFormat: " + guid + "]";
        }
    }
}