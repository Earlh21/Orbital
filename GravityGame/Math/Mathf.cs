using System;
using SFML.System;

namespace GravityGame
{
    public static class Mathf
    {
        public static float PI = (float) Math.PI;
        public static float AmbientTemp = 2.73f;
        public static float Insulation = 150.0f;
        public static float G = 360.0f;
        public static Gradient TemperatureColorGradient;
        //Amount of momentum converted into heat on collision
        public static float HeatRatio = 0.1f;
        
        public static float Pow(float val, float exponent)
        {
            return (float) Math.Pow(val, exponent);
        }

        public static float Round(float a)
        {
            return (float) Math.Round(a);
        }

        public static float Sign(float a)
        {
            if (a == 0)
            {
                return 0;
            }

            return a > 0 ? 1 : -1;
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

        public static float Log(float val, float log_base = 10.0f)
        {
            return (float) Math.Log(val, log_base);
        }
        
        public static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        public static float InvLerp(float a, float b, float c)
        {
            return (c - a) / (b - a);
        }

        public static float InvLerp2(float start, float middle, float end, float value)
        {
            if (value < middle)
            {
                return InvLerp(start, middle, value) * 0.5f;
            }
            else
            {
                return InvLerp(middle, end, value) * 0.5f + 0.5f;
            }
        }

        public static float Cos(float value)
        {
            return (float) Math.Cos(value);
        }

        public static float Sin(float value)
        {
            return (float) Math.Sin(value);
        }

        public static float Acos(float value)
        {
            return (float) Math.Acos(value);
        }
        
        public static Vector2f Lerp(Vector2f a, Vector2f b, float t)
        {
            return a + t * (b - a);
        }

        public static float AngleTo(Vector2f a, Vector2f b)
        {
            return Mathf.Atan2(b.Y - a.Y, b.X - a.X);
        }

        public static float Atan2(float y, float x)
        {
            return (float) Math.Atan2(y, x);
        }

        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }
    }
}