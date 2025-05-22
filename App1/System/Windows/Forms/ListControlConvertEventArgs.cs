namespace System.Windows.Forms
{
    using System;
    public class ListControlConvertEventArgs : ConvertEventArgs
    {
        object listItem;
        public ListControlConvertEventArgs(object value, Type desiredType, object listItem) : base(value, desiredType)
        {
            this.listItem = listItem;
        }

        public object ListItem
        {
            get
            {
                return this.listItem;
            }
        }
    }
}