using System;
using System.Collections.Generic;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
	public class PlanetTypeMap : Drawable
	{
		private float inner_radius;
		private float outer_radius;
		private float radius_increment;
		private int ring_count;
		private List<PlanetTypeRing> Rings { get; }

		public PlanetTypeMap(float inner_radius, float outer_radius, int ring_count, float distance_per_region)
		{
			this.inner_radius = inner_radius;
			this.outer_radius = outer_radius;
			this.ring_count = ring_count;
			radius_increment = (outer_radius - inner_radius) / (ring_count - 1);

			List<Planet.PlanetType> types = new List<Planet.PlanetType>();

			foreach (Planet.PlanetType type in Enum.GetValues(typeof(Planet.PlanetType)))
			{
				types.Add(type);

				if (type != Planet.PlanetType.Gas)
				{
					types.Add(type);
					types.Add(type);
					types.Add(type);
				}
			}
			
			Rings = new List<PlanetTypeRing>();
			for (int i = 0; i < ring_count; i++)
			{
				float radius = inner_radius + i * radius_increment;
				float circumference = Mathf.PI * radius * 2;
				int region_count = (int)(circumference / distance_per_region);

				if (region_count < 1)
				{
					region_count = 1;
				}
				
				List<Planet.PlanetType> ring_types = new List<Planet.PlanetType>();
				for (int j = 0; j < region_count; j++)
				{
					int index = Program.R.Next(types.Count);
					ring_types.Add(types[index]);
				}
				
				PlanetTypeRing ring = new PlanetTypeRing(ring_types, radius);

				Rings.Add(ring);
			}
		}

		public Planet.PlanetType GetClosestType(Vector2f position)
		{
			float radius = position.Length();

			int index = (int) Mathf.Round((radius - inner_radius) / radius_increment);
			
			if (index >= ring_count)
			{
				index = ring_count - 1;
			}
			if (index < 0)
			{
				index = 0;
			}

			return Rings[index].GetClosestType(position);
		}

		public void Update(Scene scene, float time)
		{
			foreach (PlanetTypeRing ring in Rings)
			{
				ring.Update(scene, time);
			}
		}

		public void Draw(RenderTarget target, RenderStates states)
		{
			foreach (PlanetTypeRing ring in Rings)
			{
				target.Draw(ring);
			}
		}
	}
}