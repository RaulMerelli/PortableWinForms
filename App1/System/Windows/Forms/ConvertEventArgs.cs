namespace System.Windows.Forms
{
    using System;

    public class ConvertEventArgs : EventArgs
    {

        private object value;
        private Type desiredType;

        public ConvertEventArgs(object value, Type desiredType)
        {
            this.value = value;
            this.desiredType = desiredType;
        }

        public object Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public Type DesiredType
        {
            get
            {
                return desiredType;
            }
        }
    }
}