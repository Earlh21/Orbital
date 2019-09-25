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
		}

		//TODO: Avoid colors that don't contrast the temperature colors
		private static Color RandomColor()
		{
			int red = Program.R.Next(200) + 30;
			int green = Program.R.Next(200) + 30;
			int blue = Program.R.Next(200) + 30;

			if (green - red - blue > 100)
			{
				return RandomColor();
			}
			
			return new Color((byte)red, (byte)green, (byte)blue, 255);
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
			return names[id];
		}

		public static Color GetColor(int id)
		{
			return colors[id];
		}
	}
}