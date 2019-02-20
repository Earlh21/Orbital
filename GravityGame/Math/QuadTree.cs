using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SFML.Graphics;
using SFML.System;

namespace GravityGame
{
    public class QuadTree : Drawable
    {
        public QuadTree TopLeft { get; set; }
        public QuadTree TopRight { get; set; }
        public QuadTree BottomLeft { get; set; }
        public QuadTree BottomRight { get; set; }
        public QuadTree Parent { get; set; }
        
        private PointMass center_of_mass;
        private int body_count;

        public PointMass CenterOfMass => center_of_mass;
        public int BodyCount => body_count;

        public Rectangle Domain { get; set; }
        public Vector2f Position => Domain.Position;
        public Vector2f Size => Domain.Size;
        public float X => Position.X;
        public float Y => Position.Y;
        public float Width => Size.X;
        public float Height => Size.Y;

        public bool IsLeaf => TopLeft == null;
        public bool HasNode => Node != null;
        public bool IsRoot => Parent != null;
        
        public Body Node { get; set; }

        public void CalculateCenterOfMass()
        {
            if (IsLeaf)
            {
                if (HasNode && Node.DoesGravity)
                {
                    center_of_mass = new PointMass(Node.Position, Node.Mass);
                    body_count = 1;
                    return;
                }
                else
                {
                    center_of_mass = new PointMass(new Vector2f(0, 0), 0);
                    body_count = 0;
                    return;
                }
            }

            body_count = 0;
            
            TopLeft.CalculateCenterOfMass();
            TopRight.CalculateCenterOfMass();
            BottomLeft.CalculateCenterOfMass();
            BottomRight.CalculateCenterOfMass();

            PointMass[] points_mass = new PointMass[4];
            points_mass[0] = TopLeft.CenterOfMass;
            body_count += TopLeft.body_count;
            points_mass[1] = TopRight.CenterOfMass;
            body_count += TopRight.body_count;
            points_mass[2] = BottomLeft.CenterOfMass;
            body_count += BottomLeft.body_count;
            points_mass[3] = BottomRight.CenterOfMass;
            body_count += BottomRight.body_count;

            center_of_mass = PointMass.CenterOfMass(points_mass);
        }
        
        public void Insert(Body node)
        {
            if (!ContainsPoint(node))
            {
                throw new ArgumentException("Node isn't within quadtree bounds.");
            }
            
            if (HasNode || !IsLeaf)
            {
                if (IsLeaf)
                {
                    BottomLeft = new QuadTree(Position, Size / 2,this);
                    BottomRight = new QuadTree(Position + new Vector2f(BottomLeft.Size.X, 0), BottomLeft.Size, this);
                    TopLeft = new QuadTree(Position + new Vector2f(0, BottomLeft.Size.Y), BottomLeft.Size, this);
                    TopRight = new QuadTree(Position + BottomLeft.Size, BottomLeft.Size, this);
                }

                if (TopLeft.ContainsPoint(node))
                {
                    TopLeft.Insert(node);
                }
                else if (TopRight.ContainsPoint(node))
                {
                    TopRight.Insert(node);
                }
                else if (BottomLeft.ContainsPoint(node))
                {
                    BottomLeft.Insert(node);
                }
                else
                {
                    BottomRight.Insert(node);
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

        public bool ContainsPoint(Body node)
        {
            return Domain.ContainsPoint(node.Position);
        }

        public bool FullyContains(Body node)
        {
            return Domain.FullyContains(node);
        }
        
        public QuadTree(Vector2f position, Vector2f size, QuadTree parent)
        {
            Domain = new Rectangle(position, size);
            Parent = parent;
        }

        public QuadTree(Rectangle domain, QuadTree parent)
        {
            Domain = domain;
            Parent = parent;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            RenderWindow window = (RenderWindow) target;
            View view = window.GetView();
            
            RectangleShape rs = new RectangleShape();
            rs.Position = new Vector2f(X, -Y - Height);
            rs.Size = Size;
            rs.FillColor = Color.Transparent;
            rs.OutlineColor = Color.Green;
            rs.OutlineThickness = window.GetView().Size.X / 1000.0f;
            target.Draw(rs);
            
            if (!IsLeaf)
            {
                target.Draw(TopLeft);
                target.Draw(BottomRight);
                target.Draw(BottomLeft);
                target.Draw(TopRight);
            }
        }
    }
}