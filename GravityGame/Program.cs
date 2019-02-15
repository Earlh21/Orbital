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
        public static Vector2f view_position = new Vector2f(0, 0);
        public static float view_scale = 2.5f;

        public static bool panning = false;
        public static bool firing = false;
        public static Vector2f mouse_original_pos;
        
        public static void Main(string[] args)
        {
            window = new RenderWindow(new VideoMode(800, 600), "The Game of Life");

            //Scene specification
            List<RenderBody> bodies = new List<RenderBody>();
            bodies.Add(new Planet(new Vector2f(-100, 0), 100, new Vector2f(0,0), 1, 300));
            bodies.Add(new Planet(new Vector2f(100, 0), 100, new Vector2f(0,0), 1, 300));
            
            view = new View();
            UpdateView();

            Clock clock = new Clock();

            Gradient g = new Gradient();
            List < GradientKey > keys = new List<GradientKey>();
            keys.Add(new GradientKey(0, new Colorf(0, 0, 1, 1)));
            keys.Add(new GradientKey(300, new Colorf(0, 1, 0, 1)));
            keys.Add(new GradientKey(10000, new Colorf(1, 0, 0, 1)));
            g.Keys = keys;
            Mathf.TemperatureColorGradient = g;

            window.Closed += OnClose;
            window.Resized += OnResize;
            window.MouseWheelMoved += OnMouseWheelScroll;
            window.MouseButtonPressed += OnMouseButtonPress;
            window.MouseButtonReleased += OnMouseButtonRelease;
            
            while (window.IsOpen)
            {
                window.DispatchEvents();

                float time = clock.ElapsedTime.AsSeconds() * 3.0f;
                clock.Restart();
                
                float topx = float.MinValue;
                float bottomx = float.MaxValue;
                float topy = float.MinValue;
                float bottomy = float.MaxValue;
                
                //Find bounds for quadtree
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

                Vector2f bottomleftbound = new Vector2f(bottomx, bottomy) * 1.1f - new Vector2f(1, 1);
                Vector2f range = new Vector2f(topx, topy) - bottomleftbound * 1.1f + new Vector2f(1, 1);

                //Construct tree
                QuadTree tree = new QuadTree(bottomleftbound, range);

                foreach (Body body in bodies)
                {
                    tree.Insert(body);
                }
                
                //Cache centers of mass
                tree.CalculateCenterOfMass();

                //Do NBody force
                foreach (Body body in bodies)
                {
                    body.ApplyForce(body.GetForceFrom(tree), time);
                    body.Update(time);
                }

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

                                
                window.Clear();
                
                if (panning)
                {
                    view_position += GetMouseCoordsWorld() - mouse_original_pos;
                    UpdateView();
                    mouse_original_pos = GetMouseCoordsWorld();
                }

                if (firing)
                {
                    Vector2f mouse_pos = GetMouseCoordsWorld();

                    VertexArray arr = new VertexArray();
                    arr.Append(new Vertex(mouse_original_pos,Color.Green));
                    arr.Append(new Vertex(mouse_pos, Color.Red));

                    arr.PrimitiveType = PrimitiveType.Lines;
                    
                    window.Draw(arr);
                }
                
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

        public static void UpdateView()
        {
            view.Size = (Vector2f) window.Size / view_scale;
            view.Center = view_position;
            window.SetView(view);
        }

        public static void OnClose(object sender, EventArgs e)
        {
            ((Window) sender).Close();
        }

        public static void OnResize(object sender, EventArgs e)
        {
            UpdateView();
        }

        public static void OnMouseWheelScroll(object sender, EventArgs e)
        {
            MouseWheelEventArgs args = (MouseWheelEventArgs) e;
            if (args.Delta > 0)
            {
                view_scale *= 1.1f;
            }
            else
            {
                view_scale /= 1.1f;
            }
            UpdateView();
        }

        public static void OnMouseButtonPress(object sender, EventArgs e)
        {
            MouseButtonEventArgs args = (MouseButtonEventArgs) e;

            if (!panning && !firing)
            {
                if (args.Button == Mouse.Button.Left)
                {
                    firing = true;

                    mouse_original_pos = GetMouseCoordsWorld();
                }
                else if (args.Button == Mouse.Button.Right)
                {
                    panning = true;
                    
                    window.SetMouseCursorVisible(false);
                    mouse_original_pos = GetMouseCoordsWorld();
                }
            }
        }

        public static Vector2f GetMouseCoordsWorld()
        {
            Vector2i mouse = Mouse.GetPosition(window);
            return window.MapPixelToCoords(mouse, view);
        }

        public static void OnMouseButtonRelease(object sender, EventArgs e)
        {
            MouseButtonEventArgs args = (MouseButtonEventArgs) e;

            if (firing && args.Button == Mouse.Button.Left)
            {
                firing = false;
            }
            
            if (panning && args.Button == Mouse.Button.Right)
            {
                panning = false;
                
                window.SetMouseCursorVisible(true);
            }
        }
    }
}