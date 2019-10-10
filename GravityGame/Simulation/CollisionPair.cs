using System;
using System.Net.Sockets;
using System.Threading;
using GravityGame.Extension;
using NUnit.Framework;
using SFML.System;

namespace GravityGame
{
    public struct CollisionPair
    {
        public Body A { get; private set; }
        public Body B { get; private set; }

        public CollisionPair(Body a, Body b)
        {
            A = a;
            B = b;
        }

        public Body Resolve(Scene scene)
        {
            if (!A.Exists || !B.Exists)
            {
                //One of these is already flagged for deletion by another collision
                return null;
            }
            
            Type a_type = A.GetType();
            Type b_type = B.GetType();

            Type planet = typeof(Planet);
            Type star = typeof(Star);
            Type ship = typeof(Ship);

            if (!A.DoesGravity && !B.DoesGravity)
            {
                return null;
            }
            
            if (a_type == planet && b_type == planet)
            {
                return ResolvePlanetPlanet(scene);
            }

            if (a_type == star || b_type == star)
            {
                return ResolveBodyStar(scene);
            }

            if (A is Satellite || B is Satellite)
            {
                return ResolveSatelliteBody();
            }

            if ((A is Ship && b_type == planet) || (a_type == planet && B is Ship))
            {
                return ResolvePlanetShip(scene);
            }

            if (A is Ship && B is Ship)
            {
                return null;
            }

            throw new InvalidOperationException("Collision between two bodies was not covered: " + a_type.ToString() + " : " + b_type.ToString());
        }

        private Body ResolveSatelliteBody()
        {
            Satellite satellite;
            Body body;

            if (A is Satellite)
            {
                satellite = (Satellite) A;
                body = B;
            }
            else
            {
                satellite = (Satellite) B;
                body = A;
            }

            satellite.Exists = false;
            return body;
        }
        
        private Body ResolvePlanetShip(Scene scene)
        {
            Planet planet;
            Ship ship;

            if (A is Ship)
            {
                ship = (Ship) A;
                planet = (Planet) B;
            }
            else
            {
                ship = (Ship) B;
                planet = (Planet) A;
            }

            if (!planet.HasLife)
            {
                planet.Life = ship.Life;
            }
            else if(planet.Life.Faction == ship.Life.Faction)
            {
                if (planet.Life.TechLevel < ship.Life.TechLevel)
                {
                    planet.Life.TechLevel = ship.Life.TechLevel;
                }
            }

            ship.Exists = false;
            return planet;
        }
        
        private Body ResolveBodyStar(Scene scene)
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
            star.Translate(displacement * body.Mass / (star.Mass + body.Mass));
            
            if (body.IsSelected)
            {
                scene.Deselect();
                scene.SelectAt(star.Position);
            }
            
            //Add mass of planet to star
            star.AddComposition(Composition.Basic(body.Mass));
            
            //Add momentum of planet to star
            star.Momentum += body.Momentum;
            
            //Flag the planet for deletion
            body.Exists = false;

            star.Started = false;
            return star;
        }
        
        private Body ResolvePlanetPlanet(Scene scene)
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
            bigger.Translate(displacement * smaller.Mass / (bigger.Mass + smaller.Mass));
            
            if (smaller.IsSelected)
            {
                scene.Deselect();
                scene.SelectAt(bigger.Position);
            }
                
            //Add mass of smaller to bigger
            bigger.AddComposition(smaller);
                
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
            
            //Combine water-area
            bigger.WaterArea += smaller.WaterArea;

            bigger.Started = false;
            return bigger;
        }
    }
}