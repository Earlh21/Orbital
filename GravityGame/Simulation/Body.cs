using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Messaging;
using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
    public class Body : ISelectable
    {
        private Vector2f last_position;
        private Vector2f next_position;
        private Vector2f position;

        public virtual float Theta => 0.5f;

        private float radius;
        private float mass;

        public readonly float Density;

        public Vector2f Velocity => Momentum / Mass;
        public Vector2f LastPosition => last_position;
        public Vector2f NextPosition => next_position;
        public Vector2f Position => position;
        public Vector2f Force { get; set; }
        public Vector2f Acceleration => Force / Mass;

        public bool Started { get; set; } = false;

        public bool ForcesDone { get; set; } = false;

        public virtual bool DoesGravity => true;

        public virtual float Mass
        {
            get => mass;
            set
            {
                mass = value;
                radius = Mathf.Sqrt(Mass / Mathf.PI * Density);
            }
        }

        public bool IsSelected { get; set; }
        public virtual bool IsSelectable => true;

        //Used for resolving collisions
        public bool Exists { get; set; } = true;

        public virtual float Radius => radius;
        public float Area => Mathf.PI * Radius * Radius;
        public float Circumference => Mathf.PI * Radius * 2;

        public Vector2f Momentum { get; set; }

        public Body() : this(new Vector2f(0, 0), 0, new Vector2f(0, 0), 1)
        {
        }

        public Body(Vector2f position, float mass) : this(position, mass, new Vector2f(0, 0), 1)
        {
        }

        public Body(Vector2f position, float mass, Vector2f velocity) : this(position, mass, velocity, 1)
        {
        }

        public Body(Vector2f position, float mass, Vector2f velocity, float density)
        {
            Density = density;
            Mass = mass;
            Momentum = velocity * Mass;
            last_position = position;
            next_position = position;
            this.position = position;
        }

        public void InterpolatePosition(float t)
        {
            position = Mathf.Lerp(last_position, next_position, t);
        }

        private Vector2f SlopeFunction(Vector2f pos, float t)
        {
            return Velocity + Acceleration * t;
        }
        
        public void Iterate(float time)
        {
            //Using Runge-Kutta fourth order
            last_position = next_position;
            Vector2f k1 = SlopeFunction(last_position, 0);
            Vector2f k2 = SlopeFunction(last_position + k1 * time / 2, time / 2);
            Vector2f k3 = SlopeFunction(last_position + k2 * time / 2, time / 2);
            Vector2f k4 = SlopeFunction(last_position + k3 * time, time);
            next_position = last_position + (k1 + 2 * k2 + 2 * k3 + k4) / 6 * time;

            Momentum += Force * time;
            Force = new Vector2f(0, 0);
        }

        public virtual void Update(Scene scene, float time)
        {
            //Nothing to see here
        }

        public float DistanceSquared(Body other)
        {
            return (other.Position - Position).LengthSquared();
        }

        public float Distance(Body other)
        {
            return (other.Position - Position).Length();
        }

        public float DistanceSquared(PointMass other)
        {
            return (other.Position - Position).LengthSquared();
        }

        public float Distance(PointMass other)
        {
            return (other.Position - Position).Length();
        }

        public float DistanceSquared(Vector2f other)
        {
            return (other - Position).LengthSquared();
        }

        public float Distance(Vector2f other)
        {
            return (other - Position).Length();
        }

        public bool CheckCollide(Body other) =>
            DistanceSquared(other) <= Mathf.Pow(Radius + other.Radius, 2) && this != other;

        public bool Contains(Vector2f point) => DistanceSquared(point) <= radius * radius;

        public Vector2f GetForceFrom(PointMass other)
        {
            Vector2f displacement = other.Position - NextPosition;
            return displacement.Unit() * Mathf.G * Mass * other.Mass / displacement.LengthSquared();
        }

        /// <summary>
        /// Finds the smallest quad that fully contains this object.
        /// </summary>
        /// <param name="leaf">The quad this object is in</param>
        /// <returns>The smallest quad</returns>
        public QuadTree GetSmallestContainingTree(QuadTree leaf)
        {
            if (leaf.FullyContains(this))
            {
                return leaf;
            }

            return GetSmallestContainingTree(leaf.Parent);
        }

        public List<Pair> GetCollisions(QuadTree tree)
        {
            List<Pair> collisions = new List<Pair>();

            if (tree == null)
            {
                return collisions;
            }

            if (tree.IsLeaf)
            {
                if (tree.HasNode)
                {
                    if (this != tree.Node && CheckCollide(tree.Node))
                    {
                        collisions.Add(new Pair(this, tree.Node));
                    }
                }
            }
            else
            {
                collisions.AddRange(GetCollisions(tree.TopLeft));
                collisions.AddRange(GetCollisions(tree.TopRight));
                collisions.AddRange(GetCollisions(tree.BottomLeft));
                collisions.AddRange(GetCollisions(tree.BottomRight));
            }

            return collisions;
        }

        public void Translate(Vector2f amount)
        {
            next_position += amount;
            last_position += amount;
            position += amount;
        }

        public static List<Pair> GetAllCollisions(QuadTree tree)
        {
            List<Pair> collisions = new List<Pair>();

            if (tree == null)
            {
                return collisions;
            }

            if (tree.IsLeaf)
            {
                if (tree.HasNode)
                {
                    if (tree.Node.Exists)
                    {
                        return tree.Node.GetCollisions(tree.Node.GetSmallestContainingTree(tree));
                    }
                }
            }
            else
            {
                collisions.AddRange(GetAllCollisions(tree.TopLeft));
                collisions.AddRange(GetAllCollisions(tree.TopRight));
                collisions.AddRange(GetAllCollisions(tree.BottomLeft));
                collisions.AddRange(GetAllCollisions(tree.BottomRight));
            }

            return collisions;
        }

        public Vector2f GetForceFrom(QuadTree tree)
        {
            Vector2f total_force = new Vector2f(0, 0);

            if (tree.IsLeaf)
            {
                if (tree.HasNode && this != tree.Node)
                {
                    total_force += GetForceFrom(tree.CenterOfMass);
                }
            }
            else
            {
                float sd = tree.Domain.Width / Distance(tree.CenterOfMass);
                if (sd < Theta)
                {
                    total_force += GetForceFrom(tree.CenterOfMass);
                }
                else
                {
                    total_force += GetForceFrom(tree.TopLeft);
                    total_force += GetForceFrom(tree.TopRight);
                    total_force += GetForceFrom(tree.BottomLeft);
                    total_force += GetForceFrom(tree.BottomRight);
                }
            }

            return total_force;
        }
    }
}