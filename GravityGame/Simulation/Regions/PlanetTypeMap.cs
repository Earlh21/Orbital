using System;
using System.Collections.Generic;
using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
	public class PlanetTypeMap
	{
		private float inner_radius;
		private float outer_radius;
		private float radius_increment;
		private int ring_count;
		private List<PlanetTypeRing> rings { get; }

		public PlanetTypeMap(float inner_radius, float outer_radius, int ring_count, int regions_per_ring)
		{
			this.inner_radius = inner_radius;
			this.outer_radius = outer_radius;
			this.ring_count = ring_count;
			radius_increment = (outer_radius - inner_radius) / (ring_count - 1);

			List<Planet.PlanetType> types = new List<Planet.PlanetType>();

			foreach (Planet.PlanetType type in Enum.GetValues(typeof(Planet.PlanetType)))
			{
				types.Add(type);
			}
			
			rings = new List<PlanetTypeRing>();
			for (int i = 0; i < ring_count; i++)
			{
				List<Planet.PlanetType> ring_types = new List<Planet.PlanetType>();
				for (int j = 0; j < regions_per_ring; j++)
				{
					int index = Program.R.Next(types.Count);
					ring_types.Add(types[index]);
				}

				float radius = inner_radius + i * radius_increment;
				PlanetTypeRing ring = new PlanetTypeRing(ring_types, radius);

				rings.Add(ring);
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

			return rings[index].GetClosestType(position);
		}
	}
}