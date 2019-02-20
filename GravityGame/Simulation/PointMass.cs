using System;
using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
    public class PointMass
    {
        public virtual float Mass { get; set; }
        public Vector2f Position { get; set; }

        public PointMass() : this(new Vector2f(0, 0), 0)
        {
        }
        
        public PointMass(Vector2f position, float mass)
        {
            Position = position;
            Mass = mass;
        }

        public float DistanceSquared(Vector2f position)
        {
            return (Position - position).LengthSquared();
        }
        
        public float DistanceSquared(PointMass other)
        {
            return DistanceSquared(other.Position);
        }

        public float Distance(Vector2f position)
        {
            return (Position - position).Length();
        }
        
        public float Distance(PointMass other)
        {
            return Distance(other.Position);
        }

        public static PointMass CenterOfMass(PointMass[] points_mass)
        {
            if (points_mass.Length == 0)
            {
                throw new ArgumentException("Array must have at least one point.");
            }

            if (points_mass.Length == 1)
            {
                return new PointMass(points_mass[0].Position, points_mass[0].Mass);
            }

            PointMass total_point_mass = new PointMass();
            foreach (PointMass point in points_mass)
            {
                total_point_mass.Position += point.Position * point.Mass;
                total_point_mass.Mass += point.Mass;
            }

            total_point_mass.Position /= total_point_mass.Mass;
            return total_point_mass;
        }

        public static PointMass operator +(PointMass a, PointMass b)
        {
            return new PointMass(a.Position + b.Position, a.Mass + b.Mass);
        }

        public static PointMass operator -(PointMass a, PointMass b)
        {
            return new PointMass(a.Position - b.Position, a.Mass - b.Mass);
        }

        public static PointMass operator *(PointMass a, float b)
        {
            return new PointMass(a.Position * b, a.Mass * b);
        }

        public static PointMass operator /(PointMass a, float b)
        {
            return new PointMass(a.Position / b, a.Mass / b);
        }
    }
}