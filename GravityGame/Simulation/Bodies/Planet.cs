using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class Planet : TemperatureBody, IDrawsText
    {
        public static float growth_rate = 1;
        public static float life_chance = 1 / 1000.0f;
        public Life Life { get; set; }
        public bool HasLife => Life != null;
        public override Color? OutlineColor => HasLife ? (Color?)Civilizations.GetColor(Life.Faction) : null;

        private void FormatText(Text text, float level, RenderWindow window)
        {
            View view = window.GetView();
            
            text.Position = Position.InvY() + new Vector2f(-Radius, Radius + level * view.Size.Y / 30);
            text.CharacterSize = 50;
            float scale = view.Size.X / window.Size.X;
            text.Scale = 0.5f * new Vector2f(scale, scale);
        }

        
        //TODO: Improve targeting heuristic and measure effectiveness of various methods
        public void FireShip(Scene scene, Body target)
        {
            Life life = new Life(Temperature, Life.Faction, Life.TechLevel, 100);

            Star star = scene.GetMainStar();

            Vector2f force = new Vector2f(0, 0);
            if (star != null)
            {
                //force = target.GetForceFrom(star);
            }
            Vector2f acceleration = force / target.Mass;
            
            float speed = Mathf.Sqrt(2 * Mathf.G * Mass / Radius) * 1.1f;
            float distance = Distance(target) * 0.75f;
            float time = distance / speed;
            Vector2f other_position = target.Position + target.Velocity * time + acceleration.Multiply(acceleration) * time / 2;
            float angle = Mathf.AngleTo(Position, other_position);

            Vector2f velocity = speed * new Vector2f(Mathf.Cos(angle), Mathf.Sin(angle));
            Ship ship = new ThrusterShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
            
            scene.AddBody(ship);
            
            LaserEffect effect = new LaserEffect(Position, target.Position, Color.Red, 1.5f);
            
            scene.AddEffect(effect);
        }

        private void FireMatter(Scene scene, Body target)
        {
            float speed = Mathf.Sqrt(2 * Mathf.G * Mass / Radius) * (10.0f + Life.TechLevel * 2);
            float mass = 12 + Life.TechLevel;
            
            float distance = Distance(target);
            float time = distance / speed;
            Vector2f other_position = target.Position + target.Velocity * time;
            float angle = Mathf.AngleTo(Position, other_position);
            
            Vector2f velocity = speed * new Vector2f(Mathf.Cos(angle), Mathf.Sin(angle));
            Planet bullet = new Planet(Position + velocity.Unit() * Radius * 2.5f, mass, velocity, 1, 100000);
            
            scene.AddBody(bullet);
        }
        
        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);

            if (DrawText)
            {
                RenderWindow window = (RenderWindow) target;
                View view = window.GetView();

                Text temperature_text = new Text((int) Temperature + " K", Program.Font);
                temperature_text.Color = Mathf.TemperatureColorGradient.GetColor(Temperature);
                FormatText(temperature_text, 0, window);

                target.Draw(temperature_text);

                if (HasLife)
                {
                    Text population_text = new Text(Format.PopulationText(Life.Population), Program.Font);
                    population_text.Color = Color.White;
                    Text tech_level_text = new Text(Life.TechLevel.ToString(), Program.Font);
                    tech_level_text.Color = Color.White;
                    Text faction_text = new Text(Civilizations.GetName(Life.Faction), Program.Font);
                    faction_text.Color = Civilizations.GetColor(Life.Faction);

                    FormatText(population_text, 1, window);
                    FormatText(tech_level_text, 2, window);
                    FormatText(faction_text, 3, window);
                    
                    window.Draw(population_text);
                    window.Draw(tech_level_text);
                    window.Draw(faction_text);
                }
            }
        }

        public override void Update(Scene scene, float time)
        {
            base.Update(scene, time);
            UpdateLife(scene, time);
        }

        public void UpdateLife(Scene scene, float time)
        {
            if (HasLife)
            {
                if (Life.IsDead)
                {
                    Life = null;
                    return;
                }
                
                //TODO: Make this chance a variable instead of a magic number
                if (Program.R.NextDouble() < 1 - Math.Pow(1 - 1 / 7.5f, time))
                {
                    Body[] buffer = new Body[5];
                    int height = 2;
                    BodyFilter filter = new BodyFilter(typeof(Planet), BodyFilter.LifeFilter.False);

                    int count = scene.QuadTree.FindNearbyBodies(Position, height, filter, buffer);

                    if (count < 1)
                    {
                        return;
                    }
                    
                    int index = Program.R.Next(count);
                    Body target = buffer[index];
                    
                    FireShip(scene, target);
                }

                if (Program.R.NextDouble() < 1 - Math.Pow(1 - 1 / 60.0f, time))
                {
                    Body[] buffer = new Body[5];
                    int height = 2;
                    BodyFilter filter = new BodyFilter(typeof(Planet), BodyFilter.LifeFilter.True, Life.Faction, false);
                    
                    int count = scene.QuadTree.FindNearbyBodies(Position, height, filter, buffer);

                    if (count < 1)
                    {
                        return;
                    }
                    
                    int index = Program.R.Next(count);
                    Body target = buffer[index];
                    
                    FireMatter(scene, target);
                }
                
                Life.Update(time, Temperature);
            }
            else
            {
                //Randomly evolve life
                if (Program.R.NextDouble() < 1 - Math.Pow(1 - life_chance, time))
                {
                    EvolveLife();
                }
            }
        }

        public void EvolveLife()
        {
            Life = new Life(Temperature);
        }
        
        public Planet(Vector2f position, float mass, Vector2f velocity, float density, float temperature) : base(position,
            mass, velocity, density, temperature)
        {
            Life = null;
        }
    }
}