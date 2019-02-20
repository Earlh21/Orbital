using System;
using SFML.System;

namespace GravityGame
{
    public static class Mathf
    {
        public static float PI = (float) Math.PI;
        public static float AmbientTemp = 2.73f;
        public static float Insulation = 10.0f;
        public static float G = 360.0f;
        public static Gradient TemperatureColorGradient;
        //Amount of momentum converted into heat on collision
        public static float HeatRatio = 0.1f;
        
        public static float Pow(float val, float exponent)
        {
            return (float) Math.Pow(val, exponent);
        }

        public static float Sqrt(float val)
        {
            return (float) Math.Sqrt(val);
        }

        public static float Clamp(float a, float b, float t)
        {
            if (t < a)
            {
                return a;
            }

            if (t > b)
            {
                return b;
            }

            return t;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        public static Vector2f Lerp(Vector2f a, Vector2f b, float t)
        {
            return a + t * (b - a);
        }
    }
}