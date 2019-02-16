using System;
using System.Collections.Generic;
using System.Threading;
using GravityGame.Extension;
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
        public static Scene scene;

        public static bool panning = false;
        public static bool firing = false;
        public static float spawn_radius = 5;
        public static Vector2f mouse_original_pos;

        public static void Main(string[] args)
        {
            window = new RenderWindow(new VideoMode(800, 600), "The Game of Life");

            //Scene specification
            scene = new Scene();
            scene.AddBody(new Planet(new Vector2f(0, 50), 100, new Vector2f(-5, 0), 1, 300));
            scene.AddBody(new Planet(new Vector2f(0, -50), 100, new Vector2f(8, 0), 1, 300));
            scene.AddBody(new Planet(new Vector2f(0, -70), 50, new Vector2f(3, 0), 1, 300));
            
            view = new View();
            UpdateView();

            Clock clock = new Clock();

            Gradient g = new Gradient();
            List<GradientKey> keys = new List<GradientKey>();
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

                scene.Update(time);
                
                //DEBUGGING: write the total momentum of the system to the console
                Vector2f momentum = new Vector2f(0, 0);                
                foreach (Body body in scene.Bodies)
                {
                    momentum += body.Momentum;
                }
                Console.WriteLine(momentum.Length());

                //Start drawing
                window.Clear();

                window.Draw(scene);
                
                if (panning)
                {
                    view_position -= GetMouseCoordsWorld() - mouse_original_pos;
                    UpdateView();
                    mouse_original_pos = GetMouseCoordsWorld();
                }

                if (firing)
                {
                    Vector2f mouse_pos = GetMouseCoordsWorld();

                    VertexArray arr = new VertexArray();
                    arr.Append(new Vertex(mouse_original_pos, Color.Green));
                    arr.Append(new Vertex(mouse_pos, Color.Red));

                    arr.PrimitiveType = PrimitiveType.Lines;

                    window.Draw(arr);
                }

                CircleShape ghost = new CircleShape();
                ghost.Radius = spawn_radius;
                ghost.FillColor = new Color(0, 255, 0, 100);
                ghost.OutlineColor = new Color(0, 255, 0, 100);
                ghost.Position = firing ? mouse_original_pos : GetMouseCoordsWorld();
                ghost.Position -= new Vector2f(spawn_radius, spawn_radius);
                window.Draw(ghost);
                
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

            if (Keyboard.IsKeyPressed(Keyboard.Key.LControl) || panning)
            {
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
            else
            {
                if (args.Delta > 0)
                {
                    spawn_radius *= 1.1f;
                }
                else
                {
                    spawn_radius /= 1.1f;
                }
            }
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
                else if(args.Button == Mouse.Button.Middle)
                {
                    Vector2f mouse_pos = GetMouseCoordsWorld();

                    scene.MakeStarAt(InvY(mouse_pos));
                }
            }
        }

        public static Vector2f GetMouseCoordsWorld()
        {
            Vector2i mouse = Mouse.GetPosition(window);
            return window.MapPixelToCoords(mouse, view);
        }

        public static Vector2f InvY(Vector2f vector)
        {
            return new Vector2f(vector.X, -vector.Y);
        }

        public static void OnMouseButtonRelease(object sender, EventArgs e)
        {
            MouseButtonEventArgs args = (MouseButtonEventArgs) e;

            if (firing && args.Button == Mouse.Button.Left)
            {
                firing = false;

                Vector2f mouse_pos = GetMouseCoordsWorld();
                Planet p = new Planet(InvY(mouse_original_pos), Mathf.PI * spawn_radius * spawn_radius, InvY(mouse_pos - mouse_original_pos), 1, 300);
                scene.AddBody(p);
            }

            if (panning && args.Button == Mouse.Button.Right)
            {
                panning = false;

                window.SetMouseCursorVisible(true);
            }
        }
    }
}