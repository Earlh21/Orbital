using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using SFML.Graphics;

namespace GravityGame
{
    public class Gradient
    {
        public List<GradientKey> Keys
        {
            private get => Keys;
            set
            {
                Keys = value;
                Keys.Sort();
            }
        }

        public Gradient()
        {
            Keys = new List<GradientKey>();
        }

        public Gradient(List<GradientKey> keys)
        {
            Keys = keys;
        }

        public Color GetColor(float value)
        {
            if (Keys.Count == 0)
            {
                throw new ArgumentException("Keys is empty.");
            }

            if (Keys.Count == 1)
            {
                return Keys[0].Colorf.ToColor();
            }
            
            if (value < Keys[0].T || value > Keys[Keys.Count - 1].T)
            {
                throw new ArgumentException("Value exceeds gradient range.");
            }

            for (int i = 0; i < Keys.Count; i++)
            {
                if (value > Keys[i].T && value < Keys[i + 1].T)
                {
                    return Interpolate(Keys[i], Keys[i + 1], value).ToColor();
                }
            }
            
            throw new Exception("Something has gone very wrong. This shouldn't be reached.");
        }

        private Colorf Interpolate(GradientKey a, GradientKey b, float value)
        {
            float range = b.T - a.T;

            float a_close = 1 - (value - a.T) / range;
            float b_close = 1 - (b.T - value) / range;

            return a.Colorf * a_close + b.Colorf * b_close;
        }
    }
}