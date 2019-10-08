using System;
using System.Collections.Generic;
using System.Threading;
using GravityGame.Extension;
using NUnit.Framework.Constraints;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
    //TODO: Cursor gets stuck when you right click on the top strip of the window
    //TODO: Try to find some way to simplify dealing with the selected object or at least make variables more descriptive
    internal class Program
    {
        private static RenderWindow window;
        private static View view;
        private static Scene scene;
        private static float time_scale = 1;
        
        private static bool panning = false;
        private static bool firing = false;
        private static float spawn_radius = 5;
        private static Vector2f mouse_original_pos;
        private static Vector2f mouse_fire_offset;
        private const float add_mass_period = 0.2f;
        private static float add_mass_time = 0.0f;

        public static float ViewScale { get; private set; } = 2.5f;
        public static Vector2f ViewOffset { get; private set; } = new Vector2f(0, 0);
        public static Random R { get; private set; }
        public static Font Font { get; private set; }

        //TODO: Multithread drawing and updating?
        //TODO: Add a leaderboard for number of planets colonized
        public static void Main(string[] args)
        {
            window = new RenderWindow(new VideoMode(800, 600), "The Game of Life");
            view = new View();

            Font = new Font(GetResourcesDirectory() + "\\Fonts\\monsterrat.ttf");

            R = new Random();
            
            scene = new Scene();

            Gradient g = new Gradient();
            List<GradientKey> keys = new List<GradientKey>();
            keys.Add(new GradientKey(0, new Colorf(0, 0, 1, 1)));
            keys.Add(new GradientKey(300, new Colorf(0, 1, 0, 1)));
            keys.Add(new GradientKey(5000, new Colorf(1, 0, 0, 1)));
            g.Keys = keys;
            Mathf.TemperatureColorGradient = g;

            window.Closed += OnClose;
            window.MouseWheelMoved += OnMouseWheelScroll;
            window.MouseButtonPressed += OnMouseButtonPress;
            window.MouseButtonReleased += OnMouseButtonRelease;
            window.KeyPressed += OnKeyPress;

            Clock clock = new Clock();
            float max_time_step = 1 / 60f;
            
            while (window.IsOpen)
            {
                window.DispatchEvents();

                float time = Math.Min(max_time_step, clock.ElapsedTime.AsSeconds());
                clock.Restart();
                
                add_mass_time += time;
                while (add_mass_time > add_mass_period)
                {
                    add_mass_time -= add_mass_period;
                    AddMatter(true);
                }
                
                scene.Update(time_scale * time, GetImportantArea());
                    
                if (panning)
                {
                    Vector2f diff = (Vector2f) Mouse.GetPosition() - mouse_original_pos;
                    ViewOffset -= 0.5f * diff / ViewScale;
                    Mouse.SetPosition((Vector2i)mouse_original_pos);
                }
                
                //Start drawing
                UpdateView();
                
                window.Clear();
                window.Draw(scene);

                if (firing)
                {
                    Vector2f mouse_pos = GetMouseCoordsWorld();

                    VertexArray arr = new VertexArray();
                    arr.Append(new Vertex(InvY( scene.GetSelectedPosition()) + mouse_fire_offset, Color.Green));
                    arr.Append(new Vertex(mouse_pos, Color.Red));

                    arr.PrimitiveType = PrimitiveType.Lines;

                    window.Draw(arr);
                }

                if (!panning)
                {
                    CircleShape ghost = new CircleShape();
                    ghost.Radius = spawn_radius;
                    ghost.FillColor = new Color(0, 255, 0, 100);
                    ghost.OutlineColor = new Color(0, 255, 0, 100);
                    ghost.Position = firing ? InvY(scene.GetSelectedPosition()) + mouse_fire_offset : GetMouseCoordsWorld();
                    ghost.Position -= new Vector2f(spawn_radius, spawn_radius);
                    window.Draw(ghost);
                }

                window.Display();
            }
        }

        private static Rectangle GetImportantArea()
        {
            Vector2f size = view.Size * 1.2f;
            Vector2f topleft = view.Center - view.Size / 2f;
            
            return new Rectangle(topleft, size);
        }

        public static void UpdateView()
        {
            view.Size = (Vector2f) window.Size / ViewScale;
            view.Center = scene.Selected == null ? ViewOffset : InvY(scene.GetSelectedPosition()) + ViewOffset;
            window.SetView(view);
        }

        public static void OnClose(object sender, EventArgs e)
        {
            ((Window) sender).Close();
        }

        private static void GenerateDisk(int n)
        {
            Star star = new Star(new Vector2f(0, 0), 100000, new Vector2f(0, 0), 1);
            scene.AddBody(star);
            scene.ForceBodyBufferInsert();
                
            for (int i = 0; i < n; i++)
            {
                AddMatter(false);
            }
        }

        public static void OnKeyPress(object sender, EventArgs e)
        {
            KeyEventArgs args = (KeyEventArgs) e;

            if (args.Code == Keyboard.Key.Delete)
            {
                scene.DrawOutlines = !scene.DrawOutlines;
            }
            else if (args.Code == Keyboard.Key.Escape)
            {
                scene.Deselect();
            }
            else if(args.Code == Keyboard.Key.P)
            {
                Random R = new Random(1);
                
                Console.Write("Enter number of planets: ");

                int n = Convert.ToInt32(Console.ReadLine());
                GenerateDisk(n);
            }
            else if(args.Code == Keyboard.Key.Return)
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))
                {
                    time_scale = 1;
                }
                else
                {
                    ViewOffset = new Vector2f(0, 0);
                }
            }
            else if(args.Code == Keyboard.Key.BackSpace)
            {
                scene.DrawText = !scene.DrawText;
            }
            else if(args.Code == Keyboard.Key.R)
            {
                Vector2f mouse_pos = GetMouseCoordsWorld();
                
                scene.EvolveLifeAtPosition(InvY(mouse_pos));
            }
        }

        private static void AddMatter(bool leech_mass)
        {
            Star star = scene.GetMainStar();

            if (star == null)
            {
                return;
            }
            
            float radius = 30000;
            float inner_radius = 400;
            float mass = 100;
            float mass_variance = 50;
            float velocity_variance = 0.1f;
            
            float angle = NextFloatAbs(R, 2 * Mathf.PI);
            float vel_var = 1 + NextFloat(R, velocity_variance);
            float distance = NextFloatAbs(R, radius) + inner_radius;
            float n_mass = mass + NextFloat(R, mass_variance);
                    
            Vector2f n_position = new Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;

            Planet planet = new Planet(n_position, n_mass, new Vector2f(0, 0), 1, 300);
            planet.AutoOrbit(star);
            scene.AddBody(planet);

            if (leech_mass)
            {
                scene.LeechStarMass(n_mass * 0.6f);
            }
        }
        
        public static string GetDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetResourcesDirectory()
        {
            return GetDirectory() + "Resources";
        }
        
        private static float NextFloat(Random R, float amplitude)
        {
            float t = ((float)R.NextDouble() - 0.5f) * 2;
            return t * amplitude;
        }

        private static float NextFloatAbs(Random R, float amplitude)
        {
            return Math.Abs(NextFloat(R, amplitude));
        }
        
        public static void OnMouseWheelScroll(object sender, EventArgs e)
        {
            MouseWheelEventArgs args = (MouseWheelEventArgs) e;

            if (Keyboard.IsKeyPressed(Keyboard.Key.LControl) || panning)
            {
                if (args.Delta > 0)
                {
                    ViewScale *= 1.1f;
                }
                else
                {
                    ViewScale /= 1.1f;
                }
            }
            else if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))
            {
                if (args.Delta > 0)
                {
                    time_scale *= 1.1f;
                }
                else
                {
                    time_scale /= 1.1f;
                }
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
                    Vector2f mouse_pos = GetMouseCoordsWorld();
                    bool found = scene.SelectAt(InvY(mouse_pos));

                    if (!found)
                    {
                        firing = true;

                        mouse_fire_offset = GetMouseCoordsRelative();
                    }
                    else
                    {
                        ViewOffset = new Vector2f(0, 0);
                    }
                }
                else if (args.Button == Mouse.Button.Right)
                {
                    panning = true;
                    
                    window.SetMouseCursorVisible(false);
                    mouse_original_pos = (Vector2f)Mouse.GetPosition();
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

        public static Vector2f GetMouseCoordsRelative()
        {
            return GetMouseCoordsWorld() - InvY(scene.GetSelectedPosition());
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
                Vector2f position = scene.GetSelectedPosition() + InvY(mouse_fire_offset);
                Planet p = new Planet(position, Mathf.PI * spawn_radius * spawn_radius, scene.GetSelectedVelocity() + InvY(mouse_pos) - position, 1, 300);

                if (Keyboard.IsKeyPressed(Keyboard.Key.LShift) && scene.Selected != null)
                {
                    p.AutoOrbit(scene.Selected);
                }
                
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