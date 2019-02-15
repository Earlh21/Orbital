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
    }
}