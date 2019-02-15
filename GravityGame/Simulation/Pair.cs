namespace GravityGame
{
    public struct Pair
    {
        private Body a;
        private Body b;

        public Body A => a;
        public Body B => b;

        public Pair(Body a, Body b)
        {
            this.a = a;
            this.b = b;
        }
    }
}