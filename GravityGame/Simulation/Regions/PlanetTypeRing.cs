using System.Collections.Generic;
using System.Dynamic;
using SFML.System;

namespace GravityGame
{
	public class PlanetTypeRing
	{
		private float Radius { get; }
		private float AngleOffset { get; set; } = 0;
		private List<PlanetTypeRegion> Types { get; }

		public PlanetTypeRing(List<Planet.PlanetType> types, float radius)
		{
			Types = new List<PlanetTypeRegion>();
			for (int i = 0; i < types.Count; i++)
			{
				Types.Add(new PlanetTypeRegion(types[i]));
			}

			Radius = radius;
		}

		public void Update(Scene scene, float timestep)
		{
			Star star = scene.GetMainStar();

			float acceleration = Mathf.G * star.Mass / Radius / Radius;
			float angular_speed = Mathf.Sqrt(acceleration * Radius) / Radius;
			AngleOffset += angular_speed * timestep;
		}
		
		public Planet.PlanetType GetClosestType(float angle)
		{
			float angle_increment = 2 * Mathf.PI / Types.Count;
			int index = (int) Mathf.Round((angle - AngleOffset) / angle_increment);

			return Types[index].Type;
		}

		private class PlanetTypeRegion
		{
			public Planet.PlanetType Type { get; }
			
			public PlanetTypeRegion(Planet.PlanetType type)
			{
				Type = type;
			}
		}
	}
}