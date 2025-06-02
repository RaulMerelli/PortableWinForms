namespace System.Drawing
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Windows.Forms;

    /**
     * Represent a Texture brush object
     */
    public sealed class TextureBrush : Brush
    {


        /**
         * Create a new texture brush object
         *
         * @notes Should the rectangle parameter be Rectangle or RectF?
         *  We'll use Rectangle to specify pixel unit source image
         *  rectangle for now. Eventually, we'll need a mechanism
         *  to specify areas of an image in a resolution-independent way.
         *
         * @notes We'll make a copy of the bitmap object passed in.
         */

        // When creating a texture brush from a metafile image, the dstRect
        // is used to specify the size that the metafile image should be
        // rendered at in the device units of the destination graphics.
        // It is NOT used to crop the metafile image, so only the width 
        // and height values matter for metafiles.
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public TextureBrush(Image bitmap)
            : this(bitmap, System.Drawing.Drawing2D.WrapMode.Tile)
        {
        }

        // When creating a texture brush from a metafile image, the dstRect
        // is used to specify the size that the metafile image should be
        // rendered at in the device units of the destination graphics.
        // It is NOT used to crop the metafile image, so only the width 
        // and height values matter for metafiles.
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public TextureBrush(Image image, WrapMode wrapMode)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            //validate the WrapMode enum
            //valid values are 0x0 to 0x4
            if (!ClientUtils.IsEnumValid(wrapMode, unchecked((int)wrapMode), (int)WrapMode.Tile, (int)WrapMode.Clamp))
            {
                throw new InvalidEnumArgumentException("wrapMode", unchecked((int)wrapMode), typeof(WrapMode));
            }

            IntPtr brush = IntPtr.Zero;

            //int status = SafeNativeMethods.Gdip.GdipCreateTexture(new HandleRef(image, image.nativeImage),
            //                                       (int)wrapMode,
            //                                       out brush);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //    throw SafeNativeMethods.Gdip.StatusException(status);

            SetNativeBrushInternal(brush);
        }

        // When creating a texture brush from a metafile image, the dstRect
        // is used to specify the size that the metafile image should be
        // rendered at in the device units of the destination graphics.
        // It is NOT used to crop the metafile image, so only the width 
        // and height values matter for metafiles.
        // float version
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public TextureBrush(Image image, WrapMode wrapMode, RectangleF dstRect)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            //validate the WrapMode enum
            //valid values are 0x0 to 0x4
            if (!ClientUtils.IsEnumValid(wrapMode, unchecked((int)wrapMode), (int)WrapMode.Tile, (int)WrapMode.Clamp))
            {
                throw new InvalidEnumArgumentException("wrapMode", unchecked((int)wrapMode), typeof(WrapMode));
            }

            IntPtr brush = IntPtr.Zero;

            //int status = SafeNativeMethods.Gdip.GdipCreateTexture2(new HandleRef(image, image.nativeImage),
            //                                        unchecked((int)wrapMode),
            //                                        dstRect.X,
            //                                        dstRect.Y,
            //                                        dstRect.Width,
            //                                        dstRect.Height,
            //                                        out brush);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //    throw SafeNativeMethods.Gdip.StatusException(status);

            SetNativeBrushInternal(brush);
        }

        // int version
        // When creating a texture brush from a metafile image, the dstRect
        // is used to specify the size that the metafile image should be
        // rendered at in the device units of the destination graphics.
        // It is NOT used to crop the metafile image, so only the width 
        // and height values matter for metafiles.
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public TextureBrush(Image image, WrapMode wrapMode, Rectangle dstRect)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            //validate the WrapMode enum
            //valid values are 0x0 to 0x4
            if (!ClientUtils.IsEnumValid(wrapMode, unchecked((int)wrapMode), (int)WrapMode.Tile, (int)WrapMode.Clamp))
            {
                throw new InvalidEnumArgumentException("wrapMode", unchecked((int)wrapMode), typeof(WrapMode));
            }

            IntPtr brush = IntPtr.Zero;

            //int status = SafeNativeMethods.Gdip.GdipCreateTexture2I(new HandleRef(image, image.nativeImage),
            //                                         unchecked((int)wrapMode),
            //                                         dstRect.X,
            //                                         dstRect.Y,
            //                                         dstRect.Width,
            //                                         dstRect.Height,
            //                                         out brush);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}

            SetNativeBrushInternal(brush);
        }


        // When creating a texture brush from a metafile image, the dstRect
        // is used to specify the size that the metafile image should be
        // rendered at in the device units of the destination graphics.
        // It is NOT used to crop the metafile image, so only the width 
        // and height values matter for metafiles.
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public TextureBrush(Image image, RectangleF dstRect)
        : this(image, dstRect, (ImageAttributes)null) { }

        // When creating a texture brush from a metafile image, the dstRect
        // is used to specify the size that the metafile image should be
        // rendered at in the device units of the destination graphics.
        // It is NOT used to crop the metafile image, so only the width 
        // and height values matter for metafiles.
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public TextureBrush(Image image, RectangleF dstRect,
                            ImageAttributes imageAttr)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            IntPtr brush = IntPtr.Zero;

            //int status = SafeNativeMethods.Gdip.GdipCreateTextureIA(new HandleRef(image, image.nativeImage),
            //                                         new HandleRef(imageAttr, (imageAttr == null) ?
            //                                           IntPtr.Zero : imageAttr.nativeImageAttributes),
            //                                         dstRect.X,
            //                                         dstRect.Y,
            //                                         dstRect.Width,
            //                                         dstRect.Height,
            //                                         out brush);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}

            SetNativeBrushInternal(brush);
        }

        // When creating a texture brush from a metafile image, the dstRect
        // is used to specify the size that the metafile image should be
        // rendered at in the device units of the destination graphics.
        // It is NOT used to crop the metafile image, so only the width 
        // and height values matter for metafiles.
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public TextureBrush(Image image, Rectangle dstRect)
        : this(image, dstRect, (ImageAttributes)null) { }

        // When creating a texture brush from a metafile image, the dstRect
        // is used to specify the size that the metafile image should be
        // rendered at in the device units of the destination graphics.
        // It is NOT used to crop the metafile image, so only the width 
        // and height values matter for metafiles.
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public TextureBrush(Image image, Rectangle dstRect,
                            ImageAttributes imageAttr)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            IntPtr brush = IntPtr.Zero;

            //int status = SafeNativeMethods.Gdip.GdipCreateTextureIAI(new HandleRef(image, image.nativeImage),
            //                                         new HandleRef(imageAttr, (imageAttr == null) ?
            //                                           IntPtr.Zero : imageAttr.nativeImageAttributes),
            //                                         dstRect.X,
            //                                         dstRect.Y,
            //                                         dstRect.Width,
            //                                         dstRect.Height,
            //                                         out brush);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}

            SetNativeBrushInternal(brush);
        }

        internal TextureBrush(IntPtr nativeBrush)
        {
            Debug.Assert(nativeBrush != IntPtr.Zero, "Initializing native brush with null.");
            SetNativeBrushInternal(nativeBrush);
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public override Object Clone()
        {
            IntPtr cloneBrush = IntPtr.Zero;

            //int status = SafeNativeMethods.Gdip.GdipCloneBrush(new HandleRef(this, this.NativeBrush), out cloneBrush);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //    throw SafeNativeMethods.Gdip.StatusException(status);

            return new TextureBrush(cloneBrush);
        }


        /**
         * Set/get brush transform
         */
        private void _SetTransform(Matrix matrix)
        {
            //int status = SafeNativeMethods.Gdip.GdipSetTextureTransform(new HandleRef(this, this.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix));

            //if (status != SafeNativeMethods.Gdip.Ok)
            //    throw SafeNativeMethods.Gdip.StatusException(status);
        }

        private Matrix _GetTransform()
        {
            Matrix matrix = new Matrix();

            //int status = SafeNativeMethods.Gdip.GdipGetTextureTransform(new HandleRef(this, this.NativeBrush), new HandleRef(matrix, matrix.nativeMatrix));

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}

            return matrix;
        }

        public Matrix Transform
        {
            get { return _GetTransform(); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _SetTransform(value);
            }
        }

        /**
         * Set/get brush wrapping mode
         */
        private void _SetWrapMode(WrapMode wrapMode)
        {
            //int status = SafeNativeMethods.Gdip.GdipSetTextureWrapMode(new HandleRef(this, this.NativeBrush), unchecked((int)wrapMode));

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}
        }

        private WrapMode _GetWrapMode()
        {
            int mode = 0;

            //int status = SafeNativeMethods.Gdip.GdipGetTextureWrapMode(new HandleRef(this, this.NativeBrush), out mode);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}

            return (WrapMode)mode;
        }

        public WrapMode WrapMode
        {
            get
            {
                return _GetWrapMode();
            }
            set
            {
                //validate the WrapMode enum
                //valid values are 0x0 to 0x4
                if (!ClientUtils.IsEnumValid(value, unchecked((int)value), (int)WrapMode.Tile, (int)WrapMode.Clamp))
                {
                    throw new InvalidEnumArgumentException("value", unchecked((int)value), typeof(WrapMode));
                }

                _SetWrapMode(value);
            }
        }

        public Image Image
        {
            get
            {
                IntPtr image;
                image = IntPtr.Zero;
                //int status = SafeNativeMethods.Gdip.GdipGetTextureImage(new HandleRef(this, this.NativeBrush), out image);

                //if (status != SafeNativeMethods.Gdip.Ok)
                //{
                //    throw SafeNativeMethods.Gdip.StatusException(status);
                //}

                return Image.CreateImageObject(image);
            }
        }

        public void ResetTransform()
        {
            //int status = SafeNativeMethods.Gdip.GdipResetTextureTransform(new HandleRef(this, this.NativeBrush));

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}
        }

        public void MultiplyTransform(Matrix matrix)
        { MultiplyTransform(matrix, MatrixOrder.Prepend); }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            //int status = SafeNativeMethods.Gdip.GdipMultiplyTextureTransform(new HandleRef(this, this.NativeBrush),
            //                                                  new HandleRef(matrix, matrix.nativeMatrix),
            //                                                  order);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}
        }

        public void TranslateTransform(float dx, float dy)
        { TranslateTransform(dx, dy, MatrixOrder.Prepend); }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            //int status = SafeNativeMethods.Gdip.GdipTranslateTextureTransform(new HandleRef(this, this.NativeBrush),
            //                                                   dx,
            //                                                   dy,
            //                                                   order);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}
        }

        public void ScaleTransform(float sx, float sy)
        { ScaleTransform(sx, sy, MatrixOrder.Prepend); }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            //int status = SafeNativeMethods.Gdip.GdipScaleTextureTransform(new HandleRef(this, this.NativeBrush),
            //                                               sx,
            //                                               sy,
            //                                               order);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}
        }

        public void RotateTransform(float angle)
        { RotateTransform(angle, MatrixOrder.Prepend); }

        public void RotateTransform(float angle, MatrixOrder order)
        {
            //int status = SafeNativeMethods.Gdip.GdipRotateTextureTransform(new HandleRef(this, this.NativeBrush),
            //                                                angle,
            //                                                order);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}
        }
    }
}