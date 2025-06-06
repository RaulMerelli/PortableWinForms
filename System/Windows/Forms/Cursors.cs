﻿namespace System.Windows.Forms
{
    using System;

    public sealed class Cursors
    {
        private static Cursor appStarting = null;
        private static Cursor arrow = null;
        private static Cursor cross = null;
        private static Cursor defaultCursor = null;
        private static Cursor iBeam = null;
        private static Cursor no = null;
        private static Cursor sizeAll = null;
        private static Cursor sizeNESW = null;
        private static Cursor sizeNS = null;
        private static Cursor sizeNWSE = null;
        private static Cursor sizeWE = null;
        private static Cursor upArrow = null;
        private static Cursor wait = null;
        private static Cursor help = null;
        private static Cursor hSplit = null;
        private static Cursor vSplit = null;
        private static Cursor noMove2D = null;
        private static Cursor noMoveHoriz = null;
        private static Cursor noMoveVert = null;
        private static Cursor panEast = null;
        private static Cursor panNE = null;
        private static Cursor panNorth = null;
        private static Cursor panNW = null;
        private static Cursor panSE = null;
        private static Cursor panSouth = null;
        private static Cursor panSW = null;
        private static Cursor panWest = null;
        private static Cursor hand = null;

        private Cursors()
        {
        }

        internal static Cursor KnownCursorFromHCursor(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                return new Cursor(handle);
            }

            // if (handle == Cursors.AppStarting.Handle)   return Cursors.AppStarting;
            // if (handle == Cursors.Arrow.Handle)         return Cursors.Arrow;
            // if (handle == Cursors.IBeam.Handle)         return Cursors.IBeam;
            // if (handle == Cursors.Cross.Handle)         return Cursors.Cross;
            // if (handle == Cursors.Default.Handle)       return Cursors.Default;
            // if (handle == Cursors.No.Handle)            return Cursors.No;
            // if (handle == Cursors.SizeAll.Handle)       return Cursors.SizeAll;
            // if (handle == Cursors.SizeNS.Handle)        return Cursors.SizeNS;
            // if (handle == Cursors.SizeWE.Handle)        return Cursors.SizeWE;
            // if (handle == Cursors.SizeNWSE.Handle)      return Cursors.SizeNWSE;
            // if (handle == Cursors.SizeNESW.Handle)      return Cursors.SizeNESW;
            // if (handle == Cursors.VSplit.Handle)        return Cursors.VSplit;
            // if (handle == Cursors.HSplit.Handle)        return Cursors.HSplit;
            // if (handle == Cursors.WaitCursor.Handle)    return Cursors.WaitCursor;
            // if (handle == Cursors.Help.Handle)          return Cursors.Help;
            // if (handle == IntPtr.Zero)     return null;

            //         appStarting = new Cursor(NativeMethods.IDC_APPSTARTING,0);
            //         arrow = new Cursor(NativeMethods.IDC_ARROW,0);
            //         cross = new Cursor(NativeMethods.IDC_CROSS,0);
            //         defaultCursor = new Cursor(NativeMethods.IDC_ARROW,0);
            //         iBeam = new Cursor(NativeMethods.IDC_IBEAM,0);
            //         no = new Cursor(NativeMethods.IDC_NO,0);
            //         sizeAll = new Cursor(NativeMethods.IDC_SIZEALL,0);
            //         sizeNESW = new Cursor(NativeMethods.IDC_SIZENESW,0);
            //         sizeNS      = new Cursor(NativeMethods.IDC_SIZENS,0);
            //         sizeNWSE    = new Cursor(NativeMethods.IDC_SIZENWSE,0);
            //         sizeWE      = new Cursor(NativeMethods.IDC_SIZEWE,0);
            //         upArrow     = new Cursor(NativeMethods.IDC_UPARROW,0);
            //         wait        = new Cursor(NativeMethods.IDC_WAIT,0);
            //         help        = new Cursor(NativeMethods.IDC_HELP,0);
        }

        public static Cursor AppStarting
        {
            get
            {
                if (appStarting == null)
                    appStarting = new Cursor(NativeMethods.IDC_APPSTARTING, 0);
                return appStarting;
            }
        }

        public static Cursor Arrow
        {
            get
            {
                if (arrow == null)
                    arrow = new Cursor(NativeMethods.IDC_ARROW, 0);
                return arrow;
            }
        }

        public static Cursor Cross
        {
            get
            {
                if (cross == null)
                    cross = new Cursor(NativeMethods.IDC_CROSS, 0);
                return cross;
            }
        }

        public static Cursor Default
        {
            get
            {
                if (defaultCursor == null)
                    defaultCursor = new Cursor(NativeMethods.IDC_ARROW, 0);
                return defaultCursor;
            }
        }

        public static Cursor IBeam
        {
            get
            {
                if (iBeam == null)
                    iBeam = new Cursor(NativeMethods.IDC_IBEAM, 0);
                return iBeam;
            }
        }

        public static Cursor No
        {
            get
            {
                if (no == null)
                    no = new Cursor(NativeMethods.IDC_NO, 0);
                return no;
            }
        }

        public static Cursor SizeAll
        {
            get
            {
                if (sizeAll == null)
                    sizeAll = new Cursor(NativeMethods.IDC_SIZEALL, 0);
                return sizeAll;
            }
        }

        public static Cursor SizeNESW
        {
            get
            {
                if (sizeNESW == null)
                    sizeNESW = new Cursor(NativeMethods.IDC_SIZENESW, 0);
                return sizeNESW;
            }
        }

        public static Cursor SizeNS
        {
            get
            {
                if (sizeNS == null)
                    sizeNS = new Cursor(NativeMethods.IDC_SIZENS, 0);
                return sizeNS;
            }
        }

        public static Cursor SizeNWSE
        {
            get
            {
                if (sizeNWSE == null)
                    sizeNWSE = new Cursor(NativeMethods.IDC_SIZENWSE, 0);
                return sizeNWSE;
            }
        }

        public static Cursor SizeWE
        {
            get
            {
                if (sizeWE == null)
                    sizeWE = new Cursor(NativeMethods.IDC_SIZEWE, 0);
                return sizeWE;
            }
        }

        public static Cursor UpArrow
        {
            get
            {
                if (upArrow == null)
                    upArrow = new Cursor(NativeMethods.IDC_UPARROW, 0);
                return upArrow;
            }
        }

        public static Cursor WaitCursor
        {
            get
            {
                if (wait == null)
                    wait = new Cursor(NativeMethods.IDC_WAIT, 0);
                return wait;
            }
        }

        public static Cursor Help
        {
            get
            {
                if (help == null)
                    help = new Cursor(NativeMethods.IDC_HELP, 0);
                return help;
            }
        }

        public static Cursor HSplit
        {
            get
            {
                if (hSplit == null)
                    hSplit = new Cursor("hsplit.cur", 0);
                return hSplit;
            }
        }

        public static Cursor VSplit
        {
            get
            {
                if (vSplit == null)
                    vSplit = new Cursor("vsplit.cur", 0);
                return vSplit;
            }
        }

        public static Cursor NoMove2D
        {
            get
            {
                if (noMove2D == null)
                    noMove2D = new Cursor("nomove2d.cur", 0);
                return noMove2D;
            }
        }

        public static Cursor NoMoveHoriz
        {
            get
            {
                if (noMoveHoriz == null)
                    noMoveHoriz = new Cursor("nomoveh.cur", 0);
                return noMoveHoriz;
            }
        }

        public static Cursor NoMoveVert
        {
            get
            {
                if (noMoveVert == null)
                    noMoveVert = new Cursor("nomovev.cur", 0);
                return noMoveVert;
            }
        }

        public static Cursor PanEast
        {
            get
            {
                if (panEast == null)
                    panEast = new Cursor("east.cur", 0);
                return panEast;
            }
        }

        public static Cursor PanNE
        {
            get
            {
                if (panNE == null)
                    panNE = new Cursor("ne.cur", 0);
                return panNE;
            }
        }

        public static Cursor PanNorth
        {
            get
            {
                if (panNorth == null)
                    panNorth = new Cursor("north.cur", 0);
                return panNorth;
            }
        }

        public static Cursor PanNW
        {
            get
            {
                if (panNW == null)
                    panNW = new Cursor("nw.cur", 0);
                return panNW;
            }
        }

        public static Cursor PanSE
        {
            get
            {
                if (panSE == null)
                    panSE = new Cursor("se.cur", 0);
                return panSE;
            }
        }

        public static Cursor PanSouth
        {
            get
            {
                if (panSouth == null)
                    panSouth = new Cursor("south.cur", 0);
                return panSouth;
            }
        }

        public static Cursor PanSW
        {
            get
            {
                if (panSW == null)
                    panSW = new Cursor("sw.cur", 0);
                return panSW;
            }
        }

        public static Cursor PanWest
        {
            get
            {
                if (panWest == null)
                    panWest = new Cursor("west.cur", 0);
                return panWest;
            }
        }

        public static Cursor Hand
        {
            get
            {
                if (hand == null)
                    hand = new Cursor("hand.cur", 0);
                return hand;
            }
        }
    }
}
