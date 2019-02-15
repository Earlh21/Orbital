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
    }
}