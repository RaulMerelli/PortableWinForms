namespace System.Drawing.Imaging
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    // sdkinc\imaging.h
    public sealed class ImageCodecInfo
    {
        Guid clsid;
        Guid formatID;
        string codecName;
        string dllName;
        string formatDescription;
        string filenameExtension;
        string mimeType;
        ImageCodecFlags flags;
        int version;
        byte[][] signaturePatterns;
        byte[][] signatureMasks;

        internal ImageCodecInfo()
        {
        }

        public Guid Clsid
        {
            get { return clsid; }
            set { clsid = value; }
        }
        public Guid FormatID
        {
            get { return formatID; }
            set { formatID = value; }
        }
        public string CodecName
        {
            get { return codecName; }
            set { codecName = value; }
        }
        public string DllName
        {
            [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity")]
            get
            {
                if (dllName != null)
                {
                    //a valid path is a security concern, demand
                    //path discovery permission....
                    new System.Security.Permissions.FileIOPermission(System.Security.Permissions.FileIOPermissionAccess.PathDiscovery, dllName).Demand();
                }
                return dllName;
            }
            [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity")]
            set
            {
                if (value != null)
                {
                    //a valid path is a security concern, demand
                    //path discovery permission....
                    new System.Security.Permissions.FileIOPermission(System.Security.Permissions.FileIOPermissionAccess.PathDiscovery, value).Demand();
                }
                dllName = value;
            }
        }
        public string FormatDescription
        {
            get { return formatDescription; }
            set { formatDescription = value; }
        }
        public string FilenameExtension
        {
            get { return filenameExtension; }
            set { filenameExtension = value; }
        }
        public string MimeType
        {
            get { return mimeType; }
            set { mimeType = value; }
        }
        public ImageCodecFlags Flags
        {
            get { return flags; }
            set { flags = value; }
        }
        public int Version
        {
            get { return version; }
            set { version = value; }
        }
        [CLSCompliant(false)]
        public byte[][] SignaturePatterns
        {
            get { return signaturePatterns; }
            set { signaturePatterns = value; }
        }
        [CLSCompliant(false)]
        public byte[][] SignatureMasks
        {
            get { return signatureMasks; }
            set { signatureMasks = value; }
        }

        // Encoder/Decoder selection APIs

        public static ImageCodecInfo[] GetImageDecoders()
        {
            ImageCodecInfo[] imageCodecs;
            int numDecoders;
            int size;

            imageCodecs = new ImageCodecInfo[0];
            //int status = SafeNativeMethods.Gdip.GdipGetImageDecodersSize(out numDecoders, out size);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}

            //IntPtr memory = Marshal.AllocHGlobal(size);

            //try
            //{
            //    status = SafeNativeMethods.Gdip.GdipGetImageDecoders(numDecoders, size, memory);

            //    if (status != SafeNativeMethods.Gdip.Ok)
            //    {
            //        throw SafeNativeMethods.Gdip.StatusException(status);
            //    }

            //    imageCodecs = ImageCodecInfo.ConvertFromMemory(memory, numDecoders);
            //}
            //finally
            //{
            //    Marshal.FreeHGlobal(memory);
            //}

            return imageCodecs;
        }

        public static ImageCodecInfo[] GetImageEncoders()
        {
            ImageCodecInfo[] imageCodecs;
            int numEncoders;
            int size;

            imageCodecs = new ImageCodecInfo[0];
            //int status = SafeNativeMethods.Gdip.GdipGetImageEncodersSize(out numEncoders, out size);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}

            //IntPtr memory = Marshal.AllocHGlobal(size);

            //try
            //{
            //    status = SafeNativeMethods.Gdip.GdipGetImageEncoders(numEncoders, size, memory);

            //    if (status != SafeNativeMethods.Gdip.Ok)
            //    {
            //        throw SafeNativeMethods.Gdip.StatusException(status);
            //    }

            //    imageCodecs = ImageCodecInfo.ConvertFromMemory(memory, numEncoders);
            //}
            //finally
            //{
            //    Marshal.FreeHGlobal(memory);
            //}

            return imageCodecs;
        }

        private static ImageCodecInfo[] ConvertFromMemory(IntPtr memoryStart, int numCodecs)
        {
            ImageCodecInfo[] codecs = new ImageCodecInfo[numCodecs];

            int index;

            for (index = 0; index < numCodecs; index++)
            {
                IntPtr curcodec = (IntPtr)((long)memoryStart + (int)Marshal.SizeOf(typeof(ImageCodecInfoPrivate)) * index);
                ImageCodecInfoPrivate codecp = new ImageCodecInfoPrivate();
                Drawing.UnsafeNativeMethods.PtrToStructure(curcodec, codecp);

                codecs[index] = new ImageCodecInfo();
                codecs[index].Clsid = codecp.Clsid;
                codecs[index].FormatID = codecp.FormatID;
                codecs[index].CodecName = Marshal.PtrToStringUni(codecp.CodecName);
                codecs[index].DllName = Marshal.PtrToStringUni(codecp.DllName);
                codecs[index].FormatDescription = Marshal.PtrToStringUni(codecp.FormatDescription);
                codecs[index].FilenameExtension = Marshal.PtrToStringUni(codecp.FilenameExtension);
                codecs[index].MimeType = Marshal.PtrToStringUni(codecp.MimeType);

                codecs[index].Flags = (ImageCodecFlags)codecp.Flags;
                codecs[index].Version = (int)codecp.Version;

                codecs[index].SignaturePatterns = new byte[codecp.SigCount][];
                codecs[index].SignatureMasks = new byte[codecp.SigCount][];

                for (int j = 0; j < codecp.SigCount; j++)
                {
                    codecs[index].SignaturePatterns[j] = new byte[codecp.SigSize];
                    codecs[index].SignatureMasks[j] = new byte[codecp.SigSize];

                    Marshal.Copy((IntPtr)((long)codecp.SigMask + j * codecp.SigSize), codecs[index].SignatureMasks[j], 0, codecp.SigSize);
                    Marshal.Copy((IntPtr)((long)codecp.SigPattern + j * codecp.SigSize), codecs[index].SignaturePatterns[j], 0, codecp.SigSize);
                }
            }

            return codecs;
        }

    }

}