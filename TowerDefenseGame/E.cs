using SFML.System;
using SharpNav.Geometry;
using System;
using System.Collections.Generic;

namespace TowerDefenseGame
{
    internal static class E
    {
        public static float ManhattanDistance(Vector2f a, Vector2f b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public static float Scale(float x, float y, float speed)
        {
            return (float)Math.Sqrt((Math.Pow(speed, 2) / ((Math.Pow(x, 2) + Math.Pow(y, 2)))));
        }

        public static float Distance(Vector2f a, Vector2f b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static float Distance(Entity a, Entity b)
        {
            return Distance(a.position, b.position);
        }

        public static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            else
            {
                return minA - maxB;
            }
        }

        public static Vector2f Normalize(Vector2f source)
        {
            float length = (float)Math.Sqrt((source.X * source.X) + (source.Y * source.Y));
            if (length != 0)
                return new Vector2f(source.X / length, source.Y / length);
            else
                return source;
        }

        public static float DotProduct(Vector2f a, Vector2f b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static void ProjectPolygon(Vector2f axis, List<Vector2f> polygon,
                   ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            float dotProduct = E.DotProduct(axis, polygon[0]);
            min = dotProduct;
            max = dotProduct;
            for (int i = 0; i < polygon.Count; i++)
            {
                dotProduct = E.DotProduct(polygon[i], axis);
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else if (dotProduct > max)
                {
                    max = dotProduct;
                }
            }
        }

        internal static float ManhattanDistance(Entity e1, Entity e2)
        {
            return ManhattanDistance(e1.position, e2.position);
        }

        internal static Vector3 ToV3(this Vector2f vector)
        {
            return new Vector3(vector.X, 1, vector.Y);
        }

        internal static bool AlmostEqual(ref Vector3 a, ref Vector3 b)
        {
            float threshold = (1.0f / 16384.0f);
            return AlmostEqual(ref a, ref b, threshold);
        }

        internal static bool AlmostEqual(ref Vector3 a, ref Vector3 b, float threshold)
        {
            float distSq = (b - a).LengthSquared();

            return distSq < threshold;
        }

        internal static bool InRange(Vector3 v1, Vector3 v2, float r, float h)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;
            return (dx * dx + dz * dz) < (r * r) && Math.Abs(dy) < h;
        }

        internal static void VMad(ref Vector3 dest, Vector3 v1, Vector3 v2, float s)
        {
            dest.X = v1.X + v2.X * s;
            dest.Y = v1.Y + v2.Y * s;
            dest.Z = v1.Z + v2.Z * s;
        }

        internal static Vector2f ToV2(this Vector3 v)
        {
            return new Vector2f(v.X, v.Z);
        }

        internal static float Distance(Vector3 position, Vector3 center)
        {
            return (float)Math.Sqrt(Math.Pow(position.X - center.X, 2) + Math.Pow(position.Y - center.Y, 2));
        }
    }
}