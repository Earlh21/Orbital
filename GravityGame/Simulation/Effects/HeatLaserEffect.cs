using GravityGame.Extension;
using SFML.Graphics;

namespace GravityGame
{
	public class HeatLaserEffect : Effect
	{
		private Body original;
		private Body target;
		private Colorf colorf;
		private float heat_change;

		public Color Color
		{
			get => colorf.ToColor();
			set => colorf = Colorf.FromColor(value);
		}

		public HeatLaserEffect(Body original, Body target, Color color, float heat_change, float time)
		{
			this.original = original;
			this.target = target;
			this.heat_change = heat_change;
			Color = color;
			KillTime = time;
		}

		public override void Update(float time)
		{
			base.Update(time);

			if (target is TemperatureBody temperature_body)
			{
				temperature_body.Heat += heat_change * time;
			}
		}

		public override void Draw(RenderTarget target, RenderStates states)
		{
			Colorf interp = Colorf.Interpolate(colorf, new Colorf(0, 0, 0, 0), LifeTime / KillTime);

			Vertex a = new Vertex(original.Position.InvY(), interp.ToColor());
			Vertex b = new Vertex(this.target.Position.InvY(), interp.ToColor());
			Vertex[] vertices = {a, b};
			target.Draw(vertices, PrimitiveType.Lines);
		}
	}
}