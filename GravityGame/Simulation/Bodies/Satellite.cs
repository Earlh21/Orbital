using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
	public class Satellite : RenderBody, IDrawsText
	{
		public int Faction { get; private set; }
		public Planet Home { get; private set; }
		public float ThrusterAcceleration { get; private set; }

		public override bool DoesGravity => false;
		public bool DrawText { get; set; }

		public override Color? OutlineColor
		{
			get
			{
				Color color = Civilizations.GetColor(Faction);
				color.A = 60;
				return color;
				
			}
		}
		public override uint TexturePadding => 0;

		public Satellite(Vector2f position, Vector2f velocity, Planet home, int faction) : base(position, velocity,
			Composition.Basic(10))
		{
			Faction = faction;
			Home = home;
			ThrusterAcceleration = (float) Program.R.NextDouble() * 50 + 10.0f;
		}

		private void FormatText(Text text, float level, RenderWindow window)
		{
			View view = window.GetView();

			text.Position = Position.InvY() + new Vector2f(-Radius, Radius + level * view.Size.Y / 30);
			text.CharacterSize = 50;
			float scale = view.Size.X / window.Size.X;
			text.Scale = 0.5f * new Vector2f(scale, scale);
		}

		public override void Update(Scene scene, float time)
		{
			base.Update(scene, time);

			Vector2f force = GetForceFrom(Home);
			float acceleration = (force / Mass).Length();
			float velocity = Mathf.Sqrt(acceleration * Distance(Home));

			float angle = Mathf.AngleTo(Position, Home.Position);
			Vector2f velocity_unit =
				new Vector2f(Mathf.Cos(angle + Mathf.PI / 2), Mathf.Sin(angle + Mathf.PI / 2));
			Vector2f target_vel = Home.Velocity + velocity_unit * velocity;

			Vector2f accel_dir = target_vel - Velocity;
			Force += accel_dir.Unit() * ThrusterAcceleration * Mass;
		}

		public override void Draw(RenderTarget target, RenderStates states)
		{
			base.Draw(target, states);

			if (DrawText)
			{
				RenderWindow window = (RenderWindow) target;
				View view = window.GetView();

				Text faction_text = new Text(Civilizations.GetName(Faction), Program.Font);
				faction_text.FillColor = Civilizations.GetColor(Faction);
				FormatText(faction_text, 0, window);

				target.Draw(faction_text);
			}
		}
	}
}