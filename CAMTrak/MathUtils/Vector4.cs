using System;
using System.Text;
namespace CAMTrak.MathUtils
{
    public struct Vector4
    {
        public static readonly Vector4 Empty = new Vector4(0f, 0f, 0f, 0f);
        public static readonly Vector4 Invalid = new Vector4(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        public static readonly Vector4 OutOfWorld = new Vector4(-20000f, -20000f, -20000f, 1f);
        public static readonly Vector4 Zero = Vector4.Empty;
        public static readonly Vector4 Origin = Vector4.Empty;
        public static readonly Vector4 UnitX = new Vector4(1f, 0f, 0f, 0f);
        public static readonly Vector4 UnitY = new Vector4(0f, 1f, 0f, 0f);
        public static readonly Vector4 UnitZ = new Vector4(0f, 0f, 1f, 0f);
        public static readonly Vector4 UnitW = new Vector4(0f, 0f, 0f, 1f);
        public double x;
        public double y;
        public double z;
        public double w;
        public Vector3 V3
        {
            get
            {
                return new Vector3(this);
            }
            set
            {
                this.x = value.x;
                this.y = value.y;
                this.z = value.z;
            }
        }
        public Vector4(Vector4 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = v.w;
        }
        public Vector4(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = 1f;
        }
        public Vector4(Vector2 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = 0f;
            this.w = 1f;
        }
        public static Vector4 CreateWorldVector4FromVector2(Vector2 v)
        {
            return new Vector4(v.x, 0f, v.y, 1f);
        }
        public Vector4(double _x, double _y, double _z, double _w)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
            this.w = _w;
        }
        public Vector4(double _x, double _y, double _z)
        {
            this = new Vector4(_x, _y, _z, 1f);
        }
        public void Set(double _x, double _y, double _z, double _w)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
            this.w = _w;
        }
        public void Set(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
            this.w = 1f;
        }
        public static Vector4 operator -(Vector4 vec)
        {
            return new Vector4(-vec.x, -vec.y, -vec.z, vec.w);
        }
        public static Vector4 operator +(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }
        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }
        public static Vector4 operator *(Vector4 vec, double scaler)
        {
            return new Vector4(vec.x * scaler, vec.y * scaler, vec.z * scaler, vec.w * scaler);
        }
        public static Vector4 operator /(Vector4 vec, double scaler)
        {
            return new Vector4(vec.x / scaler, vec.y / scaler, vec.z / scaler, vec.w / scaler);
        }
        public static double operator *(Vector4 a, Vector4 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }
        public double Length()
        {
            return (double)Math.Sqrt((double)this.LengthSqr());
        }
        public double LengthSqr()
        {
            return this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w;
        }
        public Vector4 Normalize()
        {
            double num = this.Length();
            if (Math.Abs(num) < 1E-05f)
            {
                this.x = (this.y = (this.z = (this.w = 0f)));
                return this;
            }
            num = 1f / num;
            this.x *= num;
            this.y *= num;
            this.z *= num;
            this.w *= num;
            return this;
        }
        public static Vector4 CrossProduct(Vector4 a, Vector4 b)
        {
            return new Vector4(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x, 0f);
        }
        public bool IsSimilarTo(Vector4 v)
        {
            return (this - v).LengthSqr() < 9.99999944E-11f;
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            stringBuilder.Append(this.x.ToString("0.0000"));
            stringBuilder.Append(", ");
            stringBuilder.Append(this.y.ToString("0.0000"));
            stringBuilder.Append(", ");
            stringBuilder.Append(this.z.ToString("0.0000"));
            stringBuilder.Append(", ");
            stringBuilder.Append(this.w.ToString("0.0000"));
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
    }
}
