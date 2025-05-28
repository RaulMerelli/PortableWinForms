using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
    public class PictureBox : Control, ISupportInitialize
    {
        private enum ImageInstallationType
        {
            DirectlySpecified,
            ErrorOrInitial,
            FromUrl
        }

        private BorderStyle borderStyle;
        //private Image image;
        private PictureBoxSizeMode sizeMode;
        private Size savedSize;
        private bool currentlyAnimating;
        private BitVector32 pictureBoxState;
        private string imageLocation;
        private ImageInstallationType imageInstallationType;

        private static readonly object EVENT_SIZEMODECHANGED = new object();

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AllowDrop
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                base.AllowDrop = value;
            }
        }

        [DefaultValue(BorderStyle.None)]
        [DispId(-504)]
        public BorderStyle BorderStyle
        {
            get
            {
                return borderStyle;
            }
            set
            {
                if (!ClientUtils.IsEnumValid(value, (int)value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(BorderStyle));
                }

                if (borderStyle != value)
                {
                    borderStyle = value;
                    //RecreateHandle();
                    //AdjustSize();
                }
            }
        }

        [Localizable(true)]
        [DefaultValue(null)]
        [RefreshProperties(RefreshProperties.All)]
        public string ImageLocation
        {
            get
            {
                return imageLocation;
            }
            set
            {
                imageLocation = value;
                pictureBoxState[32] = !string.IsNullOrEmpty(imageLocation);
                if (string.IsNullOrEmpty(imageLocation) && imageInstallationType != 0)
                {
                    //InstallNewImage(null, ImageInstallationType.DirectlySpecified);
                }

                if (/*WaitOnLoad && */!pictureBoxState[64] && !string.IsNullOrEmpty(imageLocation))
                {
                    //Load();
                }

                //Invalidate();
            }
        }

        public new bool CausesValidation
        {
            get
            {
                return base.CausesValidation;
            }
            set
            {
                base.CausesValidation = value;
            }
        }

        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }

        protected override ImeMode DefaultImeMode => ImeMode.Disable;

        protected override Size DefaultSize => new Size(100, 50);

        void ISupportInitialize.BeginInit()
        {
            pictureBoxState[64] = true;
        }

        void ISupportInitialize.EndInit()
        {
            if (ImageLocation != null && ImageLocation.Length != 0 /*&& WaitOnLoad*/)
            {
                //Load();
            }

            pictureBoxState[64] = false;
        }
    }
}
