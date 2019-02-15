using System;
using SFML.Graphics;

namespace GravityGame
{
    public class GradientKey : IComparable
    {
        public int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(GradientKey))
            {
                throw new ArgumentException();
            }

            GradientKey other = (GradientKey) obj;
            return T.CompareTo(other.T);
        }

        public float T { get; set; }
        public Colorf Colorf { get; set; }

        public GradientKey(float t, Colorf colorf)
        {
            T = t;
            Colorf = colorf;
        }
        
        
    }
}