using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class VectorFieldL
    {
        public List<VectorField> levels;

        public int Levels => levels.Count;

        public VectorFieldL(Rectangle domain, int level_count)
        {
            this.levels = new List<VectorField>();

            int dimensions = 2;
            for (int i = 0; i < level_count; i++)
            {
                this.levels.Add(new VectorField(domain, dimensions, dimensions));
                dimensions *= 2;
            }
        }

        public void Affect(Point point)
        {
            foreach (VectorField level in levels)
            {
                level.AffectAdjacent(point);
            }
        }
        
        public Vector2f GetValue(Vector2f position)
        {
            Vector2f total = new Vector2f(0, 0);
            
            foreach (VectorField level in levels)
            {
                total += level.GetValue(position);
            }

            return total;
        }
    }
}