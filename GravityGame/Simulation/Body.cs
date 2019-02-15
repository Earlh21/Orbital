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
                mass = value;
                radius = Mathf.Sqrt(Mass / Mathf.PI * Density);
            }
        }
        public Vector2f Velocity { get; set; }
        public bool IsSelected { get; set; }

        public float Radius => radius;
        public float Area => Mathf.PI * Radius * Radius;
        public float Circumference => Mathf.PI * Radius * 2;

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

        public bool CheckCollide(Body other) => DistanceSquared(other) <= Mathf.Pow(Radius + other.Radius, 2);
        public Vector2f GetForceFrom(Point other)
        {
            Vector2f displacement = other.Position - Position;
            return Mathf.G * Mass * other.Mass / displacement.LengthSquared() * displacement;
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