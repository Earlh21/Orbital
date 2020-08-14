using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class TemperatureBody : RenderBody
    {
        private float heat;
        
        public float Temperature => Heat / Area;

        public float Heat
        {
            get => heat;
            set
            {
                if (value < 0.1f)
                {
                    heat = 0.1f;
                    return;
                }

                heat = value;
            }
        }
        public bool DrawText { get; set; }
        
        public TemperatureBody(Vector2f position, Vector2f velocity, Composition composition, float temperature) : base(
            position, velocity, composition)
        {
            Heat = temperature * Area;
        }
        
        public override void Update(Scene scene, float time)
        {
            UpdateTemperature(time);
            base.Update(scene, time);
        }

        private void UpdateTemperature(float time)
        {
            if (Temperature == Mathf.AmbientTemp)
            {
                return;
            }

            float too_high = 1;

            if (Temperature > 10001)
            {
                too_high = Mathf.Max(1, Mathf.Log(Temperature - 10000));
            }
            
            Heat -= too_high * Circumference * (Temperature * Mathf.Pow(Temperature, 0.4f) - Mathf.AmbientTemp) / Mathf.Insulation * time;
        }

        public float GetHeatFlowFrom(Star star)
        {
            return star.Area * 100000.0f / Mathf.Pow(Distance(star), 2);
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