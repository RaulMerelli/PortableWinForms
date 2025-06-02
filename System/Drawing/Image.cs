using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace System.Drawing
{
    public abstract class Image : MarshalByRefObject, ISerializable, ICloneable, IDisposable
    {
        // However, as this delegate is not used in both GDI 1.0 and 1.1, we choose not
        // to modify it in Dev10, in order not to break exsiting code
        public delegate bool GetThumbnailImageAbort();

        /*
         * Handle to native image object
         */
        internal IntPtr nativeImage;

        // used to work around lack of animated gif encoder... rarely set...
        //
        byte[] rawData;

        //userData : so that user can use TAGS with IMAGES..
        private object userData;

        int width;
        int height;

        /**
         * Constructor can't be invoked directly
         */
        internal Image()
        {
        }

        /**
         * Constructor used in deserialization
         */
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal Image(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator sie = info.GetEnumerator();
            if (sie == null)
            {
                return;
            }
            for (; sie.MoveNext();)
            {
                if (String.Equals(sie.Name, "Data", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        byte[] dat = (byte[])sie.Value;
                        if (dat != null)
                        {
                            //InitializeFromStream(new MemoryStream(dat));
                        }

                    }
                    catch (ExternalException e)
                    {
                        Debug.Fail("failure: " + e.ToString());
                    }
                    catch (ArgumentException e)
                    {
                        Debug.Fail("failure: " + e.ToString());
                    }
                    catch (OutOfMemoryException e)
                    {
                        Debug.Fail("failure: " + e.ToString());
                    }
                    catch (InvalidOperationException e)
                    {
                        Debug.Fail("failure: " + e.ToString());
                    }
                    catch (NotImplementedException e)
                    {
                        Debug.Fail("failure: " + e.ToString());
                    }
                    catch (FileNotFoundException e)
                    {
                        Debug.Fail("failure: " + e.ToString());
                    }
                }
            }
        }

        [
        Localizable(false),
        Bindable(true),
        DefaultValue(null),
        TypeConverter(typeof(StringConverter))
        ]
        public object Tag
        {
            get
            {
                return userData;
            }
            set
            {
                userData = value;
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (nativeImage != IntPtr.Zero)
            {
                try
                {
                    //SafeNativeMethods.Gdip.GdipDisposeImage(new HandleRef(this, nativeImage));
                }
                catch (Exception ex)
                {
                    if (ClientUtils.IsSecurityOrCriticalException(ex))
                    {
                        throw;
                    }

                    Debug.Fail("Exception thrown during Dispose: " + ex.ToString());
                }
                finally
                {
                    nativeImage = IntPtr.Zero;
                }
            }
        }

        ~Image()
        {
            Dispose(false);
        }


        internal static Image CreateImageObject(IntPtr nativeImage)
        {
            Image image;

            int type = -1;

            //int status = SafeNativeMethods.Gdip.GdipGetImageType(new HandleRef(null, nativeImage), out type);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //    throw SafeNativeMethods.Gdip.StatusException(status);

            switch ((ImageTypeEnum)type)
            {
                case ImageTypeEnum.Bitmap:
                    image = Bitmap.FromGDIplus(nativeImage);
                    break;

                case ImageTypeEnum.Metafile:
                    image = Metafile.FromGDIplus(nativeImage);
                    break;

                default:
                    throw new ArgumentException("InvalidImage");
            }

            return image;
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal static void EnsureSave(Image image, string filename, Stream dataStream)
        {
            if (image.RawFormat.Equals(ImageFormat.Gif))
            {
                bool animatedGif = false;

                //Guid[] dimensions = image.FrameDimensionsList;
                //foreach (Guid guid in dimensions)
                //{
                //    FrameDimension dimension = new FrameDimension(guid);
                //    if (dimension.Equals(FrameDimension.Time))
                //    {
                //        animatedGif = image.GetFrameCount(FrameDimension.Time) > 1;
                //        break;
                //    }
                //}


                if (animatedGif)
                {
                    try
                    {
                        Stream created = null;
                        long lastPos = 0;
                        if (dataStream != null)
                        {
                            lastPos = dataStream.Position;
                            dataStream.Position = 0;
                        }

                        try
                        {
                            if (dataStream == null)
                            {
                                created = dataStream = File.OpenRead(filename);
                            }

                            image.rawData = new byte[(int)dataStream.Length];
                            dataStream.Read(image.rawData, 0, (int)dataStream.Length);
                        }
                        finally
                        {
                            if (created != null)
                            {
                                created.Close();
                            }
                            else
                            {
                                dataStream.Position = lastPos;
                            }
                        }
                    }
                    // possible exceptions for reading the filename
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (DirectoryNotFoundException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                    // possible exceptions for setting/getting the position inside dataStream
                    catch (NotSupportedException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    // possible exception when reading stuff into dataStream
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        private enum ImageTypeEnum
        {
            Bitmap = 1,
            Metafile = 2,
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {

        }

        public ImageFormat RawFormat
        {
            get
            {
                Guid guid = new Guid();

                return new ImageFormat(guid);
            }
        }

        public PixelFormat PixelFormat
        {
            get
            {
                return PixelFormat.Undefined;
            }
        }

        public Size Size
        {
            get
            {
                return new Size(Width, Height);
            }
        }

        [
        DefaultValue(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Width
        {
            get
            {
                return width;
            }
        }

        [
        DefaultValue(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Height
        {
            get
            {
                return height;
            }
        }

        public int Flags { get; internal set; }
    }
}
