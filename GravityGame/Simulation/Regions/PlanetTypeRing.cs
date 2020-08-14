using System.Collections.Generic;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
	/// <summary>
	/// Ok, so this class doesn't really work for some reason.
	/// You get regions of planets, but they don't correspond correctly.
	/// Like, functionally it works but if I need to make precise changes,
	/// I'm going to have to fix this first.
	/// </summary>
	public class PlanetTypeRing : Drawable
	{
		private float Radius { get; }
		private float AngleOffset { get; set; }
		private List<PlanetTypeRegion> Regions { get; }
		
		public PlanetTypeRing(List<Planet.PlanetType> types, float radius)
		{
			Regions = new List<PlanetTypeRegion>();
			for (int i = 0; i < types.Count; i++)
			{
				Regions.Add(new PlanetTypeRegion(types[i]));
			}

			Radius = radius;
		}
		
		public void Update(Scene scene, float timestep)
		{
			Star star = scene.GetMainStar();

			if (star == null)
			{
				return;
			}

			float acceleration = Mathf.G * star.Mass / Radius / Radius;
			float angular_speed = Mathf.Sqrt(acceleration * Radius) / Radius;
			AngleOffset -= angular_speed * timestep;
		}
		
		public Planet.PlanetType GetClosestType(Vector2f position)
		{	
			float angle = Mathf.Atan2(position.Y, position.X) + Mathf.PI;
			
			float angle_increment = 2 * Mathf.PI / Regions.Count;
			int index = (int) Mathf.Round((angle - AngleOffset - angle_increment / 2) / angle_increment);

			if (index > Regions.Count - 1)
			{
				index = Regions.Count - 1;
			}

			if (index < 0)
			{
				index = 0;
			}
			
			return Regions[index].Type;
		}

		public void Draw(RenderTarget target, RenderStates states)
		{
			float angle_increment = 2 * Mathf.PI / Regions.Count;

			for (int i = 0; i < Regions.Count; i++)
			{
				PlanetTypeRegion region = Regions[i];
				float angle = AngleOffset + angle_increment * i;
				Vector2f position = Radius * new Vector2f(Mathf.Cos(angle), Mathf.Sin(angle));
				
				CircleShape circle = new CircleShape();
				circle.Radius = 100;
				circle.SetPointCount(16);

				Color color;
				if (region.Type == Planet.PlanetType.Gas)
				{
					color = Color.Green;
				}
				else if (region.Type == Planet.PlanetType.Rocky)
				{
					color = Color.Yellow;
				}
				else
				{
					color = Color.Blue;
				}

				circle.FillColor = color;
				circle.Position = (position - new Vector2f(circle.Radius, circle.Radius)).InvY();
				
				target.Draw(circle);
			}
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