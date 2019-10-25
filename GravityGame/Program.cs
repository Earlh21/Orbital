using System;
using System.Collections.Generic;
using System.Threading;
using GravityGame.Extension;
using NUnit.Framework.Constraints;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using GravityGame.Guis;
using GravityGame.Guis.PrebuiltGuis;

namespace GravityGame
{
    //TODO: Cursor gets stuck when you right click on the top strip of the window
    //TODO: Try to find some way to simplify dealing with the selected object or at least make variables more descriptive
    internal class Program
    {
        private static bool can_fire = false;
        
        public static RenderWindow window;
        private static View view;
        private static Scene scene;
        private static float time_scale = 1;
        
        private static bool panning = false;
        private static bool firing = false;
        private static float spawn_radius = 5;
        private static Vector2f mouse_original_pos;
        private static Vector2f mouse_fire_offset;
        private const float add_mass_period = 0.15f;
        private static float add_mass_time = 0.0f;

        private static PlanetTypeMap planet_type_map;

        public static float ViewScale { get; private set; } = 1.0f;
        public static Vector2f ViewOffset { get; private set; } = new Vector2f(0, 0);
        public static Random R { get; private set; }
        public static Font Font { get; private set; }
        public static float Time { get; private set; }
        public static Vector2f ViewSize => view.Size;

        //TODO: Multithread drawing and updating?
        //TODO: Add a leaderboard for number of planets colonized
        public static void Main(string[] args)
        {
            ContextSettings settings = new ContextSettings();
            settings.AntialiasingLevel = 8;
            
            window = new RenderWindow(new VideoMode(800, 600), "The Game of Life", Styles.Default, settings);
            view = new View();
            Font = new Font(GetResourcesDirectory() + "\\Fonts\\monsterrat.ttf");
            R = new Random();
            scene = new Scene();
            planet_type_map = new PlanetTypeMap(400, 30000, 18, 4000.0f);

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
            float max_time_step = 1 / 100f;
            
            Gui gui = new Gui();
            GuiText text = new GuiText();
            GuiText text2 = new GuiText();
            GuiText text3 = new GuiText();
            ColumnContainer column = new ColumnContainer();
            
            column.BackgroundColor = Color.Blue;
            
            TextTest(text);
            TextTest(text2);
            TextTest(text3);
            
            column.AddEntry(text);
            column.AddEntry(text2);
            column.AddEntry(text3);
            
            gui.Contents.AddEntry(column);
            
            PlanetInfoGui planet_info = new PlanetInfoGui(null);
            
            while (window.IsOpen)
            {
                window.DispatchEvents();

                float timestep = Math.Min(max_time_step, time_scale * clock.ElapsedTime.AsSeconds());
                clock.Restart();

                Time += timestep;
                
                add_mass_time += timestep;
                while (add_mass_time > add_mass_period)
                {
                    add_mass_time -= add_mass_period;
                    AddMatter(true);
                }
                
                scene.Update(timestep, GetImportantArea());
                planet_type_map.Update(scene, timestep);
                    
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

                window.Draw(planet_info.GetGUI());
                //window.Draw(gui);
                
                window.Display();
            }
        }

        private static void TextTest(GuiText text)
        {
            text.Font = Font;
            text.Color = Color.White;
            text.FontSize = 1;
            text.Contents = "Test";
        }

        public static Vector2f ScreenPositionToWorld(Vector2i pos)
        {
            return window.MapPixelToCoords(pos);
        }

        public static Vector2f ScreenSizeToWorld(Vector2i size)
        {
            float uvx = (float) size.X / window.Size.X;
            float uvy = (float) size.Y / window.Size.Y;
            
            return new Vector2f(uvx, uvy).Multiply(view.Size);
        }

        public static Vector2i WorldSizeToScreen(Vector2f size)
        {
            Vector2f uv = size.Divide(view.Size);
            return new Vector2i((int)(uv.X * window.Size.X), (int)(uv.Y * window.Size.Y));
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
            Star star = new Star(new Vector2f(0, 0), new Vector2f(0, 0), 1000000);
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
                if (scene.Selected != null)
                {
                    ViewOffset += scene.Selected.Position.InvY();
                    scene.Deselect();
                }
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
            
            float angle = NextFloatAbs(R, 2 * Mathf.PI);
            float distance = NextFloatAbs(R, radius) + inner_radius;
            float n_mass = mass + NextFloat(R, mass_variance);
                    
            Vector2f n_position = new Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;

            Planet.PlanetType closest = planet_type_map.GetClosestType(n_position);
            float rand = (float)R.NextDouble();
            Planet.PlanetType type;
            if (rand > 0.95f)
            {
                float rand2 = (float) R.NextDouble();

                if (rand2 < 1.0f / 3.0f)
                {
                    type = Planet.PlanetType.Gas;
                }
                else if (rand2 < 2.0f / 3.0f)
                {
                    type = Planet.PlanetType.Ocean;
                }
                else
                {
                    type = Planet.PlanetType.Rocky;
                }
            }
            else
            {
                type = closest;
            }

            Composition composition =
                type == Planet.PlanetType.Gas ? Composition.Gas(n_mass) : Composition.Rocky(n_mass);
            Planet planet = new Planet(n_position, new Vector2f(0, 0), composition, 300);

            float water_percent;
            if (type == Planet.PlanetType.Ocean)
            {
                water_percent = (float) R.NextDouble() * 0.1f + 0.9f;
                planet.WaterArea = planet.Area * water_percent;
            }
            else
            {
                water_percent = (float) R.NextDouble() * 0.2f;
                planet.WaterArea = planet.Area * water_percent;
            }
            
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
                    bool found = scene.Select(scene.Lookup(InvY(mouse_pos), 30 / ViewScale));

                    if (!found && can_fire)
                    {
                        firing = true;

                        mouse_fire_offset = GetMouseCoordsRelative();
                    }

                    if (found)
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
                float mass = Mathf.PI * spawn_radius * spawn_radius;
                Planet p = new Planet(position, scene.GetSelectedVelocity() + InvY(mouse_pos) - position, Composition.Rocky(mass), 300);

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