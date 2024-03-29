using System;
using GravityGame.Extension;
using GravityGame.Graphics;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
	public class Planet : TemperatureBody, IDrawsText
	{
		public enum PlanetType
		{
			Rocky,
			Gas,
			Ocean
		}

		private Text temperature_text;
		private Text population_text;
		private Text tech_level_text;
		private Text faction_text;

		/**
         * The seed used by the shader to generate a height map.
         */
		private float shader_seed;

		/**
         * Amount of water-area in the planet's atmosphere and surface.
         */
		public float WaterArea { get; set; }

		/**
         * Percentage of water-area that's frozen
         */
		public float IcePercentage => 1 - Mathf.InvLerp(100.0f, 273.0f, Temperature);

		/**
         * Amount of water-area on the planet's surface.
         */
		public float SurfaceWaterArea
		{
			get
			{
				float boil = Mathf.InvLerp(373.0f, 1000.0f, Temperature);
				return WaterArea - WaterArea * Mathf.Clamp(0, 1, boil);
			}
		}

		/**
         * Percentage of the surface covered by water.
         */
		public float WaterPercentage => SurfaceWaterArea / Area;

		public PlanetType Type
		{
			get
			{
				if (GasPercent > 0.6f)
				{
					return PlanetType.Gas;
				}

				if (WaterPercentage > 0.95f)
				{
					return PlanetType.Ocean;
				}

				return PlanetType.Rocky;
			}
		}

		private static float life_chance = 1 / 300.0f;
		private Civilizations.DemeanorData demeanor_data;

		private Life life;

		public Life Life
		{
			get => life;
			set
			{
				if (HasLife)
				{
					Civilizations.DecrementFactionCount(Life.Faction);
				}

				if (value != null)
				{
					Civilizations.IncrementFactionCount(value.Faction);

					demeanor_data = Civilizations.GetDemeanorData(value.Faction);
					value.ScienceMultiplier = demeanor_data.ScienceMultiplier;
					value.TempMultiplier = demeanor_data.TemperatureMultiplier;
				}

				life = value;
			}
		}

		public bool HasLife => Life != null;
		public override Color? OutlineColor => HasLife ? (Color?) Civilizations.GetColor(Life.Faction) : Color.Green;

		public string Name { get; set; }

		public Planet(Vector2f position, Vector2f velocity, Composition composition, float temperature) : base(position,
			velocity, composition, temperature)
		{
			Life = null;
			shader_seed = (float) Program.R.NextDouble() * 200.0f;
			WaterArea = Area * (float) Program.R.NextDouble();

			
			//Temporary name generation
			Name = "";

			switch (Type)
			{
				case PlanetType.Gas:
					Name += "G";
					break;
				case PlanetType.Ocean:
					Name += "O";
					break;
				case PlanetType.Rocky:
					Name += "R";
					break;
			}

			Name += composition.GetLargestType().ToString()[0];
			Name += "-";
			for (int i = 0; i < 6; i++)
			{
				Name += Program.R.Next(10).ToString();
			}
			
			temperature_text = new Text("", Program.Font);
			population_text = new Text("", Program.Font);
			tech_level_text = new Text("", Program.Font);
			faction_text = new Text("", Program.Font);
		}

		private void FormatText(Text text, float level, RenderWindow window)
		{
			View view = window.GetView();

			text.Position = Position.InvY() + new Vector2f(-Radius, Radius + level * view.Size.Y / 30);
			text.CharacterSize = 50;
			float scale = view.Size.X / window.Size.X;
			text.Scale = 0.5f * new Vector2f(scale, scale);
		}

		private void FireShip(Scene scene, Body target)
		{
			Life life = new Life(Life);

			float speed = Mathf.Sqrt(2 * Mathf.G * Mass / Radius) * 1.1f;
			float distance = Distance(target) * 0.75f;
			float time = distance / speed;
			Vector2f other_position = target.Position + target.Velocity * time;
			float angle = Mathf.AngleTo(Position, other_position);

			Vector2f velocity = speed * new Vector2f(Mathf.Cos(angle), Mathf.Sin(angle));

			Ship ship;

			if (Life.TechLevel <= 1)
			{
				ship = new Ship(Position + velocity.Unit() * Radius * 1.5f, velocity, life);
			}
			else if (Life.TechLevel <= 3)
			{
				ship = new ThrusterShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
			}
			else if (Life.TechLevel <= 6)
			{
				double random = Program.R.NextDouble();

				if (random > 0.2)
				{
					ship = new OrbitShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
				}
				else
				{
					ship = new ThrusterShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
				}
			}
			else
			{
				double random = Program.R.NextDouble();

				if (random > 0.4)
				{
					ship = new OrbitShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
				}
				else
				{
					ship = new TeleportShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
				}
			}

			scene.AddBody(ship);
		}

		private void FireMatter(Scene scene, Body target)
		{
			float speed = Mathf.Sqrt(2 * Mathf.G * Mass / Radius) * (3.5f + Life.TechLevel / 6.0f);
			float mass = 30 + Life.TechLevel * 12;

			float distance = Distance(target);
			float time = distance / speed;
			Vector2f other_position = target.Position + target.Velocity * time;
			float angle = Mathf.AngleTo(Position, other_position);

			Vector2f velocity = speed * new Vector2f(Mathf.Cos(angle), Mathf.Sin(angle));
			Composition composition = Composition.Single(Compound.CompoundType.Osmium, mass);
			Planet bullet = new Planet(Position + velocity.Unit() * Radius * 2.5f, velocity, composition, 100000);

			scene.AddBody(bullet);
		}

		private void FireLaser(Scene scene, Planet target)
		{
			float heat_change_sign = Mathf.Sign(target.Temperature - target.Life.NormalTemp);
			Color color;
			float heat_change;

			if (heat_change_sign > 0 || target.Life.TechLevel > 2)
			{
				color = Color.Red;
				heat_change = 10000 + Mathf.Pow(Life.TechLevel, 1.8f) * 2500;
			}
			else
			{
				color = Color.Blue;
				heat_change = -1000 - Life.TechLevel * 100;
			}

			HeatLaserEffect laser = new HeatLaserEffect(this, target, color, heat_change, 3.0f);
			scene.AddEffect(laser);
		}

		private void FireLaserSatellite(Scene scene)
		{
			float angle = Program.R.Next();
			float speed = Mathf.Sqrt(2 * Mathf.G * Mass / Radius) * 0.8f;
			Vector2f velocity = Velocity + new Vector2f(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
			Vector2f position = Position + velocity.Unit() * Radius * 1.5f;

			Satellite satellite = new LaserSatellite(position, velocity, this, Life.Faction);
			scene.AddBody(satellite);
		}

		public override void Draw(RenderTarget target, RenderStates states)
		{
			base.Draw(target, states);

			if (DrawText)
			{
				RenderWindow window = (RenderWindow) target;

				temperature_text.DisplayedString = (int) Temperature + " K";
				temperature_text.Color = Mathf.TemperatureColorGradient.GetColor(Temperature);
				FormatText(temperature_text, 0, window);

				target.Draw(temperature_text);

				if (HasLife)
				{
					population_text.DisplayedString = Format.PopulationText(Life.Population);
					population_text.Color = Color.White;

					tech_level_text.DisplayedString = Life.TechLevel + " | " + Civilizations.GetDemeanor(Life.Faction);
					tech_level_text.Color = Color.White;

					faction_text.DisplayedString = Civilizations.GetName(Life.Faction);
					faction_text.Color = Civilizations.GetColor(Life.Faction);

					FormatText(population_text, 1, window);
					FormatText(tech_level_text, 2, window);
					FormatText(faction_text, 3, window);

					target.Draw(population_text);
					target.Draw(tech_level_text);
					target.Draw(faction_text);
				}
			}
		}

		public override void Update(Scene scene, float time)
		{
			base.Update(scene, time);
			UpdateLife(scene, time);
		}

		public void UpdateLife(Scene scene, float time)
		{
			if (!HasLife)
			{
				if (Program.R.NextDouble() < 1 - Math.Pow(1 - life_chance, time))
				{
					EvolveLife();
				}

				return;
			}

			if (Life.IsDead)
			{
				Life = null;
				return;
			}

			if (Life.TechLevel >= 1 && Program.R.NextDouble() < 1 - Math.Pow(1 - demeanor_data.ShipChance, time))
			{
				Body[] buffer = new Body[5];
				int height = 2;
				BodyFilter filter = new BodyFilter(typeof(Planet), BodyFilter.LifeFilter.False);

				int count = scene.QuadTree.FindNearbyBodies(Position, height, filter, buffer);

				if (count < 1)
				{
					return;
				}

				int index = Program.R.Next(count);
				Body target = buffer[index];

				FireShip(scene, target);
			}

			if (Life.TechLevel >= 2 && Program.R.NextDouble() < 1 - Math.Pow(1 - demeanor_data.LaserChance, time))
			{
				Body[] buffer = new Body[5];
				int height = 2;
				BodyFilter filter = new BodyFilter(typeof(Planet), BodyFilter.LifeFilter.True, Life.Faction, false);

				int count = scene.QuadTree.FindNearbyBodies(Position, height, filter, buffer);

				if (count < 1)
				{
					return;
				}

				int index = Program.R.Next(count);
				Body target = buffer[index];

				FireLaser(scene, (Planet) target);
			}

			if (Life.TechLevel >= 3 && Program.R.NextDouble() < 1 - Math.Pow(1 - demeanor_data.MatterChance, time))
			{
				Body[] buffer = new Body[5];
				int height = 2;
				BodyFilter filter = new BodyFilter(typeof(Planet), BodyFilter.LifeFilter.True, Life.Faction, false);

				int count = scene.QuadTree.FindNearbyBodies(Position, height, filter, buffer);

				if (count < 1)
				{
					return;
				}

				int index = Program.R.Next(count);
				Body target = buffer[index];

				FireMatter(scene, target);
			}

			if (Life.TechLevel >= 2 && Program.R.NextDouble() < 1 - Math.Pow(1 - demeanor_data.SatelliteChance, time))
			{
				FireLaserSatellite(scene);
			}

			Life.Update(time, Temperature, Type);
		}

		public void EvolveLife()
		{
			Life = new Life(Temperature, Type);
		}

		protected override Shader GetShader()
		{
			RockyShader.Temperature = Temperature;
			RockyShader.IcePercentage = IcePercentage;
			RockyShader.WaterPercentage = WaterPercentage;
			RockyShader.Seed = shader_seed;
			RockyShader.IceTexture = Textures.Ice;
			RockyShader.LandTexture = Textures.RedRocky;
			RockyShader.Radius = Radius;
			RockyShader.AtmosphereColor = GetGasColor();
			RockyShader.AtmosphereStrength = Mathf.Clamp(0, 1, Mathf.InvLerp(0.0f, 0.3f, GasPercent));

			RockyShader.Load(Texture);
			return RockyShader.Shader;
		}
	}
}