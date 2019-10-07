using System;
using System.Runtime.CompilerServices;
using System.Text;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class RenderBody : Body, Drawable
    {
        private Sprite graphic;
        private static Texture empty_texture;
        protected virtual Shader Shader => null;

        public bool DrawOutline { get; set; } = false;
        public virtual Color? OutlineColor => null;

        static RenderBody()
        {
            empty_texture = new Texture(1, 1);
        }
        
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
            graphic.Draw(window, states);
                        
            if (DrawOutline)
            {
                CircleShape outline = new CircleShape();
                
                outline.Radius = 12.0f / Program.ViewScale;
                outline.Position = new Vector2f(Position.X - outline.Radius, -Position.Y - outline.Radius);
                outline.OutlineColor = OutlineColor == null ? Color.Green : (Color) OutlineColor;
                outline.FillColor = Color.Transparent;
                outline.SetPointCount(12);
                outline.OutlineThickness = 3.5f / Program.ViewScale;
                
                target.Draw(graphic);
            }
        }

        private void UpdateGraphic()
        {   
            if (graphic == null)
            {
                graphic = new Sprite();
            }
            
            graphic.Scale = new Vector2f(Radius, Radius);
            graphic.Position = new Vector2f(Position.X - Radius, -Position.Y - Radius);
            graphic.Texture = empty_texture;
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