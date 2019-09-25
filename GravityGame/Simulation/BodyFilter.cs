using System;
using System.Dynamic;
using System.Media;
using System.Security.Authentication.ExtendedProtection;
using NUnit.Framework;
using SFML.Window;

namespace GravityGame
{
	public struct BodyFilter
	{
		public enum LifeFilter
		{
			True,
			False,
			Either
		}
		
		public Type Type { get; private set; }

		public LifeFilter HasLife { get; private set; }
		
		public int Faction { get; private set; }
		
		public bool MatchFaction { get; private set; }

		public BodyFilter(Type type, LifeFilter life_filter, int faction = -1, bool match_faction = true)
		{
			Type = type;
			HasLife = life_filter;
			Faction = faction;
			MatchFaction = match_faction;
		}

		public bool Contains(Body body)
		{
			if (body is Planet planet)
			{
				switch (HasLife)
				{
					case LifeFilter.True:
						if (!planet.HasLife)
						{
							return false;
							
						}

						break;
					case LifeFilter.False:
						if (planet.HasLife)
						{
							return false;
						}

						break;
				}

				if (Faction != -1)
				{
					if (MatchFaction)
					{
						if (planet.Life.Faction != Faction)
						{
							return false;
						}
					}
					else
					{
						if (planet.Life.Faction == Faction)
						{
							return false;
						}
					}
				}
			}
			else if (HasLife == LifeFilter.True)
			{
				return false;
			}
			
			if (!body.GetType().IsSubclassOf(Type) && body.GetType() != Type)
			{
				return false;
			}

			return true;
		}
	}
}