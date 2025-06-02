namespace System.Windows.Forms.Design
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System;
    using System.Drawing;
    using System.Runtime.Versioning;

    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public abstract class PropertyTab : IExtenderProvider
    {

        private Object[] components;
        private Bitmap bitmap;
        private bool checkedBmp;

        ~PropertyTab()
        {
            Dispose(false);
        }

        // don't override this. Just put a 16x16 bitmap in a file with the same name as your class in your resources.
        public virtual Bitmap Bitmap
        {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get
            {
                if (!checkedBmp && bitmap == null)
                {
                    string bmpName = GetType().Name + ".bmp";
                    try
                    {
                        bitmap = new Bitmap(GetType(), bmpName);
                    }
                    catch (Exception ex)
                    {
                        Debug.Fail("Failed to find bitmap '" + bmpName + "' for class " + GetType().FullName, ex.ToString());
                    }
                    checkedBmp = true;
                }
                return bitmap;
            }
        }

        // don't override this either.
        public virtual Object[] Components
        {
            get
            {
                return components;
            }
            set
            {
                this.components = value;
            }
        }

        // okay.  Override this to give a good TabName.
        public abstract string TabName
        {
            get;
        }

        public virtual string HelpKeyword
        {
            get
            {
                return TabName;
            }
        }

        // override this to reject components you don't want to support.
        public virtual bool CanExtend(Object extendee)
        {
            return true;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
        }

        // return the default property item
        public virtual PropertyDescriptor GetDefaultProperty(Object component)
        {
            return TypeDescriptor.GetDefaultProperty(component);
        }

        // okay, override this to return whatever you want to return... All properties must apply to component.
        public virtual PropertyDescriptorCollection GetProperties(Object component)
        {
            return GetProperties(component, null);
        }

        public abstract PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes);

        public virtual PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            return GetProperties(component, attributes);
        }
    }
}
