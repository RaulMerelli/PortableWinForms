﻿namespace System.Windows.Forms
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms;


    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    [SuppressMessage("Microsoft.Security", "CA2108:ReviewDeclarativeSecurityOnValueTypes")]
    public struct Message
    {
#if DEBUG
        static TraceSwitch AllWinMessages = new TraceSwitch("AllWinMessages", "Output every received message");
#endif

        IntPtr hWnd;
        int msg;
        IntPtr wparam;
        IntPtr lparam;
        IntPtr result;


        public IntPtr HWnd
        {
            get { return hWnd; }
            set { hWnd = value; }
        }

        public int Msg
        {
            get { return msg; }
            set { msg = value; }
        }

        public IntPtr WParam
        {
            get { return wparam; }
            set { wparam = value; }
        }

        public IntPtr LParam
        {
            get { return lparam; }
            set { lparam = value; }
        }

        public IntPtr Result
        {
            get { return result; }
            set { result = value; }
        }

        public object GetLParam(Type cls)
        {
            return UnsafeNativeMethods.PtrToStructure(lparam, cls);
        }

        public static Message Create(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            Message m = new Message();
            m.hWnd = hWnd;
            m.msg = msg;
            m.wparam = wparam;
            m.lparam = lparam;
            m.result = IntPtr.Zero;

#if DEBUG
            if (AllWinMessages.TraceVerbose)
            {
                Debug.WriteLine(m.ToString());
            }
#endif
            return m;
        }

        public override bool Equals(object o)
        {
            if (!(o is Message))
            {
                return false;
            }

            Message m = (Message)o;
            return hWnd == m.hWnd &&
                   msg == m.msg &&
                   wparam == m.wparam &&
                   lparam == m.lparam &&
                   result == m.result;
        }

        public static bool operator !=(Message a, Message b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(Message a, Message b)
        {
            return a.Equals(b);
        }

        public override int GetHashCode()
        {
            return (int)hWnd << 4 | msg;
        }

        public override string ToString()
        {
            // ----URT : 151574. Link Demand on System.Windows.Forms.Message
            // fails to protect overriden methods.
            bool unrestricted = false;
            try
            {
                IntSecurity.UnmanagedCode.Demand();
                unrestricted = true;
            }
            catch (SecurityException)
            {
                // eat the exception.
            }

            if (unrestricted)
            {
                return MessageDecoder.ToString(this);
            }
            else
            {
                return base.ToString();
            }
        }
    }
}