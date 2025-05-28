using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace System.Windows.Forms
{
    public struct Padding
    {
        private bool _all;
        private int _top;
        private int _left;
        private int _right;
        private int _bottom;

        public static readonly Padding Empty = new Padding(0);

        [RefreshProperties(RefreshProperties.All)]
        public int All
        {
            get
            {
                if (!_all)
                {
                    return -1;
                }

                return _top;
            }
            set
            {
                if (!_all || _top != value)
                {
                    _all = true;
                    _top = (_left = (_right = (_bottom = value)));
                }
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public int Bottom
        {
            get
            {
                if (_all)
                {
                    return _top;
                }

                return _bottom;
            }
            set
            {
                if (_all || _bottom != value)
                {
                    _all = false;
                    _bottom = value;
                }
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public int Left
        {
            get
            {
                if (_all)
                {
                    return _top;
                }

                return _left;
            }
            set
            {
                if (_all || _left != value)
                {
                    _all = false;
                    _left = value;
                }
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public int Right
        {
            get
            {
                if (_all)
                {
                    return _top;
                }

                return _right;
            }
            set
            {
                if (_all || _right != value)
                {
                    _all = false;
                    _right = value;
                }
            }
        }

        //
        // Riepilogo:
        //     Ottiene o imposta il valore di spaziatura interna del bordo superiore.
        //
        // Valori restituiti:
        //     Spaziatura interna, in pixel, del bordo superiore.
        [RefreshProperties(RefreshProperties.All)]
        public int Top
        {
            get
            {
                return _top;
            }
            set
            {
                if (_all || _top != value)
                {
                    _all = false;
                    _top = value;
                }
            }
        }

        [Browsable(false)]
        public int Horizontal => Left + Right;

        [Browsable(false)]
        public int Vertical => Top + Bottom;

        [Browsable(false)]
        public Size Size => new Size(Horizontal, Vertical);

        public Padding(int all)
        {
            _all = true;
            _top = (_left = (_right = (_bottom = all)));
        }

        public Padding(int left, int top, int right, int bottom)
        {
            _top = top;
            _left = left;
            _right = right;
            _bottom = bottom;
            _all = _top == _left && _top == _right && _top == _bottom;
        }

        public static Padding Add(Padding p1, Padding p2)
        {
            return p1 + p2;
        }

        public static Padding Subtract(Padding p1, Padding p2)
        {
            return p1 - p2;
        }

        public override bool Equals(object other)
        {
            if (other is Padding)
            {
                return (Padding)other == this;
            }

            return false;
        }

        public static Padding operator +(Padding p1, Padding p2)
        {
            return new Padding(p1.Left + p2.Left, p1.Top + p2.Top, p1.Right + p2.Right, p1.Bottom + p2.Bottom);
        }

        public static Padding operator -(Padding p1, Padding p2)
        {
            return new Padding(p1.Left - p2.Left, p1.Top - p2.Top, p1.Right - p2.Right, p1.Bottom - p2.Bottom);
        }

        public static bool operator ==(Padding p1, Padding p2)
        {
            if (p1.Left == p2.Left && p1.Top == p2.Top && p1.Right == p2.Right)
            {
                return p1.Bottom == p2.Bottom;
            }

            return false;
        }

        public static bool operator !=(Padding p1, Padding p2)
        {
            return !(p1 == p2);
        }

        public override int GetHashCode()
        {
            return Left ^ WindowsFormsUtils.RotateLeft(Top, 8) ^ WindowsFormsUtils.RotateLeft(Right, 16) ^ WindowsFormsUtils.RotateLeft(Bottom, 24);
        }

        public override string ToString()
        {
            return "{Left=" + Left.ToString(CultureInfo.CurrentCulture) + ",Top=" + Top.ToString(CultureInfo.CurrentCulture) + ",Right=" + Right.ToString(CultureInfo.CurrentCulture) + ",Bottom=" + Bottom.ToString(CultureInfo.CurrentCulture) + "}";
        }

        private void ResetAll()
        {
            All = 0;
        }

        private void ResetBottom()
        {
            Bottom = 0;
        }

        private void ResetLeft()
        {
            Left = 0;
        }

        private void ResetRight()
        {
            Right = 0;
        }

        private void ResetTop()
        {
            Top = 0;
        }

        internal void Scale(float dx, float dy)
        {
            _top = (int)((float)_top * dy);
            _left = (int)((float)_left * dx);
            _right = (int)((float)_right * dx);
            _bottom = (int)((float)_bottom * dy);
        }

        internal bool ShouldSerializeAll()
        {
            return _all;
        }

        [Conditional("DEBUG")]
        private void Debug_SanityCheck()
        {
            _ = _all;
        }
    }
}
