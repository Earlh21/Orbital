using System;
using System.Runtime.CompilerServices;
using SFML.System;

namespace GravityGame.Extension
{
    public static class Vector2fExtension
    {
        public static float LengthSquared(this Vector2f v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        public static float Length(this Vector2f v)
        {
            return Mathf.Sqrt(v.LengthSquared());
        }

        public static Vector2f Unit(this Vector2f v)
        {
            return v / v.Length();
        }

        public static Vector2f Multiply(this Vector2f v, Vector2f other)
        {
            return new Vector2f(v.X * other.X, v.Y * other.Y);
        }
        
        public static Vector2f Divide(this Vector2f v, Vector2f other)
        {
            return new Vector2f(v.X / other.X, v.Y / other.Y);
        }
        
        public static Vector2i Floor(this Vector2f v)
        {
            return new Vector2i((int)v.X, (int)v.Y);
        }

        public static Vector2f InvY(this Vector2f v)
        {
            return new Vector2f(v.X, -v.Y);
        }

        public static float Dot(this Vector2f a, Vector2f b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
    }
}