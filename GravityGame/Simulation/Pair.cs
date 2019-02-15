using System;
using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
    public struct Pair
    {
        private Body a;
        private Body b;

        public Body A => a;
        public Body B => b;

        public Pair(Body a, Body b)
        {
            this.a = a;
            this.b = b;
        }

        public void Resolve()
        {
            if (!A.Exists || !B.Exists)
            {
                //One of these is already flagged for deletion by another collision
                return;
            }
            
            Type a_type = A.GetType();
            Type b_type = B.GetType();

            if (a_type == typeof(Planet) && b_type == typeof(Planet))
            {
                Planet bigger;
                Planet smaller;
                if (A.Mass > B.Mass)
                {
                    bigger = (Planet) A;
                    smaller = (Planet) B;
                }
                else
                {
                    bigger = (Planet) B;
                    smaller = (Planet) A;
                }

                //Move the bigger planet towards the smaller planet
                Vector2f displacement = smaller.Position - bigger.Position;
                bigger.Position += displacement * smaller.Mass / (bigger.Mass + smaller.Mass);
                
                //Add mass of smaller to bigger
                bigger.Mass += smaller.Mass;
                
                //Add momentum of smaller to bigger, but convert some of the momentum into heat
                bigger.Heat += smaller.Momentum.Length() * Mathf.HeatRatio;
                bigger.Momentum += smaller.Momentum;
                bigger.Momentum *= 1 - Mathf.HeatRatio;
                
                //Add heat of smaller to bigger
                bigger.Heat += smaller.Heat;
                
                //Add heat simply because of the collision
                bigger.Heat += Mathf.Pow(smaller.Area, 3);
                
                //Flag smaller for deletion
                smaller.Exists = false;
            }
        }
    }
}