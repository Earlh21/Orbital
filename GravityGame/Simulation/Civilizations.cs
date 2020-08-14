using System;
using System.Collections.Generic;
using System.IO;
using SFML.Graphics;

namespace GravityGame
{
	public static class Civilizations
	{
		private static List<String> names;
		private static List<Color> colors;
		private static Dictionary<int, int> faction_count;
		private static Dictionary<int, Demeanor> demeanors;
		
		static Civilizations()
		{
			using (StreamReader reader = new StreamReader(Program.GetResourcesDirectory() + "\\Text\\alien_names.txt"))
			{
				names = new List<string>();
				colors = new List<Color>();
				
				string line = reader.ReadLine();

				while (!String.IsNullOrEmpty(line))
				{
					names.Add(line);
					colors.Add(RandomColor());

					line = reader.ReadLine();
				}
			}
			
			Shuffle(names);
			
			faction_count = new Dictionary<int, int>();
			demeanors = new Dictionary<int, Demeanor>();
		}

		public static int GetFactionCount(int faction)
		{
			return faction_count[faction];
		}

		public static void DecrementFactionCount(int faction)
		{
			faction_count[faction] -= 1;
		}

		public static void IncrementFactionCount(int faction)
		{
			if (!faction_count.ContainsKey(faction))
			{
				faction_count.Add(faction, 0);
			}
			
			faction_count[faction] += 1;
		}
		
		private static Color RandomColor()
		{
			int red = Program.R.Next(200) + 30;
			int green = Program.R.Next(200) + 30;
			int blue = Program.R.Next(200) + 30;

			if (green - red - blue > 80)
			{
				return RandomColor();
			}

			if (red - green - blue > 80)
			{
				return RandomColor();
			}

			if (blue - green - red > 80)
			{
				return RandomColor();
			}
			
			return new Color((byte)red, (byte)green, (byte)blue, 255);
		}

		private static Demeanor GetRandomDemeanor()
		{
			double random = Program.R.NextDouble();

			if (random < 0.2)
			{
				return Demeanor.Normal;
			}
			else if (random < 0.4)
			{
				return Demeanor.Colonial;
			}
			else if(random < 0.6)
			{
				return Demeanor.Scientist;
			}
			else if (random < 0.8)
			{
				return Demeanor.Individual;
			}
			else
			{
				return Demeanor.Hardy;
			}
		}
		
		public static Demeanor GetDemeanor(int faction)
		{
			if (!demeanors.ContainsKey(faction))
			{
				demeanors.Add(faction, GetRandomDemeanor());
			}

			return demeanors[faction];
		}
		
		public static DemeanorData GetDemeanorData(int faction)
		{
			return new DemeanorData(GetDemeanor(faction));
		}
		
		private static void Shuffle<T> (List<T> array)
		{
			int n = array.Count;
			while (n > 1) 
			{
				int k = Program.R.Next(n--);
				T temp = array[n];
				array[n] = array[k];
				array[k] = temp;
			}
		}

		public static string GetName(int id)
		{
			return names[id % names.Count];
		}

		public static Color GetColor(int id)
		{
			return colors[id % colors.Count];
		}

		public enum Demeanor
		{
			Colonial,
			Individual,
			Scientist,
			Hardy,
			Normal
		}

		public struct DemeanorData
		{
			public float ShipChance { get; private set; }
			public float LaserChance { get; private set; }
			public float MatterChance { get; private set; }
			public float SatelliteChance { get; private set; }
			
			public float TemperatureMultiplier { get; private set; }
			public float ScienceMultiplier { get; private set; }
			
			public DemeanorData(Demeanor demeanor)
			{
				switch (demeanor)
				{
					case Demeanor.Normal:
						ShipChance = 1 / 8.0f;
						LaserChance = 1 / 7.5f;
						MatterChance = 1 / 60.0f;
						SatelliteChance = 1 / 50.0f;
						TemperatureMultiplier = 1.0f;
						ScienceMultiplier = 1.0f;
						break;
					case Demeanor.Scientist:
						ShipChance = 1 / 14.0f;
						LaserChance = 1 / 11.0f;
						MatterChance = 1 / 75.0f;
						SatelliteChance = 1 / 60.0f;
						TemperatureMultiplier = 1.0f;
						ScienceMultiplier = 1.6f;
						break;
					case Demeanor.Individual:
						ShipChance = 1 / 19.0f;
						LaserChance = 1 / 6.0f;
						MatterChance = 1 / 50.0f;
						SatelliteChance = 1 / 13.0f;
						TemperatureMultiplier = 1.5f;
						ScienceMultiplier = 1.2f;
						break;
					case Demeanor.Colonial:
						ShipChance = 1 / 4.5f;
						LaserChance = 1 / 9.0f;
						MatterChance = 1 / 70.0f;
						SatelliteChance = 1 / 60.0f;
						TemperatureMultiplier = 1.1f;
						ScienceMultiplier = 0.9f;
						break;
					case Demeanor.Hardy:
						ShipChance = 1 / 10.0f;
						LaserChance = 1 / 9.0f;
						MatterChance = 1 / 70.0f;
						SatelliteChance = 1 / 60.0f;
						TemperatureMultiplier = 1.5f;
						ScienceMultiplier = 1.1f;
						break;
					default:
						ShipChance = -1;
						LaserChance = -1;
						MatterChance = -1;
						SatelliteChance = -1;
						TemperatureMultiplier = -1;
						ScienceMultiplier = -1;
						break;
				}
			}
		}
	}
}