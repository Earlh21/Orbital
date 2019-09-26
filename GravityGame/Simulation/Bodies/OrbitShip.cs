using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
	public class OrbitShip : Ship
	{
		private Body target;
		private float orbit_time = 1000;
		private float close_time = 1000;
		private float orbit2_time = 1000;
		private float deorbit_time = 1000;
		private float stop_time = 1000;
		private float start_radius = 300;
		private bool begun = false;
		private float thruster_acceleration = 80.0f;
		private float orbit_acceleration = 130.0f;
		
		public OrbitShip(Vector2f position, Vector2f velocity, Life life, Body target) : base(position,  velocity, life)
		{
			this.target = target;
			float distance = Distance(target);

		}
		
		public override void Update(Scene scene, float time)
		{
			base.Update(scene, time);

			if (!begun)
			{
				if (start_radius * start_radius > DistanceSquared(target))
				{
					orbit_time = LifeTime;
					close_time = orbit_time + 2.0f;
					orbit2_time = close_time + 1.5f;
					deorbit_time = orbit2_time + 1.5f;
					stop_time = deorbit_time + 1.0f;

					begun = true;
				}
			}
			
			if ((LifeTime > orbit_time && LifeTime < close_time) || (LifeTime > orbit2_time && LifeTime < deorbit_time))
			{
				Vector2f force = GetForceFrom(target);
				float acceleration = (force / Mass).Length();
				float velocity = Mathf.Sqrt(acceleration * Distance(target));

				float angle = Mathf.AngleTo(Position, target.Position);
				Vector2f velocity_unit = new Vector2f(Mathf.Cos(angle + Mathf.PI / 2), (float)Mathf.Sin(angle + Mathf.PI / 2));
				Vector2f target_vel = target.Velocity + velocity_unit * velocity;

				Vector2f accel_dir = target_vel - Velocity;
				Force += accel_dir.Unit() * orbit_acceleration * Mass;
			}

			if (LifeTime > close_time && LifeTime < orbit2_time)
			{
				Vector2f target_pos = target.Position + target.Velocity * time * 4;
				Vector2f acceleration_unit = (target_pos - Position).Unit();
				Vector2f thruster_force = acceleration_unit * Mass * thruster_acceleration;
				Force += thruster_force;
			}
			
			if (LifeTime > deorbit_time && LifeTime < stop_time)
			{
				Vector2f acceleration_unit = (target.Velocity - Velocity).Unit();
				Vector2f thruster_force = acceleration_unit * Mass * thruster_acceleration;
				Force += thruster_force;
			}
		}
	}
}