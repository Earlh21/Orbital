using System;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
	public class LaserSatellite : Satellite
	{
		private float laser_chance = 1 / 4.0f;
		
		public LaserSatellite(Vector2f position, Vector2f velocity, Planet home, int faction) : base(position, velocity, home, faction)
		{
		}
		
		private void FireLaser(Scene scene, Planet target)
		{
			float heat_change_sign = Mathf.Sign(target.Temperature - target.Life.NormalTemp);
			Color color;
			float heat_change;

			if (heat_change_sign > 0 || target.Life.TechLevel > 2)
			{
				color = Color.Red;
				heat_change = 5000 + Mathf.Pow(target.Life.TechLevel, 1.8f) * 1250;
			}
			else
			{
				color = Color.Blue;
				heat_change = -500 - target.Life.TechLevel * 50;
			}
            
			HeatLaserEffect laser = new HeatLaserEffect(this, target, color, heat_change, 3.0f);
			scene.AddEffect(laser);
		}

		public override void Update(Scene scene, float time)
		{
			base.Update(scene, time);
			
			if (Program.R.NextDouble() < 1 - Math.Pow(1 - laser_chance, time))
			{
				Body[] buffer = new Body[5];
				int height = 1;
				BodyFilter filter = new BodyFilter(typeof(Planet), BodyFilter.LifeFilter.True, Faction, false);
                    
				int count = scene.QuadTree.FindNearbyBodies(Position, height, filter, buffer);

				if (count < 1)
				{
					return;
				}
                    
				int index = Program.R.Next(count);
				Body target = buffer[index];
                    
				FireLaser(scene, (Planet)target);
			}

			if (!Home.Exists || !Home.HasLife)
			{
				Exists = false;
			}
		}
	}
}