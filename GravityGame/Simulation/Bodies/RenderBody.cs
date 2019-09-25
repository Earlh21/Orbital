using System;
using System.Text;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class RenderBody : Body, Drawable
    {
        private CircleShape shape;
        protected virtual Shader Shader => null;

        public bool DrawOutline { get; set; } = false;
        public virtual Color? OutlineColor => null;

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

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            RenderWindow window = (RenderWindow) target;
            View view = window.GetView();
            
            UpdateGraphic();
            shape.SetPointCount((uint)Mathf.Clamp(10, 80, 18 + shape.Radius * (window.Size.X / view.Size.X) / 3.5f));

            if (Shader == null)
            {
                target.Draw(shape);
            }
            else
            {
                target.Draw(shape, new RenderStates(Shader));
            }
                        
            if (DrawOutline)
            {
                shape.Radius = 12.0f / Program.ViewScale;
                shape.Position = new Vector2f(Position.X - shape.Radius, -Position.Y - shape.Radius);
                shape.OutlineColor = OutlineColor == null ? Color.Green : (Color) OutlineColor;
                shape.FillColor = Color.Transparent;
                shape.SetPointCount(12);
                shape.OutlineThickness = 3.5f / Program.ViewScale;
                
                target.Draw(shape);
            }
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
            shape.OutlineThickness = 0.0f;
        }

        protected virtual Color GetColor()
        {
            if (IsSelected)
            {
                return new Color(200, 0, 200, 255);
            }
            return Color.Green;
        }
    }
}