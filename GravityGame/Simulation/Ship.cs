using GravityGame.Extension;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class Ship : TemperatureBody
    {
        public Life Life { get; set; }

        public override void Update(float time)
        {
            base.Update(time);
            
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
}