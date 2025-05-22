namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Collections;
    using ArrayList = System.Collections.ArrayList;

    public class BaseCollection : MarshalByRefObject, ICollection
    {

        //==================================================
        // the ICollection methods
        //==================================================
        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)
        ]
        public virtual int Count
        {
            get
            {
                return List.Count;
            }
        }

        public void CopyTo(Array ar, int index)
        {
            List.CopyTo(ar, index);
        }

        public IEnumerator GetEnumerator()
        {
            return List.GetEnumerator();
        }

        [
        Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)
        ]
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsSynchronized
        {
            get
            {
                // so the user will know that it has to lock this object
                return false;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        protected virtual ArrayList List
        {
            get
            {
                return null;
            }
        }
    }
}