using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
    internal class Program
    {
        public static RenderWindow window;
        public static View view;
        
        public static void Main(string[] args)
        {
            window = new RenderWindow(new VideoMode(800, 600), "The Game of Life");

            //Scene specification
            List<RenderBody> bodies = new List<RenderBody>();
            bodies.Add(new Planet(new Vector2f(0, 0), 100, new Vector2f(0,0), 1, 300));
            
            view = new View();
            SetView(new Vector2f(0, 0), 2.5f);

            Clock clock = new Clock();

            Gradient g = new Gradient();
            List < GradientKey > keys = new List<GradientKey>();
            keys.Add(new GradientKey(0, new Colorf(0, 0, 1, 1)));
            keys.Add(new GradientKey(300, new Colorf(0, 1, 0, 1)));
            keys.Add(new GradientKey(10000, new Colorf(1, 0, 0, 1)));
            g.Keys = keys;
            Mathf.TemperatureColorGradient = g;
            
            while (window.IsOpen)
            {
                window.DispatchEvents();

                float time = clock.ElapsedTime.AsSeconds();
                clock.Restart();
                
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

                Vector2f bottomleftbound = new Vector2f(bottomx, bottomy) - new Vector2f(1, 1);
                Vector2f range = new Vector2f(topx, topy) - bottomleftbound + new Vector2f(1, 1);
                bottomleftbound *= 1.1f;
                range *= 1.2f;

                QuadTree tree = new QuadTree(bottomleftbound, range);

                foreach (Body body in bodies)
                {
                    tree.Insert(body);
                }
                
                tree.CalculateCenterOfMass();

                foreach (Body body in bodies)
                {
                    body.ApplyForce(body.GetForceFrom(tree), time);
                    body.Update(time);
                }
                
                window.Clear();
                Draw(bodies);
                window.Display();
            }
        }

        public static void Draw(List<RenderBody> bodies)
        {
            foreach (RenderBody body in bodies)
            {
                window.Draw(body);
            }
        }

        public static void ResolveCollision(Pair pair, List<RenderBody> bodies)
        {
            
        }

        public static void SetView(Vector2f position, float scale)
        {
            view.Size = (Vector2f) window.Size / scale;
            view.Center = position;
            window.SetView(view);
        }
    }
}