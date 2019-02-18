using System;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class Planet : TemperatureBody
    {
        public static float growth_rate = 1;
        public static float life_chance = 1 / 1200.0f;
        public static float tech_chance = 1 / 1200.0f;
        public Life Life { get; set; }
        public bool DrawText { get; set; }
        public bool HasLife => Life != null;

        public float CarryingCapacity
        {
            get
            {
                if (!HasLife) return 0;
                float temp_diff = Math.Abs(Temperature - Life.NormalTemp);
                float temp_mod = temp_diff / (float)Math.Pow(Life.TechLevel, 2) / 10;
                float temp_divisor = 1 + temp_mod;
                float value = 1000 * Mathf.Pow(Life.TechLevel, 8) / temp_divisor - temp_mod * 100;
                
                if (value < 1)
                {
                    return 1;
                }

                return value;
            }
        }

        private void FormatText(Text text, float level, RenderWindow window)
        {
            View view = window.GetView();
            
            text.Position = Position.InvY() + new Vector2f(-Radius, Radius + level * view.Size.Y / 30);
            text.CharacterSize = 50;
            float scale = view.Size.X / window.Size.X;
            text.Scale = 0.5f * new Vector2f(scale, scale);
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
                    Text population_text = new Text(((int) Life.Population).ToString(), Program.font);
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

        public override void Update(float time)
        {
            base.Update(time);
            UpdateLife(time);
        }

        public void UpdateLife(float time)
        {
            if (HasLife)
            {
                float growth = time * growth_rate * Life.Population * (1 - Life.Population / CarryingCapacity);
                
                if (Life.Population < 2)
                {
                    Life = null;
                    return;
                }

                Life.Population += growth;

                if (Program.R.NextDouble() > 1 - tech_chance)
                {
                    Life.TechLevel++;
                }
            }
            else
            {
                //Randomly evolve life
                if (Program.R.NextDouble() > 1 - life_chance)
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