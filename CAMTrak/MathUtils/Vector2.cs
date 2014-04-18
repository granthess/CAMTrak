using System;
using System.Text;
namespace CAMTrak.MathUtils
{
    public struct Vector2
    {
        public static readonly Vector2 Empty = new Vector2(0f, 0f);
        public static readonly Vector2 Invalid = new Vector2(double.PositiveInfinity, double.PositiveInfinity);
        public static readonly Vector2 OutOfWorld = new Vector2(-20000f, -20000f);
        public static readonly Vector2 Zero = Vector2.Empty;
        public static readonly Vector2 Origin = Vector2.Empty;
        public static readonly Vector2 UnitX = new Vector2(1f, 0f);
        public static readonly Vector2 UnitY = new Vector2(0f, 1f);
        public static readonly Vector2 UnitZ = Vector2.UnitY;
        public double x;
        public double y;
        public Vector2(double _x, double _y)
        {
            this.x = _x;
            this.y = _y;
        }
        public Vector2(Vector2 Source)
        {
            this.x = Source.x;
            this.y = Source.y;
        }
        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return a.x != b.x || a.y != b.y;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != base.GetType())
            {
                return false;
            }
            Vector2 vector = (Vector2)obj;
            return this.x == vector.x && this.y == vector.y;
        }
        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode();
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            stringBuilder.Append(this.x.ToString("0.0000"));
            stringBuilder.Append(", ");
            stringBuilder.Append(this.y.ToString("0.0000"));
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
        public static bool TryParse(string values, out Vector2 vector2)
        {
            vector2 = default(Vector2);
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
            return array.Length == 2 && double.TryParse(array[0], out vector2.x) && double.TryParse(array[1], out vector2.y);
        }
        public void Set(double _x, double _y)
        {
            this.x = _x;
            this.y = _y;
        }
        public static Vector2 operator -(Vector2 vec)
        {
            return new Vector2(-vec.x, -vec.y);
        }
        public static Vector2 operator +(Vector2 vec, double scaler)
        {
            return new Vector2(vec.x + scaler, vec.y + scaler);
        }
        public static Vector2 operator -(Vector2 vec, double scaler)
        {
            return new Vector2(vec.x - scaler, vec.y - scaler);
        }
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
        public static Vector2 operator *(Vector2 vec, double scaler)
        {
            return new Vector2(vec.x * scaler, vec.y * scaler);
        }
        public static Vector2 operator /(Vector2 vec, double scaler)
        {
            return new Vector2(vec.x / scaler, vec.y / scaler);
        }
        public static double operator *(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }
        public Vector2 Multiply(Vector2 b)
        {
            return new Vector2(this.x * b.x, this.y * b.y);
        }
        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }
        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
        }
        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
        }
        public static Vector2 Floor(Vector2 v)
        {
            return new Vector2(MathUtils.Floor(v.x), MathUtils.Floor(v.y));
        }
        public static Vector2 Clamp(Vector2 a, Vector2 clampMin, Vector2 clampMax)
        {
            return new Vector2(MathUtils.Clamp(a.x, clampMin.x, clampMax.x), MathUtils.Clamp(a.y, clampMin.y, clampMax.y));
        }
        public double Length()
        {
            return (double)Math.Sqrt((double)this.LengthSqr());
        }
        public double LengthSqr()
        {
            return this.x * this.x + this.y * this.y;
        }
        public static Vector2 Normalize(Vector2 vec, out double length)
        {
            length = vec.Length();
            if (Math.Abs(length) < 1E-05f)
            {
                return Vector2.Zero;
            }
            double num = 1f / length;
            return new Vector2(vec.x * num, vec.y * num);
        }
        public static Vector2 Normalize(Vector2 vec)
        {
            double num;
            return Vector2.Normalize(vec, out num);
        }
        public double Normalize()
        {
            double result;
            this = Vector2.Normalize(this, out result);
            return result;
        }
    }
}
