using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.ConstrainedExecution;
using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
    public class Body : Point, ISelectable
    {
        private const float base_theta = 0.7f;

        //When a node has this many bodies, effective theta will be base_theta
        private const float max_bodies = 5;
        private const float min_theta = 0.1f;

        private float radius;
        private float mass;

        public readonly float Density;

        public override float Mass
        {
            get => mass;
            set
            {
                //Conserve momentum
                Velocity = Velocity * mass / value;
                mass = value;
                radius = Mathf.Sqrt(Mass / Mathf.PI * Density);
            }
        }

        public Vector2f Velocity { get; set; }

        public bool IsSelected { get; set; }

        //Used for resolving collisions
        public bool Exists { get; set; } = true;

        public float Radius => radius;
        public float Area => Mathf.PI * Radius * Radius;
        public float Circumference => Mathf.PI * Radius * 2;

        public Vector2f Momentum
        {
            get => Velocity * Mass;
            set => Velocity = value / Mass;
        }

        public Body() : this(new Vector2f(0, 0), 0, new Vector2f(0, 0), 1)
        {
        }

        public Body(Vector2f position, float mass) : this(position, mass, new Vector2f(0, 0), 1)
        {
        }

        public Body(Vector2f position, float mass, Vector2f velocity) : this(position, mass, velocity, 1)
        {
        }

        public Body(Vector2f position, float mass, Vector2f velocity, float density) : base(position, mass)
        {
            Density = density;
            Velocity = velocity;
            Position = position;
            Mass = mass;
        }

        public virtual void Update(float time)
        {
            Position += Velocity * time;
        }

        public void ApplyForce(Vector2f force, float time)
        {
            Velocity += force / Mass * time;
        }

        public bool CheckCollide(Body other) =>
            DistanceSquared(other) <= Mathf.Pow(Radius + other.Radius, 2) && this != other;

        public bool Contains(Vector2f point) => DistanceSquared(point) <= radius * radius;

        public Vector2f GetForceFrom(Point other)
        {
            Vector2f displacement = other.Position - Position;
            return Mathf.G * Mass * other.Mass / displacement.LengthSquared() * displacement;
        }

        /// <summary>
        /// Finds the smallest quad that fully contains this object.
        /// </summary>
        /// <param name="leaf">The quad this object is in</param>
        /// <returns>The smallest quad</returns>
        public QuadTree GetSmallestContainingTree(QuadTree leaf)
        {
            if (leaf.Contains(this))
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

        public void ApplyForceFrom(VectorFieldL field, float time)
        {
            Vector2f value = field.GetValue(Position);
            ApplyForce(value * Mass * Mathf.G, time);
        }
        
        public Vector2f GetForceFrom(QuadTree tree)
        {
            float approximate_threshold = Math.Max(min_theta, base_theta / Math.Max(1, max_bodies - tree.BodyCount));

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
                if (sd < 0.5f)
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