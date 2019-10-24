using System;

namespace GravityGame
{
    public class Life
    {
        private static int last_faction;
        private static int max_tech_level = 10;
        
        public static float growth_rate = 1;
        
        public float Science { get; private set; } = 0;
        public float Population { get; private set; }
        public float NormalTemp { get; private set; }
        public int TechLevel { get; private set; }
        public Planet.PlanetType NormalType { get; }
        public float TempMultiplier { get; set; }
        public float ScienceMultiplier { get; set; }
        public bool IsDead => Population < 2;
        public int Faction { get; }

        /// <summary>
        /// Generations a new faction and creates a Life instance for it.
        /// </summary>
        /// <param name="temperature">normal temperature</param>
        /// <param name="normal_type">normal planet type</param>
        /// <param name="temp_multiplier">temperature tolerance multiplier</param>
        /// <param name="science_multiplier">science gain multiplier</param>
        public Life(float temperature, Planet.PlanetType normal_type, float temp_multiplier = 1.0f, float science_multiplier = 1.0f)
        {
            NormalTemp = temperature;
            last_faction++;
            Faction = last_faction;
            TechLevel = 1;
            Population = 2;
            TempMultiplier = temp_multiplier;
            ScienceMultiplier = science_multiplier;
            NormalType = normal_type;
        }

        /// <summary>
        /// Creates a new Life instance that copies values from a given Life instance.
        /// </summary>
        /// <param name="original">instance to copy</param>
        /// <returns>created life instance</returns>
        public Life(Life original)
        {
            NormalTemp = original.NormalTemp;
            Faction = original.Faction;
            TechLevel = original.TechLevel;
            Population = 10;
            TempMultiplier = original.TempMultiplier;
            ScienceMultiplier = original.ScienceMultiplier;
            NormalType = original.NormalType;
        }

        public float GetCarryingCapacity(float temperature, Planet.PlanetType planet_type)
        {
            float type_modifier = planet_type == NormalType ? 1.0f : TechLevel * 0.08f;
            
            float min_temp = NormalTemp - 50 * TechLevel * TempMultiplier * type_modifier;
            float max_temp = NormalTemp + 100 * TechLevel * TechLevel * TempMultiplier * type_modifier;

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
        
        public void Update(float time, float temperature, Planet.PlanetType planet_type)
        {
            NormalTemp += Mathf.Sign(NormalTemp - temperature) * 0.1f * time;
                
            float growth = time * growth_rate * Population * (1 - Population / GetCarryingCapacity(temperature, planet_type));

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
            return 60.0f + tech_level * 18.0f;
        }
    }
}