using System;
using System.Text;
namespace CAMTrak.MathUtils
{
    public struct Vector3
    {
        public static readonly Vector3 Empty = new Vector3(0f, 0f, 0f);
        public static readonly Vector3 Invalid = new Vector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        public static readonly Vector3 OutOfWorld = new Vector3(-20000f, -20000f, -20000f);
        public static readonly Vector3 Zero = Vector3.Empty;
        public static readonly Vector3 Origin = new Vector3(0f, 0f, 0f);
        public static readonly Vector3 UnitX = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 UnitY = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 UnitZ = new Vector3(0f, 0f, 1f);
        public double x;
        public double y;
        public double z;
        public Vector4 P4
        {
            get
            {
                return new Vector4(this.x, this.y, this.z, 1f);
            }
        }
        public Vector4 V4
        {
            get
            {
                return new Vector4(this.x, this.y, this.z, 0f);
            }
        }
        public Vector3(Vector4 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }
        public Vector3(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }
        public Vector3(Vector2 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = 0f;
        }
        public static Vector3 CreateWorldVector3FromVector2(Vector2 v)
        {
            return new Vector3(v.x, 0f, v.y);
        }
        public Vector3(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != base.GetType())
            {
                return false;
            }
            Vector3 vector = (Vector3)obj;
            return this.x == vector.x && this.y == vector.y && this.z == vector.z;
        }
        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode();
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
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
        public static bool TryParse(string values, out Vector3 vector3)
        {
            vector3 = default(Vector3);
            if (values == null)
            {
                return false;
            }
            string text = values.Replace(" ", "");
            text = text.Replace("f", "");
            string[] array = text.Split(new char[]
			{
				','
			});
            return array.Length == 3 && double.TryParse(array[0], out vector3.x) && double.TryParse(array[1], out vector3.y) && double.TryParse(array[2], out vector3.z);
        }
        public void Set(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }
        public static Vector3 operator -(Vector3 vec)
        {
            return new Vector3(-vec.x, -vec.y, -vec.z);
        }
        public static Vector3 operator +(Vector3 vec, double scaler)
        {
            return new Vector3(vec.x + scaler, vec.y + scaler, vec.z + scaler);
        }
        public static Vector3 operator -(Vector3 vec, double scaler)
        {
            return new Vector3(vec.x - scaler, vec.y - scaler, vec.z - scaler);
        }
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Vector3 operator *(Vector3 vec, double scaler)
        {
            return new Vector3(vec.x * scaler, vec.y * scaler, vec.z * scaler);
        }
        public static Vector3 operator /(Vector3 vec, double scaler)
        {
            return new Vector3(vec.x / scaler, vec.y / scaler, vec.z / scaler);
        }
        public static double operator *(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }
        public Vector3 Multiply(Vector3 b)
        {
            return new Vector3(this.x * b.x, this.y * b.y, this.z * b.z);
        }
        public static Vector3 operator /(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            return new Vector3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
        }
        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            return new Vector3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }
        public static Vector3 Floor(Vector3 v)
        {
            return new Vector3(MathUtils.Floor(v.x), MathUtils.Floor(v.y), MathUtils.Floor(v.z));
        }
        public static Vector3 Lerp(Vector3 a, Vector3 b, double t)
        {
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }
        public static Vector3 Clamp(Vector3 a, Vector3 clampMin, Vector3 clampMax)
        {
            return new Vector3(MathUtils.Clamp(a.x, clampMin.x, clampMax.x), MathUtils.Clamp(a.y, clampMin.y, clampMax.y), MathUtils.Clamp(a.z, clampMin.z, clampMax.z));
        }
        public static Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }
        public Vector3 Normalize()
        {
            double num = this.Length();
            if (Math.Abs(num) < 1E-05f)
            {
                this.x = (this.y = (this.z = 0f));
                return this;
            }
            num = 1f / num;
            this.x *= num;
            this.y *= num;
            this.z *= num;
            return this;
        }
        public double Length()
        {
            return (double)Math.Sqrt((double)this.LengthSqr());
        }
        public double LengthSqr()
        {
            return this.x * this.x + this.y * this.y + this.z * this.z;
        }
        public static Vector3 operator *(Matrix44 xf, Vector3 vec)
        {
            Vector3 vector = default(Vector3);
            vector = xf.right.V3 * vec.x;
            vector += xf.up.V3 * vec.y;
            vector += xf.at.V3 * vec.z;
            vector += xf.pos.V3;
            return vector;
        }
        public bool IsSimilarTo(Vector3 v)
        {
            return (this - v).LengthSqr() < 9.99999944E-11f;
        }
    }
}
