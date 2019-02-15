using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SFML.System;

namespace GravityGame
{
    public class QuadTree
    {
        public QuadTree top_left;
        public QuadTree top_right;
        public QuadTree bottom_left;
        public QuadTree bottom_right;

        private Point center_of_mass;

        public Point CenterOfMass => center_of_mass;

        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public bool IsLeaf => top_left == null;
        public bool HasNode => Node != null;
        
        public Body Node { get; set; }

        public void CalculateCenterOfMass()
        {
            if (IsLeaf)
            {
                if (HasNode)
                {
                    center_of_mass = new Point(Node.Position, Node.Mass);
                    return;
                }
                else
                {
                    center_of_mass = new Point(new Vector2f(0, 0), 0);
                    return;
                }
            }
            
            top_left.CalculateCenterOfMass();
            top_right.CalculateCenterOfMass();
            bottom_left.CalculateCenterOfMass();
            bottom_right.CalculateCenterOfMass();

            Point[] points = new Point[4];
            points[0] = top_left.CenterOfMass;
            points[1] = top_right.CenterOfMass;
            points[2] = bottom_left.CenterOfMass;
            points[3] = bottom_right.CenterOfMass;

            center_of_mass = Point.CenterOfMass(points);
        }
        
        public void Insert(Body node)
        {
            if (!Contains(node))
            {
                throw new ArgumentException("Node isn't within quadtree bounds.");
            }
            
            if (HasNode || !IsLeaf)
            {
                if (IsLeaf)
                {
                    bottom_left = new QuadTree(Position, Size / 2);
                    bottom_right = new QuadTree(Position + new Vector2f(bottom_left.Size.X, 0), bottom_left.Size);
                    top_left = new QuadTree(Position + new Vector2f(0, bottom_left.Size.Y), bottom_left.Size);
                    top_right = new QuadTree(Position + bottom_left.Size, bottom_left.Size);
                }

                if (top_left.Contains(node))
                {
                    top_left.Insert(node);
                }
                else if (top_right.Contains(node))
                {
                    top_right.Insert(node);
                }
                else if (bottom_left.Contains(node))
                {
                    bottom_left.Insert(node);
                }
                else
                {
                    bottom_right.Insert(node);
                }
                
                //Make sure the old node gets put into a lower quadtree
                if (HasNode)
                {
                    Body old = Node;
                    Node = null;
                    Insert(old);
                }
            }
            else
            {
                Node = node;
            }
        }

        public bool Contains(Body node)
        {
            if (node.Position.X < Position.X || node.Position.X > Position.X + Size.X)
            {
                return false;
            }

            if (node.Position.Y < Position.Y || node.Position.Y > Position.Y + Size.Y)
            {
                return false;
            }

            return true;
        }
        
        public QuadTree(Vector2f position, Vector2f size)
        {
            Position = position;
            Size = size;
        }
    }
}