namespace GravityGame
{
	public abstract class Compound
	{
		public enum CompoundType
		{
			Hydrogen,
			Helium,
			Methane,
			Carbon,
			Iron,
			Nickel,
			Silicon
		}
	
		public float Mass { get; private set; }
		public abstract float Density { get; }
		public abstract bool Gas { get; }

		public float Area => Mass / Density;

		public Compound() : this(0)
		{
			
		}
		
		public Compound(float mass)
		{
			Mass = mass;
		}

		public Compound(Compound original)
		{
			Mass = original.Mass;
		}
	}
}