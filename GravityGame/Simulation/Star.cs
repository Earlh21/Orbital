using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class Star : RenderBody
    {
        public Star(Vector2f position, float mass, Vector2f velocity, float density) : base(position, mass,
            velocity, density)
        {
        }
        
        protected override Color GetColor()
        {
            return Color.Yellow;
        }
    }
}