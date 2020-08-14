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

            //Will probably need this for debugging later
            /**if (this is Planet planet)
            {
                CircleShape circle = new CircleShape();
                circle.Radius = 30;
                circle.SetPointCount(8);

                Color color;
                if (planet.Type == Planet.PlanetType.Gas)
                {
                    color = Color.Green;
                }
                else if (planet.Type == Planet.PlanetType.Rocky)
                {
                    color = Color.Yellow;
                }
                else
                {
                    color = Color.Blue;
                }

                circle.FillColor = color;
                circle.Position = (Position - new Vector2f(circle.Radius, circle.Radius)).InvY();
				
                target.Draw(circle);
            }**/
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