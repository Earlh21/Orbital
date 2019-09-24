using System.Reflection.Emit;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
	public class ThrusterShip : Ship
	{
		private Body target;
		private bool started_burn = false;
		private float thruster_start_time = 5.0f;
		private float thruster_end_time = 18.0f;
		private float thruster_acceleration = 100.0f;
		private Vector2f thruster_force;
		
		public ThrusterShip(Vector2f position, Vector2f velocity, Life life, Body target) : base(position,  velocity, life)
		{
			this.target = target;
			float distance = Distance(target);
			thruster_start_time = distance / velocity.Length() - 1.0f;
			thruster_end_time = thruster_start_time + 5.0f;
		}

		public override void Update(Scene scene, float time)
		{
			base.Update(scene, time);
			if (!started_burn && LifeTime > thruster_start_time)
			{
				started_burn = false;
				
			}

			if (LifeTime > thruster_start_time && LifeTime < thruster_end_time)
			{
				Vector2f target_pos = target.Position + target.Velocity * 1.5f;
				Vector2f acceleration_unit = (target_pos - Position).Unit();
				thruster_force = acceleration_unit * Mass * thruster_acceleration;
				Force += thruster_force;
			}
		}

		public override void Draw(RenderTarget target, RenderStates states)
		{
			Vector2f target_pos = this.target.Position + this.target.Velocity * 1.5f;

			Vertex[] line = new[] {new Vertex( Position.InvY()),new Vertex(target_pos.InvY())};

			Color color = LifeTime > thruster_start_time && LifeTime < thruster_end_time ? Color.Yellow : Color.White;
			line[0].Color = color;
			line[1].Color = color;
			
			target.Draw(line, PrimitiveType.Lines);
			
			base.Draw(target, states);
			
			CircleShape shape = new CircleShape();

			shape.Radius = this.target.Radius * 1.2f;
			shape.Position = new Vector2f(target_pos.X - shape.Radius, -target_pos.Y - shape.Radius);
			shape.FillColor = Color.Transparent;
			shape.OutlineColor = Color.Yellow;
			shape.OutlineThickness = 5.0f;
			
			target.Draw(shape);
		}
	}
}