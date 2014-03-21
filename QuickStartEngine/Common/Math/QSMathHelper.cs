/*
 * QSMathHelper.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace QuickStart.Mathmatics
{
    /// <summary>
    /// Class for helping with common math functions
    /// </summary>
    public static class QSMath
    {
        /// <summary>
        /// Used for creating random numbers
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Returns a direction vector from one point to another.
        /// </summary>
        /// <param name="posFirst">First position</param>
        /// <param name="posSecond">Second position</param>
        /// <returns>Direction vector from first position to second position</returns>
        public static Vector3 DirectionFirstToSecond(Vector3 posFirst, Vector3 posSecond)
        {
            return Vector3.Normalize(posSecond - posFirst);
        }

        /// <summary>
        /// Finds the distance between two points in 3D space.
        /// </summary>
        /// <param name="posFirst">First position</param>
        /// <param name="posSecond">Second position</param>
        /// <returns>Distance between the two positions.</returns>
        public static float DistancePointToPoint(Vector3 posFirst, Vector3 posSecond)
        {
            Vector3 diffVect = posFirst - posSecond;
            return diffVect.Length();
        }

        /// <summary>
        /// Finds the distance between two points in 3D space.
        /// </summary>
        /// <param name="posFirst">First position</param>
        /// <param name="posSecond">Second position</param>
        /// <returns>Distance between the two positions.</returns>
        public static float DistancePointToPoint(Vector3 posFirst, ref Vector3 posSecond)
        {
            Vector3 diffVect = posFirst - posSecond;
            return diffVect.Length();
        }

        /// <summary>
        /// Finds the distance between two points in 3D space.
        /// </summary>
        /// <param name="posFirst">First position</param>
        /// <param name="posSecond">Second position</param>
        /// <returns>Distance between the two positions.</returns>
        public static float DistancePointToPoint(ref Vector3 posFirst, ref Vector3 posSecond)
        {
            Vector3 diffVect = posFirst - posSecond;
            return diffVect.Length();
        }

        /// <summary>
        /// Checks if a integer is a power-of-two value (i.e. 2, 4, 8, 16, etc...)
        /// </summary>
        /// <param name="Value">Value to check for power-of-two</param>
        /// <returns>True if number is a power-of-two, otherwise returns false</returns>
        public static bool IsPowerOfTwo(int Value)
        {
            if (Value < 2)
            {
                return false;
            }

            if ((Value & (Value - 1)) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a number between two values.
        /// </summary>
        /// <param name="min">Lower bound value</param>
        /// <param name="max">Upper bound value</param>
        /// <returns>Random number between bounds.</returns>
        public static float RandomBetween(double min, double max)
        {
            return (float)(min + (float)random.NextDouble() * (max - min));
        }

        /// <summary>
        /// 50/50 chance of returning either -1 or 1
        /// </summary>
        public static int Random5050
        {
            get
            {
                if (RandomBetween(0, 2) >= 1)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Helper function chooses a random location on a triangle between three positions.
        /// </summary>
        /// <param name="position1">A point representing a corner of a triangle</param>
        /// <param name="position2">A point representing a corner of a triangle</param>
        /// <param name="position3">A point representing a corner of a triangle</param>
        /// <param name="randomPosition">Vector returning a random position on the triangle.</param>
        public static void PickRandomPoint(Vector3 position1, Vector3 position2, Vector3 position3,
                                           out Vector3 randomPosition)
        {
            float a = (float)random.NextDouble();
            float b = (float)random.NextDouble();

            if ((a + b) > 1)
            {
                a = 1 - a;
                b = 1 - b;
            }

            randomPosition = Vector3.Barycentric(position1, position2, position3, a, b);
        }

        /// <summary>
        /// Check if a point lies inside a <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="point">3D Point</param>
        /// <param name="box">Bounding box</param>
        /// <returns>True if point lies inside the bounding box</returns>
        public static bool PointInsideBoundingBox(Vector3 point, BoundingBox box)
        {
            if (point.X < box.Min.X)
            {
                return false;
            }

            if (point.Y < box.Min.Y)
            {
                return false;
            }

            if (point.Z < box.Min.Z)
            {
                return false;
            }

            if (point.X > box.Max.X)
            {
                return false;
            }

            if (point.Y > box.Max.Y)
            {
                return false;
            }

            if (point.Z > box.Max.Z)
            {
                return false;
            }

            // Point must be inside box
            return true;
        }

        /// <summary>
        /// Check if a point lies inside a conical region. Good for checking if a point lies in something's
        /// field-of-view cone.
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <param name="coneOrigin">Cone's origin</param>
        /// <param name="coneDirection">Cone's forward direction</param>
        /// <param name="coneAngle">Cone's theta angle (radians)</param>
        /// <returns>True if point is inside the conical region</returns>
        public static bool PointInsideCone(Vector3 point, Vector3 coneOrigin, Vector3 coneDirection, double coneAngle)
        {
            Vector3 tempVect = Vector3.Normalize(point - coneOrigin);

            return Vector3.Dot(coneDirection, tempVect) >= Math.Cos(coneAngle);
        }

        /// <summary>
        /// Check if a point lies inside of a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="point">3D Point</param>
        /// <param name="sphere">Sphere to check against</param>
        /// <returns>True if point is inside of the sphere</returns>
        public static bool PointInsideBoundingSphere(Vector3 point, BoundingSphere sphere)
        {
            Vector3 diffVect = point - sphere.Center;

            return diffVect.Length() < sphere.Radius;
        }

        /// <summary>
        /// Check if a point lies in a sphere. Good for checking is a point lies within a specific
        /// distance of another point, like proximity checking.
        /// </summary>
        /// <param name="point">3D Point</param>
        /// <param name="sphereCenter">Sphere's center</param>
        /// <param name="sphereRadius">Sphere's radius</param>
        /// <returns>True if point is inside of the sphere</returns>
        public static bool PointInsideSphere(Vector3 point, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 diffVect = point - sphereCenter;

            return diffVect.Length() < sphereRadius;
        }

        /// <summary>
        /// Find the angle between two vectors. This will not only give the angle difference, but the direction.
        /// For example, it may give you -1 radian, or 1 radian, depending on the direction. Angle given will be the 
        /// angle from the FromVector to the DestVector, in radians.
        /// </summary>
        /// <param name="FromVector">Vector to start at.</param>
        /// <param name="DestVector">Destination vector.</param>
        /// <param name="DestVectorsRight">Right vector of the destination vector</param>
        /// <returns>Signed angle, in radians</returns>  
        /// <remarks>All three vectors must lie along the same plane.</remarks>
        public static double GetSignedAngleBetween2DVectors(Vector3 FromVector, Vector3 DestVector, Vector3 DestVectorsRight)
        {
            FromVector.Normalize();
            DestVector.Normalize();
            DestVectorsRight.Normalize();

            float forwardDot = Vector3.Dot(FromVector, DestVector);
            float rightDot = Vector3.Dot(FromVector, DestVectorsRight);

            // Keep dot in range to prevent rounding errors (may not be needed, should be tested for)
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            double angleBetween = Math.Acos((float)forwardDot);

            if (rightDot < 0.0f)
            {
                angleBetween *= -1.0f;
            }

            return angleBetween;
        }

        /// <summary>
        /// Translates a point around an origin
        /// </summary>
        /// <param name="point">Point that is going to be translated</param>
        /// <param name="originPoint">Origin of rotation</param>
        /// <param name="rotationAxis">Axis to rotate around</param>
        /// <param name="radiansToRotate">Radians to rotate</param>
        /// <returns>Translated point</returns>
        public static Vector3 RotateAroundPoint(Vector3 point, Vector3 originPoint, Vector3 rotationAxis, float radiansToRotate)
        {
            Vector3 rotatedVect = Vector3.Transform(point, Matrix.CreateFromAxisAngle(rotationAxis, radiansToRotate));

            rotatedVect -= originPoint;

            return rotatedVect;
        }

        /// <summary>
        /// Returns velocity of deflection
        /// </summary>
        /// <param name="CurrentVelocity">Velocity vector if item to be bounced</param>
        /// <param name="Elasticity">Elasticity of item to be bounced</param>
        /// <param name="CollisionNormal">Normal vector of plane the item is bouncing off of</param>
        /// <returns>Velocity vector of deflection</returns>
        public static Vector3 CalculateDeflection(Vector3 CurrentVelocity, float Elasticity, Vector3 CollisionNormal)
        {
            return Vector3.Reflect(CurrentVelocity, CollisionNormal) * Elasticity;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static void Project(Vector3 from, Vector3 to, out Vector3 result)
        {
            float dot;
            Vector3.Dot(ref from, ref to, out dot);

            result = (dot / to.LengthSquared()) * to;
        }

        public static T Min<T>(T value1, T value2) where T : System.IComparable
        {
            return (Comparer<T>.Default.Compare(value1, value2) > 0) ? value2 : value1;
        }

        public static T Max<T>(T value1, T value2) where T : System.IComparable
        {
            return (Comparer<T>.Default.Compare(value1, value2) > 0) ? value1 : value2;
        }

        /// <summary>
        /// Unitizes a <see cref="Vector3"/> and returns its length
        /// </summary>
        /// <param name="vector">Vector3 to be unitized</param>
        /// <returns>Length of vector before it was unitized.</returns>
        public static float Unitize( this Vector3 vector )
        {
            float length = vector.Length();
            float inverseLength = 1.0f / length;
            vector.X *= inverseLength;
            vector.Y *= inverseLength;
            vector.Z *= inverseLength;
            return length;
        }
    }
}

