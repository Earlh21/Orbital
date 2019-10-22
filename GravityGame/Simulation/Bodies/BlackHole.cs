using GravityGame.Extension;
using GravityGame.Graphics;
using SFML.Graphics;
using SFML.Graphics.Glsl;
using SFML.System;

namespace GravityGame
{
	public class BlackHole : RenderBody
	{
		public BlackHole(Vector2f position, Vector2f velocity, float mass) :base(position,velocity, Composition.Basic(mass))
		{
			
		}

		public override void Draw(RenderTarget target, RenderStates states)
		{
			Texture screen = new Texture(target.Size.X, target.Size.Y);
			screen.Update((RenderWindow)target);
			
			Sprite blackhole = new Sprite(screen);
			blackhole.Position = Program.ScreenPositionToWorld(new Vector2i(0, 0));
			blackhole.Scale = new Vector2f(1.0f / Program.ViewScale, 1.0f / Program.ViewScale);

			BlackHoleShader.Position = Position;
			BlackHoleShader.Lensing = Radius;
			BlackHoleShader.Load(screen);
			
			target.Draw(blackhole, new RenderStates(BlackHoleShader.Shader));
		}
	}
}