using System;
using System.Net.Sockets;
using System.Threading;
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

            Type planet = typeof(Planet);
            Type star = typeof(Star);

            if (a_type == planet && b_type == planet)
            {
                ResolvePlanetPlanet();
            }

            if (a_type == star || b_type == star)
            {
                ResolveBodyStar();
            }
        }

        private void ResolveBodyStar()
        {
            Star star;
            Body body;
            
            if (A.GetType() == typeof(Star))
            {
                star = (Star)A;
                body = B;
            }
            else
            {
                star = (Star) B;
                body = A;
            }
            
            //Move the star towards the planet
            Vector2f displacement = body.Position - star.Position;
            star.Position += displacement * body.Mass / (star.Mass + body.Mass);
            
            //Add mass of planet to star
            star.Mass += body.Mass;
            
            //Add momentum of planet to star
            star.Momentum += body.Momentum;
            
            //Flag the planet for deletion
            body.Exists = false;
        }
        
        private void ResolvePlanetPlanet()
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
                
            //Add momentum of smaller to bigger, but convert some of the total momentum into heat
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