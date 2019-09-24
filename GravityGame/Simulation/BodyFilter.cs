using System;
using System.Dynamic;
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

		public BodyFilter(Type type, LifeFilter life_filter)
		{
			Type = type;
			HasLife = life_filter;
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
			}

			if (!Type.IsInstanceOfType(body))
			{
				return false;
			}

			return true;
		}
	}
}