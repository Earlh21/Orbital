using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using NUnit.Framework.Constraints;
using SFML.Graphics;

namespace GravityGame
{
	public struct Compound
	{
		public enum CompoundType
		{
			Helium,
			Hydrogen,
			Methane,
			Nitrogen,
			Oxygen,
			Carbon,
			Iron,
			Nickel,
			Silicon,
			Basic
		}

		private static bool IsTypeGas(CompoundType type)
		{
			switch (type)
			{
				case CompoundType.Helium:
					return true;
				case CompoundType.Hydrogen:
					return true;
				case CompoundType.Methane:
					return true;
				case CompoundType.Nitrogen:
					return true;
				case CompoundType.Oxygen:
					return true;
				default:
					return false;
			}
		}
		
		private static List<CompoundType> GasTypes;
		private static List<CompoundType> RockTypes;
		
		public CompoundType Type { get; private set; }
		
		public float Mass { get; private set; }
		public float Area => Mass / Density;
		public float Density
		{
			get
			{
				switch (Type)
				{
					case CompoundType.Helium:
						return 0.2f;
					case CompoundType.Hydrogen:
						return 0.1f;
					case CompoundType.Oxygen:
						return 1.4f;
					case CompoundType.Nitrogen:
						return 1.3f;
					case CompoundType.Methane:
						return 0.7f;
					case CompoundType.Carbon:
						return 2.5f;
					case CompoundType.Iron:
						return 5.0f;
					case CompoundType.Nickel:
						return 10.0f;
					case CompoundType.Silicon:
						return 3.0f;
					default:
						return 1.0f;
				}
			}
		}

		public Colorf Color
		{
			get
			{
				Color color;
				
				switch (Type)
				{
					case CompoundType.Helium:
						color = new Color(200, 80, 20, 255);
						break;
					case CompoundType.Hydrogen:
						color = new Color(180, 120, 60, 255);
						break;
					case CompoundType.Methane:
						color = new Color(0, 200, 45, 255);
						break;
					case CompoundType.Nitrogen:
						color = new Color(180, 180, 180, 255);
						break;
					case CompoundType.Oxygen:
						color = new Color(50, 80, 90, 255);
						break;
					default:
						color = new Color(0, 0, 0, 0);
						break;
				}

				return Colorf.FromColor(color);
			}
		}

		public bool IsGas => IsTypeGas(Type);
		public bool IsRock => !IsGas;

		public Compound(CompoundType type, float mass)
		{
			Type = type;
			Mass = mass;
		}

		static Compound()
		{
			GasTypes = new List<CompoundType>();
			RockTypes = new List<CompoundType>();
			
			foreach (CompoundType type in Enum.GetValues(typeof(CompoundType)))
			{
				if (IsTypeGas(type))
				{
					GasTypes.Add(type);
					continue;
				}

				RockTypes.Add(type);
			}
		}

		public static Compound operator +(Compound a, Compound b)
		{
			if (a.Type != b.Type)
			{
				throw new ArgumentException("Compound types don't match.");
			}
			
			return new Compound(a.Type, a.Mass + b.Mass);
		}

		public static Compound GetRandomRockyCompound(float mass)
		{
			int index = Program.R.Next(RockTypes.Count);
			
			return new Compound(RockTypes[index], mass);
		}

		public static Compound GetRandomGasCompound(float mass)
		{
			int index = Program.R.Next(GasTypes.Count);

			return new Compound(GasTypes[index], mass);
		}
	}
}