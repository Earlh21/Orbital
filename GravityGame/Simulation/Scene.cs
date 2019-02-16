using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
    //TODO: Make this class
    public class Scene : Drawable
    {
        private List<RenderBody> bodies;
        private List<Star> star_cache;

        public List<RenderBody> Bodies => bodies;

        public Scene()
        {
            bodies = new List<RenderBody>();
            star_cache = new List<Star>();
        }
        
        public void AddBody(RenderBody body)
        {
            bodies.Add(body);
            if (body is Star)
            {
                star_cache.Add((Star)body);
            }
        }

        public void RemoveBody(RenderBody body)
        {
            bodies.Remove(body);
            if (body is Star)
            {
                star_cache.Remove((Star) body);
            }
        }

        public void MakeStarAt(Vector2f position)
        {
            RenderBody b = null;
            
            foreach (RenderBody body in bodies)
            {
                if (body.Contains(position))
                {
                    body.Exists = false;
                    Star star = new Star(body.Position, body.Mass, body.Velocity, body.Density);
                    bodies.Add(star);
                    star_cache.Add(star);
                    b = body;
                    break;
                }
            }

            bodies.Remove(b);
        }

        //TODO: See if there's a way to cache some of this
        public Rectangle GetAABB()
        {
            float topx = float.MinValue;
            float bottomx = float.MaxValue;
            float topy = float.MinValue;
            float bottomy = float.MaxValue;

            foreach (Body body in bodies)
            {
                if (body.Position.X > topx)
                {
                    topx = body.Position.X;
                }

                if (body.Position.X < bottomx)
                {
                    bottomx = body.Position.X;
                }

                if (body.Position.Y > topy)
                {
                    topy = body.Position.Y;
                }

                if (body.Position.Y < bottomy)
                {
                    bottomy = body.Position.Y;
                }
            }

            Vector2f bottomleftbound = new Vector2f(bottomx, bottomy);
            Vector2f range = (new Vector2f(topx, topy) - bottomleftbound);
            
            Vector2f size = range + new Vector2f(2, 2) + range * 20.0f;
            bottomleftbound -= new Vector2f(1, 1) + range * 10.0f;

            return new Rectangle(bottomleftbound, size);
        }

        private QuadTree GetQuadTree()
        {
            QuadTree tree = new QuadTree(GetAABB());

            foreach (Body body in bodies)
            {
                tree.Insert(body);
            }

            //Cache centers of mass
            tree.CalculateCenterOfMass();

            return tree;
        }
        
        public void Draw(RenderTarget target, RenderStates states)
        {
            RenderWindow window = (RenderWindow) target;
            
            foreach (RenderBody body in bodies)
            {
                target.Draw(body);
            }
        }

        private void ApplyForces(QuadTree tree, float time)
        {
            foreach (Body body in bodies)
            {
                body.ApplyForce(body.GetForceFrom(tree), time);
            }
        }

        private void UpdateAll(float time)
        {
            foreach (Body body in bodies)
            {
                body.Update(time);
            }
        }

        private void ResolveCollisions(QuadTree tree)
        {
            //Find collisions
            List<Pair> collisions = new List<Pair>();

            foreach (Body body in bodies)
            {
                collisions.AddRange(body.GetCollisions(tree));
            }

            //Resolve collisions
            foreach (Pair collision in collisions)
            {
                collision.Resolve();
            }

            //Remove nonexistent bodies
            for (int i = bodies.Count - 1; i > -1; i--)
            {
                if (!bodies[i].Exists)
                {
                    bodies.RemoveAt(i);
                }
            }
        }
        
        private void ApplyStarHeat(float time)
        {
            foreach (Body body in bodies)
            {
                if (body is TemperatureBody)
                {
                    TemperatureBody t_body = (TemperatureBody) body;
                    foreach (Star star in star_cache)
                    {
                        t_body.Heat += t_body.GetHeatFlowFrom(star) * time;
                    }
                }
            }
        }
        
        public void Update(float time)
        {
            QuadTree tree = GetQuadTree();
            ApplyForces(tree, time);
            UpdateAll(time);
            ResolveCollisions(tree);
            ApplyStarHeat(time);
        }
    }
}