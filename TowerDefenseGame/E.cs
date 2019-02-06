using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefenseGame
{
    static class E
    {
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
    }
}
