using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using SFML.Graphics;

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

		public Composition() : this(new List<Compound>())
		{
		}

		/// <summary>
		/// Adds all of another Composition's Compounds to this Composition.
		/// </summary>
		/// <param name="other">Composition to add</param>
		public void AddComposition(Composition other)
		{
			foreach (Compound compound in other.compounds)
			{
				AddCompound(compound);
			}
		}

		/// <summary>
		/// Returns the mass-percentage of rock Compounds in this Composition.
		/// </summary>
		/// <returns>the mass-percentage of rock Compounds in this Composition</returns>
		public float GetRockPercent()
		{
			return 1 - GetGasPercent();
		}
		
		/// <summary>
		/// Returns the mass-percentage of gas Compounds in this Composition.
		/// </summary>
		/// <returns>the mass-percentage of gas Compounds in this Composition</returns>
		public float GetGasPercent()
		{
			float gas_mass = 0;

			foreach (Compound compound in compounds)
			{
				if (compound.IsGas)
				{
					gas_mass += compound.Mass;
				}
			}

			return gas_mass / Mass;
		}

		/// <summary>
		/// Adds a Compound to this Composition.
		/// </summary>
		/// <param name="c">Compound to add</param>
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

		public Compound.CompoundType GetLargestType()
		{
			float largest_mass = float.MinValue;
			Compound.CompoundType largest_type = Compound.CompoundType.Basic;

			foreach (Compound compound in Compounds)
			{
				if (compound.Mass > largest_mass)
				{
					largest_mass = compound.Mass;
					largest_type = compound.Type;
				}
			}

			return largest_type;
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
			float rock_percent = Mathf.Clamp(0, 1, 0.9f + (float) (Program.R.NextDouble() - 0.5f) * 2.0f * 0.15f);
			float gas_percent = 1 - rock_percent;

			float rock_mass = mass * rock_percent;
			float gas_mass = mass * gas_percent;

			return Generate(rock_mass, gas_mass);
		}

		public static Composition Gas(float mass)
		{
			float rock_percent = Mathf.Clamp(0, 1, 0.15f + (float) (Program.R.NextDouble() - 0.5f) * 2.0f * 0.2f);
			float gas_percent = 1 - rock_percent;

			float rock_mass = mass * rock_percent;
			float gas_mass = mass * gas_percent;

			return Generate(rock_mass, gas_mass);
		}

		private static Composition Generate(float rock_mass, float gas_mass)
		{	
			float variance = 0.2f;
			
			Composition composition = new Composition();

			float original_rock_mass = rock_mass;
			float original_gas_mass = gas_mass;
			
			while (rock_mass > 0)
			{
				float c_mass = original_rock_mass / 10;
				c_mass *= (float) Program.R.NextDouble() * variance + (1 - variance / 2.0f);

				if (c_mass > rock_mass)
				{
					c_mass = rock_mass;
					rock_mass = -1;
				}
				else
				{
					rock_mass -= c_mass;
				}
				
				composition.AddCompound(Compound.GetRandomRockyCompound(c_mass));
			}
			
			while (gas_mass > 0)
			{
				float c_mass = original_gas_mass / 10;
				c_mass *= (float) Program.R.NextDouble() * variance + (1 - variance / 2.0f);

				if (c_mass > gas_mass)
				{
					c_mass = gas_mass;
					gas_mass = -1;
				}
				else
				{
					gas_mass -= c_mass;
				}
				
				composition.AddCompound(Compound.GetRandomGasCompound(c_mass));
			}

			return composition;
		}

		public Colorf GetGasColor()
		{
			float gas_mass = Mass * GetGasPercent();
			
			Colorf total = new Colorf(0, 0, 0, 1.0f);

			foreach (Compound compound in compounds)
			{
				if (compound.IsGas)
				{
					total += compound.Color * compound.Mass / gas_mass;
				}
			}

			return total;
		}
	}
}