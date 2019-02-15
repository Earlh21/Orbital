using System;

namespace GravityGame
{
    public static class Mathf
    {
        public static float PI = (float) Math.PI;
        public static float AmbientTemp = 2.73f;
        public static float Insulation = 15.0f;
        public static float G = 1.0f;
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
    }
}