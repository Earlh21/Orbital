using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class RenderBody : Body, Drawable
    {
        private CircleShape shape;

        public RenderBody() : base()
        {
        }
        
        public RenderBody(Vector2f position, float mass) : this(position, mass, new Vector2f(0, 0), 1)
        {
            
        }
        
        public RenderBody(Vector2f position, float mass, Vector2f velocity) : this(position, mass, velocity, 1)
        {
            
        }
        
        public RenderBody(Vector2f position, float mass, Vector2f velocity, float density) : base(position, mass,
            velocity, density)
        {
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(shape);
        }

        public override void Update(float time)
        {
            base.Update(time);
            
            UpdateGraphic();
        }

        private void UpdateGraphic()
        {
            if (shape == null)
            {
                shape = new CircleShape();
            }
            
            shape.Radius = Radius;
            shape.Position = new Vector2f(Position.X - Radius, -Position.Y - Radius);
            Color color = GetColor();
            shape.FillColor = color;
            shape.OutlineColor = color;
        }

        protected virtual Color GetColor()
        {
            if (IsSelected)
            {
                return Color.Yellow;
            }
            return Color.Green;
        }
    }
}