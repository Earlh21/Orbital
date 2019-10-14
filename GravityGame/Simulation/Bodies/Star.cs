using GravityGame.Extension;
using GravityGame.Graphics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
	public class Star : RenderBody
	{
		public override uint TexturePadding => 120 + (uint)(Radius);

		public Star(Vector2f position, Vector2f velocity, float mass) : base(position, velocity,
			Composition.Basic(mass))
		{
		}

		protected override Color GetColor()
		{
			if (IsSelected)
			{
				return new Color(255, 0, 255, 255);
			}

			return Color.Yellow;
		}

		protected override Shader GetShader()
		{
			StarShader.Radius = Radius;
			StarShader.DimColor = new Colorf(214f / 255f, 102f / 255f, 12f / 255f, 1.0f);
			StarShader.BrightColor = Colorf.FromColor(Color.White);
			
			StarShader.Load(texture);
			return StarShader.Shader;
		}
	}
}