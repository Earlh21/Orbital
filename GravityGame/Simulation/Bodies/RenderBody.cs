using System;
using System.Runtime.CompilerServices;
using System.Text;
using GravityGame.Extension;
using GravityGame.Graphics;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class RenderBody : Body, Drawable
    {
        protected Texture texture;
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
            Sprite sprite = new Sprite(texture);
            sprite.Position = Position.InvY() - new Vector2f(Radius, Radius);
            
            target.Draw(sprite, new RenderStates(GetShader()));
            
            //TODO: Put outline drawing back
        }

        protected virtual Color GetColor()
        {
            if (IsSelected)
            {
                return new Color(200, 0, 200, 255);
            }
            return Color.Green;
        }

        protected virtual Shader GetShader()
        {
            CircleShader.Load(texture, Colorf.FromColor(GetColor()));
            return CircleShader.Shader;
        }

        protected override void OnRadiusChange()
        {
            base.OnRadiusChange();
            uint size = (uint) (Radius * 2);
            texture = new Texture(size, size);
        }
    }
}