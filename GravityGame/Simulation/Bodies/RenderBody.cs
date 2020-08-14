using GravityGame.Extension;
using GravityGame.Graphics;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class RenderBody : Body, Drawable
    {
        private CircleShape outline = new CircleShape();
        private Sprite sprite;
            
        private Texture texture;
        
        public bool DrawOutline { get; set; }
        public virtual Color? OutlineColor => null;
        public virtual uint TexturePadding => 45;

        protected Texture Texture
        {
            get => texture;
            set
            {
                texture = value;
                sprite = new Sprite(value);
            }
        }
        
        public RenderBody(Vector2f position, Vector2f velocity, Composition composition) : base(position,
            velocity, composition)
        {
            outline.FillColor = Color.Transparent;
            outline.SetPointCount(14);
        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            if (sprite == null)
            {
                return;
            }
            
            sprite.Position = Position.InvY() - new Vector2f(Radius + TexturePadding, Radius + TexturePadding);
            
            if (DrawOutline)
            {
                //TODO: Ships should have smaller outlines
                const float base_outline_size = 8.0f;
                const float thickness_percent = 0.14f;
                
                Color? color = OutlineColor;
                if (color == null)
                {
                    return;
                }
                
                outline.Radius = base_outline_size / Program.ViewScale;
                outline.OutlineThickness = thickness_percent * outline.Radius;
                outline.OutlineColor = color.Value;
                outline.Position = Position.InvY() - new Vector2f(outline.Radius, outline.Radius);

                target.Draw(outline);
            }
            
            target.Draw(sprite, new RenderStates(GetShader()));
        }

        protected virtual Color GetColor()
        {
            if (IsSelected)
            {
                return new Color(200, 0, 200, 255);
            }
            return Color.Green;
        }
        //TODO: Levels of detail
        protected virtual Shader GetShader()
        {
            CircleShader.Load(Texture, Colorf.FromColor(GetColor()));
            return CircleShader.Shader;
        }

        protected override void OnRadiusChange()
        {
            base.OnRadiusChange();
            uint size = (uint) (Radius * 2) + TexturePadding * 2;
            Texture = new Texture(size, size);
        }
    }
}