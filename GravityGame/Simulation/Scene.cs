using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
    public class Scene : Drawable
    {
        private List<RenderBody> bodies;
        private List<Star> star_cache;
        private RenderBody selected;
        private QuadTree tree;

        public bool DrawOutlines { get; set; }
        public RenderBody Selected => selected;
        
        public List<RenderBody> Bodies => bodies;

        public QuadTree Tree => tree;

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
            if (body.IsSelected)
            {
                selected = null;
            }
            
            bodies.Remove(body);
            if (body is Star)
            {
                star_cache.Remove((Star) body);
            }
        }

        public void Deselect()
        {
            if (selected != null)
            {
                selected.IsSelected = false;
            }
        }
        
        public bool SelectAt(Vector2f position)
        {
            bool found = false;
            
            foreach (RenderBody body in bodies)
            {
                if (body.Contains(position))
                {
                    Select(body);

                    found = true;
                    break;
                }
            }

            return found;
        }

        public Vector2f GetSelectedPosition()
        {
            if (selected == null)
            {
                return new Vector2f(0, 0);
            }
            
            return selected.Position;
        }

        public Vector2f GetSelectedVelocity()
        {
            if (selected == null)
            {
                return new Vector2f(0, 0);
            }

            return selected.Velocity;
        }

        public void Select(RenderBody body)
        {
            if (selected != null)
            {
                selected.IsSelected = false;
            }
                    
            selected = body;
            body.IsSelected = true;
        }
        
        public bool MakeStarAt(Vector2f position)
        {
            RenderBody b = null;
            bool found = false;
            
            foreach (RenderBody body in bodies)
            {
                if (body.Contains(position) && !(body is Star))
                {
                    body.Exists = false;
                    Star star = new Star(body.Position, body.Mass, body.Velocity, body.Density);
                    AddBody(star);
                    Select(star);
                    b = body;

                    found = true;
                    break;
                }
            }

            RemoveBody(b);
            return found;
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
                if (body.Position.X + body.Radius > topx)
                {
                    topx = body.Position.X + body.Radius;
                }

                if (body.Position.X - body.Radius < bottomx)
                {
                    bottomx = body.Position.X - body.Radius;
                }

                if (body.Position.Y + body.Radius > topy)
                {
                    topy = body.Position.Y + body.Radius;
                }

                if (body.Position.Y - body.Radius < bottomy)
                {
                    bottomy = body.Position.Y - body.Radius;
                }
            }

            float high_x = topx > Math.Abs(bottomx) ? topx : Math.Abs(bottomx);
            float high_y = topy > Math.Abs(bottomy) ? topy : Math.Abs(bottomy);

            Vector2f bottomleftbound = new Vector2f(-high_x, -high_y) * 1.1f;
            Vector2f size = bottomleftbound * -2;
            
            /**Vector2f bottomleftbound = new Vector2f(bottomx, bottomy);
            Vector2f range = (new Vector2f(topx, topy) - bottomleftbound);
            
            Vector2f size = range + new Vector2f(2, 2) + range * 20.0f;
            bottomleftbound -= new Vector2f(1, 1) + range * 10.0f;**/

            return new Rectangle(bottomleftbound, size);
        }

        private QuadTree GetQuadTree()
        {
            QuadTree tree = new QuadTree(GetAABB(), null);

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
            View view = window.GetView();
            
            Vector2f real_center = new Vector2f(view.Center.X, -view.Center.Y);
            Vector2f bottom_left = real_center - view.Size / 2;
            Rectangle domain = new Rectangle(bottom_left, view.Size);

            foreach (RenderBody body in bodies)
            {
                if (domain.FullyContains(body))
                {
                    body.DrawOutline = DrawOutlines;

                    target.Draw(body);
                }
            }
        }

        private void ApplyForces(float time)
        {
            foreach (Body body in bodies)
            {
                body.ApplyForce(body.GetForceFrom(Tree), time);
            }
        }

        private void UpdateAll(float time)
        {
            foreach (Body body in bodies)
            {
                body.Update(time);
            }
        }

        private void ResolveCollisions()
        {
            //Find collisions
            List<Pair> collisions = new List<Pair>();

            collisions.AddRange(Body.GetAllCollisions(Tree));

            //Resolve collisions
            foreach (Pair collision in collisions)
            {
                collision.Resolve(this);
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
            tree = GetQuadTree();
            ApplyForces(time);
            UpdateAll(time);
            ResolveCollisions();
            ApplyStarHeat(time);
        }
    }
}