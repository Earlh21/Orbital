using GravityGame.Extension;
using SFML.Graphics;

namespace GravityGame
{
	public class HeatLaserEffect : Effect
	{
		private Vertex[] line_vertices = new Vertex[2];
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

		public override void Draw(RenderTarget render_target, RenderStates states)
		{
			Colorf interp = Colorf.Interpolate(colorf, new Colorf(0, 0, 0, 0), LifeTime / KillTime);

			line_vertices[0].Position = original.Position.InvY();
			line_vertices[0].Color = interp.ToColor();

			line_vertices[1].Position = target.Position.InvY();
			line_vertices[1].Color = interp.ToColor();
			
			render_target.Draw(line_vertices, PrimitiveType.Lines);
		}
	}
}