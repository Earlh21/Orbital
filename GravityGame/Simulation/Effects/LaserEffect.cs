using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
	public class LaserEffect : Effect
	{
		private Vector2f start;
		private Vector2f end;
		private Colorf colorf;

		public Color Color
		{
			get => colorf.ToColor();
			set => colorf = Colorf.FromColor(value);
		}

		public LaserEffect(Vector2f start, Vector2f end, Color color, float time)
		{
			this.start = start;
			this.end = end;
			Color = color;
			KillTime = time;
		}

		public override void Draw(RenderTarget target, RenderStates states)
		{
			Colorf interp = Colorf.Interpolate(colorf, new Colorf(0, 0, 0, 0), LifeTime / KillTime);
			
			Vertex[] vertices = new[] {new Vertex(start.InvY(), interp.ToColor()), new Vertex(end.InvY(), interp.ToColor())};
			target.Draw(vertices, PrimitiveType.Lines);
		}
	}
}