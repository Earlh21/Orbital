using System;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    
    //TODO: Make ships not a part of the quad tree
    public class Ship : TemperatureBody, IDrawsText
    {
        private float kill_time = 60.0f;
        private float life_time = 0.0f;
        
        public override bool IsSelectable => false;

        public override bool DoesGravity => false;
        public Life Life { get; set; }
        public bool HasLife => Life != null;

        public Ship(Vector2f position, Vector2f velocity, Life life) : base(position, 10, velocity, 1, life.NormalTemp)
        {
            Life = life;
        }

        //TODO: Ships dying screws up the incremental quad tree, pretty sure
        public override void Update(Scene scene, float time)
        {
            base.Update(scene, time);
            if (HasLife)
            {

                Heat -= Circumference * (Temperature - Life.NormalTemp) / Mathf.Insulation * time;
                life_time += time;
                if (life_time > kill_time)
                {
                    Exists = false;
                    return;
                }

                UpdateLife(scene, time);
            }
            else
            {
                Exists = false;
            }
        }

        private void UpdateLife(Scene scene, float time)
        {
            if (HasLife)
            {
                if (Life.IsDead)
                {
                    Life = null;
                    Exists = false;
                    return;
                }
                
                Life.Update(time, Temperature);
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

            if (DrawText && HasLife)
            {
                RenderWindow window = (RenderWindow) target;
                View view = window.GetView();

                Text temperature_text = new Text((int) Temperature + " K (S)", Program.Font);
                temperature_text.Color = Mathf.TemperatureColorGradient.GetColor(Temperature);
                FormatText(temperature_text, 0, window);

                target.Draw(temperature_text);

                Text population_text = new Text(Format.PopulationText(Life.Population), Program.Font);
                population_text.Color = Color.White;
                Text tech_level_text = new Text(Life.TechLevel.ToString(), Program.Font);
                tech_level_text.Color = Color.White;
                Text faction_text = new Text(Life.Faction.ToString(), Program.Font);
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
}