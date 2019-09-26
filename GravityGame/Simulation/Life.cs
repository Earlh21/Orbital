using System;

namespace GravityGame
{
    public class Life
    {
        private static int last_faction;
        public static float growth_rate = 1;
        private float tech_chance = 1 / 150.0f;

        private readonly int faction;

        public float Population { get; set; }
        public float NormalTemp { get; set; }
        public int TechLevel { get; set; }
        public bool IsDead => Population < 2;
        
        public int Faction => faction;
        
        public Life(float temperature)
        {
            NormalTemp = temperature;
            last_faction++;
            faction = last_faction;
            TechLevel = 1;
            Population = 2;
            Population = 2;
        }

        public Life(float temperature, int faction, int tech_level, float population)
        {
            NormalTemp = temperature;
            this.faction = faction;
            TechLevel = tech_level;
            Population = population;
        }

        //TODO: Overhaul this calculation, it's just not working at high tech levels
        public float GetCarryingCapacity(float temperature)
        {
            float temp_diff = Math.Abs(temperature - NormalTemp) * Mathf.Pow(Mathf.Max(1, 300 - temperature), 0.75f) / 15;
            float temp_mod = temp_diff / TechLevel / 10;
            float temp_divisor = 1 + temp_mod;
            float value = 1000 * Mathf.Pow(TechLevel, 8) / temp_divisor - temp_mod * 100;
                
            if (value < 1)
            {
                return 1;
            }

            return value;
        }
        
        public void Update(float time, float temperature)
        {
            NormalTemp += Mathf.Sign(NormalTemp - temperature) * 0.1f * time;
                
            float growth = time * growth_rate * Population * (1 - Population / GetCarryingCapacity(temperature));

            if (Population < 2)
            {
                Population = 0;
            }
            
            Population += growth;

            if (Program.R.NextDouble() < 1 - Math.Pow(1 - tech_chance, time))
            {
                TechLevel++;
                tech_chance = 1.0f / (150.0f + (TechLevel - 1.0f) * 50.0f);
            }
        }
    }
}