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
        private static float life_chance = 1 / 1000.0f;
        private static float laser_chance = 1 / 8.0f;
        private static float ship_chance = 1 / 7.5f;
        private static float matter_chance = 1 / 50.0f;
        private static float satellite_chance = 1 / 50.0f;
        
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
        private void FireShip(Scene scene, Body target)
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

            Ship ship;
            
            if (Life.TechLevel <= 1)
            {
                ship = new Ship(Position + velocity.Unit() * Radius * 1.5f, velocity, life);
            }
            else if (Life.TechLevel <= 3)
            {
                ship = new ThrusterShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
            }
            else
            {
                double random = Program.R.NextDouble();

                if (random > 0.5)
                {
                    ship = new OrbitShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
                }
                else
                {
                    ship = new ThrusterShip(Position + velocity.Unit() * Radius * 1.5f, velocity, life, target);
                }
            }

            scene.AddBody(ship);
        }

        private void FireMatter(Scene scene, Body target)
        {
            float speed = Mathf.Sqrt(2 * Mathf.G * Mass / Radius) * (10.0f + Life.TechLevel / 2.0f);
            float mass = 12 + Life.TechLevel * 6;
            
            float distance = Distance(target);
            float time = distance / speed;
            Vector2f other_position = target.Position + target.Velocity * time;
            float angle = Mathf.AngleTo(Position, other_position);
            
            Vector2f velocity = speed * new Vector2f(Mathf.Cos(angle), Mathf.Sin(angle));
            Planet bullet = new Planet(Position + velocity.Unit() * Radius * 2.5f, mass, velocity, 1, 100000);
            
            scene.AddBody(bullet);
        }

        private void FireLaser(Scene scene, Planet target)
        {
            float heat_change_sign = Mathf.Sign(target.Temperature - target.Life.NormalTemp);
            Color color;
            float heat_change;

            if (heat_change_sign > 0 || target.Life.TechLevel > 2)
            {
                color = Color.Red;
                heat_change = 10000 + Mathf.Pow(Life.TechLevel, 1.5f) * 2500;
            }
            else
            {
                color = Color.Blue;
                heat_change = -1000 - Life.TechLevel * 100;
            }
            
            HeatLaserEffect laser = new HeatLaserEffect(this, target, color, heat_change, 3.0f);
            scene.AddEffect(laser);
        }

        private void FireLaserSatellite(Scene scene)
        {
            float angle = Program.R.Next();
            float speed = Mathf.Sqrt(2 * Mathf.G * Mass / Radius) * 0.8f;
            Vector2f velocity = Velocity + new Vector2f(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            Vector2f position = Position + velocity.Unit() * Radius * 1.5f;

            Satellite satellite = new LaserSatellite(position, velocity, this, Life.Faction);
            scene.AddBody(satellite);
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
                
                if (Life.TechLevel >= 1 && Program.R.NextDouble() < 1 - Math.Pow(1 - ship_chance, time))
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

                if (Life.TechLevel >= 2 && Program.R.NextDouble() < 1 - Math.Pow(1 - laser_chance, time))
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
                    
                    FireLaser(scene, (Planet)target);
                }
                
                if (Life.TechLevel >= 3 && Program.R.NextDouble() < 1 - Math.Pow(1 - matter_chance, time))
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

                if (Life.TechLevel >= 2 && Program.R.NextDouble() < 1 - Math.Pow(1 - satellite_chance, time))
                {
                    FireLaserSatellite(scene);
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