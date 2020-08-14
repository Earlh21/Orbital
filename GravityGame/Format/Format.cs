namespace GravityGame
{
    public static class Format
    {
        public static string PopulationText(float population)
        {
            long value = (long) population;

            if (population > 999999999999)
            {
                return TopPlaceDigits(value) + " T";
            }

            if (population > 999999999)
            {
                return TopPlaceDigits(value) + " B";
            }

            if (population > 999999)
            {
                return TopPlaceDigits(value) + " M";
            }

            return value.ToString();
        }

        private static string TopPlaceDigits(long value)
        {
            string text = value.ToString();
            int digits = text.Length % 3;
            if (digits == 0) digits = 3;

            return text.Substring(0, digits);
        }
    }
}