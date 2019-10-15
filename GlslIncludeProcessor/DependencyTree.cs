using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GlslIncludeProcessor
{
	public class DependencyTree<T>
	{
		public TreeNode<T> Root { get; set; }

		public TreeNode<T> AddChild(TreeNode<T> parent, T value)
		{
			if (HasAncestor(parent, value))
			{
				throw new ArgumentException("Circular dependency found.");
			}

			if (Search(Root, value))
			{
				return null;
			}
			
			TreeNode<T> node = new TreeNode<T>(parent, value);
			parent.children.Add(node);
			return node;
		}

		public void Remove(TreeNode<T> node)
		{
			node.Parent = null;
		}

		private bool Search(TreeNode<T> node, T value)
		{
			if (node.Value.Equals(value))
			{
				return true;
			}

			foreach (TreeNode<T> child in node.children)
			{
				if (Search(child, value))
				{
					return true;
				}
			}

			return false;
		}
		
		private bool HasAncestor(TreeNode<T> node, T value)
		{
			if (node.Value.Equals(value))
			{
				return true;
			}

			if (node.Parent != null)
			{
				return HasAncestor(node.Parent, value);
			}

			return false;
		}
	}
}