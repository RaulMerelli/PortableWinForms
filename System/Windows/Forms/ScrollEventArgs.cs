namespace System.Windows.Forms
{
    using System;
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ScrollEventArgs : EventArgs
    {
        readonly ScrollEventType type;
        int newValue;
        private ScrollOrientation scrollOrientation;
        int oldValue = -1;

        public ScrollEventArgs(ScrollEventType type, int newValue)
        {
            this.type = type;
            this.newValue = newValue;
        }

        public ScrollEventArgs(ScrollEventType type, int newValue, ScrollOrientation scroll)
        {
            this.type = type;
            this.newValue = newValue;
            this.scrollOrientation = scroll;
        }

        public ScrollEventArgs(ScrollEventType type, int oldValue, int newValue)
        {
            this.type = type;
            this.newValue = newValue;
            this.oldValue = oldValue;

        }

        public ScrollEventArgs(ScrollEventType type, int oldValue, int newValue, ScrollOrientation scroll)
        {
            this.type = type;
            this.newValue = newValue;
            this.scrollOrientation = scroll;
            this.oldValue = oldValue;
        }

        public ScrollOrientation ScrollOrientation
        {
            get
            {
                return scrollOrientation;
            }
        }

        public ScrollEventType Type
        {
            get
            {
                return type;
            }
        }

        public int NewValue
        {
            get
            {
                return newValue;
            }
            set
            {
                newValue = value;
            }
        }

        public int OldValue
        {
            get
            {
                return oldValue;
            }
        }
    }
}