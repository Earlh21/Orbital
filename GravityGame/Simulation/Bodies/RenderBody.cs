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
        public virtual uint TexturePadding => 45;
        
        public RenderBody(Vector2f position, Vector2f velocity, Composition composition) : base(position,
            velocity, composition)
        {
            
        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {            
            Sprite sprite = new Sprite(texture);
            sprite.Position = Position.InvY() - new Vector2f(Radius + TexturePadding, Radius + TexturePadding);
            
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
            uint size = (uint) (Radius * 2) + TexturePadding * 2;
            texture = new Texture(size, size);
        }
    }
}