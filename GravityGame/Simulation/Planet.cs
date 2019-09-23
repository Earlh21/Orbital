using System;
using System.Runtime.Remoting.Messaging;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class Planet : TemperatureBody, IDrawsText
    {
        public static float growth_rate = 1;
        //This number is stupidly sensitive
        public static float life_chance = 1 / 1000.0f;
        public Life Life { get; set; }
        public bool HasLife => Life != null;
        private void FormatText(Text text, float level, RenderWindow window)
        {
            View view = window.GetView();
            
            text.Position = Position.InvY() + new Vector2f(-Radius, Radius + level * view.Size.Y / 30);
            text.CharacterSize = 50;
            float scale = view.Size.X / window.Size.X;
            text.Scale = 0.5f * new Vector2f(scale, scale);
        }

        private void FireShip(Scene scene, float angle)
        {
            float speed = Mathf.Sqrt(2 * Mathf.G * Mass / Radius) * 1.1f;

            Vector2f direction = new Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle));
            Vector2f ship_vel = Velocity + direction * speed;
            Life life = new Life(Temperature, Life.Faction, Life.TechLevel, Life.Population * 0.1f);
            Life.Population *= 0.999f;

            Ship ship = new Ship(Position + direction * Radius * 1.5f, ship_vel, life);
            scene.AddBody(ship);
        }
        
        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);

            if (DrawText)
            {
                RenderWindow window = (RenderWindow) target;
                View view = window.GetView();

                Text temperature_text = new Text((int) Temperature + " K", Program.font);
                temperature_text.Color = Mathf.TemperatureColorGradient.GetColor(Temperature);
                FormatText(temperature_text, 0, window);

                target.Draw(temperature_text);

                if (HasLife)
                {
                    Text population_text = new Text(Format.PopulationText(Life.Population), Program.font);
                    population_text.Color = Color.White;
                    Text tech_level_text = new Text(Life.TechLevel.ToString(), Program.font);
                    tech_level_text.Color = Color.White;
                    Text faction_text = new Text(Life.Faction.ToString(), Program.font);
                    faction_text.Color = Color.White;

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
                
                if (Program.R.NextDouble() < 1 - Math.Pow(1 - (1 / 15.0f), time))
                {
                    FireShip(scene, (float)Program.R.NextDouble() * Mathf.PI * 2);
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