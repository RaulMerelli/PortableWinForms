namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;

    public sealed class Matrix : MarshalByRefObject, IDisposable
    {
        private float[] elements = new float[6]; // m11, m12, m21, m22, dx, dy

        public Matrix()
        {
            Reset();
        }

        public Matrix(float m11, float m12, float m21, float m22, float dx, float dy)
        {
            elements[0] = m11;
            elements[1] = m12;
            elements[2] = m21;
            elements[3] = m22;
            elements[4] = dx;
            elements[5] = dy;
        }

        public Matrix(RectangleF rect, PointF[] plgpts)
        {
            if (plgpts == null)
                throw new ArgumentNullException(nameof(plgpts));
            if (plgpts.Length != 3)
                throw new ArgumentException("Three points required.", nameof(plgpts));

            float x = rect.X;
            float y = rect.Y;
            float width = rect.Width;
            float height = rect.Height;

            PointF p0 = plgpts[0];
            PointF p1 = plgpts[1];
            PointF p2 = plgpts[2];

            float a = (p1.X - p0.X) / width;
            float b = (p1.Y - p0.Y) / width;
            float c = (p2.X - p0.X) / height;
            float d = (p2.Y - p0.Y) / height;

            elements[0] = a;
            elements[1] = b;
            elements[2] = c;
            elements[3] = d;
            elements[4] = p0.X - (a * x + c * y);
            elements[5] = p0.Y - (b * x + d * y);
        }

        public Matrix(Rectangle rect, Point[] plgpts)
        {
            if (plgpts == null)
                throw new ArgumentNullException(nameof(plgpts));
            if (plgpts.Length != 3)
                throw new ArgumentException("Three points required.", nameof(plgpts));

            float x = rect.X;
            float y = rect.Y;
            float width = rect.Width;
            float height = rect.Height;

            Point p0 = plgpts[0];
            Point p1 = plgpts[1];
            Point p2 = plgpts[2];

            float a = (p1.X - p0.X) / width;
            float b = (p1.Y - p0.Y) / width;
            float c = (p2.X - p0.X) / height;
            float d = (p2.Y - p0.Y) / height;

            elements[0] = a;
            elements[1] = b;
            elements[2] = c;
            elements[3] = d;
            elements[4] = p0.X - (a * x + c * y);
            elements[5] = p0.Y - (b * x + d * y);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // No unmanaged resources to release
        }

        ~Matrix() => Dispose(false);

        public Matrix Clone()
        {
            return new Matrix(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
        }

        public float[] Elements => (float[])elements.Clone();

        public float OffsetX => elements[4];
        public float OffsetY => elements[5];

        public void Reset()
        {
            elements[0] = 1.0f;
            elements[1] = 0.0f;
            elements[2] = 0.0f;
            elements[3] = 1.0f;
            elements[4] = 0.0f;
            elements[5] = 0.0f;
        }

        public void Multiply(Matrix matrix) => Multiply(matrix, MatrixOrder.Prepend);

        public void Multiply(Matrix matrix, MatrixOrder order)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            float[] m1 = elements;
            float[] m2 = matrix.elements;

            float a, b, c, d, tx, ty;

            if (order == MatrixOrder.Prepend)
            {
                a = m1[0] * m2[0] + m1[1] * m2[2];
                b = m1[0] * m2[1] + m1[1] * m2[3];
                c = m1[2] * m2[0] + m1[3] * m2[2];
                d = m1[2] * m2[1] + m1[3] * m2[3];
                tx = m1[0] * m2[4] + m1[2] * m2[5] + m1[4];
                ty = m1[1] * m2[4] + m1[3] * m2[5] + m1[5];
            }
            else
            {
                a = m2[0] * m1[0] + m2[1] * m1[2];
                b = m2[0] * m1[1] + m2[1] * m1[3];
                c = m2[2] * m1[0] + m2[3] * m1[2];
                d = m2[2] * m1[1] + m2[3] * m1[3];
                tx = m2[0] * m1[4] + m2[2] * m1[5] + m2[4];
                ty = m2[1] * m1[4] + m2[3] * m1[5] + m2[5];
            }

            elements[0] = a;
            elements[1] = b;
            elements[2] = c;
            elements[3] = d;
            elements[4] = tx;
            elements[5] = ty;
        }

        public void Translate(float offsetX, float offsetY) => Translate(offsetX, offsetY, MatrixOrder.Prepend);

        public void Translate(float offsetX, float offsetY, MatrixOrder order)
        {
            Matrix translation = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            Multiply(translation, order);
        }

        public void Scale(float scaleX, float scaleY) => Scale(scaleX, scaleY, MatrixOrder.Prepend);

        public void Scale(float scaleX, float scaleY, MatrixOrder order)
        {
            Matrix scale = new Matrix(scaleX, 0, 0, scaleY, 0, 0);
            Multiply(scale, order);
        }

        public void Rotate(float angle) => Rotate(angle, MatrixOrder.Prepend);

        public void Rotate(float angle, MatrixOrder order)
        {
            float radians = (float)(angle * Math.PI / 180);
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            Matrix rotation = new Matrix(cos, sin, -sin, cos, 0, 0);
            Multiply(rotation, order);
        }

        public void RotateAt(float angle, PointF point) => RotateAt(angle, point, MatrixOrder.Prepend);

        public void RotateAt(float angle, PointF point, MatrixOrder order)
        {
            Matrix temp = new Matrix();
            temp.Translate(-point.X, -point.Y, MatrixOrder.Prepend);
            temp.Rotate(angle, MatrixOrder.Prepend);
            temp.Translate(point.X, point.Y, MatrixOrder.Prepend);
            Multiply(temp, order);
        }

        public void Shear(float shearX, float shearY) => Shear(shearX, shearY, MatrixOrder.Prepend);

        public void Shear(float shearX, float shearY, MatrixOrder order)
        {
            Matrix shearMatrix = new Matrix(1, shearY, shearX, 1, 0, 0);
            Multiply(shearMatrix, order);
        }

        public void Invert()
        {
            float a = elements[0];
            float b = elements[1];
            float c = elements[2];
            float d = elements[3];
            float tx = elements[4];
            float ty = elements[5];

            float det = a * d - b * c;
            if (det == 0)
                throw new InvalidOperationException("Matrix is not invertible.");

            float invDet = 1.0f / det;
            elements[0] = d * invDet;
            elements[1] = -b * invDet;
            elements[2] = -c * invDet;
            elements[3] = a * invDet;
            elements[4] = (c * ty - d * tx) * invDet;
            elements[5] = (b * tx - a * ty) * invDet;
        }

        public void TransformPoints(PointF[] pts)
        {
            if (pts == null)
                throw new ArgumentNullException(nameof(pts));

            float a = elements[0];
            float b = elements[1];
            float c = elements[2];
            float d = elements[3];
            float tx = elements[4];
            float ty = elements[5];

            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X;
                float y = pts[i].Y;
                pts[i].X = x * a + y * c + tx;
                pts[i].Y = x * b + y * d + ty;
            }
        }

        public void TransformPoints(Point[] pts)
        {
            if (pts == null)
                throw new ArgumentNullException(nameof(pts));

            float a = elements[0];
            float b = elements[1];
            float c = elements[2];
            float d = elements[3];
            float tx = elements[4];
            float ty = elements[5];

            for (int i = 0; i < pts.Length; i++)
            {
                int x = pts[i].X;
                int y = pts[i].Y;
                pts[i].X = (int)Math.Round(x * a + y * c + tx);
                pts[i].Y = (int)Math.Round(x * b + y * d + ty);
            }
        }

        public void TransformVectors(PointF[] pts)
        {
            if (pts == null)
                throw new ArgumentNullException(nameof(pts));

            float a = elements[0];
            float b = elements[1];
            float c = elements[2];
            float d = elements[3];

            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X;
                float y = pts[i].Y;
                pts[i].X = x * a + y * c;
                pts[i].Y = x * b + y * d;
            }
        }

        public void VectorTransformPoints(Point[] pts) => TransformVectors(pts);

        public void TransformVectors(Point[] pts)
        {
            if (pts == null)
                throw new ArgumentNullException(nameof(pts));

            float a = elements[0];
            float b = elements[1];
            float c = elements[2];
            float d = elements[3];

            for (int i = 0; i < pts.Length; i++)
            {
                int x = pts[i].X;
                int y = pts[i].Y;
                pts[i].X = (int)Math.Round(x * a + y * c);
                pts[i].Y = (int)Math.Round(x * b + y * d);
            }
        }

        public bool IsInvertible
        {
            get
            {
                float det = elements[0] * elements[3] - elements[1] * elements[2];
                return det != 0;
            }
        }

        public bool IsIdentity
        {
            get
            {
                return elements[0] == 1.0f && elements[1] == 0.0f &&
                       elements[2] == 0.0f && elements[3] == 1.0f &&
                       elements[4] == 0.0f && elements[5] == 0.0f;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Matrix other &&
                   elements[0] == other.elements[0] &&
                   elements[1] == other.elements[1] &&
                   elements[2] == other.elements[2] &&
                   elements[3] == other.elements[3] &&
                   elements[4] == other.elements[4] &&
                   elements[5] == other.elements[5];
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (float element in elements)
                    hash = hash * 31 + element.GetHashCode();
                return hash;
            }
        }

        internal Matrix(IntPtr nativeMatrix)
        {
            // This constructor is part of the original code but not used in managed implementation.
            // If required, convert from native matrix if needed (not applicable here).
            Reset();
        }

        internal void SetNativeMatrix(IntPtr nativeMatrix)
        {
            // Not applicable in managed implementation.
        }
    }
}