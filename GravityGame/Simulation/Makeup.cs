using System.ComponentModel.Design;

namespace GravityGame
{
	public struct Makeup
	{
		public float Hydrogen { get; private set; }
		public float Helium { get; private set; }
		public float Methane { get; private set; }
		
		public float Iron { get; private set; }
		public float Silicon { get; private set; }
		public float Nickel { get; private set; }
		public float Carbon { get; private set; }

		private Makeup(float hydrogen, float helium, float methane, float iron, float silicon, float nickel,
			float carbon)
		{
			Hydrogen = hydrogen;
			Helium = helium;
			Methane = methane;
			Iron = iron;
			Silicon = silicon;
			Nickel = nickel;
			Carbon = carbon;
		}
		
		public Makeup Mix(Makeup a, float a_mass, Makeup b, float b_mass)
		{
			
		}

		public static Makeup operator *(Makeup a, float b)
		{
			
		}
	}
}