using System;
using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
    public class Point
    {
        public virtual float Mass { get; set; }
        public Vector2f Position { get; set; }

        public Point() : this(new Vector2f(0, 0), 0)
        {
        }
        
        public Point(Vector2f position, float mass)
        {
            Position = position;
            Mass = mass;
        }

        public float DistanceSquared(Vector2f position)
        {
            return (Position - position).LengthSquared();
        }
        
        public float DistanceSquared(Point other)
        {
            return DistanceSquared(other.Position);
        }

        public float Distance(Vector2f position)
        {
            return (Position - position).Length();
        }
        
        public float Distance(Point other)
        {
            return Distance(other.Position);
        }

        public static Point CenterOfMass(Point[] points)
        {
            if (points.Length == 0)
            {
                throw new ArgumentException("Array must have at least one point.");
            }

            if (points.Length == 1)
            {
                return new Point(points[0].Position, points[0].Mass);
            }

            Point total_point = new Point();
            foreach (Point point in points)
            {
                total_point.Position += point.Position * point.Mass;
                total_point.Mass += point.Mass;
            }

            total_point.Position /= total_point.Mass;
            return total_point;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.Position + b.Position, a.Mass + b.Mass);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.Position - b.Position, a.Mass - b.Mass);
        }

        public static Point operator *(Point a, float b)
        {
            return new Point(a.Position * b, a.Mass * b);
        }

        public static Point operator /(Point a, float b)
        {
            return new Point(a.Position / b, a.Mass / b);
        }
    }
}