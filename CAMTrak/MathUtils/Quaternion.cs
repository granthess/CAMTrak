using System;
namespace CAMTrak.MathUtils
{
    public struct Quaternion
    {
        
        public static readonly Quaternion Identity = new Quaternion(0f, 0f, 0f, 1f);
        public Vector3 v;
        public double n;
        public double Magnitude
        {
            get
            {
                return (double)Math.Sqrt((double)(this.n * this.n + this.v.x * this.v.x + this.v.y * this.v.y + this.v.z * this.v.z));
            }
        }
        public Vector3 Vector
        {
            get
            {
                return this.v;
            }
        }
        public double Scaler
        {
            get
            {
                return this.n;
            }
        }
        public Quaternion(double _x, double _y, double _z, double _n)
        {
            this.n = _n;
            this.v = new Vector3(_x, _y, _z);
        }
        public Matrix44 ToMatrix()
        {
            Matrix44 gIdentMatrix = MathConstants.gIdentMatrix44;
            gIdentMatrix.right.x = this.n * this.n + this.v.x * this.v.x - this.v.y * this.v.y - this.v.z * this.v.z;
            gIdentMatrix.right.y = 2f * this.v.x * this.v.y - 2f * this.v.z * this.n;
            gIdentMatrix.right.z = 2f * this.v.x * this.v.z + 2f * this.v.y * this.n;
            gIdentMatrix.up.x = 2f * this.v.x * this.v.y + 2f * this.v.z * this.n;
            gIdentMatrix.up.y = this.n * this.n - this.v.x * this.v.x + this.v.y * this.v.y - this.v.z * this.v.z;
            gIdentMatrix.up.z = 2f * this.v.y * this.v.z - 2f * this.v.x * this.n;
            gIdentMatrix.at.x = 2f * this.v.z * this.v.x - 2f * this.v.y * this.n;
            gIdentMatrix.at.y = 2f * this.v.z * this.v.y + 2f * this.v.x * this.n;
            gIdentMatrix.at.z = this.n * this.n - this.v.x * this.v.x - this.v.y * this.v.y + this.v.z * this.v.z;
            return gIdentMatrix;
        }
        public static Quaternion operator ~(Quaternion a)
        {
            return new Quaternion(-a.v.x, -a.v.y, -a.v.z, a.n);
        }
        public static Quaternion operator +(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.v.x + b.v.x, a.v.y + b.v.y, a.v.z + b.v.z, a.n + b.n);
        }
        public static Quaternion operator -(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.v.x - b.v.x, a.v.y - b.v.y, a.v.z - b.v.z, a.n - b.n);
        }
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.n * b.v.x + a.v.x * b.n + a.v.y * b.v.z - a.v.z * b.v.y, a.n * b.v.y + a.v.y * b.n + a.v.z * b.v.x - a.v.x * b.v.z, a.n * b.v.z + a.v.z * b.n + a.v.x * b.v.y - a.v.y * b.v.x, a.n * b.n - a.v.x * b.v.x - a.v.y * b.v.y - a.v.z * b.v.z);
        }
        public static Quaternion operator *(Quaternion a, double s)
        {
            return new Quaternion(a.v.x * s, a.v.y * s, a.v.z * s, a.n * s);
        }
        public static Quaternion operator *(double s, Quaternion a)
        {
            return new Quaternion(a.v.x * s, a.v.y * s, a.v.z * s, a.n * s);
        }
        public static Quaternion operator *(Quaternion q, Vector3 v)
        {
            return new Quaternion(q.n * v.x + q.v.y * v.z - q.v.z * v.y, q.n * v.y + q.v.z * v.x - q.v.x * v.z, q.n * v.z + q.v.x * v.y - q.v.y * v.x, -(q.v.x * v.x + q.v.y * v.y + q.v.z * v.z));
        }
        public static Quaternion operator *(Vector3 v, Quaternion q)
        {
            return new Quaternion(q.n * v.x + q.v.z * v.y - q.v.y * v.z, q.n * v.y + q.v.x * v.z - q.v.z * v.x, q.n * v.z + q.v.y * v.x - q.v.x * v.y, -(q.v.x * v.x + q.v.y * v.y + q.v.z * v.z));
        }
        public static Quaternion operator /(Quaternion q, double s)
        {
            return new Quaternion(q.v.x / s, q.v.y / s, q.v.z / s, q.n / s);
        }
        public static Quaternion operator /(double s, Quaternion q)
        {
            return new Quaternion(q.v.x / s, q.v.y / s, q.v.z / s, q.n / s);
        }
        public static double GetAngle(Quaternion q)
        {
            return (double)(2.0 * Math.Acos((double)q.n));
        }
        public static Vector3 GetAxis(Quaternion q)
        {
            Vector3 vec = q.v;
            double num = vec.Length();
            if (num <= 0.0001f)
            {
                return new Vector3(0f, 0f, 0f);
            }
            return vec / num;
        }
        public static Quaternion Rotate(Quaternion q1, Quaternion q2)
        {
            return q1 * q2 * ~q1;
        }
        public static Vector3 VRotate(Quaternion q, Vector3 v)
        {
            return (q * v * ~q).v;
        }
        public static Quaternion MakeFromEulerAngles(double x, double y, double z)
        {
            double num = (double)x;
            double num2 = (double)y;
            double num3 = (double)z;
            double num4 = Math.Cos(0.5 * num3);
            double num5 = Math.Cos(0.5 * num2);
            double num6 = Math.Cos(0.5 * num);
            double num7 = Math.Sin(0.5 * num3);
            double num8 = Math.Sin(0.5 * num2);
            double num9 = Math.Sin(0.5 * num);
            double num10 = num4 * num5;
            double num11 = num7 * num8;
            double num12 = num4 * num8;
            double num13 = num7 * num5;
            Quaternion result = new Quaternion((double)(num10 * num9 - num11 * num6), (double)(num12 * num6 + num13 * num9), (double)(num13 * num6 - num12 * num9), (double)(num10 * num6 + num11 * num9));
            return result;
        }
        public static Quaternion MakeFromForwardVector(Vector3 forward)
        {
            forward = forward.Normalize();
            Vector3 unitY = Vector3.UnitY;
            Vector3 b = Vector3.CrossProduct(unitY, forward);
            Vector3 vector = Vector3.CrossProduct(forward, b);
            Matrix44 xf;
            xf.right = new Vector4(b);
            xf.up = new Vector4(vector);
            xf.at = new Vector4(forward);
            xf.pos = new Vector4(0f, 0f, 0f, 1f);
            return Quaternion.MakeFromMatrix44(xf);
        }
        public static Quaternion MakeFromMatrix44(Matrix44 xf)
        {
            double x = 0f;
            double y = 0f;
            double z = 0f;
            double num = 0f;
            double num2 = xf.right.x + xf.up.y + xf.at.z;
            if (num2 >= 0f)
            {
                double num3 = (double)Math.Sqrt((double)(num2 + 1f));
                num = 0.5f * num3;
                num3 = 0.5f / num3;
                x = (xf.up.z - xf.at.y) * num3;
                y = (xf.at.x - xf.right.z) * num3;
                z = (xf.right.y - xf.up.x) * num3;
            }
            else
            {
                int num4 = 0;
                if (xf.up.y > xf.right.x)
                {
                    num4 = 1;
                    if (xf.at.z > xf.up.y)
                    {
                        num4 = 2;
                    }
                }
                else
                {
                    if (xf.at.z > xf.right.x)
                    {
                        num4 = 2;
                    }
                }
                switch (num4)
                {
                    case 0:
                        {
                            double num3 = (double)Math.Sqrt((double)(xf.right.x - (xf.up.y + xf.at.z) + 1f));
                            x = 0.5f * num3;
                            num3 = 0.5f / num3;
                            y = (xf.up.x + xf.right.y) * num3;
                            z = (xf.right.z + xf.at.x) * num3;
                            num = (xf.up.z - xf.at.y) * num3;
                            break;
                        }
                    case 1:
                        {
                            double num3 = (double)Math.Sqrt((double)(xf.up.y - (xf.at.z + xf.right.x) + 1f));
                            y = 0.5f * num3;
                            num3 = 0.5f / num3;
                            z = (xf.at.y + xf.up.z) * num3;
                            x = (xf.up.x + xf.right.y) * num3;
                            num = (xf.at.x - xf.right.z) * num3;
                            break;
                        }
                    case 2:
                        {
                            double num3 = (double)Math.Sqrt((double)(xf.at.z - (xf.right.x + xf.up.y) + 1f));
                            z = 0.5f * num3;
                            num3 = 0.5f / num3;
                            x = (xf.right.z + xf.at.x) * num3;
                            y = (xf.at.y + xf.up.z) * num3;
                            num = (xf.right.y - xf.up.x) * num3;
                            break;
                        }
                }
            }
            return new Quaternion(x, y, z, num);
        }

        public Quaternion Normalize()
        {
            if (Scaler < 0)
                return new Quaternion(-this.v.x, -this.v.y, -this.v.z, -this.Scaler);
            else
                return new Quaternion(this.v.x, this.v.y, this.v.z, this.Scaler);
        }

        public override string ToString()
        {
            return string.Format("V:{0} M:{1}", Vector, Scaler);
        }

    }
}