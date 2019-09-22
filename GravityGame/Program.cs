﻿using System;
using System.Collections.Generic;
using System.Threading;
using GravityGame.Extension;
using NUnit.Framework.Constraints;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GravityGame
{
    //TODO: Try to find some way to simplify dealing with the selected object or at least make variables more descriptive
    internal class Program
    {
        public static RenderWindow window;
        public static View view;
        public static Vector2f view_offset = new Vector2f(0, 0);
        public static float view_scale = 2.5f;
        public static Scene scene;
        public static float time_scale = 1;
        public static Font font;
        
        public static bool panning = false;
        public static bool firing = false;
        public static float spawn_radius = 5;
        public static Vector2f mouse_original_pos;
        public static Vector2f mouse_fire_offset;
        public static Random R;

        public static void Main(string[] args)
        {
            window = new RenderWindow(new VideoMode(800, 600), "The Game of Life");
            view = new View();

            font = new Font(GetDirectory() + "monsterrat.ttf");

            R = new Random();
            
            scene = new Scene();
            //scene.AddBody(new Ship(new Vector2f(0, 0), new Vector2f(0, 0), new Life(300, 1, 1, 1000)));

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

                                
                window.Clear();
                
                scene.Update(time_scale * time, GetImportantArea());
                    
                if (panning)
                {
                    Vector2f diff = (Vector2f) Mouse.GetPosition() - mouse_original_pos;
                    view_offset -= 0.5f * diff / view_scale;
                    Mouse.SetPosition((Vector2i)mouse_original_pos);
                }
                
                //Start drawing
                UpdateView();

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
            view.Center = scene.Selected == null ? view_offset : InvY(scene.GetSelectedPosition()) + view_offset;
            window.SetView(view);
        }

        public static void OnClose(object sender, EventArgs e)
        {
            ((Window) sender).Close();
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
                float radius = 16000;
                float inner_radius = 400;
                float mass = 100;
                float mass_variance = 50;
                float velocity = 500;
                float velocity_variance = 25;

                scene.AddBody(new Star(new Vector2f(0, 0), 100000, new Vector2f(0, 0), 1));
                
                for (int i = 0; i < n; i++)
                {
                    float angle = NextFloatAbs(R, 2 * Mathf.PI);
                    float distance = NextFloatAbs(R, radius) + inner_radius;
                    float n_mass = mass + NextFloat(R, mass_variance);
                    
                    Vector2f velocity_unit = new Vector2f((float)Math.Cos(angle + Mathf.PI / 2), (float)Math.Sin(angle + Mathf.PI / 2));
                    Vector2f n_velocity = velocity_unit * (velocity + NextFloat(R, velocity_variance)) / Mathf.Pow(distance / 100, 0.33f);
                    
                    Vector2f n_position = new Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
                    
                    scene.AddBody(new Planet(n_position, n_mass, n_velocity, 1, 300));
                }
            }
            else if(args.Code == Keyboard.Key.Return)
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))
                {
                    time_scale = 1;
                }
                else
                {
                    view_offset = new Vector2f(0, 0);
                }
            }
            else if(args.Code == Keyboard.Key.BackSpace)
            {
                scene.DrawText = !scene.DrawText;
            }
        }

        public static string GetDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
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
                    view_scale *= 1.1f;
                }
                else
                {
                    view_scale /= 1.1f;
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
                        view_offset = new Vector2f(0, 0);
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