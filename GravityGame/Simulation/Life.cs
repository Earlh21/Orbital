using System;

namespace GravityGame
{
    public class Life
    {
        private static int last_faction;
        private static int max_tech_level = 12;
        
        public static float growth_rate = 1;

        private readonly int faction;
        
        public float Science { get; private set; } = 0;
        public float Population { get; set; }
        public float NormalTemp { get; set; }
        public int TechLevel { get; set; }
        public float TempMultiplier { get; set; }
        public float ScienceMultiplier { get; set; }
        public bool IsDead => Population < 2;
        
        public int Faction => faction;

        public Life(float temperature, float temp_multiplier = 1.0f, float science_multiplier = 1.0f)
        {
            NormalTemp = temperature;
            last_faction++;
            faction = last_faction;
            TechLevel = 1;
            Population = 2;
            TempMultiplier = temp_multiplier;
            ScienceMultiplier = science_multiplier;
        }

        public Life(float temperature, int faction, int tech_level, float population, float temp_multiplier = 1.0f)
        {
            NormalTemp = temperature;
            this.faction = faction;
            TechLevel = tech_level;
            Population = population;
            TempMultiplier = temp_multiplier;
        }

        public float GetCarryingCapacity(float temperature)
        {
            float min_temp = NormalTemp - 50 * TechLevel * TempMultiplier;
            float max_temp = NormalTemp + 100 * TechLevel * TechLevel * TempMultiplier;

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

            Science += (float)(Program.R.NextDouble() / 0.5f + 1) * ScienceMultiplier * time;
            if (Science > GetScienceRequired(TechLevel + 1) && TechLevel < max_tech_level)
            {
                TechLevel++;
                Science = 0;
            }
        }

        private static float GetScienceRequired(int tech_level)
        {
            return 60.0f + tech_level * 12.0f;
        }
    }
}