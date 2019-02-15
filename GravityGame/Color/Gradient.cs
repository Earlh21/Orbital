using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using SFML.Graphics;

namespace GravityGame
{
    public class Gradient
    {
        private List<GradientKey> keys;
        
        public List<GradientKey> Keys
        {
            private get => keys;
            set
            {
                keys = value;
                keys.Sort();
            }
        }

        public Gradient()
        {

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
            
            if (Keys.Count == 1 || value < Keys[0].T)
            {
                return Keys[0].Colorf.ToColor();
            }

            if (value > Keys[Keys.Count - 1].T)
            {
                return Keys[Keys.Count - 1].Colorf.ToColor();
            }

            for (int i = 0; i < Keys.Count; i++)
            {
                if (value > Keys[i].T && value < Keys[i + 1].T)
                {
                    return Interpolate(Keys[i], Keys[i + 1], value).ToColor();
                }
            }
            
            throw new Exception("Something has gone wrong. This shouldn't be reached.");
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