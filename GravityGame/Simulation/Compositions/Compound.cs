using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using NUnit.Framework.Constraints;

namespace GravityGame
{
	public struct Compound
	{
		public enum CompoundType
		{
			Helium,
			Hydrogen,
			Methane,
			Carbon,
			Iron,
			Nickel,
			Silicon,
			Basic
		}

		public CompoundType Type { get; private set; }
		public float Mass { get; private set; }

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
					case CompoundType.Methane:
						return 0.7f;
					case CompoundType.Carbon:
						return 5.0f;
					case CompoundType.Iron:
						return 10.0f;
					case CompoundType.Nickel:
						return 15.0f;
					case CompoundType.Silicon:
						return 5.0f;
					default:
						return 1.0f;
				}
			}
		}

		public bool Gas
		{
			get
			{
				switch (Type)
				{
					case CompoundType.Helium:
						return true;
					case CompoundType.Hydrogen:
						return true;
					case CompoundType.Methane:
						return true;
					default:
						return false;
				}
			}

		}

		public float Area => Mass / Density;

		public Compound(CompoundType type, float mass)
		{
			Type = type;
			Mass = mass;
		}

		public static Compound operator +(Compound a, Compound b)
		{
			if (a.Type != b.Type)
			{
				throw new ArgumentException("Compound types don't match.");
			}
			
			return new Compound(a.Type, a.Mass + b.Mass);
		}
	}
}