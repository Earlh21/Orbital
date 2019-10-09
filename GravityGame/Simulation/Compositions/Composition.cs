using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace GravityGame
{
	public class Composition
	{
		private float mass;
		private List<Compound> compounds;

		public ReadOnlyCollection<Compound> Compounds => compounds.AsReadOnly();

		private Composition(List<Compound> compounds)
		{
			this.compounds = compounds;
		}

		public void AddComposition(Composition other)
		{
			foreach (Compound compound in other.compounds)
			{
				Compound mine = compounds.Find(x => x.GetType() == compound.GetType());

				if (mine == null)
				{
					compounds.Add(compound);
				}
			}
		}
	}
}