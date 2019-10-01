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

        public float GetCarryingCapacity(float temperature)
        {
            float min_temp = NormalTemp - 75 * TechLevel;
            float max_temp = NormalTemp + 100 * TechLevel * TechLevel;

            float max_population = 2000 + 1000000 * (TechLevel - 1) * (TechLevel - 1);

            float t = Mathf.InvLerp2(min_temp, NormalTemp, max_temp, temperature);
            if (t > 0.5)
            {
                t = 1 - t;
            }

            float capacity = max_population * t * 2;

            if (capacity < 0)
            {
                capacity = 1;
            }

            return capacity;
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

            if (TechLevel <= 9 && Program.R.NextDouble() < 1 - Math.Pow(1 - tech_chance, time))
            {
                TechLevel++;
                tech_chance = 1.0f / (150.0f + (TechLevel - 1.0f) * 50.0f);
            }
        }
    }
}