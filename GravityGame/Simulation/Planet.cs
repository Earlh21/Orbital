using SFML.System;

namespace GravityGame
{
    public class Planet : TemperatureBody
    {
        public Life Life { get; set; }
        
        public override void Update(float time)
        {
            base.Update(time);
            UpdateLife(time);
        }

        public void UpdateLife(float time)
        {
            
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