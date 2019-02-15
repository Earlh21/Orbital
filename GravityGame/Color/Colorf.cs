using System.Data.Common;
using SFML.Graphics;

namespace GravityGame
{
    public class Colorf
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public Color ToColor()
        {
            return new Color((byte)(R * 255), (byte)(G * 255),(byte)(B * 255),(byte)(A * 255));
        }

        public Colorf()
        {
            R = 0;
            G = 0;
            B = 0;
            A = 0;
        }

        public Colorf(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Colorf operator +(Colorf a, Colorf b)
        {
            return new Colorf(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
        }

        public static Colorf operator *(Colorf a, float b)
        {
            return new Colorf(a.R * b, a.G * b, a.B * b, a.A * b);
        }
    }
}