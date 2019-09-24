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
		private const float thruster_start_time = 5.0f;
		private const float thruster_end_time = 20.0f;
		private const float thruster_acceleration = 30.0f;
		private Vector2f thruster_force;
		
		public ThrusterShip(Vector2f position, Vector2f velocity, Life life, Body target) : base(position,  velocity, life)
		{
			this.target = target;
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
				Vector2f target_pos = Position + target.Velocity * 3.0f;
				Vector2f acceleration_unit = (target_pos - Position).Unit();
				thruster_force = acceleration_unit * Mass * thruster_acceleration;
				Force += thruster_force;
			}
		}

		public override void Draw(RenderTarget target, RenderStates states)
		{
			base.Draw(target, states);
			
			Vector2f target_pos = Position + this.target.Velocity * 3.0f;
			
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