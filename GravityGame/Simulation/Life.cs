namespace GravityGame
{
    public struct Life
    {
        private static int last_faction;

        private readonly int faction;

        public float Population { get; set; }
        public float NormalTemp { get; set; }
        public int TechLevel { get; set; }
        
        public int Faction => faction;

        public Life(float temperature)
        {
            NormalTemp = temperature;
            last_faction++;
            faction = last_faction;
            TechLevel = 0;
            Population = 2;
        }
    }
}