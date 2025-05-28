namespace System.Windows.Forms
{
    using System;
    using System.Collections;


    public class InputLanguageCollection : ReadOnlyCollectionBase
    {

        internal InputLanguageCollection(InputLanguage[] value)
        {
            InnerList.AddRange(value);
        }

        public InputLanguage this[int index]
        {
            get
            {
                return ((InputLanguage)(InnerList[index]));
            }
        }

        public bool Contains(InputLanguage value)
        {
            return InnerList.Contains(value);
        }

        public void CopyTo(InputLanguage[] array, int index)
        {
            InnerList.CopyTo(array, index);
        }

        public int IndexOf(InputLanguage value)
        {
            return InnerList.IndexOf(value);
        }
    }
}