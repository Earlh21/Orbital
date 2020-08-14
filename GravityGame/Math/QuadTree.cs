using System;
using System.Collections.Generic;
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

		public PointMass CenterOfMass { get; private set; }
		public int BodyCount { get; private set; }

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

		public QuadTree Search(Body body)
		{
			if (IsLeaf)
			{
				if (HasNode && Node == body)
				{
					return this;
				}

				return SearchUp(body);
			}
			else
			{
				if (TopLeft.ContainsBody(body))
				{
					return TopLeft.Search(body);
				}
				else if (TopRight.ContainsBody(body))
				{
					return TopRight.Search(body);
				}
				else if(BottomLeft.ContainsBody(body))
				{
					return BottomLeft.Search(body);
				}
				else
				{
					return BottomRight.Search(body);
				}
			}
		}

		private QuadTree SearchDown(Body body)
		{
			if (IsLeaf)
			{
				if (Node == body)
				{
					return this;
				}

				return null;
			}
			else
			{
				QuadTree topleft_result = TopLeft.SearchDown(body);
				if (topleft_result != null)
				{
					return topleft_result;
				}

				QuadTree topright_result = TopRight.SearchDown(body);
				if (topright_result != null)
				{
					return topright_result;
				}

				QuadTree bottomleft_result = BottomLeft.SearchDown(body);
				if (bottomleft_result != null)
				{
					return bottomleft_result;
				}

				QuadTree bottomright_result = BottomRight.SearchDown(body);
				return bottomright_result;
			}
		}

		private QuadTree SearchUp(Body body)
		{
			QuadTree result = null;

			if (Parent == null)
			{
				return null;
			}
			
			if (Parent.TopLeft != this)
			{
				result = Parent.TopLeft.SearchDown(body);

				if (result != null)
				{
					return result;
				}
			}

			if (Parent.TopRight != this)
			{
				result = Parent.TopRight.SearchDown(body);
				
				if (result != null)
				{
					return result;
				}
			}
			
			if (Parent.BottomLeft != this)
			{
				result = Parent.BottomLeft.SearchDown(body);
				
				if (result != null)
				{
					return result;
				}
			}
			
			if (Parent.BottomRight != this)
			{
				result = Parent.BottomRight.SearchDown(body);
				
				if (result != null)
				{
					return result;
				}
			}
			
			return Parent.SearchUp(body);
		}
		
		public void CalculateCenterOfMass()
		{
			if (IsLeaf)
			{
				if (HasNode && Node.DoesGravity)
				{
					CenterOfMass = new PointMass(Node.Position, Node.Mass);

					BodyCount = 1;
					return;
				}
				else
				{
					CenterOfMass = new PointMass(new Vector2f(0, 0), 0);
					BodyCount = 0;
					return;
				}
			}

			BodyCount = 0;

			TopLeft.CalculateCenterOfMass();
			TopRight.CalculateCenterOfMass();
			BottomLeft.CalculateCenterOfMass();
			BottomRight.CalculateCenterOfMass();

			PointMass[] points_mass = new PointMass[4];
			points_mass[0] = TopLeft.CenterOfMass;
			BodyCount += TopLeft.BodyCount;
			points_mass[1] = TopRight.CenterOfMass;
			BodyCount += TopRight.BodyCount;
			points_mass[2] = BottomLeft.CenterOfMass;
			BodyCount += BottomLeft.BodyCount;
			points_mass[3] = BottomRight.CenterOfMass;
			BodyCount += BottomRight.BodyCount;

			CenterOfMass = PointMass.CenterOfMass(points_mass);
		}

		public void Insert(Body node)
		{
			if (!ContainsBody(node))
			{
				throw new ArgumentException("Node isn't within quadtree bounds.");
			}

			if (HasNode || !IsLeaf)
			{
				if (IsLeaf)
				{
					BottomLeft = new QuadTree(Position, Size / 2, this);
					BottomRight = new QuadTree(Position + new Vector2f(BottomLeft.Size.X, 0), BottomLeft.Size, this);
					TopLeft = new QuadTree(Position + new Vector2f(0, BottomLeft.Size.Y), BottomLeft.Size, this);
					TopRight = new QuadTree(Position + BottomLeft.Size, BottomLeft.Size, this);
				}

				if (TopLeft.ContainsBody(node))
				{
					TopLeft.Insert(node);
				}
				else if (TopRight.ContainsBody(node))
				{
					TopRight.Insert(node);
				}
				else if (BottomLeft.ContainsBody(node))
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

		public bool ContainsBody(Body node)
		{
			return Domain.ContainsPoint(node.Position);
		}

		public bool ContainsPoint(Vector2f point)
		{
			return Domain.ContainsPoint(point);
		}

		public bool FullyContains(Body node)
		{
			return Domain.FullyContains(node);
		}

		
		public void Draw(RenderTarget target, RenderStates states)
		{
			RenderWindow window = (RenderWindow) target;

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

		public QuadTree SearchPosition(Vector2f position)
		{
			if (IsLeaf)
			{
				if (HasNode)
				{
					return this;
				}

				return null;
			}
			else
			{
				if (TopLeft.ContainsPoint(position))
				{
					return TopLeft.SearchPosition(position);
				}
				else if (TopRight.ContainsPoint(position))
				{
					return TopRight.SearchPosition(position);
				}
				else if(BottomLeft.ContainsPoint(position))
				{
					return BottomLeft.SearchPosition(position);
				}
				else
				{
					return BottomRight.SearchPosition(position);
				}
			}
		}
		
		//TODO: Max distance
		public int FindNearbyBodies(Vector2f position, int tree_height, BodyFilter filter, Body[] buffer)
		{
			QuadTree leaf = SearchPosition(position);

			if (leaf == null)
			{
				return 0;
			}
			
			QuadTree previous = leaf;
			QuadTree current = leaf.Parent;

			int count = 0;
			int height = 0;
			
			while (count < buffer.Length && current != null && height < tree_height)
			{
				if (current.TopLeft != previous)
				{
					count = current.TopLeft.FindBodies(filter, count, buffer);
				}
				
				if (current.TopRight != previous)
				{
					count = current.TopRight.FindBodies(filter, count, buffer);
				}

				if (current.BottomLeft != previous)
				{
					count = current.BottomLeft.FindBodies(filter, count, buffer);
				}

				if (current.BottomRight != previous)
				{
					count = current.BottomRight.FindBodies(filter, count, buffer);
				}

				previous = current;
				current = current.Parent;
				height++;
			}

			return count;
		}

		private int FindBodies(BodyFilter filter, int current_count, Body[] buffer)
		{
			if (current_count >= buffer.Length)
			{
				return current_count;
			}
			
			if (IsLeaf)
			{
				if (HasNode)
				{
					if (filter.Contains(Node))
					{
						buffer[current_count] = Node;
						current_count++;
					}

					return current_count;
				}

				return current_count;
			}

			current_count = TopLeft.FindBodies(filter, current_count, buffer);
			current_count = TopRight.FindBodies(filter, current_count, buffer);
			current_count = BottomLeft.FindBodies(filter, current_count, buffer);
			current_count = BottomRight.FindBodies(filter, current_count, buffer);

			return current_count;
		}
	}
}