using System.Collections.Generic;
using System.Data;
using System.Runtime.ConstrainedExecution;
using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
    public class Body : Point, ISelectable
    {
        private const float approximate_threshold = 0.5f;
        
        private float radius;
        private float mass;

        public readonly float Density;
        
        public override float Mass
        {
            get => mass;
            set
            {
                //Conserve momentum
                Velocity -= Velocity * mass / value;
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

        public bool CheckCollide(Body other) => DistanceSquared(other) <= Mathf.Pow(Radius + other.Radius, 2) && this != other;
        public bool Contains(Vector2f point) => DistanceSquared(point) <= radius * radius;
        public Vector2f GetForceFrom(Point other)
        {
            Vector2f displacement = other.Position - Position;
            return Mathf.G * Mass * other.Mass / displacement.LengthSquared() * displacement;
        }

        public List<Pair> GetCollisions(QuadTree tree)
        {
            List<Pair> collisions = new List<Pair>();
            
            if (tree == null)
            {
                return collisions;
            }

            if (tree.IsLeaf && tree.HasNode)
            {
                if (this != tree.Node && CheckCollide(tree.Node))
                {
                    collisions.Add(new Pair(this, tree.Node));
                }
            }
            else
            {
                float sd = tree.Size.X / Distance(tree.CenterOfMass);
                if (sd < approximate_threshold)
                {
                    //Skip this line
                }
                else
                {
                    collisions.AddRange(GetCollisions(tree.top_left));
                    collisions.AddRange(GetCollisions(tree.top_right));
                    collisions.AddRange(GetCollisions(tree.bottom_left));
                    collisions.AddRange(GetCollisions(tree.bottom_right));
                }
            }

            return collisions;
        }
        
        public Vector2f GetForceFrom(QuadTree tree)
        {
            Vector2f total_force = new Vector2f(0, 0);

            if (tree == null)
            {
                return total_force;
            }
            
            if (tree.IsLeaf && tree.HasNode)
            {
                if (this != tree.Node)
                {
                    total_force += GetForceFrom(tree.CenterOfMass);
                }
            }
            else
            {
                float sd = tree.Size.X / Distance(tree.CenterOfMass);
                if (sd < approximate_threshold)
                {
                    total_force += GetForceFrom(tree.CenterOfMass);
                }
                else
                {
                    total_force += GetForceFrom(tree.top_left);
                    total_force += GetForceFrom(tree.top_right);
                    total_force += GetForceFrom(tree.bottom_left);
                    total_force += GetForceFrom(tree.bottom_right);
                }
            }

            return total_force;
        }
    }
}