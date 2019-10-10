using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
	public class Star : RenderBody
	{
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
	}
}