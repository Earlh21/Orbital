using System;
using System.Runtime.CompilerServices;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class TemperatureBody : RenderBody
    {

        public float Temperature => Heat / Area;
        public float Heat { get; set; }
        public bool DrawText { get; set; }

        public TemperatureBody() : base()
        {
            Heat = Mathf.AmbientTemp;
        }
        
        public TemperatureBody(Vector2f position, float mass) : this(position, mass, new Vector2f(0, 0), 1, 0)
        {
            
        }

        public TemperatureBody(Vector2f position, float mass, Vector2f velocity) : this(position, mass, velocity, 1, 0)
        {
            
        }

        public TemperatureBody(Vector2f position, float mass, Vector2f velocity, float density) : this(position, mass,
            velocity, density, Mathf.AmbientTemp)

        {
            
        }
        
        public TemperatureBody(Vector2f position, float mass, Vector2f velocity, float density, float temperature) : base(
            position, mass, velocity, density)
        {
            Heat = temperature * Area;
        }
        
        public override void Update(float time)
        {
            UpdateTemperature(time);
            base.Update(time);
        }

        private void UpdateTemperature(float time)
        {
            
            if (Temperature == Mathf.AmbientTemp)
            {
                return;
            }
            
            Heat -= Circumference * (Temperature / Mathf.AmbientTemp) / Mathf.Insulation * time;
        }

        public float GetHeatFlowFrom(Star star)
        {
            return star.Area * 200000.0f / Mathf.Pow(Distance(star), 2);
        }

        protected override Color GetColor()
        {
            if (IsSelected)
            {
                return new Color(200, 0, 200, 255);
            }
            
            return Mathf.TemperatureColorGradient.GetColor(Temperature);
        }
    }
}