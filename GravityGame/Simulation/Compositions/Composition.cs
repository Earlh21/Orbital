using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace GravityGame
{
	public class Composition
	{
		private float mass;
		private float area;
		private List<Compound> compounds;

		public ReadOnlyCollection<Compound> Compounds => compounds.AsReadOnly();
		public float Mass => mass;
		public float Area => area;
		public float Density => mass / area;

		public Composition(List<Compound> compounds)
		{
			this.compounds = new List<Compound>();
			
			foreach (Compound compound in compounds)
			{
				AddCompound(compound);
			}
		}

		public void AddComposition(Composition other)
		{
			compounds = new List<Compound>();
			
			foreach (Compound compound in other.compounds)
			{
				AddCompound(compound);
			}
		}

		public float GetRockPercent()
		{
			return 1 - GetGasPercent();
		}
		
		public float GetGasPercent()
		{
			float gas_mass = 0;

			foreach (Compound compound in compounds)
			{
				if (compound.Gas)
				{
					gas_mass += compound.Mass;
				}
			}

			return gas_mass / Mass;
		}

		private void AddCompound(Compound c)
		{
			mass += c.Mass;
			area += c.Area;
			int index = compounds.FindIndex(x => x.Type == c.Type);

			if (index == -1)
			{
				compounds.Add(new Compound(c.Type, c.Mass));
			}
			else
			{
				compounds[index] += c;
			}
		}

		private void SubtractCompound(Compound c)
		{
			int index = compounds.FindIndex(x => x.Type == c.Type);

			if (index != -1)
			{
				float c_mass = compounds[index].Mass;
				float o_mass = c_mass;
				c_mass -= c.Mass;

				if (c_mass <= 0)
				{
					compounds.RemoveAt(index);
					mass -= o_mass;
					return;
				}
				
				compounds[index] = new Compound(c.Type, c_mass);

				mass -= c.Mass;
			}
		}

		//TODO: Turn basic compositions into an inherited class
		public void RemoveBasic(float amount)
		{
			if (compounds.Count != 1 || compounds[0].Type != Compound.CompoundType.Basic)
			{
				throw new InvalidOperationException("This Composition is not a basic Composition.");
			}
			
			SubtractCompound(new Compound(Compound.CompoundType.Basic, amount));
		}
		
		public static Composition Basic(float mass)
		{
			return Single(Compound.CompoundType.Basic, mass);
		}

		public static Composition Single(Compound.CompoundType type, float mass)
		{
			List<Compound> c = new List<Compound>();
			c.Add(new Compound(type, mass));
			return new Composition(c);
		}

		public static Composition Rocky(float mass)
		{
			throw new NotImplementedException();
		}

		public static Composition Gas(float mass)
		{
			throw new NotImplementedException();
		}
	}
}