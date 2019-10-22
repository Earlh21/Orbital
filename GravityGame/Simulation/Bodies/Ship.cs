using System;
using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    
    //TODO: Make ships last for less time
    //TODO: Make ships more accurate
    public class Ship : TemperatureBody, IDrawsText
    {
        private float kill_time = 60.0f;
        
        protected float LifeTime { get; private set; }
        
        public override bool IsSelectable => false;

        public override bool DoesGravity => false;
        public Life Life { get; set; }
        public bool HasLife => Life != null;
        public override Color? OutlineColor => new Color(0, 200, 0, 100);
        public override uint TexturePadding => 0;

        public Ship(Vector2f position, Vector2f velocity, Life life) : base(position, velocity, Composition.Basic(10), life.NormalTemp)
        {
            Life = life;
        }

        public override void Update(Scene scene, float time)
        {
            base.Update(scene, time);
            if (HasLife)
            {
                Heat += 1000 * Mathf.Sign(Life.NormalTemp - Temperature) * time;
                LifeTime += time;
                if (LifeTime > kill_time)
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
                
                Life.Update(time, Temperature, Life.NormalType);
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
}