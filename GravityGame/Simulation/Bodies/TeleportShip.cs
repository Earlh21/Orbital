using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
	public class TeleportShip : Ship
	{
		private Body target;
		private bool teleported;
		private float distance_from_target = 100.0f;
		private float max_teleport_distance = 800.0f;
		
		public TeleportShip(Vector2f position, Vector2f velocity, Life life, Body target) : base(position,  velocity, life)
		{
			this.target = target;
		}

		public override void Update(Scene scene, float time)
		{
			base.Update(scene, time);

			if (LifeTime > 1.0f && !teleported && DistanceSquared(target) < max_teleport_distance * max_teleport_distance)
			{
				teleported = true;
				
				Vector2f old_pos = Position;
				float vel_angle = Mathf.AngleTo(new Vector2f(0, 0), Velocity);
				float teleport_angle = vel_angle + Mathf.PI;

				Vector2f target_pos = target.Position;
				Vector2f teleport_angle_vector = new Vector2f(Mathf.Cos(teleport_angle), Mathf.Sin(teleport_angle));
				Vector2f teleport_pos = target_pos + (target.Radius + distance_from_target) * teleport_angle_vector;

				Translate(teleport_pos - Position);
				Momentum = target.Velocity * Mass;
				
				LaserEffect laser = new LaserEffect(old_pos, teleport_pos, Color.Cyan, 1.0f);
				scene.AddEffect(laser);
			}
		}
	}
}