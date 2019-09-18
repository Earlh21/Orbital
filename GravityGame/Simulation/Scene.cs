using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
    public class Scene : Drawable
    {
        private readonly int THREAD_COUNT = Environment.ProcessorCount;
        
        private List<RenderBody> bodies;
        private List<Star> star_cache;
        private List<RenderBody> body_buffer;
        private Body selected;
        private QuadTree collision_tree;
        private QuadTree iteration_tree;

        private List<Thread> forces_threads;
        private List<AutoResetEvent> forces_handles;
        private List<AutoResetEvent> main_handles;
        
        public float TimeStep { get; set; } = 0.15f;
        private float last_step;

        public bool DrawOutlines { get; set; }
        public bool DrawText { get; set; }
        public Body Selected => selected;

        public List<RenderBody> Bodies => bodies;

        public Scene()
        {
            bodies = new List<RenderBody>();
            star_cache = new List<Star>();
            body_buffer = new List<RenderBody>();
            last_step = TimeStep;

            forces_threads = new List<Thread>();
            forces_handles = new List<AutoResetEvent>();
            main_handles = new List<AutoResetEvent>();
            for (int i = 0; i < THREAD_COUNT; i++)
            {
                Thread thread = new Thread(() => PartialForces(i));
                forces_threads.Add(thread);
                forces_handles.Add(new AutoResetEvent(false));
                main_handles.Add(new AutoResetEvent(false));
            }

            foreach (Thread thread in forces_threads)
            {
                thread.Start();
            }
        }

        private void PartialForces(int thread_id)
        {
            while (true)
            {
                int length = bodies.Count;
                
                int section_length = length / THREAD_COUNT;
                
                int low = section_length * thread_id;
                int high = section_length * (thread_id + 1);

                for (int i = low; i < high; i++)
                {
                    Body body = bodies[i];
                    if (body.Exists && !body.ForcesDone)
                    {
                        body.Force += body.GetForceFrom(iteration_tree);
                        body.ForcesDone = true;
                    }
                }

                main_handles[thread_id].Set();
                forces_handles[thread_id].WaitOne();
            }
        }
        
        public void AddBody(RenderBody body)
        {
            body_buffer.Add(body);
        }

        private void AddBodies()
        {
            foreach (RenderBody body in body_buffer)
            {
                bodies.Add(body);
                if (body is Star)
                {
                    star_cache.Add((Star)body);
                }

                if (body is Ship)
                {
                    int d = 3;
                }
            }

            body_buffer.Clear();
        }

        public Vector2f GetTotalMomentum()
        {
            Vector2f total = new Vector2f(0, 0);
            foreach (Body body in bodies)
            {
                total += body.Momentum;
            }

            return total;
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
                selected = null;
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

        public void Select(Body body)
        {
            if (selected != null)
            {
                selected.IsSelected = false;
            }

            if (body.IsSelectable)
            {
                selected = body;
                body.IsSelected = true;
            }
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
                    b = body;

                    found = true;
                    break;
                }
            }

            if (found)
            {
                RemoveBody(b);
            }

            return found;
        }

        public Rectangle WorldSize()
        {
            return new Rectangle(new Vector2f(-50000, -50000), new Vector2f(100000, 100000));
        }

        public Rectangle GetAABB()
        {
            float highest = 0;

            foreach (Body body in bodies)
            {
                if (body.Position.X + body.Radius > highest)
                {
                    highest = body.Position.X + body.Radius;
                }

                if (body.Position.X - body.Radius < -highest)
                {
                    highest = -body.Position.X + body.Radius;
                }

                if (body.Position.Y + body.Radius > highest)
                {
                    highest = body.Position.Y + body.Radius;
                }

                if (body.Position.Y - body.Radius < -highest)
                {
                    highest = -body.Position.Y + body.Radius;
                }
            }

            Vector2f bottomleftbound = new Vector2f(-highest, -highest) * 1.1f;
            Vector2f size = bottomleftbound * -2;

            return new Rectangle(bottomleftbound, size);
        }

        private QuadTree GetQuadTree(bool iteration)
        {
            QuadTree tree = new QuadTree(WorldSize(), null, iteration);

            foreach (Body body in bodies)
            {
                if (iteration)
                {
                    if (!WorldSize().FullyContains(body.NextPosition, body.Radius))
                    {
                        body.Exists = false;
                        continue;
                    }
                }
                else
                {
                    if (!WorldSize().FullyContains(body))
                    {
                        body.Exists = false;
                        continue;
                    }
                }

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
                if (domain.PartiallyContains(body) && body.Exists)
                {
                    body.DrawOutline = DrawOutlines;
                    if (body is IDrawsText)
                    {
                        ((IDrawsText) body).DrawText = DrawText;
                    }

                    target.Draw(body);
                }
            }
        }

        private void Iterate()
        {
            foreach (Body body in bodies)
            {
                if (body.Exists)
                {
                    body.Iterate(TimeStep);
                    body.Started = true;
                    body.ForcesDone = false;
                }
            }
        }
        
        private void MoveAll(float time)
        {
            foreach (Body body in bodies)
            {
                if (body.Exists)
                {
                    body.Update(this, time);
                    body.InterpolatePosition(last_step / TimeStep);
                }
            }
        }

        private void ResolveCollisions()
        {
            //Find collisions
            List<Pair> collisions = new List<Pair>();

            collisions.AddRange(Body.GetAllCollisions(collision_tree));

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

        public void DoForces(float time)
        {
            for (int i = (int) ((last_step - time) / TimeStep * bodies.Count); i < (int) (last_step / TimeStep *
                                                                                          bodies.Count) && i < bodies.Count; i++)
            {
                if (i < 0)
                {
                    continue;
                }
                
                Body body = bodies[i];
                if (body.Exists && !body.ForcesDone)
                {
                    body.Force += body.GetForceFrom(iteration_tree);
                    body.ForcesDone = true;
                }
            }
        }
        
        public void FinishForces()
        {
            foreach (Body body in bodies)
            {
                if (!body.ForcesDone)
                {
                    body.Force += body.GetForceFrom(iteration_tree);
                    body.ForcesDone = true;
                }
            }
        }

        public void StartBodies()
        {
            foreach (Body body in bodies)
            {
                if (!body.Started && body.Exists)
                {
                    body.Iterate(TimeStep - last_step);
                    body.Started = true;
                }
            }
        }

        public void Update(float time, RenderWindow window)
        {         
            last_step += time;
            
            collision_tree = GetQuadTree(false);

            if (last_step < TimeStep)
            {
                //DoForces(time);
            }
            
            while(last_step > TimeStep)
            {
                foreach (AutoResetEvent main_handle in main_handles)
                {
                    main_handle.WaitOne();
                }
                
                last_step -= TimeStep;
                Iterate();
                iteration_tree = GetQuadTree(true);

                foreach (AutoResetEvent forces_handle in forces_handles)
                {
                    forces_handle.Set();
                }
            }
            
            StartBodies();
            ResolveCollisions();
            MoveAll(time);
            ApplyStarHeat(time);
            AddBodies();
        }
    }
}