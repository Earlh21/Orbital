using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Messaging;
using GravityGame.Extension;
using SFML.System;

namespace GravityGame
{
    public abstract class Body : ISelectable
    {
        private Composition composition;

        /**
         * The mass composition of this body.
         */
        private Composition Composition
        {
            get => composition;
            set
            {
                composition = value;
                OnRadiusChange();
            }
        }

        public IReadOnlyList<Compound> Compounds => composition.Compounds;

        public float Mass => Composition.Mass;
        public float Density => Composition.Density;
        public float Area => Composition.Area;
        public float Radius => Mathf.Sqrt(Area / Mathf.PI);
        public float Circumference => Mathf.PI * Radius * 2;

        public float GasPercent => composition.GetGasPercent();
        public float RockPercent => composition.GetRockPercent();
        
        public Vector2f Momentum { get; set; }
        public Vector2f Velocity => Momentum / Mass;
        public Vector2f Position { get; private set; }
        public Vector2f Force { get; set; }
        public Vector2f Acceleration => Force / Mass;

        public bool Started { get; set; } = false;
        public bool ForcesDone { get; set; } = false;
        public virtual bool DoesGravity => true;
        public bool IsSelected { get; set; }
        public virtual bool IsSelectable => true;
        public bool Exists { get; set; } = true;

        public Body(Vector2f position, Vector2f velocity, Composition composition)
        {
            Composition = composition;
            Momentum = velocity * Mass;
            Position = position;
        }

        private Vector2f SlopeFunction(Vector2f pos, float t)
        {
            return Velocity + Acceleration * t;
        }
        
        public void Iterate(float time)
        {
            Position += Momentum * time / Mass;

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

        public bool Contains(Vector2f point) => DistanceSquared(point) <= Radius * Radius;

        public Vector2f GetForceFrom(PointMass other)
        {
            Vector2f displacement = other.Position - Position;
            return displacement.Unit() * Mathf.G * Mass * other.Mass / displacement.LengthSquared();
        }

        public Vector2f GetForceFrom(Body other)
        {
            Vector2f displacement = other.Position - Position;
            return displacement.Unit() * Mathf.G * Mass * other.Mass / displacement.LengthSquared();
        }

        /// <summary>
        /// Finds the smallest quad that fully contains this object.
        /// </summary>
        /// <param name="leaf">The quad this object is in</param>
        /// <returns>The smallest quad</returns>
        public QuadTree GetSmallestContainingTree(QuadTree leaf)
        {
            if (leaf == null)
            {
                return null;
            }
            
            if (leaf.FullyContains(this))
            {
                return leaf;
            }
            
            return GetSmallestContainingTree(leaf.Parent);
        }

        public void AutoOrbit(Body target)
        {
            Vector2f force = GetForceFrom(target);
            float acceleration = (force / Mass).Length();
            float velocity = Mathf.Sqrt(acceleration * Distance(target));

            float angle = Mathf.AngleTo(Position, target.Position);
            Vector2f velocity_unit = new Vector2f((float)Math.Cos(angle + Mathf.PI / 2), (float)Math.Sin(angle + Mathf.PI / 2));
            Momentum = Mass * (target.Velocity + velocity_unit * velocity);
        }

        public List<CollisionPair> GetCollisions(QuadTree tree)
        {
            List<CollisionPair> collisions = new List<CollisionPair>();

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
                        collisions.Add(new CollisionPair(this, tree.Node));
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
            Position += amount;
        }

        public static List<CollisionPair> GetAllCollisions(QuadTree tree)
        {
            List<CollisionPair> collisions = new List<CollisionPair>();

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

        public Vector2f GetForceFrom(QuadTree tree, float theta)
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
                if (sd < theta)
                {
                    total_force += GetForceFrom(tree.CenterOfMass);
                }
                else
                {
                    total_force += GetForceFrom(tree.TopLeft, theta);
                    total_force += GetForceFrom(tree.TopRight, theta);
                    total_force += GetForceFrom(tree.BottomLeft, theta);
                    total_force += GetForceFrom(tree.BottomRight, theta);
                }
            }

            return total_force;
        }

        public void AddComposition(Composition other)
        {
            Composition.AddComposition(other);
            OnRadiusChange();
        }

        public void AddComposition(Body other)
        {
            AddComposition(other.Composition);
        }

        public void SubtractBasicMass(float amount)
        {
            Composition.RemoveBasic(amount);
            OnRadiusChange();
        }

        public Colorf GetGasColor()
        {
            return composition.GetGasColor();
        }
        
        protected virtual void OnRadiusChange()
        {
            
        }
    }
}