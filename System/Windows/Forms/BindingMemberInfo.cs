﻿namespace System.Windows.Forms
{
    using System;

    public struct BindingMemberInfo
    {
        private string dataList;
        private string dataField;

        public BindingMemberInfo(string dataMember)
        {
            if (dataMember == null)
                dataMember = "";

            int lastDot = dataMember.LastIndexOf(".");
            if (lastDot != -1)
            {
                dataList = dataMember.Substring(0, lastDot);
                dataField = dataMember.Substring(lastDot + 1);
            }
            else
            {
                dataList = "";
                dataField = dataMember;
            }
        }

        public string BindingPath
        {
            get
            {
                return (dataList != null ? dataList : "");
            }
        }

        public string BindingField
        {
            get
            {
                return (dataField != null ? dataField : "");
            }
        }

        public string BindingMember
        {
            get
            {
                return (BindingPath.Length > 0 ? BindingPath + "." + BindingField : BindingField);
            }
        }

        public override bool Equals(object otherObject)
        {
            if (otherObject is BindingMemberInfo)
            {
                BindingMemberInfo otherMember = (BindingMemberInfo)otherObject;
                return (String.Equals(this.BindingMember, otherMember.BindingMember, StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        public static bool operator ==(BindingMemberInfo a, BindingMemberInfo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BindingMemberInfo a, BindingMemberInfo b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}