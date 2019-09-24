using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NUnit.Framework.Internal.Commands;
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
		private bool iteration = false;

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
			if (!ContainsBody(node))
			{
				throw new ArgumentException("Node isn't within quadtree bounds.");
			}

			if (HasNode || !IsLeaf)
			{
				if (IsLeaf)
				{
					BottomLeft = new QuadTree(Position, Size / 2, this, iteration);
					BottomRight = new QuadTree(Position + new Vector2f(BottomLeft.Size.X, 0), BottomLeft.Size, this,
						iteration);
					TopLeft = new QuadTree(Position + new Vector2f(0, BottomLeft.Size.Y), BottomLeft.Size, this,
						iteration);
					TopRight = new QuadTree(Position + BottomLeft.Size, BottomLeft.Size, this, iteration);
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

		public QuadTree(Vector2f position, Vector2f size, QuadTree parent, bool iteration)
		{
			Domain = new Rectangle(position, size);
			Parent = parent;
			this.iteration = iteration;
		}

		public QuadTree(Rectangle domain, QuadTree parent, bool iteration)
		{
			Domain = domain;
			Parent = parent;
			this.iteration = iteration;
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

		private void ConsolidateUp()
		{
			QuadTree parent = Parent;

			if (parent == null)
			{
				return;
			}

			int nodes = 0;
			Body node = null;

			if (!parent.TopLeft.IsLeaf)
			{
				nodes = 100;
				goto Skip;
			}
			else
			{
				if (parent.TopLeft.HasNode)
				{
					nodes++;
					node = parent.TopLeft.Node;
				}
			}

			if (!parent.TopRight.IsLeaf)
			{
				nodes = 100;
				goto Skip;
			}
			else
			{
				if (parent.TopRight.HasNode)
				{
					nodes++;
					node = parent.TopRight.Node;
				}
			}

			if (!parent.BottomLeft.IsLeaf)
			{
				nodes = 100;
				goto Skip;
			}
			else
			{
				if (parent.BottomLeft.HasNode)
				{
					nodes++;
					node = parent.BottomLeft.Node;
				}
			}

			if (!parent.BottomRight.IsLeaf)
			{
				nodes = 100;
				goto Skip;
			}
			else
			{
				if (parent.BottomRight.HasNode)
				{
					nodes++;
					node = parent.BottomRight.Node;
				}
			}
			
			Skip:

			if (nodes < 2)
			{
				parent.TopLeft = null;
				parent.TopRight = null;
				parent.BottomLeft = null;
				parent.BottomRight = null;

				if (nodes == 1)
				{
					parent.Node = node;
				}
				
				parent.ConsolidateUp();
			}
		}

		public void Remove(Body body)
		{
			QuadTree leaf = Search(body);

			if (leaf == null)
			{
				return;
			}
			
			leaf.Node = null;
			leaf.ConsolidateUp();
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

		public void UpdateBody(Body body)
		{
			QuadTree containing = Search(body);

			if (!containing.ContainsPoint(body.Position))
			{
				containing.Remove(body);
				//TODO: Start inserting farther down instead of at the root since the body probably hasn't moved much
				if (Domain.ContainsPoint(body.Position))
				{
					Insert(body);
				}
			}
		}

		//TODO: Add a max height
		//TODO: Make the user give an array to fill instead of returning a list
		public int FindNearbyBodies(Vector2f position, int tree_height, BodyFilter filter, Body[] buffer)
		{
			QuadTree leaf = SearchPosition(position);
			
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
			List<Body> bodies = new List<Body>();

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

		public QuadTree FindSmallestContainingTree(Body body)
		{
			QuadTree leaf = Search(body);

			while (!leaf.FullyContains(body))
			{
				leaf = leaf.Parent;
			}

			return leaf;
		}
	}
}